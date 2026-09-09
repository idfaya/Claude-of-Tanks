using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72B3MTurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddTurretShell(turret, color);
            AddReliktCheeks(turret, color);
            AddSightsAndCrewStations(turret, color);
            AddBustleAndSlats(turret, color);
            AddSmokeAndAntennas(turret, color);
        }

        private static void AddTurretShell(
            Transform turret,
            Color color)
        {
            Transform dome = Part(
                "Painted-T72B3M-CastDome",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.28f, -0.2f),
                new Vector3(2.42f, 0.72f, 2.05f),
                color * 0.67f);
            dome.localRotation =
                Quaternion.Euler(0f, 0f, 0f);
            Part(
                "Painted-T72B3M-TurretCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.02f, -0.2f),
                new Vector3(1.24f, 0.08f, 1.24f),
                color * 0.48f);
            Part(
                "Painted-T72B3M-MantletBlock",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.15f, 0.98f),
                new Vector3(0.72f, 0.34f, 0.42f),
                color * 0.46f);
        }

        private static void AddReliktCheeks(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int index = 0;
                    index < 5;
                    index++)
                {
                    Transform cassette = Part(
                        "Painted-T72B3M-ReliktTurretCassette",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * (0.42f + index * 0.15f),
                            0.32f + index * 0.018f,
                            0.72f - index * 0.18f),
                        new Vector3(0.36f, 0.2f, 0.14f),
                        color * 0.54f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            0f,
                            side * (26f + index * 4f),
                            0f);
                }
                for (int row = 0;
                    row < 4;
                    row++)
                {
                    Part(
                        "Painted-T72B3M-TurretSoftBag",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 1.18f,
                            0.02f + row * 0.09f,
                            -0.9f - row * 0.34f),
                        new Vector3(0.22f, 0.16f, 0.3f),
                        TankT72B3MFamilyDetails.Cloth());
                }
            }
        }

        private static void AddSightsAndCrewStations(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T72B3M-SosnaUHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.62f, 0.88f, -0.62f),
                new Vector3(0.36f, 0.42f, 0.32f),
                color * 0.62f);
            Part(
                "T72B3M-SosnaULens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.62f, 0.86f, -0.43f),
                new Vector3(0.24f, 0.18f, 0.035f),
                TankT72B3MFamilyDetails.Glass());
            Part(
                "Painted-T72B3M-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.62f, 0.77f, -0.24f),
                new Vector3(0.32f, 0.11f, 0.32f),
                color * 0.58f);
            Part(
                "Painted-T72B3M-PanoramicMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.34f, 1.02f, -0.16f),
                new Vector3(0.08f, 0.34f, 0.08f),
                color * 0.5f);
            Part(
                "T72B3M-NsvtReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.72f, 1.02f, 0.34f),
                new Vector3(0.18f, 0.15f, 0.38f),
                TankT72B3MFamilyDetails.Dark());
            Part(
                "T72B3M-NsvtBarrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.72f, 1.04f, 0.78f),
                new Vector3(0.035f, 0.035f, 0.62f),
                TankT72B3MFamilyDetails.Dark());
        }

        private static void AddBustleAndSlats(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T72B3M-RearStowageStack",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.25f, 0.2f, -1.42f),
                new Vector3(1.75f, 0.34f, 0.55f),
                color * 0.6f);
            for (int slat = 0;
                slat < 7;
                slat++)
            {
                Part(
                    "T72B3M-TurretBasketSlat",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(-0.84f + slat * 0.28f, 0.34f, -1.78f),
                    new Vector3(0.035f, 0.34f, 0.05f),
                    TankT72B3MFamilyDetails.Dark());
            }
            Part(
                "T72B3M-BustleTopRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.55f, -1.78f),
                new Vector3(1.9f, 0.035f, 0.08f),
                TankT72B3MFamilyDetails.Dark());
        }

        private static void AddSmokeAndAntennas(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 6;
                    tube++)
                {
                    Transform launcher = Part(
                        "Painted-T72B3M-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (1.1f + tube * 0.015f),
                            0.34f + tube * 0.018f,
                            0.45f - tube * 0.055f),
                        new Vector3(0.045f, 0.12f, 0.045f),
                        color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(62f, 0f, side * 20f);
                }
                Part(
                    "T72B3M-RadioWhip",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.72f, 1.05f, -1.28f),
                    new Vector3(0.012f, 0.72f, 0.012f),
                    TankT72B3MFamilyDetails.Dark());
            }
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
