using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class SpecialActionTests
    {
        [Test]
        public void MixedGunTogglesGuidedRoundAndRestoresPreviousShell()
        {
            TankSpec spec = TankSpec.Medium();
            spec.Shells = new[]
            {
                spec.Shell,
                new ShellSpec
                {
                    Name = "ATGM",
                    Type = "HEAT",
                    Guided = true,
                    Count = 4,
                    ReloadS = 3f
                }
            };
            TankState tank = Tank(spec);

            Assert.That(
                SpecialActionSimulation.KindFor(spec),
                Is.EqualTo(
                    SpecialActionKind
                        .GuidedMissile));
            Assert.That(
                SpecialActionSimulation.Activate(tank),
                Is.True);
            Assert.That(tank.Combat.ShellSlot, Is.EqualTo(1));
            Assert.That(
                SpecialActionSimulation.Activate(tank),
                Is.True);
            Assert.That(tank.Combat.ShellSlot, Is.Zero);
        }

        [Test]
        public void AutoloaderActionStartsManualMagazineReload()
        {
            TankSpec spec = TankSpec.Medium();
            spec.MagazineSize = 3;
            spec.MagazineReloadS = 12f;
            spec.IntraClipS = 2f;
            TankState tank = Tank(spec);
            tank.Combat.Magazine.Rounds = 1;

            Assert.That(
                SpecialActionSimulation.KindFor(spec),
                Is.EqualTo(
                    SpecialActionKind
                        .MagazineReload));
            Assert.That(
                SpecialActionSimulation.Activate(tank),
                Is.True);
            Assert.That(
                tank.Combat.GunReload.Kind,
                Is.EqualTo(
                    DamageReloadKind.Magazine));
        }

        [Test]
        public void NetworkActionMapsToSharedSpecialAction()
        {
            TankState tank = Tank(TankSpec.Medium());
            TankInput input =
                NetworkProtocol.ToTankInput(
                    new NetworkInputCommand
                    {
                        PlayerId = "peer",
                        ClientTick = 1,
                        SnapshotAckTick = -1,
                        AimDistanceM = 100f,
                        Actions =
                            NetworkActionBits
                                .SpecialAction
                    },
                    tank);

            Assert.That(input.SpecialAction, Is.True);
        }

        private static TankState Tank(TankSpec spec)
        {
            return new TankState(
                "tank",
                Team.Alpha,
                spec,
                Float3.Zero,
                0f);
        }
    }
}
