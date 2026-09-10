using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80RunningGearDetails
    {
        private const float LaneX = 1.345f;
        private const float WheelRadius = 0.335f;
        private const float WheelWidth = 0.21f;
        private const float WheelY = 0.44f;
        private const float TrackWidth = 0.58f;

        private static readonly float[] RoadStations =
        {
            -1.60f, -0.88f, -0.16f, 0.56f, 1.28f, 2.00f
        };

        private static readonly float[] RollerStations =
        {
            -1.24f, -0.52f, 0.20f, 0.92f, 1.64f
        };

        public static void Build(
            Transform vehicleRoot,
            Transform parent,
            Color color)
        {
            HideByPrefix(vehicleRoot, "RoadWheel-");
            HideByPrefix(vehicleRoot, "Sprocket-");
            HideByPrefix(vehicleRoot, "Idler-");
            HideByPrefix(vehicleRoot, "ReturnRoller-");
            HideByPrefix(vehicleRoot, "TrackLinks-");
            AddRoadWheels(parent, color);
            AddSuspension(parent);
            TankT80EndWheelDetails.Build(parent, color);
            AddReturnRollers(parent, color);
            TankLinkedTrackShapeFactory.Build(
                "T80",
                parent,
                LaneX,
                TrackWidth,
                0.09f,
                0.165f,
                0.85f,
                0.06f,
                2.1675f,
                -1.7675f,
                new TankTrackLoopEnd(2.72f, 0.86f, 0.19f, 7),
                new TankTrackLoopEnd(-2.55f, 0.95f, 0.235f, 7),
                new[]
                {
                    Support(-1.24f),
                    Support(-0.52f),
                    Support(0.20f),
                    Support(0.92f),
                    Support(1.64f)
                },
                false,
                Track());
        }

        private static void AddRoadWheels(
            Transform root,
            Color color)
        {
            const float dishRadius = WheelRadius * 0.80f;
            const float ribOrbit =
                WheelRadius * (0.20f + 0.80f * 0.84f) * 0.5f;
            const float ribLength =
                WheelRadius * (0.80f * 0.84f - 0.20f);
            const float boltOrbit =
                WheelRadius * 0.80f * 0.70f;
            for (int side = -1; side <= 1; side += 2)
            for (int station = 0;
                station < RoadStations.Length;
                station++)
            {
                float z = RoadStations[station];
                Vector3 center = V(side * LaneX, WheelY, z);
                Cylinder("T80-RoadWheelTire", root, center,
                    WheelRadius, WheelWidth, 26, Rubber());
                Cylinder("T80-RoadWheelTireShoulder", root, center,
                    WheelRadius * 0.92f, WheelWidth * 1.02f, 26,
                    Rubber());
                Cylinder("Painted-T80-RoadWheelDisc", root, center,
                    dishRadius, WheelWidth * 1.12f, 26,
                    color * 0.52f);
                Cylinder("T80-RoadWheelDishWell", root, center,
                    WheelRadius * 0.67f, WheelWidth * 1.17f, 26,
                    Dark());
                for (int rib = 0; rib < 6; rib++)
                {
                    float angle =
                        rib * Mathf.PI * 2f / 6f + 0.08f;
                    Box(
                        "Painted-T80-RoadWheelRib",
                        root,
                        V(
                            side * LaneX,
                            WheelY + Mathf.Sin(angle) * ribOrbit,
                            z + Mathf.Cos(angle) * ribOrbit),
                        V(
                            WheelWidth * 1.24f,
                            WheelRadius * 0.12f,
                            ribLength),
                        color * 0.54f,
                        Quaternion.Euler(
                            -angle * Mathf.Rad2Deg,
                            0f,
                            0f));
                }
                Cylinder("Painted-T80-RoadWheelHub", root, center,
                    WheelRadius * 0.24f, WheelWidth * 1.34f, 12,
                    color * 0.56f);
                Cylinder("Painted-T80-RoadWheelHubCap", root, center,
                    WheelRadius * 0.14f, WheelWidth * 1.52f, 10,
                    color * 0.58f);
                for (int bolt = 0; bolt < 6; bolt++)
                {
                    float angle =
                        bolt * Mathf.PI * 2f / 6f + 0.13f;
                    Cylinder(
                        "T80-RoadWheelBolt",
                        root,
                        V(
                            side * LaneX,
                            WheelY + Mathf.Sin(angle) * boltOrbit,
                            z + Mathf.Cos(angle) * boltOrbit),
                        WheelRadius * 0.040f,
                        WheelWidth * 1.40f,
                        6,
                        Dark());
                }
            }
        }

        private static void AddSuspension(Transform root)
        {
            const float anchorY =
                WheelY + WheelRadius * 0.82f;
            const float trail = WheelRadius * 1.05f;
            const float armWidth = WheelWidth * 0.58f;
            const float armHeight = WheelRadius * 0.18f;
            const float jointRadius = WheelRadius * 0.17f;
            const float jointWidth = WheelWidth * 0.72f;
            const float suspensionX = 1.0978f;
            float length = Mathf.Sqrt(
                trail * trail +
                (anchorY - WheelY) * (anchorY - WheelY));
            float angle = Mathf.Atan2(
                anchorY - WheelY,
                -trail) * Mathf.Rad2Deg;
            for (int side = -1; side <= 1; side += 2)
            for (int station = 0;
                station < RoadStations.Length;
                station++)
            {
                float wheelZ = RoadStations[station];
                float anchorZ = wheelZ + trail;
                Transform arm = TankShapeFactory.CylinderPart(
                    "T80-SuspensionArm",
                    root,
                    0.36f,
                    0.50f,
                    1f,
                    8,
                    TankShapeAxis.Z,
                    Dark());
                arm.localPosition = V(
                    side * suspensionX,
                    (anchorY + WheelY) * 0.5f,
                    (anchorZ + wheelZ) * 0.5f);
                arm.localScale = V(armWidth, armHeight, length);
                arm.localRotation = Quaternion.Euler(angle, 0f, 0f);
                AddJoint(root, side, anchorY, anchorZ,
                    jointRadius, jointWidth);
                AddJoint(root, side, WheelY, wheelZ,
                    jointRadius, jointWidth);
            }
        }

        private static void AddJoint(
            Transform root,
            int side,
            float y,
            float z,
            float radius,
            float width)
        {
            Vector3 center = V(side * 1.0978f, y, z);
            Cylinder("T80-SuspensionJoint", root, center,
                radius, width * 0.72f, 8, Dark());
            Cylinder("T80-SuspensionJointStep", root, center,
                radius * 0.67f, width, 8, Dark());
            Cylinder("T80-SuspensionJointCap", root, center,
                radius * 0.30f, width * 1.12f, 8, Dark());
        }

        private static void AddReturnRollers(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            for (int index = 0;
                index < RollerStations.Length;
                index++)
            {
                Vector3 center =
                    V(side * LaneX, 0.86f, RollerStations[index]);
                Cylinder("T80-ReturnRollerTire", root, center,
                    0.08f, TrackWidth * 0.50f, 20, Rubber());
                Cylinder("T80-ReturnRollerTireShoulder", root, center,
                    0.0736f, TrackWidth * 0.54f, 20, Rubber());
                Cylinder("Painted-T80-ReturnRollerDisc", root, center,
                    0.0608f, TrackWidth * 0.57f, 20,
                    color * 0.50f);
                Cylinder("Painted-T80-ReturnRollerHub", root, center,
                    0.0368f, TrackWidth * 0.63f, 8,
                    color * 0.54f);
                Cylinder("Painted-T80-ReturnRollerCap", root, center,
                    0.0144f, TrackWidth * 0.69f, 8,
                    color * 0.56f);
            }
        }

        private static TankTrackSupport Support(float z)
        {
            return new TankTrackSupport(z, 0.86f, 0.08f);
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

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color,
            Quaternion rotation)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            part.localRotation = rotation;
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
            return new Color(0.16f, 0.165f, 0.155f);
        }

        private static Color Dark()
        {
            return new Color(0.15f, 0.155f, 0.145f);
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }
    }
}
