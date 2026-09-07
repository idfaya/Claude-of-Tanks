using System;
using System.Collections;
using System.Diagnostics;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    public sealed class PrivateRoomGameFlowTests
    {
        [UnityTest]
        public IEnumerator GarageBattleLobbyAndRematchStayConnected()
        {
            int port = SignalingServerTestHarness.ReservePort();
            string origin = "http://127.0.0.1:" + port;
            Process server =
                SignalingServerTestHarness.StartServer(port, origin);
            GameObject hostRoot = null;
            GameObject clientRoot = null;
            try
            {
                yield return SignalingServerTestHarness.WaitForServer(
                    server,
                    port,
                    TimeSpan.FromSeconds(5));
                hostRoot = new GameObject("HostGameFlow");
                clientRoot = new GameObject("ClientGameFlow");
                GameFlowController host =
                    hostRoot.AddComponent<GameFlowController>();
                GameFlowController client =
                    clientRoot.AddComponent<GameFlowController>();
                host.PrivateRoom.ConfigurePlayerId("game-flow-host");
                client.PrivateRoom.ConfigurePlayerId("game-flow-client");
                host.Select(5, 3, GameModeId.ZoneControl);
                client.Select(7, 0, GameModeId.Standard);
                Assert.That(
                    host.Loadout.SetEquipment(
                        "toolbox",
                        true),
                    Is.True);
                Assert.That(
                    host.Loadout.SetCamouflage(
                        "winter"),
                    Is.True);
                Assert.That(
                    client.Loadout.SetEquipment(
                        "optics",
                        true),
                    Is.True);
                Assert.That(
                    client.Loadout.SetCamouflage(
                        "desert"),
                    Is.True);
                string hostMapId = host.SelectedMapId;
                string clientVehicleId = client.SelectedVehicleId;
                string endpoint = origin.Replace("http://", "ws://") +
                    "/signal";
                host.PrivateRoomPanel.SetConnectionFields(
                    endpoint,
                    "Flow Host",
                    string.Empty);
                client.PrivateRoomPanel.SetConnectionFields(
                    endpoint,
                    "Flow Client",
                    string.Empty);

                Assert.That(host.PrivateRoomPanel.CreateRoom(), Is.True);
                yield return WaitUntil(
                    () => host.PrivateRoom.LobbyState != null,
                    TimeSpan.FromSeconds(8));
                client.PrivateRoomPanel.SetConnectionFields(
                    endpoint,
                    "Flow Client",
                    host.PrivateRoomPanel.RoomCode);
                Assert.That(client.PrivateRoomPanel.JoinRoom(), Is.True);
                yield return WaitUntil(
                    () => host.PrivateRoom.LobbyState?.Players?.Length == 2 &&
                        client.PrivateRoom.LobbyState?.Players?.Length == 2,
                    TimeSpan.FromSeconds(12));
                yield return WaitUntil(
                    () =>
                        HasLoadout(
                            host.PrivateRoom.LobbyState,
                            "game-flow-host",
                            "toolbox",
                            "winter") &&
                        HasLoadout(
                            host.PrivateRoom.LobbyState,
                            "game-flow-client",
                            "optics",
                            "desert"),
                    TimeSpan.FromSeconds(5));

                yield return ReadyAndStart(host, client);
                yield return WaitForBattle(host, client, 1);
                long firstTick =
                    client.ActiveNetworkBattle.LatestSnapshot.Tick;
                Assert.That(firstTick, Is.GreaterThanOrEqualTo(3));
                Assert.That(
                    client.ActiveNetworkBattle.VisibleTankCount,
                    Is.GreaterThanOrEqualTo(1));
                Assert.That(
                    client.ActiveNetworkBattle.LatestSnapshot.GameMode,
                    Is.EqualTo(GameModeId.ZoneControl));
                Assert.That(
                    client.ActiveNetworkBattle.LatestSnapshot.MatchMode.Zones[0],
                    Is.Not.EqualTo(
                        client.ActiveNetworkBattle.LatestSnapshot.MatchMode.Zones[2]));

                client.ActiveNetworkBattle.ReturnToRoom();
                host.ActiveNetworkBattle.ReturnToRoom();
                yield return WaitUntil(
                    () => host.IsGarageVisible &&
                        client.IsGarageVisible &&
                        host.PrivateRoom.LobbyState?.Phase ==
                            RoomPhase.Waiting &&
                        client.PrivateRoom.LobbyState?.Phase ==
                            RoomPhase.Waiting,
                    TimeSpan.FromSeconds(8));
                Assert.That(
                    host.PrivateRoom.LobbyState.Round,
                    Is.EqualTo(1));
                Assert.That(host.SelectedMapId, Is.EqualTo(hostMapId));
                Assert.That(client.SelectedMapId, Is.EqualTo(hostMapId));
                Assert.That(
                    client.SelectedVehicleId,
                    Is.EqualTo(clientVehicleId));
                Assert.That(
                    client.SelectedMode,
                    Is.EqualTo(GameModeId.ZoneControl));

                yield return ReadyAndStart(host, client);
                yield return WaitForBattle(host, client, 2);
                Assert.That(
                    client.ActiveNetworkBattle.LatestSnapshot.Tick,
                    Is.GreaterThanOrEqualTo(3));
            }
            finally
            {
                hostRoot?.GetComponent<GameFlowController>()
                    ?.Loadout.Reset();
                clientRoot?.GetComponent<GameFlowController>()
                    ?.Loadout.Reset();
                if (clientRoot != null)
                    UnityEngine.Object.DestroyImmediate(clientRoot);
                if (hostRoot != null)
                    UnityEngine.Object.DestroyImmediate(hostRoot);
                SignalingServerTestHarness.StopServer(server);
            }
        }

        private static IEnumerator ReadyAndStart(
            GameFlowController host,
            GameFlowController client)
        {
            Assert.That(host.PrivateRoomPanel.ToggleReady(), Is.True);
            Assert.That(client.PrivateRoomPanel.ToggleReady(), Is.True);
            yield return WaitUntil(
                () => AllReady(host.PrivateRoom.LobbyState) &&
                    AllReady(client.PrivateRoom.LobbyState),
                TimeSpan.FromSeconds(5));
            Assert.That(host.PrivateRoomPanel.StartMatch(), Is.True);
        }

        private static IEnumerator WaitForBattle(
            GameFlowController host,
            GameFlowController client,
            int round)
        {
            yield return WaitUntil(
                () => host.ActiveNetworkBattle != null &&
                    client.ActiveNetworkBattle != null &&
                    host.ActiveNetworkBattle.Round == round &&
                    client.ActiveNetworkBattle.Round == round &&
                    host.ActiveNetworkBattle.LatestSnapshot != null &&
                    client.ActiveNetworkBattle.LatestSnapshot != null,
                TimeSpan.FromSeconds(15));
            Assert.That(host.IsGarageVisible, Is.False);
            Assert.That(client.IsGarageVisible, Is.False);
        }

        private static bool AllReady(RoomStateSnapshot state)
        {
            if (state?.Players == null || state.Players.Length != 2)
                return false;
            for (int i = 0; i < state.Players.Length; i++)
                if (!state.Players[i].Ready) return false;
            return true;
        }

        private static bool HasLoadout(
            RoomStateSnapshot state,
            string playerId,
            string equipment,
            string camouflage)
        {
            if (state?.Players == null) return false;
            for (int i = 0; i < state.Players.Length; i++)
            {
                RoomPlayerSnapshot player = state.Players[i];
                if (player.PlayerId != playerId) continue;
                return player.CamoId == camouflage &&
                    player.Equipment != null &&
                    Array.IndexOf(
                        player.Equipment,
                        equipment) >= 0;
            }
            return false;
        }

        private static IEnumerator WaitUntil(
            Func<bool> predicate,
            TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (!predicate() && DateTime.UtcNow < deadline)
                yield return null;
            Assert.That(
                predicate(),
                Is.True,
                "Private room GameFlow operation timed out.");
        }
    }
}
