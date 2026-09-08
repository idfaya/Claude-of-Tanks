using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class DamageSimulationTests
    {
        [Test]
        public void CreateCombatStateUsesAuthoredModulesCrewAndAmmunition()
        {
            DamageTankSpec spec = MakeSpec();
            spec.IsPostwar = true;
            spec.ModuleIds.Clear();
            spec.ModuleIds.Add("engine");
            spec.ModuleIds.Add("ammoRack");
            spec.CrewIds.Clear();
            spec.CrewIds.Add("commander");
            spec.CrewIds.Add("gunner");
            spec.CrewIds.Add("driver");

            DamageCombatState state = DamageSimulation.CreateCombatState(spec);

            Assert.That(state.Modules.Count, Is.EqualTo(2));
            Assert.That(state.Modules["engine"].MaxHealth, Is.EqualTo(400f));
            Assert.That(state.Crew.ContainsKey("loader"), Is.False);
            Assert.That(state.Ammo, Is.EqualTo(new[] { 8, 16 }));
            Assert.That(state.AmmoCapacity, Is.EqualTo(new[] { 8, 16 }));
        }

        [Test]
        public void AmmoRackTurningRedDestroysTank()
        {
            DamageCombatState state = DamageSimulation.CreateCombatState(MakeSpec());

            ModuleDamageResult result = DamageSimulation.DamageModule(
                state,
                "ammoRack",
                state.Modules["ammoRack"].MaxHealth);

            Assert.That(result.Condition, Is.EqualTo(DamageModuleCondition.Red));
            Assert.That(result.AmmoRacked, Is.True);
            Assert.That(result.Destroyed, Is.True);
            Assert.That(state.Health, Is.Zero);
        }

        [Test]
        public void LosingAllAuthoredCrewDestroysTank()
        {
            DamageTankSpec spec = MakeSpec();
            spec.CrewIds.Clear();
            spec.CrewIds.Add("commander");
            spec.CrewIds.Add("driver");
            DamageCombatState state = DamageSimulation.CreateCombatState(spec);

            Assert.That(DamageSimulation.KnockOutCrew(state, "commander"), Is.True);
            Assert.That(state.Destroyed, Is.False);
            Assert.That(DamageSimulation.KnockOutCrew(state, "driver"), Is.True);

            Assert.That(state.Destroyed, Is.True);
            Assert.That(state.Health, Is.Zero);
        }

        [Test]
        public void FireUsesHalfSecondTicksAndCanCookOffAmmoRack()
        {
            DamageCombatState state = DamageSimulation.CreateCombatState(MakeSpec());
            DamageSimulation.DamageModule(state, "ammoRack", 140f);
            DamageSimulation.IgniteFire(state);

            FireTickResult beforeTick = DamageSimulation.AdvanceFire(state, 0.49f, () => 0.5f);
            Assert.That(beforeTick.Ticks, Is.Zero);
            Assert.That(state.Health, Is.EqualTo(state.MaxHealth));

            FireTickResult tick = DamageSimulation.AdvanceFire(state, 0.01f, () => 0.5f);

            Assert.That(tick.Ticks, Is.EqualTo(1));
            Assert.That(tick.Destroyed, Is.True);
            Assert.That(state.Modules["ammoRack"].Condition, Is.EqualTo(DamageModuleCondition.Red));
            Assert.That(state.Fire.Burning, Is.False);
        }

        [Test]
        public void SpallLinerReducesHeSplashAndCrewHitChance()
        {
            TankState baselineTank = new TankState(
                "baseline",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0f);
            TankState protectedTank = new TankState(
                "protected",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0f);
            LoadoutSimulation.ApplyEquipment(
                protectedTank,
                new[] { "spall_liner" });

            HeSplashResult baseline = DamageSimulation.ApplyHeSplash(
                baselineTank.Combat,
                400f,
                50f,
                () => 0.075f);
            HeSplashResult protectedResult =
                DamageSimulation.ApplyHeSplash(
                    protectedTank.Combat,
                    400f,
                    50f,
                    () => 0.075f);

            Assert.That(
                protectedResult.Damage,
                Is.EqualTo(baseline.Damage * 0.75f)
                    .Within(0.00001f));
            Assert.That(baseline.CrewHitCount, Is.EqualTo(4));
            Assert.That(protectedResult.CrewHitCount, Is.Zero);
        }

        [Test]
        public void RedModuleRepairsToYellowOnlyAfterFullDuration()
        {
            DamageCombatState state = DamageSimulation.CreateCombatState(MakeSpec());
            DamageSimulation.DamageModule(
                state,
                "trackL",
                state.Modules["trackL"].MaxHealth);

            Assert.That(
                DamageSimulation.TickModuleRepairs(state, 9.99f),
                Is.Empty);
            Assert.That(state.Modules["trackL"].Condition, Is.EqualTo(DamageModuleCondition.Red));

            Assert.That(
                DamageSimulation.TickModuleRepairs(state, 0.01f),
                Is.EqualTo(new[] { "trackL" }));
            Assert.That(state.Modules["trackL"].Condition, Is.EqualTo(DamageModuleCondition.Yellow));
            Assert.That(
                state.Modules["trackL"].Health,
                Is.EqualTo(state.Modules["trackL"].MaxHealth * 0.5f));

            Assert.That(DamageSimulation.RepairAllModules(state), Is.EqualTo(new[] { "trackL" }));
            Assert.That(state.Modules["trackL"].Condition, Is.EqualTo(DamageModuleCondition.Ok));
            Assert.That(state.Modules["trackL"].RepairElapsedS, Is.Zero);
        }

        [Test]
        public void ConventionalReloadUsesCrewRackAndAmmunitionState()
        {
            DamageTankSpec spec = MakeSpec();
            spec.Gun.Autoloader = null;
            DamageCombatState state = DamageSimulation.CreateCombatState(spec);
            state.Crew["loader"] = false;
            state.Modules["ammoRack"].Health = 60f;
            state.Modules["ammoRack"].Condition = DamageModuleCondition.Yellow;

            Assert.That(DamageSimulation.ConsumeAmmunition(state, 0), Is.True);
            Assert.That(state.Ammo[0], Is.EqualTo(7));
            DamageSimulation.StartPostShotReload(state, spec);

            Assert.That(state.Reload.Kind, Is.EqualTo(DamageReloadKind.Shell));
            Assert.That(state.Reload.TotalS, Is.EqualTo(13.5f));
            Assert.That(DamageSimulation.TickReload(state, 13.49f), Is.False);
            Assert.That(DamageSimulation.TickReload(state, 0.01f), Is.True);
            Assert.That(state.Reload.Kind, Is.EqualTo(DamageReloadKind.Ready));

            state.Ammo[0] = 0;
            Assert.That(DamageSimulation.SelectShell(state, 0, spec), Is.False);
            Assert.That(DamageSimulation.SelectFirstAvailableShell(state, spec), Is.EqualTo(1));
        }

        [Test]
        public void AutoloaderCyclesAndRefillsMagazineAtomically()
        {
            DamageTankSpec spec = MakeAutoloaderSpec();
            DamageCombatState state = DamageSimulation.CreateCombatState(spec);

            DamageSimulation.StartPostShotReload(state, spec);
            Assert.That(state.Magazine.Rounds, Is.EqualTo(2));
            Assert.That(state.Reload.Kind, Is.EqualTo(DamageReloadKind.IntraClip));
            Assert.That(DamageSimulation.TickReload(state, 2.5f), Is.True);

            DamageSimulation.StartPostShotReload(state, spec);
            DamageSimulation.TickReload(state, 2.5f);
            DamageSimulation.StartPostShotReload(state, spec);
            Assert.That(state.Magazine.Rounds, Is.Zero);
            Assert.That(state.Reload.Kind, Is.EqualTo(DamageReloadKind.Magazine));

            Assert.That(DamageSimulation.TickReload(state, 20.99f), Is.False);
            Assert.That(state.Magazine.Rounds, Is.Zero);
            Assert.That(DamageSimulation.TickReload(state, 0.01f), Is.True);
            Assert.That(state.Magazine.Rounds, Is.EqualTo(3));
            Assert.That(state.Reload.Kind, Is.EqualTo(DamageReloadKind.Ready));
        }

        [Test]
        public void GuidedLauncherReloadsAlongsideCannonMagazine()
        {
            DamageTankSpec spec = MakeAutoloaderSpec();
            spec.Gun.Shells.Add(new DamageShellSpec
            {
                Type = "HEAT",
                Count = 2,
                Guided = true,
                ReloadS = 3f
            });
            DamageCombatState state = DamageSimulation.CreateCombatState(spec);

            DamageSimulation.StartPostShotReload(state, spec);
            DamageSimulation.TickReload(state, 2.5f);
            Assert.That(DamageSimulation.SelectShell(state, 2, spec), Is.True);
            DamageReloadState launcher = state.Reload;
            Assert.That(launcher.Kind, Is.EqualTo(DamageReloadKind.Shell));
            Assert.That(DamageSimulation.StartMagazineReload(state, spec), Is.True);
            Assert.That(state.GunReload.Kind, Is.EqualTo(DamageReloadKind.Magazine));
            Assert.That(state.Reload, Is.SameAs(launcher));

            Assert.That(DamageSimulation.TickReload(state, 3f), Is.True);
            Assert.That(launcher.Kind, Is.EqualTo(DamageReloadKind.Ready));
            Assert.That(state.GunReload.RemainingS, Is.EqualTo(18f));
        }

        private static DamageTankSpec MakeSpec()
        {
            DamageTankSpec spec = new DamageTankSpec
            {
                MaxHealth = 1000f
            };
            spec.Gun.ReloadS = 6f;
            spec.Gun.Shells.Add(new DamageShellSpec { Type = "AP", Count = 8 });
            spec.Gun.Shells.Add(new DamageShellSpec { Type = "HEAT" });
            return spec;
        }

        private static DamageTankSpec MakeAutoloaderSpec()
        {
            DamageTankSpec spec = MakeSpec();
            spec.CrewIds.Add("commander");
            spec.CrewIds.Add("gunner");
            spec.CrewIds.Add("driver");
            spec.ModuleIds.Add("gun");
            spec.ModuleIds.Add("ammoRack");
            spec.ModuleIds.Add("autoloader");
            spec.Gun.Autoloader = new DamageAutoloaderSpec
            {
                MagazineSize = 3,
                IntraClipS = 2.5f,
                FullReloadS = 21f
            };
            return spec;
        }
    }
}
