using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomHostLobbyRuntime : IDisposable
    {
        private readonly PrivateRoomHostRtcSession _rtc;
        private readonly AuthoritativeRoom _room;
        private readonly Dictionary<string, LobbyHostPeerPump> _peers =
            new Dictionary<string, LobbyHostPeerPump>(StringComparer.Ordinal);
        private readonly HashSet<string> _matchReady =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<LobbyHostPeerPump> _pumpScratch =
            new List<LobbyHostPeerPump>();
        private readonly List<string> _closedScratch = new List<string>();
        private RoomMatchPlan _matchPlan;
        private bool _handoffReadyPublished;
        private bool _released;
        private bool _disposed;

        public PrivateRoomHostLobbyRuntime(
            PrivateRoomHostRtcSession rtc,
            AuthoritativeRoom room)
        {
            _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
            _room = room ?? throw new ArgumentNullException(nameof(room));
            if (_rtc.RoomCode != _room.RoomCode ||
                _rtc.LocalPeerId != _room.HostPlayerId)
            {
                throw new ArgumentException(
                    "RTC host and authoritative lobby identities differ.");
            }
            _rtc.TransportReady += AttachPeer;
            _rtc.PeerLeft += OnRtcPeerLeft;
            PrivateRoomPeerTransport[] ready = _rtc.GetReadyTransports();
            for (int i = 0; i < ready.Length; i++) AttachPeer(ready[i]);
        }

        public RoomStateSnapshot State => _room.Snapshot();
        public RoomMatchPlan MatchPlan => _matchPlan;
        public int PeerCount => _peers.Count;
        public bool CanReleaseForMatch =>
            _matchPlan != null && _matchReady.Count == _peers.Count;

        public event Action<RoomStateSnapshot> StateChanged;
        public event Action<RoomMatchPlan> MatchStarting;
        public event Action HandoffReady;
        public event Action<string, string> PeerError;

        public int Pump(int maximumMessagesPerPeer = int.MaxValue)
        {
            RequireActive();
            int delivered = _rtc.Pump();
            _pumpScratch.Clear();
            foreach (LobbyHostPeerPump peer in _peers.Values)
                _pumpScratch.Add(peer);
            for (int i = 0; i < _pumpScratch.Count; i++)
            {
                LobbyHostPeerPump peer = _pumpScratch[i];
                if (_peers.ContainsKey(peer.PlayerId) && peer.IsOpen)
                    delivered += peer.Pump(maximumMessagesPerPeer);
            }
            return delivered;
        }

        public RoomMatchPlan SubmitHostCommand(LobbyCommand command)
        {
            RequireActive();
            return ApplyCommand(_room.HostPlayerId, command, null);
        }

        public PrivateRoomHostMatchHandoff ReleaseForMatch()
        {
            RequireActive();
            if (!CanReleaseForMatch)
                throw new InvalidOperationException(
                    "All connected peers must acknowledge match start.");
            _released = true;
            _rtc.TransportReady -= AttachPeer;
            _rtc.PeerLeft -= OnRtcPeerLeft;
            foreach (LobbyHostPeerPump peer in _peers.Values)
                peer.ReleaseTransport();
            _peers.Clear();
            return new PrivateRoomHostMatchHandoff(
                _rtc,
                _room,
                _matchPlan);
        }

        public void Dispose()
        {
            if (_disposed || _released) return;
            _disposed = true;
            _rtc.TransportReady -= AttachPeer;
            _rtc.PeerLeft -= OnRtcPeerLeft;
            foreach (LobbyHostPeerPump peer in _peers.Values) peer.Dispose();
            _peers.Clear();
            _room.Dispose();
            _rtc.Dispose();
            StateChanged = null;
            MatchStarting = null;
            HandoffReady = null;
            PeerError = null;
        }

        private void AttachPeer(PrivateRoomPeerTransport connection)
        {
            if (_disposed || _released || connection == null) return;
            DetachPeerPump(connection.PeerId, false);
            try
            {
                EnsureRoomPlayer(connection);
            }
            catch (RoomPolicyException error)
            {
                connection.Transport.Close(error.Code);
                PeerError?.Invoke(connection.PeerId, error.Code);
                return;
            }
            LobbyHostPeerPump peer = new LobbyHostPeerPump(
                connection.PeerId,
                connection.Transport);
            peer.StateRequested += SendState;
            peer.CommandReceived += OnCommand;
            peer.MatchReady += OnMatchReady;
            peer.LeaveRequested += OnPeerLeaveRequested;
            _peers.Add(connection.PeerId, peer);
            _matchReady.Remove(connection.PeerId);
            SendState(peer);
            if (_matchPlan != null) peer.SendMatchStart(_matchPlan);
            PublishState();
        }

        private void EnsureRoomPlayer(PrivateRoomPeerTransport connection)
        {
            RoomStateSnapshot state = _room.Snapshot();
            for (int i = 0; i < state.Players.Length; i++)
            {
                if (state.Players[i].PlayerId == connection.PeerId) return;
            }
            _room.Join(
                connection.PeerId,
                connection.Player?.name ?? "Player");
        }

        private void SendState(LobbyHostPeerPump peer)
        {
            if (!_peers.ContainsKey(peer.PlayerId)) return;
            if (!peer.SendState(_room.Snapshot()))
                DetachPeerPump(peer.PlayerId, false);
        }

        private void OnCommand(
            LobbyHostPeerPump peer,
            LobbyCommand command)
        {
            ApplyCommand(peer.PlayerId, command, peer);
        }

        private RoomMatchPlan ApplyCommand(
            string playerId,
            LobbyCommand command,
            LobbyHostPeerPump source)
        {
            try
            {
                RoomMatchPlan plan =
                    LobbyCommandApplier.Apply(_room, playerId, command);
                PublishState();
                if (plan != null) BeginMatchHandoff(plan);
                return plan;
            }
            catch (RoomPolicyException error)
            {
                source?.SendError(error.Code, error.Message);
                PeerError?.Invoke(playerId, error.Code);
                return null;
            }
            catch (Exception error)
            {
                source?.SendError(
                    "invalid_lobby_command",
                    error.Message);
                PeerError?.Invoke(playerId, "invalid_lobby_command");
                return null;
            }
        }

        private void BeginMatchHandoff(RoomMatchPlan plan)
        {
            _matchPlan = plan;
            _matchReady.Clear();
            _handoffReadyPublished = false;
            foreach (LobbyHostPeerPump peer in _peers.Values)
                peer.SendMatchStart(plan);
            MatchStarting?.Invoke(plan);
            PublishHandoffReady();
        }

        private void OnMatchReady(LobbyHostPeerPump peer)
        {
            if (_matchPlan == null || !_peers.ContainsKey(peer.PlayerId))
                return;
            _matchReady.Add(peer.PlayerId);
            PublishHandoffReady();
        }

        private void PublishHandoffReady()
        {
            if (_handoffReadyPublished || !CanReleaseForMatch) return;
            _handoffReadyPublished = true;
            HandoffReady?.Invoke();
        }

        private void PublishState()
        {
            RoomStateSnapshot state = _room.Snapshot();
            _closedScratch.Clear();
            foreach (LobbyHostPeerPump peer in _peers.Values)
            {
                if (!peer.SendState(state))
                    _closedScratch.Add(peer.PlayerId);
            }
            for (int i = 0; i < _closedScratch.Count; i++)
                DetachPeerPump(_closedScratch[i], false);
            StateChanged?.Invoke(state);
        }

        private void OnPeerLeaveRequested(
            LobbyHostPeerPump peer,
            string reason)
        {
            if (reason == "client_leave")
            {
                _room.Leave(peer.PlayerId);
                DetachPeerPump(peer.PlayerId, true);
                PublishState();
            }
            else
            {
                DetachPeerPump(peer.PlayerId, false);
            }
        }

        private void OnRtcPeerLeft(string peerId, string reason)
        {
            _room.Leave(peerId);
            DetachPeerPump(peerId, true);
            PublishState();
        }

        private void DetachPeerPump(string peerId, bool close)
        {
            LobbyHostPeerPump peer;
            if (!_peers.TryGetValue(peerId ?? string.Empty, out peer)) return;
            _peers.Remove(peerId);
            _matchReady.Remove(peerId);
            if (close) peer.Close("lobby_peer_left");
            else peer.ReleaseTransport();
        }

        private void RequireActive()
        {
            if (_disposed || _released)
                throw new ObjectDisposedException(
                    nameof(PrivateRoomHostLobbyRuntime));
        }
    }
}
