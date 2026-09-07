using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class LoadoutTests
    {
        [Test]
        public void EquipmentSanitizesDuplicatesEraAndSlotLimit()
        {
            string[] result = LoadoutSimulation.SanitizeEquipment(
                new[] { "rammer", "rammer", "vstab", "toolbox", "wet_rack" },
                false,
                false);
            Assert.That(result, Is.EqualTo(new[] { "rammer", "toolbox", "wet_rack" }));
            Assert.That(
                LoadoutSimulation.SanitizeEquipment(new[] { "rammer" }, true, true),
                Is.Empty);
        }

        [Test]
        public void EquipmentAppliesCombatAndMobilityEffects()
        {
            TankState tank = Tank();
            float rackHealth = tank.Combat.Modules["ammoRack"].MaxHealth;
            LoadoutSimulation.ApplyEquipment(
                tank, new[] { "rotation", "toolbox", "wet_rack" });
            Assert.That(tank.TraverseMultiplier, Is.EqualTo(1.1f));
            Assert.That(tank.TurretMultiplier, Is.EqualTo(1.1f));
            Assert.That(tank.Combat.Equipment.RepairRate, Is.EqualTo(1.25f));
            Assert.That(tank.Combat.Modules["ammoRack"].MaxHealth, Is.EqualTo(rackHealth * 1.5f));
        }

        [Test]
        public void ConsumablesRepairHealAndExtinguishWithCooldown()
        {
            TankState tank = Tank();
            DamageSimulation.DamageModule(tank.Combat, "trackL", 1000f);
            Assert.That(
                LoadoutSimulation.UseConsumable(tank, ConsumableSlot.RepairKit, 10f), Is.True);
            Assert.That(tank.Combat.Modules["trackL"].Condition, Is.EqualTo(DamageModuleCondition.Ok));
            Assert.That(
                LoadoutSimulation.UseConsumable(tank, ConsumableSlot.RepairKit, 20f), Is.False);

            DamageSimulation.KnockOutCrew(tank.Combat, "loader");
            Assert.That(
                LoadoutSimulation.UseConsumable(tank, ConsumableSlot.FirstAidKit, 10f), Is.True);
            Assert.That(tank.Combat.Crew["loader"], Is.True);

            DamageSimulation.IgniteFire(tank.Combat);
            Assert.That(
                LoadoutSimulation.UseConsumable(tank, ConsumableSlot.FireExtinguisher, 10f), Is.True);
            Assert.That(tank.Combat.Fire.Burning, Is.False);
        }

        [Test]
        public void BattleInputEmitsConsumableEvent()
        {
            BattleState state = new BattleState(new FlatHeightField(), 1u);
            TankState tank = Tank();
            state.Tanks.Add(tank);
            DamageSimulation.DamageModule(tank.Combat, "trackL", 1000f);
            BattleSimulation simulation = new BattleSimulation(state);
            simulation.Step(
                new System.Collections.Generic.Dictionary<string, TankInput>
                {
                    [tank.Id] = new TankInput { UseRepairKit = true, AimPoint = new Float3(0f, 0f, 1f) }
                },
                BattleState.FixedDeltaTime);
            Assert.That(state.Events.Exists(e => e.Type == BattleEventType.ConsumableUsed), Is.True);
        }

        private static TankState Tank()
        {
            return new TankState("tank", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
        }
    }
}
