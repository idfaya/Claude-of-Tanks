using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MBowArmorDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                AddShoulderBrow(root, color, side);
                AddLowerNose(root, color, side);
                AddUpperBlanket(root, color, side);
            }
            Box(
                "T90M-UpperGlacisCenterWeld",
                root,
                V(0f, 1.435f, 2.30f),
                V(0.075f, 0.040f, 0.68f),
                Dark(),
                R(-0.32f, 0f, 0f));
        }

        private static void AddShoulderBrow(
            Transform root,
            Color color,
            int side)
        {
            Slab(
                "Painted-T90M-TrackShoulder",
                root,
                side,
                color * 0.58f,
                V(0.96f, 1.00f, 3.16f),
                V(1.80f, 0.98f, 3.10f),
                V(1.70f, 1.33f, 2.43f),
                V(0.91f, 1.33f, 2.65f),
                V(0.94f, 1.09f, 3.10f),
                V(1.77f, 1.08f, 3.04f),
                V(1.65f, 1.43f, 2.44f),
                V(0.89f, 1.43f, 2.64f));
            Slab(
                "Painted-T90M-TrackShoulderLower",
                root,
                side,
                color * 0.56f,
                V(1.34f, 0.93f, 3.18f),
                V(1.82f, 0.91f, 3.12f),
                V(1.77f, 1.17f, 2.72f),
                V(1.28f, 1.19f, 2.80f),
                V(1.32f, 0.99f, 3.14f),
                V(1.80f, 0.98f, 3.08f),
                V(1.73f, 1.25f, 2.73f),
                V(1.27f, 1.26f, 2.80f));
            Box(
                "T90M-TrackShoulderWeld",
                root,
                V(side * 1.44f, 1.435f, 2.55f),
                V(0.50f, 0.026f, 0.050f),
                Dark(),
                R(-0.30f, -side * 0.22f, 0f));
            Box(
                "T90M-TrackShoulderLowerWeld",
                root,
                V(side * 1.58f, 1.265f, 2.83f),
                V(0.42f, 0.026f, 0.045f),
                Dark(),
                R(-0.42f, -side * 0.16f, 0f));

            float[,] cassettes =
            {
                { 1.13f, 1.445f, 2.54f, 0.26f, 0.38f, 0.30f },
                { 1.43f, 1.385f, 2.67f, 0.38f, 0.34f, 0.28f },
                { 1.63f, 1.275f, 2.88f, 0.28f, 0.25f, 0.24f }
            };
            for (int index = 0;
                index < cassettes.GetLength(0);
                index++)
            {
                float x = cassettes[index, 0];
                float y = cassettes[index, 1];
                float z = cassettes[index, 2];
                float yaw = cassettes[index, 3];
                float width = cassettes[index, 4];
                float depth = cassettes[index, 5];
                Quaternion rotation =
                    R(-0.35f, -side * yaw, 0f);
                Box(
                    "Painted-T90M-ShoulderRelikt",
                    root,
                    V(side * x, y, z),
                    V(width, 0.070f, depth),
                    color * 0.52f,
                    rotation);
                Box(
                    "T90M-ShoulderReliktSeam",
                    root,
                    V(side * x, y + 0.043f, z - depth * 0.34f),
                    V(width * 0.70f, 0.010f, 0.030f),
                    Dark(),
                    rotation);
            }
        }

        private static void AddLowerNose(
            Transform root,
            Color color,
            int side)
        {
            Slab(
                "Painted-T90M-LowerNose",
                root,
                side,
                color * 0.57f,
                V(0.04f, 0.72f, 3.235f),
                V(1.06f, 0.77f, 3.17f),
                V(1.39f, 1.29f, 2.64f),
                V(0.04f, 1.29f, 2.73f),
                V(0.04f, 0.79f, 3.19f),
                V(1.01f, 0.85f, 3.13f),
                V(1.34f, 1.37f, 2.61f),
                V(0.04f, 1.37f, 2.70f));
            Box(
                "T90M-LowerNoseCenterWeld",
                root,
                V(side * 0.035f, 1.07f, 2.96f),
                V(0.035f, 0.030f, 0.70f),
                Dark(),
                R(-0.49f, 0f, 0f));
            Box(
                "T90M-LowerNoseServiceSeam",
                root,
                V(side * 0.52f, 1.00f, 3.02f),
                V(0.80f, 0.022f, 0.045f),
                Dark(),
                R(-0.47f, -side * 0.05f, 0f));
        }

        private static void AddUpperBlanket(
            Transform root,
            Color color,
            int side)
        {
            float[,] cassettes =
            {
                { 0.22f, 1.500f, 2.10f, 0.10f, 0.34f, 0.28f },
                { 0.50f, 1.485f, 2.20f, 0.20f, 0.38f, 0.30f },
                { 0.80f, 1.460f, 2.31f, 0.30f, 0.40f, 0.30f },
                { 1.09f, 1.430f, 2.43f, 0.40f, 0.36f, 0.28f }
            };
            for (int index = 0;
                index < cassettes.GetLength(0);
                index++)
            {
                float x = cassettes[index, 0];
                float y = cassettes[index, 1];
                float z = cassettes[index, 2];
                float yaw = cassettes[index, 3];
                float width = cassettes[index, 4];
                float depth = cassettes[index, 5];
                Quaternion rotation =
                    R(-0.32f, -side * yaw, 0f);
                Box(
                    "Painted-T90M-UpperGlacisRelikt",
                    root,
                    V(side * x, y, z),
                    V(width, 0.065f, depth),
                    color * 0.52f,
                    rotation);
                Box(
                    "T90M-UpperGlacisReliktSeam",
                    root,
                    V(side * x, y + 0.040f, z - depth * 0.34f),
                    V(width * 0.74f, 0.010f, 0.025f),
                    Dark(),
                    rotation);
            }
        }

        private static void Slab(
            string name,
            Transform root,
            int side,
            Color color,
            Vector3 b0,
            Vector3 b1,
            Vector3 b2,
            Vector3 b3,
            Vector3 t0,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3)
        {
            TankShapeFactory.OrientedSlabPart(
                name,
                root,
                M(b0, side), M(b1, side),
                M(b2, side), M(b3, side),
                M(t0, side), M(t1, side),
                M(t2, side), M(t3, side),
                color);
        }

        private static Vector3 M(Vector3 point, int side)
        {
            point.x *= side;
            return point;
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
