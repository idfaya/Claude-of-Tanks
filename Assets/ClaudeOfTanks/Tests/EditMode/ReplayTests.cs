using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class ReplayTests
    {
        [Test]
        public void RecordedInputsReplayToIdenticalAuthoritativeState()
        {
            BattleState state = new BattleState(new FlatHeightField(), 6000u);
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), new Float3(0f, 0f, -20f), 0f));
            state.Tanks.Add(new TankState(
                "bravo", Team.Bravo, TankSpec.Heavy(), new Float3(0f, 0f, 30f), MathUtil.Pi));
            BattleSimulation original = new BattleSimulation(state, GameModeId.Standard);
            BattleReplayRecorder recorder = new BattleReplayRecorder(state, GameModeId.Standard);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>();

            for (int tick = 0; tick < 360; tick++)
            {
                inputs["alpha"] = new TankInput
                {
                    Throttle = tick < 90 ? 1f : 0f,
                    Steer = tick >= 90 && tick < 130 ? 0.4f : 0f,
                    Fire = tick == 140,
                    AimPoint = state.Tanks[1].Position + new Float3(0f, 1.2f, 0f)
                };
                inputs["bravo"] = new TankInput
                {
                    AimPoint = state.Tanks[0].Position + new Float3(0f, 1.2f, 0f)
                };
                recorder.Record(inputs, BattleState.FixedDeltaTime);
                original.Step(inputs, BattleState.FixedDeltaTime);
            }

            BattleSimulation replay = BattleReplayPlayer.Play(recorder.Recording);
            Assert.That(recorder.Recording.FrameCount, Is.EqualTo(360));
            Assert.That(replay.State.TimeS, Is.EqualTo(original.State.TimeS));
            Assert.That(replay.State.Tanks[0].Position, Is.EqualTo(original.State.Tanks[0].Position));
            Assert.That(replay.State.Tanks[0].Yaw, Is.EqualTo(original.State.Tanks[0].Yaw));
            Assert.That(replay.State.Tanks[1].Health, Is.EqualTo(original.State.Tanks[1].Health));
            Assert.That(replay.State.Tanks[0].Combat.Ammo, Is.EqualTo(original.State.Tanks[0].Combat.Ammo));
            Assert.That(replay.MatchMode.Winner, Is.EqualTo(original.MatchMode.Winner));

            BattleReplaySession session = new BattleReplaySession(recorder.Recording);
            session.Seek(180);
            Float3 midpoint = session.Simulation.State.Tanks[0].Position;
            Assert.That(session.CurrentFrame, Is.EqualTo(180));
            Assert.That(session.CurrentTimeS, Is.EqualTo(3f).Within(0.001f));
            session.Seek(360);
            Assert.That(session.Simulation.State.Tanks[0].Position,
                Is.EqualTo(original.State.Tanks[0].Position));
            session.Seek(180);
            Assert.That(session.Simulation.State.Tanks[0].Position, Is.EqualTo(midpoint));
            Assert.That(session.Complete, Is.False);
            session.Seek(session.FrameCount);
            Assert.That(session.Complete, Is.True);
            session.SeekTime(2.5f);
            Assert.That(session.CurrentTimeS, Is.EqualTo(2.5f).Within(0.02f));
            session.SeekTime(999f);
            Assert.That(session.Complete, Is.True);
            session.SeekTime(-5f);
            Assert.That(session.CurrentFrame, Is.EqualTo(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                session.SeekTime(float.NaN));
        }
    }
}
