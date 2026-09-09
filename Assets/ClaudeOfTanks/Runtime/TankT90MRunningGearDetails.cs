using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MRunningGearDetails
    {
        private static readonly float[] RoadStations =
        {
            -1.65f, -0.93f, -0.21f, 0.51f, 1.23f, 1.95f
        };

        private static readonly float[] RollerStations =
        {
            -1.48f, -0.39f, 0.70f, 1.79f
        };

        public static void Build(
            Transform vehicleRoot,
            Transform parent)
        {
            HideByPrefix(vehicleRoot, "RoadWheel-");
            HideByPrefix(vehicleRoot, "Sprocket-");
            HideByPrefix(vehicleRoot, "Idler-");
            HideByPrefix(vehicleRoot, "ReturnRoller-");
            HideByPrefix(vehicleRoot, "TrackLinks-");
            AddRoadWheels(parent);
            AddEnds(parent);
            AddReturnRollers(parent);
            TankLinkedTrackShapeFactory.Build(
                "T90M",
                parent,
                1.435f,
                0.50f,
                0.09f,
                0.165f,
                0.98f,
                0.05f,
                2.22f,
                -2.14f,
                new TankTrackLoopEnd(
                    2.54f,
                    0.69f,
                    0.29f,
                    7),
                new TankTrackLoopEnd(
                    -2.46f,
                    0.84f,
                    0.33f,
                    7),
                new[]
                {
                    Support(-1.48f),
                    Support(-0.39f),
                    Support(0.70f),
                    Support(1.79f)
                },
                false,
                Track());
        }

        private static void AddRoadWheels(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < RoadStations.Length;
                    index++)
                {
                    float z = RoadStations[index];
                    Vector3 center = V(
                        side * 1.435f,
                        0.395f,
                        z);
                    Cylinder(
                        "T90M-RoadWheelTire",
                        root,
                        center,
                        0.31f,
                        0.22f,
                        26,
                        Tire());
                    Cylinder(
                        "Painted-T90M-RoadWheelDisc",
                        root,
                        center,
                        0.266f,
                        0.236f,
                        26,
                        Wheel());
                    Torus(
                        "T90M-RoadWheelOuterRim",
                        root,
                        V(side * 1.544f, 0.395f, z),
                        0.266f,
                        0.009f,
                        24,
                        Detail());
                    Torus(
                        "T90M-RoadWheelInnerRim",
                        root,
                        V(side * 1.545f, 0.395f, z),
                        0.150f,
                        0.007f,
                        18,
                        Detail());
                    Cylinder(
                        "Painted-T90M-RoadWheelHub",
                        root,
                        V(side * 1.543f, 0.395f, z),
                        0.090f,
                        0.052f,
                        14,
                        Wheel());
                    Cylinder(
                        "T90M-RoadWheelHubInset",
                        root,
                        V(side * 1.546f, 0.395f, z),
                        0.050f,
                        0.068f,
                        12,
                        Dark());
                    AddBolts(
                        root,
                        side,
                        z);
                }
            }
        }

        private static void AddBolts(
            Transform root,
            int side,
            float z)
        {
            const float orbit = 0.106f;
            for (int index = 0; index < 8; index++)
            {
                float angle =
                    index * Mathf.PI * 2f / 8f;
                Cylinder(
                    "T90M-RoadWheelBolt",
                    root,
                    V(
                        side * 1.548f,
                        0.395f + Mathf.Cos(angle) * orbit,
                        z + Mathf.Sin(angle) * orbit),
                    0.011f,
                    0.070f,
                    8,
                    Dark());
            }
        }

        private static void AddEnds(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                AddEnd(
                    root,
                    side,
                    "Sprocket",
                    -2.46f,
                    0.84f,
                    0.33f);
                AddEnd(
                    root,
                    side,
                    "Idler",
                    2.54f,
                    0.69f,
                    0.29f);
            }
        }

        private static void AddEnd(
            Transform root,
            int side,
            string label,
            float z,
            float y,
            float radius)
        {
            Vector3 center = V(side * 1.435f, y, z);
            Cylinder(
                "T90M-" + label + "Tire",
                root,
                center,
                radius,
                0.22f,
                22,
                Tire());
            Cylinder(
                "Painted-T90M-" + label,
                root,
                center,
                radius * 0.76f,
                0.235f,
                20,
                Wheel());
            Cylinder(
                "T90M-" + label + "Hub",
                root,
                V(side * 1.55f, y, z),
                radius * 0.30f,
                0.04f,
                14,
                Dark());
        }

        private static void AddReturnRollers(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < RollerStations.Length;
                    index++)
                {
                    Cylinder(
                        "Painted-T90M-ReturnRoller",
                        root,
                        V(
                            side * 1.435f,
                            0.97f,
                            RollerStations[index]),
                        0.096f,
                        0.14f,
                        14,
                        Wheel());
                }
            }
        }

        private static TankTrackSupport Support(float z)
        {
            return new TankTrackSupport(z, 0.97f, 0.096f);
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radius,
                radius,
                length,
                segments,
                TankShapeAxis.X,
                color);
            part.localPosition = position;
            return part;
        }

        private static Transform Torus(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float tubeRadius,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.TorusPart(
                name,
                parent,
                radius,
                tubeRadius,
                segments,
                color);
            part.localPosition = position;
            part.localRotation = Quaternion.Euler(0f, 90f, 0f);
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

        private static Color Wheel()
        {
            return new Color(0.20f, 0.22f, 0.173f);
        }

        private static Color Tire()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }

        private static Color Dark()
        {
            return new Color(0.125f, 0.145f, 0.118f);
        }

        private static Color Detail()
        {
            return new Color(0.19f, 0.20f, 0.17f);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }
    }
}
