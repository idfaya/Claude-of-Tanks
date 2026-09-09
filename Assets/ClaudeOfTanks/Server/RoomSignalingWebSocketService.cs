using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClaudeOfTanks.Network;
using UnityEngine;

namespace ClaudeOfTanks.Server
{
    public sealed class RoomSignalingWebSocketService : IDisposable
    {
        private const string WebSocketAcceptSuffix =
            "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private readonly object _gate = new object();
        private readonly TcpListener _listener;
        private readonly HashSet<string> _allowedOrigins;
        private readonly RoomSignalingStore _store =
            new RoomSignalingStore();
        private readonly HashSet<RoomSignalingConnection> _connections =
            new HashSet<RoomSignalingConnection>();
        private readonly CancellationTokenSource _lifetime =
            new CancellationTokenSource();
        private bool _started;
        private bool _disposed;

        public RoomSignalingWebSocketService(
            string listenPrefix,
            IEnumerable<string> allowedOrigins = null)
        {
            if (!Uri.TryCreate(
                    listenPrefix,
                    UriKind.Absolute,
                    out Uri uri) ||
                uri.Scheme != "http" ||
                uri.AbsolutePath != "/" ||
                uri.Port < 1)
            {
                throw new ArgumentException(
                    "Signaling listen prefix must be an HTTP origin ending in '/'.",
                    nameof(listenPrefix));
            }
            IPAddress address;
            if (uri.Host == "localhost")
                address = IPAddress.Loopback;
            else if (!IPAddress.TryParse(uri.Host, out address))
                throw new ArgumentException(
                    "Signaling host must be a literal IP address or localhost.",
                    nameof(listenPrefix));
            _listener = new TcpListener(address, uri.Port);
            _allowedOrigins =
                new HashSet<string>(StringComparer.Ordinal);
            if (allowedOrigins == null) return;
            foreach (string origin in allowedOrigins)
            {
                if (string.IsNullOrWhiteSpace(origin))
                    throw new ArgumentException(
                        "Allowed origins cannot contain empty values.",
                        nameof(allowedOrigins));
                _allowedOrigins.Add(origin.Trim());
            }
        }

        public bool IsRunning => _started && !_disposed;
        public string LastError { get; private set; }

        public void Start()
        {
            ThrowIfDisposed();
            if (_started) return;
            _listener.Start();
            _started = true;
            _ = Task.Run(() => AcceptLoopAsync(_lifetime.Token));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _lifetime.Cancel();
            _listener.Stop();
            RoomSignalingConnection[] connections;
            lock (_gate)
            {
                connections =
                    new RoomSignalingConnection[_connections.Count];
                _connections.CopyTo(connections);
                _connections.Clear();
            }
            for (int i = 0; i < connections.Length; i++)
                connections[i].Dispose();
            _lifetime.Dispose();
        }

        private async Task AcceptLoopAsync(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync()
                        .ConfigureAwait(false);
                }
                catch (SocketException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (ObjectDisposedException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                _ = Task.Run(
                    () => AcceptConnectionAsync(
                        client,
                        cancellationToken));
            }
        }

        private async Task AcceptConnectionAsync(
            TcpClient client,
            CancellationToken cancellationToken)
        {
            RoomSignalingConnection connection = null;
            try
            {
                client.NoDelay = true;
                NetworkStream stream = client.GetStream();
                using (CancellationTokenSource timeout =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken))
                {
                    timeout.CancelAfter(5000);
                    DedicatedTransportRequest request =
                        await DedicatedHttpTransport.ReadAsync(
                            stream,
                            timeout.Token).ConfigureAwait(false);
                    if (!OriginAllowed(request.Origin))
                    {
                        await RejectAsync(
                            stream,
                            403,
                            "Forbidden",
                            timeout.Token).ConfigureAwait(false);
                        return;
                    }
                    if (!request.IsWebSocket ||
                        request.Method != "GET" ||
                        request.Path != "/signal")
                    {
                        await RejectAsync(
                            stream,
                            404,
                            "Not Found",
                            timeout.Token).ConfigureAwait(false);
                        return;
                    }
                    await AcceptUpgradeAsync(
                        stream,
                        request.WebSocketKey,
                        timeout.Token).ConfigureAwait(false);
                    WebSocket socket = WebSocket.CreateFromStream(
                        stream,
                        true,
                        null,
                        TimeSpan.FromSeconds(20));
                    connection =
                        new RoomSignalingConnection(socket, client);
                    client = null;
                    lock (_gate) _connections.Add(connection);
                }
                await ReceiveLoopAsync(
                    connection,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception error)
                when (!(error is OperationCanceledException))
            {
                LastError = error.GetType().Name + ": " + error.Message;
            }
            finally
            {
                if (connection != null)
                {
                    _store.Detach(connection);
                    lock (_gate) _connections.Remove(connection);
                    connection.Dispose();
                }
                client?.Dispose();
            }
        }

        private async Task ReceiveLoopAsync(
            RoomSignalingConnection connection,
            CancellationToken cancellationToken)
        {
            byte[] buffer =
                new byte[RoomSignalingProtocolLimits.MaximumMessageBytes];
            while (!cancellationToken.IsCancellationRequested &&
                connection.Socket.State == WebSocketState.Open)
            {
                int length = 0;
                WebSocketReceiveResult result;
                do
                {
                    if (length >= buffer.Length)
                        throw new FormatException(
                            "Signaling message exceeds its size limit.");
                    result = await connection.Socket.ReceiveAsync(
                        new ArraySegment<byte>(
                            buffer,
                            length,
                            buffer.Length - length),
                        cancellationToken).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                        return;
                    if (result.MessageType != WebSocketMessageType.Text)
                        throw new FormatException(
                            "Signaling accepts text messages only.");
                    length += result.Count;
                }
                while (!result.EndOfMessage);
                RoomSignalingEnvelope request =
                    Deserialize(buffer, length);
                await HandleAsync(
                    connection,
                    request,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task HandleAsync(
            RoomSignalingConnection connection,
            RoomSignalingEnvelope request,
            CancellationToken cancellationToken)
        {
            try
            {
                RoomSignalingWirePayload payload =
                    request.payload ?? new RoomSignalingWirePayload();
                RoomSignalingResult result;
                string responseType = null;
                switch (request.type)
                {
                    case "room_create":
                        result = _store.Create(connection, payload);
                        responseType = "room_created";
                        break;
                    case "room_join":
                        result = _store.Join(connection, payload);
                        responseType = "room_joined";
                        break;
                    case "room_signal":
                        ValidateSignal(payload.signal);
                        result = _store.Relay(connection, payload);
                        break;
                    case "room_poll":
                        result = _store.Poll(connection, payload.roomCode);
                        responseType = "room_polled";
                        result.Response =
                            new RoomSignalingWirePayload
                            {
                                roomCode = payload.roomCode
                            };
                        break;
                    case "room_leave":
                        result = _store.Leave(
                            connection,
                            "client_leave");
                        break;
                    default:
                        throw new RoomSignalingStoreException(
                            "unknown_message",
                            "Unknown signaling message.");
                }
                if (responseType != null)
                {
                    await connection.SendAsync(
                        new RoomSignalingEnvelope
                        {
                            type = responseType,
                            requestId = request.requestId,
                            payload = result.Response
                        },
                        cancellationToken).ConfigureAwait(false);
                }
                await DeliverAsync(
                    result.Deliveries,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (RoomSignalingStoreException error)
            {
                await SendErrorAsync(
                    connection,
                    request.requestId,
                    error.Code,
                    error.Message,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                await SendErrorAsync(
                    connection,
                    request.requestId,
                    "invalid_request",
                    error.Message,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task DeliverAsync(
            List<RoomSignalingDelivery> deliveries,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < deliveries.Count; i++)
            {
                RoomSignalingConnection connection =
                    deliveries[i].Peer.Connection;
                if (connection == null ||
                    connection.Socket.State != WebSocketState.Open)
                    continue;
                await connection.SendAsync(
                    deliveries[i].Message,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private bool OriginAllowed(string origin)
        {
            return _allowedOrigins.Count == 0 ||
                (!string.IsNullOrEmpty(origin) &&
                 _allowedOrigins.Contains(origin));
        }

        private static void ValidateSignal(
            RoomSignalingWireSignal signal)
        {
            if (signal == null)
                throw new RoomSignalingStoreException(
                    "invalid_signal",
                    "RTC signal is missing.");
            if (signal.kind == "restart") return;
            if (signal.kind == "description" &&
                signal.description != null &&
                (signal.description.type == "offer" ||
                 signal.description.type == "answer") &&
                !string.IsNullOrEmpty(signal.description.sdp) &&
                signal.description.sdp.Length <= 96000)
                return;
            if (signal.kind == "ice" &&
                signal.candidate != null &&
                !string.IsNullOrEmpty(signal.candidate.candidate) &&
                signal.candidate.candidate.Length <= 8000)
                return;
            throw new RoomSignalingStoreException(
                "invalid_signal",
                "RTC signal is invalid.");
        }

        private static Task SendErrorAsync(
            RoomSignalingConnection connection,
            string requestId,
            string code,
            string message,
            CancellationToken cancellationToken)
        {
            return connection.SendAsync(
                new RoomSignalingEnvelope
                {
                    type = "error",
                    requestId = requestId,
                    payload = new RoomSignalingWirePayload
                    {
                        code = code,
                        message = message
                    }
                },
                cancellationToken);
        }

        private static RoomSignalingEnvelope Deserialize(
            byte[] bytes,
            int length)
        {
            if (length < 1 ||
                length > RoomSignalingProtocolLimits.MaximumMessageBytes)
                throw new FormatException("Signaling message size is invalid.");
            string json =
                new UTF8Encoding(false, true).GetString(bytes, 0, length);
            RoomSignalingEnvelope envelope =
                JsonUtility.FromJson<RoomSignalingEnvelope>(json);
            if (envelope == null || string.IsNullOrEmpty(envelope.type))
                throw new FormatException("Signaling message type is missing.");
            return envelope;
        }

        private static async Task AcceptUpgradeAsync(
            Stream stream,
            string key,
            CancellationToken cancellationToken)
        {
            byte[] hash;
            using (SHA1 sha = SHA1.Create())
                hash = sha.ComputeHash(
                    Encoding.ASCII.GetBytes(
                        key + WebSocketAcceptSuffix));
            string response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Accept: " +
                Convert.ToBase64String(hash) +
                "\r\n\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(
                bytes,
                0,
                bytes.Length,
                cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        private static async Task RejectAsync(
            Stream stream,
            int status,
            string reason,
            CancellationToken cancellationToken)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(
                "HTTP/1.1 " + status + " " + reason +
                "\r\nConnection: close\r\nContent-Length: 0\r\n\r\n");
            await stream.WriteAsync(
                bytes,
                0,
                bytes.Length,
                cancellationToken).ConfigureAwait(false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(RoomSignalingWebSocketService));
        }
    }

}
