using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72BUTurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddShell(turret, color);
            AddKontakt5(turret, color);
            AddShtoraAndSights(turret, color);
            AddRoofStation(turret, color);
            AddBustle(turret, color);
        }

        private static void AddShell(
            Transform turret,
            Color color)
        {
            Transform dome = Part(
                "Painted-T72BU-CastDome",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.22f, -0.02f),
                new Vector3(2.96f, 0.74f, 2.12f),
                color * 0.65f);
            dome.localRotation =
                Quaternion.Euler(0f, 0f, 0f);
            Part(
                "Painted-T72BU-TurretRingCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.02f, -0.02f),
                new Vector3(1.54f, 0.05f, 1.54f),
                color * 0.45f);
            Part(
                "Painted-T72BU-MantletTunnel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.18f, 0.98f),
                new Vector3(0.48f, 0.26f, 0.54f),
                color * 0.43f);
        }

        private static void AddKontakt5(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < 2;
                    row++)
                {
                    for (int index = 0;
                        index < 6;
                        index++)
                    {
                        Transform wedge = Part(
                            "Painted-T72BU-K5CheekWedge",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * (0.27f + index * 0.16f),
                                0.22f + row * 0.16f - index * 0.012f,
                                1.08f - index * 0.12f - row * 0.07f),
                            new Vector3(0.24f, 0.11f, 0.36f),
                            color * 0.53f);
                        wedge.localRotation =
                            Quaternion.Euler(
                                -7f,
                                side * (22f + index * 5f),
                                0f);
                    }
                }
            }
        }

        private static void AddShtoraAndSights(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T72BU-ShtoraHousing",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.52f, 0.43f, 1.3f),
                    new Vector3(0.24f, 0.14f, 0.55f),
                    color * 0.58f);
                Transform lens = Part(
                    "T72BU-ShtoraLens",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.52f, 0.43f, 1.6f),
                    new Vector3(0.088f, 0.025f, 0.088f),
                    TankT72BUFamilyDetails.ShtoraGlass());
                lens.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            Part(
                "Painted-T72BU-LunaHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.83f, 0.36f, 0.93f),
                new Vector3(0.55f, 0.17f, 0.34f),
                color * 0.6f);
            Part(
                "T72BU-LunaLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.83f, 0.37f, 1.12f),
                new Vector3(0.28f, 0.09f, 0.028f),
                TankT72BUFamilyDetails.Glass());
        }

        private static void AddRoofStation(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T72BU-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.58f, 0.57f, -0.04f),
                new Vector3(0.34f, 0.1f, 0.36f),
                color * 0.6f);
            Part(
                "Painted-T72BU-AgatSightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.44f, 0.58f, 0.5f),
                new Vector3(0.38f, 0.26f, 0.34f),
                color * 0.6f);
            Part(
                "T72BU-AgatLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.44f, 0.59f, 0.69f),
                new Vector3(0.27f, 0.12f, 0.024f),
                TankT72BUFamilyDetails.Glass());
            Part(
                "T72BU-NsvtReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.46f, 0.92f, -0.18f),
                new Vector3(0.22f, 0.16f, 0.42f),
                TankT72BUFamilyDetails.Dark());
            Part(
                "T72BU-NsvtBarrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.46f, 0.94f, 0.32f),
                new Vector3(0.04f, 0.04f, 0.7f),
                TankT72BUFamilyDetails.Dark());
        }

        private static void AddBustle(
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
                        "Painted-T72BU-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (0.78f + tube * 0.06f),
                            0.39f + (tube % 2) * 0.024f,
                            0.8f - tube * 0.07f),
                        new Vector3(0.04f, 0.12f, 0.04f),
                        color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(62f, 0f, side * 18f);
                }
                Part(
                    "T72BU-RadioWhip",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.88f, 0.94f, -0.5f),
                    new Vector3(0.012f, 0.5f, 0.012f),
                    TankT72BUFamilyDetails.Dark());
            }
            Part(
                "Painted-T72BU-BustleFloor",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.12f, -1.46f),
                new Vector3(1.48f, 0.055f, 0.51f),
                color * 0.56f);
            for (int slat = 0;
                slat < 5;
                slat++)
            {
                Part(
                    "T72BU-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(-0.64f + slat * 0.32f, 0.32f, -1.68f),
                    new Vector3(0.04f, 0.24f, 0.045f),
                    TankT72BUFamilyDetails.Dark());
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
