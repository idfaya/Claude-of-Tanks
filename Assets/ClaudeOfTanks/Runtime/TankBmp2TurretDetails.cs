using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp2TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddConicalBody(
                turret,
                color);
            AddCrewStations(
                turret,
                color);
            AddKonkursLauncher(
                turret,
                color);
            AddSmokeBanks(
                turret,
                color);
            AddModernizedProtection(
                turret,
                color);
            AddRoofMachineGun(
                turret,
                color);
            AddAntennas(
                turret,
                color);
        }

        private static void AddConicalBody(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp2-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.025f, 0.02f),
                new Vector3(0.93f, 0.05f, 0.79f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-ConicalTurret",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.25f, 0.01f),
                new Vector3(1.92f, 0.5f, 1.95f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-TurretBasket",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.39f, 0.113f),
                new Vector3(0.56f, 0.39f, 0.558f),
                color * 0.54f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-RearRoofRiser",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.21f, -0.65f),
                new Vector3(1.36f, 0.415f, 0.3f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-MantletBoss",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.22f, 0.86f),
                new Vector3(0.5f, 0.26f, 0.3f),
                color * 0.61f);
        }

        private static void AddCrewStations(
            Transform turret,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Bmp2-CommanderCupola",
                turret,
                0.285f,
                0.09f,
                0.38f,
                0.4f,
                -0.11f,
                color * 0.66f);
            AddVerticalCylinder(
                "Painted-Bmp2-CommanderLid",
                turret,
                0.21f,
                0.026f,
                0.46f,
                0.49f,
                -0.11f,
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-Tkn3Head",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.66f, 0.585f, 0.2f),
                new Vector3(0.13f, 0.085f, 0.16f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Bmp2-Tkn3Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.66f, 0.598f, 0.287f),
                new Vector3(0.1f, 0.03f, 0.014f),
                TankBmp2FamilyDetails.Lens());
            AddVerticalCylinder(
                "Painted-Bmp2-GunnerHatch",
                turret,
                0.235f,
                0.036f,
                -0.42f,
                0.49f,
                -0.17f,
                color * 0.66f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-BpkSightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.415f, 0.598f, -0.02f),
                new Vector3(0.29f, 0.37f, 0.3f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Bmp2-BpkSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.415f, 0.64f, 0.138f),
                new Vector3(0.12f, 0.055f, 0.016f),
                TankBmp2FamilyDetails.Lens());
            TankDetailGeometry.Part(
                "Painted-Bmp2-Ou3Spotlight",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.58f, 0.36f, 0.78f),
                new Vector3(0.16f, 0.13f, 0.22f),
                color * 0.58f);
            AddAxialCylinder(
                "Bmp2-Ou3Lens",
                turret,
                0.052f,
                0.024f,
                0.902f,
                TankBmp2FamilyDetails.Lens(),
                0.58f,
                0.38f);
        }

        private static void AddKonkursLauncher(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp2-KonkursCradle",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.085f, 0.53f, -0.305f),
                new Vector3(0.39f, 0.2f, 0.31f),
                color * 0.59f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-KonkursPedestal",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.05f, 0.5f, -0.385f),
                new Vector3(0.12f, 0.14f, 0.13f),
                color * 0.62f);
            AddAxialCylinder(
                "Bmp2-KonkursTube",
                turret,
                0.072f,
                0.66f,
                -0.175f,
                TankBmp2FamilyDetails.Dark(),
                0.05f,
                0.655f);
            AddAxialCylinder(
                "Painted-Bmp2-KonkursMuzzleRing",
                turret,
                0.1f,
                0.05f,
                0.17f,
                color * 0.48f,
                0.06f,
                0.655f);
            AddAxialCylinder(
                "Bmp2-KonkursRearCap",
                turret,
                0.076f,
                0.04f,
                -0.515f,
                TankBmp2FamilyDetails.Gunmetal(),
                0.05f,
                0.655f);
            TankDetailGeometry.Part(
                "Bmp2-KonkursIrSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.26f, 0.66f, -0.125f),
                new Vector3(0.09f, 0.09f, 0.05f),
                TankBmp2FamilyDetails.Lens());
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 3;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Bmp2-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (0.36f +
                                     tube * 0.075f),
                                0.33f +
                                    tube * 0.045f,
                                0.72f -
                                    tube * 0.035f),
                            new Vector3(0.04f, 0.11f, 0.04f),
                            color * 0.48f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            64f,
                            0f,
                            side * 17f);
                }
            }
        }

        private static void AddModernizedProtection(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 3;
                    panel++)
                {
                    Transform cheek =
                        TankDetailGeometry.Part(
                            "Painted-Bmp2-TurretCheekPanel",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side *
                                    (0.27f +
                                     panel * 0.225f),
                                0.255f -
                                    panel * 0.012f,
                                0.79f -
                                    panel * 0.085f),
                            new Vector3(0.205f, 0.145f, 0.27f),
                            color * 0.68f);
                    cheek.localRotation =
                        Quaternion.Euler(
                            0f,
                            -side *
                                (9f +
                                 panel * 7f),
                            0f);
                }
                for (int panel = 0;
                    panel < 3;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Bmp2-TurretSidePanel",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.91f,
                            0.22f,
                            0.32f - panel * 0.31f),
                        new Vector3(0.13f, 0.17f, 0.25f),
                        color * 0.65f);
                }
                TankDetailGeometry.Part(
                    "Painted-Bmp2-RearEquipmentCell",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.73f, 0.16f, -0.72f),
                    new Vector3(0.34f, 0.2f, 0.34f),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "Painted-Bmp2-LaserWarningHead",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.73f, 0.43f, 0.43f),
                    new Vector3(0.16f, 0.13f, 0.14f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Bmp2-LaserWarningLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.73f, 0.445f, 0.505f),
                    new Vector3(0.105f, 0.06f, 0.025f),
                    TankBmp2FamilyDetails.Lens());
            }
            AddVerticalCylinder(
                "Painted-Bmp2-CommanderThermalRing",
                turret,
                0.16f,
                0.08f,
                0.57f,
                0.45f,
                -0.39f,
                color * 0.61f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-CommanderThermalHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.57f, 0.58f, -0.35f),
                new Vector3(0.19f, 0.18f, 0.2f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Bmp2-CommanderThermalLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.57f, 0.6f, -0.238f),
                new Vector3(0.13f, 0.09f, 0.025f),
                TankBmp2FamilyDetails.Lens());
        }

        private static void AddRoofMachineGun(
            Transform turret,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Bmp2-RoofPktMount",
                turret,
                0.08f,
                0.04f,
                -0.42f,
                0.67f,
                0.06f,
                color * 0.55f);
            TankDetailGeometry.Part(
                "Bmp2-RoofPktReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, 0.76f, -0.04f),
                new Vector3(0.12f, 0.12f, 0.28f),
                TankBmp2FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Bmp2-RoofPktBarrel",
                turret,
                0.016f,
                0.58f,
                -0.48f,
                TankBmp2FamilyDetails.Dark(),
                -0.42f,
                0.78f);
        }

        private static void AddAntennas(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float height =
                    side < 0
                        ? 0.72f
                        : 0.6f;
                AddVerticalCylinder(
                    "Painted-Bmp2-AntennaBase",
                    turret,
                    0.07f,
                    0.08f,
                    side * 0.805f,
                    0.43f,
                    -0.51f,
                    color * 0.56f);
                Transform whip =
                    TankDetailGeometry.Part(
                        "Bmp2-RadioAntenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * 0.805f,
                            0.53f + height * 0.5f,
                            -0.51f),
                        new Vector3(0.009f, height * 0.5f, 0.009f),
                        TankBmp2FamilyDetails.Dark());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 1.7f);
            }
        }

        private static void AddVerticalCylinder(
            string name,
            Transform parent,
            float radius,
            float height,
            float x,
            float y,
            float z,
            Color color)
        {
            TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(radius, height, radius),
                color);
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x,
            float y)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
