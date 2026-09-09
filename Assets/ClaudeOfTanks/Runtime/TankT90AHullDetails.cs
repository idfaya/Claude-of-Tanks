using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AHullDetails
    {
        private static readonly float[] WheelStations =
        {
            -1.78f, -0.992f, -0.204f,
            0.584f, 1.372f, 2.16f
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
            AddGlacisKit(root, color);
            AddSkirts(root, color);
            AddRunningGear(root, color);
            AddRearService(root, color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T90A-LowerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.76f, -0.03f),
                new Vector3(2.32f, 0.72f, 5.86f),
                color * 0.46f);
            Part(
                "Painted-T90A-UpperHull",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.18f, -0.05f),
                new Vector3(3.28f, 0.36f, 5.94f),
                color * 0.67f);
            Transform glacis = Part(
                "Painted-T90A-BluntGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.08f, 2.58f),
                new Vector3(2.94f, 0.12f, 0.88f),
                color * 0.61f);
            glacis.localRotation =
                Quaternion.Euler(-21f, 0f, 0f);
            Part(
                "Painted-T90A-DriverHatch",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.36f, 1.48f),
                new Vector3(0.5f, 0.08f, 0.54f),
                color * 0.7f);
        }

        private static void AddGlacisKit(
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
                        column < 3;
                        column++)
                    {
                        Transform brick = Part(
                            "Painted-T90A-K5GlacisCassette",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * (0.22f + column * 0.34f),
                                1.14f - row * 0.065f,
                                2.5f + row * 0.29f),
                            new Vector3(0.3f, 0.06f, 0.26f),
                            color * 0.54f);
                        brick.localRotation =
                            Quaternion.Euler(-17f, 0f, side * 8f);
                    }
                    for (int seam = 0;
                        seam < 2;
                        seam++)
                    {
                        Part(
                            "T90A-K5GlacisSeam",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * (0.395f + seam * 0.34f),
                                1.137f - row * 0.065f,
                                2.5f + row * 0.29f),
                            new Vector3(0.03f, 0.05f, 0.24f),
                            TankT90AFamilyDetails.Dark());
                    }
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "T90A-HeadlightPod",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.95f, 1.1f, 2.97f),
                    new Vector3(0.22f, 0.12f, 0.16f),
                    TankT90AFamilyDetails.Dark());
                Transform hook = Part(
                    "T90A-TowHook",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.4f, 0.68f, 3.04f),
                    new Vector3(0.06f, 0.025f, 0.06f),
                    TankT90AFamilyDetails.Dark());
                hook.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddSkirts(
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
                    Part(
                        "Painted-T90A-RubberSkirtBand",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.7675f, 1.06f, -2.58f + panel * 0.82f),
                        new Vector3(0.036f, 0.32f, 0.68f),
                        color * 0.39f);
                }
                for (int panel = 0;
                    panel < 4;
                    panel++)
                {
                    Part(
                        "Painted-T90A-K5SkirtCassette",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.863f, 0.955f, 2.4f - panel * 0.55f),
                        new Vector3(0.05f, 0.53f, 0.56f),
                        color * 0.54f);
                }
                Part(
                    "Painted-T90A-RearMudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.52f, 0.8f, -3.06f),
                    new Vector3(0.36f, 0.3f, 0.05f),
                    color * 0.34f);
                Part(
                    "Painted-T90A-FrontMudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.55f, 0.85f, 3.345f),
                    new Vector3(0.4f, 0.36f, 0.05f),
                    color * 0.34f);
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
                        new Vector3(side * 1.62f, 0.455f, WheelStations[index]);
                    Transform dish = Part(
                        "T90A-RoadWheelDish",
                        PrimitiveType.Cylinder,
                        root,
                        center,
                        new Vector3(0.32f, 0.045f, 0.32f),
                        TankT90AFamilyDetails.Dark());
                    dish.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                    for (int spoke = 0;
                        spoke < 6;
                        spoke++)
                    {
                        Transform arm = Part(
                            "Painted-T90A-RoadWheelSpoke",
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
                        "T90A-ReturnRoller",
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

        private static void AddRearService(
            Transform root,
            Color color)
        {
            for (int line = 0;
                line < 5;
                line++)
            {
                Part(
                    "T90A-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0.12f, 1.34f, -1.74f - line * 0.12f),
                    new Vector3(1.5f, 0.018f, 0.055f),
                    TankT90AFamilyDetails.Dark());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T90A-RearStowageBox",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.72f, 1.26f, -2.81f),
                    new Vector3(1.18f, 0.16f, 0.28f),
                    color * 0.6f);
                Transform drum = Part(
                    "Painted-T90A-RearFuelDrum",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.72f, 1.325f, -3.18f),
                    new Vector3(0.145f, 0.23f, 0.145f),
                    color * 0.6f);
                drum.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Transform log = Part(
                    "T90A-SplitUnditchingLog",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.575f, 1.36f, -3.23f),
                    new Vector3(0.095f, 0.425f, 0.095f),
                    TankT90AFamilyDetails.Wood());
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
