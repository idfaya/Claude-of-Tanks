using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMSternDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddTailRacks(root, color);
            AddCommonStowage(root, color);
            AddScallopedCurtain(root, color);
            AddRearQuarterCage(root, color);
            AddRearServiceField(root);
            AddCornerFlaps(root, color);
        }

        private static void AddTailRacks(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                TankShapeFactory.OrientedSlabPart(
                    "Painted-T90SM-InnerTailRack",
                    root,
                    V(side * 0.345f, 0.76f, -2.92f),
                    V(side * 0.415f, 0.76f, -2.92f),
                    V(side * 0.415f, 1.02f, -3.18f),
                    V(side * 0.345f, 1.02f, -3.18f),
                    V(side * 0.345f, 1.375f, -2.92f),
                    V(side * 0.415f, 1.375f, -2.92f),
                    V(side * 0.415f, 1.375f, -3.18f),
                    V(side * 0.345f, 1.375f, -3.18f),
                    color * 0.52f);
                Box("Painted-T90SM-InnerRackWeb", root,
                    V(side * 0.38f, 1.0675f, -2.995f),
                    V(0.07f, 0.615f, 0.11f), color * 0.52f);
                Box("Painted-T90SM-InnerRackBody", root,
                    V(side * 0.38f, 1.21f, -3.20f),
                    V(0.07f, 0.34f, 0.22f), color * 0.52f);
                TankShapeFactory.OrientedSlabPart(
                    "Painted-T90SM-OuterTailRack",
                    root,
                    V(side * 0.80f, 0.76f, -2.885f),
                    V(side * 0.87f, 0.76f, -2.885f),
                    V(side * 0.87f, 1.02f, -3.17f),
                    V(side * 0.80f, 1.02f, -3.17f),
                    V(side * 0.80f, 1.38f, -2.885f),
                    V(side * 0.87f, 1.38f, -2.885f),
                    V(side * 0.87f, 1.38f, -3.17f),
                    V(side * 0.80f, 1.38f, -3.17f),
                    color * 0.52f);
                Box("Painted-T90SM-OuterRackBody", root,
                    V(side * 0.835f, 1.24f, -3.28f),
                    V(0.07f, 0.28f, 0.20f), color * 0.52f);
                Box("Painted-T90SM-OuterRackToe", root,
                    V(side * 0.835f, 1.045f, -3.41f),
                    V(0.07f, 0.29f, 0.05f), color * 0.52f);
                Box("T90SM-OuterRackEndPlate", root,
                    V(side * 0.835f, 1.15f, -3.41f),
                    V(0.06f, 0.06f, 0.022f), Dark());
                Box("Painted-T90SM-OuterRackPair", root,
                    V(side * 1.1325f, 1.17f, -3.06f),
                    V(0.145f, 0.32f, 0.20f), color * 0.52f);
                Box("Painted-T90SM-CornerBin", root,
                    V(side * 1.30f, 1.12f, -3.05f),
                    V(0.26f, 0.40f, 0.20f), color * 0.52f);
                Box("T90SM-CornerBinLid", root,
                    V(side * 1.30f, 1.309f, -3.03f),
                    V(0.20f, 0.022f, 0.18f), Dark());
            }
        }

        private static void AddCommonStowage(
            Transform root,
            Color color)
        {
            Box("Painted-T90SM-RearStowage", root,
                V(0.20f, 1.30f, -2.72f),
                V(1.53f, 0.10f, 0.38f), color * 0.56f);
            Cylinder("T90SM-UnditchingLog", root,
                V(0f, 1.28f, -2.90f),
                0.08f,
                0.08f,
                0.90f,
                14,
                TankShapeAxis.X,
                Wood());
            foreach (float x in new[] { -0.24f, 0.24f })
            {
                Box("T90SM-LogStrap", root,
                    V(x, 1.28f, -2.90f),
                    V(0.045f, 0.18f, 0.025f), Dark());
            }
            for (int index = 0; index < 4; index++)
            {
                Box("T90SM-SpareTrackLink", root,
                    V(0.32f + index * 0.20f, 1.425f, 0.60f),
                    V(0.17f, 0.045f, 0.25f), Track());
            }
        }

        private static void AddScallopedCurtain(
            Transform root,
            Color color)
        {
            const float z0 = -1.10f;
            const float depth = 0.77f;
            for (int side = -1; side <= 1; side += 2)
            {
                float innerX = side * 1.675f;
                float outerX = side * 1.705f;
                for (int index = 0; index < 5; index++)
                {
                    float a = z0 + index * depth;
                    float middle = a + depth * 0.5f;
                    float b = a + depth;
                    float edgeY =
                        index == 0 || index == 4
                            ? 0.68f
                            : 0.72f;
                    float lobeY =
                        0.43f + (index % 2) * 0.035f;
                    TankShapeFactory.OrientedSlabPart(
                        "Painted-T90SM-ScallopedSkirt",
                        root,
                        V(innerX, edgeY, a),
                        V(outerX, edgeY, a),
                        V(outerX, lobeY, middle),
                        V(innerX, lobeY, middle),
                        V(innerX, 1.21f, a),
                        V(outerX, 1.21f, a),
                        V(outerX, 1.21f, middle),
                        V(innerX, 1.21f, middle),
                        color * 0.48f);
                    TankShapeFactory.OrientedSlabPart(
                        "Painted-T90SM-ScallopedSkirt",
                        root,
                        V(innerX, lobeY, middle),
                        V(outerX, lobeY, middle),
                        V(outerX, edgeY, b),
                        V(innerX, edgeY, b),
                        V(innerX, 1.21f, middle),
                        V(outerX, 1.21f, middle),
                        V(outerX, 1.21f, b),
                        V(innerX, 1.21f, b),
                        color * 0.48f);
                    Box("T90SM-ScallopedSkirtBatten", root,
                        V(outerX, 0.98f, b - 0.012f),
                        V(0.026f, 0.44f, 0.024f),
                        Dark());
                }
            }
        }

        private static void AddRearQuarterCage(
            Transform root,
            Color color)
        {
            const float rearZ = -2.72f;
            const float frontZ = -1.02f;
            const float bottomY = 0.60f;
            const float topY = 1.24f;
            const float outerX = 1.85f;
            const float innerX = 1.68f;
            const float depth = frontZ - rearZ;
            const float height = topY - bottomY;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 6; index++)
                {
                    float y =
                        bottomY + height * index / 5f;
                    Box("Painted-T90SM-CageRail", root,
                        V(side * outerX, y, (rearZ + frontZ) * 0.5f),
                        V(0.044f, 0.038f, depth),
                        color * 0.46f);
                }
                for (int index = 0; index < 7; index++)
                {
                    float z = rearZ + depth * index / 6f;
                    Box("Painted-T90SM-CageStile", root,
                        V(side * outerX, (bottomY + topY) * 0.5f, z),
                        V(0.048f, height + 0.04f, 0.042f),
                        color * 0.46f);
                }
                for (int index = 0; index < 4; index++)
                {
                    float z =
                        rearZ + depth * (index + 0.5f) / 4f;
                    foreach (float y in new[]
                    {
                        bottomY + 0.09f,
                        topY - 0.09f
                    })
                    {
                        Box("Painted-T90SM-CageStub", root,
                            V(side * 1.8175f, y, z),
                            V(0.065f, 0.052f, 0.065f),
                            color * 0.46f);
                    }
                    Box("Painted-T90SM-CagePost", root,
                        V(side * innerX, 0.92f, z),
                        V(0.052f, height * 0.62f, 0.070f),
                        color * 0.46f);
                }
                Box("Painted-T90SM-CageAnchor", root,
                    V(side * 1.765f, topY - 0.035f, frontZ - 0.015f),
                    V(0.25f, 0.09f, 0.11f),
                    color * 0.48f);
            }
        }

        private static void AddRearServiceField(Transform root)
        {
            Box("T90SM-RearServiceField", root,
                V(0f, 1.16f, -2.934f),
                V(1.50f, 0.42f, 0.026f), Dark());
            float[,] bays =
            {
                { -0.52f, 0.42f, 4f },
                { 0.08f, 0.58f, 6f },
                { 0.58f, 0.30f, 3f }
            };
            for (int bay = 0; bay < bays.GetLength(0); bay++)
            {
                int count = Mathf.RoundToInt(bays[bay, 2]);
                for (int index = 0; index < count; index++)
                {
                    float x = bays[bay, 0] -
                        bays[bay, 1] * 0.5f +
                        (index + 0.5f) *
                        bays[bay, 1] / count;
                    Box("T90SM-RearServiceLouvre", root,
                        V(x, 1.16f, -2.952f),
                        V(0.028f, 0.34f, 0.030f), Detail());
                }
            }
            Box("T90SM-RearPipeRail", root,
                V(0f, 0.98f, -2.96f),
                V(1.38f, 0.036f, 0.040f), Detail());
            for (int side = -1; side <= 1; side += 2)
            {
                Transform eye = TankShapeFactory.TorusPart(
                    "T90SM-RearTowEye",
                    root,
                    0.085f,
                    0.020f,
                    14,
                    Detail());
                eye.localPosition =
                    V(side * 0.48f, 0.90f, -2.97f);
                eye.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Box("T90SM-RearTowEyePost", root,
                    V(side * 0.48f, 0.99f, -2.95f),
                    V(0.045f, 0.18f, 0.045f), Dark());
            }
        }

        private static void AddCornerFlaps(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T90SM-RearMudFlap", root,
                    V(side * 1.475f, 1.04f, -3.055f),
                    V(0.44f, 0.19f, 0.19f), Rubber());
                Box("Painted-T90SM-RearFlapBracket", root,
                    V(side * 1.44f, 1.10f, -3.04f),
                    V(0.05f, 0.16f, 0.16f), color * 0.50f);
            }
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

        private static Color Rubber()
        {
            return new Color(0.23f, 0.227f, 0.20f);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }

        private static Color Wood()
        {
            return new Color(0.278f, 0.243f, 0.196f);
        }
    }
}
