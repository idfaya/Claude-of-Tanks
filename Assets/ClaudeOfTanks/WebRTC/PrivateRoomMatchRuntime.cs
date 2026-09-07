using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomAuthoritativeHostRuntime : IDisposable
    {
        private readonly PrivateRoomHostRtcSession _rtc;
        private readonly AuthoritativeMatchHost _authority;
        private readonly LoopbackNetworkTransportPair _localTransport;
        private readonly AuthoritativeHostPump _localHostPump;
        private readonly Dictionary<string, RemotePeerRuntime> _remote =
            new Dictionary<string, RemotePeerRuntime>(StringComparer.Ordinal);
        private bool _disposed;

        public PrivateRoomAuthoritativeHostRuntime(
            PrivateRoomHostRtcSession rtc,
            AuthoritativeMatchHost authority,
            string hostPlayerId,
            string hostEntityId,
            LocalTankPredictor predictor = null)
        {
            _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
            _authority = authority ??
                throw new ArgumentNullException(nameof(authority));
            ValidateRegisteredPeer(authority, hostPlayerId);
            if (string.IsNullOrEmpty(hostEntityId))
                throw new ArgumentException(
                    "Host entity id is required.",
                    nameof(hostEntityId));

            _localTransport = LoopbackNetworkTransportPair.Create();
            _localHostPump = new AuthoritativeHostPump(
                authority,
                hostPlayerId,
                _localTransport.Host);
            LocalClient = new NetworkClientPump(
                hostPlayerId,
                hostEntityId,
                _localTransport.Client,
                predictor);
            _rtc.TransportReady += AttachRemote;
            _rtc.PeerLeft += DetachRemote;
            PrivateRoomPeerTransport[] ready = _rtc.GetReadyTransports();
            for (int i = 0; i < ready.Length; i++)
                AttachRemote(ready[i]);
        }

        public NetworkClientPump LocalClient { get; }
        public int RemotePeerCount => _remote.Count;

        public event Action<string, WebRtcNetworkEndpoint> PeerAttached;
        public event Action<string, string> PeerDetached;
        public event Action<string, string> Failed;

        public int Update(int requestedTicks)
        {
            ThrowIfDisposed();
            _rtc.Pump();
            _localHostPump.PumpIncoming();
            foreach (RemotePeerRuntime peer in _remote.Values)
                peer.Pump.PumpIncoming();

            int advanced = _authority.AdvanceTicks(requestedTicks);
            _localHostPump.PublishSnapshotIfDue();
            List<string> closed = null;
            foreach (RemotePeerRuntime peer in _remote.Values)
            {
                if (!peer.Transport.IsOpen)
                {
                    if (closed == null) closed = new List<string>();
                    closed.Add(peer.PeerId);
                    continue;
                }
                try
                {
                    peer.Pump.PublishSnapshotIfDue();
                }
                catch (InvalidOperationException)
                {
                    if (closed == null) closed = new List<string>();
                    closed.Add(peer.PeerId);
                }
            }
            if (closed != null)
            {
                for (int i = 0; i < closed.Count; i++)
                    DetachRemote(closed[i], "transport_closed");
            }
            LocalClient.Pump();
            return advanced;
        }

        public bool SendLocalInput(NetworkInputCommand command)
        {
            ThrowIfDisposed();
            return LocalClient.SendInput(command);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _rtc.TransportReady -= AttachRemote;
            _rtc.PeerLeft -= DetachRemote;
            List<string> peerIds = new List<string>(_remote.Keys);
            for (int i = 0; i < peerIds.Count; i++)
                DetachRemote(peerIds[i], "host_runtime_disposed");
            LocalClient.Dispose();
            _localHostPump.Dispose();
            _localTransport.Client.Dispose();
            _localTransport.Host.Dispose();
            _rtc.Dispose();
            PeerAttached = null;
            PeerDetached = null;
            Failed = null;
        }

        private void AttachRemote(PrivateRoomPeerTransport connection)
        {
            if (_disposed || connection == null) return;
            try
            {
                ValidateRegisteredPeer(_authority, connection.PeerId);
            }
            catch (Exception error)
            {
                connection.Transport.Close("unregistered_peer");
                Failed?.Invoke(connection.PeerId, error.Message);
                return;
            }

            DetachRemote(connection.PeerId, "transport_replaced", false);
            RemotePeerRuntime peer = new RemotePeerRuntime(
                connection.PeerId,
                connection.Transport,
                new AuthoritativeHostPump(
                    _authority,
                    connection.PeerId,
                    connection.Transport));
            _remote.Add(connection.PeerId, peer);
            PeerAttached?.Invoke(connection.PeerId, connection.Transport);
        }

        private void DetachRemote(string peerId, string reason)
        {
            DetachRemote(peerId, reason, true);
        }

        private void DetachRemote(
            string peerId,
            string reason,
            bool publish)
        {
            RemotePeerRuntime peer;
            if (string.IsNullOrEmpty(peerId) ||
                !_remote.TryGetValue(peerId, out peer))
            {
                return;
            }
            _remote.Remove(peerId);
            peer.Pump.Dispose();
            if (peer.Transport.IsOpen) peer.Transport.Close(reason);
            if (publish) PeerDetached?.Invoke(peerId, reason);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(PrivateRoomAuthoritativeHostRuntime));
        }

        private static void ValidateRegisteredPeer(
            AuthoritativeMatchHost authority,
            string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException("Player id is required.");
            authority.GetSnapshotAcknowledgement(playerId);
        }

        private sealed class RemotePeerRuntime
        {
            public RemotePeerRuntime(
                string peerId,
                WebRtcNetworkEndpoint transport,
                AuthoritativeHostPump pump)
            {
                PeerId = peerId;
                Transport = transport;
                Pump = pump;
            }

            public string PeerId { get; }
            public WebRtcNetworkEndpoint Transport { get; }
            public AuthoritativeHostPump Pump { get; }
        }
    }

    public sealed class PrivateRoomNetworkClientRuntime : IDisposable
    {
        private readonly PrivateRoomClientRtcSession _rtc;
        private readonly string _playerId;
        private readonly string _entityId;
        private readonly LocalTankPredictor _predictor;
        private readonly SnapshotFrameReceiver _receiver =
            new SnapshotFrameReceiver();
        private readonly SnapshotBuffer _buffer = new SnapshotBuffer();
        private NetworkClientPump _client;
        private bool _disposed;

        public PrivateRoomNetworkClientRuntime(
            PrivateRoomClientRtcSession rtc,
            string playerId,
            string entityId,
            LocalTankPredictor predictor = null)
        {
            _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
            _playerId = !string.IsNullOrEmpty(playerId)
                ? playerId
                : throw new ArgumentException(
                    "Player id is required.",
                    nameof(playerId));
            _entityId = !string.IsNullOrEmpty(entityId)
                ? entityId
                : throw new ArgumentException(
                    "Entity id is required.",
                    nameof(entityId));
            _predictor = predictor;
            _rtc.TransportReady += AttachTransport;
            if (_rtc.Transport != null && _rtc.Transport.IsOpen)
                AttachTransport(_rtc.Transport);
        }

        public NetworkWorldSnapshot LatestSnapshot => _receiver.Latest;
        public SnapshotBuffer Buffer => _buffer;
        public int TransportGeneration { get; private set; }
        public bool IsConnected => _client != null &&
            _rtc.Transport != null &&
            _rtc.Transport.IsOpen;

        public event Action<int, WebRtcNetworkEndpoint> TransportAttached;

        public bool SendInput(NetworkInputCommand command)
        {
            ThrowIfDisposed();
            return _client != null && _client.SendInput(command);
        }

        public int Pump()
        {
            ThrowIfDisposed();
            int signalingEvents = _rtc.Pump();
            return signalingEvents + (_client?.Pump() ?? 0);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _rtc.TransportReady -= AttachTransport;
            _client?.Dispose();
            _client = null;
            _rtc.Dispose();
            TransportAttached = null;
        }

        private void AttachTransport(WebRtcNetworkEndpoint transport)
        {
            if (_disposed || transport == null) return;
            _client?.Dispose();
            _client = new NetworkClientPump(
                _playerId,
                _entityId,
                transport,
                _predictor,
                _receiver,
                _buffer);
            TransportGeneration++;
            TransportAttached?.Invoke(TransportGeneration, transport);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(PrivateRoomNetworkClientRuntime));
        }
    }
}
