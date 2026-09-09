using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t90sm";
        }

        public static void BuildHull(
            Transform root,
            Color color)
        {
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideByPrefix(root, "Armor-");
            TankHullLoftShapeFactory.Build(
                "Painted-T90SM-HullLoft",
                root,
                Curve(
                    -2.92f, 1.40f,
                    -1.75f, 1.45f,
                    -0.45f, 1.44f,
                    1.13f, 1.40f,
                    1.99f, 1.40f,
                    2.42f, 1.29f,
                    2.85f, 1.23f,
                    3.02f, 1.17f),
                Curve(
                    -2.92f, 0.70f,
                    -2.07f, 0.44f,
                    2.57f, 0.45f,
                    3.02f, 0.49f),
                Curve(
                    -2.92f, 1.20f,
                    -2.79f, 1.60f,
                    2.88f, 1.60f,
                    3.02f, 1.55f),
                Curve(
                    -2.92f, 0.92f,
                    -2.12f, 1.06f,
                    2.48f, 1.06f,
                    3.02f, 0.92f),
                Curve(
                    -2.92f, 1.22f,
                    -2.84f, 1.35f,
                    -2.06f, 1.35f,
                    -1.78f, 1.22f,
                    3.02f, 1.22f),
                color);
            TankT90SMRunningGearDetails.Build(root, color);
            TankT90SMHullArmorDetails.Build(root, color);
            TankT90SMSternDetails.Build(root, color);
            AddFinalHullReceipts(root, color);
        }

        private static void AddFinalHullReceipts(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90SM-BellyEdgeChannel", root,
                    V(side * 1.00f, 0.39f, 0f),
                    V(0.035f, 0.12f, 4.40f), color * 0.48f);
            }
            Box("Painted-T90SM-CenterKeel", root,
                V(0f, 0.425f, 0f),
                V(0.09f, 0.05f, 4.40f), color * 0.48f);
            Box("T90SM-RightInnerSkirtLug", root,
                V(1.68f, 1.055f, -1.70f),
                V(0.04f, 0.37f, 0.33f), Rubber());
            Box("Painted-T90SM-WidthSliver", root,
                V(-1.88f, 1.365f, -1.60f),
                V(0.02f, 0.05f, 0.18f), color * 0.52f);
            Box("Painted-T90SM-RearDeckModule", root,
                V(0.20f, 1.465f, -2.81f),
                V(1.38f, 0.15f, 0.04f), color * 0.58f);
            Box("T90SM-RearDeckModuleSeam", root,
                V(0.20f, 1.532f, -2.81f),
                V(1.32f, 0.012f, 0.032f), Dark());
            Vector2[] feet =
            {
                new Vector2(-1.14f, 3.05f),
                new Vector2(1.14f, 3.05f),
                new Vector2(-1.68f, 0.83f),
                new Vector2(1.68f, 0.83f)
            };
            for (int index = 0; index < feet.Length; index++)
            {
                Box("Painted-T90SM-HullModuleFoot", root,
                    V(
                        feet[index].x,
                        feet[index].y > 3f ? 1.13f : 1.18f,
                        feet[index].y),
                    V(0.18f, 0.08f, 0.18f),
                    color * 0.54f);
            }
            foreach (float x in new[] { -1.14f, 1.14f })
            {
                Box("Painted-T90SM-BowModuleLap", root,
                    V(x, 1.18f, 2.92f),
                    V(0.18f, 0.025f, 0.12f),
                    color * 0.54f);
            }
        }

        private static TankHullProfilePoint[] Curve(
            params float[] values)
        {
            TankHullProfilePoint[] result =
                new TankHullProfilePoint[values.Length / 2];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = new TankHullProfilePoint(
                    values[index * 2],
                    values[index * 2 + 1]);
            }
            return result;
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

        private static void HideByPrefix(
            Transform root,
            string prefix)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < parts.Length; index++)
            {
                if (parts[index].name.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    HideRenderer(parts[index]);
                }
            }
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
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
            return new Color(0.23f, 0.227f, 0.20f);
        }
    }
}
