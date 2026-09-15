using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class AuthoritativeParityTests
    {
        [Test]
        public void CatalogArmorPlatesDriveAuthoritativeTrace()
        {
            VehicleDefinition definition =
                ContentCatalog.Load().GetVehicle("m1a2");
            TankState tank = new TankState(
                "abrams",
                Team.Bravo,
                definition.ToTankSpec(),
                Float3.Zero,
                0f);
            ArmorPlateModel plate =
                tank.Spec.Armor.HullPlates[0];
            Float3 center = Float3.Zero;
            for (int i = 0; i < plate.Vertices.Length; i++)
                center += plate.Vertices[i];
            center /= plate.Vertices.Length;
            Float3 normal = Float3.Cross(
                plate.Vertices[1] - plate.Vertices[0],
                plate.Vertices[2] - plate.Vertices[0])
                .Normalized;
            ArmorPlateTrace[] hits =
                new ArmorPlateTrace[384];

            int count = TankArmorTrace.TracePlates(
                center + normal * 10f,
                center - normal * 10f,
                tank,
                hits);

            Assert.That(count, Is.GreaterThan(0));
            bool found = false;
            for (int i = 0; i < count; i++)
                found |= hits[i].Plate.Name == plate.Name;
            Assert.That(found, Is.True);
        }

        [Test]
        public void SelectedShellControlsAuthoritativeProjectile()
        {
            TankSpec spec = TankSpec.Medium();
            spec.Shells = new[]
            {
                spec.Shell,
                new ShellSpec
                {
                    Name = "HE test",
                    Type = "HE",
                    CaliberMm = 120f,
                    VelocityMps = 800f,
                    Damage = 500f,
                    Pen100Mm = 50f,
                    Pen1000Mm = 50f,
                    ReloadS = 0.05f,
                    Count = 5
                }
            };
            BattleState state =
                new BattleState(
                    new FlatHeightField(),
                    440u);
            TankState tank = new TankState(
                "tank",
                Team.Alpha,
                spec,
                Float3.Zero,
                0f);
            state.Tanks.Add(tank);
            BattleSimulation simulation =
                new BattleSimulation(state);
            Dictionary<string, TankInput> inputs =
                new Dictionary<string, TankInput>
                {
                    ["tank"] = new TankInput
                    {
                        ShellSlot = 1,
                        AimPoint =
                            new Float3(0f, 1f, 100f)
                    }
                };
            for (int i = 0; i < 5; i++)
                simulation.Step(
                    inputs,
                    BattleState.FixedDeltaTime);
            inputs["tank"] = new TankInput
            {
                ShellSlot = 1,
                Fire = true,
                AimPoint = new Float3(0f, 1f, 100f)
            };

            simulation.Step(
                inputs,
                BattleState.FixedDeltaTime);

            Assert.That(
                tank.Combat.ShellSlot,
                Is.EqualTo(1));
            Assert.That(
                state.Events.Exists(item =>
                    item.Type ==
                        BattleEventType.ShellFired &&
                    item.ShellType == "HE"),
                Is.True);
            Assert.That(
                tank.Combat.Ammo[1],
                Is.EqualTo(4));
        }

        [Test]
        public void RedGunBlocksFireAndDrivetrainDamageReducesMotion()
        {
            TankState healthy = new TankState(
                "healthy",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0f);
            TankState damaged = new TankState(
                "damaged",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0f);
            DamageSimulation.DamageModule(
                damaged.Combat,
                "engine",
                10000f,
                () => 1f);
            DamageSimulation.DamageModule(
                damaged.Combat,
                "trackL",
                10000f,
                () => 1f);
            DamageSimulation.KnockOutCrew(
                damaged.Combat,
                "driver");
            TankInput drive = new TankInput
            {
                Throttle = 1f,
                AimPoint = new Float3(0f, 1f, 100f)
            };
            for (int i = 0; i < 60; i++)
            {
                TankMovement.Step(
                    healthy,
                    drive,
                    new FlatHeightField(),
                    BattleState.FixedDeltaTime);
                TankMovement.Step(
                    damaged,
                    drive,
                    new FlatHeightField(),
                    BattleState.FixedDeltaTime);
            }
            Assert.That(
                damaged.Position.Z,
                Is.LessThan(healthy.Position.Z * 0.4f));

            BattleState state =
                new BattleState(
                    new FlatHeightField(),
                    441u);
            state.Tanks.Add(damaged);
            DamageSimulation.DamageModule(
                damaged.Combat,
                "gun",
                10000f,
                () => 1f);
            int ammunition =
                damaged.Combat.Ammo[0];
            new BattleSimulation(state).Step(
                new Dictionary<string, TankInput>
                {
                    [damaged.Id] =
                        new TankInput
                        {
                            Fire = true,
                            AimPoint =
                                new Float3(
                                    0f,
                                    1f,
                                    100f)
                        }
                },
                BattleState.FixedDeltaTime);
            Assert.That(
                damaged.Combat.Ammo[0],
                Is.EqualTo(ammunition));
            Assert.That(state.Shells, Is.Empty);
        }

        [Test]
        public void CatalogPreservesTsCombatControlAndReactiveArmorData()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankSpec t72 =
                catalog.GetVehicle("t72b3")
                    .ToTankSpec();
            ArmorPlateModel era = null;
            for (int i = 0;
                i < t72.Armor.HullPlates.Length;
                i++)
            {
                if (t72.Armor.HullPlates[i].Kind ==
                    "era")
                {
                    era =
                        t72.Armor.HullPlates[i];
                    break;
                }
            }
            Assert.That(era, Is.Not.Null);
            Assert.That(
                era.EraKeReduction,
                Is.EqualTo(0.2f).Within(0.00001f));
            Assert.That(
                era.EraCeFlatMm,
                Is.EqualTo(450f)
                    .Within(0.00001f));
            Assert.That(
                t72.GunPitchDegS,
                Is.EqualTo(24f));

            TankSpec mbt70 =
                catalog.GetVehicle("mbt70")
                    .ToTankSpec();
            ShellSpec guided = null;
            for (int i = 0;
                i < mbt70.Shells.Length;
                i++)
            {
                if (mbt70.Shells[i].Guided)
                {
                    guided = mbt70.Shells[i];
                    break;
                }
            }
            Assert.That(guided, Is.Not.Null);
            Assert.That(
                guided.GuidanceTurnRateRadS,
                Is.EqualTo(0.72f)
                    .Within(0.00001f));
            Assert.That(
                guided.ModuleDamage,
                Is.EqualTo(152f));
        }

        [Test]
        public void ReplayRoundTripsCompleteCombatSpecification()
        {
            VehicleDefinition definition =
                ContentCatalog.Load().GetVehicle("m1a2");
            BattleState state =
                new BattleState(
                    new FlatHeightField(),
                    442u);
            state.Tanks.Add(new TankState(
                "abrams",
                Team.Alpha,
                definition.ToTankSpec(),
                Float3.Zero,
                0f));
            BattleReplayRecorder recorder =
                new BattleReplayRecorder(
                    state,
                    GameModeId.Standard);
            recorder.Record(
                new Dictionary<string, TankInput>
                {
                    ["abrams"] =
                        new TankInput
                        {
                            ShellSlot = 1,
                            AimPoint =
                                new Float3(
                                    0f,
                                    1f,
                                    100f)
                        }
                },
                BattleState.FixedDeltaTime);
            ReplayRecording decoded =
                ReplayWireCodec.Decode(
                    ReplayWireCodec.Encode(
                        recorder.Recording));
            BattleSimulation replay =
                BattleReplayPlayer.Play(decoded);
            TankSpec restored =
                replay.State.Tanks[0].Spec;

            Assert.That(
                restored.Shells,
                Has.Length.EqualTo(3));
            Assert.That(
                restored.Shells[1].Type,
                Is.EqualTo("HEAT"));
            Assert.That(
                restored.Shells[1].ModuleDamage,
                Is.EqualTo(
                    state.Tanks[0].Spec
                        .Shells[1].ModuleDamage));
            Assert.That(
                restored.GunPitchDegS,
                Is.EqualTo(
                    state.Tanks[0].Spec
                        .GunPitchDegS));
            Assert.That(
                restored.Armor.HullPlates,
                Has.Length.EqualTo(
                    definition.armor
                        .hullPlates.Length));
            Assert.That(
                restored.Armor.Modules,
                Has.Length.EqualTo(
                    definition.armor.modules.Length));
            Assert.That(
                restored.Armor.Modules[0].Shapes,
                Has.Length.EqualTo(
                    definition.armor.modules[0]
                        .shapes.Length));
            Assert.That(
                restored.Armor.Crew,
                Has.Length.EqualTo(
                    definition.armor.crew.Length));
            Assert.That(
                replay.State.Tanks[0]
                    .Combat.ShellSlot,
                Is.EqualTo(1));
        }

        [Test]
        public void PreciseCrewShapesDriveInternalTrace()
        {
            TankState tank = new TankState(
                "tiger",
                Team.Bravo,
                ContentCatalog.Load()
                    .GetVehicle("tiger1")
                    .ToTankSpec(),
                Float3.Zero,
                0f);
            ArmorVolumeModel driver = null;
            for (int i = 0;
                i < tank.Spec.Armor.Crew.Length;
                i++)
            {
                if (tank.Spec.Armor.Crew[i].Id ==
                    "driver")
                {
                    driver =
                        tank.Spec.Armor.Crew[i];
                    break;
                }
            }
            Assert.That(driver, Is.Not.Null);
            Assert.That(
                driver.Shapes,
                Is.Not.Empty);
            Float3 center =
                driver.Shapes[0].Center;
            ArmorVolumeTrace[] hits =
                new ArmorVolumeTrace[16];

            int count = TankArmorTrace.TraceVolumes(
                center + new Float3(-10f, 0f, 0f),
                center + new Float3(10f, 0f, 0f),
                tank,
                true,
                hits);

            Assert.That(count, Is.GreaterThan(0));
            bool found = false;
            for (int i = 0; i < count; i++)
                found |= hits[i].Volume.Id == "driver";
            Assert.That(found, Is.True);
        }

        [Test]
        public void NetworkDamageMaskCarriesEveryCatalogCrewRole()
        {
            VehicleDefinition definition =
                ContentCatalog.Load().GetVehicle(
                    "tiger1");
            TankState source = new TankState(
                "source",
                Team.Alpha,
                definition.ToTankSpec(),
                Float3.Zero,
                0f);
            Assert.That(
                source.Combat.Crew.ContainsKey(
                    "radioOperator"),
                Is.True);
            DamageSimulation.KnockOutCrew(
                source.Combat,
                "radioOperator");
            NetworkEntitySnapshot snapshot =
                new NetworkEntitySnapshot();
            NetworkDamageState.CaptureInto(
                source.Combat,
                snapshot);
            TankState restored = new TankState(
                "restored",
                Team.Alpha,
                definition.ToTankSpec(),
                Float3.Zero,
                0f);

            NetworkDamageState.Apply(
                restored.Combat,
                snapshot);

            Assert.That(
                restored.Combat.Crew[
                    "radioOperator"],
                Is.False);
        }

        [Test]
        public void CatalogCrewReplacesGenericCrewTemplate()
        {
            TankState tank = new TankState(
                "t90",
                Team.Alpha,
                ContentCatalog.Load()
                    .GetVehicle("t90m")
                    .ToTankSpec(),
                Float3.Zero,
                0f);

            Assert.That(
                tank.Combat.Crew.Keys,
                Is.EquivalentTo(
                    new[]
                    {
                        "driver",
                        "gunner",
                        "commander"
                    }));
            Assert.That(
                tank.Combat.Crew.ContainsKey("loader"),
                Is.False);
        }
    }
}
