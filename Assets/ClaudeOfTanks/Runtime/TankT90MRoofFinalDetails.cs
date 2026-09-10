using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MRoofFinalDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddSearchlight(root, color);
            AddFinalRoofPass(root, color);
            AddFinalRoofPass(root, color);
        }

        private static void AddSearchlight(
            Transform root,
            Color color)
        {
            Box(
                "Painted-T90M-SearchlightShoe",
                root,
                V(0.98f, 0.71f, 0.70f),
                V(0.46f, 0.22f, 0.46f),
                color * 0.54f,
                R(-0.16f, -0.22f, 0f));
            Box(
                "Painted-T90M-SearchlightYoke",
                root,
                V(0.78f, 0.80f, 0.76f),
                V(0.055f, 0.34f, 0.34f),
                color * 0.52f,
                R(-0.12f, 0f, -0.18f));
            Box(
                "Painted-T90M-SearchlightYoke",
                root,
                V(1.18f, 0.80f, 0.70f),
                V(0.055f, 0.34f, 0.34f),
                color * 0.52f,
                R(-0.12f, 0f, 0.18f));
            Cylinder(
                "Painted-T90M-SearchlightHousing",
                root,
                V(0.98f, 0.83f, 0.88f),
                0.245f,
                0.245f,
                0.32f,
                20,
                TankShapeAxis.Z,
                color * 0.50f,
                R(-0.06f, 0f, 0f));
            Cylinder(
                "T90M-SearchlightLens",
                root,
                V(0.98f, 0.83f, 1.055f),
                0.205f,
                0.205f,
                0.025f,
                20,
                TankShapeAxis.Z,
                Glass(),
                R(-0.06f, 0f, 0f));
            Torus(
                "T90M-SearchlightRim",
                root,
                V(0.98f, 0.83f, 1.070f),
                0.238f,
                0.025f,
                20,
                Detail(),
                Quaternion.identity);
        }

        private static void AddFinalRoofPass(
            Transform root,
            Color color)
        {
            Cylinder(
                "Painted-T90M-CommanderRoofCollar",
                root,
                V(-0.48f, 0.995f, -0.34f),
                0.39f,
                0.41f,
                0.060f,
                20,
                TankShapeAxis.Y,
                color * 0.56f,
                Quaternion.identity);
            Cylinder(
                "Painted-T90M-GunnerRoofCollar",
                root,
                V(0.38f, 0.985f, -0.28f),
                0.33f,
                0.35f,
                0.055f,
                18,
                TankShapeAxis.Y,
                color * 0.56f,
                Quaternion.identity);
            float[,] periscopes =
            {
                { -0.78f, -0.31f, -0.52f },
                { -0.65f, -0.04f, -0.20f },
                { -0.40f, 0.04f, 0.12f },
                { -0.19f, -0.12f, 0.38f },
                { 0.12f, -0.04f, -0.32f },
                { 0.39f, 0.03f, 0.02f },
                { 0.64f, -0.10f, 0.32f }
            };
            for (int index = 0;
                index < periscopes.GetLength(0);
                index++)
            {
                float x = periscopes[index, 0];
                float z = periscopes[index, 1];
                Quaternion rotation = Quaternion.Euler(
                    0f,
                    periscopes[index, 2] * Mathf.Rad2Deg,
                    0f);
                Box(
                    "T90M-FinalRoofPeriscopeSlot",
                    root,
                    V(x, 1.035f, z),
                    V(0.13f, 0.055f, 0.085f),
                    Dark(),
                    rotation);
                Box(
                    "T90M-FinalRoofPeriscopeGlass",
                    root,
                    V(x, 1.045f, z + 0.048f),
                    V(0.088f, 0.030f, 0.010f),
                    Glass(),
                    rotation);
            }
            AddEquipmentBoxes(root, color);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "T90M-RoofCableChannel",
                    root,
                    V(side * 1.03f, 0.67f, -0.50f),
                    V(0.035f, 0.028f, 0.90f),
                    Dark(),
                    R(0f, 0f, -side * 0.05f));
                Torus(
                    "T90M-RoofLiftEye",
                    root,
                    V(side * 1.05f, 0.71f, -0.92f),
                    0.070f,
                    0.015f,
                    12,
                    Detail(),
                    Quaternion.Euler(90f, 0f, 0f));
                Torus(
                    "T90M-RoofHatchHandle",
                    root,
                    V(side * 0.73f, 0.78f, -1.28f),
                    0.060f,
                    0.014f,
                    12,
                    Detail(),
                    Quaternion.Euler(90f, 0f, 0f));
            }
        }

        private static void AddEquipmentBoxes(
            Transform root,
            Color color)
        {
            float[,] boxes =
            {
                { 0.78f, 0.82f, -0.72f,
                    0.42f, 0.16f, 0.34f, 0.10f },
                { -0.98f, 0.75f, -0.98f,
                    0.36f, 0.13f, 0.42f, -0.12f },
                { 0.08f, 0.80f, -1.04f,
                    0.48f, 0.10f, 0.30f, 0.04f }
            };
            for (int index = 0;
                index < boxes.GetLength(0);
                index++)
            {
                float x = boxes[index, 0];
                float y = boxes[index, 1];
                float z = boxes[index, 2];
                float width = boxes[index, 3];
                float height = boxes[index, 4];
                float depth = boxes[index, 5];
                Quaternion rotation =
                    R(0f, boxes[index, 6], 0f);
                Box(
                    "Painted-T90M-RoofEquipmentTray",
                    root,
                    V(x, y - height * 0.45f, z),
                    V(width + 0.08f, 0.045f, depth + 0.08f),
                    color * 0.56f,
                    rotation);
                Box(
                    "Painted-T90M-RoofEquipmentBox",
                    root,
                    V(x, y, z),
                    V(width, height, depth),
                    color * 0.52f,
                    rotation);
                Box(
                    "T90M-RoofEquipmentLatch",
                    root,
                    V(x, y + height * 0.53f, z + depth * 0.32f),
                    V(width * 0.72f, 0.012f, 0.028f),
                    Dark(),
                    rotation);
            }
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

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color,
            Quaternion rotation)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            part.localRotation = rotation;
            return part;
        }

        private static Transform Torus(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float tube,
            int segments,
            Color color,
            Quaternion rotation)
        {
            Transform part = TankShapeFactory.TorusPart(
                name, parent, radius, tube, segments, color);
            part.localPosition = position;
            part.localRotation = rotation;
            return part;
        }

        private static Quaternion R(float x, float y, float z)
        {
            return Quaternion.Euler(
                x * Mathf.Rad2Deg,
                y * Mathf.Rad2Deg,
                z * Mathf.Rad2Deg);
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.20f, 0.22f, 0.16f);
        }

        private static Color Detail()
        {
            return new Color(0.28f, 0.30f, 0.24f);
        }

        private static Color Glass()
        {
            return new Color(0.10f, 0.17f, 0.18f);
        }
    }
}
