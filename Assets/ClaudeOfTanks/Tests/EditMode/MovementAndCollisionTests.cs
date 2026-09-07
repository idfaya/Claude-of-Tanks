using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using System.Collections.Generic;

namespace ClaudeOfTanks.Tests
{
    public sealed class MovementAndCollisionTests
    {
        [Test]
        public void HighResistanceTerrainReducesAcceleration()
        {
            TankInput input = new TankInput { Throttle = 1f, AimPoint = new Float3(0f, 0f, 100f) };
            TankState fast = new TankState("fast", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            TankState slow = new TankState("slow", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            TankMovement.Step(fast, input, new TestSurface(1f), 1f);
            TankMovement.Step(slow, input, new TestSurface(2f), 1f);
            Assert.That(slow.SpeedMps, Is.LessThan(fast.SpeedMps));
        }

        [Test]
        public void HeavyTankDealsMoreRamDamageToLightTank()
        {
            RamDamageResult result = CollisionSimulation.RamDamage(65f, 20f, 12f);
            Assert.That(result.ToVictim, Is.GreaterThan(result.ToRammer));
            Assert.That(result.Total, Is.LessThanOrEqualTo(900f));
            Assert.That(CollisionSimulation.RamDamage(45f, 45f, 2f).Total, Is.Zero);
        }

        [Test]
        public void LandformHeightFieldIsDeterministicAndReturnsSlopeNormal()
        {
            LandformHeightField field = new LandformHeightField(new[]
            {
                new TerrainLandform
                {
                    Kind = "knoll", Height = 6f, RadiusX = 20f, RadiusZ = 10f
                }
            });
            Assert.That(field.HeightAt(0f, 0f), Is.EqualTo(6f));
            Assert.That(field.HeightAt(20f, 0f), Is.Zero);
            Assert.That(field.HeightAt(8f, 0f), Is.EqualTo(field.HeightAt(-8f, 0f)));
            Assert.That(field.NormalAt(8f, 0f).X, Is.GreaterThan(0f));
        }

        [Test]
        public void TankCannotCrossRotatedStaticObstacle()
        {
            StaticObstacle obstacle = new StaticObstacle(
                "building",
                new Float3(0f, 0f, 8f),
                5f,
                1.5f,
                6f,
                0.35f,
                StaticObstacleFlags.All);
            BattleState state = new BattleState(
                new FlatHeightField(),
                41u,
                500f,
                new[] { obstacle });
            TankState tank = new TankState(
                "tank",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0f);
            state.Tanks.Add(tank);
            BattleSimulation simulation = new BattleSimulation(state);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>
            {
                ["tank"] = new TankInput
                {
                    Throttle = 1f,
                    AimPoint = new Float3(0f, 1f, 100f)
                }
            };

            for (int i = 0; i < 240; i++)
                simulation.Step(inputs, BattleState.FixedDeltaTime);

            Assert.That(tank.Position.Z, Is.LessThan(6f));
            Assert.That(tank.SpeedMps, Is.Zero);
            Assert.That(
                CollisionSimulation.CircleIntersectsObstacle(
                    tank.Position,
                    tank.Spec.CollisionRadiusM,
                    obstacle),
                Is.False);
        }

        [Test]
        public void SegmentHitReturnsRotatedObstacleSurface()
        {
            StaticObstacle obstacle = new StaticObstacle(
                "wall",
                new Float3(0f, 0f, 10f),
                4f,
                0.5f,
                3f,
                MathUtil.Pi * 0.25f,
                StaticObstacleFlags.All);

            float fraction;
            Float3 normal;
            Assert.That(
                CollisionSimulation.SegmentIntersectsObstacle(
                    new Float3(0f, 1f, 0f),
                    new Float3(0f, 1f, 20f),
                    obstacle,
                    out fraction,
                    out normal),
                Is.True);
            Assert.That(fraction, Is.InRange(0f, 1f));
            Assert.That(normal.Magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void DestroyedObstacleStopsBlockingMovementAndVision()
        {
            StaticObstacle obstacle = new StaticObstacle(
                "destructible",
                new Float3(0f, 0f, 8f),
                2f,
                1f,
                3f,
                0f,
                StaticObstacleFlags.All,
                true);
            BattleState state = new BattleState(
                new FlatHeightField(),
                42u,
                500f,
                new[] { obstacle });
            Assert.That(
                state.IsVisionOccluded(
                    new Float3(0f, 1f, 0f),
                    new Float3(0f, 1f, 20f)),
                Is.True);
            Assert.That(state.DamageStaticObstacle(0, 1000f), Is.True);
            Assert.That(state.DamageStaticObstacle(0, 1000f), Is.False);
            Assert.That(state.IsStaticObstacleDestroyed(0), Is.True);
            Assert.That(state.StaticObstacleRevision, Is.EqualTo(1u));
            Assert.That(
                state.IsVisionOccluded(
                    new Float3(0f, 1f, 0f),
                    new Float3(0f, 1f, 20f)),
                Is.False);

            TankState tank = new TankState(
                "tank", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            state.Tanks.Add(tank);
            BattleSimulation simulation = new BattleSimulation(state);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>
            {
                ["tank"] = new TankInput
                {
                    Throttle = 1f,
                    AimPoint = new Float3(0f, 1f, 100f)
                }
            };
            for (int i = 0; i < 240; i++)
                simulation.Step(inputs, BattleState.FixedDeltaTime);

            Assert.That(tank.Position.Z, Is.GreaterThan(10f));
        }

        private sealed class TestSurface : ITerrainSurface
        {
            private readonly float _resistance;
            public TestSurface(float resistance) { _resistance = resistance; }
            public float HeightAt(float x, float z) { return 0f; }
            public Float3 NormalAt(float x, float z) { return new Float3(0f, 1f, 0f); }
            public float ResistanceAt(float x, float z) { return _resistance; }
        }
    }
}
