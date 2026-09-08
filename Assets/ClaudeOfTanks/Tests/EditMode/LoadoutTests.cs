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
        public void FirepowerEquipmentStacksCanonicalMultipliers()
        {
            TankState tank = Tank();

            LoadoutSimulation.ApplyEquipment(
                tank,
                new[] { "rammer", "gld", "vents" });

            Assert.That(
                tank.DamageSpec.Gun.ReloadS,
                Is.EqualTo(tank.Spec.Shell.ReloadS * 0.9f * 0.975f)
                    .Within(0.00001f));
            Assert.That(
                tank.Combat.Reload.TotalS,
                Is.EqualTo(tank.Spec.Shell.ReloadS * 0.9f * 0.975f)
                    .Within(0.00001f));
            Assert.That(
                tank.Combat.Equipment.AimTime,
                Is.EqualTo(0.9f * 0.975f).Within(0.00001f));
            Assert.That(
                tank.Combat.Equipment.ViewRange,
                Is.EqualTo(1.025f).Within(0.00001f));
            Assert.That(
                tank.Combat.Equipment.Camouflage,
                Is.EqualTo(0.02f).Within(0.00001f));
        }

        [Test]
        public void StabilizerAndGunLayingDriveImproveDeterministicAim()
        {
            TankState baseline = Tank();
            TankState improved = Tank();
            improved.Spec.IsModern = true;
            baseline.AimBloom = 3f;
            improved.AimBloom = 3f;
            LoadoutSimulation.ApplyEquipment(
                improved,
                new[] { "vstab", "gld" });

            TankInput drive = new TankInput
            {
                Throttle = 1f,
                Steer = 0.5f,
                AimPoint = new Float3(100f, 0f, 100f)
            };
            for (int i = 0; i < 60; i++)
            {
                TankMovement.Step(
                    baseline,
                    drive,
                    new FlatHeightField(),
                    BattleState.FixedDeltaTime);
                TankMovement.Step(
                    improved,
                    drive,
                    new FlatHeightField(),
                    BattleState.FixedDeltaTime);
            }

            Assert.That(improved.AimBloom, Is.LessThan(baseline.AimBloom));
            Assert.That(
                TankMovement.DispersionSigmaRad(improved),
                Is.LessThan(TankMovement.DispersionSigmaRad(baseline)));

            baseline.AimBloom = 3f;
            improved.AimBloom = 3f;
            TankInput settle = new TankInput
            {
                AimPoint = Float3.Forward(0f) * 100f
            };
            TankMovement.Step(baseline, settle, new FlatHeightField(), 0.5f);
            TankMovement.Step(improved, settle, new FlatHeightField(), 0.5f);
            Assert.That(improved.AimBloom, Is.LessThan(baseline.AimBloom));
        }

        [Test]
        public void ApplyEquipmentRejectsEraAndAutoloaderIllegalItems()
        {
            TankSpec wwiiSpec = TankSpec.Medium();
            wwiiSpec.IsModern = false;
            TankState wwii = new TankState(
                "wwii",
                Team.Alpha,
                wwiiSpec,
                Float3.Zero,
                0f);
            LoadoutSimulation.ApplyEquipment(
                wwii,
                new[] { "vstab", "auto_ext", "rammer" });
            Assert.That(wwii.Equipment, Is.EqualTo(new[] { "rammer" }));

            TankState autoloader = Tank();
            autoloader.Combat.Magazine =
                new MagazineState { Capacity = 3, Rounds = 3 };
            LoadoutSimulation.ApplyEquipment(
                autoloader,
                new[] { "rammer", "vents" });
            Assert.That(autoloader.Equipment, Is.EqualTo(new[] { "vents" }));
        }

        [Test]
        public void SurvivalEquipmentAppliesDurabilityAndFireModifiers()
        {
            TankState durability = Tank();
            float trackHealth =
                durability.Combat.Modules["trackL"].MaxHealth;
            float rackHealth =
                durability.Combat.Modules["ammoRack"].MaxHealth;
            float fuelHealth =
                durability.Combat.Modules["fuelTank"].MaxHealth;
            LoadoutSimulation.ApplyEquipment(
                durability,
                new[] { "susp", "wet_rack", "fuel_safety" });

            Assert.That(
                durability.Combat.Modules["trackL"].MaxHealth,
                Is.EqualTo(trackHealth * 1.5f));
            Assert.That(
                durability.Combat.Modules["trackR"].MaxHealth,
                Is.EqualTo(trackHealth * 1.5f));
            Assert.That(
                durability.Combat.Modules["ammoRack"].MaxHealth,
                Is.EqualTo(rackHealth * 1.5f));
            Assert.That(
                durability.Combat.Modules["fuelTank"].MaxHealth,
                Is.EqualTo(fuelHealth * 1.5f));
            Assert.That(
                durability.Combat.Equipment.EngineFire,
                Is.EqualTo(0.5f));

            TankState fireControl = Tank();
            fireControl.Spec.IsModern = true;
            LoadoutSimulation.ApplyEquipment(
                fireControl,
                new[] { "auto_ext", "toolbox", "spall_liner" });
            Assert.That(
                fireControl.Combat.Equipment.FireTicks,
                Is.EqualTo(0.5f));
            Assert.That(
                fireControl.Combat.Equipment.Extinguish,
                Is.EqualTo(2f));
            Assert.That(
                fireControl.Combat.Equipment.RepairRate,
                Is.EqualTo(1.25f));
            Assert.That(
                fireControl.Combat.Equipment.HeSplash,
                Is.EqualTo(0.75f));
            Assert.That(
                fireControl.Combat.Equipment.CrewHe,
                Is.EqualTo(0.5f));
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
