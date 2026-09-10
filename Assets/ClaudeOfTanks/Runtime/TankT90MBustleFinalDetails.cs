using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MBustleFinalDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddEnhancedRear(root);
            AddArmoredShouldersAndStores(root, color);
            AddArmoredShouldersAndStores(root, color);
        }

        private static void AddEnhancedRear(Transform root)
        {
            const float terminalZ = -3.33f;
            Box(
                "T90M-BustleRearBacking",
                root,
                V(0f, 0.43f, terminalZ),
                V(1.54f, 0.26f, 0.012f),
                Dark());
            foreach (float y in new[]
            {
                0.33f, 0.41f, 0.49f, 0.57f
            })
            {
                Box(
                    "T90M-BustleRearLouvre",
                    root,
                    V(0f, y, terminalZ - 0.010f),
                    V(1.34f, 0.018f, 0.014f),
                    Detail());
            }
            foreach (float x in new[]
            {
                -0.60f, -0.20f, 0.20f, 0.60f
            })
            {
                Box(
                    "T90M-BustleRearLouvrePost",
                    root,
                    V(x, 0.45f, terminalZ - 0.012f),
                    V(0.020f, 0.23f, 0.014f),
                    Detail());
            }
            foreach (float x in new[]
            {
                -0.66f, -0.22f, 0.22f, 0.66f
            })
            {
                foreach (float y in new[] { 0.31f, 0.57f })
                {
                    Box(
                        "T90M-BustleRearReturn",
                        root,
                        V(x, y, -3.135f),
                        V(0.032f, 0.030f, 0.39f),
                        Detail());
                }
            }
            foreach (float x in new[] { -0.68f, 0.68f })
            {
                Box(
                    "T90M-BustleRearEndPost",
                    root,
                    V(x, 0.44f, terminalZ),
                    V(0.038f, 0.30f, 0.038f),
                    Detail());
            }
            AddRearCylinder(root);
        }

        private static void AddRearCylinder(Transform root)
        {
            const float centerZ = -2.60f;
            Cylinder(
                "T90M-BustleRearCylinder",
                root,
                V(0f, 0.45f, centerZ),
                0.22f,
                1.42f,
                16,
                Canvas());
            foreach (float x in new[]
            {
                -0.58f, -0.20f, 0.20f, 0.58f
            })
            {
                Box(
                    "T90M-BustleRearCylinderStrap",
                    root,
                    V(x, 0.45f, centerZ),
                    V(0.040f, 0.42f, 0.18f),
                    Dark());
                Box(
                    "T90M-BustleRearCylinderCradle",
                    root,
                    V(x, 0.30f, -2.53f),
                    V(0.045f, 0.055f, 0.34f),
                    Detail());
                Box(
                    "T90M-BustleRearCylinderReturn",
                    root,
                    V(x, 0.34f, -2.50f),
                    V(0.052f, 0.15f, 0.24f),
                    Dark(),
                    R(-0.18f, 0f, 0f));
            }
            foreach (float x in new[] { -0.62f, 0.62f })
            {
                Box(
                    "T90M-BustleRearUpperCradle",
                    root,
                    V(x, 0.59f, -2.53f),
                    V(0.050f, 0.055f, 0.34f),
                    Detail());
            }
            Box(
                "T90M-BustleRearCrossShoe",
                root,
                V(0f, 0.27f, -2.47f),
                V(1.34f, 0.050f, 0.18f),
                Detail());
        }

        private static void AddArmoredShouldersAndStores(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Slab(
                    "Painted-T90M-BustleShoulder",
                    root,
                    side,
                    V(0.88f, 0.17f, -1.05f),
                    V(1.42f, 0.17f, -1.22f),
                    V(1.22f, 0.22f, -2.62f),
                    V(0.76f, 0.24f, -2.48f),
                    V(0.86f, 0.68f, -1.08f),
                    V(1.31f, 0.60f, -1.24f),
                    V(1.10f, 0.60f, -2.52f),
                    V(0.75f, 0.68f, -2.42f),
                    color * 0.56f);
                AddShoulderPanels(root, color, side);
                foreach (float y in new[] { 0.30f, 0.58f })
                {
                    Box(
                        "T90M-BustleCageSideRail",
                        root,
                        V(side * 0.78f, y, -2.88f),
                        V(0.036f, 0.032f, 0.90f),
                        Detail());
                }
                foreach (float z in new[]
                {
                    -2.55f, -2.88f, -3.21f
                })
                {
                    Box(
                        "T90M-BustleCageSidePost",
                        root,
                        V(side * 0.78f, 0.44f, z),
                        V(0.038f, 0.30f, 0.038f),
                        Detail());
                }
            }
            foreach (float y in new[] { 0.31f, 0.58f })
            {
                Box(
                    "T90M-BustleCageRearRail",
                    root,
                    V(0f, y, -3.31f),
                    V(1.58f, 0.032f, 0.038f),
                    Detail());
            }
            AddTopStores(root, color);
        }

        private static void AddShoulderPanels(
            Transform root,
            Color color,
            int side)
        {
            float[,] panels =
            {
                { -1.28f, 0.40f, 0.31f, 0.31f },
                { -1.62f, 0.39f, 0.32f, 0.30f },
                { -1.95f, 0.38f, 0.30f, 0.28f },
                { -2.26f, 0.37f, 0.27f, 0.25f }
            };
            for (int index = 0;
                index < panels.GetLength(0);
                index++)
            {
                float z = panels[index, 0];
                float y = panels[index, 1];
                float height = panels[index, 2];
                float depth = panels[index, 3];
                float x =
                    1.31f - Mathf.Max(0f, -z - 1.28f) * 0.12f;
                Quaternion rotation =
                    R(-0.08f, -side * 0.08f, 0f);
                Box(
                    "Painted-T90M-BustleShoulderPanel",
                    root,
                    V(side * x, y, z),
                    V(0.22f, height, depth),
                    color * 0.54f,
                    rotation);
                Box(
                    "T90M-BustleShoulderPanelFace",
                    root,
                    V(side * (x + 0.118f), y, z),
                    V(0.014f, height * 0.74f, depth * 0.74f),
                    Dark(),
                    rotation);
                Box(
                    "T90M-BustleShoulderPanelLatch",
                    root,
                    V(side * x, y + height * 0.53f, z + depth * 0.30f),
                    V(0.16f, 0.018f, 0.034f),
                    Detail());
            }
        }

        private static void AddTopStores(
            Transform root,
            Color color)
        {
            float[,] stores =
            {
                { -0.58f, 0.785f, -1.50f, 0.58f, 0.42f },
                { 0.52f, 0.785f, -1.62f, 0.50f, 0.36f }
            };
            for (int index = 0;
                index < stores.GetLength(0);
                index++)
            {
                float x = stores[index, 0];
                float y = stores[index, 1];
                float z = stores[index, 2];
                float width = stores[index, 3];
                float depth = stores[index, 4];
                Box(
                    "Painted-T90M-BustleStoreTray",
                    root,
                    V(x, y - 0.035f, z),
                    V(width + 0.08f, 0.055f, depth + 0.08f),
                    color * 0.56f);
                Box(
                    "Painted-T90M-BustleTopStore",
                    root,
                    V(x, y + 0.035f, z),
                    V(width, 0.11f, depth),
                    color * 0.52f);
                Box(
                    "T90M-BustleTopStoreLatch",
                    root,
                    V(x, y + 0.096f, z + depth * 0.34f),
                    V(width * 0.76f, 0.012f, 0.030f),
                    Dark());
            }
            Cylinder(
                "T90M-BustleCanvasRoll",
                root,
                V(0.10f, 0.84f, -2.13f),
                0.105f,
                0.90f,
                14,
                Canvas());
            foreach (float x in new[] { -0.22f, 0.10f, 0.42f })
            {
                Box(
                    "T90M-BustleCanvasStrap",
                    root,
                    V(x, 0.84f, -2.13f),
                    V(0.035f, 0.23f, 0.18f),
                    Dark());
            }
        }

        private static void Slab(
            string name,
            Transform root,
            int side,
            Vector3 b0,
            Vector3 b1,
            Vector3 b2,
            Vector3 b3,
            Vector3 t0,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3,
            Color color)
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

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radius,
                radius,
                length,
                segments,
                TankShapeAxis.X,
                color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 M(Vector3 point, int side)
        {
            point.x *= side;
            return point;
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

        private static Color Detail()
        {
            return new Color(0.28f, 0.30f, 0.24f);
        }

        private static Color Canvas()
        {
            return new Color(0.259f, 0.271f, 0.184f);
        }
    }
}
