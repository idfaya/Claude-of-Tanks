using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPumaTurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            bool s1 =
                TankPumaFamilyDetails.IsS1(
                    definition);
            float scale = s1 ? 0.9f : 1f;
            AddUnmannedCore(
                turret,
                color,
                scale,
                s1);
            AddFrontElectronics(
                turret,
                color,
                scale,
                s1);
            AddProtectionSuite(
                turret,
                color,
                scale,
                s1);
            AddBustleEquipment(
                turret,
                color,
                scale,
                s1);
        }

        private static void AddUnmannedCore(
            Transform turret,
            Color color,
            float scale,
            bool s1)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-Rct30LowerCore",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    0.22f * scale,
                    -0.2f * scale),
                new Vector3(
                    1.78f * scale,
                    0.38f * scale,
                    2.36f * scale),
                color * 0.69f);
            Transform roof =
                TankDetailGeometry.Part(
                    "Painted-Puma-Rct30Roof",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -0.04f * scale,
                        0.65f * scale,
                        -0.2f * scale),
                    new Vector3(
                        1.3f * scale,
                        0.18f * scale,
                        2.22f * scale),
                    color * 0.78f);
            roof.localRotation =
                Quaternion.Euler(
                    -2.5f,
                    0f,
                    0f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform cheek =
                    TankDetailGeometry.Part(
                        "Painted-Puma-Rct30Cheek",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.65f * scale,
                            0.39f * scale,
                            0.72f * scale),
                        new Vector3(
                            0.54f * scale,
                            0.5f * scale,
                            1.18f * scale),
                        color * 0.74f);
                cheek.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 20f,
                        side * -8f);
            }
            TankDetailGeometry.Part(
                "Painted-Puma-Rct30Bustle",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.04f * scale,
                    0.45f * scale,
                    -1.29f * scale),
                new Vector3(
                    1.46f * scale,
                    0.52f * scale,
                    0.54f * scale),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Puma-Rct30BustleVent",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.04f * scale,
                    0.49f * scale,
                    -1.575f * scale),
                new Vector3(
                    0.72f * scale,
                    0.22f * scale,
                    0.025f * scale),
                TankPumaFamilyDetails.Dark());
            if (s1)
            {
                TankDetailGeometry.Part(
                    "Painted-PumaS1-PlanarCrown",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0.76f * scale,
                        -0.12f * scale),
                    new Vector3(
                        0.92f * scale,
                        0.055f * scale,
                        1.5f * scale),
                    color * 0.82f);
            }
        }

        private static void AddFrontElectronics(
            Transform turret,
            Color color,
            float scale,
            bool s1)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform shoulder =
                    TankDetailGeometry.Part(
                        "Painted-Puma-FrontElectronicsShoulder",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.62f * scale,
                            0.43f * scale,
                            1.18f * scale),
                        new Vector3(
                            0.46f * scale,
                            0.4f * scale,
                            0.62f * scale),
                        color * 0.6f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 10f,
                        side * -4f);
                TankDetailGeometry.Part(
                    "Puma-GunSideOpticLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.5f * scale,
                        0.5f * scale,
                        1.515f * scale),
                    new Vector3(
                        0.17f * scale,
                        0.105f * scale,
                        0.014f * scale),
                    TankPumaFamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "Puma-GunSideAuxLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.64f * scale,
                        0.36f * scale,
                        1.475f * scale),
                    new Vector3(
                        0.065f * scale,
                        0.052f * scale,
                        0.014f * scale),
                    TankPumaFamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "Painted-Puma-FlankMissionBox",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.01f * scale,
                        0.47f * scale,
                        -0.5f * scale),
                    new Vector3(
                        0.2f * scale,
                        0.44f * scale,
                        0.72f * scale),
                    color * 0.58f);
                for (int louvre = 0;
                    louvre < 4;
                    louvre++)
                {
                    TankDetailGeometry.Part(
                        "Puma-FlankMissionLouvre",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 1.12f * scale,
                            (0.35f +
                             louvre * 0.082f) *
                            scale,
                            -0.5f * scale),
                        new Vector3(
                            0.022f * scale,
                            0.03f * scale,
                            0.48f * scale),
                        TankPumaFamilyDetails.Dark());
                }
                if (s1)
                {
                    TankDetailGeometry.Part(
                        "Painted-PumaS1-RoofElectronicsBox",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.58f * scale,
                            0.81f * scale,
                            -0.05f * scale),
                        new Vector3(
                            0.4f * scale,
                            0.12f * scale,
                            0.34f * scale),
                        color * 0.63f);
                }
            }
        }

        private static void AddProtectionSuite(
            Transform turret,
            Color color,
            float scale,
            bool s1)
        {
            int smokePerSide = s1 ? 6 : 4;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Puma-SmokeBankBracket",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.94f * scale,
                        0.35f * scale,
                        0.23f * scale),
                    new Vector3(
                        0.18f * scale,
                        0.14f * scale,
                        0.42f * scale),
                    color * 0.55f);
                for (int tube = 0;
                    tube < smokePerSide;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Puma-RosyLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (0.92f +
                                     tube * 0.025f) *
                                    scale,
                                (0.43f +
                                 tube * 0.025f) *
                                    scale,
                                (0.43f -
                                 tube * 0.065f) *
                                    scale),
                            new Vector3(
                                0.04f * scale,
                                0.135f * scale,
                                0.04f * scale),
                            color * 0.5f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            68f,
                            side * 35f,
                            side * 12f);
                }
            }
            if (!s1)
            {
                Vector3[] heads =
                {
                    new Vector3(-0.72f, 0.71f, 0.3f),
                    new Vector3(0.6f, 0.72f, 0.02f),
                    new Vector3(-0.66f, 0.76f, -0.92f),
                    new Vector3(0.56f, 0.76f, -0.92f)
                };
                for (int head = 0;
                    head < heads.Length;
                    head++)
                {
                    TankDetailGeometry.Part(
                        "Puma-MussSensorHead",
                        PrimitiveType.Cylinder,
                        turret,
                        heads[head],
                        new Vector3(0.045f, 0.045f, 0.045f),
                        TankPumaFamilyDetails.Gunmetal());
                    TankDetailGeometry.Part(
                        "Puma-MussSensorCap",
                        PrimitiveType.Cylinder,
                        turret,
                        heads[head] +
                            Vector3.up * 0.05f,
                        new Vector3(0.04f, 0.015f, 0.04f),
                        TankPumaFamilyDetails.Dark());
                }
            }
            else
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    for (int station = 0;
                        station < 2;
                        station++)
                    {
                        float z =
                            station == 0
                                ? 1.15f
                                : -0.72f;
                        TankDetailGeometry.Part(
                            "Painted-PumaS1-AllAroundCamera",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * 0.88f * scale,
                                0.69f * scale,
                                z * scale),
                            new Vector3(
                                0.18f * scale,
                                0.16f * scale,
                                0.18f * scale),
                            color * 0.54f);
                        TankDetailGeometry.Part(
                            "PumaS1-AllAroundCameraLens",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * 0.985f * scale,
                                0.7f * scale,
                                z * scale),
                            new Vector3(
                                0.014f * scale,
                                0.09f * scale,
                                0.1f * scale),
                            TankPumaFamilyDetails.Lens());
                    }
                }
            }
        }

        private static void AddBustleEquipment(
            Transform turret,
            Color color,
            float scale,
            bool s1)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Puma-AntennaCollar",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * 0.72f * scale,
                        0.78f * scale,
                        -1.12f * scale),
                    new Vector3(
                        0.05f * scale,
                        0.035f * scale,
                        0.05f * scale),
                    color * 0.52f);
                Transform whip =
                    TankDetailGeometry.Part(
                        "Puma-AntennaWhip",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * 0.72f * scale,
                            (1.18f +
                             (side < 0
                                 ? 0.08f
                                 : 0f)) *
                            scale,
                            -1.12f * scale),
                        new Vector3(
                            0.01f * scale,
                            (s1
                                ? 0.36f
                                : 0.42f) *
                            scale,
                            0.01f * scale),
                        TankPumaFamilyDetails.Dark());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 2f);
            }
            TankDetailGeometry.Part(
                "Painted-Puma-BustleRack",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    0.38f * scale,
                    -1.63f * scale),
                new Vector3(
                    1.25f * scale,
                    0.18f * scale,
                    0.34f * scale),
                color * 0.5f);
        }

    }
}
