using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClaudeOfTanks.WebRTC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    internal sealed class RtcTestCoroutineOwner : MonoBehaviour
    {
    }

    public sealed class PrivateRoomRtcSessionTests
    {
        private const string TestOrigin = "https://unity.test";

        [UnityTest]
        public IEnumerator RealSignalingComposesAndRotatesRtcPeer()
        {
            int port = ReservePort();
            Process server = StartServer(port);
            GameObject ownerObject = new GameObject("PrivateRoomRtcTest");
            RtcTestCoroutineOwner owner =
                ownerObject.AddComponent<RtcTestCoroutineOwner>();
            RoomSignalingClient hostSignaling = null;
            RoomSignalingClient clientSignaling = null;
            RoomSignalingClient secondClientSignaling = null;
            PrivateRoomHostRtcSession host = null;
            PrivateRoomClientRtcSession client = null;
            PrivateRoomClientRtcSession secondClient = null;
            try
            {
                yield return WaitForServer(
                    server,
                    port,
                    TimeSpan.FromSeconds(5));
                Uri endpoint =
                    new Uri("ws://127.0.0.1:" + port + "/signal");
                hostSignaling = CreateSignaling(endpoint, "host-session-one");
                clientSignaling = CreateSignaling(
                    endpoint,
                    "client-session-one");

                Task<SignalingRoomInfo> create =
                    hostSignaling.CreateRoomAsync(
                        "unity-host",
                        "Unity Host",
                        4);
                yield return WaitForTask(create, TimeSpan.FromSeconds(8));
                SignalingRoomInfo created = create.GetAwaiter().GetResult();
                host = new PrivateRoomHostRtcSession(
                    hostSignaling,
                    created,
                    owner);

                Task<SignalingRoomInfo> join =
                    clientSignaling.JoinRoomAsync(
                        created.RoomCode,
                        "unity-client",
                        "Unity Client");
                yield return WaitForTask(join, TimeSpan.FromSeconds(8));
                SignalingRoomInfo joined = join.GetAwaiter().GetResult();
                client = new PrivateRoomClientRtcSession(
                    clientSignaling,
                    joined,
                    owner,
                    new PrivateRoomRtcSessionOptions
                    {
                        FailedRebuildDelayMs = 500,
                        DisconnectedRebuildDelayMs = 500
                    });

                secondClientSignaling = CreateSignaling(
                    endpoint,
                    "second-client-session-one");
                Task<SignalingRoomInfo> secondJoin =
                    secondClientSignaling.JoinRoomAsync(
                        created.RoomCode,
                        "unity-client-two",
                        "Unity Client Two");
                yield return WaitForTask(
                    secondJoin,
                    TimeSpan.FromSeconds(8));
                SignalingRoomInfo secondJoined =
                    secondJoin.GetAwaiter().GetResult();
                secondClient = new PrivateRoomClientRtcSession(
                    secondClientSignaling,
                    secondJoined,
                    owner);

                List<PrivateRoomPeerTransport> hostTransports =
                    new List<PrivateRoomPeerTransport>();
                List<WebRtcNetworkEndpoint> clientTransports =
                    new List<WebRtcNetworkEndpoint>();
                List<WebRtcNetworkEndpoint> secondClientTransports =
                    new List<WebRtcNetworkEndpoint>();
                List<string> failures = new List<string>();
                host.TransportReady += hostTransports.Add;
                host.Failed += (peerId, reason) =>
                    failures.Add(peerId + ":" + reason);
                client.TransportReady += clientTransports.Add;
                client.Failed += failures.Add;
                secondClient.TransportReady += secondClientTransports.Add;
                secondClient.Failed += failures.Add;

                yield return PumpUntil(
                    host,
                    new[] { client, secondClient },
                    () => hostTransports.Count == 2 &&
                        clientTransports.Count == 1 &&
                        secondClientTransports.Count == 1,
                    TimeSpan.FromSeconds(12));
                Assert.That(failures, Is.Empty);
                Assert.That(host.PeerCount, Is.EqualTo(2));
                PrivateRoomPeerTransport clientHostTransport =
                    hostTransports.Find(
                        value => value.PeerId == joined.PeerId);
                Assert.That(
                    clientHostTransport.PeerId,
                    Is.EqualTo(joined.PeerId));
                Assert.That(
                    clientHostTransport.SessionId,
                    Is.EqualTo(clientSignaling.SessionId));

                byte firstPayload = 0;
                clientTransports[0].ControlReceived +=
                    packet => firstPayload = packet[0];
                Assert.That(
                    clientHostTransport.Transport.SendControl(
                        new byte[] { 41 }),
                    Is.True);
                yield return PumpTransportUntil(
                    host,
                    new[] { client, secondClient },
                    clientHostTransport.Transport,
                    clientTransports[0],
                    () => firstPayload == 41,
                    TimeSpan.FromSeconds(3));

                string oldSessionId = clientSignaling.SessionId;
                WebRtcNetworkEndpoint oldClientTransport =
                    clientTransports[0];
                Assert.That(
                    client.RequestRebuild("test_session_rotation"),
                    Is.True);
                yield return PumpUntil(
                    host,
                    new[] { client, secondClient },
                    () => hostTransports.Count == 3 &&
                        clientTransports.Count == 2,
                    TimeSpan.FromSeconds(12));

                Assert.That(failures, Is.Empty);
                Assert.That(
                    clientSignaling.SessionId,
                    Is.Not.EqualTo(oldSessionId));
                Assert.That(oldClientTransport.IsOpen, Is.False);
                Assert.That(host.PeerCount, Is.EqualTo(2));
                Assert.That(secondClientTransports[0].IsOpen, Is.True);
                PrivateRoomPeerTransport replacementHostTransport =
                    hostTransports.FindLast(
                        value => value.PeerId == joined.PeerId);
                Assert.That(
                    replacementHostTransport.SessionId,
                    Is.EqualTo(clientSignaling.SessionId));

                byte replacementPayload = 0;
                clientTransports[1].ControlReceived +=
                    packet => replacementPayload = packet[0];
                Assert.That(
                    replacementHostTransport.Transport.SendControl(
                        new byte[] { 82 }),
                    Is.True);
                yield return PumpTransportUntil(
                    host,
                    new[] { client, secondClient },
                    replacementHostTransport.Transport,
                    clientTransports[1],
                    () => replacementPayload == 82,
                    TimeSpan.FromSeconds(3));

                string oldHostSessionId = hostSignaling.SessionId;
                WebRtcNetworkEndpoint oldSecondClientTransport =
                    secondClientTransports[0];
                Assert.That(
                    host.RestartRoomSession("test_host_session_rotation"),
                    Is.True);
                yield return PumpUntil(
                    host,
                    new[] { client, secondClient },
                    () => hostTransports.Count == 5 &&
                        clientTransports.Count == 3 &&
                        secondClientTransports.Count == 2,
                    TimeSpan.FromSeconds(12));

                Assert.That(failures, Is.Empty);
                Assert.That(
                    hostSignaling.SessionId,
                    Is.Not.EqualTo(oldHostSessionId));
                Assert.That(client.HostSessionId, Is.EqualTo(
                    hostSignaling.SessionId));
                Assert.That(secondClient.HostSessionId, Is.EqualTo(
                    hostSignaling.SessionId));
                Assert.That(oldSecondClientTransport.IsOpen, Is.False);
                Assert.That(host.PeerCount, Is.EqualTo(2));

                PrivateRoomPeerTransport secondReplacementHostTransport =
                    hostTransports.FindLast(
                        value => value.PeerId == secondJoined.PeerId);
                byte secondPayload = 0;
                secondClientTransports[1].ControlReceived +=
                    packet => secondPayload = packet[0];
                Assert.That(
                    secondReplacementHostTransport.Transport.SendControl(
                        new byte[] { 123 }),
                    Is.True);
                yield return PumpTransportUntil(
                    host,
                    new[] { client, secondClient },
                    secondReplacementHostTransport.Transport,
                    secondClientTransports[1],
                    () => secondPayload == 123,
                    TimeSpan.FromSeconds(3));
            }
            finally
            {
                secondClient?.Dispose();
                client?.Dispose();
                host?.Dispose();
                secondClientSignaling?.Dispose();
                clientSignaling?.Dispose();
                hostSignaling?.Dispose();
                UnityEngine.Object.DestroyImmediate(ownerObject);
                StopServer(server);
            }
        }

        private static IEnumerator PumpUntil(
            PrivateRoomHostRtcSession host,
            PrivateRoomClientRtcSession[] clients,
            Func<bool> condition,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                host.Pump();
                for (int i = 0; i < clients.Length; i++)
                    clients[i].Pump();
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("Private-room RTC operation timed out.");
        }

        private static IEnumerator PumpTransportUntil(
            PrivateRoomHostRtcSession host,
            PrivateRoomClientRtcSession[] clients,
            WebRtcNetworkEndpoint hostTransport,
            WebRtcNetworkEndpoint clientTransport,
            Func<bool> condition,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                host.Pump();
                for (int i = 0; i < clients.Length; i++)
                    clients[i].Pump();
                hostTransport.Pump();
                clientTransport.Pump();
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("Private-room RTC packet timed out.");
        }

        private static RoomSignalingClient CreateSignaling(
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
                    Origin = TestOrigin
                });
        }

        private static IEnumerator WaitForTask(
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

        private static IEnumerator WaitForServer(
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

        private static Process StartServer(int port)
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
                TestOrigin;
            Assert.That(process.Start(), Is.True);
            return process;
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

        private static int ReservePort()
        {
            TcpListener listener =
                new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static void StopServer(Process server)
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
    }
}
