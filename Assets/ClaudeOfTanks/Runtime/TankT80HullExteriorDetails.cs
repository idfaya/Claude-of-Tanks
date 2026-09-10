using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80HullExteriorDetails
    {
        public static void Build(
            Transform root,
            Color color,
            string id)
        {
            AddTurbineShoulders(root, color);
            AddEngineDeck(root, color);
            AddGlacis(root, color);
            AddBowCorners(root, color);
            AddSkirts(root, color, id == "t80bv");
        }

        private static void AddTurbineShoulders(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T80-TurbineShoulder", root,
                    V(side * 1.2175f, 1.635f, -3.1625f),
                    V(0.875f, 0.45f, 0.215f), color * 0.56f);
                Box("Painted-T80-TurbineLip", root,
                    V(
                        side * (side < 0 ? 1.21f : 1.225f),
                        1.56f,
                        -3.33f),
                    V(side < 0 ? 0.82f : 0.85f, 0.305f, 0.12f),
                    color * 0.54f);
                Box("Painted-T80-TurbineForwardStep", root,
                    V(side * 1.2125f, 1.7125f, -2.90f),
                    V(0.885f, 0.155f, 0.11f), color * 0.58f);
                Box("Painted-T80-RearSidePlate", root,
                    V(side * 1.21f, 1.215f, -3.045f),
                    V(0.90f, 0.39f, 0.19f), color * 0.52f);
                Box("Painted-T80-FenderCap", root,
                    V(side * 1.4775f, 1.245f, 0.225f),
                    V(0.475f, 0.030f, 4.35f), color * 0.56f);
                Box("Painted-T80-FenderInnerRail", root,
                    V(side * 1.220f, 1.215f, 0.225f),
                    V(0.060f, 0.030f, 4.35f), color * 0.54f);
                Box("Painted-T80-FenderOuterLip", root,
                    V(side * 1.6925f, 1.1875f, 0.225f),
                    V(0.045f, 0.125f, 4.35f), color * 0.52f);
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color)
        {
            Box("T80-EngineDeckGrilleBacking", root,
                V(0f, 1.462f, -1.95f),
                V(1.60f, 0.02f, 1.05f), Dark());
            for (int index = 0; index < 5; index++)
            {
                Box("T80-EngineDeckGrilleRib", root,
                    V(0f, 1.468f, -1.62f - index * 0.15f),
                    V(1.52f, 0.02f, 0.05f), Detail());
            }
            Box("Painted-T80-TurbineIntakeHump", root,
                V(0.40f, 1.472f, -1.50f),
                V(0.95f, 0.06f, 0.58f), color * 0.58f);
            Box("Painted-T80-SplashRidge", root,
                V(0f, 1.253f, 2.78f),
                V(1.90f, 0.045f, 0.16f), color * 0.56f);
        }

        private static void AddGlacis(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "T80-GlacisBrace",
                    root,
                    V(side * 0.48f, 1.19f, 2.72f),
                    V(0.90f, 0.045f, 0.05f),
                    Detail(),
                    R(-0.35f, side * 0.25f, 0f));
                Box(
                    "T80-BowHook",
                    root,
                    V(side * 0.90f, 0.82f, 3.12f),
                    V(0.10f, 0.12f, 0.14f),
                    Dark(),
                    R(-0.30f, 0f, 0f));
                Torus(
                    "T80-BowTowEye",
                    root,
                    V(side * 0.82f, 0.82f, 3.02f),
                    0.085f,
                    0.016f,
                    10,
                    Detail(),
                    Quaternion.Euler(90f, 0f, 0f));
                AddHeadlight(root, color, side * 1.32f);
            }
        }

        private static void AddHeadlight(
            Transform root,
            Color color,
            float x)
        {
            Transform group =
                new GameObject("T80-HeadlightAssembly").transform;
            group.SetParent(root, false);
            group.localPosition = V(x, 1.26f, 2.86f);
            group.localRotation = R(-0.30f, 0f, 0f);
            Cylinder(
                "Painted-T80-HeadlightHousing",
                group,
                V(),
                0.05f,
                0.05f,
                0.0675f,
                12,
                TankShapeAxis.Z,
                color * 0.50f);
            Cylinder(
                "T80-HeadlightLens",
                group,
                V(0f, 0f, 0.036f),
                0.04f,
                0.04f,
                0.02f,
                12,
                TankShapeAxis.Z,
                Glass());
            Box(
                "T80-HeadlightGuard",
                group,
                V(0f, 0f, 0.025f),
                V(0.02f, 0.115f, 0.02f),
                Dark());
        }

        private static void AddBowCorners(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T80-BowArrowInner", root,
                    V(side * 0.46f, 1.11f, 3.075f),
                    V(0.33f, 0.10f, 0.05f), color * 0.54f,
                    R(0f, -side * 0.273f, 0f));
                Box("Painted-T80-BowArrowOuter", root,
                    V(side * 0.83f, 1.11f, 3.275f),
                    V(0.57f, 0.10f, 0.05f), color * 0.54f,
                    R(0f, -side * 0.624f, 0f));
                Box("Painted-T80-BowPocket", root,
                    V(side * 0.82f, 1.10f, 3.06f),
                    V(0.38f, 0.07f, 0.18f), color * 0.52f);
                Box("Painted-T80-BowCorner", root,
                    V(side * 1.2725f, 1.10f, 3.285f),
                    V(0.945f, 0.10f, 0.21f), color * 0.52f);
                Box("Painted-T80-BowCornerCap", root,
                    V(side * 1.2725f, 1.155f, 3.34f),
                    V(0.945f, 0.05f, 0.10f), color * 0.54f);
                Box("T80-FrontMudFlap", root,
                    V(side * 1.38f, 0.945f, 3.30f),
                    V(0.34f, 0.30f, 0.045f), Rubber());
                Box("T80-FrontMudFlap", root,
                    V(side * 1.38f, 0.99f, 3.3675f),
                    V(0.34f, 0.30f, 0.045f), Rubber());
                Box("T80-RearMudFlap", root,
                    V(side * 1.36f, 1.00f, -3.10f),
                    V(0.34f, 0.26f, 0.045f), Rubber());
            }
        }

        private static void AddSkirts(
            Transform root,
            Color color,
            bool bv)
        {
            float x = bv ? 1.744f : 1.71f;
            float thickness = bv ? 0.032f : 0.10f;
            float z0 = bv ? -2.93f : -2.66f;
            float z1 = bv ? 3.30f : 2.96f;
            float top = bv ? 1.23f : 1.10f;
            float bottom = bv ? 1.03f : 0.79f;
            float lipY = bv ? 1.045f : 0.805f;
            float depth = (z1 - z0) / 7f;
            for (int side = -1; side <= 1; side += 2)
            for (int panel = 0; panel < 7; panel++)
            {
                float z = z0 + depth * (panel + 0.5f);
                Box("Painted-T80-SkirtPanel", root,
                    V(side * x, (top + bottom) * 0.5f, z),
                    V(thickness, top - bottom, depth * 0.94f),
                    color * 0.50f);
                Box("T80-SkirtBatten", root,
                    V(side * (x - 0.009f), (top + bottom) * 0.5f,
                        z + depth * 0.5f),
                    V(0.048f, (top - bottom) * 0.90f, 0.020f),
                    Dark());
                Cylinder("T80-SkirtBolt", root,
                    V(side * (x + 0.003f), top - 0.07f, z),
                    0.014f, 0.014f, 0.014f, 8,
                    TankShapeAxis.X, Dark());
                Box("T80-SkirtBottomLip", root,
                    V(side * 1.727f, lipY, z),
                    V(0.042f, 0.09f, depth * 0.92f),
                    Dark());
            }
            if (bv)
            {
                for (int side = -1; side <= 1; side += 2)
                for (int index = 0; index < 3; index++)
                {
                    Box("T80BV-K1SkirtFrontPlate", root,
                        V(side * 1.745f, 0.95f, 2.98f - index * 0.55f),
                        V(0.028f, 0.42f, 0.50f), Track());
                }
            }
            else
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Box("T80-FrontSkirtReturn", root,
                        V(side * 1.67f, 1.045f, 3.345f),
                        V(0.10f, 0.37f, 0.09f), Track());
                }
            }
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
            float tube,
            int segments,
            Color color,
            Quaternion rotation)
        {
            Transform part = TankShapeFactory.TorusPart(
                name, parent, radius, tube, segments, color);
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

        private static Color Glass()
        {
            return new Color(0.16f, 0.21f, 0.25f);
        }

        private static Color Rubber()
        {
            return new Color(0.16f, 0.165f, 0.155f);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }
    }
}
