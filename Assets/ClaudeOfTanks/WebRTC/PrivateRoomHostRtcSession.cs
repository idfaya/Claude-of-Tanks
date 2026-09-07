using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomHostRtcSession : IDisposable
    {
        private readonly RoomSignalingClient _signaling;
        private readonly SignalingRoomInfo _room;
        private readonly RTCConfiguration? _configuration;
        private readonly WebRtcCoroutineDispatcher _coroutines;
        private readonly Dictionary<string, HostPeer> _peers =
            new Dictionary<string, HostPeer>(StringComparer.Ordinal);
        private Task<bool> _restartTask;
        private bool _closed;

        public PrivateRoomHostRtcSession(
            RoomSignalingClient signaling,
            SignalingRoomInfo room,
            MonoBehaviour coroutineOwner,
            PrivateRoomRtcSessionOptions options = null)
        {
            _signaling = signaling ??
                throw new ArgumentNullException(nameof(signaling));
            _room = room ?? throw new ArgumentNullException(nameof(room));
            if (room.PeerId != room.HostId)
                throw new ArgumentException(
                    "Host RTC session requires the canonical room host.",
                    nameof(room));
            _configuration = options?.Configuration;
            _coroutines = new WebRtcCoroutineDispatcher(coroutineOwner);
            _signaling.EventReceived += OnSignalingEvent;
            ReconcilePeers(room.Peers);
        }

        public string RoomCode => _room.RoomCode;
        public int PeerCount => _peers.Count;
        public bool IsRestarting => _restartTask != null;

        public event Action<PrivateRoomPeerTransport> TransportReady;
        public event Action<string, string> PeerLeft;
        public event Action<string, string> Failed;

        public int Pump(int maximumEvents = int.MaxValue)
        {
            RequireOpen();
            AdvanceRoomRestart();
            int delivered = _signaling.PumpEvents(maximumEvents);
            foreach (HostPeer peer in _peers.Values)
            {
                if (peer.Transport != null &&
                    !peer.Transport.IsOpen &&
                    !peer.Restarting)
                {
                    RestartPeer(peer);
                }
            }
            return delivered;
        }

        public bool RestartRoomSession(string reason = "rtc_host_rebuild")
        {
            RequireOpen();
            if (_restartTask != null) return false;
            ClosePeers(reason);
            _restartTask = _signaling.RestartRoomSessionAsync(reason);
            return true;
        }

        public async Task CloseAsync(string reason = "host_closed")
        {
            if (_closed) return;
            _closed = true;
            _signaling.EventReceived -= OnSignalingEvent;
            ClosePeers(reason);
            await _signaling.CloseAsync(reason).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_closed) return;
            try { CloseAsync("disposed").GetAwaiter().GetResult(); }
            catch { }
            TransportReady = null;
            PeerLeft = null;
            Failed = null;
        }

        private void OnSignalingEvent(SignalingEnvelope message)
        {
            SignalingPayload payload = message?.payload;
            if (_closed || payload == null || payload.roomCode != RoomCode)
                return;
            if (message.type == "peer_joined")
            {
                AddOrReplacePeer(
                    payload.peerId,
                    payload.sessionId,
                    payload.player);
            }
            else if (message.type == "room_signal")
            {
                HandleSignal(payload);
            }
            else if (message.type == "peer_left")
            {
                RemovePeer(payload.peerId, payload.reason ?? "peer_left", true);
            }
            else if (message.type == "signaling_resumed")
            {
                ReconcilePeers(payload.peers);
            }
            else if (message.type == "room_closed")
            {
                ClosePeers(payload.reason ?? "room_closed");
            }
        }

        private void ReconcilePeers(SignalingPeer[] peers)
        {
            HashSet<string> present = new HashSet<string>(StringComparer.Ordinal);
            SignalingPeer[] values = peers ?? Array.Empty<SignalingPeer>();
            for (int i = 0; i < values.Length; i++)
            {
                SignalingPeer peer = values[i];
                if (peer == null || peer.peerId == _room.PeerId) continue;
                present.Add(peer.peerId);
                AddOrReplacePeer(peer.peerId, peer.sessionId, peer.player);
            }
            List<string> stale = new List<string>();
            foreach (string peerId in _peers.Keys)
                if (!present.Contains(peerId)) stale.Add(peerId);
            for (int i = 0; i < stale.Count; i++)
                RemovePeer(stale[i], "signaling_reconciled", true);
        }

        private void AddOrReplacePeer(
            string peerId,
            string sessionId,
            SignalingPlayer player)
        {
            if (!ValidIdentity(peerId, 1, 48) ||
                !ValidIdentity(sessionId, 8, 64))
            {
                Fail(peerId, "invalid_peer_identity");
                return;
            }
            HostPeer existing;
            if (_peers.TryGetValue(peerId, out existing))
            {
                if (existing.SessionId == sessionId)
                {
                    RTCPeerConnectionState state =
                        existing.Session.Peer.ConnectionState;
                    if (state == RTCPeerConnectionState.Failed ||
                        state == RTCPeerConnectionState.Disconnected)
                    {
                        RestartPeer(existing);
                    }
                    return;
                }
                RemovePeer(peerId, "peer_replaced", false);
            }

            WebRtcPeerSession session = _configuration.HasValue
                ? new WebRtcPeerSession(
                    WebRtcPeerRole.Host,
                    _configuration.Value)
                : new WebRtcPeerSession(WebRtcPeerRole.Host);
            HostPeer entry = new HostPeer(
                peerId,
                sessionId,
                player ?? new SignalingPlayer
                {
                    id = peerId,
                    name = "Player"
                },
                session);
            _peers.Add(peerId, entry);
            session.SignalReady += signal => SendSignal(entry, signal);
            session.TransportReady += transport =>
                PublishTransport(entry, transport);
            session.ConnectionStateChanged += state =>
                ConnectionStateChanged(entry, state);
            session.Failed += reason => SessionFailed(entry, reason);
            Run(entry, session.Start());
        }

        private void HandleSignal(SignalingPayload payload)
        {
            HostPeer peer;
            if (payload.signal == null ||
                !_peers.TryGetValue(payload.fromPeerId ?? string.Empty, out peer) ||
                payload.fromSessionId != peer.SessionId)
            {
                return;
            }
            Run(peer, peer.Session.HandleSignal(payload.signal));
        }

        private void SendSignal(HostPeer peer, WebRtcSignal signal)
        {
            if (!IsCurrent(peer)) return;
            try
            {
                _signaling.SendSignal(peer.PeerId, peer.SessionId, signal);
            }
            catch (Exception error)
            {
                Fail(peer.PeerId, ErrorCode(error, "signal_send_failed"));
            }
        }

        private void PublishTransport(
            HostPeer peer,
            WebRtcNetworkEndpoint transport)
        {
            if (!IsCurrent(peer))
            {
                transport.Close("rtc_generation_replaced");
                return;
            }
            peer.Transport = transport;
            TransportReady?.Invoke(new PrivateRoomPeerTransport(
                peer.PeerId,
                peer.SessionId,
                peer.Player,
                transport));
        }

        private void ConnectionStateChanged(
            HostPeer peer,
            RTCPeerConnectionState state)
        {
            if (!IsCurrent(peer)) return;
            if (state == RTCPeerConnectionState.Failed ||
                state == RTCPeerConnectionState.Disconnected)
            {
                RestartPeer(peer);
            }
        }

        private void SessionFailed(HostPeer peer, string reason)
        {
            if (!IsCurrent(peer)) return;
            if (reason == "rtc_connection_closed" ||
                reason == "rtc_channel_closed")
            {
                return;
            }
            Fail(peer.PeerId, reason);
            if (reason == "rtc_connection_failed")
                RestartPeer(peer);
        }

        private void RestartPeer(HostPeer peer)
        {
            if (!IsCurrent(peer) || peer.Restarting) return;
            peer.Restarting = true;
            _coroutines.Run(
                peer.Session.Restart(),
                () => IsCurrent(peer),
                error =>
                {
                    peer.Restarting = false;
                    Fail(peer.PeerId, ErrorCode(error, "rtc_restart_failed"));
                },
                () => peer.Restarting = false);
        }

        private void Run(HostPeer peer, System.Collections.IEnumerator routine)
        {
            _coroutines.Run(
                routine,
                () => IsCurrent(peer),
                error => Fail(
                    peer.PeerId,
                    ErrorCode(error, "rtc_operation_failed")));
        }

        private void AdvanceRoomRestart()
        {
            Task<bool> restart = _restartTask;
            if (restart == null || !restart.IsCompleted) return;
            _restartTask = null;
            if (restart.IsFaulted)
            {
                Fail(_room.PeerId, ErrorCode(
                    restart.Exception?.InnerException ?? restart.Exception,
                    "signaling_restart_failed"));
            }
            else if (restart.IsCanceled || !restart.Result)
            {
                Fail(_room.PeerId, "signaling_restart_failed");
            }
        }

        private void RemovePeer(
            string peerId,
            string reason,
            bool publish)
        {
            HostPeer peer;
            if (string.IsNullOrEmpty(peerId) ||
                !_peers.TryGetValue(peerId, out peer))
            {
                return;
            }
            _peers.Remove(peerId);
            peer.Session.Dispose();
            if (publish) PeerLeft?.Invoke(peerId, reason);
        }

        private void ClosePeers(string reason)
        {
            List<string> peerIds = new List<string>(_peers.Keys);
            for (int i = 0; i < peerIds.Count; i++)
                RemovePeer(peerIds[i], reason, false);
        }

        private bool IsCurrent(HostPeer peer)
        {
            HostPeer current;
            return !_closed &&
                _peers.TryGetValue(peer.PeerId, out current) &&
                ReferenceEquals(current, peer);
        }

        private void Fail(string peerId, string reason)
        {
            Failed?.Invoke(peerId ?? string.Empty, reason);
        }

        private void RequireOpen()
        {
            if (_closed)
                throw new ObjectDisposedException(nameof(PrivateRoomHostRtcSession));
        }

        private static bool ValidIdentity(
            string value,
            int minimum,
            int maximum)
        {
            return value != null &&
                value.Length >= minimum &&
                value.Length <= maximum;
        }

        private static string ErrorCode(Exception error, string fallback)
        {
            RoomSignalingException signaling = error as RoomSignalingException;
            return signaling != null ? signaling.Code : fallback;
        }

        private sealed class HostPeer
        {
            public HostPeer(
                string peerId,
                string sessionId,
                SignalingPlayer player,
                WebRtcPeerSession session)
            {
                PeerId = peerId;
                SessionId = sessionId;
                Player = player;
                Session = session;
            }

            public string PeerId { get; }
            public string SessionId { get; }
            public SignalingPlayer Player { get; }
            public WebRtcPeerSession Session { get; }
            public WebRtcNetworkEndpoint Transport;
            public bool Restarting;
        }
    }
}
