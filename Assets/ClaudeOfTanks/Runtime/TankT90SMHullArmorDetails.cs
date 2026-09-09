using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMHullArmorDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddBowStaircase(root, color);
            AddFenderLips(root, color);
            AddDeck(root, color);
            AddGlacisFittings(root, color);
            AddRelikt(root, color);
            AddSkirts(root, color);
            AddBowFlapsAndHorns(root, color);
        }

        private static void AddBowStaircase(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90SM-BowShoulder", root,
                    V(side * 0.805f, 1.02f, 3.06f),
                    V(0.29f, 0.34f, 0.30f), color * 0.56f);
                Box("Painted-T90SM-FenderBridge", root,
                    V(side * 1.17f, 1.0975f, 3.175f),
                    V(0.46f, 0.045f, 0.18f), color * 0.58f);
                Transform prong =
                    TankShapeFactory.OrientedSlabPart(
                        "Painted-T90SM-BowProng",
                        root,
                        V(side * 0.995f, 0.85f, 3.25f),
                        V(side * 1.385f, 0.85f, 3.25f),
                        V(side * 1.385f, 0.785f, 3.465f),
                        V(side * 0.995f, 0.785f, 3.465f),
                        V(side * 0.995f, 1.12f, 3.25f),
                        V(side * 1.385f, 1.12f, 3.25f),
                        V(side * 1.385f, 0.92f, 3.465f),
                        V(side * 0.995f, 0.92f, 3.465f),
                        color * 0.54f);
                prong.localPosition = Vector3.zero;
                Box("T90SM-BowShoulderSeam", root,
                    V(side * 0.805f, 1.02f, 3.196f),
                    V(0.20f, 0.022f, 0.022f), Dark());
                Box("T90SM-BowProngSeam", root,
                    V(side * 1.255f, 1.02f, 3.416f),
                    V(0.16f, 0.022f, 0.022f), Dark());
            }
        }

        private static void AddFenderLips(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 10; index++)
                {
                    Box("Painted-T90SM-FenderLip", root,
                        V(
                            side * 1.70f,
                            index == 9 ? 1.2525f : 1.3475f,
                            -2.70f + index * 0.545f),
                        V(0.20f, 0.05f, 0.50f),
                        color * 0.56f);
                }
                Box("Painted-T90SM-LowBowFenderLip", root,
                    V(side * 1.70f, 1.2025f, 2.7275f),
                    V(0.20f, 0.045f, 0.55f),
                    color * 0.56f);
            }
        }

        private static void AddDeck(
            Transform root,
            Color color)
        {
            Cylinder("Painted-T90SM-DriverHatch", root,
                V(0f, 1.38f, 2.00f),
                0.24f, 0.24f, 0.04f, 14,
                TankShapeAxis.Y, color * 0.58f);
            Cylinder("T90SM-DriverHatchRim", root,
                V(0f, 1.387f, 2.00f),
                0.247f, 0.247f, 0.012f, 14,
                TankShapeAxis.Y, Dark());
            foreach (float x in new[] { -0.16f, 0.16f })
            {
                Box("T90SM-DriverPeriscope", root,
                    V(x, 1.22f, 2.30f),
                    V(0.11f, 0.055f, 0.075f), Detail());
                Box("T90SM-DriverPeriscopeLens", root,
                    V(x, 1.225f, 2.341f),
                    V(0.075f, 0.030f, 0.010f), Glass());
            }
            for (int index = 0; index < 5; index++)
            {
                float z = -1.67f - index * 0.24f;
                Box("T90SM-EngineGrille", root,
                    V(0f, 1.412f, z),
                    V(1.50f, 0.018f, 0.075f), Dark());
                Box("T90SM-EngineGrilleRib", root,
                    V(0f, 1.406f, z - 0.12f),
                    V(1.50f, 0.028f, 0.026f), Detail());
            }
        }

        private static void AddGlacisFittings(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Transform bar = Box(
                    "T90SM-GlacisBar",
                    root,
                    V(side * 0.56f, 1.22f, 2.61f),
                    V(1.05f, 0.045f, 0.05f),
                    Detail());
                bar.localRotation = Quaternion.Euler(
                    -0.35f * Mathf.Rad2Deg,
                    side * 0.25f * Mathf.Rad2Deg,
                    0f);
                Transform hook = Box(
                    "T90SM-TowHook",
                    root,
                    V(side * 1.05f, 0.69f, 2.99f),
                    V(0.10f, 0.12f, 0.14f),
                    Dark());
                hook.localRotation = Quaternion.Euler(
                    -0.30f * Mathf.Rad2Deg,
                    0f,
                    0f);
                Transform eye = TankShapeFactory.TorusPart(
                    "T90SM-TowEye",
                    root,
                    0.085f,
                    0.016f,
                    10,
                    Detail());
                eye.localPosition =
                    V(side * 0.98f, 0.50f, 2.88f);
                eye.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                AddHeadlight(
                    root,
                    side * 1.02f,
                    1.13f,
                    2.75f,
                    color);
            }
        }

        private static void AddHeadlight(
            Transform root,
            float x,
            float y,
            float z,
            Color color)
        {
            Transform body = Cylinder(
                "Painted-T90SM-Headlight",
                root,
                V(x, y, z),
                0.09f,
                0.11f,
                0.13f,
                14,
                TankShapeAxis.Z,
                color * 0.48f);
            body.localRotation =
                Quaternion.Euler(-0.30f * Mathf.Rad2Deg, 0.05f, 0f);
            Transform lens = Cylinder(
                "T90SM-HeadlightLens",
                root,
                V(x, y + 0.007f, z + 0.071f),
                0.073f,
                0.073f,
                0.012f,
                14,
                TankShapeAxis.Z,
                Glass());
            lens.localRotation = body.localRotation;
        }

        private static void AddRelikt(
            Transform root,
            Color color)
        {
            float[] boxes = { 0.225f, 0.60f, 0.975f };
            float[] gaps = { 0.4125f, 0.7875f };
            for (int row = 0; row < 2; row++)
            {
                float y = 1.26f - row * 0.06f;
                float z = 2.05f + row * 0.27f;
                for (int side = -1; side <= 1; side += 2)
                {
                    Quaternion rotation = Quaternion.Euler(
                        -0.42f * Mathf.Rad2Deg,
                        side * 0.14f * Mathf.Rad2Deg,
                        0f);
                    for (int index = 0;
                        index < boxes.Length;
                        index++)
                    {
                        Transform cassette = Box(
                            "Painted-T90SM-GlacisRelikt",
                            root,
                            V(side * boxes[index], y, z),
                            V(0.33f, 0.075f, 0.28f),
                            color * 0.52f);
                        cassette.localRotation = rotation;
                    }
                    for (int index = 0;
                        index < gaps.Length;
                        index++)
                    {
                        Transform seam = Box(
                            "T90SM-GlacisReliktSeam",
                            root,
                            V(side * gaps[index], y - 0.004f, z),
                            V(0.03f, 0.06f, 0.26f),
                            Dark());
                        seam.localRotation = rotation;
                    }
                }
            }
        }

        private static void AddSkirts(
            Transform root,
            Color color)
        {
            const float panelDepth = 0.70f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 6; index++)
                {
                    float z =
                        -1.10f + panelDepth * (index + 0.5f);
                    Box("Painted-T90SM-SkirtPanel", root,
                        V(side * 1.765f, 1.09f, z),
                        V(0.08f, 0.30f, panelDepth * 0.94f),
                        color * 0.48f);
                    Box("T90SM-SkirtBatten", root,
                        V(side * 1.768f, 1.09f, z + panelDepth * 0.5f),
                        V(0.048f, 0.27f, 0.02f),
                        Dark());
                    Transform bolt = Cylinder(
                        "T90SM-SkirtBolt",
                        root,
                        V(side * 1.78f, 1.17f, z),
                        0.014f, 0.014f, 0.014f, 8,
                        TankShapeAxis.Z, Dark());
                    bolt.localRotation =
                        Quaternion.Euler(0f, side * 90f, 0f);
                    Box("T90SM-SkirtBottomLip", root,
                        V(
                            side * 1.763f,
                            side < 0 ? 0.985f : 0.91f,
                            z),
                        V(0.042f, 0.09f, panelDepth * 0.92f),
                        Dark());
                }
                Box("Painted-T90SM-BowSkirtCap", root,
                    V(side * 1.7325f, 1.11f, 3.02f),
                    V(0.145f, 0.34f, 0.08f),
                    color * 0.50f);
            }
        }

        private static void AddBowFlapsAndHorns(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T90SM-FrontMudFlap", root,
                    V(side * 1.46f, 1.14f, 3.12f),
                    V(0.60f, 0.13f, 0.045f),
                    Rubber());
                Box("Painted-T90SM-FenderHorn", root,
                    V(side * 1.60f, 1.10f, 3.27f),
                    V(0.30f, 0.05f, 0.12f),
                    color * 0.54f);
                Box("Painted-T90SM-OuterFenderHorn", root,
                    V(side * 1.79f, 1.10f, 3.23f),
                    V(0.08f, 0.05f, 0.18f),
                    color * 0.54f);
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

        private static Color Glass()
        {
            return TankT90AFamilyDetails.Glass();
        }

        private static Color Rubber()
        {
            return new Color(0.23f, 0.227f, 0.20f);
        }
    }
}
