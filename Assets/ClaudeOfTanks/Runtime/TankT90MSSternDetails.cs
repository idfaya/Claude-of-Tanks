using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSSternDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddTailRack(root, color);
            AddFuelDrums(root, color);
            AddSkirtRelikt(root, color);
            AddRearCage(root);
            AddServiceBays(root, color);
            AddSkirtBand(root, color);
            AddMudFlaps(root, color);
        }

        private static void AddTailRack(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90MS-TailRack", root,
                    V(side * 0.60f, 1.50f, -3.26f),
                    V(0.98f, 0.30f, 0.30f), color * 0.54f);
                Box("T90MS-TailRackSliver", root,
                    V(side * 0.60f, 1.652f, -3.14f),
                    V(0.90f, 0.014f, 0.03f), Dark());
                Box("Painted-T90MS-TailOuterRack", root,
                    V(side * 1.42f, 1.44f, -3.24f),
                    V(0.30f, 0.26f, 0.26f), color * 0.54f);
                Box("Painted-T90MS-TailRackFiller", root,
                    V(side * 1.18f, 1.46f, -3.26f),
                    V(0.20f, 0.16f, 0.26f), color * 0.54f);
                Box("Painted-T90MS-TailCornerBracket", root,
                    V(side * 1.53f, 1.21f, -3.05f),
                    V(0.16f, 0.20f, 0.18f), color * 0.54f);
                Box("Painted-T90MS-TailCap", root,
                    V(side * 0.60f, 1.235f, -3.50f),
                    V(0.07f, 0.14f, 0.16f), color * 0.54f);
                Box("T90MS-TailCapFace", root,
                    V(side * 0.60f, 1.235f, -3.49f),
                    V(0.05f, 0.10f, 0.03f), Dark());
            }
        }

        private static void AddFuelDrums(
            Transform root,
            Color color)
        {
            AddFuelDrum(root, color, -0.64f, 0.82f, 0.150f);
            AddFuelDrum(root, color, 0.57f, 0.70f, 0.135f);
        }

        private static void AddFuelDrum(
            Transform root,
            Color color,
            float x,
            float length,
            float radius)
        {
            Cylinder("Painted-T90MS-RearFuelDrum", root,
                V(x, 1.49f, -3.40f),
                radius, radius, length, 14,
                TankShapeAxis.X, color * 0.50f);
            foreach (float offset in new[] { -0.28f, 0.28f })
            {
                Cylinder("T90MS-RearFuelDrumBand", root,
                    V(x + offset * length, 1.49f, -3.40f),
                    radius + 0.009f,
                    radius + 0.009f,
                    0.035f,
                    14,
                    TankShapeAxis.X,
                    Dark());
            }
        }

        private static void AddSkirtRelikt(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 3; index++)
                {
                    float z = 2.07f - index * 1.33f;
                    Box("Painted-T90MS-SkirtRelikt", root,
                        V(side * 1.76f, 1.17f, z),
                        V(0.06f, 0.24f, 1.26f), color * 0.52f);
                    Box("T90MS-SkirtReliktEndSeam", root,
                        V(side * 1.7675f, 1.17f, z - 0.63f),
                        V(0.045f, 0.19f, 0.03f), Dark());
                    foreach (float offset in new[]
                    {
                        -0.32f, 0f, 0.32f
                    })
                    {
                        Box("T90MS-SkirtReliktFaceSeam", root,
                            V(
                                side * 1.7675f,
                                1.07f + (offset + 0.32f) * 0.14f,
                                z + offset * 0.2f),
                            V(0.045f, 0.026f, 1.20f),
                            Dark());
                    }
                    Box("T90MS-SkirtReliktRubberHem", root,
                        V(side * 1.75f, 0.98f, z),
                        V(0.04f, 0.08f, 1.24f), Rubber());
                }
            }
        }

        private static void AddRearCage(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 5; row++)
                {
                    Box("T90MS-FlankCageRail", root,
                        V(side * 1.868f, 0.60f + row * 0.17f, -2.12f),
                        V(0.028f, 0.028f, 1.55f), Dark());
                }
                for (int column = 0; column < 6; column++)
                {
                    Box("T90MS-FlankCagePost", root,
                        V(side * 1.866f, 0.94f, -1.40f - column * 0.29f),
                        V(0.024f, 0.72f, 0.024f), Dark());
                }
                foreach (float z in new[]
                {
                    -1.50f, -2.10f, -2.70f
                })
                {
                    Box("T90MS-FlankCageBracket", root,
                        V(side * 1.828f, 0.94f, z),
                        V(0.10f, 0.05f, 0.05f), Dark());
                }
            }
            for (int row = 0; row < 5; row++)
            {
                Box("T90MS-TransomCageRail", root,
                    V(0f, 0.60f + row * 0.16f, -3.455f),
                    V(3.40f, 0.028f, 0.028f), Dark());
            }
            foreach (float x in new[]
            {
                -1.62f, -1.25f, -0.75f, -0.25f,
                0.25f, 0.75f, 1.25f, 1.62f
            })
            {
                Box("T90MS-TransomCagePost", root,
                    V(x, 0.92f, -3.45f),
                    V(0.024f, 0.68f, 0.024f), Dark());
            }
        }

        private static void AddServiceBays(
            Transform root,
            Color color)
        {
            float[,] bays =
            {
                { -1.20f, 0.34f, 0.19f, 1.11f, 3f },
                { -0.69f, 0.48f, 0.25f, 1.01f, 4f },
                { -0.12f, 0.30f, 0.17f, 1.14f, 3f },
                { 0.48f, 0.44f, 0.22f, 1.03f, 4f }
            };
            for (int bay = 0; bay < bays.GetLength(0); bay++)
            {
                float x = bays[bay, 0];
                float width = bays[bay, 1];
                float height = bays[bay, 2];
                float y = bays[bay, 3];
                int count = Mathf.RoundToInt(bays[bay, 4]);
                Box("T90MS-ServiceBayBacking", root,
                    V(x, y, -3.425f),
                    V(width, height, 0.025f), Dark());
                for (int index = 0; index < count; index++)
                {
                    float louvreX =
                        x - width * 0.36f +
                        index * width * 0.72f /
                        Mathf.Max(1, count - 1);
                    Box("Painted-T90MS-ServiceLouvre", root,
                        V(louvreX, y, -3.445f),
                        V(0.025f, height * 0.62f, 0.018f),
                        color * 0.54f);
                }
                Box("Painted-T90MS-ServiceBayEdge", root,
                    V(x + width * 0.42f, y, -3.448f),
                    V(0.035f, height * 0.80f, 0.020f),
                    color * 0.54f);
            }
            Box("Painted-T90MS-RearLowFitting", root,
                V(-1.18f, 0.72f, -3.46f),
                V(0.34f, 0.12f, 0.05f), color * 0.54f);
            Box("T90MS-RearLowFitting", root,
                V(1.20f, 0.80f, -3.46f),
                V(0.24f, 0.09f, 0.052f), Dark());
            AddRearRoundFittings(root, color);
            Box("T90MS-RearServicePost", root,
                V(1.34f, 0.78f, -3.46f),
                V(0.055f, 0.30f, 0.050f), Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T90MS-RearCageStay", root,
                    V(side * 1.30f, 0.95f, -3.21f),
                    V(0.03f, 0.03f, 0.50f), Dark());
                Box("Painted-T90MS-WidthAnchor", root,
                    V(side * 1.884f, 0.95f, -1.60f),
                    V(0.012f, 0.02f, 0.02f), color * 0.54f);
            }
        }

        private static void AddRearRoundFittings(
            Transform root,
            Color color)
        {
            Cylinder("T90MS-RearRoundHousing", root,
                V(1.06f, 1.13f, -3.47f),
                0.11f, 0.11f, 0.048f, 14,
                TankShapeAxis.Z, Dark());
            Cylinder("Painted-T90MS-RearRoundCap", root,
                V(1.06f, 1.13f, -3.502f),
                0.068f, 0.068f, 0.050f, 14,
                TankShapeAxis.Z, color * 0.54f);
            foreach (float x in new[] { -0.58f, 0.52f })
            {
                Cylinder("T90MS-RearTowHousing", root,
                    V(x, 0.69f, -3.48f),
                    0.085f, 0.085f, 0.045f, 12,
                    TankShapeAxis.Z, Dark());
                Cylinder("Painted-T90MS-RearTowCap", root,
                    V(x, 0.69f, -3.505f),
                    0.052f, 0.052f, 0.048f, 12,
                    TankShapeAxis.Z, color * 0.54f);
            }
        }

        private static void AddSkirtBand(
            Transform root,
            Color color)
        {
            const int panels = 7;
            const float start = -2.72f;
            const float end = 2.82f;
            const float depth = (end - start) / panels;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < panels; index++)
                {
                    float z = start + depth * (index + 0.5f);
                    Box("Painted-T90MS-SkirtPanel", root,
                        V(side * 1.7675f, 1.14f, z),
                        V(0.036f, 0.28f, depth * 0.94f),
                        color * 0.48f);
                    Box("T90MS-SkirtBatten", root,
                        V(side * 1.7705f, 1.14f, z + depth * 0.5f),
                        V(0.048f, 0.252f, 0.02f), Dark());
                    Transform bolt = Cylinder(
                        "T90MS-SkirtBolt",
                        root,
                        V(side * 1.7825f, 1.21f, z),
                        0.014f, 0.014f, 0.014f, 8,
                        TankShapeAxis.Z, Dark());
                    bolt.localRotation =
                        Quaternion.Euler(0f, side * 90f, 0f);
                    Box("T90MS-SkirtBottomLip", root,
                        V(side * 1.755f, 0.97f, z),
                        V(0.042f, 0.09f, depth * 0.92f),
                        Dark());
                }
            }
        }

        private static void AddMudFlaps(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T90MS-RearMudFlap", root,
                    V(side * 1.52f, 0.80f, -3.06f),
                    V(0.36f, 0.30f, 0.05f), Rubber());
                TankMudguardShapeFactory.Build(
                    "T90MS-FrontMudFlap",
                    root,
                    V(side * 1.53f, 0.73f, 3.345f),
                    Quaternion.Euler(0f, 90f, 0f),
                    0.05f,
                    0.40f,
                    0.72f,
                    Rubber(),
                    color * 0.54f,
                    0.018f,
                    0.075f,
                    supportName:
                        "Painted-T90MS-FrontMudFlapSupport");
                Box("T90MS-FrontMudFlapRidge", root,
                    V(side * 1.53f, 1.075f, 3.345f),
                    V(0.38f, 0.035f, 0.060f), Dark());
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

        private static Color Rubber()
        {
            return new Color(0.231f, 0.227f, 0.200f);
        }
    }
}
