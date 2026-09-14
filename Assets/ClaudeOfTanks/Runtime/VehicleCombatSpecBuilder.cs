using System;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    internal static class VehicleCombatSpecBuilder
    {
        public static TankSpec Build(VehicleDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            float width =
                definition.dims != null &&
                definition.dims.widthM > 0f
                    ? definition.dims.widthM
                    : 3.4f;
            VehicleGun gun = definition.gun ?? new VehicleGun();
            VehicleGunBloom bloom =
                gun.bloom ?? new VehicleGunBloom();
            ShellSpec[] shells = BuildShells(gun);
            TankArmorModel armor = BuildArmor(definition.armor);
            return new TankSpec
            {
                Id = definition.id,
                DisplayName = definition.name,
                Role = string.IsNullOrEmpty(definition.role)
                    ? "medium"
                    : definition.role,
                IsModern = definition.IsModern,
                MaxHealth = definition.hp,
                EnginePowerHp = definition.enginePowerHp,
                WeightTons = definition.weightTons,
                TopSpeedKmh = definition.topSpeedKmh,
                ReverseSpeedKmh = definition.reverseSpeedKmh,
                HullTraverseDegS = definition.hullTraverseDegS,
                TurretTraverseDegS = definition.turretTraverseDegS,
                CollisionRadiusM = width * 0.62f,
                AimTimeS = gun.aimTimeS > 0f
                    ? gun.aimTimeS
                    : 2f,
                BaseAccuracyMAt100 = gun.baseAccuracy > 0f
                    ? gun.baseAccuracy
                    : 0.36f,
                AimBloomMove = bloom.move,
                AimBloomHullRotation = bloom.hullRot,
                AimBloomTurretRotation = bloom.turret,
                AimBloomAfterShot = bloom.afterShot,
                ViewRangeM = SpottingSimulation.BaseViewRangeM(
                    definition.id,
                    definition.role),
                CamouflageStill = SpottingSimulation.BaseCamouflage(
                    definition.id,
                    definition.role,
                    false),
                CamouflageMoving = SpottingSimulation.BaseCamouflage(
                    definition.id,
                    definition.role,
                    true),
                MagazineSize =
                    gun.autoloader != null &&
                    gun.autoloader.magazineSize > 1
                        ? gun.autoloader.magazineSize
                        : 1,
                MagazineReloadS =
                    gun.autoloader != null
                        ? gun.autoloader.fullReloadS
                        : 0f,
                IntraClipS =
                    gun.autoloader != null
                        ? gun.autoloader.intraClipS
                        : 0f,
                HydropneumaticAim =
                    definition.hydropneumaticAim != null &&
                    definition.hydropneumaticAim.IsValid
                        ? definition.hydropneumaticAim.ToSpec()
                        : null,
                FixedHydraulicGun =
                    definition.armor != null &&
                    definition.armor.turretless &&
                    definition.hydropneumaticAim != null &&
                    definition.hydropneumaticAim.IsValid,
                Shell = shells[0],
                Shells = shells,
                Armor = armor
            };
        }

        private static ShellSpec[] BuildShells(VehicleGun gun)
        {
            VehicleShell[] source =
                gun.shells != null && gun.shells.Length > 0
                    ? gun.shells
                    : new[] { new VehicleShell() };
            ShellSpec[] result = new ShellSpec[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                VehicleShell shell = source[i] ?? new VehicleShell();
                result[i] = new ShellSpec
                {
                    Name = shell.name,
                    Type = shell.type,
                    CaliberMm = shell.caliberMm,
                    VelocityMps = shell.velocityMps,
                    Damage = shell.dmg,
                    Pen100Mm = shell.pen100Mm,
                    Pen1000Mm = shell.pen1000Mm,
                    Pen2000Mm = shell.pen2000Mm,
                    ReloadS = shell.reloadS > 0f
                        ? shell.reloadS
                        : gun.reloadS > 0f
                            ? gun.reloadS
                            : 5.5f,
                    Count = shell.count,
                    Guided = shell.guided
                };
            }
            return result;
        }

        private static TankArmorModel BuildArmor(
            VehicleArmor source)
        {
            if (source == null) return null;
            return new TankArmorModel
            {
                TurretPivot = Point(source.turretPivot),
                BoundingRadiusM = source.boundingRadiusM,
                HullPlates = Plates(source.hullPlates),
                TurretPlates = Plates(source.turretPlates),
                Modules = Modules(source.modules),
                Crew = Crew(source.crew)
            };
        }

        private static ArmorPlateModel[] Plates(
            ArmorPlateDefinition[] source)
        {
            if (source == null) return Array.Empty<ArmorPlateModel>();
            ArmorPlateModel[] result =
                new ArmorPlateModel[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                ArmorPlateDefinition plate = source[i];
                CatalogPoint[] vertices =
                    plate?.verts ?? Array.Empty<CatalogPoint>();
                Float3[] points = new Float3[vertices.Length];
                for (int vertex = 0; vertex < vertices.Length; vertex++)
                    points[vertex] = Point(vertices[vertex]);
                result[i] = new ArmorPlateModel
                {
                    Name = plate?.name,
                    Kind = string.IsNullOrEmpty(plate?.kind)
                        ? "main"
                        : plate.kind,
                    PhysicalMm = plate?.physicalMm ?? 0f,
                    KeMm = plate?.keMm ?? 0f,
                    CeMm = plate?.ceMm ?? 0f,
                    Vertices = points
                };
            }
            return result;
        }

        private static ArmorVolumeModel[] Modules(
            ArmorModuleDefinition[] source)
        {
            if (source == null)
                return Array.Empty<ArmorVolumeModel>();
            ArmorVolumeModel[] result =
                new ArmorVolumeModel[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                ArmorModuleDefinition volume = source[i];
                result[i] = Volume(
                    volume?.module,
                    volume?.turretLocal ?? false,
                    volume?.external ?? false,
                    volume?.min,
                    volume?.max,
                    volume?.shapes);
            }
            return result;
        }

        private static ArmorVolumeModel[] Crew(
            ArmorCrewDefinition[] source)
        {
            if (source == null)
                return Array.Empty<ArmorVolumeModel>();
            ArmorVolumeModel[] result =
                new ArmorVolumeModel[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                ArmorCrewDefinition volume = source[i];
                result[i] = Volume(
                    volume?.crew,
                    volume?.turretLocal ?? false,
                    false,
                    volume?.min,
                    volume?.max,
                    volume?.shapes);
            }
            return result;
        }

        private static ArmorVolumeModel Volume(
            string id,
            bool turretLocal,
            bool external,
            float[] minimum,
            float[] maximum,
            ArmorVolumeShapeDefinition[] shapes)
        {
            return new ArmorVolumeModel
            {
                Id = id,
                TurretLocal = turretLocal,
                External = external,
                Minimum = Point(minimum),
                Maximum = Point(maximum),
                Shapes = Shapes(shapes)
            };
        }

        private static ArmorVolumeShapeModel[] Shapes(
            ArmorVolumeShapeDefinition[] source)
        {
            if (source == null)
                return Array.Empty<ArmorVolumeShapeModel>();
            ArmorVolumeShapeModel[] result =
                new ArmorVolumeShapeModel[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                ArmorVolumeShapeDefinition shape =
                    source[i];
                result[i] =
                    new ArmorVolumeShapeModel
                    {
                        Kind = shape?.kind,
                        Center = Point(shape?.center),
                        Radii = Point(shape?.radii),
                        A = Point(shape?.a),
                        B = Point(shape?.b),
                        Radius = shape?.radius ?? 0f,
                        RadiusA = Value(
                            shape?.radii,
                            0),
                        RadiusB = Value(
                            shape?.radii,
                            1),
                        Axis = shape?.axis ?? 0,
                        HalfLength =
                            shape?.halfLength ?? 0f
                    };
            }
            return result;
        }

        private static float Value(
            float[] values,
            int index)
        {
            return values != null &&
                index >= 0 &&
                index < values.Length
                    ? values[index]
                    : 0f;
        }

        private static Float3 Point(CatalogPoint point)
        {
            return point == null
                ? Float3.Zero
                : new Float3(point.x, point.y, point.z);
        }

        private static Float3 Point(float[] point)
        {
            return point == null || point.Length < 3
                ? Float3.Zero
                : new Float3(point[0], point[1], point[2]);
        }
    }
}
