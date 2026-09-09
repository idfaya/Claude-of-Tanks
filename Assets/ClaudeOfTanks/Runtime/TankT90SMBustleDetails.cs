using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMBustleDetails
    {
        private static readonly float[] RailYs =
            { 0.17f, 0.27f, 0.37f, 0.47f };

        public static void Build(
            Transform root,
            Color color)
        {
            AddBustleBody(root, color);
            AddRearSlatGrid(root);
            AddSideCells(root, color);
            AddFlowingSideCage(root);
            AddBasketRing(root);
            AddSnorkel(root);
        }

        private static void AddBustleBody(
            Transform root,
            Color color)
        {
            Slab(
                "Painted-T90SM-BustleForward",
                root,
                color * 0.58f,
                V(-0.91f, 0.125f, -1.26f),
                V(0.91f, 0.125f, -1.26f),
                V(0.91f, 0.235f, -1.95f),
                V(-0.91f, 0.235f, -1.95f),
                V(-0.91f, 0.525f, -1.26f),
                V(0.91f, 0.525f, -1.26f),
                V(0.91f, 0.525f, -1.95f),
                V(-0.91f, 0.525f, -1.95f));
            Slab(
                "Painted-T90SM-BustleMiddle",
                root,
                color * 0.56f,
                V(-0.91f, 0.235f, -1.95f),
                V(0.91f, 0.235f, -1.95f),
                V(0.91f, 0.31f, -2.31f),
                V(-0.91f, 0.31f, -2.31f),
                V(-0.91f, 0.525f, -1.95f),
                V(0.91f, 0.525f, -1.95f),
                V(0.91f, 0.525f, -2.31f),
                V(-0.91f, 0.525f, -2.31f));
            Box("Painted-T90SM-BustleRearStep", root,
                V(0f, 0.4225f, -2.36f),
                V(1.82f, 0.205f, 0.20f), color * 0.56f);
        }

        private static void AddRearSlatGrid(Transform root)
        {
            Box("T90SM-BustleRearBackdrop", root,
                V(0f, 0.37f, -2.462f),
                V(1.32f, 0.155f, 0.012f), Dark());
            Box("T90SM-BustleRearTopRail", root,
                V(0f, 0.48f, -2.51f),
                V(1.34f, 0.10f, 0.05f), Detail());
            Box("T90SM-BustleRearBottomRail", root,
                V(0f, 0.304f, -2.51f),
                V(1.34f, 0.028f, 0.05f), Detail());
            foreach (float y in new[] { 0.337f, 0.37f, 0.403f })
            {
                Box("T90SM-BustleRearSlat", root,
                    V(0f, y, -2.508f),
                    V(1.30f, 0.022f, 0.044f), Detail());
            }
            foreach (float x in new[]
            {
                -0.66f, -0.33f, 0f, 0.33f, 0.66f
            })
            {
                Box("T90SM-BustleRearStile", root,
                    V(x, 0.37f, -2.51f),
                    V(0.024f, 0.16f, 0.05f), Detail());
            }
            foreach (float x in new[] { -0.52f, 0.52f })
            {
                Box("T90SM-BustleRearStrut", root,
                    V(x, 0.37f, -2.472f),
                    V(0.03f, 0.03f, 0.05f), Dark());
            }
        }

        private static void AddSideCells(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                AddSideCell(
                    root,
                    color,
                    side,
                    0.985f,
                    side < 0 ? -1.63f : -1.57f,
                    side < 0 ? 0.90f : 0.78f,
                    true);
                AddSideCell(
                    root,
                    color,
                    side,
                    1.12f,
                    -1.17f,
                    0.38f,
                    false);
                AddSideCell(
                    root,
                    color,
                    side,
                    1.24f,
                    -0.90f,
                    0.32f,
                    false);
                AddSideCell(
                    root,
                    color,
                    side,
                    1.3725f,
                    -0.56f,
                    0.33f,
                    false);
            }
            Box("Painted-T90SM-LeftDeepBustleStep", root,
                V(-1.095f, 0.29f, -1.375f),
                V(0.08f, 0.36f, 0.60f), color * 0.54f);
            Box("Painted-T90SM-RightDeepBustleStep", root,
                V(1.06f, 0.29f, -1.375f),
                V(0.03f, 0.36f, 0.60f), color * 0.54f);
        }

        private static void AddSideCell(
            Transform root,
            Color color,
            int side,
            float x,
            float z,
            float depth,
            bool backing)
        {
            const float y = 0.31f;
            const float height = 0.36f;
            if (backing)
            {
                Box("T90SM-BustleSideBacking", root,
                    V(side * x, y, z),
                    V(0.025f, height * 0.82f, depth * 0.90f),
                    Dark());
            }
            Box("Painted-T90SM-BustleSideFoot", root,
                V(
                    side * (x - 0.02f),
                    y - height * 0.34f,
                    z + depth * 0.28f),
                V(0.12f, 0.12f, 0.16f),
                color * 0.54f);
        }

        private static void AddFlowingSideCage(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float aftZ = side < 0 ? -2.08f : -1.96f;
                Vector2[] path =
                {
                    V2(0.67f, -2.51f),
                    V2(1.037f, aftZ),
                    V2(1.037f, -1.30f),
                    V2(1.172f, -1.06f),
                    V2(1.172f, -0.99f),
                    V2(1.292f, -0.80f),
                    V2(1.292f, -0.75f),
                    V2(1.4245f, -0.545f),
                    V2(1.4245f, -0.40f)
                };
                foreach (float y in RailYs)
                {
                    for (int index = 0;
                        index < path.Length - 1;
                        index++)
                    {
                        float x0 = side * path[index].x;
                        float x1 = side * path[index + 1].x;
                        float z0 = path[index].y;
                        float z1 = path[index + 1].y;
                        float dx = x1 - x0;
                        float dz = z1 - z0;
                        Transform rail = Box(
                            "T90SM-BustleSideRail",
                            root,
                            V(
                                (x0 + x1) * 0.5f,
                                y,
                                (z0 + z1) * 0.5f),
                            V(
                                0.030f,
                                0.026f,
                                Mathf.Sqrt(dx * dx + dz * dz) +
                                    0.026f),
                            Detail());
                        rail.localRotation = Quaternion.Euler(
                            0f,
                            Mathf.Atan2(dx, dz) * Mathf.Rad2Deg,
                            0f);
                    }
                }
                Vector2[] posts =
                {
                    V2(1.037f, aftZ),
                    V2(1.037f, (aftZ - 1.30f) * 0.5f),
                    V2(1.037f, -1.30f),
                    V2(1.172f, -1.025f),
                    V2(1.292f, -0.775f),
                    V2(1.4245f, -0.545f),
                    V2(1.4245f, -0.42f)
                };
                for (int index = 0; index < posts.Length; index++)
                {
                    Box("T90SM-BustleSidePost", root,
                        V(
                            side * posts[index].x,
                            0.31f,
                            posts[index].y),
                        V(0.028f, 0.317f, 0.028f),
                        Detail());
                }
                Box("T90SM-BustleCornerPost", root,
                    V(side * 0.67f, 0.325f, -2.49f),
                    V(0.028f, 0.33f, 0.036f), Detail());
                Box("T90SM-BustleDeckPad", root,
                    V(side * 0.55f, 0.50f, -1.85f),
                    V(0.72f, 0.10f, 0.88f), Detail());
            }
        }

        private static void AddBasketRing(Transform root)
        {
            const float y = 0.55f;
            foreach (float z in new[] { -2.30f, -1.72f })
            {
                Box("T90SM-BustleBasketCrossRail", root,
                    V(0f, y, z),
                    V(1.70f, 0.024f, 0.024f), Detail());
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T90SM-BustleBasketSideRail", root,
                    V(side * 0.84f, y, -2.01f),
                    V(0.024f, 0.024f, 0.60f), Detail());
                foreach (float z in new[]
                {
                    -2.28f, -2.01f, -1.74f
                })
                {
                    Box("T90SM-BustleBasketPost", root,
                        V(side * 0.80f, y - 0.038f, z),
                        V(0.02f, 0.06f, 0.02f), Detail());
                }
            }
        }

        private static void AddSnorkel(Transform root)
        {
            Transform snorkel = TankShapeFactory.CylinderPart(
                "T90SM-OPVTSnorkel",
                root,
                0.05f,
                0.05f,
                0.72f,
                10,
                TankShapeAxis.Z,
                Dark());
            snorkel.localPosition = V(-0.62f, 0.50f, -2.05f);
            foreach (float z in new[] { -1.85f, -2.25f })
            {
                Box("T90SM-OPVTStrap", root,
                    V(-0.62f, 0.545f, z),
                    V(0.12f, 0.02f, 0.03f), Detail());
            }
        }

        private static Transform Slab(
            string name,
            Transform parent,
            Color color,
            Vector3 bottom0,
            Vector3 bottom1,
            Vector3 bottom2,
            Vector3 bottom3,
            Vector3 top0,
            Vector3 top1,
            Vector3 top2,
            Vector3 top3)
        {
            return TankShapeFactory.OrientedSlabPart(
                name,
                parent,
                bottom0,
                bottom1,
                bottom2,
                bottom3,
                top0,
                top1,
                top2,
                top3,
                color);
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

        private static Vector2 V2(float x, float y)
        {
            return new Vector2(x, y);
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
