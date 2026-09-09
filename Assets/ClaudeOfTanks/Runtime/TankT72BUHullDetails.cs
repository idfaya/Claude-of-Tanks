using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72BUHullDetails
    {
        private static readonly float[] WheelStations =
        {
            2.07f, 1.262f, 0.454f,
            -0.354f, -1.162f, -1.97f
        };

        private static readonly float[] RollerStations =
        {
            -1.42f, -0.18f, 1.08f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            AddHull(root, color);
            AddHullEra(root, color);
            AddSkirts(root, color);
            AddRunningGear(root, color);
            AddEngineDeck(root, color);
            AddRearService(root, color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T72BU-LowerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.74f, -0.03f),
                new Vector3(2.18f, 0.72f, 5.75f),
                color * 0.48f);
            Part(
                "Painted-T72BU-UpperHull",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.12f, 0.02f),
                new Vector3(3.25f, 0.34f, 5.7f),
                color * 0.68f);
            Transform glacis = Part(
                "Painted-T72BU-SweptGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.1f, 2.36f),
                new Vector3(2.92f, 0.12f, 0.58f),
                color * 0.64f);
            glacis.localRotation =
                Quaternion.Euler(-17f, 0f, 0f);
            Transform lower = Part(
                "Painted-T72BU-LowerGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.67f, 2.62f),
                new Vector3(2.45f, 0.12f, 0.55f),
                color * 0.46f);
            lower.localRotation =
                Quaternion.Euler(28f, 0f, 0f);
        }

        private static void AddHullEra(
            Transform root,
            Color color)
        {
            for (int row = 0;
                row < 3;
                row++)
            {
                for (int column = -4;
                    column <= 4;
                    column++)
                {
                    if (row == 0 && Math.Abs(column) == 4) continue;
                    Transform brick = Part(
                        "Painted-T72BU-K5GlacisWedge",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            column * 0.295f,
                            1.18f - row * 0.05f,
                            1.72f + row * 0.255f),
                        new Vector3(0.26f, 0.11f, 0.22f),
                        color * 0.54f);
                    brick.localRotation =
                        Quaternion.Euler(-17f, 0f, 0f);
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T72BU-BowFenderProng",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.6f, 1.21f, 2.8f),
                    new Vector3(0.34f, 0.16f, 0.38f),
                    color * 0.63f);
                Transform eye = Part(
                    "T72BU-RecoveryEye",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.86f, 0.68f, 2.81f),
                    new Vector3(0.08f, 0.02f, 0.08f),
                    TankT72BUFamilyDetails.Dark());
                eye.localRotation =
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
                    panel < 8;
                    panel++)
                {
                    float z =
                        -2.52f + panel * 0.68f;
                    Part(
                        "Painted-T72BU-K5SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.74f, 1.02f, z),
                        new Vector3(0.085f, 0.34f, 0.56f),
                        color * 0.57f);
                    Part(
                        "T72BU-SkirtSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.79f, 1.01f, z + 0.28f),
                        new Vector3(0.018f, 0.28f, 0.026f),
                        TankT72BUFamilyDetails.Dark());
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
                        new Vector3(side * 1.62f, 0.47f, WheelStations[index]);
                    Transform inset = Part(
                        "T72BU-RoadWheelInset",
                        PrimitiveType.Cylinder,
                        root,
                        center,
                        new Vector3(0.35f, 0.045f, 0.35f),
                        TankT72BUFamilyDetails.Dark());
                    inset.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                    for (int spoke = 0;
                        spoke < 6;
                        spoke++)
                    {
                        Transform arm = Part(
                            "Painted-T72BU-RoadWheelSpoke",
                            PrimitiveType.Cube,
                            root,
                            center,
                            new Vector3(0.038f, 0.08f, 0.53f),
                            color * 0.54f);
                        arm.localRotation =
                            Quaternion.Euler(spoke * 30f, 0f, 0f);
                    }
                }
                for (int index = 0;
                    index < RollerStations.Length;
                    index++)
                {
                    Transform roller = Part(
                        "T72BU-ReturnRoller",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(side * 1.48f, 0.89f, RollerStations[index]),
                        new Vector3(0.088f, 0.035f, 0.088f),
                        color * 0.46f);
                    roller.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T72BU-DriverPlinth",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.33f, 0.78f),
                new Vector3(0.5f, 0.2f, 0.72f),
                color * 0.7f);
            for (int line = 0;
                line < 7;
                line++)
            {
                Part(
                    "T72BU-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0.16f, 1.4f, -1.42f - line * 0.15f),
                    new Vector3(1.56f, 0.018f, 0.055f),
                    TankT72BUFamilyDetails.Dark());
            }
            Part(
                "Painted-T72BU-WadingMastBase",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.58f, 1.43f, -1.92f),
                new Vector3(0.3f, 0.07f, 0.34f),
                color * 0.6f);
            Part(
                "T72BU-WadingMast",
                PrimitiveType.Cylinder,
                root,
                new Vector3(-0.58f, 1.73f, -1.92f),
                new Vector3(0.07f, 0.38f, 0.07f),
                TankT72BUFamilyDetails.Dark());
        }

        private static void AddRearService(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T72BU-RearTransom",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.23f, -2.88f),
                new Vector3(2.66f, 0.28f, 0.1f),
                color * 0.55f);
            float[] xs =
            {
                -0.94f, -0.39f, 0.19f, 0.76f
            };
            for (int index = 0;
                index < xs.Length;
                index++)
            {
                Transform drum = Part(
                    "Painted-T72BU-RearFuelDrum",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(xs[index], 1.34f, -2.92f),
                    new Vector3(0.24f, 0.24f, 0.24f),
                    color * 0.6f);
                drum.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            Transform log = Part(
                "T72BU-UnditchingLog",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, 0.96f, -2.96f),
                new Vector3(0.075f, 0.93f, 0.075f),
                TankT72BUFamilyDetails.Wood());
            log.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
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
