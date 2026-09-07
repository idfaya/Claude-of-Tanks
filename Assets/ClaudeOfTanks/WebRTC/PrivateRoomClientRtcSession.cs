using System;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomClientRtcSession : IDisposable
    {
        private readonly RoomSignalingClient _signaling;
        private readonly SignalingRoomInfo _room;
        private readonly RTCConfiguration? _configuration;
        private readonly WebRtcCoroutineDispatcher _coroutines;
        private readonly int _failedRebuildDelayMs;
        private readonly int _disconnectedRebuildDelayMs;
        private WebRtcPeerSession _peer;
        private WebRtcNetworkEndpoint _transport;
        private Task<bool> _restartTask;
        private string _hostSessionId;
        private string _rebuildReason;
        private double _rebuildAt;
        private int _peerGeneration;
        private bool _closed;

        public PrivateRoomClientRtcSession(
            RoomSignalingClient signaling,
            SignalingRoomInfo room,
            MonoBehaviour coroutineOwner,
            PrivateRoomRtcSessionOptions options = null)
        {
            _signaling = signaling ??
                throw new ArgumentNullException(nameof(signaling));
            _room = room ?? throw new ArgumentNullException(nameof(room));
            if (room.PeerId == room.HostId)
                throw new ArgumentException(
                    "Client RTC session cannot own the room host seat.",
                    nameof(room));
            PrivateRoomRtcSessionOptions settings =
                options ?? new PrivateRoomRtcSessionOptions();
            if (settings.FailedRebuildDelayMs < 0 ||
                settings.DisconnectedRebuildDelayMs < 0)
            {
                throw new ArgumentException(
                    "RTC rebuild delays cannot be negative.",
                    nameof(options));
            }
            _configuration = settings.Configuration;
            _failedRebuildDelayMs = settings.FailedRebuildDelayMs;
            _disconnectedRebuildDelayMs =
                settings.DisconnectedRebuildDelayMs;
            _coroutines = new WebRtcCoroutineDispatcher(coroutineOwner);
            _hostSessionId = FindHostSession(room);
            if (!ValidSessionId(_hostSessionId))
                throw new ArgumentException(
                    "Joined room is missing the host signaling session.",
                    nameof(room));
            _signaling.EventReceived += OnSignalingEvent;
            CreatePeer();
        }

        public string RoomCode => _room.RoomCode;
        public string LocalPeerId => _room.PeerId;
        public string HostPeerId => _room.HostId;
        public string HostSessionId => _hostSessionId;
        public WebRtcNetworkEndpoint Transport => _transport;
        public bool IsRestarting => _restartTask != null;

        public event Action<WebRtcNetworkEndpoint> TransportReady;
        public event Action<string> Failed;
        public event Action<string> Closed;

        public int Pump(int maximumEvents = int.MaxValue)
        {
            RequireOpen();
            AdvanceRoomRestart();
            int delivered = _signaling.PumpEvents(maximumEvents);
            if (_transport != null && !_transport.IsOpen)
                RequestRebuild(
                    "rtc_transport_closed",
                    _failedRebuildDelayMs);
            if (_restartTask == null &&
                _rebuildReason != null &&
                Time.realtimeSinceStartupAsDouble >= _rebuildAt)
            {
                StartRoomRestart();
            }
            return delivered;
        }

        public bool RequestRebuild(
            string reason = "rtc_session_rebuild",
            int delayMs = 0)
        {
            RequireOpen();
            if (delayMs < 0)
                throw new ArgumentOutOfRangeException(nameof(delayMs));
            if (_restartTask != null) return false;
            double at = Time.realtimeSinceStartupAsDouble +
                delayMs / 1000.0;
            if (_rebuildReason == null || at < _rebuildAt)
            {
                _rebuildReason = string.IsNullOrEmpty(reason)
                    ? "rtc_session_rebuild"
                    : reason;
                _rebuildAt = at;
            }
            return true;
        }

        public async Task CloseAsync(string reason = "client_closed")
        {
            if (_closed) return;
            _closed = true;
            _signaling.EventReceived -= OnSignalingEvent;
            DisposePeer(reason);
            Closed?.Invoke(reason);
            await _signaling.CloseAsync(reason).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_closed) return;
            try { CloseAsync("disposed").GetAwaiter().GetResult(); }
            catch { }
            TransportReady = null;
            Failed = null;
            Closed = null;
        }

        private void OnSignalingEvent(SignalingEnvelope message)
        {
            SignalingPayload payload = message?.payload;
            if (_closed || payload == null || payload.roomCode != RoomCode)
                return;
            if (message.type == "room_signal")
            {
                HandleSignal(payload);
            }
            else if (message.type == "peer_joined" &&
                payload.peerId == HostPeerId)
            {
                ObserveHostSession(payload.sessionId);
            }
            else if (message.type == "signaling_resumed")
            {
                ObserveHostSession(
                    FindHostSession(payload.peers, HostPeerId));
            }
            else if (message.type == "room_closed")
            {
                string reason = payload.reason ?? "room_closed";
                _ = CloseAsync(reason);
            }
        }

        private void ObserveHostSession(string sessionId)
        {
            if (!ValidSessionId(sessionId))
            {
                Fail("invalid_host_session");
                return;
            }
            if (sessionId != _hostSessionId)
            {
                _hostSessionId = sessionId;
                ReplacePeer("host_session_replaced");
                return;
            }
            if (_peer != null)
            {
                RTCPeerConnectionState state = _peer.Peer.ConnectionState;
                if (state == RTCPeerConnectionState.Failed ||
                    state == RTCPeerConnectionState.Disconnected)
                {
                    Run(_peer, _peerGeneration, _peer.Restart());
                }
            }
        }

        private void HandleSignal(SignalingPayload payload)
        {
            WebRtcPeerSession peer = _peer;
            int generation = _peerGeneration;
            if (peer == null ||
                payload.fromPeerId != HostPeerId ||
                payload.fromSessionId != _hostSessionId ||
                payload.signal == null)
            {
                return;
            }
            Run(peer, generation, peer.HandleSignal(payload.signal));
        }

        private void CreatePeer()
        {
            if (_closed) return;
            int generation = ++_peerGeneration;
            WebRtcPeerSession peer = _configuration.HasValue
                ? new WebRtcPeerSession(
                    WebRtcPeerRole.Client,
                    _configuration.Value)
                : new WebRtcPeerSession(WebRtcPeerRole.Client);
            _peer = peer;
            peer.SignalReady += signal => SendSignal(peer, generation, signal);
            peer.TransportReady += transport =>
                PublishTransport(peer, generation, transport);
            peer.ConnectionStateChanged += state =>
                ConnectionStateChanged(peer, generation, state);
            peer.Failed += reason => SessionFailed(peer, generation, reason);
            Run(peer, generation, peer.Start());
        }

        private void ReplacePeer(string reason)
        {
            DisposePeer(reason);
            _rebuildReason = null;
            CreatePeer();
        }

        private void SendSignal(
            WebRtcPeerSession peer,
            int generation,
            WebRtcSignal signal)
        {
            if (!IsCurrent(peer, generation)) return;
            try
            {
                _signaling.SendSignal(
                    HostPeerId,
                    _hostSessionId,
                    signal);
            }
            catch (Exception error)
            {
                Fail(ErrorCode(error, "signal_send_failed"));
            }
        }

        private void PublishTransport(
            WebRtcPeerSession peer,
            int generation,
            WebRtcNetworkEndpoint transport)
        {
            if (!IsCurrent(peer, generation))
            {
                transport.Close("rtc_generation_replaced");
                return;
            }
            _transport = transport;
            _rebuildReason = null;
            TransportReady?.Invoke(transport);
        }

        private void ConnectionStateChanged(
            WebRtcPeerSession peer,
            int generation,
            RTCPeerConnectionState state)
        {
            if (!IsCurrent(peer, generation)) return;
            if (state == RTCPeerConnectionState.Connected)
            {
                _rebuildReason = null;
            }
            else if (state == RTCPeerConnectionState.Failed)
            {
                RequestRebuild(
                    "rtc_connection_failed",
                    _failedRebuildDelayMs);
            }
            else if (state == RTCPeerConnectionState.Disconnected)
            {
                RequestRebuild(
                    "rtc_connection_disconnected",
                    _disconnectedRebuildDelayMs);
            }
            else if (state == RTCPeerConnectionState.Closed)
            {
                RequestRebuild("rtc_connection_closed", 0);
            }
        }

        private void SessionFailed(
            WebRtcPeerSession peer,
            int generation,
            string reason)
        {
            if (!IsCurrent(peer, generation)) return;
            bool recoverableClose =
                reason == "rtc_connection_closed" ||
                reason == "rtc_channel_closed";
            if (!recoverableClose) Fail(reason);
            int delay =
                reason == "rtc_connection_failed" ||
                recoverableClose
                ? _failedRebuildDelayMs
                : 0;
            RequestRebuild(reason, delay);
        }

        private void Run(
            WebRtcPeerSession peer,
            int generation,
            System.Collections.IEnumerator routine)
        {
            _coroutines.Run(
                routine,
                () => IsCurrent(peer, generation),
                error =>
                {
                    Fail(ErrorCode(error, "rtc_operation_failed"));
                    RequestRebuild("rtc_operation_failed", 0);
                });
        }

        private void StartRoomRestart()
        {
            string reason = _rebuildReason ?? "rtc_session_rebuild";
            _rebuildReason = null;
            DisposePeer(reason);
            try
            {
                _restartTask =
                    _signaling.RestartRoomSessionAsync(reason);
            }
            catch (Exception error)
            {
                Fail(ErrorCode(error, "signaling_restart_failed"));
                _rebuildReason = reason;
                _rebuildAt = Time.realtimeSinceStartupAsDouble +
                    _failedRebuildDelayMs / 1000.0;
            }
        }

        private void AdvanceRoomRestart()
        {
            Task<bool> restart = _restartTask;
            if (restart == null || !restart.IsCompleted) return;
            _restartTask = null;
            if (restart.IsFaulted)
            {
                Fail(ErrorCode(
                    restart.Exception?.InnerException ?? restart.Exception,
                    "signaling_restart_failed"));
                ScheduleRestartRetry();
                return;
            }
            if (restart.IsCanceled || !restart.Result)
            {
                Fail("signaling_restart_failed");
                ScheduleRestartRetry();
                return;
            }
            CreatePeer();
        }

        private void ScheduleRestartRetry()
        {
            if (_closed) return;
            _rebuildReason = "signaling_restart_retry";
            _rebuildAt = Time.realtimeSinceStartupAsDouble +
                _failedRebuildDelayMs / 1000.0;
        }

        private void DisposePeer(string reason)
        {
            WebRtcPeerSession peer = _peer;
            _peer = null;
            _peerGeneration++;
            _transport = null;
            if (peer != null) peer.Dispose();
        }

        private bool IsCurrent(
            WebRtcPeerSession peer,
            int generation)
        {
            return !_closed &&
                ReferenceEquals(_peer, peer) &&
                _peerGeneration == generation;
        }

        private void Fail(string reason)
        {
            Failed?.Invoke(reason);
        }

        private void RequireOpen()
        {
            if (_closed)
                throw new ObjectDisposedException(
                    nameof(PrivateRoomClientRtcSession));
        }

        private static string FindHostSession(SignalingRoomInfo room)
        {
            return FindHostSession(room?.Peers, room?.HostId);
        }

        private static string FindHostSession(
            SignalingPeer[] peers,
            string hostId)
        {
            SignalingPeer[] values = peers ?? Array.Empty<SignalingPeer>();
            for (int i = 0; i < values.Length; i++)
            {
                SignalingPeer peer = values[i];
                if (peer == null) continue;
                if ((!string.IsNullOrEmpty(hostId) &&
                     peer.peerId == hostId) ||
                    (string.IsNullOrEmpty(hostId) && peer.isHost))
                {
                    return ValidSessionId(peer.sessionId)
                        ? peer.sessionId
                        : string.Empty;
                }
            }
            return string.Empty;
        }

        private static bool ValidSessionId(string value)
        {
            return value != null &&
                value.Length >= 8 &&
                value.Length <= 64;
        }

        private static string ErrorCode(Exception error, string fallback)
        {
            RoomSignalingException signaling = error as RoomSignalingException;
            return signaling != null ? signaling.Code : fallback;
        }
    }
}
