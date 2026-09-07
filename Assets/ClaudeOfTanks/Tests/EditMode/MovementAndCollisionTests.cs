using ClaudeOfTanks.Simulation;
using NUnit.Framework;

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
