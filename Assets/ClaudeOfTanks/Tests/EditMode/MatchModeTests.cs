using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class MatchModeTests
    {
        [Test]
        public void StandardEndsWhenOneTeamIsEliminated()
        {
            BattleState state = State();
            Destroy(state.Tanks[1]);
            BattleSimulation simulation = new BattleSimulation(state, GameModeId.Standard);
            simulation.Step(null, BattleState.FixedDeltaTime);
            Assert.That(simulation.MatchMode.Winner, Is.EqualTo(Team.Alpha));
        }

        [Test]
        public void CaptureTheFlagScoresAtCarrierHome()
        {
            BattleState state = State();
            BattleSimulation simulation = new BattleSimulation(state, GameModeId.CaptureTheFlag);
            simulation.Step(null, BattleState.FixedDeltaTime);
            state.Tanks[0].Position = new Float3(0f, 0f, 100f);
            simulation.Step(null, BattleState.FixedDeltaTime);
            state.Tanks[0].Position = new Float3(0f, 0f, -100f);
            simulation.Step(null, BattleState.FixedDeltaTime);
            Assert.That(simulation.MatchMode.AlphaScore, Is.EqualTo(1f));
        }

        [Test]
        public void ZoneControlCapturesAndAwardsScore()
        {
            BattleState state = State();
            BattleSimulation simulation = new BattleSimulation(state, GameModeId.ZoneControl);
            simulation.Step(null, BattleState.FixedDeltaTime);
            state.Tanks[0].Position = simulation.MatchMode.Zones[1];
            state.Tanks[1].Position = new Float3(400f, 0f, 400f);
            for (int i = 0; i < 600; i++) simulation.Step(null, BattleState.FixedDeltaTime);
            Assert.That(simulation.MatchMode.ZoneOwners[1], Is.EqualTo(Team.Alpha));
            Assert.That(simulation.MatchMode.AlphaScore, Is.GreaterThan(0f));
        }

        [Test]
        public void TurboBallScoresInEnemyGoal()
        {
            BattleState state = State();
            BattleSimulation simulation = new BattleSimulation(state, GameModeId.TurboBall);
            simulation.Step(null, BattleState.FixedDeltaTime);
            simulation.MatchMode.BallPosition = state.Tanks[1].Position;
            simulation.MatchMode.BallVelocity = Float3.Zero;
            simulation.Step(null, BattleState.FixedDeltaTime);
            Assert.That(simulation.MatchMode.AlphaScore, Is.EqualTo(1f));
        }

        [Test]
        public void HordeRevivesEnemiesAndAdvancesWave()
        {
            BattleState state = State();
            Destroy(state.Tanks[1]);
            BattleSimulation simulation = new BattleSimulation(state, GameModeId.EndlessHorde);
            simulation.Step(new Dictionary<string, TankInput>(), BattleState.FixedDeltaTime);
            Assert.That(simulation.MatchMode.HordeWave, Is.EqualTo(2));
            Assert.That(state.Tanks[1].Destroyed, Is.False);
            Assert.That(state.Tanks[1].Health, Is.EqualTo(state.Tanks[1].Spec.MaxHealth));
        }

        private static BattleState State()
        {
            BattleState state = new BattleState(new FlatHeightField(), 99u);
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), new Float3(0f, 0f, -100f), 0f));
            state.Tanks.Add(new TankState(
                "bravo", Team.Bravo, TankSpec.Medium(), new Float3(0f, 0f, 100f), MathUtil.Pi));
            return state;
        }

        private static void Destroy(TankState tank)
        {
            tank.Combat.Health = 0f;
            tank.Combat.Destroyed = true;
            tank.Health = 0f;
            tank.Destroyed = true;
        }
    }
}
