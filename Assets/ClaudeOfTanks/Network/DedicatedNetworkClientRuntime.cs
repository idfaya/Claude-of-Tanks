using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeOfTanks.Network
{
    public sealed class DedicatedNetworkClientRuntime :
        INetworkBattleClientRuntime
    {
        private const int MaximumReconnectDelayMs = 5000;
        private readonly Uri _endpoint;
        private readonly string _matchId;
        private readonly string _playerId;
        private readonly string _entityId;
        private readonly LocalTankPredictor _predictor;
        private readonly SnapshotFrameReceiver _receiver =
            new SnapshotFrameReceiver();
        private readonly SnapshotBuffer _buffer = new SnapshotBuffer();
        private readonly CancellationTokenSource _lifetime =
            new CancellationTokenSource();
        private DedicatedSocketConnection _connection;
        private NetworkClientPump _client;
        private Task<DedicatedSocketConnection> _reconnect;
        private string _sessionToken;
        private long _nextReconnectAtMs;
        private int _reconnectAttempts;
        private bool _disposed;

        public DedicatedNetworkClientRuntime(
            Uri endpoint,
            DedicatedSocketConnection connection,
            LocalTankPredictor predictor = null)
        {
            _endpoint = endpoint ??
                throw new ArgumentNullException(nameof(endpoint));
            _connection = connection ??
                throw new ArgumentNullException(nameof(connection));
            _matchId = connection.Admission.MatchId;
            _playerId = connection.Admission.PlayerId;
            _entityId = connection.Admission.EntityId;
            _sessionToken = connection.Admission.SessionToken;
            _predictor = predictor;
            Attach(connection);
        }

        public NetworkWorldSnapshot LatestSnapshot => _receiver.Latest;
        public SnapshotBuffer Buffer => _buffer;
        public bool IsConnected =>
            _connection != null && _connection.Transport.IsOpen;
        public bool IsReconnecting => _reconnect != null;
        public int ConnectionGeneration { get; private set; }
        public string LastError { get; private set; }

        public bool SendInput(NetworkInputCommand command)
        {
            ThrowIfDisposed();
            if (!IsConnected)
            {
                StartReconnectIfDue();
                return false;
            }
            try
            {
                return _client.SendInput(command);
            }
            catch (InvalidOperationException)
            {
                StartReconnectIfDue();
                return false;
            }
        }

        public int Pump()
        {
            ThrowIfDisposed();
            CompleteReconnect();
            int delivered = _client?.Pump() ?? 0;
            if (!IsConnected) StartReconnectIfDue();
            return delivered;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _lifetime.Cancel();
            if (_reconnect != null &&
                _reconnect.Status == TaskStatus.RanToCompletion)
            {
                _reconnect.Result.Dispose();
            }
            _reconnect = null;
            _client?.Dispose();
            _client = null;
            _connection?.Dispose();
            _connection = null;
            _lifetime.Dispose();
        }

        private void Attach(DedicatedSocketConnection connection)
        {
            _client?.Dispose();
            if (_connection != null && !ReferenceEquals(_connection, connection))
                _connection.Dispose();
            _connection = connection;
            _sessionToken = connection.Admission.SessionToken;
            ConnectionGeneration =
                connection.Admission.ConnectionGeneration;
            _client = new NetworkClientPump(
                _playerId,
                _entityId,
                connection.Transport,
                _predictor,
                _receiver,
                _buffer);
            _reconnectAttempts = 0;
            _nextReconnectAtMs = 0;
            LastError = null;
        }

        private void StartReconnectIfDue()
        {
            if (_disposed || _reconnect != null ||
                UnixTimeMs() < _nextReconnectAtMs)
            {
                return;
            }
            string token = _sessionToken;
            _reconnect = WebSocketNetworkEndpoint.ConnectDedicatedAsync(
                _endpoint,
                new DedicatedSocketAuthRequest
                {
                    Kind = DedicatedSocketAuthKind.Reconnect,
                    MatchId = _matchId,
                    PlayerId = _playerId,
                    Token = token
                },
                cancellationToken: _lifetime.Token);
        }

        private void CompleteReconnect()
        {
            if (_reconnect == null || !_reconnect.IsCompleted) return;
            Task<DedicatedSocketConnection> completed = _reconnect;
            _reconnect = null;
            try
            {
                DedicatedSocketConnection connection =
                    completed.GetAwaiter().GetResult();
                if (_disposed)
                {
                    connection.Dispose();
                    return;
                }
                Attach(connection);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                LastError = error.Message;
                _reconnectAttempts++;
                int delay = Math.Min(
                    MaximumReconnectDelayMs,
                    250 << Math.Min(_reconnectAttempts - 1, 4));
                _nextReconnectAtMs = UnixTimeMs() + delay;
            }
        }

        private static long UnixTimeMs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(DedicatedNetworkClientRuntime));
        }
    }
}
