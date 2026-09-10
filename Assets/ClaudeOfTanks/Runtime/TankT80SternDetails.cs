using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80SternDetails
    {
        public static void Build(
            Transform root,
            string id)
        {
            AddTransom(root);
            AddFuelDrums(root);
            AddUnditchingLog(root);
            AddBowTowCable(root);
            AddDeckStowage(root, Variant(id));
        }

        private static void AddTransom(Transform root)
        {
            Box(
                "T80-RearTurbineGrille",
                root,
                V(0f, 1.19f, -3.095f),
                V(1.20f, 0.32f, 0.05f),
                Dark());
            for (int index = 0; index < 4; index++)
            {
                Box(
                    "T80-RearTurbineLouvre",
                    root,
                    V(0f, 1.05f + index * 0.09f, -3.085f),
                    V(1.16f, 0.04f, 0.05f),
                    Detail());
            }
        }

        private static void AddFuelDrums(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Transform drum = Cylinder(
                    "T80-RearFuelDrum",
                    root,
                    V(side * 1.02f, 1.55f, -3.12f),
                    0.135f,
                    0.135f,
                    0.58f,
                    12,
                    TankShapeAxis.Y,
                    Detail());
                drum.localRotation =
                    Quaternion.Euler(0f, 0f, side * 0.08f * Mathf.Rad2Deg);

                Transform cap = Cylinder(
                    "T80-RearFuelDrumCap",
                    root,
                    V(side * 1.02f, 1.815f, -3.13f),
                    0.14f,
                    0.14f,
                    0.03f,
                    12,
                    TankShapeAxis.Y,
                    Dark());
                cap.localRotation =
                    Quaternion.Euler(0f, 0f, side * 0.08f * Mathf.Rad2Deg);
            }
        }

        private static void AddUnditchingLog(Transform root)
        {
            Cylinder(
                "T80-UnditchingLog",
                root,
                V(0f, 0.97f, -3.00f),
                0.10f,
                0.10f,
                1.95f,
                10,
                TankShapeAxis.X,
                Wood());
            foreach (float x in new[] { -0.75f, 0.75f })
            {
                Cylinder(
                    "T80-UnditchingLogStrap",
                    root,
                    V(x, 0.97f, -3.00f),
                    0.107f,
                    0.107f,
                    0.04f,
                    10,
                    TankShapeAxis.X,
                    Dark());
            }
        }

        private static void AddBowTowCable(Transform root)
        {
            TankFittingShapeFactory.BuildTowCable(
                "T80-BowTowCable",
                root,
                new[]
                {
                    V(-1.02f, 1.30f, 2.72f),
                    V(0f, 1.34f, 2.42f),
                    V(1.02f, 1.30f, 2.72f)
                },
                0.022f,
                20,
                6,
                Dark());
        }

        private static void AddDeckStowage(
            Transform root,
            int variant)
        {
            float linksX = variant == 1 ? -0.58f : 0.58f;
            float linksZ = variant == 2 ? 0.30f : 0.60f;
            Transform links =
                new GameObject("T80-SpareTrackCarrier").transform;
            links.SetParent(root, false);
            links.localPosition = V(linksX, 1.395f, linksZ);
            AddSpareTrackLinks(links);

            Vector3[] cablePoints;
            if (variant == 2)
            {
                cablePoints = new[]
                {
                    V(0.45f, 1.420f, 0.95f),
                    V(0.90f, 1.410f, 0.35f),
                    V(0.50f, 1.425f, -0.25f)
                };
            }
            else if (variant == 1)
            {
                cablePoints = new[]
                {
                    V(-0.50f, 1.432f, -0.55f),
                    V(-0.95f, 1.445f, -1.15f),
                    V(-0.60f, 1.478f, -1.60f)
                };
            }
            else
            {
                cablePoints = new[]
                {
                    V(0.50f, 1.432f, -0.55f),
                    V(0.95f, 1.445f, -1.15f),
                    V(0.60f, 1.478f, -1.60f)
                };
            }
            TankFittingShapeFactory.BuildTowCable(
                "T80-DeckTowCable",
                root,
                cablePoints,
                0.018f,
                20,
                6,
                Dark());
        }

        private static void AddSpareTrackLinks(Transform root)
        {
            const int count = 4;
            const float width = 0.50f;
            const float pitch = 0.165f;
            const float runDepth = 0.645f;
            foreach (float x in new[] { -width * 0.32f, width * 0.32f })
            {
                Box(
                    "T80-SpareTrackCarrierRail",
                    root,
                    V(x, -0.0315f, 0f),
                    V(0.030f, 0.018f, runDepth),
                    Detail());
                foreach (float z in new[]
                {
                    -runDepth * 0.4f,
                    runDepth * 0.4f
                })
                {
                    Box(
                        "T80-SpareTrackCarrierFoot",
                        root,
                        V(x, -0.031f, z),
                        V(0.105f, 0.019f, 0.045f),
                        Detail());
                }
            }
            for (int index = 0; index < count; index++)
            {
                float z = (index - 1.5f) * pitch;
                Box(
                    "T80-SpareTrackLink",
                    root,
                    V(0f, 0f, z),
                    V(width, 0.045f, 0.15f),
                    Track());
                Box(
                    "T80-SpareTrackLinkRidge",
                    root,
                    V(0f, 0.02f, z),
                    V(width * 0.88f, 0.06f, 0.05f),
                    Track());
            }
        }

        private static int Variant(string id)
        {
            if (id == "t80b") return 1;
            return id == "t80bv" ? 2 : 0;
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

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.21f, 0.20f, 0.18f);
        }

        private static Color Detail()
        {
            return new Color(0.30f, 0.31f, 0.27f);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }

        private static Color Wood()
        {
            return new Color(0.26f, 0.16f, 0.09f);
        }
    }
}
