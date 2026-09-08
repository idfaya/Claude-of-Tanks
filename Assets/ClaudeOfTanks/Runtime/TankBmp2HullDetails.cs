using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp2HullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddBoatHull(
                root,
                color);
            AddBow(
                root,
                color);
            AddFenders(
                root,
                color);
            AddDeck(
                root,
                color);
            AddPassengerCompartment(
                root,
                color);
            AddModernizedProtection(
                root,
                color);
            AddRearDoors(
                root,
                color);
        }

        private static void AddBoatHull(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp2-CenterTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.025f, -0.534f),
                new Vector3(2.08f, 1.23f, 4.58f),
                color * 0.66f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp2-Sponson",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.135f,
                        1.45f,
                        -0.669f),
                    new Vector3(0.33f, 0.36f, 4.89f),
                    color * 0.73f);
            }
            TankDetailGeometry.Part(
                "Painted-Bmp2-RoofPlate",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.5965f, -0.634f),
                new Vector3(2.6f, 0.065f, 4.78f),
                color * 0.78f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-SternBody",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.28f, -2.99f),
                new Vector3(2.58f, 0.62f, 0.62f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-BellyTail",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.53f, -2.91f),
                new Vector3(2.02f, 0.3f, 0.48f),
                color * 0.57f);
        }

        private static void AddBow(
            Transform root,
            Color color)
        {
            Transform glacis =
                TankDetailGeometry.Part(
                    "Painted-Bmp2-UpperGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.42f, 2.48f),
                    new Vector3(2.18f, 0.11f, 1.78f),
                    color * 0.81f);
            glacis.localRotation =
                Quaternion.Euler(12.7f, 0f, 0f);
            Transform lower =
                TankDetailGeometry.Part(
                    "Painted-Bmp2-LowerProw",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 0.77f, 2.54f),
                    new Vector3(2.02f, 0.12f, 1.77f),
                    color * 0.61f);
            lower.localRotation =
                Quaternion.Euler(-34.7f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-NoseLip",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.17f, 3.245f),
                new Vector3(2.13f, 0.34f, 0.24f),
                color * 0.68f);
            Transform trimVane =
                TankDetailGeometry.Part(
                    "Painted-Bmp2-TrimVane",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.405f, 3.06f),
                    new Vector3(2.04f, 0.055f, 0.085f),
                    color * 0.56f);
            trimVane.localRotation =
                Quaternion.Euler(-11.5f, 0f, 0f);
            for (int rib = 0;
                rib < 6;
                rib++)
            {
                Transform waveRib =
                    TankDetailGeometry.Part(
                        "Bmp2-WaveBreakerRib",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            0f,
                            1.54f - rib * 0.032f,
                            2.03f + rib * 0.15f),
                        new Vector3(2f, 0.026f, 0.065f),
                        TankBmp2FamilyDetails.Gunmetal());
                waveRib.localRotation =
                    Quaternion.Euler(-12.7f, 0f, 0f);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform shoulder =
                    TankDetailGeometry.Part(
                        "Painted-Bmp2-BowShoulder",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.29f,
                            1.23f,
                            3.08f),
                        new Vector3(0.5f, 0.24f, 0.5f),
                        color * 0.72f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        7f,
                        side * 35f,
                        0f);
            }
        }

        private static void AddFenders(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp2-FrontFender",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.42f, 1.23f, 2.44f),
                    new Vector3(0.21f, 0.055f, 1.16f),
                    color * 0.65f);
                TankDetailGeometry.Part(
                    "Painted-Bmp2-RearFender",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.43f, 1.23f, -2.685f),
                    new Vector3(0.23f, 0.055f, 1.14f),
                    color * 0.64f);
                TankDetailGeometry.Part(
                    "Bmp2-RubberMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.475f, 0.955f, 2.2f),
                    new Vector3(0.15f, 0.56f, 0.6f),
                    TankBmp2FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Bmp2-RubberMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.475f, 0.955f, -2.4f),
                    new Vector3(0.15f, 0.56f, 1.4f),
                    TankBmp2FamilyDetails.Dark());
                for (int rail = 0;
                    rail < 4;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Bmp2-RearFenderRail",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.535f,
                            0.955f,
                            -2.86f + rail * 0.42f),
                        new Vector3(0.03f, 0.21f, 0.38f),
                        TankBmp2FamilyDetails.Gunmetal());
                }
            }
        }

        private static void AddDeck(
            Transform root,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Bmp2-DriverHatch",
                root,
                0.24f,
                0.024f,
                -0.62f,
                1.626f,
                1.576f,
                color * 0.66f);
            AddVerticalCylinder(
                "Painted-Bmp2-InfantryHatch",
                root,
                0.22f,
                0.024f,
                -0.62f,
                1.626f,
                0.736f,
                color * 0.66f);
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Bmp2-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.84f + scope * 0.21f,
                        1.65f,
                        1.536f),
                    new Vector3(0.13f, 0.055f, 0.08f),
                    TankBmp2FamilyDetails.Lens());
            }
            TankDetailGeometry.Part(
                "Painted-Bmp2-EngineGrille",
                PrimitiveType.Cube,
                root,
                new Vector3(0.66f, 1.632f, 1.136f),
                new Vector3(0.9f, 0.02f, 1.1f),
                color * 0.48f);
            for (int louvre = 0;
                louvre < 5;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Bmp2-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0.66f,
                        1.647f,
                        1.556f - louvre * 0.21f),
                    new Vector3(0.82f, 0.024f, 0.055f),
                    TankBmp2FamilyDetails.Dark());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp2-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.05f, 1.405f, 2.45f),
                    new Vector3(0.3f, 0.16f, 0.15f),
                    color * 0.63f);
                TankDetailGeometry.Part(
                    "Bmp2-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(side * 1.05f, 1.43f, 2.535f),
                    new Vector3(0.1f, 0.1f, 0.07f),
                    TankBmp2FamilyDetails.Lens());
            }
        }

        private static void AddPassengerCompartment(
            Transform root,
            Color color)
        {
            AddPorts(
                root,
                -1,
                new[]
                {
                    -0.504f,
                    -1.144f,
                    -1.784f,
                    -2.414f
                });
            AddPorts(
                root,
                1,
                new[]
                {
                    -0.824f,
                    -1.464f,
                    -2.104f
                });
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp2-TroopHatch",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.105f,
                        1.674f,
                        -2.034f),
                    new Vector3(0.14f, 0.024f, 0.24f),
                    color * 0.64f);
            }
        }

        private static void AddPorts(
            Transform root,
            int side,
            float[] stations)
        {
            for (int port = 0;
                port < stations.Length;
                port++)
            {
                TankDetailGeometry.Part(
                    "Bmp2-FiringPort",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 1.305f,
                        1.4f,
                        stations[port]),
                    new Vector3(0.04f, 0.07f, 0.07f),
                    TankBmp2FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Bmp2-FiringPortVisionBlock",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.307f,
                        1.525f,
                        stations[port] + 0.1f),
                    new Vector3(0.025f, 0.045f, 0.08f),
                    TankBmp2FamilyDetails.Lens());
            }
        }

        private static void AddModernizedProtection(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 7;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Bmp2-SideCassette",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.385f,
                            1.31f,
                            1.36f - panel * 0.58f),
                        new Vector3(
                            0.145f,
                            0.3f,
                            panel == 0 || panel == 6
                                ? 0.48f
                                : 0.52f),
                        color * (0.69f -
                            panel % 2 * 0.025f));
                }
            }
            for (int row = 0;
                row < 2;
                row++)
            {
                for (int panel = 0;
                    panel < 4;
                    panel++)
                {
                    Transform cassette =
                        TankDetailGeometry.Part(
                            "Painted-Bmp2-GlacisCassette",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                -0.75f + panel * 0.5f,
                                row == 0
                                    ? 1.505f
                                    : 1.415f,
                                row == 0
                                    ? 2.18f
                                    : 2.52f),
                            new Vector3(0.4f, 0.115f, 0.31f),
                            color * 0.7f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            -14.6f,
                            0f,
                            0f);
                }
            }
        }

        private static void AddRearDoors(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp2-RearDoor",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.36f, 1.345f, -3.3f),
                    new Vector3(0.7f, 0.42f, 0.05f),
                    color * 0.67f);
                TankDetailGeometry.Part(
                    "Painted-Bmp2-RearDoorBulge",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(side * 0.36f, 1.32f, -3.33f),
                    new Vector3(0.52f, 0.42f, 0.08f),
                    color * 0.61f);
                TankDetailGeometry.Part(
                    "Bmp2-RearDoorHinge",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.745f, 1.34f, -3.31f),
                    new Vector3(0.04f, 0.4f, 0.055f),
                    TankBmp2FamilyDetails.Gunmetal());
                TankDetailGeometry.Part(
                    "Bmp2-Taillight",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.93f, 1.4f, -3.345f),
                    new Vector3(0.15f, 0.07f, 0.04f),
                    new Color(0.35f, 0.035f, 0.025f));
            }
            TankDetailGeometry.Part(
                "Bmp2-RearDoorSeam",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.345f, -3.336f),
                new Vector3(0.03f, 0.4f, 0.02f),
                TankBmp2FamilyDetails.Dark());
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
    }
}
