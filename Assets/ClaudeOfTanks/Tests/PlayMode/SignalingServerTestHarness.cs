using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClaudeOfTanks.WebRTC;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    internal static class SignalingServerTestHarness
    {
        public const string Origin = "https://unity.test";

        public static RoomSignalingClient CreateClient(
            Uri endpoint,
            string sessionId)
        {
            return new RoomSignalingClient(
                endpoint,
                new RoomSignalingClientOptions
                {
                    SessionId = sessionId,
                    ConnectTimeoutMs = 2000,
                    RequestTimeoutMs = 3000,
                    EventPollIntervalMs = 50,
                    EventPollTimeoutMs = 2000,
                    ReconnectDelaysMs = new[] { 20, 50, 100 },
                    Origin = Origin
                });
        }

        public static IEnumerator WaitForTask(
            Task task,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (!task.IsCompleted && DateTime.UtcNow < deadline)
                yield return null;
            Assert.That(task.IsCompleted, Is.True, "Async operation timed out.");
            if (task.IsFaulted)
                throw task.Exception?.InnerException ?? task.Exception;
        }

        public static IEnumerator WaitForServer(
            Process server,
            int port,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (server.HasExited)
                    Assert.Fail(
                        "Signaling server exited: " +
                        server.StandardError.ReadToEnd());
                using (TcpClient probe = new TcpClient())
                {
                    Task connect = probe.ConnectAsync(
                        IPAddress.Loopback,
                        port);
                    DateTime attemptDeadline =
                        DateTime.UtcNow.AddMilliseconds(250);
                    while (!connect.IsCompleted &&
                        DateTime.UtcNow < attemptDeadline)
                    {
                        yield return null;
                    }
                    if (connect.Status == TaskStatus.RanToCompletion)
                        yield break;
                }
                yield return null;
            }
            Assert.Fail("Signaling server did not become ready.");
        }

        public static Process StartServer(
            int port,
            string allowedOrigin = Origin)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ResolveNodeExecutable(),
                    Arguments = "\"" +
                        Path.Combine(root, "server/signalingServer.ts") +
                        "\" --host 127.0.0.1 --port " + port,
                    WorkingDirectory = root,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.EnvironmentVariables["COT_ALLOWED_ORIGINS"] =
                allowedOrigin;
            Assert.That(process.Start(), Is.True);
            return process;
        }

        public static int ReservePort()
        {
            TcpListener listener =
                new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static void StopServer(Process server)
        {
            if (server == null) return;
            try
            {
                if (!server.HasExited) server.Kill();
                server.WaitForExit(2000);
            }
            catch
            {
            }
            server.Dispose();
        }

        private static string ResolveNodeExecutable()
        {
            string[] pathEntries = (
                Environment.GetEnvironmentVariable("PATH") ??
                string.Empty).Split(Path.PathSeparator);
            for (int i = 0; i < pathEntries.Length; i++)
            {
                string candidate = Path.Combine(pathEntries[i], "node");
                if (File.Exists(candidate)) return candidate;
            }
            string localBin = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                ".local/bin");
            if (Directory.Exists(localBin))
            {
                string[] installs = Directory.GetDirectories(
                    localBin,
                    ".node-*",
                    SearchOption.TopDirectoryOnly);
                Array.Sort(installs, StringComparer.Ordinal);
                for (int i = installs.Length - 1; i >= 0; i--)
                {
                    string candidate =
                        Path.Combine(installs[i], "bin/node");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            Assert.Fail("Node executable was not found for signaling test.");
            return string.Empty;
        }
    }
}
