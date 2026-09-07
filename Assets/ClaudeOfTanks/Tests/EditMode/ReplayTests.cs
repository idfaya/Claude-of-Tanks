using System.Collections.Generic;
using System;
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

        [Test]
        public void ReplayWireCodecRoundTripsLandformsAndAuthoritativeResult()
        {
            TerrainLandform[] landforms =
            {
                new TerrainLandform
                {
                    Kind = "ridge",
                    X = 12f,
                    Z = -8f,
                    Height = 5f,
                    Length = 80f,
                    Width = 24f,
                    RadiusX = 12f,
                    RadiusZ = 40f,
                    YawRad = 0.4f
                }
            };
            BattleState state = new BattleState(
                new LandformHeightField(landforms),
                902u,
                500f,
                new[]
                {
                    new StaticObstacle(
                        "replay-building",
                        new Float3(30f, 1f, 40f),
                        6f,
                        9f,
                        12f,
                        0.4f,
                        StaticObstacleFlags.All,
                        true)
                });
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), new Float3(0f, 0f, -20f), 0f));
            state.Tanks.Add(new TankState(
                "bravo", Team.Bravo, TankSpec.Heavy(), new Float3(0f, 0f, 30f), MathUtil.Pi));
            BattleReplayRecorder recorder = new BattleReplayRecorder(state, GameModeId.Standard);
            BattleSimulation original = new BattleSimulation(state, GameModeId.Standard);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>();
            for (int tick = 0; tick < 240; tick++)
            {
                inputs["alpha"] = new TankInput
                {
                    Throttle = tick < 60 ? 1f : 0f,
                    Fire = tick == 90,
                    AimPoint = state.Tanks[1].Position
                };
                inputs["bravo"] = new TankInput { AimPoint = state.Tanks[0].Position };
                recorder.Record(inputs, BattleState.FixedDeltaTime);
                original.Step(inputs, BattleState.FixedDeltaTime);
            }

            byte[] encoded = ReplayWireCodec.Encode(recorder.Recording);
            ReplayRecording decoded = ReplayWireCodec.Decode(encoded);
            BattleSimulation replay = BattleReplayPlayer.Play(decoded);
            Assert.That(decoded.FrameCount, Is.EqualTo(240));
            Assert.That(decoded.TankCount, Is.EqualTo(2));
            Assert.That(decoded.GetTankId(0), Is.EqualTo("alpha"));
            Assert.That(decoded.GetTankSpecId(1), Is.EqualTo("heavy"));
            Assert.That(decoded.GetTankTeam(1), Is.EqualTo(Team.Bravo));
            Assert.That(decoded.StaticObstacleCount, Is.EqualTo(1));
            Assert.That(replay.State.StaticObstacles[0].Id, Is.EqualTo("replay-building"));
            Assert.That(replay.State.StaticObstacles[0].Center, Is.EqualTo(new Float3(30f, 1f, 40f)));
            Assert.That(replay.State.StaticObstacles[0].HalfWidthM, Is.EqualTo(6f));
            Assert.That(replay.State.StaticObstacles[0].HalfLengthM, Is.EqualTo(9f));
            Assert.That(replay.State.StaticObstacles[0].HeightM, Is.EqualTo(12f));
            Assert.That(replay.State.StaticObstacles[0].YawRad, Is.EqualTo(0.4f));
            Assert.That(replay.State.StaticObstacles[0].Flags, Is.EqualTo(StaticObstacleFlags.All));
            Assert.That(replay.State.StaticObstacles[0].Destructible, Is.True);
            Assert.That(replay.State.Tanks[0].Position, Is.EqualTo(original.State.Tanks[0].Position));
            Assert.That(replay.State.Tanks[1].Health, Is.EqualTo(original.State.Tanks[1].Health));
            Assert.That(replay.State.HeightField.HeightAt(12f, -8f), Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void ReplayWireCodecRejectsTruncatedAndTrailingData()
        {
            BattleState state = new BattleState(new FlatHeightField(), 22u);
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f));
            BattleReplayRecorder recorder = new BattleReplayRecorder(state, GameModeId.Standard);
            recorder.Record(new Dictionary<string, TankInput>(), BattleState.FixedDeltaTime);
            byte[] packet = ReplayWireCodec.Encode(recorder.Recording);

            byte[] truncated = new byte[packet.Length - 1];
            Buffer.BlockCopy(packet, 0, truncated, 0, truncated.Length);
            Assert.Throws<FormatException>(() => ReplayWireCodec.Decode(truncated));

            Array.Resize(ref packet, packet.Length + 1);
            Assert.Throws<FormatException>(() => ReplayWireCodec.Decode(packet));
        }

        [Test]
        public void ReplayWireCodecReadsVersionOneWithoutStaticObstacles()
        {
            BattleState state = new BattleState(new FlatHeightField(), 23u);
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f));
            BattleReplayRecorder recorder = new BattleReplayRecorder(state, GameModeId.Standard);
            byte[] versionTwo = ReplayWireCodec.Encode(recorder.Recording);
            const int obstacleCountOffset = 16;
            byte[] versionOne = new byte[versionTwo.Length - 2];
            Buffer.BlockCopy(versionTwo, 0, versionOne, 0, obstacleCountOffset);
            Buffer.BlockCopy(
                versionTwo,
                obstacleCountOffset + 2,
                versionOne,
                obstacleCountOffset,
                versionTwo.Length - obstacleCountOffset - 2);
            versionOne[4] = 1;
            versionOne[5] = 0;

            ReplayRecording decoded = ReplayWireCodec.Decode(versionOne);

            Assert.That(decoded.StaticObstacleCount, Is.Zero);
            Assert.That(decoded.TankCount, Is.EqualTo(1));
        }

        [Test]
        public void ReplayWireCodecRejectsInputsForUnknownTank()
        {
            BattleState state = new BattleState(new FlatHeightField(), 24u);
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f));
            BattleReplayRecorder recorder = new BattleReplayRecorder(state, GameModeId.Standard);
            recorder.Record(
                new Dictionary<string, TankInput>
                {
                    ["unknown"] = new TankInput { AimPoint = Float3.Zero }
                },
                BattleState.FixedDeltaTime);

            Assert.Throws<System.IO.InvalidDataException>(
                () => ReplayWireCodec.Encode(recorder.Recording));
        }
    }
}
