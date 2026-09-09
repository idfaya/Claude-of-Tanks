using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMTurretFinalDetails
    {
        private const float PlanScaleZ = 0.95f;

        public static void Build(
            Transform root,
            Color color)
        {
            AddRelikt(root);
            AddRearTower(root, color);
            AddRoofSensor(root);
            AddLeftStowage(root, color);
        }

        private static void AddRelikt(Transform root)
        {
            const float centerY = 0.25f;
            const float width = 0.48f;
            const float height = 0.34f;
            const float depth = 0.22f;
            const float tilt = -0.34f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 3; index++)
                {
                    float theta =
                        Mathf.PI * 0.5f +
                        side * (0.28f + index * 0.28f);
                    float distance =
                        SkinDistance(theta, centerY) - 0.05f;
                    Vector3 position = V(
                        Mathf.Cos(theta) * distance,
                        centerY,
                        Mathf.Sin(theta) * distance);
                    Quaternion rotation = Quaternion.Euler(
                        tilt * Mathf.Rad2Deg,
                        (Mathf.PI * 0.5f - theta) *
                            Mathf.Rad2Deg,
                        0f);
                    Transform cassette = Box(
                        "T90SM-TurretRelikt",
                        root,
                        position,
                        V(width, height, depth),
                        Track());
                    cassette.localRotation = rotation;
                    Transform cover = Box(
                        "T90SM-TurretReliktCover",
                        root,
                        position + rotation * V(0f, 0f, 0.114f),
                        V(0.3936f, 0.2788f, 0.014f),
                        Dark());
                    cover.localRotation = rotation;

                    float stripDistance =
                        SkinDistance(theta, 0.34f) - 0.03f;
                    Transform strip = Box(
                        "T90SM-TurretReliktStrip",
                        root,
                        V(
                            Mathf.Cos(theta) * stripDistance,
                            0.34f,
                            Mathf.Sin(theta) * stripDistance),
                        V(0.50f, 0.032f, 0.20f),
                        Dark());
                    strip.localRotation = Quaternion.Euler(
                        -0.30f * Mathf.Rad2Deg,
                        (Mathf.PI * 0.5f - theta) *
                            Mathf.Rad2Deg,
                        0f);
                }
            }
        }

        private static float SkinDistance(float theta, float y)
        {
            float radius = RingSkin(y);
            float a = radius;
            float b = radius * PlanScaleZ;
            float x = Mathf.Cos(theta) / a;
            float z = Mathf.Sin(theta) / b;
            return 1f / Mathf.Sqrt(x * x + z * z);
        }

        private static float RingSkin(float y)
        {
            if (y <= 0.309f)
            {
                return Mathf.Lerp(
                    1.55f,
                    1.488f,
                    y / 0.309f);
            }
            if (y <= 0.515f)
            {
                return Mathf.Lerp(
                    1.488f,
                    1.395f,
                    (y - 0.309f) / (0.515f - 0.309f));
            }
            return 1.395f;
        }

        private static void AddRearTower(
            Transform root,
            Color color)
        {
            Box("Painted-T90SM-RearTowerBody", root,
                V(-0.50f, 0.44f, -1.98f),
                V(0.30f, 0.20f, 0.30f), color * 0.54f);
            Box("Painted-T90SM-RearTowerPanel", root,
                V(-0.50f, 0.65f, -2.03f),
                V(0.26f, 0.38f, 0.05f), color * 0.54f);
            Box("T90SM-RearTowerLens", root,
                V(-0.50f, 0.62f, -2.030f),
                V(0.18f, 0.22f, 0.016f), Glass());
            Box("T90SM-RearTowerHood", root,
                V(-0.50f, 0.825f, -2.05f),
                V(0.26f, 0.03f, 0.02f), Dark());
        }

        private static void AddRoofSensor(Transform root)
        {
            Transform sensor = Cylinder(
                "T90SM-RoofSensor",
                root,
                V(0.32f, 0.72f, -0.90f),
                0.024f,
                0.024f,
                0.62f,
                8,
                TankShapeAxis.Z,
                Dark());
            sensor.localRotation =
                Quaternion.Euler(-0.04f * Mathf.Rad2Deg, 0f, 0f);
        }

        private static void AddLeftStowage(
            Transform root,
            Color color)
        {
            Box("Painted-T90SM-LeftRoofStowage", root,
                V(-0.85f, 0.52f, -0.27f),
                V(0.30f, 0.36f, 0.30f), color * 0.54f);
            Box("T90SM-LeftRoofStowageLid", root,
                V(-0.85f, 0.694f, -0.27f),
                V(0.26f, 0.012f, 0.26f), Dark());
            foreach (float x in new[] { -0.79f, -0.91f })
            {
                Box("T90SM-LeftRoofStowageLatch", root,
                    V(x, 0.60f, -0.123f),
                    V(0.022f, 0.05f, 0.014f), Dark());
            }
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
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }

        private static Color Glass()
        {
            return TankT90AFamilyDetails.Glass();
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }
    }
}
