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
            1.175f, -0.0416667f,
            -1.2583333f, -2.475f
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
                    -4.755f, 1.34f,
                    -3.15f, 1.40f,
                    -1.55f, 1.42f,
                    0.55f, 1.34f,
                    1.35f, 1.22f,
                    2.10f, 0.72f),
                Curve(
                    -4.755f, 0.24f,
                    -3.15f, 0.18f,
                    -1.2f, 0.18f,
                    0.75f, 0.22f,
                    2.10f, 0.34f),
                Curve(
                    -4.755f, 1.80f,
                    -3.15f, 1.84f,
                    -1.55f, 1.89f,
                    0.55f, 1.84f,
                    1.35f, 1.70f,
                    2.10f, 0.86f),
                Curve(
                    -4.755f, 1.70f,
                    -3.15f, 1.72f,
                    -1.55f, 1.75f,
                    0.55f, 1.72f,
                    1.35f, 1.62f,
                    2.10f, 0.72f),
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
                            0.95f,
                            ReturnRollerStations[index]),
                        0.11f,
                        0.11f,
                        0.16f,
                        18,
                        color * 0.40f);
                }
                Cylinder(
                    "T90AVladimir-Sprocket",
                    root,
                    new Vector3(side * 1.46f, 0.75f, -3.30f),
                    0.31f,
                    0.31f,
                    0.25f,
                    20,
                    color * 0.42f);
                Cylinder(
                    "T90AVladimir-Idler",
                    root,
                    new Vector3(side * 1.46f, 0.74f, 1.65f),
                    0.34f,
                    0.34f,
                    0.25f,
                    20,
                    color * 0.42f);
            }
        }

        private static void AddTrackRuns(Transform root)
        {
            const int horizontalPads = 58;
            const int verticalPads = 12;
            Color track = new Color(0.10f, 0.105f, 0.095f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < horizontalPads;
                    index++)
                {
                    float z = Mathf.Lerp(
                        -3.61f,
                        1.99f,
                        index / (horizontalPads - 1f));
                    Box(
                        "T90AVladimir-TrackPad",
                        root,
                        new Vector3(side * 1.46f, 0.095f, z),
                        new Vector3(0.56f, 0.065f, 0.092f),
                        track);
                    Box(
                        "T90AVladimir-TrackPad",
                        root,
                        new Vector3(side * 1.46f, 1.22f, z),
                        new Vector3(0.56f, 0.065f, 0.092f),
                        track);
                }
                for (int index = 1;
                    index < verticalPads - 1;
                    index++)
                {
                    float y = Mathf.Lerp(
                        0.095f,
                        1.22f,
                        index / (verticalPads - 1f));
                    Box(
                        "T90AVladimir-TrackPad",
                        root,
                        new Vector3(side * 1.46f, y, 1.99f),
                        new Vector3(0.56f, 0.092f, 0.065f),
                        track);
                    Box(
                        "T90AVladimir-TrackPad",
                        root,
                        new Vector3(side * 1.46f, y, -3.61f),
                        new Vector3(0.56f, 0.092f, 0.065f),
                        track);
                }
            }
        }

        private static void AddSideStructure(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-T90AVladimir-Fender",
                    root,
                    new Vector3(side * 1.75f, 1.30f, -1.30f),
                    new Vector3(0.18f, 0.12f, 5.45f),
                    color * 0.58f);
                Box(
                    "Painted-T90AVladimir-SideSkirt",
                    root,
                    new Vector3(side * 1.80f, 0.92f, -1.30f),
                    new Vector3(0.12f, 0.58f, 5.30f),
                    color * 0.52f);
                Box(
                    "Painted-T90AVladimir-MudFlap",
                    root,
                    new Vector3(side * 1.72f, 0.63f, -4.45f),
                    new Vector3(0.08f, 0.68f, 0.52f),
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
