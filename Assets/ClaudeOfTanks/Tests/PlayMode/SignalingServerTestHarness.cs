using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClaudeOfTanks.Server;
using ClaudeOfTanks.WebRTC;
using NUnit.Framework;

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
            RoomSignalingWebSocketService server,
            int port,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (!server.IsRunning)
                    Assert.Fail(
                        "Signaling server stopped: " +
                        server.LastError);
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

        public static RoomSignalingWebSocketService StartServer(
            int port,
            string allowedOrigin = Origin)
        {
            RoomSignalingWebSocketService server =
                new RoomSignalingWebSocketService(
                    "http://127.0.0.1:" + port + "/",
                    new[] { allowedOrigin });
            server.Start();
            return server;
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

        public static void StopServer(
            RoomSignalingWebSocketService server)
        {
            server?.Dispose();
        }
    }
}
