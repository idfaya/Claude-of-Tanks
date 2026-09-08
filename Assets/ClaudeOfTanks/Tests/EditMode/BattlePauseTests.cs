using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattlePauseTests
    {
        [Test]
        public void NetworkPauseSendsNeutralBrakingCommand()
        {
            TankState tank = new TankState(
                "player",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0.7f);
            tank.TurretYaw = -0.2f;

            NetworkInputCommand command =
                NetworkBattlePauseInput.Create(
                    tank,
                    12u,
                    5u,
                    48L);

            Assert.That(command.Sequence, Is.EqualTo(12u));
            Assert.That(command.ActionSequence, Is.EqualTo(5u));
            Assert.That(command.ClientTick, Is.EqualTo(48L));
            Assert.That(command.Throttle, Is.Zero);
            Assert.That(command.Steer, Is.Zero);
            Assert.That(command.Brake, Is.True);
            Assert.That(command.Actions, Is.EqualTo(NetworkActionBits.None));
            Assert.That(command.AimYawRad, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(command.AimPitchRad, Is.Zero);
            Assert.That(command.AimDistanceM, Is.EqualTo(100f));
        }
    }
}
