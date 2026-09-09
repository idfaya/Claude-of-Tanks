using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t90m" || id == "t90m_proryv";
        }

        public static void BuildHull(
            Transform root,
            Color color)
        {
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideByPrefix(root, "Armor-");
            HideByPrefix(root, "SideArmor");

            GameObject presentationObject =
                new GameObject("T90M-HullPresentationRoot");
            Transform presentation = presentationObject.transform;
            presentation.SetParent(root, false);
            TankT90FamilyHullShapeFactory.Build(
                "Painted-T90M-HullLoft",
                presentation,
                color,
                -0.04f,
                1.35f);
            AddCenterGlacis(presentation, color);
            AddShoulderBridges(presentation, color);
        }

        private static void AddCenterGlacis(
            Transform root,
            Color color)
        {
            TankShapeFactory.OrientedSlabPart(
                "Painted-T90M-CenterGlacis",
                root,
                V(-1.00f, 0.78f, 3.16f),
                V(1.00f, 0.78f, 3.16f),
                V(1.42f, 1.36f, 2.63f),
                V(-1.42f, 1.36f, 2.63f),
                V(-0.96f, 0.85f, 3.11f),
                V(0.96f, 0.85f, 3.11f),
                V(1.38f, 1.43f, 2.58f),
                V(-1.38f, 1.43f, 2.58f),
                color * 0.60f);
            Transform seam = Box(
                "T90M-CenterGlacisSeam",
                root,
                V(0f, 0.94f, 3.055f),
                V(1.72f, 0.035f, 0.055f),
                Dark());
            seam.localRotation =
                Quaternion.Euler(-0.50f * Mathf.Rad2Deg, 0f, 0f);
        }

        private static void AddShoulderBridges(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                TankShapeFactory.OrientedSlabPart(
                    "Painted-T90M-ShoulderBridge",
                    root,
                    V(side * 0.92f, 1.33f, 2.76f),
                    V(side * 1.59f, 1.33f, 2.55f),
                    V(side * 1.69f, 1.36f, 2.16f),
                    V(side * 0.82f, 1.48f, 2.05f),
                    V(side * 0.92f, 1.41f, 2.73f),
                    V(side * 1.57f, 1.41f, 2.52f),
                    V(side * 1.66f, 1.46f, 2.18f),
                    V(side * 0.82f, 1.56f, 2.08f),
                    color * 0.58f);
                Transform seam = Box(
                    "T90M-ShoulderBridgeSeam",
                    root,
                    V(side * 1.25f, 1.415f, 2.54f),
                    V(0.48f, 0.035f, 0.055f),
                    Dark());
                seam.localRotation = Quaternion.Euler(
                    -0.23f * Mathf.Rad2Deg,
                    -side * 0.28f * Mathf.Rad2Deg,
                    0f);
            }
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

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            Transform part = TankShapeFactory.BoxPart(
                name,
                parent,
                scale,
                color);
            part.localPosition = position;
            return part;
        }

        private static Color Dark()
        {
            return new Color(0.20f, 0.22f, 0.16f);
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }
}
