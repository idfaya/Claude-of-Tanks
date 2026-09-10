using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankChevronRow
    {
        public readonly float BottomY;
        public readonly float TopY;
        public readonly float BottomZOffset;
        public readonly float TopZOffset;

        public TankChevronRow(
            float bottomY,
            float topY,
            float bottomZOffset,
            float topZOffset)
        {
            BottomY = bottomY;
            TopY = topY;
            BottomZOffset = bottomZOffset;
            TopZOffset = topZOffset;
        }
    }

    internal static class TankSovietChevronArmorFactory
    {
        public static void Build(
            string prefix,
            Transform parent,
            Vector2[][] plans,
            TankChevronRow[] rows,
            Vector2[] tileRanges,
            float gasketDepth,
            float tileDepth,
            float tilePadY,
            Color carrierColor,
            Color gasketColor,
            Color tileColor)
        {
            Validate(prefix, parent, plans, rows, tileRanges);
            for (int side = -1; side <= 1; side += 2)
            for (int rowIndex = 0;
                rowIndex < rows.Length;
                rowIndex++)
            for (int planIndex = 0;
                planIndex < plans.Length;
                planIndex++)
            {
                BuildCarrier(
                    prefix + "-Carrier",
                    parent,
                    side,
                    plans[planIndex],
                    rows[rowIndex],
                    carrierColor);
                for (int tile = 0;
                    tile < tileRanges.Length;
                    tile++)
                {
                    BuildTile(
                        prefix + "-Gasket",
                        parent,
                        side,
                        plans[planIndex],
                        rows[rowIndex],
                        tileRanges[tile].x,
                        tileRanges[tile].y,
                        gasketDepth,
                        0.015f,
                        -0.006f,
                        gasketColor);
                    BuildTile(
                        prefix + "-Tile",
                        parent,
                        side,
                        plans[planIndex],
                        rows[rowIndex],
                        tileRanges[tile].x,
                        tileRanges[tile].y,
                        tileDepth,
                        0f,
                        tilePadY,
                        tileColor);
                }
            }
        }

        private static void BuildCarrier(
            string name,
            Transform parent,
            int side,
            Vector2[] plan,
            TankChevronRow row,
            Color color)
        {
            Vector3[] corners = new Vector3[8];
            for (int index = 0; index < 4; index++)
            {
                corners[index] = V(
                    side * plan[index].x,
                    row.BottomY,
                    plan[index].y + row.BottomZOffset);
                corners[index + 4] = V(
                    side * plan[index].x,
                    row.TopY,
                    plan[index].y + row.TopZOffset);
            }
            Slab(name, parent, corners, color);
        }

        private static void BuildTile(
            string name,
            Transform parent,
            int side,
            Vector2[] plan,
            TankChevronRow row,
            float start,
            float end,
            float depth,
            float padT,
            float padY,
            Color color)
        {
            Vector2 a = plan[1];
            Vector2 b = plan[2];
            float dx = b.x - a.x;
            float dz = b.y - a.y;
            float edgeLength = Mathf.Max(
                0.000001f,
                Mathf.Sqrt(dx * dx + dz * dz));
            float normalX = side * (-dz / edgeLength);
            float normalZ = dx / edgeLength;
            float low = Mathf.Max(0f, start - padT);
            float high = Mathf.Min(1f, end + padT);
            float lowY = row.BottomY + padY;
            float highY = row.TopY - padY;
            Vector3[] corners =
            {
                Point(side, a, dx, dz, row,
                    low, lowY, 0f, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    high, lowY, 0f, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    high, highY, 0f, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    low, highY, 0f, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    low, lowY, depth, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    high, lowY, depth, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    high, highY, depth, normalX, normalZ),
                Point(side, a, dx, dz, row,
                    low, highY, depth, normalX, normalZ)
            };
            Slab(name, parent, corners, color);
        }

        private static Vector3 Point(
            int side,
            Vector2 start,
            float dx,
            float dz,
            TankChevronRow row,
            float t,
            float y,
            float push,
            float normalX,
            float normalZ)
        {
            float height = Mathf.Max(
                0.000001f,
                row.TopY - row.BottomY);
            float zOffset = row.BottomZOffset +
                (row.TopZOffset - row.BottomZOffset) *
                ((y - row.BottomY) / height);
            return V(
                side * (start.x + dx * t) + normalX * push,
                y,
                start.y + dz * t + zOffset + normalZ * push);
        }

        private static void Slab(
            string name,
            Transform parent,
            Vector3[] corners,
            Color color)
        {
            TankShapeFactory.OrientedSlabPart(
                name,
                parent,
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

        private static void Validate(
            string prefix,
            Transform parent,
            Vector2[][] plans,
            TankChevronRow[] rows,
            Vector2[] ranges)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Chevron prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (plans == null || plans.Length == 0)
                throw new ArgumentException(
                    "Chevron plans are required.",
                    nameof(plans));
            for (int index = 0; index < plans.Length; index++)
                if (plans[index] == null ||
                    plans[index].Length != 4)
                    throw new ArgumentException(
                        "Chevron plans require four points.",
                        nameof(plans));
            if (rows == null || rows.Length != 2 ||
                !Mathf.Approximately(
                    rows[0].TopY,
                    rows[1].BottomY))
            {
                throw new ArgumentException(
                    "Chevron rows require one shared ridge.",
                    nameof(rows));
            }
            if (ranges == null || ranges.Length == 0)
                throw new ArgumentException(
                    "Chevron tile ranges are required.",
                    nameof(ranges));
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }
}
