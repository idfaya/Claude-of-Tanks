using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t80" ||
                id == "t80b" ||
                id == "t80bv";
        }

        public static void BuildHull(
            Transform root,
            Color color,
            string id)
        {
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideByPrefix(root, "Armor-");

            Transform presentation =
                new GameObject("T80-HullPresentationRoot").transform;
            presentation.SetParent(root, false);
            TankHullLoftShapeFactory.Build(
                "Painted-T80-HullLoft",
                presentation,
                Curve(
                    -3.26f, 1.43f,
                    -2.90f, 1.41f,
                    -2.55f, 1.44f,
                    -1.95f, 1.465f,
                    -1.66f, 1.503f,
                    -1.36f, 1.503f,
                    -1.10f, 1.458f,
                    1.25f, 1.44f,
                    1.55f, 1.452f,
                    1.80f, 1.44f,
                    2.00f, 1.415f,
                    2.12f, 1.345f,
                    2.30f, 1.32f,
                    2.44f, 1.283f,
                    2.58f, 1.232f,
                    2.96f, 1.235f,
                    3.05f, 1.19f),
                Curve(
                    -3.26f, 1.35f,
                    -3.16f, 1.12f,
                    -3.06f, 0.90f,
                    -2.96f, 0.725f,
                    -2.86f, 0.73f,
                    -2.60f, 0.44f,
                    2.60f, 0.44f,
                    2.88f, 0.55f,
                    3.05f, 0.72f),
                Curve(
                    -3.26f, 1.28f,
                    3.05f, 1.28f),
                Curve(
                    -3.26f,
                    id == "t80bv" ? 1.02f : 1.05f,
                    3.05f, 1.02f),
                Curve(
                    -3.26f, 1.42f,
                    -2.32f, 1.42f,
                    -2.18f, 1.24f,
                    2.36f, 1.24f,
                    2.46f, 1.24f,
                    3.05f, 1.24f),
                color);
            TankT80RunningGearDetails.Build(
                root,
                presentation,
                color);
            TankT80HullExteriorDetails.Build(
                presentation,
                color,
                id);
            TankT80SternDetails.Build(
                presentation,
                id);
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
                if (!parts[index].name.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    continue;
                }
                HideRenderer(parts[index]);
            }
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
