using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSSurfaceArmorFactory
    {
        private const float MiddleV = 0.46f;

        public static Transform BuildSkinPatch(
            string name,
            Transform root,
            int side,
            float frontZ,
            float rearZ,
            float lowerV,
            float upperV,
            float depth,
            float seat,
            Color color)
        {
            float[] zs = { frontZ, rearZ, rearZ, frontZ };
            float[] values = { lowerV, lowerV, upperV, upperV };
            Vector3[] corners = new Vector3[8];
            for (int index = 0; index < 4; index++)
            {
                Vector3 point =
                    SkinPoint(side, zs[index], values[index]);
                Vector3 normal =
                    SkinNormal(side, zs[index], values[index]);
                corners[index] = point + normal * seat;
                corners[index + 4] =
                    point + normal * (seat + depth);
            }
            return Slab(name, root, corners, color);
        }

        public static Transform BuildRoofPatch(
            string name,
            Transform root,
            float x0,
            float x1,
            float frontZ,
            float rearZ,
            float depth,
            float seat,
            Color color)
        {
            Vector2[] anchors =
            {
                new Vector2(x0, frontZ),
                new Vector2(x1, frontZ),
                new Vector2(x1, rearZ),
                new Vector2(x0, rearZ)
            };
            Vector3[] corners = new Vector3[8];
            for (int index = 0; index < anchors.Length; index++)
            {
                Vector3 point = RoofPoint(
                    anchors[index].x,
                    anchors[index].y);
                Vector3 normal = RoofNormal(anchors[index].y);
                corners[index] = point + normal * seat;
                corners[index + 4] =
                    point + normal * (seat + depth);
            }
            return Slab(name, root, corners, color);
        }

        private static Vector3 SkinPoint(
            int side,
            float z,
            float value)
        {
            TankWeldedStation station =
                TankT90MSTurretDetails.OuterSkinStationAt(z);
            float v = Mathf.Clamp01(value);
            float bottom = side > 0
                ? station.BottomRightX
                : station.BottomLeftX;
            float middle = side > 0
                ? station.MiddleRightX
                : station.MiddleLeftX;
            float top = side > 0
                ? station.TopRightX
                : station.TopLeftX;
            float x = v <= MiddleV
                ? Mathf.Lerp(bottom, middle, v / MiddleV)
                : Mathf.Lerp(
                    middle,
                    top,
                    (v - MiddleV) / (1f - MiddleV));
            return new Vector3(
                x,
                Mathf.Lerp(station.BottomY, station.TopY, v),
                z);
        }

        private static Vector3 SkinNormal(
            int side,
            float z,
            float value)
        {
            const float delta = 0.008f;
            Vector3 tangentZ =
                SkinPoint(side, z + delta, value) -
                SkinPoint(side, z - delta, value);
            Vector3 tangentV =
                SkinPoint(side, z, Mathf.Min(1f, value + delta)) -
                SkinPoint(side, z, Mathf.Max(0f, value - delta));
            Vector3 normal = Vector3.Cross(tangentV, tangentZ);
            if (normal.x * side < 0f) normal = -normal;
            float length = normal.magnitude;
            return length > 0f ? normal / length : Vector3.zero;
        }

        private static Vector3 RoofPoint(float x, float z)
        {
            TankWeldedStation station =
                TankT90MSTurretDetails.OuterSkinStationAt(z);
            return new Vector3(x, station.TopY, z);
        }

        private static Vector3 RoofNormal(float z)
        {
            const float delta = 0.008f;
            Vector3 tangent =
                RoofPoint(0f, z + delta) -
                RoofPoint(0f, z - delta);
            Vector3 normal =
                new Vector3(-tangent.y, tangent.x, 0f);
            float length = normal.magnitude;
            return length > 0f ? normal / length : Vector3.zero;
        }

        private static Transform Slab(
            string name,
            Transform root,
            Vector3[] corners,
            Color color)
        {
            return TankShapeFactory.OrientedSlabPart(
                name,
                root,
                corners[0],
                corners[1],
                corners[2],
                corners[3],
                corners[4],
                corners[5],
                corners[6],
                corners[7],
                color);
        }
    }
}
