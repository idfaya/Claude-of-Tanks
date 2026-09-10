using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSternDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddUpperSkirtBand(root, color);
            AddSegmentedCurtain(root, color);
            AddMudguards(root, color);
            AddPrimaryTransom(root, color);
            AddFinalTransom(root, color);
        }

        private static void AddUpperSkirtBand(
            Transform root,
            Color color)
        {
            const int panels = 6;
            const float z0 = -2.84f;
            const float z1 = 2.81f;
            const float depth = (z1 - z0) / panels;
            for (int side = -1; side <= 1; side += 2)
            for (int panel = 0; panel < panels; panel++)
            {
                float z = z0 + depth * (panel + 0.5f);
                Box(
                    "Painted-T90M-UpperSkirtPanel",
                    root,
                    V(side * 1.80f, 1.255f, z),
                    V(0.075f, 0.35f, depth * 0.94f),
                    color * 0.48f);
                Box(
                    "T90M-UpperSkirtBatten",
                    root,
                    V(side * 1.723f, 1.255f, z + depth * 0.5f),
                    V(0.048f, 0.315f, 0.020f),
                    Dark());
                Cylinder(
                    "T90M-UpperSkirtBolt",
                    root,
                    V(side * 1.735f, 1.36f, z),
                    0.014f,
                    0.014f,
                    8,
                    TankShapeAxis.X,
                    Dark());
                Box(
                    "T90M-UpperSkirtBottomLip",
                    root,
                    V(side * 1.76f, 1.10f, z),
                    V(0.042f, 0.09f, depth * 0.92f),
                    Dark());
            }
        }

        private static void AddSegmentedCurtain(
            Transform root,
            Color color)
        {
            const int panels = 7;
            const float z0 = -2.50f;
            const float z1 = 2.50f;
            const float depth = (z1 - z0) / panels;
            for (int side = -1; side <= 1; side += 2)
            for (int panel = 0; panel < panels; panel++)
            {
                float a = z0 + panel * depth;
                float middle = a + depth * 0.5f;
                float b = a + depth;
                float edgeY =
                    (panel == 0 || panel == panels - 1)
                        ? 0.82f
                        : 0.86f;
                float lobeY =
                    0.63f + (panel % 2) * 0.035f;
                Slab(
                    "Painted-T90M-SkirtCurtain",
                    root,
                    side,
                    V(1.770f, edgeY, a),
                    V(1.825f, edgeY, a),
                    V(1.825f, lobeY, middle),
                    V(1.770f, lobeY, middle),
                    V(1.770f, 1.405f, a),
                    V(1.825f, 1.405f, a),
                    V(1.825f, 1.405f, middle),
                    V(1.770f, 1.405f, middle),
                    color * 0.48f);
                Slab(
                    "Painted-T90M-SkirtCurtain",
                    root,
                    side,
                    V(1.770f, lobeY, middle),
                    V(1.825f, lobeY, middle),
                    V(1.825f, edgeY, b),
                    V(1.770f, edgeY, b),
                    V(1.770f, 1.405f, middle),
                    V(1.825f, 1.405f, middle),
                    V(1.825f, 1.405f, b),
                    V(1.770f, 1.405f, b),
                    color * 0.48f);
                Box(
                    "T90M-SkirtCurtainBatten",
                    root,
                    V(side * 1.825f, 1.13f, b - 0.012f),
                    V(0.026f, 0.50f, 0.024f),
                    Dark());
                Box(
                    "T90M-SkirtCurtainTopRidge",
                    root,
                    V(side * 1.825f, 1.425f, middle),
                    V(0.032f, 0.035f, depth * 0.68f),
                    Detail());
            }
        }

        private static void AddMudguards(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                TankMudguardShapeFactory.Build(
                    "Painted-T90M-FrontMudguard",
                    root,
                    V(side * 1.64f, 0.97f, 3.20f),
                    Quaternion.Euler(
                        0f,
                        (-side * 0.20f + Mathf.PI * 0.5f) *
                        Mathf.Rad2Deg,
                        0f),
                    0.055f,
                    0.34f,
                    0.48f,
                    color * 0.52f,
                    Dark(),
                    0.025f,
                    0.04f,
                    0.02f,
                    supportName:
                        "Painted-T90M-FrontMudguardSupport");
                TankMudguardShapeFactory.Build(
                    "Painted-T90M-RearMudguard",
                    root,
                    V(side * 1.64f, 0.94f, -3.02f),
                    Quaternion.Euler(
                        0f,
                        (side * 0.16f + Mathf.PI * 0.5f) *
                        Mathf.Rad2Deg,
                        0f),
                    0.055f,
                    0.34f,
                    0.58f,
                    color * 0.52f,
                    Dark(),
                    0.025f,
                    0.02f,
                    0.05f,
                    supportName:
                        "Painted-T90M-RearMudguardSupport");
            }
        }

        private static void AddPrimaryTransom(
            Transform root,
            Color color)
        {
            Box(
                "Painted-T90M-RearPlate",
                root,
                V(0f, 1.09f, -3.29f),
                V(3.02f, 0.60f, 0.095f),
                color * 0.55f);
            Box(
                "T90M-RearPlateBacking",
                root,
                V(0f, 1.29f, -3.346f),
                V(2.82f, 0.20f, 0.020f),
                Dark());
            float[,] bays =
            {
                { -0.86f, 0.86f, 5f },
                { 0.10f, 0.72f, 4f },
                { 0.84f, 0.48f, 3f }
            };
            for (int bay = 0; bay < bays.GetLength(0); bay++)
            {
                float x = bays[bay, 0];
                float width = bays[bay, 1];
                int count = (int)bays[bay, 2];
                Box(
                    "T90M-RearServiceBay",
                    root,
                    V(x, 1.32f, -3.352f),
                    V(width, 0.24f, 0.028f),
                    Dark());
                for (int index = 0; index < count; index++)
                {
                    float y = 1.23f +
                        index * (0.18f / Mathf.Max(1, count - 1));
                    Box(
                        "Painted-T90M-RearServiceLouvre",
                        root,
                        V(x, y, -3.372f),
                        V(width * 0.82f, 0.018f, 0.015f),
                        color * 0.48f);
                }
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder(
                    "Painted-T90M-RearFuelDrum",
                    root,
                    V(side * 0.62f, 1.62f, -3.44f),
                    0.20f,
                    0.72f,
                    14,
                    TankShapeAxis.X,
                    color * 0.44f);
                foreach (float x in new[]
                {
                    side * 0.35f,
                    side * 0.66f,
                    side * 0.92f
                })
                {
                    Box(
                        "T90M-RearFuelDrumStrap",
                        root,
                        V(x, 1.62f, -3.44f),
                        V(0.035f, 0.26f, 0.30f),
                        Dark());
                }
                Box(
                    "T90M-RearFuelDrumCradle",
                    root,
                    V(side * 0.62f, 1.475f, -3.39f),
                    V(0.78f, 0.055f, 0.24f),
                    Dark());
                Torus(
                    "T90M-RearTowEye",
                    root,
                    V(side * 0.82f, 0.78f, -3.36f),
                    0.095f,
                    0.020f,
                    14,
                    Dark());
                Box(
                    "T90M-RearLampHousing",
                    root,
                    V(side * 1.27f, 1.16f, -3.36f),
                    V(0.18f, 0.12f, 0.035f),
                    Detail());
                Box(
                    "T90M-RearLampLens",
                    root,
                    V(side * 1.27f, 1.19f, -3.388f),
                    V(0.10f, 0.07f, 0.010f),
                    Glass());
            }
            Cylinder(
                "T90M-UnditchingLog",
                root,
                V(0f, 0.95f, -3.42f),
                0.105f,
                1.48f,
                14,
                TankShapeAxis.X,
                Wood());
            foreach (float x in new[]
            {
                -1.05f, -0.50f, 0.05f, 0.60f, 1.15f
            })
            {
                Box(
                    "T90M-UnditchingLogStrap",
                    root,
                    V(x, 0.95f, -3.42f),
                    V(0.045f, 0.25f, 0.24f),
                    Dark());
            }
            TankFittingShapeFactory.BuildTowCable(
                "T90M-RearTowCable",
                root,
                new[]
                {
                    V(-1.02f, 0.78f, -3.39f),
                    V(-0.52f, 0.64f, -3.43f),
                    V(0f, 0.59f, -3.44f),
                    V(0.52f, 0.64f, -3.43f),
                    V(1.02f, 0.78f, -3.39f)
                },
                0.018f,
                24,
                6,
                Dark());
        }

        private static void AddFinalTransom(
            Transform root,
            Color color)
        {
            float[,] bays =
            {
                { -0.78f, 0.70f, 5f },
                { 0.05f, 0.56f, 4f },
                { 0.70f, 0.38f, 3f }
            };
            for (int bay = 0; bay < bays.GetLength(0); bay++)
            {
                float x = bays[bay, 0];
                float width = bays[bay, 1];
                int count = (int)bays[bay, 2];
                Box(
                    "T90M-FinalRearBay",
                    root,
                    V(x, 1.10f, -3.085f),
                    V(width, 0.26f, 0.018f),
                    Dark());
                for (int index = 0; index < count; index++)
                {
                    float y = 1.00f +
                        index * (0.20f / Mathf.Max(1, count - 1));
                    Box(
                        "Painted-T90M-FinalRearLouvre",
                        root,
                        V(x, y, -3.104f),
                        V(width * 0.82f, 0.018f, 0.014f),
                        color * 0.48f);
                }
            }
            Box(
                "T90M-RearExhaustBridge",
                root,
                V(0f, 0.92f, -3.11f),
                V(1.46f, 0.038f, 0.038f),
                Detail());
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "T90M-RearRecoveryBrace",
                    root,
                    V(side * 1.18f, 1.02f, -3.10f),
                    V(0.045f, 0.25f, 0.040f),
                    Detail(),
                    Quaternion.Euler(
                        0f,
                        0f,
                        -side * 0.16f * Mathf.Rad2Deg));
                Box(
                    "T90M-RearRecoveryBlock",
                    root,
                    V(side * 1.28f, 1.29f, -3.10f),
                    V(0.22f, 0.13f, 0.040f),
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
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radius,
                radius,
                length,
                segments,
                axis,
                color);
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
            Color color)
        {
            Transform part = TankShapeFactory.TorusPart(
                name,
                parent,
                radius,
                tubeRadius,
                segments,
                color);
            part.localPosition = position;
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
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

        private static Color Glass()
        {
            return new Color(0.10f, 0.17f, 0.18f);
        }

        private static Color Wood()
        {
            return new Color(0.278f, 0.243f, 0.196f);
        }
    }
}
