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
        public const int MaximumUpgradeBytes = DedicatedHttpTransport.MaximumHeaderBytes;
        private const string WebSocketAcceptSuffix = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private readonly object _gate = new object();
        private readonly DedicatedMatchRegistry _registry;
        private readonly Func<long> _clock;
        private readonly HashSet<string> _allowedOrigins;
        private readonly IDedicatedHttpHandler _httpHandler;
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly Queue<DedicatedWebSocketSession> _pending =
            new Queue<DedicatedWebSocketSession>();
        private readonly List<DedicatedWebSocketSession> _sessions =
            new List<DedicatedWebSocketSession>();
        private readonly Dictionary<string, DedicatedServiceMatch> _matches =
            new Dictionary<string, DedicatedServiceMatch>(StringComparer.Ordinal);
        private int _admissionsInFlight;
        private bool _started;
        private bool _disposed;

        public DedicatedMatchWebSocketService(
            DedicatedMatchRegistry registry,
            string listenPrefix,
            Func<long> clock,
            IEnumerable<string> allowedOrigins = null,
            IDedicatedHttpHandler httpHandler = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _httpHandler = httpHandler;
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
            lock (_gate) _httpHandler?.Pump(_clock());

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
                    DedicatedWebSocketSession session = _sessions[sessionIndex];
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
                bool accepted;
                lock (_gate)
                {
                    accepted = _sessions.Count + _pending.Count + _admissionsInFlight <
                        MaximumConnections;
                    if (accepted) _admissionsInFlight++;
                }
                if (!accepted)
                {
                    client.Dispose();
                    continue;
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
            bool reserved = true;
            bool queued = false;
            try
            {
                client.NoDelay = true;
                NetworkStream stream = client.GetStream();
                using (CancellationTokenSource timeout =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeout.CancelAfter(AuthenticationTimeoutMs);
                    DedicatedTransportRequest request =
                        await DedicatedHttpTransport.ReadAsync(stream, timeout.Token)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(request.Origin) &&
                        !_allowedOrigins.Contains(request.Origin))
                    {
                        await RejectAsync(stream, 403, "Forbidden", timeout.Token)
                            .ConfigureAwait(false);
                        return;
                    }
                    if (!request.IsWebSocket)
                    {
                        DedicatedHttpResponse httpResponse;
                        lock (_gate)
                        {
                            httpResponse = _httpHandler != null
                                ? _httpHandler.Handle(request.ToHttpRequest(
                                    ((IPEndPoint)client.Client.RemoteEndPoint)
                                        .Address.ToString()))
                                : null;
                        }
                        await DedicatedHttpTransport.WriteResponseAsync(
                            stream,
                            httpResponse ?? new DedicatedHttpResponse
                            {
                                Status = 404,
                                Reason = "Not Found",
                                Body = Array.Empty<byte>()
                            },
                            timeout.Token).ConfigureAwait(false);
                        return;
                    }
                    if (request.Method != "GET" || request.Path != "/match")
                    {
                        await RejectAsync(stream, 404, "Not Found", timeout.Token)
                            .ConfigureAwait(false);
                        return;
                    }
                    await AcceptUpgradeAsync(stream, request.WebSocketKey, timeout.Token)
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
                    DedicatedSocketAuthRequest authRequest =
                        DedicatedSocketProtocol.DecodeRequest(packet);
                    lock (_gate)
                    {
                        admission = authRequest.Kind == DedicatedSocketAuthKind.Ticket
                            ? _registry.Admit(
                                authRequest.MatchId,
                                authRequest.PlayerId,
                                authRequest.Token,
                                _clock())
                            : _registry.Reconnect(
                                authRequest.MatchId,
                                authRequest.PlayerId,
                                authRequest.Token);
                        if (!_matches.ContainsKey(admission.MatchId))
                        {
                            DedicatedMatchRecord admittedMatch = _registry.Get(admission.MatchId);
                            if (admittedMatch == null)
                                throw new InvalidOperationException(
                                    "Dedicated match disappeared during admission.");
                            _matches.Add(
                                admission.MatchId,
                                new DedicatedServiceMatch(admittedMatch));
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
                    DedicatedWebSocketSession session = new DedicatedWebSocketSession(
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

        private DedicatedServiceMatch RequireMatch(string matchId)
        {
            lock (_gate)
            {
                DedicatedServiceMatch match;
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
                    DedicatedWebSocketSession next = _pending.Dequeue();
                    for (int i = _sessions.Count - 1; i >= 0; i--)
                    {
                        DedicatedWebSocketSession existing = _sessions[i];
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
                    DedicatedWebSocketSession session = _sessions[i];
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
                foreach (KeyValuePair<string, DedicatedServiceMatch> pair in _matches)
                {
                    DedicatedMatchRecord record = _registry.Get(pair.Key);
                    if (record == null)
                    {
                        if (removed == null) removed = new List<string>();
                        removed.Add(pair.Key);
                        continue;
                    }
                    DedicatedServiceMatch match = pair.Value;
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

    }
}
