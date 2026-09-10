using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MTurretDetails
    {
        public static void Build(
            Transform turret,
            Color color,
            string id)
        {
            HideRenderer(turret.Find("Turret"));
            GameObject rootObject =
                new GameObject("T90M-PresentationRoot");
            Transform root = rootObject.transform;
            root.SetParent(turret, false);
            root.localPosition = V(0f, 0.16f, -0.02f);
            root.localScale = V(0.95f, 0.65f, 0.913f);

            AddWeldedShell(root, color);
            AddAprons(root, color);
            AddCrown(root, color);
            AddCheekCarriers(root, color);
            TankT90MTurretArmorDetails.Build(
                root,
                color,
                id == "t90m_proryv");
            TankT90MBustleDetails.Build(root, color);
            TankT90MTurretEquipmentDetails.Build(root, color);
        }

        private static void AddWeldedShell(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90M-WeldedShell",
                root,
                new[]
                {
                    S(1.42f, -0.10f, 0.32f,
                        -0.34f, 0.34f,
                        -0.29f, 0.29f,
                        -0.24f, 0.24f),
                    S(1.12f, -0.10f, 0.52f,
                        -0.84f, 0.84f,
                        -0.70f, 0.70f,
                        -0.55f, 0.55f),
                    S(0.72f, -0.09f, 0.67f,
                        -1.34f, 1.34f,
                        -1.13f, 1.13f,
                        -0.84f, 0.84f),
                    S(0.18f, -0.07f, 0.73f,
                        -1.58f, 1.58f,
                        -1.42f, 1.42f,
                        -1.06f, 1.06f),
                    S(-0.46f, -0.05f, 0.71f,
                        -1.56f, 1.56f,
                        -1.43f, 1.43f,
                        -1.10f, 1.10f),
                    S(-0.98f, -0.01f, 0.64f,
                        -1.42f, 1.42f,
                        -1.30f, 1.30f,
                        -1.00f, 1.00f),
                    S(-1.38f, 0.04f, 0.56f,
                        -1.16f, 1.16f,
                        -1.07f, 1.07f,
                        -0.86f, 0.86f)
                },
                color * 0.62f);
            float[,] welds =
            {
                { 1.12f, 1.42f, 0.535f },
                { 0.72f, 2.22f, 0.685f },
                { 0.18f, 2.10f, 0.745f },
                { -0.46f, 2.18f, 0.725f },
                { -0.98f, 1.94f, 0.655f }
            };
            for (int index = 0;
                index < welds.GetLength(0);
                index++)
            {
                Box(
                    "T90M-ShellWeld",
                    root,
                    V(
                        0f,
                        welds[index, 2],
                        welds[index, 0]),
                    V(welds[index, 1], 0.014f, 0.035f),
                    Dark());
            }
        }

        private static void AddAprons(
            Transform root,
            Color color)
        {
            Vector2[] outline =
            {
                V2(-0.22f, 1.16f), V2(0.22f, 1.16f),
                V2(0.82f, 0.88f), V2(1.26f, 0.38f),
                V2(1.30f, -0.56f), V2(0.92f, -1.14f),
                V2(-0.92f, -1.14f), V2(-1.30f, -0.56f),
                V2(-1.26f, 0.38f), V2(-0.82f, 0.88f)
            };
            Poly(
                "Painted-T90M-RingApron",
                root,
                outline,
                0.52f,
                0.94f,
                0.96f,
                V(0f, -0.46f, -0.05f),
                color * 0.58f);
            Poly(
                "T90M-RingApronLip",
                root,
                outline,
                0.030f,
                0.95f,
                0.97f,
                V(0f, 0.045f, -0.05f),
                Dark());

            Vector2[] ring =
            {
                V2(-0.20f, 1.02f), V2(0.20f, 1.02f),
                V2(0.82f, 0.70f), V2(1.16f, 0.18f),
                V2(1.14f, -0.66f), V2(0.76f, -1.02f),
                V2(-0.76f, -1.02f), V2(-1.14f, -0.66f),
                V2(-1.16f, 0.18f), V2(-0.82f, 0.70f)
            };
            Poly(
                "Painted-T90M-RingUndercut",
                root,
                ring,
                0.35f,
                0.94f,
                0.96f,
                V(0f, -0.15f, -0.10f),
                color * 0.56f);
            Poly(
                "T90M-RingUndercutLip",
                root,
                ring,
                0.025f,
                0.95f,
                0.97f,
                V(0f, 0.19f, -0.10f),
                Dark());
            Poly(
                "Painted-T90M-LowerRace",
                root,
                ring,
                0.36f,
                0.92f,
                0.94f,
                V(0f, -0.80f, -0.10f),
                color * 0.54f);
            Poly(
                "T90M-LowerRaceLip",
                root,
                ring,
                0.025f,
                0.93f,
                0.95f,
                V(0f, -0.445f, -0.10f),
                Dark());
        }

        private static void AddCrown(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-T90M-CrownInner",
                    root,
                    V(side * 0.52f, 0.615f, 0.34f),
                    V(0.70f, 0.105f, 0.70f),
                    color * 0.62f,
                    R(-0.12f, -side * 0.13f, 0f));
                Box(
                    "Painted-T90M-CrownOuter",
                    root,
                    V(side * 1.08f, 0.54f, 0.02f),
                    V(0.58f, 0.095f, 0.66f),
                    color * 0.60f,
                    R(-0.13f, -side * 0.24f, 0f));
                Box(
                    "T90M-CrownInnerWeld",
                    root,
                    V(side * 0.52f, 0.68f, 0.06f),
                    V(0.62f, 0.015f, 0.045f),
                    Dark(),
                    R(-0.12f, -side * 0.13f, 0f));
                Box(
                    "T90M-CrownOuterWeld",
                    root,
                    V(side * 1.08f, 0.60f, -0.24f),
                    V(0.50f, 0.015f, 0.045f),
                    Dark(),
                    R(-0.13f, -side * 0.24f, 0f));
                Box(
                    "Painted-T90M-InnerRoofSaddle",
                    root,
                    V(side * 0.46f, 0.710f, -0.20f),
                    V(0.82f, 0.080f, 1.12f),
                    color * 0.62f,
                    R(-0.035f, -side * 0.055f, 0f));
                Box(
                    "T90M-InnerRoofSaddleWeld",
                    root,
                    V(side * 0.46f, 0.755f, -0.70f),
                    V(0.66f, 0.012f, 0.040f),
                    Dark(),
                    R(-0.035f, -side * 0.055f, 0f));
            }
            Box(
                "Painted-T90M-CrownCenter",
                root,
                V(0f, 0.655f, -0.15f),
                V(0.52f, 0.08f, 0.66f),
                color * 0.62f);
            Box(
                "T90M-CrownCenterWeld",
                root,
                V(0f, 0.702f, -0.16f),
                V(0.42f, 0.014f, 0.55f),
                Dark());
        }

        private static void AddCheekCarriers(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Slab(
                    "Painted-T90M-CheekCarrier",
                    root,
                    side,
                    V(0.20f, 0.02f, 1.42f),
                    V(1.70f, 0.02f, 0.52f),
                    V(1.64f, 0.02f, -0.12f),
                    V(0.26f, 0.02f, 0.56f),
                    V(0.24f, 0.45f, 1.16f),
                    V(1.54f, 0.40f, 0.43f),
                    V(1.48f, 0.37f, -0.12f),
                    V(0.31f, 0.49f, 0.50f),
                    color * 0.58f);
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

        private static Transform Poly(
            string name,
            Transform parent,
            Vector2[] plan,
            float height,
            float flare,
            float inset,
            Vector3 position,
            Color color)
        {
            Transform part = TankPolyLoftShapeFactory.BuildTurret(
                name,
                parent,
                plan,
                height,
                flare,
                inset,
                color);
            part.localPosition = position;
            return part;
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

        private static TankWeldedStation S(
            float z,
            float bottomY,
            float topY,
            float middleLeftX,
            float middleRightX,
            float bottomLeftX,
            float bottomRightX,
            float topLeftX,
            float topRightX)
        {
            return new TankWeldedStation(
                z,
                bottomY,
                topY,
                middleLeftX,
                middleRightX,
                bottomLeftX,
                bottomRightX,
                topLeftX,
                topRightX);
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
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
            return new Color(0.20f, 0.22f, 0.16f);
        }
    }
}
