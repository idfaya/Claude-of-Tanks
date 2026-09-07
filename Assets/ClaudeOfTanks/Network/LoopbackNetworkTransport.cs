using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Network
{
    public sealed class LoopbackTransportStats
    {
        public int ControlSent { get; internal set; }
        public int StateSent { get; internal set; }
        public int ControlReceived { get; internal set; }
        public int StateReceived { get; internal set; }
        public int Rejected { get; internal set; }
        public int StateCoalesced { get; internal set; }
        public int StateDropped { get; internal set; }
        public int PeakControlQueue { get; internal set; }
    }

    public sealed class LoopbackNetworkEndpoint
    {
        private readonly Queue<byte[]> _controlQueue = new Queue<byte[]>();
        private readonly int _maximumControlQueue;
        private LoopbackNetworkEndpoint _peer;
        private byte[] _pendingState;

        internal LoopbackNetworkEndpoint(string label, int maximumControlQueue)
        {
            Label = label;
            _maximumControlQueue = maximumControlQueue;
        }

        public string Label { get; }
        public bool IsOpen { get; private set; } = true;
        public int BufferedControlMessages => _controlQueue.Count;
        public bool HasBufferedState => _pendingState != null;
        public LoopbackTransportStats Stats { get; } = new LoopbackTransportStats();
        public event Action<byte[]> ControlReceived;
        public event Action<byte[]> StateReceived;
        public event Action<string> Closed;

        internal void SetPeer(LoopbackNetworkEndpoint peer)
        {
            _peer = peer;
        }

        public bool SendControl(byte[] packet)
        {
            RequirePacket(packet);
            RequireOpen();
            if (!_peer.EnqueueControl(Clone(packet)))
            {
                Stats.Rejected++;
                return false;
            }
            Stats.ControlSent++;
            return true;
        }

        public bool SendState(byte[] packet)
        {
            RequirePacket(packet);
            RequireOpen();
            bool replaced = _peer.ReplaceState(Clone(packet));
            Stats.StateSent++;
            if (replaced) Stats.StateCoalesced++;
            return true;
        }

        public int Pump(int maximumControlMessages = int.MaxValue)
        {
            if (maximumControlMessages < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumControlMessages));
            if (!IsOpen)
            {
                _controlQueue.Clear();
                _pendingState = null;
                return 0;
            }

            int delivered = 0;
            while (delivered < maximumControlMessages && _controlQueue.Count > 0)
            {
                byte[] packet = _controlQueue.Dequeue();
                Stats.ControlReceived++;
                delivered++;
                ControlReceived?.Invoke(packet);
            }
            if (_pendingState != null)
            {
                byte[] packet = _pendingState;
                _pendingState = null;
                Stats.StateReceived++;
                delivered++;
                StateReceived?.Invoke(packet);
            }
            return delivered;
        }

        public bool DropPendingState()
        {
            if (_pendingState == null) return false;
            _pendingState = null;
            Stats.StateDropped++;
            return true;
        }

        public void Close(string reason = "closed")
        {
            FinishClose(reason, true);
        }

        private bool EnqueueControl(byte[] packet)
        {
            if (!IsOpen || _controlQueue.Count >= _maximumControlQueue)
            {
                Stats.Rejected++;
                return false;
            }
            _controlQueue.Enqueue(packet);
            if (_controlQueue.Count > Stats.PeakControlQueue)
                Stats.PeakControlQueue = _controlQueue.Count;
            return true;
        }

        private bool ReplaceState(byte[] packet)
        {
            if (!IsOpen) throw new InvalidOperationException("Transport peer is closed.");
            bool replaced = _pendingState != null;
            _pendingState = packet;
            return replaced;
        }

        private void FinishClose(string reason, bool notifyPeer)
        {
            if (!IsOpen) return;
            IsOpen = false;
            _controlQueue.Clear();
            _pendingState = null;
            Closed?.Invoke(string.IsNullOrEmpty(reason) ? "closed" : reason);
            ControlReceived = null;
            StateReceived = null;
            Closed = null;
            if (notifyPeer && _peer != null) _peer.FinishClose(reason, false);
        }

        private void RequireOpen()
        {
            if (!IsOpen || _peer == null || !_peer.IsOpen)
                throw new InvalidOperationException("Transport is closed.");
        }

        private static void RequirePacket(byte[] packet)
        {
            if (packet == null || packet.Length == 0 ||
                packet.Length > SnapshotWireCodec.MaximumPacketBytes)
            {
                throw new ArgumentException("Transport packet size is invalid.", nameof(packet));
            }
        }

        private static byte[] Clone(byte[] packet)
        {
            byte[] copy = new byte[packet.Length];
            Buffer.BlockCopy(packet, 0, copy, 0, packet.Length);
            return copy;
        }
    }

    public sealed class LoopbackNetworkTransportPair
    {
        private LoopbackNetworkTransportPair(
            LoopbackNetworkEndpoint client,
            LoopbackNetworkEndpoint host)
        {
            Client = client;
            Host = host;
        }

        public LoopbackNetworkEndpoint Client { get; }
        public LoopbackNetworkEndpoint Host { get; }

        public static LoopbackNetworkTransportPair Create(int maximumControlQueue = 256)
        {
            if (maximumControlQueue < 1)
                throw new ArgumentOutOfRangeException(nameof(maximumControlQueue));
            LoopbackNetworkEndpoint client =
                new LoopbackNetworkEndpoint("client", maximumControlQueue);
            LoopbackNetworkEndpoint host =
                new LoopbackNetworkEndpoint("host", maximumControlQueue);
            client.SetPeer(host);
            host.SetPeer(client);
            return new LoopbackNetworkTransportPair(client, host);
        }
    }
}
