using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class AuthoritativeRoomTests
    {
        [Test]
        public void LobbyKeepsPlayerEntityAndVehicleIdentitiesIndependent()
        {
            using (AuthoritativeRoom room = Room(teamSize: 2))
            {
                RoomJoinReceipt second = room.Join(
                    "player-two", "Commander", RoomTeam.Alpha, "m1a2");
                RoomJoinReceipt spectator = room.Join(
                    "observer", "Observer", RoomTeam.Spectator);
                room.SelectVehicle("host", "m1a2");
                room.SetReady("host", true);
                room.SetReady("player-two", true);

                Assert.That(second.Player.DisplayName, Is.EqualTo("Commander (2)"));
                Assert.That(second.Player.EntityId, Is.Not.EqualTo("m1a2"));
                Assert.That(spectator.Player.Team, Is.EqualTo(RoomTeam.Spectator));
                Assert.Throws<RoomPolicyException>(() =>
                    room.SelectVehicle("player-two", "t90m"));

                RoomMatchPlan plan = room.Start("host", 7001u);

                Assert.That(plan.Seats, Has.Length.EqualTo(2));
                Assert.That(plan.SpectatorPlayerIds, Is.EqualTo(new[] { "observer" }));
                Assert.That(plan.Seats[0].EntityId, Is.Not.EqualTo(plan.Seats[1].EntityId));
                Assert.That(plan.Seats[0].VehicleSpecId, Is.EqualTo("m1a2"));
                Assert.That(plan.Seats[1].VehicleSpecId, Is.EqualTo("m1a2"));
                Assert.That(room.Phase, Is.EqualTo(RoomPhase.Starting));

                room.MarkPlaying();
                room.Finish("alpha", "elimination");
                RoomStateSnapshot state = room.Snapshot();
                Assert.That(state.Phase, Is.EqualTo(RoomPhase.Waiting));
                Assert.That(state.Round, Is.EqualTo(1));
                Assert.That(state.LastResult, Is.EqualTo("alpha"));
                Assert.That(Player(state, "host").Ready, Is.False);
                Assert.That(Player(state, "player-two").Ready, Is.False);
            }
        }

        [Test]
        public void HostPolicyEnforcesCapacityAndClearsReadyOnRuleChanges()
        {
            using (AuthoritativeRoom room = Room(teamSize: 1))
            {
                room.SelectVehicle("host", "m1a2");
                room.SetReady("host", true);
                RoomJoinReceipt second = room.Join(
                    "player-two", "Guest", RoomTeam.Alpha, "t90m");

                Assert.That(second.Player.Team, Is.EqualTo(RoomTeam.Bravo));
                Assert.Throws<RoomPolicyException>(() =>
                    room.Join("player-three", "Full", RoomTeam.Alpha, "leo2a7"));
                Assert.Throws<RoomPolicyException>(() =>
                    room.SetMap("player-two", "cinder"));

                room.SetMap("host", "cinder");
                Assert.That(Player(room.Snapshot(), "host").Ready, Is.False);
                room.SelectCamo("player-two", "winter");
                Assert.That(Player(room.Snapshot(), "player-two").CamoId, Is.EqualTo("winter"));

                room.SetGameMode("host", GameModeId.EndlessHorde);
                RoomStateSnapshot horde = room.Snapshot();
                Assert.That(Player(horde, "host").Team, Is.EqualTo(RoomTeam.Alpha));
                Assert.That(Player(horde, "player-two").Team, Is.EqualTo(RoomTeam.Alpha));
                Assert.That(horde.TeamSize, Is.EqualTo(2));
                Assert.Throws<RoomPolicyException>(() =>
                    room.SetTeam("player-two", RoomTeam.Bravo));
            }
        }

        [Test]
        public void ReconnectPreservesSeatAndRotatesSingleUseToken()
        {
            using (AuthoritativeRoom room = Room(teamSize: 2))
            {
                RoomJoinReceipt joined = room.Join(
                    "player-two", "Guest", RoomTeam.Bravo, "t90m");
                string entityId = joined.Player.EntityId;
                room.SetReady("player-two", true);
                room.Disconnect("player-two", 1000, 500);

                RoomStateSnapshot disconnected = room.Snapshot();
                Assert.That(Player(disconnected, "player-two").Connected, Is.False);
                Assert.That(Player(disconnected, "player-two").Ready, Is.False);
                Assert.Throws<RoomPolicyException>(() =>
                    room.Reconnect("player-two", "wrong", 1200));

                RoomJoinReceipt resumed = room.Reconnect(
                    "player-two", joined.ResumeToken, 1200);
                Assert.That(resumed.Player.EntityId, Is.EqualTo(entityId));
                Assert.That(resumed.Player.Connected, Is.True);
                Assert.That(resumed.ResumeToken, Is.Not.EqualTo(joined.ResumeToken));

                room.Disconnect("player-two", 1300, 100);
                Assert.Throws<RoomPolicyException>(() =>
                    room.Reconnect("player-two", joined.ResumeToken, 1350));
                room.ExpireReservations(1401);
                Assert.That(Player(room.Snapshot(), "player-two"), Is.Null);
            }
        }

        [Test]
        public void ExpiredHostReservationMigratesHostDeterministically()
        {
            using (AuthoritativeRoom room = Room(teamSize: 2))
            {
                room.Join("zulu", "Zulu", RoomTeam.Bravo, "t90m");
                room.Join("alpha", "Alpha", RoomTeam.Alpha, "leo2a7");
                room.Disconnect("host", 10, 10);
                room.ExpireReservations(21);

                RoomStateSnapshot state = room.Snapshot();
                Assert.That(state.HostPlayerId, Is.EqualTo("alpha"));
                Assert.That(Player(state, "alpha").IsHost, Is.True);
                Assert.That(Player(state, "host"), Is.Null);
            }
        }

        [Test]
        public void EquipmentSelectionRejectsUnknownAndVehicleIllegalItems()
        {
            using (AuthoritativeRoom room = Room(
                teamSize: 1,
                equipmentAllowed: (vehicleId, equipmentId) =>
                    vehicleId == "m1a2"
                        ? equipmentId != "camo_net"
                        : equipmentId != "vstab"))
            {
                room.SelectVehicle("host", "m1a2");
                room.SelectEquipment(
                    "host",
                    "rammer",
                    "unknown",
                    "camo_net",
                    "vents");
                Assert.That(
                    Player(room.Snapshot(), "host").Equipment,
                    Is.EqualTo(new[] { "rammer", "vents" }));

                room.SelectVehicle("host", "t90m");
                Assert.That(
                    Player(room.Snapshot(), "host").Equipment,
                    Is.EqualTo(new[] { "rammer", "vents" }));
                room.SelectEquipment("host", "vstab", "optics");
                Assert.That(
                    Player(room.Snapshot(), "host").Equipment,
                    Is.EqualTo(new[] { "optics" }));
            }
        }

        private static AuthoritativeRoom Room(
            int teamSize,
            Func<string, string, bool> equipmentAllowed = null)
        {
            return new AuthoritativeRoom(
                "ABC123",
                "host",
                "Commander",
                null,
                teamSize,
                vehicleAllowed: id => id == "m1a2" || id == "t90m" || id == "leo2a7",
                mapAllowed: id => id == "random" || id == "cinder",
                equipmentAllowed: equipmentAllowed);
        }

        private static RoomPlayerSnapshot Player(
            RoomStateSnapshot state,
            string playerId)
        {
            for (int i = 0; i < state.Players.Length; i++)
                if (state.Players[i].PlayerId == playerId) return state.Players[i];
            return null;
        }
    }
}
