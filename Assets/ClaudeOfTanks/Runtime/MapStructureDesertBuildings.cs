using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildDesertStructure(
            BuildingShape shape)
        {
            switch (shape.Kind)
            {
                case "deserttent":
                    BuildDesertTent(shape);
                    return;
                case "minaret":
                    BuildMinaret(shape);
                    return;
                case "caravanserai":
                    BuildCaravanserai(shape);
                    return;
                case "compound":
                case "compoundSouk":
                    BuildCompound(
                        shape,
                        shape.Kind == "compoundSouk");
                    return;
                case "market":
                case "marketRow":
                    BuildMarket(
                        shape,
                        shape.Kind == "marketRow");
                    return;
                default:
                    BuildAdobe(shape);
                    return;
            }
        }

        private static void BuildAdobe(
            BuildingShape shape)
        {
            AddFlatShell(shape, 0.76f);
            float wall =
                shape.Height * 0.76f;
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    shape,
                    shape.Bodies,
                    0f,
                    wall + 0.24f,
                    side * shape.Depth * 0.47f,
                    shape.Width,
                    0.48f,
                    0.2f);
                Box(
                    shape,
                    shape.Bodies,
                    side * shape.Width * 0.47f,
                    wall + 0.24f,
                    0f,
                    0.2f,
                    0.48f,
                    shape.Depth);
            }
            int beams = Mathf.Clamp(
                Mathf.RoundToInt(shape.Width / 0.9f),
                4,
                10);
            for (int i = 0; i < beams; i++)
            {
                float x = -shape.Width * 0.42f +
                    shape.Width * 0.84f *
                    i / (beams - 1f);
                PitchedBox(
                    shape,
                    shape.Details,
                    x,
                    wall - 0.28f,
                    shape.Depth * 0.53f,
                    0.13f,
                    0.13f,
                    0.68f,
                    90f);
            }
            AddFramedWindow(
                shape,
                -shape.Width * 0.24f,
                shape.Height * 0.38f,
                shape.Depth * 0.51f,
                shape.Width * 0.14f,
                shape.Height * 0.2f);
        }

        private static void BuildMarket(
            BuildingShape shape,
            bool row)
        {
            int stalls = row ? 3 : 1;
            float stallWidth =
                shape.Width / stalls;
            for (int i = 0; i < stalls; i++)
            {
                float x = -shape.Width * 0.5f +
                    stallWidth * (i + 0.5f);
                Box(
                    shape,
                    shape.Bodies,
                    x,
                    shape.Height * 0.28f,
                    -shape.Depth * 0.3f,
                    stallWidth * 0.9f,
                    shape.Height * 0.56f,
                    shape.Depth * 0.38f);
                for (int side = -1; side <= 1; side += 2)
                    Box(
                        shape,
                        shape.Details,
                        x + side *
                            stallWidth * 0.38f,
                        shape.Height * 0.34f,
                        shape.Depth * 0.25f,
                        0.12f,
                        shape.Height * 0.68f,
                        0.12f);
                PitchedBox(
                    shape,
                    shape.Roofs,
                    x,
                    shape.Height * 0.7f,
                    shape.Depth * 0.08f,
                    stallWidth,
                    0.12f,
                    shape.Depth * 0.68f,
                    -9f);
            }
        }

        private static void BuildCompound(
            BuildingShape shape,
            bool souk)
        {
            float wing = Mathf.Max(
                2.4f,
                shape.Width * 0.2f);
            float wallHeight =
                shape.Height * 0.66f;
            Box(
                shape,
                shape.Bodies,
                0f,
                wallHeight * 0.5f,
                -shape.Depth * 0.4f,
                shape.Width,
                wallHeight,
                wing);
            Box(
                shape,
                shape.Bodies,
                0f,
                wallHeight * 0.5f,
                shape.Depth * 0.4f,
                shape.Width,
                wallHeight,
                wing);
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Bodies,
                    side * shape.Width * 0.4f,
                    wallHeight * 0.5f,
                    0f,
                    wing,
                    wallHeight,
                    shape.Depth - wing * 2f);
            Box(
                shape,
                shape.Details,
                0f,
                wallHeight * 0.34f,
                shape.Depth * 0.51f,
                shape.Width * 0.22f,
                wallHeight * 0.68f,
                0.12f);
            if (!souk) return;
            for (int i = -2; i <= 2; i++)
            {
                float x = i * shape.Width * 0.15f;
                Box(
                    shape,
                    shape.Details,
                    x,
                    wallHeight * 0.26f,
                    -shape.Depth * 0.18f,
                    0.12f,
                    wallHeight * 0.52f,
                    0.12f);
            }
            PitchedBox(
                shape,
                shape.Roofs,
                0f,
                wallHeight * 0.55f,
                -shape.Depth * 0.18f,
                shape.Width * 0.72f,
                0.12f,
                shape.Depth * 0.26f,
                -6f);
        }

        private static void BuildCaravanserai(
            BuildingShape shape)
        {
            BuildCompound(shape, true);
            float towerWidth =
                Mathf.Min(
                    shape.Width,
                    shape.Depth) * 0.18f;
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    shape,
                    shape.Bodies,
                    side * shape.Width * 0.32f,
                    shape.Height * 0.43f,
                    shape.Depth * 0.36f,
                    towerWidth,
                    shape.Height * 0.86f,
                    towerWidth);
                Box(
                    shape,
                    shape.Roofs,
                    side * shape.Width * 0.32f,
                    shape.Height * 0.88f,
                    shape.Depth * 0.36f,
                    towerWidth * 1.12f,
                    0.22f,
                    towerWidth * 1.12f);
            }
        }

        private static void BuildMinaret(
            BuildingShape shape)
        {
            float radius =
                Mathf.Min(shape.Width, shape.Depth) *
                0.24f;
            Cylinder(
                shape,
                shape.Bodies,
                0f,
                0f,
                0f,
                radius,
                shape.Height * 0.72f,
                12);
            Cylinder(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.58f,
                0f,
                radius * 1.32f,
                0.3f,
                12);
            Cone(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.72f,
                0f,
                radius * 1.05f,
                shape.Height * 0.2f,
                12);
            Cylinder(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.92f,
                0f,
                0.08f,
                shape.Height * 0.08f,
                6);
        }

        private static void BuildDesertTent(
            BuildingShape shape)
        {
            AddGableRoof(
                shape.Bodies,
                shape.Center,
                shape.Width,
                shape.Depth,
                shape.Height * 0.82f,
                shape.Yaw);
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Details,
                    0f,
                    shape.Height * 0.5f,
                    side * shape.Depth * 0.52f,
                    0.12f,
                    shape.Height,
                    0.12f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    PitchedBox(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.55f,
                        shape.Height * 0.2f,
                        z * shape.Depth * 0.55f,
                        0.04f,
                        shape.Height * 0.7f,
                        0.04f,
                        x * z * 34f);
        }
    }
}
