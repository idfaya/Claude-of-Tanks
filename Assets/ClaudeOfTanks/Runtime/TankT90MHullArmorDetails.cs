using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MHullArmorDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddBaseGlacisRelikt(root, color);
            AddDeck(root, color);
            AddFrontFittings(root, color);
            AddLowerGlacisRelikt(root, color);
            // The final TS owner invokes this pass twice; preserve that
            // generated geometry rather than silently changing the source.
            TankT90MBowArmorDetails.Build(root, color);
            TankT90MBowArmorDetails.Build(root, color);
        }

        private static void AddBaseGlacisRelikt(
            Transform root,
            Color color)
        {
            float[,] rows =
            {
                { 0.34f, 1.435f, 2.38f, 0.20f, 0.58f, 0.40f },
                { 0.78f, 1.405f, 2.50f, 0.34f, 0.54f, 0.36f },
                { 1.18f, 1.365f, 2.58f, 0.48f, 0.46f, 0.31f }
            };
            for (int side = -1; side <= 1; side += 2)
            for (int index = 0; index < rows.GetLength(0); index++)
            {
                float x = rows[index, 0];
                float y = rows[index, 1];
                float z = rows[index, 2];
                float yaw = rows[index, 3];
                float width = rows[index, 4];
                float depth = rows[index, 5];
                Quaternion rotation = Quaternion.Euler(
                    -0.34f * Mathf.Rad2Deg,
                    -side * yaw * Mathf.Rad2Deg,
                    0f);
                Box(
                    "Painted-T90M-GlacisRelikt",
                    root,
                    V(side * x, y, z),
                    V(width, 0.075f, depth),
                    color * 0.54f,
                    rotation);
                Box(
                    "T90M-GlacisReliktSeam",
                    root,
                    V(side * x, y + 0.045f, z - depth * 0.34f),
                    V(width * 0.76f, 0.012f, 0.025f),
                    Dark(),
                    rotation);
            }
        }

        private static void AddDeck(
            Transform root,
            Color color)
        {
            Box(
                "Painted-T90M-DriverHatch",
                root,
                V(0f, 1.515f, 1.58f),
                V(0.86f, 0.055f, 0.72f),
                color * 0.60f);
            Box(
                "T90M-DriverHatchSeam",
                root,
                V(0f, 1.55f, 1.84f),
                V(0.72f, 0.014f, 0.055f),
                Dark());
            foreach (float x in new[] { -0.28f, 0f, 0.28f })
            {
                Box(
                    "T90M-DriverPeriscope",
                    root,
                    V(x, 1.56f, 1.93f),
                    V(0.17f, 0.055f, 0.075f),
                    Dark());
                Box(
                    "T90M-DriverPeriscopeLens",
                    root,
                    V(x, 1.575f, 1.972f),
                    V(0.12f, 0.026f, 0.010f),
                    Glass());
            }
            float[,] grilles =
            {
                { -0.82f, -1.58f, 0.84f, 1.06f },
                { 0.18f, -1.58f, 0.84f, 1.06f },
                { 0.82f, -2.46f, 0.72f, 0.55f },
                { -0.12f, -2.46f, 0.88f, 0.55f }
            };
            for (int index = 0;
                index < grilles.GetLength(0);
                index++)
            {
                float x = grilles[index, 0];
                float z = grilles[index, 1];
                float width = grilles[index, 2];
                float depth = grilles[index, 3];
                Box(
                    "T90M-EngineGrilleBacking",
                    root,
                    V(x, 1.52f, z),
                    V(width, 0.040f, depth),
                    Dark());
                for (int rib = -2; rib <= 2; rib++)
                {
                    Box(
                        "T90M-EngineGrilleRib",
                        root,
                        V(
                            x,
                            1.545f,
                            z + rib * depth * 0.15f),
                        V(width * 0.88f, 0.012f, 0.020f),
                        Detail());
                }
            }
        }

        private static void AddFrontFittings(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Quaternion rotation = Quaternion.Euler(
                    -0.18f * Mathf.Rad2Deg,
                    -side * 0.18f * Mathf.Rad2Deg,
                    0f);
                Box(
                    "Painted-T90M-LampCassette",
                    root,
                    V(side * 1.40f, 1.45f, 2.68f),
                    V(0.34f, 0.18f, 0.25f),
                    color * 0.56f,
                    rotation);
                foreach (float dx in new[] { -0.075f, 0.075f })
                {
                    Cylinder(
                        "T90M-HeadlightLens",
                        root,
                        V(side * 1.40f + dx, 1.52f, 2.76f),
                        0.047f,
                        0.052f,
                        0.025f,
                        10,
                        TankShapeAxis.Y,
                        Glass());
                }
                Box(
                    "T90M-LampGuard",
                    root,
                    V(side * 1.40f, 1.55f, 2.66f),
                    V(0.38f, 0.025f, 0.30f),
                    Dark(),
                    rotation);
                Torus(
                    "T90M-FrontTowEye",
                    root,
                    V(side * 0.73f, 0.82f, 3.09f),
                    0.105f,
                    0.024f,
                    14,
                    Dark(),
                    Quaternion.Euler(90f, 0f, 0f));
            }
        }

        private static void AddLowerGlacisRelikt(
            Transform root,
            Color color)
        {
            float[,] rows =
            {
                { 0.32f, 1.15f, 2.78f, 0.16f, 0.52f, 0.32f },
                { 0.72f, 1.09f, 2.91f, 0.28f, 0.44f, 0.28f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < rows.GetLength(0);
                    index++)
                {
                    float x = rows[index, 0];
                    float y = rows[index, 1];
                    float z = rows[index, 2];
                    float yaw = rows[index, 3];
                    float width = rows[index, 4];
                    float depth = rows[index, 5];
                    Quaternion rotation = Quaternion.Euler(
                        -0.47f * Mathf.Rad2Deg,
                        -side * yaw * Mathf.Rad2Deg,
                        0f);
                    Box(
                        "Painted-T90M-LowerGlacisRelikt",
                        root,
                        V(side * x, y, z),
                        V(width, 0.060f, depth),
                        color * 0.52f,
                        rotation);
                    Box(
                        "T90M-LowerGlacisReliktSeam",
                        root,
                        V(
                            side * x,
                            y + 0.037f,
                            z - depth * 0.33f),
                        V(width * 0.72f, 0.010f, 0.026f),
                        Dark(),
                        rotation);
                }
                Box(
                    "T90M-BowRecoveryBrace",
                    root,
                    V(side * 0.96f, 0.94f, 3.02f),
                    V(0.038f, 0.20f, 0.42f),
                    Detail(),
                    Quaternion.Euler(
                        -0.40f * Mathf.Rad2Deg,
                        0f,
                        0f));
                Torus(
                    "T90M-LowerTowEye",
                    root,
                    V(side * 0.70f, 0.77f, 3.09f),
                    0.096f,
                    0.022f,
                    16,
                    Dark(),
                    Quaternion.Euler(90f, 0f, 0f));
            }
            Box(
                "T90M-LowerGlacisCenterBar",
                root,
                V(0f, 0.94f, 3.055f),
                V(0.62f, 0.045f, 0.065f),
                Dark(),
                Quaternion.Euler(
                    -0.48f * Mathf.Rad2Deg,
                    0f,
                    0f));
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
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Torus(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float tubeRadius,
            int segments,
            Color color,
            Quaternion rotation)
        {
            Transform part = TankShapeFactory.TorusPart(
                name, parent, radius, tubeRadius, segments, color);
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

        private static Color Detail()
        {
            return new Color(0.28f, 0.30f, 0.24f);
        }

        private static Color Glass()
        {
            return new Color(0.10f, 0.17f, 0.18f);
        }
    }
}
