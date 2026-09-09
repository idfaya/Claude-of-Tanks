using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMRunningGearDetails
    {
        private static readonly float[] RoadStations =
        {
            -1.89f, -1.08f, -0.27f, 0.54f, 1.35f, 2.16f
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
            AddRoadWheels(root, color);
            AddEnds(root, color);
            AddReturnRollers(root, color);
            TankLinkedTrackShapeFactory.Build(
                "T90SM",
                root,
                1.405f,
                0.44f,
                0.165f,
                0.83f,
                0.05f,
                2.20f,
                -1.45f,
                new TankTrackLoopEnd(
                    2.90f,
                    0.78f,
                    0.21f,
                    18),
                new TankTrackLoopEnd(
                    -2.42f,
                    0.90f,
                    0.258f,
                    14),
                new[]
                {
                    new TankTrackSupport(
                        -1.40f, 0.80f, 0.086f),
                    new TankTrackSupport(
                        0f, 0.80f, 0.086f),
                    new TankTrackSupport(
                        1.44f, 0.80f, 0.086f)
                },
                true,
                Track());
        }

        private static void AddRoadWheels(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < RoadStations.Length;
                    index++)
                {
                    Vector3 position = V(
                        side * 1.405f,
                        0.46f,
                        RoadStations[index]);
                    Cylinder(
                        "Painted-T90SM-RoadWheel",
                        root,
                        position,
                        0.385f,
                        0.385f,
                        0.20f,
                        20,
                        TankShapeAxis.X,
                        color * 0.44f);
                    Cylinder(
                        "T90SM-RoadWheelTire",
                        root,
                        position,
                        0.385f,
                        0.385f,
                        0.032f,
                        20,
                        TankShapeAxis.X,
                        Rubber());
                    Cylinder(
                        "Painted-T90SM-RoadWheelHub",
                        root,
                        V(
                            side * (1.405f + 0.112f),
                            0.46f,
                            RoadStations[index]),
                        0.11f,
                        0.13f,
                        0.024f,
                        14,
                        TankShapeAxis.X,
                        color * 0.50f);
                }
            }
        }

        private static void AddEnds(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder(
                    "Painted-T90SM-Sprocket",
                    root,
                    V(side * 1.405f, 0.90f, -2.42f),
                    0.258f,
                    0.258f,
                    0.20f,
                    18,
                    TankShapeAxis.X,
                    color * 0.42f);
                Cylinder(
                    "Painted-T90SM-Idler",
                    root,
                    V(side * 1.405f, 0.78f, 2.90f),
                    0.21f,
                    0.21f,
                    0.20f,
                    18,
                    TankShapeAxis.X,
                    color * 0.46f);
            }
        }

        private static void AddReturnRollers(
            Transform root,
            Color color)
        {
            float[] stations = { -1.40f, 0f, 1.44f };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < stations.Length; index++)
                {
                    Cylinder(
                        "Painted-T90SM-ReturnRoller",
                        root,
                        V(side * 1.405f, 0.80f, stations[index]),
                        0.086f,
                        0.086f,
                        0.14f,
                        14,
                        TankShapeAxis.X,
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
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                top,
                bottom,
                length,
                segments,
                axis,
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

        private static Color Rubber()
        {
            return new Color(0.23f, 0.227f, 0.20f);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }
    }
}
