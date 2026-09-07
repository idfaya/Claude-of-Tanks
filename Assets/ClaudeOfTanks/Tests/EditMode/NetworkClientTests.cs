using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class NetworkClientTests
    {
        [Test]
        public void SnapshotBufferInterpolatesAnglesAndBoundsExtrapolation()
        {
            SnapshotBuffer buffer = new SnapshotBuffer(4);
            NetworkWorldSnapshot first = Snapshot(
                3, 50.0, Entity("remote", 0f, 179f * MathUtil.Deg2Rad, 10f));
            NetworkWorldSnapshot second = Snapshot(
                6, 100.0, Entity("remote", 10f, -179f * MathUtil.Deg2Rad, 10f));

            Assert.That(buffer.Push(first), Is.True);
            Assert.That(buffer.Push(second), Is.True);
            Assert.That(buffer.Push(first), Is.False);

            SampledEntityState sample;
            Assert.That(buffer.TrySampleEntity("remote", 75.0, out sample), Is.True);
            Assert.That(sample.Position.X, Is.EqualTo(5f).Within(0.001f));
            Assert.That(MathF.Abs(sample.Yaw), Is.EqualTo(MathUtil.Pi).Within(0.02f));
            Assert.That(sample.Extrapolated, Is.False);

            Assert.That(buffer.TrySampleEntity("remote", 500.0, out sample), Is.True);
            Assert.That(
                (sample.Position - second.Entities[0].Position).Magnitude,
                Is.EqualTo(2.5f).Within(0.01f));
            Assert.That(sample.Extrapolated, Is.True);
        }

        [Test]
        public void LatestVisibilityRemovalImmediatelyDropsBufferedEntity()
        {
            SnapshotBuffer buffer = new SnapshotBuffer();
            buffer.Push(Snapshot(3, 50.0, Entity("hidden", 0f, 0f, 0f)));
            buffer.Push(Snapshot(6, 100.0));

            SampledEntityState sample;
            Assert.That(buffer.TrySampleEntity("hidden", 75.0, out sample), Is.False);
        }

        [Test]
        public void LocalPredictionReplaysUnacknowledgedInputsAndSmoothsSmallError()
        {
            LocalTankPredictor predictor = new LocalTankPredictor(
                "peer", "entity", Team.Alpha, TankSpec.Medium(), new FlatHeightField());
            NetworkEntitySnapshot authority = Entity("entity", 0f, 0f, 0f);
            predictor.Reconcile(authority, 0u);

            Assert.That(predictor.Predict(Command(1u)), Is.True);
            Assert.That(predictor.Predict(Command(2u)), Is.True);
            Float3 before = predictor.PresentedPosition;

            authority.Position = new Float3(0f, 0f, 0.01f);
            predictor.Reconcile(authority, 1u);

            Assert.That(predictor.PendingInputCount, Is.EqualTo(1));
            Assert.That(predictor.PresentedPosition.Z, Is.EqualTo(before.Z).Within(0.001f));
            float initialCorrection =
                (predictor.PresentedPosition - predictor.State.Position).Magnitude;
            predictor.AdvancePresentation(0.11f);
            float decayedCorrection =
                (predictor.PresentedPosition - predictor.State.Position).Magnitude;
            Assert.That(decayedCorrection, Is.LessThan(initialCorrection));

            authority.Position = new Float3(100f, 0f, 0f);
            predictor.Reconcile(authority, 2u);
            Assert.That(predictor.PendingInputCount, Is.Zero);
            Assert.That(predictor.PresentedPosition, Is.EqualTo(authority.Position));
        }

        private static NetworkInputCommand Command(uint sequence)
        {
            return new NetworkInputCommand
            {
                PlayerId = "peer",
                Sequence = sequence,
                ClientTick = sequence,
                Throttle = 1f,
                AimYawRad = 0f,
                AimPitchRad = 0f,
                AimDistanceM = 100f
            };
        }

        private static NetworkEntitySnapshot Entity(
            string id,
            float x,
            float yaw,
            float speed)
        {
            return new NetworkEntitySnapshot
            {
                EntityId = id,
                VehicleSpecId = "medium",
                Team = Team.Bravo,
                Position = new Float3(x, 0f, 0f),
                Yaw = yaw,
                SpeedMps = speed,
                Health = 1000f,
                MaxHealth = 1000f
            };
        }

        private static NetworkWorldSnapshot Snapshot(
            long tick,
            double serverTimeMs,
            params NetworkEntitySnapshot[] entities)
        {
            return new NetworkWorldSnapshot
            {
                Tick = tick,
                ServerTimeMs = serverTimeMs,
                Entities = entities,
                Shells = Array.Empty<NetworkShellSnapshot>(),
                Events = Array.Empty<BattleEvent>()
            };
        }
    }
}
