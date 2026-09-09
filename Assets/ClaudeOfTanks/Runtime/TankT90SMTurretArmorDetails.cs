using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMTurretArmorDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                AddBroadCheek(root, color, side);
                AddNoseWedges(root, color, side);
                AddSideCassettes(root, color, side);
            }
            AddRoofEdgeDetails(root, color);
        }

        private static void AddBroadCheek(
            Transform root,
            Color color,
            int side)
        {
            Slab(
                "Painted-T90SM-BroadCheek",
                root,
                color * 0.60f,
                V(side * 0.2325f, 0.08f, 1.40f),
                V(side * 1.55f, 0.08f, 0.252f),
                V(side * 1.55f, 0.08f, -0.20f),
                V(side * 0.2325f, 0.08f, 0.84f),
                V(side * 0.2325f, 0.412f, 0.812f),
                V(side * 1.395f, 0.3399f, 0.07f),
                V(side * 1.395f, 0.3708f, -0.30f),
                V(side * 0.2325f, 0.4635f, 0.532f));
        }

        private static void AddNoseWedges(
            Transform root,
            Color color,
            int side)
        {
            Slab(
                "Painted-T90SM-NoseWedgeOuter",
                root,
                color * 0.62f,
                V(side * 0.78f, 0.12f, 1.30f),
                V(side * 1.16f, 0.12f, 1.24f),
                V(side * 1.16f, 0.235f, 1.585f),
                V(side * 0.78f, 0.275f, 1.80f),
                V(side * 0.78f, 0.515f, 1.30f),
                V(side * 1.16f, 0.515f, 1.24f),
                V(side * 1.16f, 0.49f, 1.585f),
                V(side * 0.78f, 0.354f, 1.80f));
            Slab(
                "Painted-T90SM-NoseWedgeMiddle",
                root,
                color * 0.62f,
                V(side * 0.44f, 0.07f, 1.30f),
                V(side * 0.78f, 0.07f, 1.30f),
                V(side * 0.78f, 0.225f, 1.80f),
                V(side * 0.44f, 0.25f, 1.868f),
                V(side * 0.44f, 0.515f, 1.30f),
                V(side * 0.78f, 0.515f, 1.30f),
                V(side * 0.78f, 0.414f, 1.80f),
                V(side * 0.44f, 0.40f, 1.868f));
            Slab(
                "Painted-T90SM-NoseWedgeInner",
                root,
                color * 0.62f,
                V(side * 0.14f, 0.07f, 1.30f),
                V(side * 0.44f, 0.07f, 1.30f),
                V(side * 0.44f, 0.25f, 1.868f),
                V(side * 0.14f, 0.25f, 1.868f),
                V(side * 0.14f, 0.515f, 1.30f),
                V(side * 0.44f, 0.515f, 1.30f),
                V(side * 0.44f, 0.40f, 1.868f),
                V(side * 0.14f, 0.40f, 1.868f));
        }

        private static void AddSideCassettes(
            Transform root,
            Color color,
            int side)
        {
            Slab(
                "Painted-T90SM-ForwardCheekWedge",
                root,
                color * 0.58f,
                V(side * 1.565f, 0.080f, 1.18f),
                V(side * 1.65f, 0.080f, 1.065f),
                V(side * 1.735f, 0.080f, 1.00f),
                V(side * 1.565f, 0.080f, 1.00f),
                V(side * 1.24f, 0.40f, 1.13f),
                V(side * 1.36f, 0.40f, 1.03f),
                V(side * 1.445f, 0.40f, 0.97f),
                V(side * 1.22f, 0.40f, 0.97f));
            Slab(
                "Painted-T90SM-MainCheekCassette",
                root,
                color * 0.56f,
                V(side * 1.42f, 0.080f, 0.14f),
                V(side * 1.735f, 0.080f, 0.18f),
                V(side * 1.735f, 0.080f, 1.02f),
                V(side * 1.48f, 0.080f, 1.08f),
                V(side * 1.10f, 0.500f, 0.10f),
                V(side * 1.36f, 0.500f, 0.18f),
                V(side * 1.36f, 0.500f, 1.00f),
                V(side * 1.13f, 0.500f, 1.06f));
            Slab(
                "Painted-T90SM-FlankTransitionAft",
                root,
                color * 0.58f,
                V(side * 1.18f, 0.12f, -0.42f),
                V(side * 1.50f, 0.12f, -0.42f),
                V(side * 1.50f, 0.12f, 0.14f),
                V(side * 1.18f, 0.12f, 0.14f),
                V(side * 1.08f, 0.56f, -0.38f),
                V(side * 1.205f, 0.56f, -0.38f),
                V(side * 1.205f, 0.56f, 0.14f),
                V(side * 1.08f, 0.56f, 0.14f));
            Slab(
                "Painted-T90SM-FlankTransitionFront",
                root,
                color * 0.58f,
                V(side * 1.38f, 0.12f, -0.14f),
                V(side * 1.63f, 0.12f, -0.14f),
                V(side * 1.63f, 0.12f, 0.18f),
                V(side * 1.38f, 0.12f, 0.18f),
                V(side * 1.20f, 0.46f, -0.12f),
                V(side * 1.35f, 0.46f, -0.12f),
                V(side * 1.35f, 0.46f, 0.16f),
                V(side * 1.20f, 0.46f, 0.16f));
        }

        private static void AddRoofEdgeDetails(
            Transform root,
            Color color)
        {
            Box(
                "Painted-T90SM-RightCassetteCrown",
                root,
                V(1.14f, 0.595f, -0.145f),
                V(0.12f, 0.07f, 0.55f),
                color * 0.58f);
            Box(
                "Painted-T90SM-LeftFlankBin",
                root,
                V(-1.04f, 0.54f, 0.15f),
                V(0.38f, 0.20f, 0.70f),
                color * 0.56f);
            Box(
                "T90SM-LeftFlankBinLid",
                root,
                V(-1.04f, 0.634f, 0.15f),
                V(0.34f, 0.016f, 0.64f),
                Dark());
            Slab(
                "Painted-T90SM-LeftRoofEdgeCassette",
                root,
                color * 0.58f,
                V(-1.23f, 0.16f, -0.125f),
                V(-1.36f, 0.16f, -0.125f),
                V(-1.36f, 0.16f, 0.425f),
                V(-1.23f, 0.16f, 0.425f),
                V(-1.10f, 0.54f, -0.125f),
                V(-1.185f, 0.54f, -0.125f),
                V(-1.185f, 0.54f, 0.425f),
                V(-1.10f, 0.54f, 0.425f));
            Box(
                "T90SM-LeftRoofEdgeCassetteLid",
                root,
                V(-1.1425f, 0.547f, 0.15f),
                V(0.075f, 0.014f, 0.49f),
                Dark());
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

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }
    }
}
