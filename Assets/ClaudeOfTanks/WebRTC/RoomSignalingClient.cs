using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ClaudeOfTanks.WebRTC
{
    public sealed class RoomSignalingClient : IDisposable
    {
        private readonly object _gate = new object();
        private readonly RoomSignalingClientOptions _options;
        private readonly CancellationTokenSource _lifetime =
            new CancellationTokenSource();
        private readonly Queue<SignalingEnvelope> _events =
            new Queue<SignalingEnvelope>();
        private readonly RoomSignalingRecovery _recovery;
        private readonly SignalingRoomRequester _requester;
        private readonly SignalingSignalQueue _signalQueue =
            new SignalingSignalQueue();
        private readonly SignalingChannelOwner _transport;
        private SignalingPlayer _player;
        private bool _roomAuthenticated;
        private bool _manualClose;
        private bool _disposed;

        public RoomSignalingClient(
            Uri endpoint,
            RoomSignalingClientOptions options = null)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (endpoint.Scheme != "ws" && endpoint.Scheme != "wss")
                throw new ArgumentException(
                    "Signaling endpoint must use ws or wss.",
                    nameof(endpoint));
            RoomSignalingClientOptions source =
                options ?? new RoomSignalingClientOptions();
            ValidateOptions(source);
            _options = new RoomSignalingClientOptions
            {
                ConnectTimeoutMs = source.ConnectTimeoutMs,
                RequestTimeoutMs = source.RequestTimeoutMs,
                EventPollIntervalMs = source.EventPollIntervalMs,
                EventPollTimeoutMs = source.EventPollTimeoutMs,
                ReconnectDelaysMs = (int[])source.ReconnectDelaysMs.Clone(),
                SessionId = source.SessionId,
                Origin = source.Origin,
                ConnectionFactory = source.ConnectionFactory
            };
            SessionId = RoomSignalingProtocol.CleanSessionId(
                string.IsNullOrEmpty(_options.SessionId)
                    ? RoomSignalingProtocol.CreateSessionId()
                    : _options.SessionId);
            _recovery = new RoomSignalingRecovery(_options, _lifetime.Token);
            _transport = new SignalingChannelOwner(endpoint, _options);
            _transport.EventReceived += OnChannelEvent;
            _transport.Closed += OnChannelClosed;
            _requester = new SignalingRoomRequester(
                _options,
                _lifetime.Token,
                ConnectAsync,
                _transport.Current,
                reason => Disconnect(reason),
                IsClosed);
        }

        public RoomSignalingState State { get; private set; } =
            RoomSignalingState.Idle;
        public string SessionId { get; private set; }
        public string RoomCode { get; private set; }
        public string PeerId { get; private set; }
        public string HostId { get; private set; }
        public int QueuedSignalCount => _signalQueue.Count;

        public event Action<SignalingEnvelope> EventReceived;

        public Task ConnectAsync()
        {
            lock (_gate)
            {
                RequireAlive();
                State = RoomSignalingState.Connecting;
            }
            return ConnectAndPublishStateAsync();
        }

        public async Task<SignalingRoomInfo> CreateRoomAsync(
            string playerId,
            string playerName,
            int maxPlayers = 14,
            string mode = "private")
        {
            if (maxPlayers < 2 || maxPlayers > 14)
                throw new ArgumentOutOfRangeException(nameof(maxPlayers));
            _manualClose = false;
            SignalingPlayer player =
                RoomSignalingProtocol.CleanPlayer(playerId, playerName);
            string cleanMode = string.IsNullOrEmpty(mode) ? "private" : mode;
            if (cleanMode.Length > 24) cleanMode = cleanMode.Substring(0, 24);
            SignalingPayload response = await _requester.RequestRoomAsync(
                "room_create",
                new SignalingPayload
                {
                    player = player,
                    sessionId = SessionId,
                    maxPlayers = maxPlayers,
                    mode = cleanMode
                }).ConfigureAwait(false);
            SignalingRoomInfo room =
                RoomSignalingProtocol.ReadRoomInfo(response);
            AdoptRoom(room, player);
            return room;
        }

        public async Task<SignalingRoomInfo> JoinRoomAsync(
            string roomCode,
            string playerId,
            string playerName)
        {
            _manualClose = false;
            string code = RoomSignalingProtocol.NormalizeRoomCode(roomCode);
            SignalingPlayer player =
                RoomSignalingProtocol.CleanPlayer(playerId, playerName);
            SignalingPayload response = await _requester.RequestRoomAsync(
                "room_join",
                JoinPayload(code, player)).ConfigureAwait(false);
            SignalingRoomInfo room =
                RoomSignalingProtocol.ReadRoomInfo(response, code, true);
            AdoptRoom(room, player);
            return room;
        }

        public bool SendSignal(
            string toPeerId,
            string toSessionId,
            WebRtcSignal signal)
        {
            string target = (toPeerId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(target))
                throw new ArgumentException("Target peer is required.", nameof(toPeerId));
            RoomSignalingProtocol.ValidateSignal(signal);
            bool open;
            lock (_gate)
            {
                RequireAlive();
                if (string.IsNullOrEmpty(RoomCode) || string.IsNullOrEmpty(PeerId))
                    throw new InvalidOperationException("Join or create a room first.");
                open = State == RoomSignalingState.Open && _roomAuthenticated;
            }
            _signalQueue.Enqueue(new SignalingPayload
            {
                roomCode = RoomCode,
                toPeerId = target,
                toSessionId = toSessionId ?? string.Empty,
                signal = signal
            });
            if (open) StartSignalFlush();
            else EnsureReconnect("signal_queued", false);
            return open;
        }

        public int PumpEvents(int maximumEvents = int.MaxValue)
        {
            if (maximumEvents < 0) throw new ArgumentOutOfRangeException(
                nameof(maximumEvents));
            int delivered = 0;
            while (delivered < maximumEvents)
            {
                SignalingEnvelope message;
                lock (_gate)
                {
                    if (_events.Count == 0) break;
                    message = _events.Dequeue();
                }
                delivered++;
                EventReceived?.Invoke(message);
            }
            return delivered;
        }

        public Task<bool> RestartRoomSessionAsync(string reason = "rtc_recovery")
        {
            lock (_gate)
            {
                RequireAlive();
                if (string.IsNullOrEmpty(RoomCode) || _player == null)
                    throw new InvalidOperationException("Join or create a room first.");
                SessionId = RoomSignalingProtocol.CreateSessionId();
            }
            _signalQueue.Clear();
            Disconnect(reason);
            return EnsureReconnect(reason, true);
        }

        public async Task CloseAsync(string reason = "client_closed")
        {
            string roomCode;
            lock (_gate)
            {
                if (_disposed || _manualClose) return;
                _manualClose = true;
                roomCode = _roomAuthenticated ? RoomCode : null;
            }
            if (!string.IsNullOrEmpty(roomCode) && _transport.IsOpen)
            {
                try
                {
                    await _transport.Current().SendAsync(
                        "room_leave",
                        new SignalingPayload { roomCode = roomCode })
                        .ConfigureAwait(false);
                }
                catch { }
            }
            _lifetime.Cancel();
            await _transport.CloseAsync(reason).ConfigureAwait(false);
            lock (_gate)
            {
                ClearRoomLocked();
                _events.Clear();
                State = RoomSignalingState.Closed;
            }
            _signalQueue.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            try { CloseAsync("disposed").GetAwaiter().GetResult(); }
            catch { }
            _disposed = true;
            _transport.Dispose();
            _lifetime.Dispose();
            EventReceived = null;
        }

        private async Task ConnectAndPublishStateAsync()
        {
            try
            {
                await _transport.ConnectAsync().ConfigureAwait(false);
                lock (_gate) State = RoomSignalingState.Open;
            }
            catch
            {
                lock (_gate) State = RoomSignalingState.Closed;
                throw;
            }
        }

        private void AdoptRoom(SignalingRoomInfo room, SignalingPlayer player)
        {
            lock (_gate)
            {
                RoomCode = room.RoomCode;
                PeerId = room.PeerId;
                HostId = room.HostId;
                _player = player;
                _roomAuthenticated = true;
            }
            _recovery.ResetAttempts();
            StartPolling();
            StartSignalFlush();
        }

        private void StartPolling()
        {
            _recovery.StartPolling(
                CanUseRoom,
                () => _requester.RequestWithStoreRetryAsync(
                    "room_poll",
                    new SignalingPayload { roomCode = RoomCode },
                    _options.EventPollTimeoutMs),
                reason => Disconnect(reason, true));
        }

        private Task<bool> EnsureReconnect(string reason, bool immediate)
        {
            return _recovery.EnsureReconnect(
                reason,
                immediate,
                CanReconnect,
                ResumeOnceAsync,
                OnReconnectState);
        }

        private async Task<RoomResumeResult> ResumeOnceAsync()
        {
            try
            {
                await ConnectAsync().ConfigureAwait(false);
                SignalingPayload response =
                    await _requester.RequestWithStoreRetryAsync(
                    "room_join",
                    JoinPayload(RoomCode, _player)).ConfigureAwait(false);
                SignalingRoomInfo room = RoomSignalingProtocol.ReadRoomInfo(
                    response,
                    RoomCode,
                    true);
                AdoptRoom(room, _player);
                QueueEvent(new SignalingEnvelope
                {
                    type = "signaling_resumed",
                    payload = response
                });
                return RoomResumeResult.Resumed;
            }
            catch (RoomSignalingException error)
            {
                if (error.Code == "room_not_found")
                {
                    QueueRoomClosed();
                    return RoomResumeResult.RoomClosed;
                }
                Disconnect("room_resume_retry");
                return RoomResumeResult.Retry;
            }
            catch (OperationCanceledException)
            {
                return RoomResumeResult.RoomClosed;
            }
            catch
            {
                Disconnect("room_resume_retry");
                return RoomResumeResult.Retry;
            }
        }

        private void StartSignalFlush()
        {
            _signalQueue.StartFlush(
                CanUseRoom,
                signal =>
                    _transport.Current().SendAsync("room_signal", signal),
                reason => Disconnect(reason, true));
        }

        private void OnChannelEvent(SignalingEnvelope message)
        {
            if (message.type == "room_signal" &&
                message.payload.toSessionId != SessionId)
            {
                return;
            }
            QueueEvent(message);
        }

        private void OnChannelClosed(string reason)
        {
            Disconnect(reason, true);
        }

        private void Disconnect(string reason, bool reconnect = false)
        {
            lock (_gate)
            {
                _roomAuthenticated = false;
                State = RoomSignalingState.Closed;
            }
            _transport.Abort(reason);
            if (reconnect) EnsureReconnect(reason, false);
        }

        private void QueueEvent(SignalingEnvelope message)
        {
            lock (_gate) QueueEventLocked(message);
        }

        private void QueueEventLocked(SignalingEnvelope message)
        {
            if (_events.Count >= RoomSignalingProtocol.MaximumQueuedEvents)
                _events.Dequeue();
            _events.Enqueue(message);
        }

        private void QueueRoomClosed()
        {
            lock (_gate)
            {
                QueueEventLocked(new SignalingEnvelope
                {
                    type = "room_closed",
                    payload = new SignalingPayload
                    {
                        roomCode = RoomCode,
                        reason = "expired"
                    }
                });
                ClearRoomLocked();
            }
        }

        private bool CanUseRoom()
        {
            lock (_gate)
            {
                return !_manualClose &&
                    _roomAuthenticated &&
                    State == RoomSignalingState.Open &&
                    _transport.IsOpen;
            }
        }

        private bool CanReconnect()
        {
            lock (_gate)
            {
                return !_manualClose &&
                    !string.IsNullOrEmpty(RoomCode) &&
                    _player != null;
            }
        }

        private bool IsClosed()
        {
            lock (_gate) return _manualClose;
        }

        private void OnReconnectState(string reason, int attempt, int delay)
        {
            lock (_gate)
            {
                State = RoomSignalingState.Reconnecting;
                QueueEventLocked(new SignalingEnvelope
                {
                    type = "signaling_state",
                    payload = new SignalingPayload
                    {
                        state = "reconnecting",
                        reason = reason,
                        attempt = attempt,
                        delayMs = delay
                    }
                });
            }
        }

        private SignalingPayload JoinPayload(
            string roomCode,
            SignalingPlayer player)
        {
            return new SignalingPayload
            {
                roomCode = roomCode,
                player = player,
                sessionId = SessionId
            };
        }

        private void ClearRoomLocked()
        {
            RoomCode = null;
            PeerId = null;
            HostId = null;
            _player = null;
            _roomAuthenticated = false;
        }

        private void RequireAlive()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomSignalingClient));
            if (_manualClose)
                throw new InvalidOperationException("Signaling client is closed.");
        }

        private static void ValidateOptions(RoomSignalingClientOptions options)
        {
            if (options.ConnectTimeoutMs < 1 ||
                options.RequestTimeoutMs < 1 ||
                options.EventPollIntervalMs < 10 ||
                options.EventPollTimeoutMs < 100)
            {
                throw new ArgumentException("Signaling timeout options are invalid.");
            }
            if (options.ReconnectDelaysMs == null ||
                options.ReconnectDelaysMs.Length == 0)
            {
                throw new ArgumentException(
                    "At least one signaling reconnect delay is required.");
            }
            for (int i = 0; i < options.ReconnectDelaysMs.Length; i++)
                if (options.ReconnectDelaysMs[i] < 0)
                    throw new ArgumentException(
                        "Signaling reconnect delays cannot be negative.");
            if (!string.IsNullOrEmpty(options.Origin) &&
                (!Uri.TryCreate(options.Origin, UriKind.Absolute, out Uri origin) ||
                 (origin.Scheme != "http" && origin.Scheme != "https")))
            {
                throw new ArgumentException(
                    "Signaling origin must use http or https.");
            }
        }
    }
}
