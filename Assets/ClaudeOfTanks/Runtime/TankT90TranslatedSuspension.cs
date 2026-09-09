using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90TranslatedSuspension
    {
        private static readonly float[] WheelStations =
        {
            -1.9f, -1.12f, -0.34f, 0.44f, 1.22f, 2f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            if (root == null) return;
            for (int side = -1; side <= 1; side += 2)
            for (int index = 0; index < WheelStations.Length; index++)
                AddStation(root, color, side, WheelStations[index]);
        }

        private static void AddStation(
            Transform root,
            Color color,
            int side,
            float z)
        {
            Part("T90-GearRoadWheelTire", PrimitiveType.Cylinder, root,
                V(side * 1.625f, 0.48f, z), V(0.37f, 0.052f, 0.37f),
                V(0f, 0f, 90f), Dark());
            Part("T90-GearRoadWheelDisc", PrimitiveType.Cylinder, root,
                V(side * 1.63f, 0.48f, z), V(0.29f, 0.058f, 0.29f),
                V(0f, 0f, 90f), color * 0.48f);
            Part("T90-GearRoadWheelInset", PrimitiveType.Cylinder, root,
                V(side * 1.635f, 0.48f, z), V(0.16f, 0.064f, 0.16f),
                V(0f, 0f, 90f), Dark());
            Part("T90-GearSuspensionLink", PrimitiveType.Cube, root,
                V(side * 1.43f, 0.38f, z - 0.08f),
                V(0.06f, 0.05f, 0.48f), V(0f, 0f, side * 14f),
                color * 0.38f);
            Part("T90-GearSuspensionJointBoss", PrimitiveType.Cylinder, root,
                V(side * 1.42f, 0.54f, z - 0.16f),
                V(0.055f, 0.032f, 0.055f), V(0f, 0f, 90f),
                Dark());
            Part("T90-GearSuspensionJointBoss", PrimitiveType.Cylinder, root,
                V(side * 1.42f, 0.33f, z + 0.12f),
                V(0.045f, 0.03f, 0.045f), V(0f, 0f, 90f),
                Dark());
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Vector3 rotation,
            Color color)
        {
            Transform part = TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
            part.localRotation = Quaternion.Euler(rotation);
            return part;
        }

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.055f, 0.065f, 0.05f);
        }
    }
}
