using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t90ms";
        }

        public static void BuildHull(
            Transform root,
            Color color)
        {
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideByPrefix(root, "Armor-");
            TankHullLoftShapeFactory.Build(
                "Painted-T90MS-HullLoft",
                root,
                Curve(
                    -3.43f, 1.35f,
                    -3.20f, 1.47f,
                    -3.00f, 1.52f,
                    -2.55f, 1.545f,
                    0.95f, 1.545f,
                    1.40f, 1.50f,
                    1.75f, 1.46f,
                    2.30f, 1.40f,
                    2.90f, 1.26f,
                    3.43f, 1.04f),
                Curve(
                    -3.43f, 1.05f,
                    -3.36f, 0.86f,
                    -3.10f, 0.72f,
                    -2.62f, 0.48f,
                    -2.40f, 0.44f,
                    2.45f, 0.44f,
                    2.80f, 0.56f,
                    3.10f, 0.71f,
                    3.43f, 0.82f),
                Curve(
                    -3.43f, 1.02f,
                    -3.09f, 1.30f,
                    -2.96f, 1.60f,
                    2.95f, 1.60f,
                    3.16f, 1.32f,
                    3.43f, 0.60f),
                Curve(
                    -3.43f, 0.64f,
                    -2.95f, 0.88f,
                    -2.30f, 0.94f,
                    2.35f, 0.94f,
                    2.85f, 0.88f,
                    3.43f, 0.64f),
                Curve(
                    -3.43f, 1.22f,
                    -2.90f, 1.22f,
                    -2.82f, 1.40f,
                    -2.05f, 1.40f,
                    -1.80f, 1.22f,
                    2.42f, 1.22f,
                    3.43f, 1.22f),
                color);
            TankShapeFactory.OrientedSlabPart(
                "Painted-T90MS-CenterGlacis",
                root,
                V(-1.06f, 1.34f, 1.75f),
                V(1.06f, 1.34f, 1.75f),
                V(1.06f, 0.72f, 3.40f),
                V(-1.06f, 0.72f, 3.40f),
                V(-1.06f, 1.46f, 1.75f),
                V(1.06f, 1.46f, 1.75f),
                V(1.06f, 0.84f, 3.43f),
                V(-1.06f, 0.84f, 3.43f),
                color * 0.58f);
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
    }
}
