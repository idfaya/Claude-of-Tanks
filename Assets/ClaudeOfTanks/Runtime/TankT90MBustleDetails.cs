using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MBustleDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddMagazine(root, color);
            AddServiceHardware(root, color);
            AddOpenCage(root);
            TankT90MBustleFinalDetails.Build(root, color);
        }

        private static void AddMagazine(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90M-BustleMagazine",
                root,
                new[]
                {
                    S(-1.18f, 0.13f, 0.68f,
                        -1.20f, 1.20f,
                        -1.10f, 1.10f,
                        -1.14f, 1.14f),
                    S(-1.55f, 0.15f, 0.70f,
                        -1.18f, 1.18f,
                        -1.06f, 1.06f,
                        -1.12f, 1.12f),
                    S(-2.03f, 0.18f, 0.67f,
                        -1.09f, 1.09f,
                        -0.96f, 0.96f,
                        -1.03f, 1.03f),
                    S(-2.42f, 0.22f, 0.60f,
                        -0.96f, 0.96f,
                        -0.82f, 0.82f,
                        -0.90f, 0.90f)
                },
                color * 0.58f);
            float[,] lids =
            {
                { -1.38f, 2.12f, 0.34f, 0.69f },
                { -1.82f, 1.98f, 0.42f, 0.70f },
                { -2.25f, 1.72f, 0.34f, 0.64f }
            };
            for (int index = 0;
                index < lids.GetLength(0);
                index++)
            {
                float z = lids[index, 0];
                float width = lids[index, 1];
                float depth = lids[index, 2];
                float y = lids[index, 3];
                Box(
                    "T90M-BustleServiceLid",
                    root,
                    V(0f, y, z),
                    V(width, 0.018f, depth),
                    Dark());
                Box(
                    "T90M-BustleServiceLatch",
                    root,
                    V(0f, y + 0.015f, z + depth * 0.40f),
                    V(width * 0.82f, 0.012f, 0.040f),
                    Detail());
            }
            Box(
                "Painted-T90M-BustleRoof",
                root,
                V(0f, 0.705f, -1.73f),
                V(1.64f, 0.080f, 1.12f),
                color * 0.60f);
            Box(
                "T90M-BustleRoofSeam",
                root,
                V(0f, 0.751f, -1.73f),
                V(1.44f, 0.012f, 0.92f),
                Dark());
        }

        private static void AddServiceHardware(
            Transform root,
            Color color)
        {
            float[,] bins =
            {
                { 1.22f, -1.32f, 0.34f },
                { 1.17f, -1.70f, 0.32f },
                { 1.10f, -2.05f, 0.30f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < bins.GetLength(0);
                    index++)
                {
                    float x = bins[index, 0];
                    float z = bins[index, 1];
                    float depth = bins[index, 2];
                    Box(
                        "Painted-T90M-BustleSideBin",
                        root,
                        V(side * x, 0.38f, z),
                        V(0.24f, 0.32f, depth),
                        color * 0.56f);
                    Box(
                        "T90M-BustleSideBinFace",
                        root,
                        V(side * (x + 0.13f), 0.38f, z),
                        V(0.016f, 0.26f, depth * 0.82f),
                        Dark());
                    Box(
                        "T90M-BustleSideBinLatch",
                        root,
                        V(side * x, 0.55f, z + depth * 0.34f),
                        V(0.18f, 0.016f, 0.035f),
                        Detail());
                }
                foreach (float y in new[]
                {
                    0.28f, 0.39f, 0.50f, 0.59f
                })
                {
                    Box(
                        "T90M-BustleSideRail",
                        root,
                        V(side * 0.83f, y, -2.28f),
                        V(0.032f, 0.024f, 0.74f),
                        Detail());
                }
                foreach (float z in new[]
                {
                    -2.52f, -2.25f, -1.98f
                })
                {
                    Box(
                        "T90M-BustleSidePost",
                        root,
                        V(side * 0.83f, 0.44f, z),
                        V(0.032f, 0.31f, 0.032f),
                        Detail());
                }
            }
            Box(
                "Painted-T90M-BustleLeftStore",
                root,
                V(-0.56f, 0.75f, -1.58f),
                V(0.82f, 0.18f, 0.56f),
                color * 0.56f);
            Box(
                "Painted-T90M-BustleRightStore",
                root,
                V(0.55f, 0.73f, -1.73f),
                V(0.72f, 0.16f, 0.50f),
                color * 0.56f);
            Box(
                "T90M-BustleLeftStoreLid",
                root,
                V(-0.56f, 0.848f, -1.58f),
                V(0.70f, 0.016f, 0.46f),
                Dark());
            Box(
                "T90M-BustleRightStoreLid",
                root,
                V(0.55f, 0.818f, -1.73f),
                V(0.60f, 0.016f, 0.40f),
                Dark());
        }

        private static void AddOpenCage(Transform root)
        {
            foreach (float y in new[] { 0.30f, 0.41f, 0.52f })
            {
                Box(
                    "T90M-BustleRearRail",
                    root,
                    V(0f, y, -2.56f),
                    V(1.64f, 0.024f, 0.040f),
                    Detail());
            }
            foreach (float x in new[]
            {
                -0.76f, -0.38f, 0f, 0.38f, 0.76f
            })
            {
                Box(
                    "T90M-BustleRearPost",
                    root,
                    V(x, 0.42f, -2.56f),
                    V(0.024f, 0.26f, 0.040f),
                    Detail());
            }
        }

        private static TankWeldedStation S(
            float z,
            float bottomY,
            float topY,
            float middleLeft,
            float middleRight,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            return new TankWeldedStation(
                z,
                bottomY,
                topY,
                middleLeft,
                middleRight,
                bottomLeft,
                bottomRight,
                topLeft,
                topRight);
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color,
            Quaternion? rotation = null)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            part.localRotation = rotation ?? Quaternion.identity;
            return part;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.20f, 0.22f, 0.16f);
        }

        private static Color Detail()
        {
            return new Color(0.28f, 0.30f, 0.24f);
        }

    }
}
