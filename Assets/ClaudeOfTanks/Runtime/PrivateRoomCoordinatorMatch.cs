using System;
using ClaudeOfTanks.WebRTC;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class PrivateRoomCoordinator
    {
        public PrivateRoomHostMatchHandoff TakeHostHandoff()
        {
            if (_hostLobby == null || !_hostLobby.CanReleaseForMatch)
                throw new InvalidOperationException(
                    "Host match handoff is not ready.");
            PrivateRoomHostMatchHandoff handoff =
                _hostLobby.ReleaseForMatch();
            UnsubscribeHost();
            _hostLobby = null;
            _hostRtc = null;
            _signaling = null;
            return handoff;
        }

        public PrivateRoomClientMatchHandoff TakeClientHandoff()
        {
            if (_clientLobby == null || _clientLobby.MatchPlan == null)
                throw new InvalidOperationException(
                    "Client match handoff is not ready.");
            PrivateRoomClientMatchHandoff handoff =
                _clientLobby.ReleaseForMatch();
            UnsubscribeClient();
            _clientLobby = null;
            _clientRtc = null;
            _signaling = null;
            return handoff;
        }

        public void ResumeHostLobby(PrivateRoomHostLobbyRuntime lobby)
        {
            RequireMatchTransition(PrivateRoomRole.Host);
            _hostLobby = lobby ??
                throw new ArgumentNullException(nameof(lobby));
            _hostLobby.StateChanged += OnLobbyState;
            _hostLobby.MatchStarting += OnMatchStarting;
            _hostLobby.HandoffReady += OnHandoffReady;
            _hostLobby.PeerError += OnPeerError;
            State = PrivateRoomUiState.Lobby;
            OnLobbyState(_hostLobby.State);
        }

        public void ResumeClientLobby(PrivateRoomClientLobbyRuntime lobby)
        {
            RequireMatchTransition(PrivateRoomRole.Client);
            _clientLobby = lobby ??
                throw new ArgumentNullException(nameof(lobby));
            _clientLobby.StateChanged += OnLobbyState;
            _clientLobby.MatchStarting += OnMatchStarting;
            _clientLobby.ErrorReceived += OnPeerError;
            State = PrivateRoomUiState.Lobby;
            Changed?.Invoke();
        }

        private void RequireMatchTransition(PrivateRoomRole expectedRole)
        {
            if (Role != expectedRole ||
                State != PrivateRoomUiState.Starting ||
                _hostLobby != null ||
                _clientLobby != null)
            {
                throw new InvalidOperationException(
                    "Private room is not in a match transition.");
            }
        }
    }
}
