using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Server;

namespace ClaudeOfTanks.StandaloneServer
{
    internal static class Program
    {
        private const int LoopDelayMs = 2;

        public static async Task<int> Main(string[] args)
        {
            if (Array.IndexOf(args, "--help") >= 0 ||
                Array.IndexOf(args, "-h") >= 0)
            {
                PrintUsage();
                return 0;
            }

            try
            {
                DedicatedServerOptions options =
                    DedicatedServerOptions.Parse(args);
                string contentPath = ResolveContentCatalog(
                    options.ContentCatalogFile);
                ContentCatalog catalog =
                    ContentCatalog.LoadFromFile(contentPath);
                string ratingFile = Path.Combine(
                    AppContext.BaseDirectory,
                    "data",
                    "ranked-ratings.bin");
                using DedicatedServerHost host = new DedicatedServerHost(
                    options,
                    catalog,
                    ratingFile);
                using CancellationTokenSource lifetime =
                    new CancellationTokenSource();
                int runForMilliseconds = RunForMilliseconds(args);
                if (runForMilliseconds > 0)
                    lifetime.CancelAfter(runForMilliseconds);
                ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    lifetime.Cancel();
                };
                EventHandler processExitHandler = (_, _) =>
                {
                    if (!lifetime.IsCancellationRequested)
                        lifetime.Cancel();
                };
                Console.CancelKeyPress += cancelHandler;
                AppDomain.CurrentDomain.ProcessExit += processExitHandler;

                try
                {
                    host.Start();
                    Console.WriteLine(
                        "Claude of Tanks dedicated server listening at " +
                        host.MatchListenPrefix);
                    Console.WriteLine(
                        "Private-room signaling listening at " +
                        host.SignalingListenPrefix);
                    Console.WriteLine("Content catalog: " + contentPath);

                    Stopwatch clock = Stopwatch.StartNew();
                    double lastTimeSeconds = clock.Elapsed.TotalSeconds;
                    while (!lifetime.IsCancellationRequested)
                    {
                        double nowSeconds = clock.Elapsed.TotalSeconds;
                        host.PumpElapsed(nowSeconds - lastTimeSeconds);
                        lastTimeSeconds = nowSeconds;
                        try
                        {
                            await Task.Delay(
                                LoopDelayMs,
                                lifetime.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    Console.CancelKeyPress -= cancelHandler;
                    AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                }
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(
                    error.GetType().Name + ": " + error.Message);
                return 1;
            }
        }

        private static string ResolveContentCatalog(string configuredPath)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                string exact = Path.GetFullPath(configuredPath);
                if (!File.Exists(exact))
                    throw new FileNotFoundException(
                        "Configured content catalog does not exist.",
                        exact);
                return exact;
            }

            string published = Path.Combine(
                AppContext.BaseDirectory,
                "content-catalog.json");
            if (File.Exists(published)) return published;
            string repository = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "Assets",
                "ClaudeOfTanks",
                "Resources",
                "Content",
                "content-catalog.json"));
            if (File.Exists(repository)) return repository;
            throw new FileNotFoundException(
                "Content catalog was not found. Set COT_CONTENT_CATALOG " +
                "or pass --cot-content=<path>.");
        }

        private static void PrintUsage()
        {
            Console.WriteLine(
                "ClaudeOfTanks.Server [options]\n" +
                "  --cot-bind=<IPv4>          Match/signaling bind address\n" +
                "  --cot-port=<port>          Match and ranked HTTP port\n" +
                "  --cot-signal-port=<port>   Private-room signaling port\n" +
                "  --cot-origins=<origins>    Comma-separated allowed origins\n" +
                "  --cot-rating-file=<path>   Persistent rating database\n" +
                "  --cot-content=<path>       Generated content catalog\n" +
                "  --cot-run-for-ms=<ms>      Bounded diagnostic lifetime");
        }

        private static int RunForMilliseconds(string[] args)
        {
            const string prefix = "--cot-run-for-ms=";
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith(prefix, StringComparison.Ordinal))
                    continue;
                string raw = args[i].Substring(prefix.Length);
                if (!int.TryParse(raw, out int value) ||
                    value < 1 ||
                    value > 300000)
                {
                    throw new ArgumentException(
                        "Diagnostic lifetime must be between 1 and 300000 ms.");
                }
                return value;
            }
            return 0;
        }
    }
}
