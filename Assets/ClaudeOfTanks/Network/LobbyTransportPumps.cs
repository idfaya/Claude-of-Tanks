using System;

namespace ClaudeOfTanks.Network
{
    public sealed class LobbyHostPeerPump : IDisposable
    {
        private readonly INetworkTransportEndpoint _transport;
        private uint _sendSequence;
        private uint _lastReceiveSequence;
        private bool _hasReceiveSequence;
        private bool _released;

        public LobbyHostPeerPump(
            string playerId,
            INetworkTransportEndpoint transport)
        {
            PlayerId = !string.IsNullOrEmpty(playerId)
                ? playerId
                : throw new ArgumentException(
                    "Lobby player id is required.",
                    nameof(playerId));
            _transport = transport ??
                throw new ArgumentNullException(nameof(transport));
            _transport.ControlReceived += OnControlReceived;
            _transport.Closed += OnTransportClosed;
        }

        public string PlayerId { get; }
        public bool IsOpen => !_released && _transport.IsOpen;
        public INetworkTransportEndpoint Transport => _transport;

        public event Action<LobbyHostPeerPump> StateRequested;
        public event Action<LobbyHostPeerPump, LobbyCommand> CommandReceived;
        public event Action<LobbyHostPeerPump> MatchReady;
        public event Action<LobbyHostPeerPump, string> LeaveRequested;

        public int Pump(int maximumMessages = int.MaxValue)
        {
            RequireAttached();
            return _transport.Pump(maximumMessages);
        }

        public bool SendState(RoomStateSnapshot state)
        {
            RequireAttached();
            return _transport.SendControl(
                LobbyWireCodec.EncodeState(NextSequence(), state));
        }

        public bool SendMatchStart(RoomMatchPlan plan)
        {
            RequireAttached();
            return _transport.SendControl(
                LobbyWireCodec.EncodeMatchStart(NextSequence(), plan));
        }

        public bool SendError(string code, string message)
        {
            RequireAttached();
            return _transport.SendControl(
                LobbyWireCodec.EncodeError(
                    NextSequence(),
                    string.IsNullOrEmpty(code)
                        ? "invalid_lobby_command"
                        : code,
                    string.IsNullOrEmpty(message)
                        ? "Lobby command was rejected."
                        : message));
        }

        public INetworkTransportEndpoint ReleaseTransport()
        {
            RequireAttached();
            _released = true;
            Unsubscribe();
            return _transport;
        }

        public void Close(string reason = "lobby_peer_closed")
        {
            if (_released) return;
            _released = true;
            Unsubscribe();
            _transport.Close(reason);
        }

        public void Dispose()
        {
            Close("lobby_peer_disposed");
            StateRequested = null;
            CommandReceived = null;
            MatchReady = null;
            LeaveRequested = null;
        }

        private void OnControlReceived(byte[] packet)
        {
            LobbyWireMessage message;
            try
            {
                message = LobbyWireCodec.Decode(packet);
            }
            catch (FormatException error)
            {
                SendError("invalid_lobby_packet", error.Message);
                return;
            }
            if (_hasReceiveSequence &&
                !NetworkProtocol.IsSequenceNewer(
                    message.Sequence,
                    _lastReceiveSequence))
            {
                return;
            }
            _hasReceiveSequence = true;
            _lastReceiveSequence = message.Sequence;

            switch (message.Kind)
            {
                case LobbyWireMessageKind.Hello:
                    StateRequested?.Invoke(this);
                    break;
                case LobbyWireMessageKind.Command:
                    CommandReceived?.Invoke(this, message.Command);
                    break;
                case LobbyWireMessageKind.Leave:
                    LeaveRequested?.Invoke(this, "client_leave");
                    break;
                case LobbyWireMessageKind.MatchReady:
                    MatchReady?.Invoke(this);
                    break;
                case LobbyWireMessageKind.Ping:
                    _transport.SendControl(LobbyWireCodec.EncodePong(
                        NextSequence(),
                        message.PingNonce));
                    break;
                default:
                    SendError(
                        "unexpected_lobby_message",
                        "Message is not valid from a lobby client.");
                    break;
            }
        }

        private void OnTransportClosed(string reason)
        {
            if (_released) return;
            LeaveRequested?.Invoke(
                this,
                string.IsNullOrEmpty(reason) ? "transport_closed" : reason);
        }

        private uint NextSequence()
        {
            uint result = _sendSequence;
            _sendSequence++;
            return result;
        }

        private void RequireAttached()
        {
            if (_released)
                throw new ObjectDisposedException(nameof(LobbyHostPeerPump));
        }

        private void Unsubscribe()
        {
            _transport.ControlReceived -= OnControlReceived;
            _transport.Closed -= OnTransportClosed;
        }
    }

    public sealed class LobbyClientPump : IDisposable
    {
        private readonly INetworkTransportEndpoint _transport;
        private uint _sendSequence;
        private uint _lastReceiveSequence;
        private bool _hasReceiveSequence;
        private bool _released;

        public LobbyClientPump(INetworkTransportEndpoint transport)
        {
            _transport = transport ??
                throw new ArgumentNullException(nameof(transport));
            _transport.ControlReceived += OnControlReceived;
            _transport.Closed += OnTransportClosed;
            SendHello();
        }

        public RoomStateSnapshot State { get; private set; }
        public RoomMatchPlan MatchPlan { get; private set; }
        public string LastErrorCode { get; private set; }
        public string LastErrorMessage { get; private set; }
        public bool IsOpen => !_released && _transport.IsOpen;
        public INetworkTransportEndpoint Transport => _transport;

        public event Action<RoomStateSnapshot> StateChanged;
        public event Action<RoomMatchPlan> MatchStarting;
        public event Action<string, string> ErrorReceived;
        public event Action<string> Closed;

        public int Pump(int maximumMessages = int.MaxValue)
        {
            RequireAttached();
            return _transport.Pump(maximumMessages);
        }

        public bool Submit(LobbyCommand command)
        {
            RequireAttached();
            return _transport.SendControl(
                LobbyWireCodec.EncodeCommand(NextSequence(), command));
        }

        public bool SendPing(uint nonce)
        {
            RequireAttached();
            return _transport.SendControl(
                LobbyWireCodec.EncodePing(NextSequence(), nonce));
        }

        public INetworkTransportEndpoint ReleaseTransport()
        {
            RequireAttached();
            _released = true;
            Unsubscribe();
            return _transport;
        }

        public void Close(string reason = "lobby_client_closed")
        {
            if (_released) return;
            if (_transport.IsOpen)
            {
                try
                {
                    _transport.SendControl(
                        LobbyWireCodec.EncodeLeave(NextSequence()));
                }
                catch (InvalidOperationException)
                {
                    // The remote may close between the state check and send.
                }
            }
            _released = true;
            Unsubscribe();
            _transport.Close(reason);
        }

        public void Dispose()
        {
            Close("lobby_client_disposed");
            StateChanged = null;
            MatchStarting = null;
            ErrorReceived = null;
            Closed = null;
        }

        private void SendHello()
        {
            _transport.SendControl(
                LobbyWireCodec.EncodeHello(NextSequence()));
        }

        private void OnControlReceived(byte[] packet)
        {
            LobbyWireMessage message;
            try
            {
                message = LobbyWireCodec.Decode(packet);
            }
            catch (FormatException error)
            {
                PublishError("invalid_lobby_packet", error.Message);
                return;
            }
            if (_hasReceiveSequence &&
                !NetworkProtocol.IsSequenceNewer(
                    message.Sequence,
                    _lastReceiveSequence))
            {
                return;
            }
            _hasReceiveSequence = true;
            _lastReceiveSequence = message.Sequence;
            if (message.Kind == LobbyWireMessageKind.State)
            {
                if (State == null ||
                    message.State.Revision >= State.Revision)
                {
                    State = message.State;
                    StateChanged?.Invoke(State);
                }
            }
            else if (message.Kind == LobbyWireMessageKind.MatchStart)
            {
                MatchPlan = message.MatchPlan;
                _transport.SendControl(
                    LobbyWireCodec.EncodeMatchReady(NextSequence()));
                MatchStarting?.Invoke(MatchPlan);
            }
            else if (message.Kind == LobbyWireMessageKind.Error)
            {
                PublishError(message.ErrorCode, message.ErrorMessage);
            }
        }

        private void OnTransportClosed(string reason)
        {
            if (!_released) Closed?.Invoke(reason);
        }

        private void PublishError(string code, string message)
        {
            LastErrorCode = code;
            LastErrorMessage = message;
            ErrorReceived?.Invoke(code, message);
        }

        private uint NextSequence()
        {
            uint result = _sendSequence;
            _sendSequence++;
            return result;
        }

        private void RequireAttached()
        {
            if (_released)
                throw new ObjectDisposedException(nameof(LobbyClientPump));
        }

        private void Unsubscribe()
        {
            _transport.ControlReceived -= OnControlReceived;
            _transport.Closed -= OnTransportClosed;
        }
    }
}
