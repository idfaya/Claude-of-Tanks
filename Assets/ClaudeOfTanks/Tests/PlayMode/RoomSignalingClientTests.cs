using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaudeOfTanks.Server;
using ClaudeOfTanks.WebRTC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    public sealed class RoomSignalingClientTests
    {
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
            int port = SignalingServerTestHarness.ReservePort();
            RoomSignalingWebSocketService server =
                SignalingServerTestHarness.StartServer(port);
            RoomSignalingClient host = null;
            RoomSignalingClient guest = null;
            try
            {
                yield return SignalingServerTestHarness.WaitForServer(
                    server,
                    port,
                    TimeSpan.FromSeconds(5));
                Uri endpoint = new Uri(
                    "ws://127.0.0.1:" + port + "/signal");
                host = SignalingServerTestHarness.CreateClient(
                    endpoint,
                    "host-session-one");
                guest = SignalingServerTestHarness.CreateClient(
                    endpoint,
                    "guest-session-one");
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
                yield return SignalingServerTestHarness.WaitForTask(
                    create,
                    TimeSpan.FromSeconds(8));
                SignalingRoomInfo room = create.GetAwaiter().GetResult();
                Assert.That(room.PeerId, Is.EqualTo("unity-host"));
                Assert.That(room.HostId, Is.EqualTo("unity-host"));
                Assert.That(room.RoomCode, Has.Length.EqualTo(6));

                Task<SignalingRoomInfo> join = guest.JoinRoomAsync(
                    room.RoomCode,
                    "unity-guest",
                    "Unity Guest");
                yield return SignalingServerTestHarness.WaitForTask(
                    join,
                    TimeSpan.FromSeconds(8));
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
                yield return SignalingServerTestHarness.WaitForTask(
                    restarted,
                    TimeSpan.FromSeconds(8));
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
                SignalingServerTestHarness.StopServer(server);
            }
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

    }
}
