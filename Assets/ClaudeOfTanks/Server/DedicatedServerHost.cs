using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;

namespace ClaudeOfTanks.Server
{
    public sealed class DedicatedServerHost : IDisposable
    {
        private readonly DedicatedServerOptions _options;
        private readonly ContentCatalog _catalog;
        private readonly string _defaultRatingFile;
        private readonly Func<long> _clock;
        private readonly DedicatedServerTickScheduler _scheduler =
            new DedicatedServerTickScheduler();
        private DedicatedMatchRegistry _registry;
        private DedicatedMatchWebSocketService _matchService;
        private RoomSignalingWebSocketService _signalingService;
        private RankedHttpApi _httpApi;
        private bool _disposed;

        public DedicatedServerHost(
            DedicatedServerOptions options,
            ContentCatalog catalog,
            string defaultRatingFile,
            Func<long> clock = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _defaultRatingFile = string.IsNullOrWhiteSpace(defaultRatingFile)
                ? throw new ArgumentException(
                    "A default rating file is required.",
                    nameof(defaultRatingFile))
                : defaultRatingFile;
            _clock = clock ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public DedicatedMatchRegistry Registry => _registry;
        public bool IsRunning =>
            _matchService != null &&
            _matchService.IsRunning &&
            _signalingService != null &&
            _signalingService.IsRunning;
        public string MatchListenPrefix => _options.ListenPrefix;
        public string SignalingListenPrefix => _options.SignalingListenPrefix;

        public void Start()
        {
            ThrowIfDisposed();
            if (_matchService != null) return;

            _registry = new DedicatedMatchRegistry();
            DedicatedServerMatchFactory factory =
                new DedicatedServerMatchFactory(_catalog);
            RankedRatingStore ratings = new RankedRatingStore(
                _options.RatingFile ?? _defaultRatingFile);
            RankedMatchmaker matchmaker = new RankedMatchmaker(
                ratings,
                _registry,
                factory.Create,
                factory.MapRotation,
                factory.IsVehicleAllowed,
                equipmentAllowed: factory.IsEquipmentAllowed);
            _httpApi = new RankedHttpApi(
                ratings,
                matchmaker,
                _registry,
                _clock);
            _matchService = new DedicatedMatchWebSocketService(
                _registry,
                _options.ListenPrefix,
                _clock,
                _options.AllowedOrigins,
                _httpApi);
            _signalingService = new RoomSignalingWebSocketService(
                _options.SignalingListenPrefix,
                _options.AllowedOrigins);
            try
            {
                _matchService.Start();
                _signalingService.Start();
            }
            catch
            {
                DisposeServices();
                throw;
            }
        }

        public int PumpElapsed(double elapsedSeconds)
        {
            ThrowIfDisposed();
            if (_matchService == null)
                throw new InvalidOperationException(
                    "Dedicated server host is not started.");
            return _matchService.Pump(_scheduler.Consume(elapsedSeconds));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            DisposeServices();
        }

        private void DisposeServices()
        {
            _signalingService?.Dispose();
            _matchService?.Dispose();
            _httpApi?.Dispose();
            _registry?.Dispose();
            _signalingService = null;
            _matchService = null;
            _httpApi = null;
            _registry = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DedicatedServerHost));
        }
    }
}
