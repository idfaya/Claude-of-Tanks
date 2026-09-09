using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90HullDetails
    {
        private static readonly float[] WheelStations =
        {
            -1.9f, -1.12f, -0.34f,
            0.44f, 1.22f, 2.0f
        };

        private static readonly float[] RollerStations =
        {
            -1.38f, 0.14f, 1.65f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            AddHull(root, color);
            AddKontakt5(root, color);
            AddSkirtsAndCage(root, color);
            AddRunningGear(root, color);
            AddDeckAndRear(root, color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T90-LowerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.76f, -0.06f),
                new Vector3(2.28f, 0.74f, 5.82f),
                color * 0.47f);
            Part(
                "Painted-T90-UpperHull",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.16f, -0.08f),
                new Vector3(3.26f, 0.34f, 5.86f),
                color * 0.67f);
            Transform glacis = Part(
                "Painted-T90-SweptGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.08f, 2.44f),
                new Vector3(2.92f, 0.12f, 0.72f),
                color * 0.62f);
            glacis.localRotation =
                Quaternion.Euler(-20f, 0f, 0f);
        }

        private static void AddKontakt5(
            Transform root,
            Color color)
        {
            for (int row = 0;
                row < 2;
                row++)
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    for (int column = 0;
                        column < 5;
                        column++)
                    {
                        Transform brick = Part(
                            "Painted-T90-K5GlacisBrick",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * (0.24f + column * 0.27f),
                                1.21f + row * 0.14f,
                                2.62f - row * 0.5f),
                            new Vector3(0.22f, 0.09f, 0.28f),
                            color * 0.54f);
                        brick.localRotation =
                            Quaternion.Euler(-20f, 0f, side * 8f);
                    }
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform eye = Part(
                    "T90-RecoveryEye",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.82f, 0.66f, 3.05f),
                    new Vector3(0.08f, 0.02f, 0.08f),
                    TankT90FamilyDetails.Dark());
                eye.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddSkirtsAndCage(
            Transform root,
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
                    Part(
                        "Painted-T90-K5SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.83f,
                            1.06f,
                            2.55f - panel * 1.02f),
                        new Vector3(0.105f, 0.7f, 0.94f),
                        color * 0.55f);
                }
                for (int panel = 0;
                    panel < 5;
                    panel++)
                {
                    Part(
                        "Painted-T90-RubberSkirt",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.77f,
                            0.98f,
                            -1.18f + panel * 0.74f),
                        new Vector3(0.04f, 0.72f, 0.58f),
                        color * 0.38f);
                }
                for (int rail = 0;
                    rail < 6;
                    rail++)
                {
                    Part(
                        "T90-RearQuarterSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.93f,
                            0.78f + rail * 0.105f,
                            -2.26f),
                        new Vector3(0.035f, 0.024f, 1.46f),
                        TankT90FamilyDetails.Dark());
                }
            }
        }

        private static void AddRunningGear(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int index = 0;
                    index < WheelStations.Length;
                    index++)
                {
                    Vector3 center =
                        new Vector3(side * 1.62f, 0.48f, WheelStations[index]);
                    Transform inset = Part(
                        "T90-RoadWheelInset",
                        PrimitiveType.Cylinder,
                        root,
                        center,
                        new Vector3(0.32f, 0.045f, 0.32f),
                        TankT90FamilyDetails.Dark());
                    inset.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                    for (int spoke = 0;
                        spoke < 6;
                        spoke++)
                    {
                        Transform arm = Part(
                            "Painted-T90-RoadWheelSpoke",
                            PrimitiveType.Cube,
                            root,
                            center,
                            new Vector3(0.036f, 0.08f, 0.48f),
                            color * 0.52f);
                        arm.localRotation =
                            Quaternion.Euler(spoke * 30f, 0f, 0f);
                    }
                }
                for (int index = 0;
                    index < RollerStations.Length;
                    index++)
                {
                    Transform roller = Part(
                        "T90-ReturnRoller",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(side * 1.48f, 0.82f, RollerStations[index]),
                        new Vector3(0.086f, 0.035f, 0.086f),
                        color * 0.45f);
                    roller.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static void AddDeckAndRear(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T90-DriverHatch",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.36f, 1.48f),
                new Vector3(0.48f, 0.08f, 0.52f),
                color * 0.7f);
            for (int line = 0;
                line < 5;
                line++)
            {
                Part(
                    "T90-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0.12f, 1.42f, -1.34f - line * 0.16f),
                    new Vector3(1.52f, 0.018f, 0.055f),
                    TankT90FamilyDetails.Dark());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T90-RearStowageBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.6f, 1.5f, -3.26f),
                    new Vector3(0.98f, 0.3f, 0.3f),
                    color * 0.6f);
                Transform log = Part(
                    "T90-SplitUnditchingLog",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.64f, 1.3f, -3.44f),
                    new Vector3(0.088f, 0.59f, 0.088f),
                    TankT90FamilyDetails.Wood());
                log.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
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
