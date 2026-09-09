using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AVladimirFamilyDetails
    {
        private static readonly float[] WheelStations =
        {
            1.03f, 0.28f, -0.47f,
            -1.22f, -1.97f, -2.72f
        };

        private static readonly float[] ReturnRollerStations =
        {
            -2.35f, -1.02f, 0.32f, 1.28f
        };

        public static bool Supports(string id)
        {
            return id == "t90a_vladimir";
        }

        public static float RoadWheelZ(int index)
        {
            return WheelStations[index];
        }

        public static void BuildHull(
            Transform root,
            Color color)
        {
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideByPrefix(root, "RoadWheel-");
            HideByPrefix(root, "Sprocket-");
            HideByPrefix(root, "Idler-");
            HideByPrefix(root, "TrackLinks-");
            TankHullLoftShapeFactory.Build(
                "Painted-T90AVladimir-HullLoft",
                root,
                Curve(
                    -4.755f, 1.51f,
                    -4.51f, 1.655f,
                    -4.29f, 1.671f,
                    -4.15f, 1.50f,
                    -4.13f, 1.50f,
                    -4.02f, 1.56f,
                    -3.92f, 1.51f,
                    -3.85f, 1.475f,
                    -3.72f, 1.51f,
                    -3.15f, 1.49f,
                    -3.05f, 1.47f,
                    -2.85f, 1.47f,
                    -2.72f, 1.50f,
                    -2.55f, 1.51f,
                    -0.92f, 1.50f,
                    -0.86f, 1.46f,
                    0.36f, 1.45f,
                    0.59f, 1.33f,
                    0.77f, 1.38f,
                    1.68f, 1.29f,
                    2.10f, 1.08f),
                Curve(
                    -4.755f, 1.50f,
                    -4.61f, 1.19f,
                    -4.46f, 1.20f,
                    -4.40f, 1.12f,
                    -4.30f, 0.80f,
                    -4.24f, 0.71f,
                    -4.13f, 0.71f,
                    -4.02f, 0.76f,
                    -3.92f, 0.83f,
                    -3.78f, 0.57f,
                    -2.87f, 0.42f,
                    1.22f, 0.42f,
                    1.68f, 0.60f,
                    2.10f, 1.08f),
                Curve(
                    -4.755f, 0.90f,
                    -4.32f, 0.95f,
                    -4.05f, 1.42f,
                    -3.95f, 1.60f,
                    -3.72f, 1.17f,
                    -3.00f, 1.17f,
                    -2.80f, 1.60f,
                    -2.70f, 1.58f,
                    -0.94f, 1.58f,
                    -0.82f, 1.60f,
                    1.22f, 1.60f,
                    1.35f, 1.17f,
                    1.68f, 1.05f,
                    2.10f, 0.94f),
                Curve(
                    -4.755f, 0.85f,
                    -4.32f, 0.90f,
                    -4.26f, 1.00f,
                    1.68f, 1.00f,
                    2.10f, 0.94f),
                1.22f,
                color);
            AddRunningGearFaces(root, color);
            AddTrackRuns(root);
            AddSideStructure(root, color);
        }

        private static void AddRunningGearFaces(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < WheelStations.Length;
                    index++)
                {
                    Cylinder(
                        "T90AVladimir-RoadWheelTire",
                        root,
                        new Vector3(
                            side * 1.46f,
                            0.50f,
                            WheelStations[index]),
                        0.375f,
                        0.375f,
                        0.21f,
                        24,
                        TankT90AFamilyDetails.Dark());
                    Transform dish = Cylinder(
                        "T90AVladimir-RoadWheelDish",
                        root,
                        new Vector3(
                            side * 1.46f,
                            0.50f,
                            WheelStations[index]),
                        0.33375f,
                        0.33375f,
                        0.025f,
                        24,
                        color * 0.48f);
                    dish.localScale =
                        new Vector3(1f, 0.88f, 1f);
                    Cylinder(
                        "T90AVladimir-RoadWheelHub",
                        root,
                        new Vector3(
                            side * 1.48f,
                            0.50f,
                            WheelStations[index]),
                        0.075f,
                        0.075f,
                        0.045f,
                        16,
                        TankT90AFamilyDetails.Dark());
                }
                for (int index = 0;
                    index < ReturnRollerStations.Length;
                    index++)
                {
                    Cylinder(
                        "T90AVladimir-ReturnRoller",
                        root,
                        new Vector3(
                            side * 1.40f,
                            0.86f,
                            ReturnRollerStations[index]),
                        0.086f,
                        0.086f,
                        0.16f,
                        18,
                        color * 0.40f);
                }
                Cylinder(
                    "T90AVladimir-Sprocket",
                    root,
                    new Vector3(side * 1.46f, 0.70f, -3.30f),
                    0.29f,
                    0.29f,
                    0.25f,
                    20,
                    color * 0.42f);
                Cylinder(
                    "T90AVladimir-Idler",
                    root,
                    new Vector3(side * 1.46f, 0.82f, 1.65f),
                    0.28f,
                    0.28f,
                    0.25f,
                    20,
                    color * 0.42f);
            }
        }

        private static void AddTrackRuns(Transform root)
        {
            Color track = new Color(0.10f, 0.105f, 0.095f);
            for (int side = -1; side <= 1; side += 2)
            {
                AddTrackSegment(root, side, track,
                    new Vector2(-2.91f, 0.11f),
                    new Vector2(1.31f, 0.11f), 44);
                AddTrackSegment(root, side, track,
                    new Vector2(1.31f, 0.11f),
                    new Vector2(1.93f, 0.72f), 9);
                AddTrackSegment(root, side, track,
                    new Vector2(1.93f, 0.72f),
                    new Vector2(1.65f, 0.90f), 5);
                AddTrackSegment(root, side, track,
                    new Vector2(1.65f, 0.90f),
                    new Vector2(-3.30f, 0.90f), 51);
                AddTrackSegment(root, side, track,
                    new Vector2(-3.30f, 0.90f),
                    new Vector2(-3.59f, 0.70f), 5);
                AddTrackSegment(root, side, track,
                    new Vector2(-3.59f, 0.70f),
                    new Vector2(-2.91f, 0.11f), 10);
            }
        }

        private static void AddTrackSegment(
            Transform root,
            int side,
            Color color,
            Vector2 start,
            Vector2 end,
            int count)
        {
            float angle = -Mathf.Atan2(
                end.y - start.y,
                end.x - start.x) * Mathf.Rad2Deg;
            for (int index = 0; index < count; index++)
            {
                float t = (index + 0.5f) / count;
                Vector2 point = Vector2.Lerp(start, end, t);
                Transform pad = Box(
                    "T90AVladimir-TrackPad",
                    root,
                    new Vector3(side * 1.46f, point.y, point.x),
                    new Vector3(0.56f, 0.03f, 0.09f),
                    color);
                pad.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }
        }

        private static void AddSideStructure(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 10; index++)
                {
                    Box(
                        "Painted-T90AVladimir-Fender",
                        root,
                        new Vector3(
                            side * 1.70f,
                            1.32f,
                            -3.90f + index * 0.545f),
                        new Vector3(0.16f, 0.05f, 0.48f),
                        color * 0.58f);
                }
                Box(
                    "Painted-T90AVladimir-FenderRoot",
                    root,
                    new Vector3(side * 1.40f, 1.30f, -3.86f),
                    new Vector3(0.50f, 0.04f, 0.54f),
                    color * 0.58f);
                Box(
                    "Painted-T90AVladimir-SideSkirt",
                    root,
                    new Vector3(side * 1.78f, 1.04f, -0.4375f),
                    new Vector3(0.10f, 0.52f, 4.275f),
                    color * 0.52f);
                Box(
                    "Painted-T90AVladimir-FrontFlap",
                    root,
                    new Vector3(side * 1.45f, 1.02f, 2.06f),
                    new Vector3(0.60f, 0.11f, 0.34f),
                    TankT90AFamilyDetails.Dark());
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

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radiusTop,
            float radiusBottom,
            float length,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radiusTop,
                radiusBottom,
                length,
                segments,
                TankShapeAxis.X,
                color);
            part.localPosition = position;
            return part;
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part = TankShapeFactory.BoxPart(
                name,
                parent,
                size,
                color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer = part == null
                ? null
                : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
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
                    System.StringComparison.Ordinal))
                    HideRenderer(parts[index]);
            }
        }
    }
}
