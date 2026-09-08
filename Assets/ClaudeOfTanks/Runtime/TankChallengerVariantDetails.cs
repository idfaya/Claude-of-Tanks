using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChallengerVariantDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            switch (definition.id)
            {
                case "fv4034":
                    AddFv4034(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "challenger2":
                    AddChallenger2Optics(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "challenger2e":
                case "ua_challenger2":
                    AddEnhancedChallenger2(
                        root,
                        turret,
                        definition,
                        color,
                        width,
                        length);
                    break;
                case "challenger_3":
                case "challenger_3x":
                    AddChallenger3Sensors(
                        turret,
                        definition,
                        color,
                        width);
                    break;
            }
        }

        private static void AddFv4034(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankChallengerFamilyDetails
                    .AddMountedMachineGun(
                        turret,
                        roof,
                        color,
                        new Vector3(
                            side * width * 0.16f,
                            0.07f,
                            -0.7f),
                        side < 0
                            ? "FV4034-MAG"
                            : "FV4034-M2",
                        side > 0);
                for (int optic = 0;
                    optic < 2;
                    optic++)
                {
                    TankDetailGeometry.Part(
                        "FV4034-Periscope",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width *
                                (0.08f +
                                 optic * 0.13f),
                            roof + 0.08f,
                            -0.28f -
                                optic * 0.16f),
                        new Vector3(
                            0.1f,
                            0.1f,
                            0.12f),
                        Sensor());
                }
            }
        }

        private static void AddChallenger2Optics(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddGps(
                turret,
                roof,
                color,
                width);
            AddSightHeads(
                turret,
                roof,
                color,
                width,
                "Challenger2");
        }

        private static void AddEnhancedChallenger2(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddSightHeads(
                turret,
                roof,
                color,
                width,
                "Challenger2E");
            for (int station = 0;
                station < 8;
                station++)
            {
                float z =
                    length * 0.34f -
                    station * length * 0.1f;
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    TankDetailGeometry.Part(
                        "Painted-Challenger2E-SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.49f,
                            definition.dims.heightM *
                                0.32f,
                            z),
                        new Vector3(
                            0.07f,
                            definition.dims.heightM *
                                0.24f,
                            length * 0.085f),
                        color * 0.76f);
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform barrel =
                    TankDetailGeometry.Part(
                        "Challenger2E-FuelBarrel",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.17f,
                            TankDetailGeometry
                                .HullRoofY(
                                    definition,
                                    1.55f) +
                            0.24f,
                            TankDetailGeometry
                                .HullRearZ(
                                    definition,
                                    -length * 0.5f) +
                            0.42f),
                        new Vector3(
                            0.24f,
                            0.5f,
                            0.24f),
                        color * 0.5f);
                barrel.localRotation =
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f);
            }
            TankChallengerFamilyDetails
                .AddMountedMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        -width * 0.15f,
                        0.07f,
                        -0.72f),
                    "Challenger2E-LoaderMAG",
                    false);
            TankChallengerFamilyDetails
                .AddMountedMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        width * 0.15f,
                        0.07f,
                        -0.62f),
                    "Challenger2E-CommanderM2",
                    true);
            TankChallengerFamilyDetails
                .AddMountedMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        0f,
                        0.04f,
                        -1.55f),
                    "Challenger2E-RearMAG",
                    false);
        }

        private static void AddChallenger3Sensors(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-Challenger3-PanoramicSight",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    -width * 0.1f,
                    roof + 0.24f,
                    -1.25f),
                new Vector3(
                    0.22f,
                    0.24f,
                    0.22f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Challenger3-PanoramicLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.1f,
                    roof + 0.25f,
                    -1.028f),
                new Vector3(
                    0.2f,
                    0.1f,
                    0.014f),
                Lens());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int end = -1;
                    end <= 1;
                    end += 2)
                {
                    TankDetailGeometry.Part(
                        "Challenger3-TrophyRadar",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.43f,
                            roof * 0.66f,
                            end > 0
                                ? 0.18f
                                : -2.45f),
                        new Vector3(
                            0.035f,
                            0.2f,
                            0.2f),
                        Sensor());
                }
            }
        }

        private static void AddGps(
            Transform turret,
            float roof,
            Color color,
            float width)
        {
            TankDetailGeometry.Part(
                "Painted-Challenger2-GPSHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.2f,
                    roof + 0.1f,
                    0.18f),
                new Vector3(
                    0.34f,
                    0.2f,
                    0.32f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Challenger2-GPSLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.2f,
                    roof + 0.11f,
                    0.348f),
                new Vector3(
                    0.2f,
                    0.09f,
                    0.014f),
                Lens());
        }

        private static void AddSightHeads(
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
                    "Painted-" + prefix +
                    "-SightHead",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.3f,
                        roof + 0.08f,
                        -0.95f),
                    new Vector3(
                        0.18f,
                        0.16f,
                        0.2f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    prefix + "-SightLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.3f,
                        roof + 0.09f,
                        -0.842f),
                    new Vector3(
                        0.1f,
                        0.07f,
                        0.014f),
                    Lens());
            }
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
