using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeOfTanks.Network
{
    public enum NetworkTransportLane : byte
    {
        Control = 1,
        State = 2
    }

    public sealed class WebSocketTransportStats
    {
        public int ControlSent { get; internal set; }
        public int StateSent { get; internal set; }
        public int ControlReceived { get; internal set; }
        public int StateReceived { get; internal set; }
        public int Rejected { get; internal set; }
        public int StateCoalesced { get; internal set; }
        public int StateDropped { get; internal set; }
        public int PeakControlQueue { get; internal set; }
        public int InvalidFrames { get; internal set; }
    }

    public interface IWebSocketConnection : IDisposable
    {
        WebSocketState State { get; }
        Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken);
        Task SendAsync(
            ArraySegment<byte> payload,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken);
        Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> payload,
            CancellationToken cancellationToken);
        Task CloseAsync(
            WebSocketCloseStatus status,
            string reason,
            CancellationToken cancellationToken);
    }

    public sealed class ClientWebSocketConnection : IWebSocketConnection
    {
        private readonly ClientWebSocket _socket = new ClientWebSocket();

        public ClientWebSocketConnection(string origin = null)
        {
            if (!string.IsNullOrEmpty(origin))
                _socket.Options.SetRequestHeader("Origin", origin);
        }

        public WebSocketState State => _socket.State;

        public Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
        {
            return _socket.ConnectAsync(endpoint, cancellationToken);
        }

        public Task SendAsync(
            ArraySegment<byte> payload,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            return _socket.SendAsync(payload, messageType, endOfMessage, cancellationToken);
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> payload,
            CancellationToken cancellationToken)
        {
            return _socket.ReceiveAsync(payload, cancellationToken);
        }

        public Task CloseAsync(
            WebSocketCloseStatus status,
            string reason,
            CancellationToken cancellationToken)
        {
            return _socket.CloseAsync(status, reason, cancellationToken);
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }

    public sealed class DedicatedSocketConnection : IDisposable
    {
        internal DedicatedSocketConnection(
            WebSocketNetworkEndpoint transport,
            DedicatedSocketAuthResponse admission)
        {
            Transport = transport;
            Admission = admission;
        }

        public WebSocketNetworkEndpoint Transport { get; }
        public DedicatedSocketAuthResponse Admission { get; }

        public void Dispose()
        {
            Transport.Dispose();
        }
    }

    public static class WebSocketLaneCodec
    {
        public const int HeaderBytes = 5;
        private const byte MagicC = 0x43;
        private const byte MagicO = 0x4f;
        private const byte MagicT = 0x54;
        private const byte Version = 1;

        public static byte[] Encode(NetworkTransportLane lane, byte[] payload)
        {
            RequireLane(lane);
            RequirePayload(payload);
            byte[] frame = new byte[payload.Length + HeaderBytes];
            frame[0] = MagicC;
            frame[1] = MagicO;
            frame[2] = MagicT;
            frame[3] = Version;
            frame[4] = (byte)lane;
            Buffer.BlockCopy(payload, 0, frame, HeaderBytes, payload.Length);
            return frame;
        }

        public static byte[] Decode(byte[] frame, out NetworkTransportLane lane)
        {
            if (frame == null ||
                frame.Length <= HeaderBytes ||
                frame.Length > SnapshotWireCodec.MaximumPacketBytes + HeaderBytes ||
                frame[0] != MagicC ||
                frame[1] != MagicO ||
                frame[2] != MagicT ||
                frame[3] != Version)
            {
                throw new FormatException("WebSocket transport frame is invalid.");
            }

            lane = (NetworkTransportLane)frame[4];
            RequireLane(lane);
            byte[] payload = new byte[frame.Length - HeaderBytes];
            Buffer.BlockCopy(frame, HeaderBytes, payload, 0, payload.Length);
            return payload;
        }

        private static void RequireLane(NetworkTransportLane lane)
        {
            if (lane != NetworkTransportLane.Control &&
                lane != NetworkTransportLane.State)
            {
                throw new FormatException("WebSocket transport lane is invalid.");
            }
        }

        private static void RequirePayload(byte[] payload)
        {
            if (payload == null ||
                payload.Length == 0 ||
                payload.Length > SnapshotWireCodec.MaximumPacketBytes)
            {
                throw new ArgumentException("Transport packet size is invalid.", nameof(payload));
            }
        }
    }

    public sealed class WebSocketNetworkEndpoint : INetworkTransportEndpoint
    {
        public const int DefaultMaximumControlQueue = 256;
        public const int MaximumAuthenticationBytes = 16 * 1024;

        private readonly object _gate = new object();
        private readonly IWebSocketConnection _connection;
        private readonly int _maximumControlQueue;
        private readonly Queue<byte[]> _incomingControl = new Queue<byte[]>();
        private readonly Queue<byte[]> _outgoingControl = new Queue<byte[]>();
        private readonly SemaphoreSlim _outgoingSignal = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private byte[] _incomingState;
        private byte[] _outgoingState;
        private string _pendingClosedReason;
        private bool _open;
        private bool _disposed;

        private WebSocketNetworkEndpoint(
            IWebSocketConnection connection,
            int maximumControlQueue)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            if (maximumControlQueue < 1)
                throw new ArgumentOutOfRangeException(nameof(maximumControlQueue));
            if (connection.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket connection must already be open.");
            _maximumControlQueue = maximumControlQueue;
            _open = true;
            Task.Run(() => SendLoopAsync(_lifetime.Token));
            Task.Run(() => ReceiveLoopAsync(_lifetime.Token));
        }

        public bool IsOpen
        {
            get
            {
                lock (_gate) return _open;
            }
        }

        public WebSocketTransportStats Stats { get; } = new WebSocketTransportStats();
        public event Action<byte[]> ControlReceived;
        public event Action<byte[]> StateReceived;
        public event Action<string> Closed;

        public static WebSocketNetworkEndpoint Attach(
            IWebSocketConnection connection,
            int maximumControlQueue = DefaultMaximumControlQueue)
        {
            return new WebSocketNetworkEndpoint(connection, maximumControlQueue);
        }

        public static async Task<WebSocketNetworkEndpoint> ConnectAsync(
            Uri endpoint,
            string authenticationJson = null,
            int maximumControlQueue = DefaultMaximumControlQueue,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (endpoint.Scheme != "ws" && endpoint.Scheme != "wss")
                throw new ArgumentException("WebSocket endpoint must use ws or wss.", nameof(endpoint));

            ClientWebSocketConnection connection = new ClientWebSocketConnection();
            try
            {
                await connection.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(authenticationJson))
                {
                    byte[] authentication = Encoding.UTF8.GetBytes(authenticationJson);
                    if (authentication.Length > MaximumAuthenticationBytes)
                        throw new ArgumentException("WebSocket authentication is too large.", nameof(authenticationJson));
                    await connection.SendAsync(
                        new ArraySegment<byte>(authentication),
                        WebSocketMessageType.Text,
                        true,
                        cancellationToken).ConfigureAwait(false);
                }
                return Attach(connection, maximumControlQueue);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static async Task<DedicatedSocketConnection> ConnectDedicatedAsync(
            Uri endpoint,
            DedicatedSocketAuthRequest request,
            int maximumControlQueue = DefaultMaximumControlQueue,
            int timeoutMs = 8000,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (endpoint.Scheme != "ws" && endpoint.Scheme != "wss")
                throw new ArgumentException("WebSocket endpoint must use ws or wss.", nameof(endpoint));
            if (timeoutMs < 1) throw new ArgumentOutOfRangeException(nameof(timeoutMs));

            ClientWebSocketConnection connection = new ClientWebSocketConnection();
            using (CancellationTokenSource timeout =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timeout.CancelAfter(timeoutMs);
                try
                {
                    await connection.ConnectAsync(endpoint, timeout.Token).ConfigureAwait(false);
                    byte[] requestPacket = DedicatedSocketProtocol.EncodeRequest(request);
                    await connection.SendAsync(
                        new ArraySegment<byte>(requestPacket),
                        WebSocketMessageType.Binary,
                        true,
                        timeout.Token).ConfigureAwait(false);
                    byte[] responsePacket = await ReceiveSingleBinaryMessageAsync(
                        connection,
                        DedicatedSocketProtocol.MaximumHandshakeBytes,
                        timeout.Token).ConfigureAwait(false);
                    DedicatedSocketAuthResponse response =
                        DedicatedSocketProtocol.DecodeResponse(responsePacket);
                    if (!string.Equals(response.MatchId, request.MatchId, StringComparison.Ordinal) ||
                        !string.Equals(response.PlayerId, request.PlayerId, StringComparison.Ordinal))
                    {
                        throw new FormatException("Dedicated auth response identity does not match.");
                    }
                    return new DedicatedSocketConnection(
                        Attach(connection, maximumControlQueue),
                        response);
                }
                catch
                {
                    connection.Dispose();
                    throw;
                }
            }
        }

        public bool SendControl(byte[] packet)
        {
            byte[] copy = ClonePacket(packet);
            lock (_gate)
            {
                RequireOpen();
                if (_outgoingControl.Count >= _maximumControlQueue)
                {
                    Stats.Rejected++;
                    return false;
                }
                _outgoingControl.Enqueue(copy);
                if (_outgoingControl.Count > Stats.PeakControlQueue)
                    Stats.PeakControlQueue = _outgoingControl.Count;
            }
            _outgoingSignal.Release();
            return true;
        }

        public bool SendState(byte[] packet)
        {
            byte[] copy = ClonePacket(packet);
            bool signal;
            lock (_gate)
            {
                RequireOpen();
                signal = _outgoingState == null;
                if (!signal) Stats.StateCoalesced++;
                _outgoingState = copy;
            }
            if (signal) _outgoingSignal.Release();
            return true;
        }

        public int Pump(int maximumControlMessages = int.MaxValue)
        {
            if (maximumControlMessages < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumControlMessages));
            int delivered = 0;
            while (delivered < maximumControlMessages)
            {
                byte[] packet;
                lock (_gate)
                {
                    if (!_open || _incomingControl.Count == 0) break;
                    packet = _incomingControl.Dequeue();
                }
                Stats.ControlReceived++;
                delivered++;
                ControlReceived?.Invoke(packet);
            }

            byte[] state = null;
            lock (_gate)
            {
                if (_open && _incomingState != null)
                {
                    state = _incomingState;
                    _incomingState = null;
                }
            }
            if (state != null)
            {
                Stats.StateReceived++;
                delivered++;
                StateReceived?.Invoke(state);
            }
            string closedReason = null;
            lock (_gate)
            {
                if (_pendingClosedReason != null)
                {
                    closedReason = _pendingClosedReason;
                    _pendingClosedReason = null;
                }
            }
            if (closedReason != null) Closed?.Invoke(closedReason);
            return delivered;
        }

        public bool DropPendingState()
        {
            lock (_gate)
            {
                if (_incomingState == null) return false;
                _incomingState = null;
                Stats.StateDropped++;
                return true;
            }
        }

        public void Close(string reason = "closed")
        {
            string closeReason = CleanCloseReason(reason);
            if (!FinishClose(closeReason)) return;
            Task.Run(async () =>
            {
                try
                {
                    if (_connection.State == WebSocketState.Open ||
                        _connection.State == WebSocketState.CloseReceived)
                    {
                        await _connection.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            closeReason,
                            CancellationToken.None).ConfigureAwait(false);
                    }
                }
                catch
                {
                    // Local close is already visible to the transport owner.
                }
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Close("disposed");
            _connection.Dispose();
            _outgoingSignal.Dispose();
            _lifetime.Dispose();
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await _outgoingSignal.WaitAsync(cancellationToken).ConfigureAwait(false);
                    while (TryTakeOutgoing(out NetworkTransportLane lane, out byte[] packet))
                    {
                        byte[] frame = WebSocketLaneCodec.Encode(lane, packet);
                        await _connection.SendAsync(
                            new ArraySegment<byte>(frame),
                            WebSocketMessageType.Binary,
                            true,
                            cancellationToken).ConfigureAwait(false);
                        if (lane == NetworkTransportLane.Control) Stats.ControlSent++;
                        else Stats.StateSent++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                FinishClose("send_error:" + error.GetType().Name);
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            byte[] message = new byte[SnapshotWireCodec.MaximumPacketBytes + WebSocketLaneCodec.HeaderBytes];
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int length = 0;
                    WebSocketReceiveResult result;
                    do
                    {
                        if (length >= message.Length)
                            throw new FormatException("WebSocket transport frame exceeds its bound.");
                        result = await _connection.ReceiveAsync(
                            new ArraySegment<byte>(message, length, message.Length - length),
                            cancellationToken).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            FinishClose("remote_closed");
                            return;
                        }
                        if (result.MessageType != WebSocketMessageType.Binary)
                            throw new FormatException("WebSocket transport accepts binary frames only.");
                        length += result.Count;
                    }
                    while (!result.EndOfMessage);

                    byte[] frame = new byte[length];
                    Buffer.BlockCopy(message, 0, frame, 0, length);
                    NetworkTransportLane lane;
                    byte[] payload = WebSocketLaneCodec.Decode(frame, out lane);
                    EnqueueIncoming(lane, payload);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (FormatException)
            {
                Stats.InvalidFrames++;
                FinishClose("invalid_frame");
            }
            catch (Exception error)
            {
                FinishClose("receive_error:" + error.GetType().Name);
            }
        }

        private bool TryTakeOutgoing(out NetworkTransportLane lane, out byte[] packet)
        {
            lock (_gate)
            {
                if (!_open)
                {
                    lane = default(NetworkTransportLane);
                    packet = null;
                    return false;
                }
                if (_outgoingControl.Count > 0)
                {
                    lane = NetworkTransportLane.Control;
                    packet = _outgoingControl.Dequeue();
                    return true;
                }
                if (_outgoingState != null)
                {
                    lane = NetworkTransportLane.State;
                    packet = _outgoingState;
                    _outgoingState = null;
                    return true;
                }
                lane = default(NetworkTransportLane);
                packet = null;
                return false;
            }
        }

        private void EnqueueIncoming(NetworkTransportLane lane, byte[] packet)
        {
            lock (_gate)
            {
                if (!_open) return;
                if (lane == NetworkTransportLane.Control)
                {
                    if (_incomingControl.Count >= _maximumControlQueue)
                    {
                        Stats.Rejected++;
                        return;
                    }
                    _incomingControl.Enqueue(packet);
                    return;
                }
                if (_incomingState != null) Stats.StateCoalesced++;
                _incomingState = packet;
            }
        }

        private bool FinishClose(string reason)
        {
            lock (_gate)
            {
                if (!_open) return false;
                _open = false;
                _incomingControl.Clear();
                _outgoingControl.Clear();
                _incomingState = null;
                _outgoingState = null;
                _pendingClosedReason = reason;
            }
            _lifetime.Cancel();
            return true;
        }

        private void RequireOpen()
        {
            if (!_open || _connection.State != WebSocketState.Open)
                throw new InvalidOperationException("Transport is closed.");
        }

        private static byte[] ClonePacket(byte[] packet)
        {
            if (packet == null ||
                packet.Length == 0 ||
                packet.Length > SnapshotWireCodec.MaximumPacketBytes)
            {
                throw new ArgumentException("Transport packet size is invalid.", nameof(packet));
            }
            byte[] copy = new byte[packet.Length];
            Buffer.BlockCopy(packet, 0, copy, 0, packet.Length);
            return copy;
        }

        private static string CleanCloseReason(string reason)
        {
            string value = string.IsNullOrEmpty(reason) ? "closed" : reason;
            return value.Length <= 120 ? value : value.Substring(0, 120);
        }

        internal static async Task<byte[]> ReceiveSingleBinaryMessageAsync(
            IWebSocketConnection connection,
            int maximumBytes,
            CancellationToken cancellationToken)
        {
            byte[] message = new byte[maximumBytes];
            int length = 0;
            WebSocketReceiveResult result;
            do
            {
                if (length >= message.Length)
                    throw new FormatException("WebSocket message exceeds its size limit.");
                result = await connection.ReceiveAsync(
                    new ArraySegment<byte>(message, length, message.Length - length),
                    cancellationToken).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                    throw new InvalidOperationException("WebSocket closed during handshake.");
                if (result.MessageType != WebSocketMessageType.Binary)
                    throw new FormatException("Dedicated handshake must be binary.");
                length += result.Count;
            }
            while (!result.EndOfMessage);
            if (length == 0) throw new FormatException("WebSocket message is empty.");
            byte[] packet = new byte[length];
            Buffer.BlockCopy(message, 0, packet, 0, length);
            return packet;
        }
    }
}
