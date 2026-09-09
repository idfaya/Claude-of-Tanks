using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT64BV1HullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddHullShell(root, color);
            AddFendersAndSkirts(root, color);
            AddDriverStation(root, color);
            AddDeckAndStowage(root, color);
            AddBowAndRear(root, color);
        }

        private static void AddHullShell(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T64-LowerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.76f, -0.2f),
                new Vector3(2.2f, 0.72f, 4.8f),
                color * 0.5f);
            Part(
                "Painted-T64-Sponson",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.2f, -0.25f),
                new Vector3(3.1f, 0.36f, 4.8f),
                color * 0.72f);
            Part(
                "Painted-T64-Deck",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.43f, -0.45f),
                new Vector3(2.8f, 0.1f, 4.75f),
                color * 0.78f);

            Transform glacis = Part(
                "Painted-T64-UpperGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.27f, 1.96f),
                new Vector3(2.95f, 0.12f, 1.25f),
                color * 0.7f);
            glacis.localRotation =
                Quaternion.Euler(-22f, 0f, 0f);

            Transform lower = Part(
                "Painted-T64-LowerGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.79f, 2.48f),
                new Vector3(2.3f, 0.1f, 0.95f),
                color * 0.53f);
            lower.localRotation =
                Quaternion.Euler(35f, 0f, 0f);

            Part(
                "Painted-T64-Transom",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.92f, -2.94f),
                new Vector3(2.5f, 0.72f, 0.16f),
                color * 0.57f);
        }

        private static void AddFendersAndSkirts(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T64-FenderRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.505f,
                        1.365f,
                        0.05f),
                    new Vector3(
                        0.15f,
                        0.08f,
                        4.5f),
                    color * 0.7f);

                for (int panel = 0;
                    panel < 8;
                    panel++)
                {
                    float z = -2.0f + panel * 0.57f;
                    Part(
                        "Painted-T64-SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.66f,
                            0.91f,
                            z),
                        new Vector3(
                            0.05f,
                            0.22f,
                            0.53f),
                        TankT64BV1FamilyDetails.Rubber());
                    Part(
                        "T64-SkirtHinge",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.69f,
                            1.025f,
                            z),
                        new Vector3(
                            0.035f,
                            0.025f,
                            0.38f),
                        TankT64BV1FamilyDetails.Dark());
                }

                for (int bin = 0;
                    bin < 8;
                    bin++)
                {
                    float z = 2.02f - bin * 0.6f;
                    Part(
                        "Painted-T64-FenderBin",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.245f,
                            1.46f,
                            z),
                        new Vector3(
                            0.4f,
                            0.115f,
                            0.56f),
                        color * 0.72f);
                    Part(
                        "T64-FenderBinLid",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.245f,
                            1.525f,
                            z),
                        new Vector3(
                            0.34f,
                            0.028f,
                            0.46f),
                        TankT64BV1FamilyDetails.Dark());
                }
            }
        }

        private static void AddDriverStation(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T64-DriverHatch",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, 1.535f, 1.1f),
                new Vector3(0.235f, 0.042f, 0.235f),
                color * 0.74f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T64-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.15f,
                        1.57f,
                        1.4f),
                    new Vector3(
                        0.13f,
                        0.07f,
                        0.1f),
                    color * 0.66f);
                Part(
                    "T64-DriverPeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.15f,
                        1.585f,
                        1.455f),
                    new Vector3(
                        0.09f,
                        0.04f,
                        0.02f),
                    TankT64BV1FamilyDetails.Glass());
            }
            Part(
                "Painted-T64-SplashBoard",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.48f, 1.5f),
                new Vector3(0.62f, 0.06f, 0.055f),
                color * 0.62f);
        }

        private static void AddDeckAndStowage(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "T64-EngineGrilleBed",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.62f,
                        1.53f,
                        -2.1f),
                    new Vector3(
                        1.02f,
                        0.035f,
                        1.02f),
                    TankT64BV1FamilyDetails.Dark());
                for (int louvre = 0;
                    louvre < 7;
                    louvre++)
                {
                    Part(
                        "Painted-T64-EngineLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.62f,
                            1.56f,
                            -2.52f +
                                louvre * 0.14f),
                        new Vector3(
                            0.92f,
                            0.025f,
                            0.048f),
                        color * 0.48f);
                }
            }

            Part(
                "Painted-T64-LeftExhaust",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.315f, 1.5f, -2.1f),
                new Vector3(0.24f, 0.17f, 0.92f),
                color * 0.52f);
            Part(
                "T64-LeftExhaustOutlet",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.415f, 1.445f, -2.1f),
                new Vector3(0.16f, 0.05f, 0.42f),
                TankT64BV1FamilyDetails.Dark());

            for (int link = 0;
                link < 5;
                link++)
            {
                Part(
                    "Painted-T64-SpareTrackLink",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -1.08f + link * 0.23f,
                        1.55f,
                        0.62f),
                    new Vector3(
                        0.2f,
                        0.055f,
                        0.42f),
                    color * 0.45f);
            }
        }

        private static void AddBowAndRear(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T64-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.13f,
                        1.25f,
                        2.42f),
                    new Vector3(
                        0.2f,
                        0.18f,
                        0.16f),
                    color * 0.58f);
                Part(
                    "T64-HeadlightLens",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 1.13f,
                        1.25f,
                        2.51f),
                    new Vector3(
                        0.09f,
                        0.09f,
                        0.05f),
                    new Color(0.55f, 0.58f, 0.42f));
                Part(
                    "T64-FrontMudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.72f,
                        0.88f,
                        2.91f),
                    new Vector3(
                        0.16f,
                        0.4f,
                        0.04f),
                    TankT64BV1FamilyDetails.Rubber());
                Part(
                    "T64-RearMudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.73f,
                        0.82f,
                        -2.96f),
                    new Vector3(
                        0.16f,
                        0.3f,
                        0.04f),
                    TankT64BV1FamilyDetails.Rubber());
                Part(
                    "T64-RecoveryEye",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * 0.82f,
                        0.7f,
                        -3.04f),
                    new Vector3(
                        0.08f,
                        0.04f,
                        0.08f),
                    TankT64BV1FamilyDetails.Dark())
                    .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }

            for (int louvre = 0;
                louvre < 6;
                louvre++)
            {
                Part(
                    "Painted-T64-RearLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.75f + louvre * 0.3f,
                        1.1f,
                        -3.035f),
                    new Vector3(
                        0.24f,
                        0.13f,
                        0.025f),
                    color * 0.5f);
            }

            Transform log = Part(
                "T64-UnditchingLog",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, 0.7f, -3.02f),
                new Vector3(0.1f, 0.9f, 0.1f),
                TankT64BV1FamilyDetails.Wood());
            log.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            for (int strap = -1;
                strap <= 1;
                strap += 2)
            {
                Part(
                    "T64-LogStrap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        strap * 0.48f,
                        0.7f,
                        -3.08f),
                    new Vector3(
                        0.055f,
                        0.24f,
                        0.035f),
                    TankT64BV1FamilyDetails.Dark());
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
