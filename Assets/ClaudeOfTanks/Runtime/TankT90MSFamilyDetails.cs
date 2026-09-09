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
            TankT90FamilyHullShapeFactory.Build(
                "Painted-T90MS-HullLoft",
                root,
                color);
            TankT90MSRunningGearDetails.Build(root, color);
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
            TankT90MSHullArmorDetails.Build(root, color);
            TankT90MSSternDetails.Build(root, color);
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
