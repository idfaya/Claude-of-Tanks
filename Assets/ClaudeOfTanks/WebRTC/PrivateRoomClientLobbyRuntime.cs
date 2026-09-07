using System;
using ClaudeOfTanks.Network;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomClientLobbyRuntime : IDisposable
    {
        private readonly PrivateRoomClientRtcSession _rtc;
        private LobbyClientPump _client;
        private RoomStateSnapshot _state;
        private RoomMatchPlan _matchPlan;
        private string _lastErrorCode;
        private string _lastErrorMessage;
        private bool _released;
        private bool _disposed;

        public PrivateRoomClientLobbyRuntime(
            PrivateRoomClientRtcSession rtc)
        {
            _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
            _rtc.TransportReady += AttachTransport;
            if (_rtc.Transport != null && _rtc.Transport.IsOpen)
                AttachTransport(_rtc.Transport);
        }

        public RoomStateSnapshot State => _state;
        public RoomMatchPlan MatchPlan => _matchPlan;
        public string LastErrorCode => _lastErrorCode;
        public string LastErrorMessage => _lastErrorMessage;
        public bool IsConnected => _client != null && _client.IsOpen;
        public int TransportGeneration { get; private set; }

        public event Action<RoomStateSnapshot> StateChanged;
        public event Action<RoomMatchPlan> MatchStarting;
        public event Action<string, string> ErrorReceived;

        public int Pump(int maximumMessages = int.MaxValue)
        {
            RequireActive();
            int delivered = _rtc.Pump();
            if (_client != null && _client.IsOpen)
                delivered += _client.Pump(maximumMessages);
            return delivered;
        }

        public bool Submit(LobbyCommand command)
        {
            RequireActive();
            return _client != null && _client.Submit(command);
        }

        public PrivateRoomClientMatchHandoff ReleaseForMatch()
        {
            RequireActive();
            RoomMatchPlan plan = MatchPlan;
            if (plan == null)
                throw new InvalidOperationException(
                    "Match start has not been received.");
            if (_client == null)
                throw new InvalidOperationException(
                    "Lobby transport is unavailable.");
            _released = true;
            _rtc.TransportReady -= AttachTransport;
            _client.ReleaseTransport();
            _client = null;
            return new PrivateRoomClientMatchHandoff(
                _rtc,
                _rtc.LocalPeerId,
                plan);
        }

        public void Dispose()
        {
            if (_disposed || _released) return;
            _disposed = true;
            _rtc.TransportReady -= AttachTransport;
            _client?.Dispose();
            _client = null;
            _rtc.Dispose();
            StateChanged = null;
            MatchStarting = null;
            ErrorReceived = null;
        }

        private void AttachTransport(WebRtcNetworkEndpoint transport)
        {
            if (_disposed || _released || transport == null) return;
            if (_client != null)
            {
                _client.StateChanged -= PublishState;
                _client.MatchStarting -= PublishMatch;
                _client.ErrorReceived -= PublishError;
                _client.ReleaseTransport();
            }
            _client = new LobbyClientPump(transport);
            _client.StateChanged += PublishState;
            _client.MatchStarting += PublishMatch;
            _client.ErrorReceived += PublishError;
            TransportGeneration++;
        }

        private void PublishState(RoomStateSnapshot state)
        {
            if (_state != null && state.Revision < _state.Revision) return;
            _state = state;
            StateChanged?.Invoke(state);
        }

        private void PublishMatch(RoomMatchPlan plan)
        {
            _matchPlan = plan;
            MatchStarting?.Invoke(plan);
        }

        private void PublishError(string code, string message)
        {
            _lastErrorCode = code;
            _lastErrorMessage = message;
            ErrorReceived?.Invoke(code, message);
        }

        private void RequireActive()
        {
            if (_disposed || _released)
                throw new ObjectDisposedException(
                    nameof(PrivateRoomClientLobbyRuntime));
        }
    }
}
