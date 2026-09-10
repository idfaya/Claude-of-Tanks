using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MTurretArmorDetails
    {
        public static void Build(
            Transform root,
            Color color,
            bool proryv)
        {
            if (proryv)
                AddProryvChevron(root, color);
            else
                AddT90MFan(root, color);
            AddFlankCourse(root, color);
            AddFinalSurfaceSeams(root);
            Box(
                "T90M-MantletGapPlate",
                root,
                V(0f, 0.24f, 1.38f),
                V(0.54f, 0.30f, 0.07f),
                Dark(),
                R(-0.29f, 0f, 0f));
        }

        private static void AddProryvChevron(
            Transform root,
            Color color)
        {
            TankSovietChevronArmorFactory.Build(
                "T90M-ProryvChevron",
                root,
                new[]
                {
                    new[]
                    {
                        V2(0.18f, 1.34f),
                        V2(0.30f, 1.48f),
                        V2(0.91f, 1.12f),
                        V2(0.78f, 0.97f)
                    },
                    new[]
                    {
                        V2(0.79f, 1.02f),
                        V2(0.93f, 1.16f),
                        V2(1.57f, 0.53f),
                        V2(1.43f, 0.39f)
                    }
                },
                new[]
                {
                    new TankChevronRow(
                        0.07f, 0.33f, -0.11f, 0.10f),
                    new TankChevronRow(
                        0.33f, 0.61f, 0.10f, -0.11f)
                },
                new[]
                {
                    V2(0.055f, 0.295f),
                    V2(0.335f, 0.665f),
                    V2(0.705f, 0.945f)
                },
                0.034f,
                0.092f,
                0.014f,
                color * 0.55f,
                Dark(),
                color * 0.52f);
            Box(
                "T90M-ProryvChevronCenterClosure",
                root,
                V(0f, 0.25f, 1.51f),
                V(0.44f, 0.26f, 0.070f),
                Dark(),
                R(-0.26f, 0f, 0f));
        }

        private static void AddT90MFan(
            Transform root,
            Color color)
        {
            float[,] primary =
            {
                { 0.27f, 0.34f, 1.31f, 0.14f, -0.34f,
                    0.32f, 0.27f, 0.39f },
                { 0.49f, 0.36f, 1.18f, 0.29f, -0.37f,
                    0.38f, 0.31f, 0.43f },
                { 0.72f, 0.36f, 1.02f, 0.43f, -0.35f,
                    0.43f, 0.34f, 0.46f },
                { 0.96f, 0.34f, 0.83f, 0.56f, -0.31f,
                    0.47f, 0.35f, 0.45f },
                { 1.20f, 0.31f, 0.60f, 0.68f, -0.27f,
                    0.48f, 0.34f, 0.43f },
                { 1.42f, 0.28f, 0.34f, 0.56f, -0.20f,
                    0.40f, 0.31f, 0.40f },
                { 1.57f, 0.25f, 0.08f, 0.33f, -0.13f,
                    0.28f, 0.27f, 0.34f }
            };
            float[,] brow =
            {
                { 0.35f, 0.48f, 0.93f, 0.20f, 0.30f, 0.34f },
                { 0.61f, 0.49f, 0.73f, 0.34f, 0.34f, 0.36f },
                { 0.88f, 0.46f, 0.50f, 0.48f, 0.36f, 0.34f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < primary.GetLength(0);
                    index++)
                {
                    float x = primary[index, 0];
                    float y = primary[index, 1];
                    float z = primary[index, 2];
                    float yaw = primary[index, 3];
                    float roll = primary[index, 4];
                    float width = primary[index, 5];
                    float height = primary[index, 6];
                    float depth = primary[index, 7];
                    Quaternion rotation =
                        R(roll, -side * yaw, -side * 0.20f);
                    Vector3 anchor = V(side * x, y, z);
                    Box(
                        "Painted-T90M-FanRelikt",
                        root,
                        anchor + rotation * V(0f, 0f, -0.075f),
                        V(width, height, depth),
                        color * 0.50f,
                        rotation);
                    Box(
                        "T90M-FanReliktSeam",
                        root,
                        anchor + rotation *
                            V(0f, height * 0.52f, 0.045f),
                        V(width * 0.80f, 0.012f, depth * 0.75f),
                        Dark(),
                        rotation);
                }
                for (int index = 0;
                    index < brow.GetLength(0);
                    index++)
                {
                    float x = brow[index, 0];
                    float y = brow[index, 1];
                    float z = brow[index, 2];
                    float yaw = brow[index, 3];
                    float width = brow[index, 4];
                    float depth = brow[index, 5];
                    Quaternion rotation =
                        R(-0.14f, -side * yaw, -side * 0.12f);
                    Box(
                        "Painted-T90M-InnerBrowRelikt",
                        root,
                        V(side * x, y, z),
                        V(width, 0.20f, depth),
                        color * 0.50f,
                        rotation);
                    Box(
                        "T90M-InnerBrowReliktSeam",
                        root,
                        V(side * x, y + 0.105f, z),
                        V(width * 0.72f, 0.014f, depth * 0.70f),
                        Dark(),
                        rotation);
                }
            }
        }

        private static void AddFlankCourse(
            Transform root,
            Color color)
        {
            float[,] rows =
            {
                { 0.02f, 0.22f, 0.28f, 0.36f, 0.10f },
                { -0.36f, 0.24f, 0.30f, 0.34f, 0.04f },
                { -0.72f, 0.22f, 0.27f, 0.31f, -0.06f },
                { -1.04f, 0.20f, 0.24f, 0.28f, -0.12f }
            };
            for (int side = -1; side <= 1; side += 2)
            for (int index = 0; index < rows.GetLength(0); index++)
            {
                float z = rows[index, 0];
                float width = rows[index, 1];
                float height = rows[index, 2];
                float depth = rows[index, 3];
                float yaw = rows[index, 4];
                Quaternion rotation =
                    R(-0.08f, -side * yaw, 0f);
                Box(
                    "Painted-T90M-FlankRelikt",
                    root,
                    V(side * 1.62f, 0.28f, z),
                    V(width, height, depth),
                    color * 0.50f,
                    rotation);
                Box(
                    "T90M-FlankReliktSeam",
                    root,
                    V(
                        side * (1.62f + width * 0.52f),
                        0.28f,
                        z),
                    V(0.015f, height * 0.78f, depth * 0.78f),
                    Dark(),
                    rotation);
            }
        }

        private static void AddFinalSurfaceSeams(
            Transform root)
        {
            float[,] front =
            {
                { 0.32f, 0.29f, 1.38f, 0.30f, -0.30f,
                    0.24f, 0.27f, 0.13f },
                { 0.52f, 0.28f, 1.28f, 0.42f, -0.31f,
                    0.27f, 0.29f, 0.14f },
                { 0.73f, 0.27f, 1.15f, 0.53f, -0.29f,
                    0.30f, 0.30f, 0.14f },
                { 0.96f, 0.25f, 0.96f, 0.66f, -0.27f,
                    0.32f, 0.30f, 0.14f },
                { 1.19f, 0.23f, 0.74f, 0.78f, -0.24f,
                    0.32f, 0.29f, 0.13f },
                { 1.37f, 0.22f, 0.48f, 0.84f, -0.20f,
                    0.25f, 0.26f, 0.12f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < front.GetLength(0);
                    index++)
                {
                    Box(
                        "T90M-FinalFrontReliktSeam",
                        root,
                        V(
                            side * front[index, 0],
                            front[index, 1] +
                                front[index, 6] * 0.48f,
                            front[index, 2]),
                        V(
                            front[index, 5] * 0.72f,
                            0.010f,
                            front[index, 7] * 0.70f),
                        Dark(),
                        R(
                            front[index, 4],
                            -side * front[index, 3],
                            -side * 0.10f));
                }
                float[,] flank =
                {
                    { 0.12f, 0.25f, 0.25f },
                    { -0.20f, 0.28f, 0.27f },
                    { -0.54f, 0.27f, 0.27f },
                    { -0.87f, 0.24f, 0.25f }
                };
                for (int index = 0;
                    index < flank.GetLength(0);
                    index++)
                {
                    Box(
                        "T90M-FinalFlankReliktSeam",
                        root,
                        V(side * 1.548f, 0.24f, flank[index, 0]),
                        V(
                            0.010f,
                            flank[index, 1] * 0.72f,
                            flank[index, 2] * 0.72f),
                        Dark(),
                        R(-0.06f, 0f, 0f));
                }
                Box(
                    "T90M-FinalFlankWeld",
                    root,
                    V(side * 1.46f, 0.49f, -0.45f),
                    V(0.035f, 0.030f, 1.18f),
                    Dark(),
                    R(0f, 0f, -side * 0.05f));
            }
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color,
            Quaternion rotation)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            part.localRotation = rotation;
            return part;
        }

        private static Quaternion R(float x, float y, float z)
        {
            return Quaternion.Euler(
                x * Mathf.Rad2Deg,
                y * Mathf.Rad2Deg,
                z * Mathf.Rad2Deg);
        }

        private static Vector2 V2(float x, float y)
        {
            return new Vector2(x, y);
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.20f, 0.22f, 0.16f);
        }
    }
}
