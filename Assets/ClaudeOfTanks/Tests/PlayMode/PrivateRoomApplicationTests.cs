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
    public sealed class PrivateRoomApplicationTests
    {
        [UnityTest]
        public IEnumerator GaragePanelsCreateJoinReadyAndStartRoom()
        {
            int port = SignalingServerTestHarness.ReservePort();
            string origin = "http://127.0.0.1:" + port;
            Process server =
                SignalingServerTestHarness.StartServer(port, origin);
            GameObject hostObject = null;
            GameObject clientObject = null;
            try
            {
                yield return SignalingServerTestHarness.WaitForServer(
                    server,
                    port,
                    TimeSpan.FromSeconds(5));
                hostObject = BuildApplication(
                    "HostApplication",
                    out PrivateRoomCoordinator host,
                    out PrivateRoomPanel hostPanel);
                clientObject = BuildApplication(
                    "ClientApplication",
                    out PrivateRoomCoordinator client,
                    out PrivateRoomPanel clientPanel);
                host.ConfigurePlayerId("application-host");
                client.ConfigurePlayerId("application-client");
                string endpoint = origin.Replace("http://", "ws://") +
                    "/signal";
                hostPanel.SetConnectionFields(
                    endpoint,
                    "Host Commander",
                    string.Empty);
                clientPanel.SetConnectionFields(
                    endpoint,
                    "Client Commander",
                    string.Empty);
                hostPanel.Open();
                clientPanel.Open();

                Assert.That(hostPanel.CreateRoom(), Is.True);
                yield return WaitUntil(
                    () => host.State == PrivateRoomUiState.Lobby &&
                        host.LobbyState != null,
                    TimeSpan.FromSeconds(8));
                Assert.That(hostPanel.RoomCode, Is.Not.Empty);

                clientPanel.SetConnectionFields(
                    endpoint,
                    "Client Commander",
                    hostPanel.RoomCode);
                Assert.That(clientPanel.JoinRoom(), Is.True);
                yield return WaitUntil(
                    () => host.LobbyState?.Players?.Length == 2 &&
                        client.LobbyState?.Players?.Length == 2 &&
                        HasVehicle(
                            client.LobbyState,
                            "application-client",
                            "t90m"),
                    TimeSpan.FromSeconds(12));
                Assert.That(
                    clientPanel.RosterText,
                    Does.Contain("Host Commander"));
                Assert.That(
                    clientPanel.RosterText,
                    Does.Contain("Client Commander"));

                Assert.That(hostPanel.ToggleReady(), Is.True);
                Assert.That(clientPanel.ToggleReady(), Is.True);
                yield return WaitUntil(
                    () => AllPlayersReady(host.LobbyState) &&
                        AllPlayersReady(client.LobbyState),
                    TimeSpan.FromSeconds(5));

                bool hostHandoffReady = false;
                bool clientHandoffReady = false;
                host.MatchHandoffReady += () => hostHandoffReady = true;
                client.MatchHandoffReady += () => clientHandoffReady = true;
                Assert.That(hostPanel.StartMatch(), Is.True);
                yield return WaitUntil(
                    () => hostHandoffReady && clientHandoffReady,
                    TimeSpan.FromSeconds(5));
                Assert.That(
                    hostPanel.StatusText,
                    Is.EqualTo("MATCH STARTING"));
                Assert.That(
                    clientPanel.StatusText,
                    Is.EqualTo("MATCH STARTING"));
            }
            finally
            {
                if (hostObject != null)
                    UnityEngine.Object.DestroyImmediate(hostObject);
                if (clientObject != null)
                    UnityEngine.Object.DestroyImmediate(clientObject);
                SignalingServerTestHarness.StopServer(server);
            }
        }

        private static GameObject BuildApplication(
            string name,
            out PrivateRoomCoordinator coordinator,
            out PrivateRoomPanel panel)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas));
            coordinator = root.AddComponent<PrivateRoomCoordinator>();
            coordinator.ConfigureContent(
                new[] { "m1a2", "t90m" },
                new[] { "cinder" });
            panel = PrivateRoomPanel.Create(
                root.transform,
                coordinator,
                () => name == "HostApplication" ? "m1a2" : "t90m",
                () => "cinder",
                () => GameModeId.Standard);
            return root;
        }

        private static bool AllPlayersReady(RoomStateSnapshot state)
        {
            if (state?.Players == null || state.Players.Length != 2)
                return false;
            for (int i = 0; i < state.Players.Length; i++)
            {
                if (!state.Players[i].Ready) return false;
            }
            return true;
        }

        private static bool HasVehicle(
            RoomStateSnapshot state,
            string playerId,
            string vehicleId)
        {
            if (state?.Players == null) return false;
            for (int i = 0; i < state.Players.Length; i++)
            {
                RoomPlayerSnapshot player = state.Players[i];
                if (player.PlayerId == playerId)
                    return player.VehicleSpecId == vehicleId;
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
                "Private room application operation timed out.");
        }
    }
}
