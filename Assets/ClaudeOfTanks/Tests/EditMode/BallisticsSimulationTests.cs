using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class BallisticsSimulationTests
    {
        [Test]
        public void UsesSecondPenetrationSegment()
        {
            ShellSpec spec = new ShellSpec { Pen100Mm = 500f, Pen1000Mm = 400f, Pen2000Mm = 300f };
            Assert.That(BallisticsSimulation.PenetrationAtDistance(spec, 1500f), Is.EqualTo(350f));
            Assert.That(BallisticsSimulation.PenetrationAtDistance(spec, 3000f), Is.EqualTo(300f));
        }

        [Test]
        public void GuidedShellHasNoGravityAndTurnsAtBoundedRate()
        {
            ShellState shell = new ShellState
            {
                Spec = new ShellSpec { Guided = true, GuidanceTurnRateRadS = 1f },
                Velocity = new Float3(0f, 0f, 100f)
            };
            Assert.That(BallisticsSimulation.GuideToward(shell, new Float3(100f, 0f, 100f), 0.1f), Is.True);
            Assert.That(shell.Velocity.X, Is.GreaterThan(0f));
            BallisticsSimulation.Step(shell, 1f);
            Assert.That(shell.Velocity.Y, Is.EqualTo(0f));
        }

        [Test]
        public void DispersionIsRepeatableAndNormalized()
        {
            Float3 first = BallisticsSimulation.ApplyDispersion(
                new Float3(0f, 0f, 1f), 0.01f, new DeterministicRandom(42));
            Float3 second = BallisticsSimulation.ApplyDispersion(
                new Float3(0f, 0f, 1f), 0.01f, new DeterministicRandom(42));
            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.Magnitude, Is.EqualTo(1f).Within(0.0001f));
        }
    }
}
