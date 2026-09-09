using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90ABurlakFamilyDetails
    {
        private const float GearGauge = 0.975f;

        public static bool Supports(string id)
        {
            return id == "t90a_burlak";
        }

        public static void BuildHull(
            Transform root,
            Color color)
        {
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideByPrefix(root, "Armor-");
            HideByPrefix(root, "ReturnRoller-");

            Transform body = Root(
                "T90ABurlak-HullSection",
                root,
                new Vector3(0f, 0.12f, 0f),
                new Vector3(0.94f, 0.92f, 1.02f));
            TankT90AHullDetails.BuildBody(body, color);
            ReseatOutboardPlates(root, body);

            ScaleGenericGearGauge(root);
            Transform gear = Root(
                "T90ABurlak-RunningGearSection",
                root,
                Vector3.zero,
                new Vector3(GearGauge, 1f, 1f));
            TankT90AHullDetails.BuildRunningGear(gear, color);
            AddFenderClosures(root, color);
        }

        private static void ReseatOutboardPlates(
            Transform root,
            Transform body)
        {
            for (int index = 0; index < body.childCount; index++)
            {
                Transform part = body.GetChild(index);
                MeshFilter filter = part.GetComponent<MeshFilter>();
                if (filter?.sharedMesh == null) continue;
                Vector3[] vertices = filter.sharedMesh.vertices;
                float minimum = float.PositiveInfinity;
                float maximum = float.NegativeInfinity;
                for (int vertex = 0; vertex < vertices.Length; vertex++)
                {
                    float x = root.InverseTransformPoint(
                        part.TransformPoint(vertices[vertex])).x;
                    minimum = Mathf.Min(minimum, x);
                    maximum = Mathf.Max(maximum, x);
                }
                float delta = 0f;
                if (minimum > 1.50f)
                    delta = 1.68f - minimum;
                else if (maximum < -1.50f)
                    delta = -1.68f - maximum;
                if (Mathf.Abs(delta) <= 0.000001f) continue;
                part.position +=
                    root.TransformVector(new Vector3(delta, 0f, 0f));
            }
        }

        private static void AddFenderClosures(
            Transform root,
            Color color)
        {
            string[] labels = { "Centre", "Forward", "Shoulder" };
            float[] z0 = { -1.30f, 0f, 1.35f };
            float[] z1 = { 0f, 1.35f, 2.35f };
            float[] y0 = { 1.362f, 1.378f, 1.352f };
            float[] y1 = { 1.378f, 1.352f, 1.272f };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < labels.Length; index++)
                {
                    AddShelf(
                        root,
                        color,
                        side,
                        labels[index],
                        z0[index],
                        z1[index],
                        y0[index],
                        y1[index]);
                }
                AddBowShelf(root, color, side);
            }
        }

        private static void AddShelf(
            Transform root,
            Color color,
            int side,
            string label,
            float z0,
            float z1,
            float innerY0,
            float innerY1)
        {
            const float innerX = 1.43f;
            const float outerX = 1.72f;
            const float outerY = 1.25f;
            const float thickness = 0.055f;
            Slab(
                "Painted-T90ABurlak-FenderClosure-" + label,
                root,
                side,
                innerX,
                outerX,
                z0,
                z1,
                innerY0,
                innerY1,
                outerY,
                outerY,
                thickness,
                color * 0.6f);
            Box(
                "T90ABurlak-FenderOuterSeam",
                root,
                new Vector3(
                    side * 1.705f,
                    outerY + 0.01f,
                    (z0 + z1) * 0.5f),
                new Vector3(0.03f, 0.03f, z1 - z0 - 0.035f),
                Detail());
            Slab(
                "T90ABurlak-FenderSupportWeb",
                root,
                side,
                1.425f,
                1.455f,
                z0 + 0.035f,
                z1 - 0.035f,
                innerY0 - 0.055f,
                innerY1 - 0.055f,
                innerY0 - 0.055f,
                innerY1 - 0.055f,
                0.065f,
                Dark());
        }

        private static void AddBowShelf(
            Transform root,
            Color color,
            int side)
        {
            const float thickness = 0.055f;
            TankShapeFactory.OrientedSlabPart(
                "Painted-T90ABurlak-FenderClosure-Bow",
                root,
                V(side * 1.10f, 1.272f - thickness, 2.35f),
                V(side * 1.72f, 1.250f - thickness, 2.35f),
                V(side * 1.38f, 1.180f - thickness, 3.42f),
                V(side * 0.62f, 1.145f - thickness, 3.42f),
                V(side * 1.10f, 1.272f, 2.35f),
                V(side * 1.72f, 1.250f, 2.35f),
                V(side * 1.38f, 1.180f, 3.42f),
                V(side * 0.62f, 1.145f, 3.42f),
                color * 0.6f);
            TankShapeFactory.OrientedSlabPart(
                "T90ABurlak-FenderBowSeam",
                root,
                V(side * 1.695f, 1.230f, 2.38f),
                V(side * 1.725f, 1.230f, 2.38f),
                V(side * 1.395f, 1.162f, 3.39f),
                V(side * 1.365f, 1.162f, 3.39f),
                V(side * 1.695f, 1.260f, 2.38f),
                V(side * 1.725f, 1.260f, 2.38f),
                V(side * 1.395f, 1.192f, 3.39f),
                V(side * 1.365f, 1.192f, 3.39f),
                Detail());
        }

        private static void Slab(
            string name,
            Transform root,
            int side,
            float innerX,
            float outerX,
            float z0,
            float z1,
            float innerY0,
            float innerY1,
            float outerY0,
            float outerY1,
            float thickness,
            Color color)
        {
            TankShapeFactory.OrientedSlabPart(
                name,
                root,
                V(side * innerX, innerY0 - thickness, z0),
                V(side * outerX, outerY0 - thickness, z0),
                V(side * outerX, outerY1 - thickness, z1),
                V(side * innerX, innerY1 - thickness, z1),
                V(side * innerX, innerY0, z0),
                V(side * outerX, outerY0, z0),
                V(side * outerX, outerY1, z1),
                V(side * innerX, innerY1, z1),
                color);
        }

        private static void ScaleGenericGearGauge(Transform root)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < parts.Length; index++)
            {
                string name = parts[index].name;
                if (!name.StartsWith("RoadWheel-", StringComparison.Ordinal) &&
                    !name.StartsWith("Sprocket-", StringComparison.Ordinal) &&
                    !name.StartsWith("Idler-", StringComparison.Ordinal) &&
                    !name.StartsWith("TrackLinks-", StringComparison.Ordinal))
                {
                    continue;
                }
                Vector3 position = parts[index].localPosition;
                position.x *= GearGauge;
                parts[index].localPosition = position;
            }
        }

        private static Transform Root(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale)
        {
            Transform root =
                new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localScale = scale;
            return root;
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

        private static Color Detail()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }
    }
}
