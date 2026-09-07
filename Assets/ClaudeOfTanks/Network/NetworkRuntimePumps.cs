using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Network
{
    public sealed class AuthoritativeHostPump : IDisposable
    {
        public const int KeyframeIntervalTicks = NetworkProtocol.TickRate;
        public const int SnapshotHistoryCapacity = 128;
        private readonly AuthoritativeMatchHost _host;
        private readonly string _playerId;
        private readonly INetworkTransportEndpoint _transport;
        private readonly Dictionary<long, NetworkWorldSnapshot> _snapshots =
            new Dictionary<long, NetworkWorldSnapshot>();
        private readonly Queue<long> _snapshotOrder = new Queue<long>();
        private long _lastPublishedTick = -1;
        private bool _disposed;

        public AuthoritativeHostPump(
            AuthoritativeMatchHost host,
            string playerId,
            INetworkTransportEndpoint transport)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _playerId = !string.IsNullOrEmpty(playerId)
                ? playerId
                : throw new ArgumentException("Player id is required.", nameof(playerId));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _transport.ControlReceived += OnControlPacket;
        }

        public int InvalidInputPackets { get; private set; }
        public int RejectedInputs { get; private set; }
        public int SnapshotsSent { get; private set; }
        public int KeyframesSent { get; private set; }
        public int DeltasSent { get; private set; }

        public int Update(int requestedTicks)
        {
            ThrowIfDisposed();
            PumpIncoming();
            int count = Math.Max(
                0,
                Math.Min(requestedTicks, AuthoritativeMatchHost.MaximumCatchUpTicks));
            for (int i = 0; i < count; i++)
            {
                _host.AdvanceTicks(1);
                PublishSnapshotIfDue();
            }
            return count;
        }

        public int PumpIncoming(int maximumControlMessages = int.MaxValue)
        {
            ThrowIfDisposed();
            return _transport.Pump(maximumControlMessages);
        }

        public bool PublishSnapshotIfDue()
        {
            ThrowIfDisposed();
            if (!_host.ShouldPublishSnapshot || _lastPublishedTick == _host.Tick)
                return false;
            PublishSnapshot();
            _lastPublishedTick = _host.Tick;
            return true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _transport.ControlReceived -= OnControlPacket;
            _snapshots.Clear();
            _snapshotOrder.Clear();
        }

        private void OnControlPacket(byte[] packet)
        {
            try
            {
                NetworkInputCommand command = InputWireCodec.Decode(packet);
                if (command.PlayerId != _playerId)
                {
                    RejectedInputs++;
                    return;
                }
                InputAdmission admission = _host.SubmitInput(command);
                if (admission != InputAdmission.Accepted &&
                    admission != InputAdmission.Stale)
                {
                    RejectedInputs++;
                }
            }
            catch (FormatException)
            {
                InvalidInputPackets++;
            }
        }

        private void PublishSnapshot()
        {
            NetworkWorldSnapshot current = _host.CreateSnapshot(_playerId);
            long acknowledgedTick = _host.GetSnapshotAcknowledgement(_playerId);
            NetworkWorldSnapshot baseline = null;
            bool forceKeyframe =
                current.Tick % KeyframeIntervalTicks == 0 ||
                acknowledgedTick < 0 ||
                !_snapshots.TryGetValue(acknowledgedTick, out baseline);
            NetworkSnapshotFrame frame = SnapshotDelta.Create(
                current,
                forceKeyframe ? null : baseline);
            _transport.SendState(SnapshotFrameWireCodec.Encode(frame));
            SnapshotsSent++;
            if (frame.IsKeyframe) KeyframesSent++;
            else DeltasSent++;
            Cache(current);
        }

        private void Cache(NetworkWorldSnapshot snapshot)
        {
            _snapshots[snapshot.Tick] = snapshot;
            _snapshotOrder.Enqueue(snapshot.Tick);
            while (_snapshotOrder.Count > SnapshotHistoryCapacity)
            {
                long tick = _snapshotOrder.Dequeue();
                _snapshots.Remove(tick);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AuthoritativeHostPump));
        }
    }

    public sealed class NetworkClientPump : IDisposable
    {
        private readonly string _playerId;
        private readonly string _entityId;
        private readonly INetworkTransportEndpoint _transport;
        private readonly SnapshotFrameReceiver _receiver;
        private readonly SnapshotBuffer _buffer;
        private readonly LocalTankPredictor _predictor;
        private bool _disposed;

        public NetworkClientPump(
            string playerId,
            string entityId,
            INetworkTransportEndpoint transport,
            LocalTankPredictor predictor = null,
            SnapshotFrameReceiver receiver = null,
            SnapshotBuffer buffer = null)
        {
            _playerId = !string.IsNullOrEmpty(playerId)
                ? playerId
                : throw new ArgumentException("Player id is required.", nameof(playerId));
            _entityId = !string.IsNullOrEmpty(entityId)
                ? entityId
                : throw new ArgumentException("Entity id is required.", nameof(entityId));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _predictor = predictor;
            _receiver = receiver ?? new SnapshotFrameReceiver();
            _buffer = buffer ?? new SnapshotBuffer();
            _transport.StateReceived += OnStatePacket;
        }

        public NetworkWorldSnapshot LatestSnapshot => _receiver.Latest;
        public SnapshotBuffer Buffer => _buffer;
        public bool NeedsKeyframe => _receiver.NeedsKeyframe;
        public int InvalidStatePackets { get; private set; }
        public int MissingBaseFrames { get; private set; }
        public int SnapshotsAccepted { get; private set; }

        public bool SendInput(NetworkInputCommand command)
        {
            ThrowIfDisposed();
            command.PlayerId = _playerId;
            command.SnapshotAckTick =
                _receiver.NeedsKeyframe || _receiver.Latest == null
                    ? -1
                    : _receiver.Latest.Tick;
            byte[] packet = InputWireCodec.Encode(command);
            bool accepted = _transport.SendControl(packet);
            if (accepted && _predictor != null) _predictor.Predict(command);
            return accepted;
        }

        public int Pump()
        {
            ThrowIfDisposed();
            return _transport.Pump();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _transport.StateReceived -= OnStatePacket;
        }

        private void OnStatePacket(byte[] packet)
        {
            try
            {
                NetworkSnapshotFrame frame = SnapshotFrameWireCodec.Decode(packet);
                NetworkWorldSnapshot snapshot;
                SnapshotFrameAdmission admission = _receiver.Receive(frame, out snapshot);
                if (admission == SnapshotFrameAdmission.MissingBase)
                {
                    MissingBaseFrames++;
                    return;
                }
                if (admission != SnapshotFrameAdmission.Accepted)
                {
                    InvalidStatePackets++;
                    return;
                }

                _buffer.Push(snapshot);
                SnapshotsAccepted++;
                if (_predictor != null && snapshot.AcknowledgedInputSequence.HasValue)
                {
                    NetworkEntitySnapshot entity = Find(snapshot, _entityId);
                    if (entity != null)
                        _predictor.Reconcile(entity, snapshot.AcknowledgedInputSequence.Value);
                }
            }
            catch (FormatException)
            {
                InvalidStatePackets++;
            }
        }

        private static NetworkEntitySnapshot Find(
            NetworkWorldSnapshot snapshot,
            string entityId)
        {
            for (int i = 0; i < snapshot.Entities.Length; i++)
                if (snapshot.Entities[i].EntityId == entityId) return snapshot.Entities[i];
            return null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NetworkClientPump));
        }
    }
}
