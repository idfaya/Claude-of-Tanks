using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Server;
using ClaudeOfTanks.Simulation;
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
        [UnityTest]
        public IEnumerator RealSignalingComposesAndRotatesRtcPeer()
        {
            int port = SignalingServerTestHarness.ReservePort();
            RoomSignalingWebSocketService server =
                SignalingServerTestHarness.StartServer(port);
            GameObject ownerObject = new GameObject("PrivateRoomRtcTest");
            RtcTestCoroutineOwner owner =
                ownerObject.AddComponent<RtcTestCoroutineOwner>();
            RoomSignalingClient hostSignaling = null;
            RoomSignalingClient clientSignaling = null;
            RoomSignalingClient secondClientSignaling = null;
            PrivateRoomHostRtcSession host = null;
            PrivateRoomClientRtcSession client = null;
            PrivateRoomClientRtcSession secondClient = null;
            PrivateRoomAuthoritativeHostRuntime hostMatch = null;
            PrivateRoomNetworkClientRuntime clientMatch = null;
            PrivateRoomNetworkClientRuntime secondClientMatch = null;
            try
            {
                yield return SignalingServerTestHarness.WaitForServer(
                    server,
                    port,
                    TimeSpan.FromSeconds(5));
                Uri endpoint =
                    new Uri("ws://127.0.0.1:" + port + "/signal");
                hostSignaling = SignalingServerTestHarness.CreateClient(
                    endpoint,
                    "host-session-one");
                clientSignaling = SignalingServerTestHarness.CreateClient(
                    endpoint,
                    "client-session-one");

                Task<SignalingRoomInfo> create =
                    hostSignaling.CreateRoomAsync(
                        "unity-host",
                        "Unity Host",
                        4);
                yield return SignalingServerTestHarness.WaitForTask(
                    create,
                    TimeSpan.FromSeconds(8));
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
                yield return SignalingServerTestHarness.WaitForTask(
                    join,
                    TimeSpan.FromSeconds(8));
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

                secondClientSignaling =
                    SignalingServerTestHarness.CreateClient(
                    endpoint,
                    "second-client-session-one");
                Task<SignalingRoomInfo> secondJoin =
                    secondClientSignaling.JoinRoomAsync(
                        created.RoomCode,
                        "unity-client-two",
                        "Unity Client Two");
                yield return SignalingServerTestHarness.WaitForTask(
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

                BattleState battle = new BattleState(
                    new FlatHeightField(),
                    1701u);
                TankState hostTank = AddTank(
                    battle,
                    "host-entity",
                    Team.Alpha,
                    -15f);
                TankState clientTank = AddTank(
                    battle,
                    "client-entity",
                    Team.Alpha,
                    0f);
                AddTank(
                    battle,
                    "second-client-entity",
                    Team.Bravo,
                    15f);
                AuthoritativeMatchHost authority =
                    new AuthoritativeMatchHost(
                        new BattleSimulation(battle));
                authority.RegisterPlayer("unity-host", hostTank.Id);
                authority.RegisterPlayer("unity-client", clientTank.Id);
                authority.RegisterPlayer(
                    "unity-client-two",
                    "second-client-entity");
                hostMatch = new PrivateRoomAuthoritativeHostRuntime(
                    host,
                    authority,
                    "unity-host",
                    hostTank.Id);
                clientMatch = new PrivateRoomNetworkClientRuntime(
                    client,
                    "unity-client",
                    clientTank.Id);
                secondClientMatch = new PrivateRoomNetworkClientRuntime(
                    secondClient,
                    "unity-client-two",
                    "second-client-entity");

                long tickBefore = authority.Tick;
                Assert.That(hostMatch.Update(1), Is.EqualTo(1));
                Assert.That(
                    authority.Tick,
                    Is.EqualTo(tickBefore + 1),
                    "authority must advance once regardless of peer count");
                Assert.That(clientMatch.SendInput(new NetworkInputCommand
                {
                    Sequence = 1u,
                    ClientTick = authority.Tick,
                    SnapshotAckTick = -1,
                    Throttle = 1f,
                    AimDistanceM = 100f
                }), Is.True);
                yield return PumpMatchUntil(
                    hostMatch,
                    new[] { clientMatch, secondClientMatch },
                    () => clientMatch.LatestSnapshot != null &&
                        secondClientMatch.LatestSnapshot != null &&
                        clientTank.Position.Z > 0f,
                    TimeSpan.FromSeconds(5));
                long snapshotTickBeforeRotation =
                    clientMatch.LatestSnapshot.Tick;

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
                Assert.That(
                    clientMatch.TransportGeneration,
                    Is.EqualTo(2));
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
                Assert.That(clientMatch.SendInput(new NetworkInputCommand
                {
                    Sequence = 2u,
                    ClientTick = authority.Tick,
                    SnapshotAckTick = snapshotTickBeforeRotation,
                    Throttle = 1f,
                    AimDistanceM = 100f
                }), Is.True);
                yield return PumpMatchUntil(
                    hostMatch,
                    new[] { clientMatch, secondClientMatch },
                    () => clientMatch.LatestSnapshot.Tick >
                        snapshotTickBeforeRotation,
                    TimeSpan.FromSeconds(5));

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
                Assert.That(hostMatch.RemotePeerCount, Is.EqualTo(2));
                Assert.That(clientMatch.TransportGeneration, Is.EqualTo(3));
                Assert.That(
                    secondClientMatch.TransportGeneration,
                    Is.EqualTo(2));

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
                long secondSnapshotTick =
                    secondClientMatch.LatestSnapshot.Tick;
                yield return PumpMatchUntil(
                    hostMatch,
                    new[] { clientMatch, secondClientMatch },
                    () => secondClientMatch.LatestSnapshot.Tick >
                        secondSnapshotTick,
                    TimeSpan.FromSeconds(5));
            }
            finally
            {
                secondClientMatch?.Dispose();
                clientMatch?.Dispose();
                hostMatch?.Dispose();
                secondClient?.Dispose();
                client?.Dispose();
                host?.Dispose();
                secondClientSignaling?.Dispose();
                clientSignaling?.Dispose();
                hostSignaling?.Dispose();
                UnityEngine.Object.DestroyImmediate(ownerObject);
                SignalingServerTestHarness.StopServer(server);
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

        private static IEnumerator PumpMatchUntil(
            PrivateRoomAuthoritativeHostRuntime host,
            PrivateRoomNetworkClientRuntime[] clients,
            Func<bool> condition,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                for (int i = 0; i < clients.Length; i++)
                    clients[i].Pump();
                host.Update(1);
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("Private-room authoritative match timed out.");
        }

        private static TankState AddTank(
            BattleState battle,
            string id,
            Team team,
            float x)
        {
            TankState tank = new TankState(
                id,
                team,
                TankSpec.Medium(),
                new Float3(x, 0f, 0f),
                0f);
            battle.Tanks.Add(tank);
            return tank;
        }

    }
}
