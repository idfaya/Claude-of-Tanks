using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChallengerFamilyDetails
    {
        public static bool Supports(string id)
        {
            switch (id)
            {
                case "fv4034":
                case "challenger2":
                case "challenger2e":
                case "ua_challenger2":
                case "challenger_3":
                case "challenger_3x":
                    return true;
                default:
                    return false;
            }
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (!Supports(definition?.id)) return;

            AddEngineDeck(
                root,
                definition,
                color,
                width,
                length);
            AddBustleRack(
                turret,
                definition,
                color,
                width);
            AddAntennas(
                turret,
                definition,
                width);

            if (IsChallenger2(definition.id))
            {
                AddChallenger2Roof(
                    turret,
                    definition,
                    color,
                    width);
            }
            else
            {
                AddChallenger3Roof(
                    turret,
                    definition,
                    color,
                    width);
            }

            TankChallengerVariantDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankChallengerProtectionDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                length);
        }

        internal static void AddRemoteWeaponStation(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            Vector3 seat,
            string prefix)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.2f, 0.06f, 0.2f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.15f,
                    seat.z + 0.14f),
                new Vector3(0.38f, 0.2f, 0.5f),
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.17f,
                    seat.z + 0.72f),
                new Vector3(0.045f, 0.045f, 0.85f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Sensor",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x - 0.26f,
                    roof + seat.y + 0.2f,
                    seat.z + 0.05f),
                new Vector3(0.16f, 0.2f, 0.17f),
                Sensor());
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x - 0.26f,
                    roof + seat.y + 0.2f,
                    seat.z + 0.142f),
                new Vector3(0.1f, 0.09f, 0.014f),
                Lens());
        }

        internal static void AddMountedMachineGun(
            Transform turret,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool heavy)
        {
            float receiverWidth = heavy ? 0.2f : 0.15f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.12f, 0.04f, 0.12f),
                color * 0.7f);
            TankDetailGeometry.Part(
                prefix + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.12f,
                    seat.z + 0.1f),
                new Vector3(
                    receiverWidth,
                    heavy ? 0.13f : 0.1f,
                    heavy ? 0.38f : 0.3f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.13f,
                    seat.z + (heavy ? 0.55f : 0.46f)),
                new Vector3(
                    heavy ? 0.035f : 0.026f,
                    heavy ? 0.035f : 0.026f,
                    heavy ? 0.75f : 0.62f),
                Gunmetal());
        }

        private static bool IsChallenger2(string id)
        {
            return id == "fv4034" ||
                id == "challenger2" ||
                id == "challenger2e" ||
                id == "ua_challenger2";
        }

        private static void AddEngineDeck(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    definition.dims.heightM * 0.58f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Challenger-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.18f,
                        roof + 0.022f,
                        rear + length * 0.2f),
                    new Vector3(
                        width * 0.3f,
                        0.03f,
                        length * 0.23f),
                    color * 0.35f);
            }
        }

        private static void AddBustleRack(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition) - 0.05f;
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            Color rail = color * 0.5f;
            for (int row = 0;
                row < 3;
                row++)
            {
                TankDetailGeometry.Part(
                    "Challenger-BustleRackBar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof * (0.2f + row * 0.2f),
                        rear),
                    new Vector3(
                        width * 0.72f,
                        0.035f,
                        0.035f),
                    rail);
            }
            for (int bar = 0;
                bar < 12;
                bar++)
            {
                TankDetailGeometry.Part(
                    "Challenger-BustleRackBar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * (-0.33f +
                            bar * 0.06f),
                        roof * 0.4f,
                        rear),
                    new Vector3(
                        0.025f,
                        roof * 0.42f,
                        0.025f),
                    rail);
            }
        }

        private static void AddAntennas(
            Transform turret,
            VehicleDefinition definition,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform antenna =
                    TankDetailGeometry.Part(
                        "Challenger-Antenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width * 0.25f,
                            roof + 0.62f,
                            rear + 0.58f),
                        new Vector3(
                            0.012f,
                            0.62f,
                            0.012f),
                        Gunmetal());
                antenna.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 2f);
            }
        }

        private static void AddChallenger2Roof(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddCrewRoof(
                turret,
                roof,
                color,
                width,
                "Challenger2");
            int smokeCount = definition.id == "fv4034" ||
                definition.id == "challenger2e" ||
                definition.id == "ua_challenger2"
                    ? 8
                    : 10;
            AddSmokeBanks(
                turret,
                roof,
                color,
                width,
                smokeCount,
                "Challenger2");

            if (definition.id != "fv4034")
            {
                AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    definition.id == "challenger2"
                        ? new Vector3(
                            width * 0.2f,
                            0.05f,
                            0.15f)
                        : new Vector3(
                            0f,
                            0.05f,
                            0.2f),
                    "Challenger2-RWS");
            }
        }

        private static void AddChallenger3Roof(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddCrewRoof(
                turret,
                roof,
                color,
                width,
                "Challenger3");
            AddSmokeBanks(
                turret,
                roof,
                color,
                width,
                10,
                "Challenger3");
            AddRemoteWeaponStation(
                turret,
                definition,
                color,
                new Vector3(
                    width * 0.18f,
                    0.04f,
                    -0.22f),
                "Challenger3-Protector");
        }

        private static void AddCrewRoof(
            Transform turret,
            float roof,
            Color color,
            float width,
            string prefix)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.15f,
                        roof + 0.025f,
                        -0.86f),
                    new Vector3(
                        width * 0.11f,
                        0.03f,
                        width * (side < 0
                            ? 0.14f
                            : 0.1f)),
                    color * 0.82f);
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            float roof,
            Color color,
            float width,
            int count,
            string prefix)
        {
            int perSide = count / 2;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < perSide;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-" + prefix +
                            "-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * width *
                                    (0.27f +
                                     tube * 0.015f),
                                roof * 0.44f +
                                    tube * 0.025f,
                                0.72f -
                                    tube * 0.08f),
                            new Vector3(
                                0.05f,
                                0.14f,
                                0.05f),
                            color * 0.58f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            64f,
                            0f,
                            side * 28f);
                }
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Sensor()
        {
            return new Color(0.11f, 0.12f, 0.1f);
        }

        private static Color Lens()
        {
            return new Color(0.04f, 0.18f, 0.2f);
        }
    }
}
