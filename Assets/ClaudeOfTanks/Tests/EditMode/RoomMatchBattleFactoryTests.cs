using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class RoomMatchBattleFactoryTests
    {
        [Test]
        public void StandardRoomFillsBothTeamsWithDeterministicBots()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            RoomMatchPlan plan = Plan(GameModeId.Standard, 3);

            BattleSimulation first =
                RoomMatchBattleFactory.Create(catalog, plan);
            BattleSimulation second =
                RoomMatchBattleFactory.Create(catalog, plan);

            Assert.That(first.State.Tanks, Has.Count.EqualTo(6));
            Assert.That(Count(first.State, Team.Alpha), Is.EqualTo(3));
            Assert.That(Count(first.State, Team.Bravo), Is.EqualTo(3));
            Assert.That(
                first.State.Tanks[2].Spec.Id,
                Is.EqualTo(second.State.Tanks[2].Spec.Id));
            Assert.That(
                first.State.Tanks[2].Position,
                Is.EqualTo(second.State.Tanks[2].Position));
        }

        [Test]
        public void HordeKeepsHumansOnAlphaAndBuildsEnemyWave()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            RoomMatchPlan plan = Plan(GameModeId.EndlessHorde, 4);
            plan.Seats = new[]
            {
                Seat("host", "horde-player", Team.Alpha, "m1a1")
            };

            BattleSimulation battle =
                RoomMatchBattleFactory.Create(catalog, plan);

            Assert.That(battle.State.Tanks, Has.Count.EqualTo(5));
            Assert.That(Count(battle.State, Team.Alpha), Is.EqualTo(1));
            Assert.That(Count(battle.State, Team.Bravo), Is.EqualTo(4));
            battle.Step(
                new System.Collections.Generic.Dictionary<string, TankInput>(),
                BattleState.FixedDeltaTime);
            Assert.That(battle.MatchMode.HordeWave, Is.EqualTo(1));
            Assert.That(battle.MatchMode.Winner, Is.Null);
        }

        private static RoomMatchPlan Plan(
            GameModeId mode,
            int teamSize)
        {
            return new RoomMatchPlan
            {
                Round = 1,
                Seed = 9401u,
                MapId = "verdant",
                GameMode = mode,
                TeamSize = teamSize,
                Seats = new[]
                {
                    Seat("alpha", "alpha-player", Team.Alpha, "m1a1"),
                    Seat("bravo", "bravo-player", Team.Bravo, "t90m")
                },
                SpectatorPlayerIds = Array.Empty<string>()
            };
        }

        private static RoomMatchSeat Seat(
            string playerId,
            string entityId,
            Team team,
            string vehicleId)
        {
            return new RoomMatchSeat
            {
                PlayerId = playerId,
                EntityId = entityId,
                DisplayName = playerId,
                Team = team,
                VehicleSpecId = vehicleId,
                Equipment = Array.Empty<string>(),
                CamoId = "factory"
            };
        }

        private static int Count(BattleState state, Team team)
        {
            int count = 0;
            for (int i = 0; i < state.Tanks.Count; i++)
                if (state.Tanks[i].Team == team) count++;
            return count;
        }
    }
}
