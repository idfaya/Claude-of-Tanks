using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
#if !COT_STANDALONE_SERVER
using ClaudeOfTanks.Runtime;
using UnityEngine;
#endif

namespace ClaudeOfTanks.Server
{
    public sealed class DedicatedServerOptions
    {
        public string BindAddress { get; private set; } = "127.0.0.1";
        public int Port { get; private set; } = 18791;
        public int SignalingPort { get; private set; } = 18792;
        public string[] AllowedOrigins { get; private set; } = Array.Empty<string>();
        public string RatingFile { get; private set; }
        public string ContentCatalogFile { get; private set; }

        public string ListenPrefix =>
            "http://" + BindAddress + ":" + Port + "/";
        public string SignalingListenPrefix =>
            "http://" + BindAddress + ":" + SignalingPort + "/";

        public static DedicatedServerOptions Parse(
            string[] arguments,
            Func<string, string> environment = null)
        {
            string[] args = arguments ?? Array.Empty<string>();
            Func<string, string> env = environment ?? Environment.GetEnvironmentVariable;
            DedicatedServerOptions result = new DedicatedServerOptions();
            string bind = Value(args, "--cot-bind") ?? env("COT_SERVER_BIND");
            string port = Value(args, "--cot-port") ?? env("COT_SERVER_PORT");
            string signalingPort =
                Value(args, "--cot-signal-port") ?? env("COT_SIGNAL_PORT");
            string origins = Value(args, "--cot-origins") ?? env("COT_ALLOWED_ORIGINS");
            string ratingFile =
                Value(args, "--cot-rating-file") ?? env("COT_RATING_FILE");
            string contentCatalogFile =
                Value(args, "--cot-content") ?? env("COT_CONTENT_CATALOG");
            if (!string.IsNullOrEmpty(bind)) result.BindAddress = bind;
            IPAddress parsedAddress;
            if (result.BindAddress != "localhost" &&
                (!IPAddress.TryParse(result.BindAddress, out parsedAddress) ||
                 parsedAddress.AddressFamily != AddressFamily.InterNetwork))
            {
                throw new ArgumentException("Dedicated server bind address is invalid.");
            }
            if (!string.IsNullOrEmpty(port))
            {
                int parsed;
                if (!int.TryParse(port, out parsed) || parsed < 1 || parsed > 65535)
                    throw new ArgumentException("Dedicated server port is invalid.");
                result.Port = parsed;
            }
            if (!string.IsNullOrEmpty(signalingPort))
            {
                int parsed;
                if (!int.TryParse(signalingPort, out parsed) ||
                    parsed < 1 ||
                    parsed > 65535)
                {
                    throw new ArgumentException(
                        "Signaling server port is invalid.");
                }
                result.SignalingPort = parsed;
            }
            if (result.SignalingPort == result.Port)
                throw new ArgumentException(
                    "Match and signaling ports must be different.");
            if (!string.IsNullOrEmpty(origins))
            {
                string[] parts = origins.Split(',');
                List<string> normalized = new List<string>();
                for (int i = 0; i < parts.Length; i++)
                {
                    string value = parts[i].Trim();
                    Uri uri;
                    if (!Uri.TryCreate(value, UriKind.Absolute, out uri) ||
                        (uri.Scheme != "http" && uri.Scheme != "https") ||
                        uri.AbsolutePath != "/")
                    {
                        throw new ArgumentException("Dedicated server origin is invalid.");
                    }
                    normalized.Add(uri.GetLeftPart(UriPartial.Authority));
                }
                result.AllowedOrigins = normalized.ToArray();
            }
            if (!string.IsNullOrWhiteSpace(ratingFile))
                result.RatingFile = Path.GetFullPath(ratingFile);
            if (!string.IsNullOrWhiteSpace(contentCatalogFile))
                result.ContentCatalogFile =
                    Path.GetFullPath(contentCatalogFile);
            Uri listen;
            if (!Uri.TryCreate(result.ListenPrefix, UriKind.Absolute, out listen))
                throw new ArgumentException("Dedicated server bind address is invalid.");
            return result;
        }

        public static bool HasServerFlag(string[] arguments)
        {
            string[] args = arguments ?? Array.Empty<string>();
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "--cot-server") return true;
            return false;
        }

        private static string Value(string[] args, string name)
        {
            string prefix = name + "=";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith(prefix, StringComparison.Ordinal))
                    return args[i].Substring(prefix.Length);
                if (args[i] == name && i + 1 < args.Length)
                    return args[i + 1];
            }
            return null;
        }
    }

    public sealed class DedicatedServerTickScheduler
    {
        private double _accumulatorS;

        public int Consume(double elapsedS)
        {
            if (double.IsNaN(elapsedS) || double.IsInfinity(elapsedS) || elapsedS < 0.0)
                throw new ArgumentOutOfRangeException(nameof(elapsedS));
            _accumulatorS += Math.Min(elapsedS, 0.25);
            int requested = Math.Min(
                AuthoritativeMatchHost.MaximumCatchUpTicks,
                (int)(_accumulatorS / BattleState.FixedDeltaTime));
            _accumulatorS -= requested * BattleState.FixedDeltaTime;
            return requested;
        }
    }

#if !COT_STANDALONE_SERVER
    public sealed class DedicatedServerRunner : MonoBehaviour
    {
        private DedicatedServerOptions _options;
        private DedicatedServerHost _host;
        private double _lastTimeS;

        public DedicatedMatchRegistry Registry => _host?.Registry;
        public bool IsRunning => _host != null && _host.IsRunning;
        public string ListenPrefix => _options?.ListenPrefix;

        public static DedicatedServerRunner Create(DedicatedServerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            GameObject root = new GameObject("Claude of Tanks Dedicated Server");
            root.SetActive(false);
            DedicatedServerRunner runner = root.AddComponent<DedicatedServerRunner>();
            runner._options = options;
            root.SetActive(true);
            if (Application.isPlaying) DontDestroyOnLoad(root);
            return runner;
        }

        private void OnEnable()
        {
            if (_options == null || _host != null) return;
            Application.runInBackground = true;
            Application.targetFrameRate = NetworkProtocol.TickRate;
            _host = new DedicatedServerHost(
                _options,
                ContentCatalog.Load(),
                Path.Combine(
                    Application.persistentDataPath,
                    "ranked-ratings.bin"));
            _host.Start();
            _lastTimeS = Time.realtimeSinceStartupAsDouble;
            Debug.Log("Dedicated server listening at " + _options.ListenPrefix);
            Debug.Log(
                "Signaling server listening at " +
                _options.SignalingListenPrefix);
        }

        private void Update()
        {
            if (_host == null) return;
            double now = Time.realtimeSinceStartupAsDouble;
            _host.PumpElapsed(now - _lastTimeS);
            _lastTimeS = now;
        }

        private void OnDestroy()
        {
            _host?.Dispose();
            _host = null;
        }
    }

    public static class DedicatedServerBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            string[] arguments = Environment.GetCommandLineArgs();
#if UNITY_SERVER
            bool shouldStart = true;
#else
            bool shouldStart =
                Application.isBatchMode &&
                DedicatedServerOptions.HasServerFlag(arguments);
#endif
            if (!shouldStart) return;
            DedicatedServerRunner.Create(DedicatedServerOptions.Parse(arguments));
        }
    }
#endif
}
