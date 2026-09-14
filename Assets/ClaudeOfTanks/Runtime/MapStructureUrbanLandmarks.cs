using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildUrbanLandmark(
            BuildingShape shape)
        {
            switch (shape.Kind)
            {
                case "parkingdeck":
                    BuildParkingDeck(shape);
                    return;
                case "arcology":
                    BuildArcology(shape);
                    return;
                case "megatower":
                    BuildMegaTower(shape);
                    return;
                case "needletower":
                    BuildSetbackTower(
                        shape,
                        CrownStyle.Needle);
                    return;
                case "broadcasttower":
                    BuildSetbackTower(
                        shape,
                        CrownStyle.Broadcast);
                    return;
                default:
                    BuildSetbackTower(
                        shape,
                        CrownStyle.Forked);
                    return;
            }
        }

        private static void BuildParkingDeck(
            BuildingShape shape)
        {
            int floors = 5;
            float floorHeight =
                shape.Height / floors;
            for (int floor = 0;
                floor <= floors;
                floor++)
                Box(
                    shape,
                    shape.Bodies,
                    0f,
                    floor * floorHeight + 0.16f,
                    0f,
                    shape.Width,
                    0.32f,
                    shape.Depth);
            float[] columns =
            {
                -0.44f,
                -0.16f,
                0.16f,
                0.44f
            };
            for (int column = 0;
                column < columns.Length;
                column++)
                for (int z = -1;
                    z <= 1;
                    z += 2)
                    Box(
                        shape,
                        shape.Bodies,
                        shape.Width * columns[column],
                        shape.Height * 0.5f,
                        z * shape.Depth * 0.44f,
                        0.62f,
                        shape.Height,
                        0.62f);
            PitchedBox(
                shape,
                shape.Roofs,
                -shape.Width * 0.18f,
                shape.Height * 0.44f,
                0f,
                shape.Width * 0.26f,
                0.28f,
                shape.Depth * 0.68f,
                -14f);
            for (int floor = 0;
                floor < floors;
                floor++)
                Box(
                    shape,
                    shape.Details,
                    0f,
                    floor * floorHeight +
                        floorHeight * 0.58f,
                    shape.Depth * 0.505f,
                    shape.Width * 0.88f,
                    floorHeight * 0.3f,
                    0.08f);
        }

        private static void BuildArcology(
            BuildingShape shape)
        {
            float towerWidth =
                shape.Width * 0.37f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float height = shape.Height *
                    (side < 0 ? 0.84f : 0.72f);
                Box(
                    shape,
                    shape.Bodies,
                    side * shape.Width * 0.28f,
                    height * 0.5f,
                    0f,
                    towerWidth,
                    height,
                    shape.Depth * 0.9f);
                AddTowerWindows(
                    shape,
                    side * shape.Width * 0.28f,
                    towerWidth,
                    shape.Depth * 0.9f,
                    height,
                    3);
                AddCrown(
                    shape,
                    side * shape.Width * 0.28f,
                    0f,
                    height,
                    towerWidth * 0.42f,
                    side < 0
                        ? CrownStyle.Forked
                        : CrownStyle.Needle);
            }
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.48f,
                -shape.Depth * 0.06f,
                shape.Width * 0.7f,
                shape.Height * 0.09f,
                shape.Depth * 0.28f);
            Box(
                shape,
                shape.Details,
                shape.Width * 0.08f,
                shape.Height * 0.48f,
                -shape.Depth * 0.05f,
                shape.Width * 0.18f,
                shape.Height * 0.07f,
                shape.Depth * 0.3f);
        }

        private static void BuildMegaTower(
            BuildingShape shape)
        {
            float podium =
                shape.Height * 0.1f;
            Box(
                shape,
                shape.Bodies,
                0f,
                podium * 0.5f,
                0f,
                shape.Width,
                podium,
                shape.Depth);
            Box(
                shape,
                shape.Bodies,
                0f,
                podium + shape.Height * 0.34f,
                0f,
                shape.Width * 0.78f,
                shape.Height * 0.68f,
                shape.Depth * 0.82f);
            Box(
                shape,
                shape.Bodies,
                -shape.Width * 0.18f,
                shape.Height * 0.82f,
                -shape.Depth * 0.05f,
                shape.Width * 0.3f,
                shape.Height * 0.28f,
                shape.Depth * 0.58f);
            AddTowerWindows(
                shape,
                0f,
                shape.Width * 0.78f,
                shape.Depth * 0.82f,
                shape.Height * 0.66f,
                5);
            AddBrokenCrown(
                shape,
                shape.Width * 0.18f,
                shape.Depth * 0.15f,
                shape.Height * 0.68f);
            AddCrown(
                shape,
                -shape.Width * 0.18f,
                -shape.Depth * 0.05f,
                shape.Height * 0.78f,
                shape.Width * 0.16f,
                CrownStyle.Broadcast);
        }

        private static void BuildSetbackTower(
            BuildingShape shape,
            CrownStyle crown)
        {
            float[] widths =
            {
                1f,
                0.76f,
                0.52f
            };
            float[] heights =
            {
                0.44f,
                0.22f,
                0.14f
            };
            float cursor = 0f;
            for (int stage = 0;
                stage < widths.Length;
                stage++)
            {
                float stageHeight =
                    shape.Height * heights[stage];
                Box(
                    shape,
                    stage == 2
                        ? shape.Roofs
                        : shape.Bodies,
                    stage == 1
                        ? -shape.Width * 0.07f
                        : 0f,
                    cursor + stageHeight * 0.5f,
                    stage == 2
                        ? shape.Depth * 0.04f
                        : 0f,
                    shape.Width * widths[stage],
                    stageHeight,
                    shape.Depth *
                        (widths[stage] + 0.05f));
                AddTowerWindows(
                    shape,
                    stage == 1
                        ? -shape.Width * 0.07f
                        : 0f,
                    shape.Width * widths[stage],
                    shape.Depth *
                        (widths[stage] + 0.05f),
                    cursor + stageHeight,
                    stage == 0 ? 5 : 3,
                    cursor);
                cursor += stageHeight;
            }
            AddCrown(
                shape,
                0f,
                shape.Depth * 0.04f,
                cursor,
                shape.Width * 0.2f,
                crown);
        }

        private static void AddTowerWindows(
            BuildingShape shape,
            float x,
            float width,
            float depth,
            float top,
            int columns,
            float bottom = 0f)
        {
            int floors = Mathf.Clamp(
                Mathf.RoundToInt(
                    (top - bottom) / 3f),
                2,
                14);
            for (int floor = 0;
                floor < floors;
                floor++)
            {
                float y = bottom +
                    (floor + 0.55f) *
                    (top - bottom) / floors;
                for (int column = 0;
                    column < columns;
                    column++)
                {
                    float localX = x - width * 0.4f +
                        width * 0.8f *
                        (column + 0.5f) / columns;
                    Box(
                        shape,
                        shape.Details,
                        localX,
                        y,
                        depth * 0.505f,
                        width * 0.55f / columns,
                        Mathf.Min(
                            1.25f,
                            (top - bottom) /
                            (floors + 1f)),
                        0.08f);
                }
            }
        }

        private static void AddBrokenCrown(
            BuildingShape shape,
            float x,
            float z,
            float baseY)
        {
            for (int level = 0;
                level < 4;
                level++)
            {
                float width =
                    shape.Width *
                    (0.34f - level * 0.025f);
                float y = baseY +
                    shape.Height * 0.07f *
                    (level + 1);
                Box(
                    shape,
                    shape.Bodies,
                    x,
                    y,
                    z,
                    width,
                    0.34f,
                    shape.Depth *
                        (0.32f - level * 0.018f));
                for (int column = -1;
                    column <= 1;
                    column++)
                    Box(
                        shape,
                        shape.Details,
                        x + column * width * 0.34f,
                        y - shape.Height * 0.035f,
                        z,
                        0.38f,
                        shape.Height * 0.07f,
                        0.38f);
            }
        }

        private static void AddCrown(
            BuildingShape shape,
            float x,
            float z,
            float roofY,
            float width,
            CrownStyle style)
        {
            float available = Mathf.Max(
                0.8f,
                shape.Height - roofY);
            Box(
                shape,
                shape.Roofs,
                x,
                roofY + 0.18f,
                z,
                width * 2f,
                0.36f,
                width * 2f);
            if (style == CrownStyle.Broadcast)
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                    PitchedBox(
                        shape,
                        shape.Details,
                        x + side * width * 0.3f,
                        roofY + available * 0.34f,
                        z,
                        0.14f,
                        available * 0.68f,
                        0.14f,
                        0f,
                        side * 5f);
                Cylinder(
                    shape,
                    shape.Details,
                    x,
                    roofY + available * 0.68f,
                    z,
                    0.08f,
                    available * 0.3f,
                    8);
                return;
            }
            if (style == CrownStyle.Forked)
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    PitchedBox(
                        shape,
                        shape.Details,
                        x + side * width * 0.28f,
                        roofY + available * 0.3f,
                        z,
                        0.24f,
                        available * 0.6f,
                        0.28f,
                        0f,
                        side * 5f);
                    Cone(
                        shape,
                        shape.Roofs,
                        x + side * width * 0.2f,
                        roofY + available * 0.6f,
                        z,
                        width * 0.14f,
                        available * 0.4f,
                        6);
                }
                return;
            }
            Cone(
                shape,
                shape.Roofs,
                x,
                roofY + available * 0.08f,
                z,
                width * 0.55f,
                available * 0.72f,
                8);
            Cylinder(
                shape,
                shape.Details,
                x,
                roofY + available * 0.72f,
                z,
                0.08f,
                available * 0.26f,
                8);
        }

        private enum CrownStyle
        {
            Needle,
            Forked,
            Broadcast
        }
    }
}
