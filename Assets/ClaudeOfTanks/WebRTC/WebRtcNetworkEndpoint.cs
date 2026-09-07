using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using Unity.WebRTC;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class WebRtcTransportStats
    {
        public int ControlSent { get; internal set; }
        public int StateSent { get; internal set; }
        public int ControlReceived { get; internal set; }
        public int StateReceived { get; internal set; }
        public int ControlRejected { get; internal set; }
        public int StateCoalesced { get; internal set; }
        public int PeakControlQueue { get; internal set; }
    }

    public sealed class WebRtcNetworkEndpoint : INetworkTransportEndpoint
    {
        public const string ControlChannelLabel = "cot-match-v1";
        public const string StateChannelLabel = "cot-state-v1";
        public const int DefaultMaximumControlQueue = 256;
        public const ulong DefaultMaximumBufferedBytes = 256 * 1024;

        private readonly RTCDataChannel _control;
        private readonly RTCDataChannel _state;
        private readonly int _maximumControlQueue;
        private readonly ulong _maximumBufferedBytes;
        private readonly Queue<byte[]> _incomingControl = new Queue<byte[]>();
        private readonly Queue<byte[]> _outgoingControl = new Queue<byte[]>();
        private byte[] _incomingState;
        private byte[] _outgoingState;
        private string _closedReason;
        private bool _open = true;
        private bool _disposed;

        private WebRtcNetworkEndpoint(
            RTCDataChannel control,
            RTCDataChannel state,
            int maximumControlQueue,
            ulong maximumBufferedBytes)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            if (maximumControlQueue < 1)
                throw new ArgumentOutOfRangeException(nameof(maximumControlQueue));
            if (maximumBufferedBytes < SnapshotWireCodec.MaximumPacketBytes)
                throw new ArgumentOutOfRangeException(nameof(maximumBufferedBytes));
            ValidateChannels(control, state);
            _maximumControlQueue = maximumControlQueue;
            _maximumBufferedBytes = maximumBufferedBytes;
            _control.OnMessage = ReceiveControl;
            _state.OnMessage = ReceiveState;
            _control.OnClose = () => MarkClosed("control_closed");
            _state.OnClose = () => MarkClosed("state_closed");
            _control.OnError = error => MarkClosed(
                "control_error:" + error.errorType);
            _state.OnError = error => MarkClosed(
                "state_error:" + error.errorType);
        }

        public bool IsOpen =>
            _open &&
            _control.ReadyState == RTCDataChannelState.Open &&
            _state.ReadyState == RTCDataChannelState.Open;
        public WebRtcTransportStats Stats { get; } = new WebRtcTransportStats();
        public event Action<byte[]> ControlReceived;
        public event Action<byte[]> StateReceived;
        public event Action<string> Closed;

        public static WebRtcNetworkEndpoint Attach(
            RTCDataChannel control,
            RTCDataChannel state,
            int maximumControlQueue = DefaultMaximumControlQueue,
            ulong maximumBufferedBytes = DefaultMaximumBufferedBytes)
        {
            return new WebRtcNetworkEndpoint(
                control,
                state,
                maximumControlQueue,
                maximumBufferedBytes);
        }

        public static RTCDataChannel CreateControlChannel(RTCPeerConnection peer)
        {
            if (peer == null) throw new ArgumentNullException(nameof(peer));
            return peer.CreateDataChannel(
                ControlChannelLabel,
                new RTCDataChannelInit { ordered = true });
        }

        public static RTCDataChannel CreateStateChannel(RTCPeerConnection peer)
        {
            if (peer == null) throw new ArgumentNullException(nameof(peer));
            return peer.CreateDataChannel(
                StateChannelLabel,
                new RTCDataChannelInit
                {
                    ordered = false,
                    maxRetransmits = 0
                });
        }

        public bool SendControl(byte[] packet)
        {
            byte[] copy = CopyPacket(packet);
            RequireOpen();
            FlushOutgoing();
            if (_control.BufferedAmount <= _maximumBufferedBytes &&
                _outgoingControl.Count == 0)
            {
                _control.Send(copy);
                Stats.ControlSent++;
                return true;
            }
            if (_outgoingControl.Count >= _maximumControlQueue)
            {
                Stats.ControlRejected++;
                return false;
            }
            _outgoingControl.Enqueue(copy);
            Stats.PeakControlQueue = Math.Max(
                Stats.PeakControlQueue,
                _outgoingControl.Count);
            return true;
        }

        public bool SendState(byte[] packet)
        {
            byte[] copy = CopyPacket(packet);
            RequireOpen();
            FlushOutgoing();
            if (_state.BufferedAmount <= _maximumBufferedBytes &&
                _outgoingState == null)
            {
                _state.Send(copy);
                Stats.StateSent++;
                return true;
            }
            if (_outgoingState != null) Stats.StateCoalesced++;
            _outgoingState = copy;
            return true;
        }

        public int Pump(int maximumControlMessages = int.MaxValue)
        {
            if (maximumControlMessages < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumControlMessages));
            FlushOutgoing();
            int delivered = 0;
            while (delivered < maximumControlMessages &&
                _incomingControl.Count > 0)
            {
                byte[] packet = _incomingControl.Dequeue();
                Stats.ControlReceived++;
                delivered++;
                ControlReceived?.Invoke(packet);
            }
            if (_incomingState != null)
            {
                byte[] packet = _incomingState;
                _incomingState = null;
                Stats.StateReceived++;
                delivered++;
                StateReceived?.Invoke(packet);
            }
            if (_closedReason != null)
            {
                string reason = _closedReason;
                _closedReason = null;
                Closed?.Invoke(reason);
            }
            return delivered;
        }

        public void Close(string reason = "closed")
        {
            if (!_open) return;
            _open = false;
            _control.OnMessage = null;
            _state.OnMessage = null;
            _control.Close();
            _state.Close();
            _incomingControl.Clear();
            _outgoingControl.Clear();
            _incomingState = null;
            _outgoingState = null;
            _closedReason = string.IsNullOrEmpty(reason) ? "closed" : reason;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Close("disposed");
            _control.OnOpen = null;
            _state.OnOpen = null;
            _control.OnClose = null;
            _state.OnClose = null;
            _control.OnError = null;
            _state.OnError = null;
            _control.Dispose();
            _state.Dispose();
        }

        private void FlushOutgoing()
        {
            if (!IsOpen) return;
            while (_outgoingControl.Count > 0 &&
                _control.BufferedAmount <= _maximumBufferedBytes)
            {
                _control.Send(_outgoingControl.Dequeue());
                Stats.ControlSent++;
            }
            if (_outgoingState != null &&
                _state.BufferedAmount <= _maximumBufferedBytes)
            {
                byte[] packet = _outgoingState;
                _outgoingState = null;
                _state.Send(packet);
                Stats.StateSent++;
            }
        }

        private void ReceiveControl(byte[] packet)
        {
            if (!_open || !ValidPacket(packet)) return;
            if (_incomingControl.Count >= _maximumControlQueue)
            {
                MarkClosed("control_overflow");
                return;
            }
            _incomingControl.Enqueue(Copy(packet));
        }

        private void ReceiveState(byte[] packet)
        {
            if (!_open || !ValidPacket(packet)) return;
            if (_incomingState != null) Stats.StateCoalesced++;
            _incomingState = Copy(packet);
        }

        private void MarkClosed(string reason)
        {
            if (!_open) return;
            _open = false;
            _closedReason = reason;
        }

        private void RequireOpen()
        {
            if (!IsOpen) throw new InvalidOperationException("WebRTC transport is closed.");
        }

        private static void ValidateChannels(
            RTCDataChannel control,
            RTCDataChannel state)
        {
            if (control.Label != ControlChannelLabel ||
                state.Label != StateChannelLabel)
            {
                throw new ArgumentException("WebRTC channel labels are invalid.");
            }
            if (!control.Ordered)
                throw new ArgumentException(
                    "WebRTC control channel must be ordered.",
                    nameof(control));
            if (state.Ordered || state.MaxRetransmits != 0)
                throw new ArgumentException(
                    "WebRTC state channel must be unordered with zero retransmits.",
                    nameof(state));
            if (control.ReadyState != RTCDataChannelState.Open ||
                state.ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException(
                    "WebRTC channels must be open before attaching.");
            }
        }

        private static byte[] CopyPacket(byte[] packet)
        {
            if (!ValidPacket(packet))
                throw new ArgumentException("Transport packet size is invalid.", nameof(packet));
            return Copy(packet);
        }

        private static bool ValidPacket(byte[] packet)
        {
            return packet != null &&
                packet.Length > 0 &&
                packet.Length <= SnapshotWireCodec.MaximumPacketBytes;
        }

        private static byte[] Copy(byte[] packet)
        {
            byte[] copy = new byte[packet.Length];
            Buffer.BlockCopy(packet, 0, copy, 0, packet.Length);
            return copy;
        }
    }
}
