using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClaudeOfTanks.Network;

namespace ClaudeOfTanks.WebRTC
{
    internal sealed class SignalingRequestChannel : IDisposable
    {
        private sealed class PendingRequest
        {
            public TaskCompletionSource<SignalingPayload> Completion;
            public CancellationTokenSource Timeout;
        }

        private readonly object _gate = new object();
        private readonly Uri _endpoint;
        private readonly RoomSignalingClientOptions _options;
        private readonly CancellationTokenSource _lifetime =
            new CancellationTokenSource();
        private readonly SemaphoreSlim _sendGate = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, PendingRequest> _pending =
            new Dictionary<string, PendingRequest>(StringComparer.Ordinal);
        private IWebSocketConnection _connection;
        private Task _connectTask;
        private int _requestSequence;
        private bool _disposed;

        public SignalingRequestChannel(
            Uri endpoint,
            RoomSignalingClientOptions options)
        {
            _endpoint = endpoint;
            _options = options;
        }

        public bool IsOpen
        {
            get
            {
                lock (_gate)
                {
                    return _connection != null &&
                        _connection.State == WebSocketState.Open;
                }
            }
        }

        public event Action<SignalingEnvelope> EventReceived;
        public event Action<string> Closed;

        public Task ConnectAsync()
        {
            lock (_gate)
            {
                RequireAlive();
                if (_connection != null &&
                    _connection.State == WebSocketState.Open)
                {
                    return Task.CompletedTask;
                }
                if (_connectTask != null) return _connectTask;
                IWebSocketConnection connection = CreateConnection();
                _connection = connection;
                _connectTask = ConnectCoreAsync(connection);
                return _connectTask;
            }
        }

        public async Task<SignalingPayload> RequestAsync(
            string type,
            SignalingPayload payload,
            int timeoutMs)
        {
            string requestId =
                Interlocked.Increment(ref _requestSequence).ToString();
            PendingRequest pending = new PendingRequest
            {
                Completion = new TaskCompletionSource<SignalingPayload>(),
                Timeout = CancellationTokenSource.CreateLinkedTokenSource(
                    _lifetime.Token)
            };
            pending.Timeout.CancelAfter(timeoutMs);
            pending.Timeout.Token.Register(
                () => TimeoutRequest(requestId, pending));
            lock (_gate) _pending.Add(requestId, pending);
            try
            {
                await SendAsync(type, requestId, payload).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                RemovePending(requestId, pending);
                throw new RoomSignalingException(
                    "signaling_closed",
                    error.Message);
            }
            return await pending.Completion.Task.ConfigureAwait(false);
        }

        public Task SendAsync(string type, SignalingPayload payload)
        {
            return SendAsync(type, null, payload);
        }

        public async Task CloseAsync(string reason)
        {
            IWebSocketConnection connection;
            lock (_gate)
            {
                if (_disposed) return;
                connection = _connection;
                _connection = null;
                _connectTask = null;
            }
            _lifetime.Cancel();
            RejectPending(new RoomSignalingException(reason, reason));
            if (connection != null)
            {
                try
                {
                    if (connection.State == WebSocketState.Open ||
                        connection.State == WebSocketState.CloseReceived)
                    {
                        string cleanReason = string.IsNullOrEmpty(reason)
                            ? "closed"
                            : reason.Substring(0, Math.Min(120, reason.Length));
                        await connection.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            cleanReason,
                            CancellationToken.None).ConfigureAwait(false);
                    }
                }
                catch { }
                connection.Dispose();
            }
        }

        public void Abort(string reason)
        {
            IWebSocketConnection connection;
            lock (_gate)
            {
                connection = _connection;
                _connection = null;
                _connectTask = null;
            }
            connection?.Dispose();
            RejectPending(new RoomSignalingException(reason, reason));
        }

        public void Dispose()
        {
            if (_disposed) return;
            try { CloseAsync("disposed").GetAwaiter().GetResult(); }
            catch { }
            _disposed = true;
            _sendGate.Dispose();
            _lifetime.Dispose();
            EventReceived = null;
            Closed = null;
        }

        private async Task ConnectCoreAsync(IWebSocketConnection connection)
        {
            using (CancellationTokenSource timeout =
                CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token))
            {
                timeout.CancelAfter(_options.ConnectTimeoutMs);
                try
                {
                    await connection.ConnectAsync(_endpoint, timeout.Token)
                        .ConfigureAwait(false);
                    lock (_gate)
                    {
                        if (_connection != connection)
                            throw new OperationCanceledException();
                        _connectTask = null;
                    }
                    _ = Task.Run(() => ReceiveLoopAsync(connection));
                }
                catch (Exception error)
                {
                    lock (_gate)
                    {
                        if (_connection == connection)
                        {
                            _connection = null;
                            _connectTask = null;
                        }
                    }
                    connection.Dispose();
                    string code = error is OperationCanceledException
                        ? "signaling_connect_timeout"
                        : "signaling_connection_failed";
                    throw new RoomSignalingException(code, error.Message);
                }
            }
        }

        private async Task SendAsync(
            string type,
            string requestId,
            SignalingPayload payload)
        {
            string json = RoomSignalingProtocol.Serialize(
                type,
                requestId,
                payload);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await _sendGate.WaitAsync(_lifetime.Token).ConfigureAwait(false);
            try
            {
                IWebSocketConnection connection;
                lock (_gate) connection = _connection;
                if (connection == null || connection.State != WebSocketState.Open)
                    throw new WebSocketException("Signaling socket is not open.");
                await connection.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    _lifetime.Token).ConfigureAwait(false);
            }
            finally
            {
                _sendGate.Release();
            }
        }

        private async Task ReceiveLoopAsync(IWebSocketConnection connection)
        {
            byte[] buffer = new byte[RoomSignalingProtocol.MaximumMessageBytes];
            try
            {
                while (!_lifetime.IsCancellationRequested)
                {
                    int length = 0;
                    WebSocketReceiveResult result;
                    do
                    {
                        if (length >= buffer.Length)
                            throw new FormatException(
                                "Signaling message exceeds its size limit.");
                        result = await connection.ReceiveAsync(
                            new ArraySegment<byte>(
                                buffer,
                                length,
                                buffer.Length - length),
                            _lifetime.Token).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                            throw new WebSocketException("Signaling socket closed.");
                        if (result.MessageType != WebSocketMessageType.Text)
                            throw new FormatException(
                                "Signaling accepts text messages only.");
                        length += result.Count;
                    }
                    while (!result.EndOfMessage);
                    Receive(RoomSignalingProtocol.Deserialize(buffer, length));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                bool active;
                lock (_gate)
                {
                    active = _connection == connection;
                    if (active)
                    {
                        _connection = null;
                        _connectTask = null;
                    }
                }
                if (active)
                {
                    connection.Dispose();
                    string reason = error is FormatException
                        ? "invalid_signaling_payload"
                        : "signaling_closed";
                    RejectPending(new RoomSignalingException(reason, reason));
                    Closed?.Invoke(reason);
                }
            }
        }

        private void Receive(SignalingEnvelope message)
        {
            PendingRequest pending = null;
            lock (_gate)
            {
                if (!string.IsNullOrEmpty(message.requestId) &&
                    _pending.TryGetValue(message.requestId, out pending))
                {
                    _pending.Remove(message.requestId);
                }
            }
            if (pending == null)
            {
                EventReceived?.Invoke(message);
                return;
            }
            pending.Timeout.Cancel();
            pending.Timeout.Dispose();
            if (message.type == "error")
            {
                pending.Completion.TrySetException(
                    new RoomSignalingException(
                        message.payload.code,
                        message.payload.message ?? "Signaling error."));
            }
            else
            {
                pending.Completion.TrySetResult(message.payload);
            }
        }

        private void TimeoutRequest(string requestId, PendingRequest pending)
        {
            if (!RemovePending(requestId, pending)) return;
            pending.Completion.TrySetException(new RoomSignalingException(
                "signaling_request_timeout",
                "Signaling request timed out."));
        }

        private bool RemovePending(string requestId, PendingRequest pending)
        {
            lock (_gate)
            {
                if (!_pending.TryGetValue(requestId, out PendingRequest current) ||
                    current != pending)
                {
                    return false;
                }
                _pending.Remove(requestId);
            }
            pending.Timeout.Dispose();
            return true;
        }

        private void RejectPending(Exception error)
        {
            PendingRequest[] pending;
            lock (_gate)
            {
                pending = new PendingRequest[_pending.Count];
                _pending.Values.CopyTo(pending, 0);
                _pending.Clear();
            }
            for (int i = 0; i < pending.Length; i++)
            {
                pending[i].Timeout.Cancel();
                pending[i].Timeout.Dispose();
                pending[i].Completion.TrySetException(error);
            }
        }

        private IWebSocketConnection CreateConnection()
        {
            return _options.ConnectionFactory != null
                ? _options.ConnectionFactory()
                : new ClientWebSocketConnection(_options.Origin);
        }

        private void RequireAlive()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SignalingRequestChannel));
        }
    }
}
