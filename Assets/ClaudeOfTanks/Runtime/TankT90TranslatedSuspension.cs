using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90TranslatedSuspension
    {
        private static readonly float[] WheelStations =
        {
            -1.9f, -1.12f, -0.34f, 0.44f, 1.22f, 2f
        };

        private static readonly float[] RollerStations =
        {
            -1.38f, 0.14f, 1.65f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            if (root == null) return;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < WheelStations.Length; index++)
                    AddStation(root, color, side, WheelStations[index]);
                AddEndWheels(root, color, side);
                AddReturnRollers(root, color, side);
            }
        }

        private static void AddStation(
            Transform root,
            Color color,
            int side,
            float z)
        {
            float x = side * 1.395f;
            Cylinder("T90-GearRoadWheelTire", root,
                V(x, 0.48f, z), 0.385f, 0.22f, 26, Dark());
            Cylinder("T90-GearRoadWheelShoulder", root,
                V(x, 0.48f, z), 0.3619f, 0.2266f, 26, Dark());
            Cylinder("T90-GearRoadWheelDisc", root,
                V(x, 0.48f, z), 0.3234f, 0.2464f, 26, color * 0.48f);
            Cylinder("T90-GearRoadWheelInset", root,
                V(x, 0.48f, z), 0.2695f, 0.2574f, 26, Dark());
            Cylinder("T90-GearRoadWheelHub", root,
                V(x, 0.48f, z), 0.0924f, 0.2948f, 12, color * 0.48f);
            Cylinder("T90-GearRoadWheelHubCap", root,
                V(x, 0.48f, z), 0.0539f, 0.3256f, 10, color * 0.48f);
            Part("T90-GearSuspensionLink", PrimitiveType.Cube, root,
                V(side * 1.2f, 0.38f, z - 0.08f),
                V(0.06f, 0.05f, 0.48f), V(0f, 0f, side * 14f),
                color * 0.38f);
            Cylinder("T90-GearSuspensionJointBoss", root,
                V(side * 1.18f, 0.54f, z - 0.16f),
                0.055f, 0.064f, 10, Dark());
            Cylinder("T90-GearSuspensionJointBoss", root,
                V(side * 1.18f, 0.33f, z + 0.12f),
                0.045f, 0.06f, 10, Dark());
        }

        private static void AddEndWheels(
            Transform root,
            Color color,
            int side)
        {
            float x = side * 1.395f;
            Cylinder("T90-Sprocket", root,
                V(x, 0.9f, -2.52f), 0.299f, 0.176f, 26,
                color * 0.45f);
            Cylinder("T90-Idler", root,
                V(x, 0.71f, 2.7f), 0.27f, 0.1628f, 26,
                color * 0.45f);
        }

        private static void AddReturnRollers(
            Transform root,
            Color color,
            int side)
        {
            float x = side * 1.395f;
            for (int index = 0; index < RollerStations.Length; index++)
            {
                Cylinder("T90-ReturnRoller", root,
                    V(x, 0.82f, RollerStations[index]),
                    0.086f, 0.305f, 20, Dark());
                Cylinder("T90-ReturnRollerDisc", root,
                    V(x, 0.82f, RollerStations[index]),
                    0.0654f, 0.3477f, 14, color * 0.45f);
            }
        }

        private static void Cylinder(
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
            if (type != PrimitiveType.Cube)
                throw new System.InvalidOperationException(
                    "Translated suspension parts must use C# shape factories.");
            Transform part = TankShapeFactory.BoxPart(
                name,
                parent,
                scale,
                color);
            part.localPosition = position;
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
