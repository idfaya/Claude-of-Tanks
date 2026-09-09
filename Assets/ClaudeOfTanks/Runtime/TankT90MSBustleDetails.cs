using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSBustleDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddJoinedShoulder(root, color);
            AddMagazineBody(root, color);
            AddServiceHardware(root, color);
            AddOpenCage(root);
        }

        private static void AddJoinedShoulder(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90MS-BustleShoulder",
                root,
                new[]
                {
                    S(-0.46f, 0.50f, 0.72f,
                        -1.18f, 1.18f, -1.10f, 1.10f,
                        -0.94f, 0.94f),
                    S(-0.86f, 0.49f, 0.71f,
                        -1.15f, 1.15f, -1.06f, 1.06f,
                        -0.92f, 0.92f),
                    S(-1.20f, 0.47f, 0.69f,
                        -1.10f, 1.10f, -1.01f, 1.01f,
                        -0.90f, 0.90f),
                    S(-1.56f, 0.45f, 0.66f,
                        -1.03f, 1.03f, -0.94f, 0.94f,
                        -0.86f, 0.86f),
                    S(-1.92f, 0.42f, 0.63f,
                        -0.94f, 0.94f, -0.85f, 0.85f,
                        -0.79f, 0.79f),
                    S(-2.24f, 0.39f, 0.59f,
                        -0.84f, 0.84f, -0.75f, 0.75f,
                        -0.70f, 0.70f),
                    S(-2.39f, 0.36f, 0.55f,
                        -0.76f, 0.76f, -0.68f, 0.68f,
                        -0.64f, 0.64f)
                },
                color * 0.59f);
        }

        private static void AddMagazineBody(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90MS-BustleMagazine",
                root,
                new[]
                {
                    S(-1.18f, 0.18f, 0.70f,
                        -1.15f, 1.15f, -1.02f, 1.02f,
                        -1.02f, 1.02f),
                    S(-1.43f, 0.18f, 0.68f,
                        -1.10f, 1.10f, -0.97f, 0.97f,
                        -0.96f, 0.96f),
                    S(-1.67f, 0.18f, 0.65f,
                        -0.99f, 0.99f, -0.88f, 0.88f,
                        -0.86f, 0.86f),
                    S(-1.90f, 0.18f, 0.62f,
                        -0.94f, 0.94f, -0.82f, 0.82f,
                        -0.79f, 0.79f),
                    S(-2.18f, 0.19f, 0.59f,
                        -0.86f, 0.86f, -0.75f, 0.75f,
                        -0.72f, 0.72f),
                    S(-2.39f, 0.20f, 0.55f,
                        -0.78f, 0.78f, -0.68f, 0.68f,
                        -0.65f, 0.65f)
                },
                color * 0.57f);
            Box("Painted-T90MS-BustleRoofPlate", root,
                V(0f, 0.642f, -1.88f),
                V(1.56f, 0.022f, 0.72f), color * 0.61f);
        }

        private static void AddServiceHardware(
            Transform root,
            Color color)
        {
            float[,] lids =
            {
                { -0.57f, 0.52f, -1.67f },
                { 0.02f, 0.46f, -1.72f },
                { 0.58f, 0.48f, -1.88f }
            };
            for (int index = 0; index < lids.GetLength(0); index++)
            {
                float x = lids[index, 0];
                float width = lids[index, 1];
                float z = lids[index, 2];
                Box("T90MS-BustleServiceLid", root,
                    V(x, 0.565f, z),
                    V(width, 0.018f, 0.34f), Dark());
                Box("T90MS-BustleServiceLatch", root,
                    V(x + width * 0.30f, 0.585f, z + 0.08f),
                    V(0.025f, 0.035f, 0.12f), Detail());
            }
            foreach (float x in new[]
            {
                -0.72f, -0.36f, 0f, 0.36f, 0.72f
            })
            {
                Box("T90MS-BustleRearPost", root,
                    V(x, 0.42f, -2.328f),
                    V(0.025f, 0.24f, 0.018f), Dark());
            }
            foreach (float y in new[] { 0.32f, 0.46f, 0.57f })
            {
                Box("T90MS-BustleRearRail", root,
                    V(0f, y, -2.337f),
                    V(1.52f, 0.020f, 0.018f), Detail());
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Transform shoulder = Box(
                    "Painted-T90MS-BustleSideShoulder",
                    root,
                    V(side * 0.98f, 0.30f, -1.76f),
                    V(0.18f, 0.26f, 0.50f),
                    color * 0.56f);
                shoulder.localRotation = Quaternion.Euler(
                    -0.08f * Mathf.Rad2Deg,
                    -side * 0.08f * Mathf.Rad2Deg,
                    0f);
                Transform face = Box(
                    "T90MS-BustleSideShoulderFace",
                    root,
                    V(side * 1.08f, 0.30f, -1.76f),
                    V(0.018f, 0.21f, 0.43f),
                    Dark());
                face.localRotation = shoulder.localRotation;
                Box("T90MS-BustleSideStrut", root,
                    V(side * 0.88f, 0.47f, -2.55f),
                    V(0.035f, 0.035f, 0.52f), Detail());
            }
        }

        private static void AddOpenCage(Transform root)
        {
            foreach (float y in new[]
            {
                0.39f, 0.49f, 0.59f, 0.68f
            })
            {
                Box("T90MS-BustleCageRearRail", root,
                    V(0f, y, -2.72f),
                    V(1.88f, 0.024f, 0.045f), Detail());
            }
            foreach (float x in new[]
            {
                -0.90f, -0.45f, 0f, 0.45f, 0.90f
            })
            {
                Box("T90MS-BustleCageRearPost", root,
                    V(x, 0.52f, -2.72f),
                    V(0.024f, 0.30f, 0.045f), Detail());
            }
            for (int side = -1; side <= 1; side += 2)
            {
                foreach (float y in new[]
                {
                    0.40f, 0.50f, 0.60f, 0.68f
                })
                {
                    Box("T90MS-BustleCageSideRail", root,
                        V(side * 0.96f, y, -2.25f),
                        V(0.035f, 0.024f, 1.18f), Detail());
                }
                foreach (float z in new[]
                {
                    -2.78f, -2.42f, -2.06f, -1.70f
                })
                {
                    Box("T90MS-BustleCageSidePost", root,
                        V(side * 0.96f, 0.53f, z),
                        V(0.035f, 0.29f, 0.024f), Detail());
                }
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
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }

        private static Color Detail()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }
    }
}
