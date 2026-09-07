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
    public sealed class RoomSignalingClientTests
    {
        private const string TestOrigin = "https://unity.test";

        [Test]
        public void ProtocolRejectsUnsafeIdentityAndOversizedSignals()
        {
            Assert.Throws<ArgumentException>(() =>
                RoomSignalingProtocol.CleanPlayer("bad id", "Player"));
            Assert.Throws<ArgumentException>(() =>
                RoomSignalingProtocol.NormalizeRoomCode("A!2345"));
            Assert.That(
                RoomSignalingProtocol.NormalizeRoomCode("a0-i1o2"),
                Is.EqualTo("AQLLQ2"));
            Assert.Throws<ArgumentException>(() =>
                new RoomSignalingClient(
                    new Uri("ws://127.0.0.1/signal"),
                    new RoomSignalingClientOptions
                    {
                        Origin = "ftp://invalid.example"
                    }));
            Assert.Throws<FormatException>(() =>
                RoomSignalingProtocol.ValidateSignal(new WebRtcSignal
                {
                    kind = "description",
                    description = new WebRtcDescriptionSignal
                    {
                        type = "offer",
                        sdp = new string(
                            'x',
                            WebRtcPeerSession.MaximumSdpCharacters + 1)
                    }
                }));
        }

        [UnityTest]
        public IEnumerator RealServerCreatesRelaysAndRotatesSession()
        {
            int port = ReservePort();
            Process server = StartServer(port);
            RoomSignalingClient host = null;
            RoomSignalingClient guest = null;
            try
            {
                yield return WaitForServer(
                    server,
                    port,
                    TimeSpan.FromSeconds(5));
                Uri endpoint = new Uri(
                    "ws://127.0.0.1:" + port + "/signal");
                host = CreateClient(endpoint, "host-session-one");
                guest = CreateClient(endpoint, "guest-session-one");
                List<SignalingEnvelope> hostEvents =
                    new List<SignalingEnvelope>();
                List<SignalingEnvelope> guestEvents =
                    new List<SignalingEnvelope>();
                host.EventReceived += hostEvents.Add;
                guest.EventReceived += guestEvents.Add;

                Task<SignalingRoomInfo> create = host.CreateRoomAsync(
                    "unity-host",
                    "Unity Host",
                    4);
                yield return WaitForTask(create, TimeSpan.FromSeconds(8));
                SignalingRoomInfo room = create.GetAwaiter().GetResult();
                Assert.That(room.PeerId, Is.EqualTo("unity-host"));
                Assert.That(room.HostId, Is.EqualTo("unity-host"));
                Assert.That(room.RoomCode, Has.Length.EqualTo(6));

                Task<SignalingRoomInfo> join = guest.JoinRoomAsync(
                    room.RoomCode,
                    "unity-guest",
                    "Unity Guest");
                yield return WaitForTask(join, TimeSpan.FromSeconds(8));
                SignalingRoomInfo joined = join.GetAwaiter().GetResult();
                Assert.That(joined.HostId, Is.EqualTo(room.PeerId));
                Assert.That(joined.Peers, Has.Length.EqualTo(1));
                yield return PumpUntil(
                    host,
                    hostEvents,
                    message => message.type == "peer_joined",
                    TimeSpan.FromSeconds(3));

                Assert.That(guest.SendSignal(
                    room.PeerId,
                    host.SessionId,
                    WebRtcSignal.Restart()), Is.True);
                yield return PumpUntil(
                    host,
                    hostEvents,
                    message => message.type == "room_signal",
                    TimeSpan.FromSeconds(3));
                SignalingEnvelope relayed = hostEvents.Find(
                    message => message.type == "room_signal");
                Assert.That(
                    relayed.payload.fromPeerId,
                    Is.EqualTo(joined.PeerId));
                Assert.That(
                    relayed.payload.fromSessionId,
                    Is.EqualTo(guest.SessionId));
                Assert.That(relayed.payload.signal.kind, Is.EqualTo("restart"));

                string previousSession = host.SessionId;
                Task<bool> restarted =
                    host.RestartRoomSessionAsync("test_rtc_rebuild");
                Assert.That(host.SendSignal(
                    joined.PeerId,
                    guest.SessionId,
                    WebRtcSignal.Restart()), Is.False);
                Assert.That(
                    host.QueuedSignalCount,
                    Is.LessThanOrEqualTo(
                        RoomSignalingProtocol.MaximumQueuedSignals));
                yield return WaitForTask(restarted, TimeSpan.FromSeconds(8));
                Assert.That(restarted.GetAwaiter().GetResult(), Is.True);
                Assert.That(host.SessionId, Is.Not.EqualTo(previousSession));
                yield return PumpUntil(
                    host,
                    hostEvents,
                    message => message.type == "signaling_resumed",
                    TimeSpan.FromSeconds(3));

                yield return PumpUntil(
                    guest,
                    guestEvents,
                    message =>
                        message.type == "peer_joined" &&
                        message.payload.peerId == room.PeerId &&
                        message.payload.sessionId == host.SessionId,
                    TimeSpan.FromSeconds(3));
                yield return PumpUntil(
                    guest,
                    guestEvents,
                    message =>
                        message.type == "room_signal" &&
                        message.payload.fromSessionId == host.SessionId,
                    TimeSpan.FromSeconds(3));
                Assert.That(host.QueuedSignalCount, Is.EqualTo(0));
            }
            finally
            {
                host?.Dispose();
                guest?.Dispose();
                StopServer(server);
            }
        }

        private static RoomSignalingClient CreateClient(
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
                    EventPollIntervalMs = 100,
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

        private static IEnumerator PumpUntil(
            RoomSignalingClient client,
            List<SignalingEnvelope> events,
            Predicate<SignalingEnvelope> match,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                client.PumpEvents();
                if (events.Exists(match)) yield break;
                yield return null;
            }
            Assert.Fail("Expected signaling event was not received.");
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
                {
                    Assert.Fail(
                        "Signaling server exited: " +
                        server.StandardError.ReadToEnd());
                }
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
            string[] pathEntries = (Environment.GetEnvironmentVariable("PATH") ??
                string.Empty).Split(Path.PathSeparator);
            for (int i = 0; i < pathEntries.Length; i++)
            {
                string candidate = Path.Combine(pathEntries[i], "node");
                if (File.Exists(candidate)) return candidate;
            }
            string localBin = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
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
                    string candidate = Path.Combine(installs[i], "bin/node");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            Assert.Fail("Node executable was not found for signaling test.");
            return string.Empty;
        }

        private static int ReservePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
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
