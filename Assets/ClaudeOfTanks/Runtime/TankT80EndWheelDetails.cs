using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80EndWheelDetails
    {
        private const float LaneX = 1.345f;
        private const float TrackWidth = 0.58f;

        public static void Build(
            Transform root,
            Color color)
        {
            AddSprockets(root, color);
            AddIdlers(root, color);
        }

        private static void AddSprockets(
            Transform root,
            Color color)
        {
            const float radius = 0.235f;
            const float width = TrackWidth * 0.80f;
            const float ringOffset = TrackWidth * 0.5f * 0.99f;
            const float rootRadius = 0.201f;
            const float tipRadius = 0.311f;
            const float toothMid = (rootRadius + tipRadius) * 0.5f;
            const float toothDepth =
                Mathf.PI * (rootRadius + tipRadius) / 10f * 0.46f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 center = V(side * LaneX, 0.95f, -2.55f);
                Cylinder("Painted-T80-SprocketBody", root, center,
                    radius * 0.88f, radius * 0.88f,
                    width * 0.80f, 26, color * 0.46f);
                Cylinder("Painted-T80-SprocketHub", root, center,
                    radius * 0.30f, radius * 0.30f,
                    width * 1.14f, 12, color * 0.50f);
                Cylinder("Painted-T80-SprocketHubCap", root, center,
                    radius * 0.17f, radius * 0.17f,
                    width * 1.26f, 10, color * 0.54f);
                for (int face = -1; face <= 1; face += 2)
                {
                    float x = side * LaneX + face * ringOffset;
                    Torus("Painted-T80-SprocketCarrierRing", root,
                        V(x, 0.95f, -2.55f),
                        radius * 0.84f, radius * 0.10f, 18,
                        color * 0.48f);
                    Torus("T80-SprocketRecessRing", root,
                        V(x, 0.95f, -2.55f),
                        radius * 0.69f, radius * 0.055f, 18,
                        Dark());
                    AddTeeth(root, x, toothMid, toothDepth,
                        rootRadius, tipRadius);
                }
                AddSprocketBolts(root, center, radius, width);
            }
        }

        private static void AddTeeth(
            Transform root,
            float x,
            float toothMid,
            float toothDepth,
            float rootRadius,
            float tipRadius)
        {
            for (int tooth = 0; tooth < 10; tooth++)
            {
                float angle =
                    tooth * Mathf.PI * 2f / 10f +
                    Mathf.PI * 0.5f;
                Box(
                    "T80-SprocketTooth",
                    root,
                    V(
                        x,
                        0.95f + Mathf.Sin(angle) * toothMid,
                        -2.55f + Mathf.Cos(angle) * toothMid),
                    V(
                        TrackWidth * 0.80f * 0.13f,
                        tipRadius - rootRadius,
                        toothDepth),
                    Dark(),
                    Quaternion.Euler(
                        (Mathf.PI * 0.5f - angle) *
                        Mathf.Rad2Deg,
                        0f,
                        0f));
            }
        }

        private static void AddSprocketBolts(
            Transform root,
            Vector3 center,
            float radius,
            float width)
        {
            for (int bolt = 0; bolt < 8; bolt++)
            {
                float angle = bolt * Mathf.PI * 2f / 8f;
                Cylinder(
                    "T80-SprocketBolt",
                    root,
                    V(
                        center.x,
                        center.y + Mathf.Sin(angle) * radius * 0.44f,
                        center.z + Mathf.Cos(angle) * radius * 0.44f),
                    0.020f,
                    0.020f,
                    width * 1.06f,
                    6,
                    Dark());
            }
        }

        private static void AddIdlers(
            Transform root,
            Color color)
        {
            const float radius = 0.19f;
            const float width = TrackWidth * 0.74f;
            const float dishDepth = 0.05f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 center = V(side * LaneX, 0.86f, 2.72f);
                Cylinder("Painted-T80-IdlerBody", root, center,
                    radius * 0.97f, radius * 0.97f,
                    width * 0.80f, 26, color * 0.48f);
                for (int face = -1; face <= 1; face += 2)
                {
                    float x = side * LaneX +
                        face * (width * 0.40f + dishDepth * 0.5f);
                    Cylinder(
                        "Painted-T80-IdlerDish",
                        root,
                        V(x, 0.86f, 2.72f),
                        face < 0
                            ? radius * 0.34f
                            : radius * 0.94f,
                        face < 0
                            ? radius * 0.94f
                            : radius * 0.34f,
                        dishDepth,
                        26,
                        color * 0.50f);
                    Torus(
                        "T80-IdlerRim",
                        root,
                        V(
                            side * LaneX +
                                face * (width * 0.40f +
                                dishDepth * 0.78f),
                            0.86f,
                            2.72f),
                        radius * 0.91f,
                        radius * 0.065f,
                        18,
                        Dark());
                }
                Cylinder("Painted-T80-IdlerHub", root, center,
                    radius * 0.26f, radius * 0.26f,
                    width + dishDepth * 1.6f, 14,
                    color * 0.52f);
                Cylinder("Painted-T80-IdlerHubCap", root, center,
                    radius * 0.15f, radius * 0.15f,
                    width + dishDepth * 2.1f, 10,
                    color * 0.54f);
                AddIdlerHardware(root, center, radius, width, dishDepth);
            }
        }

        private static void AddIdlerHardware(
            Transform root,
            Vector3 center,
            float radius,
            float width,
            float dishDepth)
        {
            for (int hole = 0; hole < 6; hole++)
            {
                float angle =
                    hole * Mathf.PI * 2f / 6f + 0.35f;
                Cylinder(
                    "T80-IdlerLighteningHole",
                    root,
                    V(
                        center.x,
                        center.y + Mathf.Sin(angle) * radius * 0.48f,
                        center.z + Mathf.Cos(angle) * radius * 0.48f),
                    radius * 0.085f,
                    radius * 0.085f,
                    width * 0.90f + dishDepth * 2.5f,
                    8,
                    Dark());
            }
            for (int bolt = 0; bolt < 8; bolt++)
            {
                float angle =
                    bolt * Mathf.PI * 2f / 8f + 0.20f;
                Cylinder(
                    "T80-IdlerBolt",
                    root,
                    V(
                        center.x,
                        center.y + Mathf.Sin(angle) * radius * 0.30f,
                        center.z + Mathf.Cos(angle) * radius * 0.30f),
                    0.022f,
                    0.022f,
                    width + dishDepth * 1.6f,
                    6,
                    Dark());
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
                name, parent, top, bottom, length, segments,
                TankShapeAxis.X, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Torus(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float tube,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.TorusPart(
                name, parent, radius, tube, segments, color);
            part.localPosition = position;
            part.localRotation = Quaternion.Euler(0f, 90f, 0f);
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

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.15f, 0.155f, 0.145f);
        }
    }
}
