using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class SimulationTests
    {
        [Test]
        public void ForwardAxisMatchesProjectConvention()
        {
            Float3 forward = Float3.Forward(0f);
            Assert.That(forward.X, Is.EqualTo(0f).Within(0.00001f));
            Assert.That(forward.Z, Is.EqualTo(1f).Within(0.00001f));

            forward = Float3.Forward(MathUtil.Pi * 0.5f);
            Assert.That(forward.X, Is.EqualTo(1f).Within(0.00001f));
            Assert.That(forward.Z, Is.EqualTo(0f).Within(0.00001f));
        }

        [Test]
        public void RandomSequenceIsRepeatable()
        {
            DeterministicRandom first = new DeterministicRandom(6000u);
            DeterministicRandom second = new DeterministicRandom(6000u);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(first.NextFloat(), Is.EqualTo(second.NextFloat()));
            }
        }

        [Test]
        public void TankMovesForwardAtFixedStep()
        {
            TankState tank = new TankState("tank", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            TankInput input = new TankInput
            {
                Throttle = 1f,
                AimPoint = new Float3(0f, 0f, 100f)
            };

            for (int i = 0; i < 60; i++)
            {
                TankMovement.Step(tank, input, new FlatHeightField(), BattleState.FixedDeltaTime);
            }

            Assert.That(tank.Position.Z, Is.GreaterThan(1f));
            Assert.That(tank.Position.X, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void PenetrationFallsOffWithDistance()
        {
            ShellSpec shell = TankSpec.Medium().Shell;
            Assert.That(BattleSimulation.PenetrationAtDistance(shell, 100f), Is.EqualTo(shell.Pen100Mm));
            Assert.That(BattleSimulation.PenetrationAtDistance(shell, 1000f), Is.EqualTo(shell.Pen1000Mm));
            Assert.That(
                BattleSimulation.PenetrationAtDistance(shell, 550f),
                Is.EqualTo((shell.Pen100Mm + shell.Pen1000Mm) * 0.5f).Within(0.001f));
        }

        [Test]
        public void ShellSweepDamagesEnemyWithoutTunneling()
        {
            BattleState state = new BattleState(new FlatHeightField(), 6000u);
            TankState shooter = new TankState(
                "shooter", Team.Alpha, TankSpec.Medium(), new Float3(0f, 0f, 0f), 0f);
            TankState target = new TankState(
                "target", Team.Bravo, TankSpec.Medium(), new Float3(0f, 0f, 20f), MathUtil.Pi);
            state.Tanks.Add(shooter);
            state.Tanks.Add(target);
            BattleSimulation simulation = new BattleSimulation(state);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>
            {
                ["shooter"] = new TankInput
                {
                    Fire = true,
                    AimPoint = target.Position + new Float3(0f, 1.25f, 0f)
                }
            };

            simulation.Step(inputs, BattleState.FixedDeltaTime);
            inputs["shooter"] = new TankInput
            {
                AimPoint = target.Position + new Float3(0f, 1.25f, 0f)
            };
            for (int i = 0; i < 4; i++)
            {
                simulation.Step(inputs, BattleState.FixedDeltaTime);
            }

            Assert.That(target.Health, Is.LessThan(target.Spec.MaxHealth));
        }

        [Test]
        public void BattleFireConsumesAmmunitionAndStartsAuthoritativeReload()
        {
            BattleState state = new BattleState(new FlatHeightField(), 7u);
            TankState tank = new TankState("tank", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            state.Tanks.Add(tank);
            new BattleSimulation(state).Step(
                new Dictionary<string, TankInput>
                {
                    ["tank"] = new TankInput { Fire = true, AimPoint = new Float3(0f, 1f, 100f) }
                },
                BattleState.FixedDeltaTime);

            Assert.That(tank.Combat.Ammo[0], Is.EqualTo(tank.Combat.AmmoCapacity[0] - 1));
            Assert.That(tank.Combat.Reload.Kind, Is.EqualTo(DamageReloadKind.Shell));
            Assert.That(tank.ReloadRemainingS, Is.GreaterThan(0f));
        }

        [Test]
        public void FixedSeedBattleReplayIsDeterministic()
        {
            BattleState first = BuildReplayState();
            BattleState second = BuildReplayState();
            BattleSimulation firstSimulation = new BattleSimulation(first);
            BattleSimulation secondSimulation = new BattleSimulation(second);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>
            {
                ["alpha"] = new TankInput { Throttle = 1f, Fire = true, AimPoint = new Float3(0f, 1f, 30f) },
                ["bravo"] = new TankInput { Steer = 0.25f, AimPoint = new Float3(0f, 1f, -30f) }
            };
            for (int i = 0; i < 180; i++)
            {
                firstSimulation.Step(inputs, BattleState.FixedDeltaTime);
                secondSimulation.Step(inputs, BattleState.FixedDeltaTime);
            }

            Assert.That(first.Tanks[0].Position, Is.EqualTo(second.Tanks[0].Position));
            Assert.That(first.Tanks[1].Health, Is.EqualTo(second.Tanks[1].Health));
            Assert.That(first.NextShellId, Is.EqualTo(second.NextShellId));
            Assert.That(first.Shells.Count, Is.EqualTo(second.Shells.Count));
        }

        private static BattleState BuildReplayState()
        {
            BattleState state = new BattleState(new FlatHeightField(), 6000u);
            state.Tanks.Add(new TankState("alpha", Team.Alpha, TankSpec.Medium(), new Float3(0f, 0f, -20f), 0f));
            state.Tanks.Add(new TankState("bravo", Team.Bravo, TankSpec.Medium(), new Float3(0f, 0f, 20f), MathUtil.Pi));
            return state;
        }

        [Test]
        public void GeneratedContentCatalogMatchesTypeScriptRegistries()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            Assert.That(catalog.SavedVehicleCount, Is.EqualTo(165));
            Assert.That(catalog.ReleaseVehicleCount, Is.EqualTo(136));
            Assert.That(catalog.ProductionVehicleCount, Is.EqualTo(126));
            Assert.That(catalog.MapCount, Is.EqualTo(20));
            Assert.That(catalog.ContainsVehicle("m1a2"), Is.True);
            Assert.That(catalog.ContainsVehicle("t90m"), Is.True);
            Assert.That(catalog.ContainsMap("verdant"), Is.True);
            Assert.That(catalog.ContainsMap("skybridge"), Is.True);
        }
    }
}
