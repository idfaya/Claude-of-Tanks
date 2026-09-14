using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildUrbanStructure(
            BuildingShape shape)
        {
            switch (shape.Kind)
            {
                case "parkingdeck":
                case "megatower":
                case "needletower":
                case "broadcasttower":
                case "terracetower":
                case "arcology":
                    BuildUrbanLandmark(shape);
                    return;
                case "church":
                    BuildChurch(shape, false);
                    return;
                case "onionchurch":
                    BuildChurch(shape, true);
                    return;
                case "bathhouse":
                    BuildBathhouse(shape);
                    return;
                case "civichall":
                    BuildCivicHall(shape);
                    return;
                case "tower":
                    BuildTownTower(shape);
                    return;
                default:
                    BuildRowhouse(
                        shape,
                        shape.Kind == "cornershop");
                    return;
            }
        }

        private static void BuildRowhouse(
            BuildingShape shape,
            bool cornerShop)
        {
            AddGableShell(shape, 0.78f);
            int floors = Mathf.Clamp(
                Mathf.RoundToInt(shape.Height / 3f),
                2,
                4);
            AddWindowGrid(shape, 3, floors, true);
            if (cornerShop)
            {
                Box(
                    shape,
                    shape.Details,
                    -shape.Width * 0.2f,
                    shape.Height * 0.17f,
                    shape.Depth * 0.51f,
                    shape.Width * 0.46f,
                    shape.Height * 0.25f,
                    0.12f);
                PitchedBox(
                    shape,
                    shape.Roofs,
                    -shape.Width * 0.2f,
                    shape.Height * 0.31f,
                    shape.Depth * 0.62f,
                    shape.Width * 0.52f,
                    0.12f,
                    shape.Depth * 0.24f,
                    -8f);
                Box(
                    shape,
                    shape.Roofs,
                    shape.Width * 0.32f,
                    shape.Height * 0.35f,
                    shape.Depth * 0.52f,
                    shape.Width * 0.28f,
                    0.42f,
                    0.1f);
            }
            else
            {
                Box(
                    shape,
                    shape.Details,
                    0f,
                    shape.Height * 0.16f,
                    shape.Depth * 0.51f,
                    shape.Width * 0.2f,
                    shape.Height * 0.32f,
                    0.12f);
            }
        }

        private static void BuildChurch(
            BuildingShape shape,
            bool onion)
        {
            BuildingShape nave = shape;
            nave.Depth *= 0.76f;
            nave.Center += Local(
                0f,
                0f,
                -shape.Depth * 0.1f,
                shape.Yaw);
            AddGableShell(nave, 0.58f);
            AddSideWindows(nave, 4);
            float towerZ =
                shape.Depth * 0.32f;
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.36f,
                towerZ,
                shape.Width * 0.5f,
                shape.Height * 0.72f,
                shape.Width * 0.5f);
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Details,
                    side * shape.Width * 0.11f,
                    shape.Height * 0.58f,
                    towerZ +
                        shape.Width * 0.255f,
                    shape.Width * 0.14f,
                    shape.Height * 0.16f,
                    0.1f);
            if (onion)
            {
                Dome(
                    shape,
                    shape.Roofs,
                    0f,
                    shape.Height * 0.72f,
                    towerZ,
                    shape.Width * 0.34f,
                    0.9f);
                Cone(
                    shape,
                    shape.Roofs,
                    0f,
                    shape.Height * 0.9f,
                    towerZ,
                    shape.Width * 0.2f,
                    shape.Height * 0.1f,
                    10);
                for (int side = -1;
                    side <= 1;
                    side += 2)
                    Dome(
                        shape,
                        shape.Roofs,
                        side * shape.Width * 0.3f,
                        shape.Height * 0.7f,
                        -shape.Depth * 0.2f,
                        shape.Width * 0.16f,
                        0.85f);
            }
            else
            {
                Cone(
                    shape,
                    shape.Roofs,
                    0f,
                    shape.Height * 0.72f,
                    towerZ,
                    shape.Width * 0.34f,
                    shape.Height * 0.28f,
                    8);
            }
        }

        private static void BuildBathhouse(
            BuildingShape shape)
        {
            AddFlatShell(shape, 0.62f);
            float[,] domes =
            {
                { -0.26f, -0.18f, 0.22f },
                { 0.24f, -0.18f, 0.2f },
                { 0f, 0.24f, 0.18f }
            };
            for (int i = 0; i < 3; i++)
                Dome(
                    shape,
                    shape.Roofs,
                    shape.Width * domes[i, 0],
                    shape.Height * 0.62f,
                    shape.Depth * domes[i, 1],
                    shape.Width * domes[i, 2],
                    0.58f);
            AddWindowGrid(shape, 2, 1);
        }

        private static void BuildCivicHall(
            BuildingShape shape)
        {
            AddFlatShell(shape, 0.72f);
            float front =
                shape.Depth * 0.58f;
            int columns = 9;
            for (int i = 0; i < columns; i++)
            {
                float x = -shape.Width * 0.4f +
                    shape.Width * 0.8f *
                    i / (columns - 1f);
                Cylinder(
                    shape,
                    shape.Details,
                    x,
                    0f,
                    front,
                    Mathf.Max(
                        0.25f,
                        shape.Width * 0.012f),
                    shape.Height * 0.62f,
                    10);
            }
            Box(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.64f,
                front,
                shape.Width * 0.92f,
                0.34f,
                shape.Depth * 0.24f);
            Dome(
                shape,
                shape.Roofs,
                -shape.Width * 0.1f,
                shape.Height * 0.72f,
                -shape.Depth * 0.05f,
                shape.Width * 0.22f,
                0.52f,
                16);
        }

        private static void BuildTownTower(
            BuildingShape shape)
        {
            AddFoundation(shape);
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.36f,
                0f,
                shape.Width,
                shape.Height * 0.72f,
                shape.Depth);
            Cone(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.72f,
                0f,
                shape.Width * 0.76f,
                shape.Height * 0.28f,
                4);
            AddWindowGrid(shape, 1, 3, true);
        }
    }
}
