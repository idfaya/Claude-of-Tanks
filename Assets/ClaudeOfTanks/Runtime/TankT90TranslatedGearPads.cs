using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90TranslatedGearPads
    {
        public static void Build(
            Transform root,
            Color color)
        {
            if (root == null) return;
            for (int side = -1; side <= 1; side += 2)
            {
                AddStraightRun(root, color, side, 30, 0.28f, -2.18f, 0.17f);
                AddStraightRun(root, color, side, 30, 0.88f, -2.05f, 0.16f);
                AddArcRun(root, color, side, 9, true);
                AddArcRun(root, color, side, 9, false);
            }
        }

        private static void AddStraightRun(
            Transform root,
            Color color,
            int side,
            int count,
            float y,
            float zStart,
            float spacing)
        {
            for (int index = 0; index < count; index++)
            {
                float z = zStart + index * spacing;
                Part(root, color, side, y, z, 0f);
            }
        }

        private static void AddArcRun(
            Transform root,
            Color color,
            int side,
            int count,
            bool front)
        {
            Vector2 center = front
                ? new Vector2(2.66f, 0.57f)
                : new Vector2(-2.48f, 0.58f);
            float radius = front ? 0.42f : 0.38f;
            float start = front ? -65f : 115f;
            float end = front ? 65f : 245f;
            for (int index = 0; index < count; index++)
            {
                float t = count == 1
                    ? 0f
                    : index / (float)(count - 1);
                float angle = Mathf.Lerp(start, end, t) * Mathf.Deg2Rad;
                float z = center.x + Mathf.Sin(angle) * radius;
                float y = center.y + Mathf.Cos(angle) * radius;
                Part(root, color, side, y, z, -angle * Mathf.Rad2Deg);
            }
        }

        private static void Part(
            Transform root,
            Color color,
            int side,
            float y,
            float z,
            float rotationX)
        {
            Transform part = TankShapeFactory.BoxPart(
                "T90-TrackPad",
                root,
                new Vector3(0.62f, 0.045f, 0.075f),
                color * 0.36f);
            part.localPosition = new Vector3(side * 1.395f, y, z);
            part.localRotation =
                Quaternion.Euler(rotationX, 0f, 0f);
        }
    }
}
