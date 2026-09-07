using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using ClaudeOfTanks.WebRTC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    public sealed class PrivateRoomLobbyRuntimeTests
    {
        [UnityTest]
        public IEnumerator LobbyCommandsHandoffToAuthoritativeMatch()
        {
            int port = SignalingServerTestHarness.ReservePort();
            Process server = SignalingServerTestHarness.StartServer(port);
            GameObject ownerObject = new GameObject("PrivateRoomLobbyTest");
            RtcTestCoroutineOwner owner =
                ownerObject.AddComponent<RtcTestCoroutineOwner>();
            RoomSignalingClient hostSignaling = null;
            RoomSignalingClient clientSignaling = null;
            PrivateRoomHostRtcSession hostRtc = null;
            PrivateRoomClientRtcSession clientRtc = null;
            AuthoritativeRoom room = null;
            PrivateRoomHostLobbyRuntime hostLobby = null;
            PrivateRoomClientLobbyRuntime clientLobby = null;
            PrivateRoomAuthoritativeHostRuntime hostMatch = null;
            PrivateRoomNetworkClientRuntime clientMatch = null;
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
                    "lobby-host-session");
                clientSignaling = SignalingServerTestHarness.CreateClient(
                    endpoint,
                    "lobby-client-session");

                Task<SignalingRoomInfo> create =
                    hostSignaling.CreateRoomAsync(
                        "lobby-host",
                        "Lobby Host",
                        2);
                yield return SignalingServerTestHarness.WaitForTask(
                    create,
                    TimeSpan.FromSeconds(8));
                SignalingRoomInfo created = create.GetAwaiter().GetResult();
                hostRtc = new PrivateRoomHostRtcSession(
                    hostSignaling,
                    created,
                    owner);
                room = new AuthoritativeRoom(
                    created.RoomCode,
                    "lobby-host",
                    "Lobby Host",
                    null,
                    teamSize: 1,
                    maximumPlayers: 2,
                    vehicleAllowed: id =>
                        id == "m1a2" || id == "t90m",
                    mapAllowed: id =>
                        id == "random" || id == "cinder");
                hostLobby = new PrivateRoomHostLobbyRuntime(hostRtc, room);

                Task<SignalingRoomInfo> join =
                    clientSignaling.JoinRoomAsync(
                        created.RoomCode,
                        "lobby-client",
                        "Lobby Client");
                yield return SignalingServerTestHarness.WaitForTask(
                    join,
                    TimeSpan.FromSeconds(8));
                SignalingRoomInfo joined = join.GetAwaiter().GetResult();
                clientRtc = new PrivateRoomClientRtcSession(
                    clientSignaling,
                    joined,
                    owner);
                clientLobby = new PrivateRoomClientLobbyRuntime(clientRtc);

                yield return PumpLobbyUntil(
                    hostLobby,
                    clientLobby,
                    () => clientLobby.State != null &&
                        Player(clientLobby.State, "lobby-client") != null,
                    TimeSpan.FromSeconds(12));
                Assert.That(hostLobby.PeerCount, Is.EqualTo(1));
                Assert.That(
                    Player(clientLobby.State, "lobby-client").EntityId,
                    Is.Not.EqualTo("t90m"));

                string oldSessionId = clientSignaling.SessionId;
                long revisionBeforeRebuild = clientLobby.State.Revision;
                Assert.That(
                    clientRtc.RequestRebuild("lobby_recovery_test"),
                    Is.True);
                yield return PumpLobbyUntil(
                    hostLobby,
                    clientLobby,
                    () => clientSignaling.SessionId != oldSessionId &&
                        clientLobby.TransportGeneration == 2 &&
                        clientLobby.State.Revision >= revisionBeforeRebuild,
                    TimeSpan.FromSeconds(12));
                Assert.That(hostLobby.PeerCount, Is.EqualTo(1));
                Assert.That(
                    Player(clientLobby.State, "lobby-client"),
                    Is.Not.Null);

                Assert.That(clientLobby.Submit(new LobbyCommand
                {
                    Kind = LobbyCommandKind.SetMap,
                    Text = "cinder"
                }), Is.True);
                yield return PumpLobbyUntil(
                    hostLobby,
                    clientLobby,
                    () => clientLobby.LastErrorCode == "host_only",
                    TimeSpan.FromSeconds(3));

                Assert.That(clientLobby.Submit(new LobbyCommand
                {
                    Kind = LobbyCommandKind.SelectVehicle,
                    Text = "t90m"
                }), Is.True);
                Assert.That(clientLobby.Submit(new LobbyCommand
                {
                    Kind = LobbyCommandKind.SetReady,
                    BoolValue = true
                }), Is.True);
                hostLobby.SubmitHostCommand(new LobbyCommand
                {
                    Kind = LobbyCommandKind.SelectVehicle,
                    Text = "m1a2"
                });
                hostLobby.SubmitHostCommand(new LobbyCommand
                {
                    Kind = LobbyCommandKind.SetReady,
                    BoolValue = true
                });
                yield return PumpLobbyUntil(
                    hostLobby,
                    clientLobby,
                    () => Player(clientLobby.State, "lobby-client").Ready &&
                        Player(clientLobby.State, "lobby-host").Ready,
                    TimeSpan.FromSeconds(3));

                RoomMatchPlan hostPlan =
                    hostLobby.SubmitHostCommand(new LobbyCommand
                    {
                        Kind = LobbyCommandKind.Start,
                        MatchSeed = 8801u
                    });
                Assert.That(hostPlan, Is.Not.Null);
                yield return PumpLobbyUntil(
                    hostLobby,
                    clientLobby,
                    () => clientLobby.MatchPlan != null &&
                        hostLobby.CanReleaseForMatch,
                    TimeSpan.FromSeconds(5));
                Assert.That(
                    clientLobby.MatchPlan.Seed,
                    Is.EqualTo(hostPlan.Seed));

                PrivateRoomClientMatchHandoff clientHandoff =
                    clientLobby.ReleaseForMatch();
                PrivateRoomHostMatchHandoff hostHandoff =
                    hostLobby.ReleaseForMatch();
                BattleState battle = BuildBattle(hostPlan);
                AuthoritativeMatchHost authority =
                    new AuthoritativeMatchHost(
                        new BattleSimulation(battle));
                hostMatch = hostHandoff.CreateMatchRuntime(authority);
                clientMatch = clientHandoff.CreateMatchRuntime();
                Assert.That(room.Phase, Is.EqualTo(RoomPhase.Playing));

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
                    clientMatch,
                    () => clientMatch.LatestSnapshot != null &&
                        clientMatch.LatestSnapshot.Tick >= 3,
                    TimeSpan.FromSeconds(5));
                Assert.That(
                    clientMatch.LatestSnapshot.ViewerEntityId,
                    Is.EqualTo(FindSeat(
                        hostPlan,
                        "lobby-client").EntityId));
            }
            finally
            {
                clientMatch?.Dispose();
                hostMatch?.Dispose();
                clientLobby?.Dispose();
                hostLobby?.Dispose();
                clientRtc?.Dispose();
                hostRtc?.Dispose();
                room?.Dispose();
                clientSignaling?.Dispose();
                hostSignaling?.Dispose();
                UnityEngine.Object.DestroyImmediate(ownerObject);
                SignalingServerTestHarness.StopServer(server);
            }
        }

        private static IEnumerator PumpLobbyUntil(
            PrivateRoomHostLobbyRuntime host,
            PrivateRoomClientLobbyRuntime client,
            Func<bool> condition,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                host.Pump();
                client.Pump();
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("Private-room lobby operation timed out.");
        }

        private static IEnumerator PumpMatchUntil(
            PrivateRoomAuthoritativeHostRuntime host,
            PrivateRoomNetworkClientRuntime client,
            Func<bool> condition,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                client.Pump();
                host.Update(1);
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("Private-room match handoff timed out.");
        }

        private static BattleState BuildBattle(RoomMatchPlan plan)
        {
            BattleState battle = new BattleState(
                new FlatHeightField(),
                plan.Seed);
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                RoomMatchSeat seat = plan.Seats[i];
                battle.Tanks.Add(new TankState(
                    seat.EntityId,
                    seat.Team,
                    TankSpec.Medium(),
                    new Float3(i * 15f, 0f, 0f),
                    0f));
            }
            return battle;
        }

        private static RoomPlayerSnapshot Player(
            RoomStateSnapshot state,
            string playerId)
        {
            for (int i = 0; i < state.Players.Length; i++)
            {
                if (state.Players[i].PlayerId == playerId)
                    return state.Players[i];
            }
            return null;
        }

        private static RoomMatchSeat FindSeat(
            RoomMatchPlan plan,
            string playerId)
        {
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                if (plan.Seats[i].PlayerId == playerId)
                    return plan.Seats[i];
            }
            return null;
        }
    }
}
