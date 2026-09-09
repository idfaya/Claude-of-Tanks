using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSRunningGearDetails
    {
        private static readonly float[] RoadStations =
        {
            -1.78f, -0.992f, -0.204f, 0.584f, 1.372f, 2.16f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            HideByPrefix(root, "RoadWheel-");
            HideByPrefix(root, "Sprocket-");
            HideByPrefix(root, "Idler-");
            HideByPrefix(root, "ReturnRoller-");
            HideByPrefix(root, "TrackLinks-");
            AddRoadWheels(root);
            AddEnds(root, color);
            AddReturnRollers(root, color);
            TankLinkedTrackShapeFactory.Build(
                "T90MS",
                root,
                1.395f,
                0.61f,
                0.09f,
                0.165f,
                0.86f,
                0.05f,
                2.4125f,
                -2.0325f,
                new TankTrackLoopEnd(
                    2.76f,
                    0.69f,
                    0.25f,
                    7),
                new TankTrackLoopEnd(
                    -2.58f,
                    0.95f,
                    0.20f,
                    7),
                new[]
                {
                    new TankTrackSupport(
                        -1.38f, 0.82f, 0.086f),
                    new TankTrackSupport(
                        0.14f, 0.82f, 0.086f),
                    new TankTrackSupport(
                        1.65f, 0.82f, 0.086f)
                },
                false,
                Track());
        }

        private static void AddRoadWheels(Transform root)
        {
            const float radius = 0.34f;
            const float width = 0.22f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < RoadStations.Length;
                    index++)
                {
                    Vector3 position = V(
                        side * 1.395f,
                        0.35f,
                        RoadStations[index]);
                    Cylinder("T90MS-RoadWheelTire", root,
                        position,
                        radius,
                        radius,
                        width,
                        26,
                        Tire());
                    Cylinder("Painted-T90MS-RoadWheelDish", root,
                        position,
                        radius * 0.72f,
                        radius * 0.72f,
                        width * 1.12f,
                        26,
                        Wheel());
                    Cylinder("T90MS-RoadWheelWell", root,
                        position,
                        radius * 0.70f,
                        radius * 0.70f,
                        width * 1.17f,
                        26,
                        Dark());
                    Cylinder("Painted-T90MS-RoadWheelHub", root,
                        position,
                        radius * 0.53f,
                        radius * 0.53f,
                        width * 1.23f,
                        18,
                        Wheel());
                    Cylinder("T90MS-RoadWheelHubWell", root,
                        position,
                        radius * 0.32f,
                        radius * 0.32f,
                        width * 1.27f,
                        14,
                        Dark());
                    Cylinder("Painted-T90MS-RoadWheelHubCap", root,
                        position,
                        radius * 0.14f,
                        radius * 0.14f,
                        width * 1.48f,
                        10,
                        Wheel());
                    AddBolts(root, position);
                }
            }
        }

        private static void AddBolts(
            Transform root,
            Vector3 center)
        {
            const float orbit = 0.1088f;
            for (int index = 0; index < 8; index++)
            {
                float angle =
                    index * Mathf.PI * 2f / 8f + 0.20f;
                Cylinder(
                    "T90MS-RoadWheelBolt",
                    root,
                    V(
                        center.x,
                        center.y + Mathf.Sin(angle) * orbit,
                        center.z + Mathf.Cos(angle) * orbit),
                    0.011424f,
                    0.011424f,
                    0.2552f,
                    6,
                    Dark());
            }
        }

        private static void AddEnds(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder("Painted-T90MS-Sprocket", root,
                    V(side * 1.395f, 0.95f, -2.58f),
                    0.20f, 0.20f, 0.22f, 20,
                    color * 0.44f);
                Cylinder("Painted-T90MS-Idler", root,
                    V(side * 1.395f, 0.69f, 2.76f),
                    0.25f, 0.25f, 0.22f, 20,
                    color * 0.46f);
            }
        }

        private static void AddReturnRollers(
            Transform root,
            Color color)
        {
            foreach (float z in new[] { -1.38f, 0.14f, 1.65f })
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Cylinder("Painted-T90MS-ReturnRoller", root,
                        V(side * 1.395f, 0.82f, z),
                        0.086f, 0.086f, 0.14f, 14,
                        color * 0.42f);
                }
            }
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                top,
                bottom,
                length,
                segments,
                TankShapeAxis.X,
                color);
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
                if (!parts[index].name.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    continue;
                }
                Renderer renderer =
                    parts[index].GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Tire()
        {
            return Rgb(0x34, 0x37, 0x2f);
        }

        private static Color Wheel()
        {
            return Rgb(0x68, 0x68, 0x4d);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }

        private static Color Rgb(int red, int green, int blue)
        {
            const float scale = 1f / 255f;
            return new Color(
                red * scale,
                green * scale,
                blue * scale);
        }
    }
}
