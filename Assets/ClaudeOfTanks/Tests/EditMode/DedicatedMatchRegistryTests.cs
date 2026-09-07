using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class DedicatedMatchRegistryTests
    {
        [Test]
        public void OneTimeTicketBecomesRotatingReconnectSession()
        {
            int token = 0;
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry(
                () => "match_fixed",
                () => "dedicated_token_value_" + (++token).ToString("D8")))
            {
                DedicatedMatchTicket[] tickets = registry.CreateMatch(
                    Plan(), Host(), 1000, "match_fixed", 500);
                DedicatedMatchAdmission first = registry.Admit(
                    "match_fixed", "alpha-player", tickets[0].TicketToken, 1200);

                Assert.That(first.EntityId, Is.EqualTo("alpha-entity"));
                Assert.That(first.ConnectionGeneration, Is.EqualTo(1));
                Assert.Throws<UnauthorizedAccessException>(() =>
                    registry.Admit("match_fixed", "alpha-player", tickets[0].TicketToken, 1201));

                DedicatedMatchAdmission second = registry.Reconnect(
                    "match_fixed", "alpha-player", first.SessionToken);
                Assert.That(second.ConnectionGeneration, Is.EqualTo(2));
                Assert.That(second.SessionToken, Is.Not.EqualTo(first.SessionToken));
                Assert.Throws<UnauthorizedAccessException>(() =>
                    registry.Reconnect("match_fixed", "alpha-player", first.SessionToken));
                Assert.That(registry.Disconnect("match_fixed", "alpha-player", 1), Is.False);
                Assert.That(registry.Get("match_fixed").ConnectedPlayerCount, Is.EqualTo(1));
                Assert.That(registry.Disconnect("match_fixed", "alpha-player", 2), Is.True);
                Assert.That(registry.Get("match_fixed").ConnectedPlayerCount, Is.Zero);
            }
        }

        [Test]
        public void ExpiredTicketIsRejected()
        {
            int token = 0;
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry(
                () => "match_expired",
                () => "dedicated_token_value_" + (++token).ToString("D8")))
            {
                DedicatedMatchTicket[] tickets = registry.CreateMatch(
                    Plan(), Host(), 100, "match_expired", 10);
                Assert.Throws<UnauthorizedAccessException>(() =>
                    registry.Admit(
                        "match_expired",
                        "alpha-player",
                        tickets[0].TicketToken,
                        111));
            }
        }

        [Test]
        public void AuthorityAdvancesUntilResultRetentionExpires()
        {
            int token = 0;
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry(
                () => "match_lifecycle",
                () => "dedicated_token_value_" + (++token).ToString("D8")))
            {
                registry.CreateMatch(Plan(), Host(), 0, "match_lifecycle");
                Assert.That(registry.Advance("match_lifecycle", 3), Is.EqualTo(3));
                registry.Finish("match_lifecycle", RatedResult.Draw, 100);
                Assert.That(registry.Advance("match_lifecycle", 3), Is.Zero);
                Assert.That(registry.Sweep(30100), Is.Zero);
                Assert.That(registry.Sweep(30101), Is.EqualTo(1));
                Assert.That(registry.MatchCount, Is.Zero);
            }
        }

        private static RoomMatchPlan Plan()
        {
            return new RoomMatchPlan
            {
                Round = 1,
                Seed = 44u,
                MapId = "verdant",
                GameMode = GameModeId.Standard,
                Seats = new[]
                {
                    new RoomMatchSeat
                    {
                        PlayerId = "alpha-player",
                        EntityId = "alpha-entity",
                        Team = Team.Alpha,
                        VehicleSpecId = "medium",
                        Equipment = Array.Empty<string>(),
                        CamoId = "factory"
                    },
                    new RoomMatchSeat
                    {
                        PlayerId = "bravo-player",
                        EntityId = "bravo-entity",
                        Team = Team.Bravo,
                        VehicleSpecId = "medium",
                        Equipment = Array.Empty<string>(),
                        CamoId = "factory"
                    }
                },
                SpectatorPlayerIds = Array.Empty<string>()
            };
        }

        private static AuthoritativeMatchHost Host()
        {
            BattleState state = new BattleState(new FlatHeightField(), 44u);
            state.Tanks.Add(new TankState(
                "alpha-entity", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f));
            state.Tanks.Add(new TankState(
                "bravo-entity", Team.Bravo, TankSpec.Medium(),
                new Float3(0f, 0f, 100f), MathUtil.Pi));
            return new AuthoritativeMatchHost(new BattleSimulation(state));
        }
    }
}
