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

namespace ClaudeOfTanks.Network
{
    public sealed class DedicatedMatchWebSocketService : IDisposable
    {
        public const int MaximumConnections = 128;
        public const int AuthenticationTimeoutMs = 5000;
        public const int MaximumUpgradeBytes = 8192;
        private const string WebSocketAcceptSuffix = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private readonly object _gate = new object();
        private readonly DedicatedMatchRegistry _registry;
        private readonly Func<long> _clock;
        private readonly HashSet<string> _allowedOrigins;
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly Queue<Session> _pending = new Queue<Session>();
        private readonly List<Session> _sessions = new List<Session>();
        private readonly Dictionary<string, ServiceMatch> _matches =
            new Dictionary<string, ServiceMatch>(StringComparer.Ordinal);
        private int _admissionsInFlight;
        private bool _started;
        private bool _disposed;

        public DedicatedMatchWebSocketService(
            DedicatedMatchRegistry registry,
            string listenPrefix,
            Func<long> clock,
            IEnumerable<string> allowedOrigins = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Uri uri;
            if (!Uri.TryCreate(listenPrefix, UriKind.Absolute, out uri) ||
                uri.Scheme != "http" ||
                uri.AbsolutePath != "/" ||
                uri.Port < 1)
            {
                throw new ArgumentException(
                    "Dedicated listen prefix must be an HTTP origin ending in '/'.",
                    nameof(listenPrefix));
            }
            IPAddress address;
            if (uri.Host == "localhost") address = IPAddress.Loopback;
            else if (!IPAddress.TryParse(uri.Host, out address))
                throw new ArgumentException(
                    "Dedicated listen prefix must use a literal IP address or localhost.",
                    nameof(listenPrefix));
            _listener = new TcpListener(address, uri.Port);
            _allowedOrigins = new HashSet<string>(StringComparer.Ordinal);
            if (allowedOrigins != null)
            {
                foreach (string origin in allowedOrigins)
                {
                    if (string.IsNullOrEmpty(origin))
                        throw new ArgumentException("Allowed origins cannot contain empty values.");
                    _allowedOrigins.Add(origin);
                }
            }
        }

        public bool IsRunning => _started && !_disposed;
        public int ActiveConnectionCount
        {
            get
            {
                lock (_gate) return _sessions.Count + _pending.Count + _admissionsInFlight;
            }
        }

        public void Start()
        {
            ThrowIfDisposed();
            if (_started) return;
            _listener.Start();
            _started = true;
            _ = Task.Run(() => AcceptLoopAsync(_lifetime.Token));
        }

        public int Pump(int requestedTicks)
        {
            ThrowIfDisposed();
            if (!_started) throw new InvalidOperationException("Dedicated service is not started.");
            int ticks = Math.Max(
                0,
                Math.Min(requestedTicks, AuthoritativeMatchHost.MaximumCatchUpTicks));
            DrainPending();
            RemoveClosed();

            for (int i = 0; i < _sessions.Count; i++)
                _sessions[i].Pump.PumpIncoming();

            List<DedicatedMatchRecord> matches = ActiveMatches();
            for (int step = 0; step < ticks; step++)
            {
                for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                {
                    DedicatedMatchRecord match = matches[matchIndex];
                    if (!match.FinishedAtMs.HasValue) match.Host.AdvanceTicks(1);
                }
                for (int sessionIndex = 0; sessionIndex < _sessions.Count; sessionIndex++)
                {
                    Session session = _sessions[sessionIndex];
                    if (!session.Closed) session.Pump.PublishSnapshotIfDue();
                }
            }
            RemoveClosed();
            return ticks;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _lifetime.Cancel();
            _listener.Stop();
            lock (_gate)
            {
                while (_pending.Count > 0) _pending.Dequeue().Dispose(_registry);
                for (int i = 0; i < _sessions.Count; i++) _sessions[i].Dispose(_registry);
                _sessions.Clear();
                _matches.Clear();
            }
            _lifetime.Dispose();
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                _ = Task.Run(() => AcceptConnectionAsync(client, cancellationToken));
            }
        }

        private async Task AcceptConnectionAsync(
            TcpClient client,
            CancellationToken cancellationToken)
        {
            AcceptedWebSocketConnection connection = null;
            WebSocketNetworkEndpoint transport = null;
            DedicatedMatchAdmission admission = null;
            bool reserved = false;
            bool queued = false;
            try
            {
                client.NoDelay = true;
                NetworkStream stream = client.GetStream();
                using (CancellationTokenSource timeout =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeout.CancelAfter(AuthenticationTimeoutMs);
                    UpgradeRequest upgrade = await ReadUpgradeAsync(stream, timeout.Token)
                        .ConfigureAwait(false);
                    if (upgrade.Path != "/match")
                    {
                        await RejectAsync(stream, 404, "Not Found", timeout.Token)
                            .ConfigureAwait(false);
                        return;
                    }
                    if (!string.IsNullOrEmpty(upgrade.Origin) &&
                        !_allowedOrigins.Contains(upgrade.Origin))
                    {
                        await RejectAsync(stream, 403, "Forbidden", timeout.Token)
                            .ConfigureAwait(false);
                        return;
                    }
                    lock (_gate)
                    {
                        if (_sessions.Count + _pending.Count + _admissionsInFlight >=
                            MaximumConnections)
                        {
                            throw new InvalidOperationException("Dedicated service is at capacity.");
                        }
                        _admissionsInFlight++;
                        reserved = true;
                    }

                    await AcceptUpgradeAsync(stream, upgrade.Key, timeout.Token)
                        .ConfigureAwait(false);
                    WebSocket socket = WebSocket.CreateFromStream(
                        stream,
                        true,
                        null,
                        TimeSpan.FromSeconds(20));
                    connection = new AcceptedWebSocketConnection(socket, client);
                    client = null;

                    byte[] packet =
                        await WebSocketNetworkEndpoint.ReceiveSingleBinaryMessageAsync(
                            connection,
                            DedicatedSocketProtocol.MaximumHandshakeBytes,
                            timeout.Token).ConfigureAwait(false);
                    DedicatedSocketAuthRequest request =
                        DedicatedSocketProtocol.DecodeRequest(packet);
                    lock (_gate)
                    {
                        admission = request.Kind == DedicatedSocketAuthKind.Ticket
                            ? _registry.Admit(
                                request.MatchId,
                                request.PlayerId,
                                request.Token,
                                _clock())
                            : _registry.Reconnect(
                                request.MatchId,
                                request.PlayerId,
                                request.Token);
                        if (!_matches.ContainsKey(admission.MatchId))
                        {
                            DedicatedMatchRecord admittedMatch = _registry.Get(admission.MatchId);
                            if (admittedMatch == null)
                                throw new InvalidOperationException(
                                    "Dedicated match disappeared during admission.");
                            _matches.Add(admission.MatchId, new ServiceMatch(admittedMatch));
                        }
                    }
                    DedicatedSocketAuthResponse response = new DedicatedSocketAuthResponse
                    {
                        MatchId = admission.MatchId,
                        PlayerId = admission.PlayerId,
                        EntityId = admission.EntityId,
                        SessionToken = admission.SessionToken,
                        ConnectionGeneration = admission.ConnectionGeneration
                    };
                    byte[] responsePacket = DedicatedSocketProtocol.EncodeResponse(response);
                    await connection.SendAsync(
                        new ArraySegment<byte>(responsePacket),
                        WebSocketMessageType.Binary,
                        true,
                        timeout.Token).ConfigureAwait(false);

                    transport = WebSocketNetworkEndpoint.Attach(connection);
                    connection = null;
                    Session session = new Session(
                        admission,
                        transport,
                        new AuthoritativeHostPump(
                            RequireMatch(admission.MatchId).Record.Host,
                            admission.PlayerId,
                            transport));
                    transport.Closed += reason => session.Closed = true;
                    lock (_gate)
                    {
                        _pending.Enqueue(session);
                        queued = true;
                    }
                }
            }
            catch
            {
                if (connection != null)
                {
                    try
                    {
                        await connection.CloseAsync(
                            WebSocketCloseStatus.PolicyViolation,
                            "authentication_failed",
                            CancellationToken.None).ConfigureAwait(false);
                    }
                    catch
                    {
                    }
                    connection.Dispose();
                }
            }
            finally
            {
                if (!queued && transport != null) transport.Dispose();
                if (!queued && admission != null)
                {
                    lock (_gate)
                    {
                        _registry.Disconnect(
                            admission.MatchId,
                            admission.PlayerId,
                            admission.ConnectionGeneration);
                    }
                }
                if (reserved)
                {
                    lock (_gate) _admissionsInFlight--;
                }
                if (client != null) client.Dispose();
            }
        }

        private ServiceMatch RequireMatch(string matchId)
        {
            lock (_gate)
            {
                ServiceMatch match;
                if (!_matches.TryGetValue(matchId, out match))
                    throw new InvalidOperationException("Dedicated match disappeared during admission.");
                return match;
            }
        }

        private void DrainPending()
        {
            lock (_gate)
            {
                while (_pending.Count > 0)
                {
                    Session next = _pending.Dequeue();
                    for (int i = _sessions.Count - 1; i >= 0; i--)
                    {
                        Session existing = _sessions[i];
                        if (existing.Admission.MatchId == next.Admission.MatchId &&
                            existing.Admission.PlayerId == next.Admission.PlayerId)
                        {
                            _sessions.RemoveAt(i);
                            existing.Dispose(_registry);
                        }
                    }
                    _sessions.Add(next);
                }
            }
        }

        private void RemoveClosed()
        {
            lock (_gate)
            {
                for (int i = _sessions.Count - 1; i >= 0; i--)
                {
                    Session session = _sessions[i];
                    session.Transport.Pump(0);
                    if (!session.Closed) continue;
                    _sessions.RemoveAt(i);
                    session.Dispose(_registry);
                }
            }
        }

        private List<DedicatedMatchRecord> ActiveMatches()
        {
            List<DedicatedMatchRecord> matches = new List<DedicatedMatchRecord>();
            lock (_gate)
            {
                List<string> removed = null;
                foreach (KeyValuePair<string, ServiceMatch> pair in _matches)
                {
                    DedicatedMatchRecord record = _registry.Get(pair.Key);
                    if (record == null)
                    {
                        if (removed == null) removed = new List<string>();
                        removed.Add(pair.Key);
                        continue;
                    }
                    ServiceMatch match = pair.Value;
                    if (!match.Started &&
                        record.ConnectedPlayerCount == record.PlayerCount)
                    {
                        match.Started = true;
                    }
                    if (match.Started) matches.Add(record);
                }
                if (removed != null)
                    for (int i = 0; i < removed.Count; i++) _matches.Remove(removed[i]);
            }
            return matches;
        }

        private static async Task<UpgradeRequest> ReadUpgradeAsync(
            Stream stream,
            CancellationToken cancellationToken)
        {
            byte[] bytes = new byte[MaximumUpgradeBytes];
            int length = 0;
            while (length < bytes.Length)
            {
                int read = await stream.ReadAsync(
                    bytes,
                    length,
                    bytes.Length - length,
                    cancellationToken).ConfigureAwait(false);
                if (read <= 0) throw new IOException("WebSocket upgrade ended early.");
                length += read;
                int end = HeaderEnd(bytes, length);
                if (end < 0) continue;
                string text = Encoding.ASCII.GetString(bytes, 0, end);
                string[] lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                string[] request = lines[0].Split(' ');
                if (request.Length != 3 || request[0] != "GET" || request[2] != "HTTP/1.1")
                    throw new FormatException("WebSocket upgrade request line is invalid.");
                Dictionary<string, string> headers =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Length == 0) continue;
                    int colon = lines[i].IndexOf(':');
                    if (colon <= 0) throw new FormatException("WebSocket upgrade header is invalid.");
                    string name = lines[i].Substring(0, colon).Trim();
                    string value = lines[i].Substring(colon + 1).Trim();
                    if (headers.ContainsKey(name)) headers[name] += "," + value;
                    else headers.Add(name, value);
                }
                string key;
                string upgrade;
                string connection;
                string version;
                headers.TryGetValue("Sec-WebSocket-Key", out key);
                headers.TryGetValue("Upgrade", out upgrade);
                headers.TryGetValue("Connection", out connection);
                headers.TryGetValue("Sec-WebSocket-Version", out version);
                if (!string.Equals(upgrade, "websocket", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrEmpty(connection) ||
                    connection.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) < 0 ||
                    version != "13" ||
                    !ValidWebSocketKey(key))
                {
                    throw new FormatException("WebSocket upgrade headers are invalid.");
                }
                string origin;
                headers.TryGetValue("Origin", out origin);
                return new UpgradeRequest(request[1], key, origin);
            }
            throw new FormatException("WebSocket upgrade exceeds its size limit.");
        }

        private static int HeaderEnd(byte[] bytes, int length)
        {
            for (int i = 3; i < length; i++)
                if (bytes[i - 3] == 13 && bytes[i - 2] == 10 &&
                    bytes[i - 1] == 13 && bytes[i] == 10)
                    return i + 1;
            return -1;
        }

        private static bool ValidWebSocketKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            try { return Convert.FromBase64String(key).Length == 16; }
            catch (FormatException) { return false; }
        }

        private static async Task AcceptUpgradeAsync(
            Stream stream,
            string key,
            CancellationToken cancellationToken)
        {
            byte[] hash;
            using (SHA1 sha = SHA1.Create())
                hash = sha.ComputeHash(Encoding.ASCII.GetBytes(key + WebSocketAcceptSuffix));
            string response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Accept: " + Convert.ToBase64String(hash) + "\r\n\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken)
                .ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        private static async Task RejectAsync(
            Stream stream,
            int status,
            string reason,
            CancellationToken cancellationToken)
        {
            string response =
                "HTTP/1.1 " + status + " " + reason + "\r\n" +
                "Connection: close\r\nContent-Length: 0\r\n\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken)
                .ConfigureAwait(false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DedicatedMatchWebSocketService));
        }

        private sealed class UpgradeRequest
        {
            public UpgradeRequest(string path, string key, string origin)
            {
                Path = path;
                Key = key;
                Origin = origin;
            }

            public string Path { get; }
            public string Key { get; }
            public string Origin { get; }
        }

        private sealed class Session
        {
            public Session(
                DedicatedMatchAdmission admission,
                WebSocketNetworkEndpoint transport,
                AuthoritativeHostPump pump)
            {
                Admission = admission;
                Transport = transport;
                Pump = pump;
            }

            public DedicatedMatchAdmission Admission { get; }
            public WebSocketNetworkEndpoint Transport { get; }
            public AuthoritativeHostPump Pump { get; }
            public bool Closed { get; set; }

            public void Dispose(DedicatedMatchRegistry registry)
            {
                try
                {
                    registry.Disconnect(
                        Admission.MatchId,
                        Admission.PlayerId,
                        Admission.ConnectionGeneration);
                }
                catch (KeyNotFoundException)
                {
                }
                Pump.Dispose();
                Transport.Dispose();
            }
        }

        private sealed class ServiceMatch
        {
            public ServiceMatch(DedicatedMatchRecord record)
            {
                Record = record;
            }

            public DedicatedMatchRecord Record { get; }
            public bool Started { get; set; }
        }

        private sealed class AcceptedWebSocketConnection : IWebSocketConnection
        {
            private readonly WebSocket _socket;
            private readonly TcpClient _client;

            public AcceptedWebSocketConnection(WebSocket socket, TcpClient client)
            {
                _socket = socket ?? throw new ArgumentNullException(nameof(socket));
                _client = client ?? throw new ArgumentNullException(nameof(client));
            }

            public WebSocketState State => _socket.State;

            public Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
            {
                throw new NotSupportedException("Accepted WebSockets are already connected.");
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
                _client.Dispose();
            }
        }
    }
}
