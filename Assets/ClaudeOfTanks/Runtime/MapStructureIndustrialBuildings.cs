using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildIndustrialStructure(
            BuildingShape shape)
        {
            switch (shape.Kind)
            {
                case "containerRow":
                    BuildContainerRow(shape);
                    return;
                case "gantry":
                    BuildGantry(shape);
                    return;
                case "stack":
                    BuildStack(shape);
                    return;
                case "watertower":
                    BuildWaterTower(shape);
                    return;
                case "motorpool":
                    BuildMotorPool(shape);
                    return;
                case "fieldhospital":
                    BuildFieldHospital(shape);
                    return;
                case "checkpointhut":
                    BuildCheckpoint(shape);
                    return;
                case "transformershed":
                    BuildTransformerShed(shape);
                    return;
                case "commandtent":
                    BuildCommandTent(shape);
                    return;
                case "guardpost":
                    BuildGuardPost(shape);
                    return;
                case "quonsethut":
                    BuildQuonset(shape);
                    return;
                case "firestation":
                    BuildFireStation(shape);
                    return;
            }
            AddGableShell(shape, 0.7f);
            AddWindowGrid(shape, 4, 2, true);
            AddLoadingDoor(shape);
            if (shape.Kind == "factory")
                AddFactoryStack(shape);
            else if (shape.Kind == "foundryoffice")
                AddSawtoothRoof(shape);
            else if (shape.Kind == "depot")
                AddDepotPlatform(shape);
        }

        private static void AddLoadingDoor(
            BuildingShape shape)
        {
            Box(
                shape,
                shape.Details,
                -shape.Width * 0.16f,
                shape.Height * 0.25f,
                shape.Depth * 0.51f,
                shape.Width * 0.34f,
                shape.Height * 0.5f,
                0.12f);
            for (int i = 1; i < 5; i++)
                Box(
                    shape,
                    shape.Roofs,
                    -shape.Width * 0.16f,
                    shape.Height * 0.1f * i,
                    shape.Depth * 0.53f,
                    shape.Width * 0.32f,
                    0.07f,
                    0.1f);
        }

        private static void AddFactoryStack(
            BuildingShape shape)
        {
            float radius =
                Mathf.Min(shape.Width, shape.Depth) *
                0.075f;
            AddTaperedCylinder(
                shape.Bodies,
                shape.Center +
                    Local(
                        shape.Width * 0.3f,
                        0f,
                        -shape.Depth * 0.32f,
                        shape.Yaw),
                radius * 1.3f,
                radius * 0.85f,
                shape.Height * 1.55f,
                shape.Yaw,
                12);
            Cylinder(
                shape,
                shape.Roofs,
                shape.Width * 0.3f,
                shape.Height * 1.48f,
                -shape.Depth * 0.32f,
                radius * 1.05f,
                shape.Height * 0.12f,
                12);
        }

        private static void AddSawtoothRoof(
            BuildingShape shape)
        {
            for (int i = 0; i < 3; i++)
            {
                float z = -shape.Depth * 0.34f +
                    i * shape.Depth * 0.34f;
                PitchedBox(
                    shape,
                    shape.Roofs,
                    shape.Width * 0.08f,
                    shape.Height * 0.8f,
                    z,
                    shape.Width * 0.98f,
                    0.15f,
                    shape.Depth * 0.32f,
                    0f,
                    -16f);
                Box(
                    shape,
                    shape.Details,
                    -shape.Width * 0.42f,
                    shape.Height * 0.82f,
                    z,
                    0.1f,
                    shape.Height * 0.22f,
                    shape.Depth * 0.28f);
            }
            AddFactoryStack(shape);
        }

        private static void AddDepotPlatform(
            BuildingShape shape)
        {
            Box(
                shape,
                shape.Bodies,
                0f,
                0.35f,
                shape.Depth * 0.62f,
                shape.Width * 1.12f,
                0.7f,
                shape.Depth * 0.22f);
            for (int i = -2; i <= 2; i++)
                Box(
                    shape,
                    shape.Details,
                    i * shape.Width * 0.18f,
                    0.75f,
                    shape.Depth * 0.72f,
                    0.12f,
                    0.8f,
                    0.12f);
        }

        private static void BuildContainerRow(
            BuildingShape shape)
        {
            int count = Mathf.Clamp(
                Mathf.RoundToInt(shape.Width / 3f),
                2,
                6);
            float width = shape.Width / count;
            for (int i = 0; i < count; i++)
            {
                float x = -shape.Width * 0.5f +
                    width * (i + 0.5f);
                Box(
                    shape,
                    shape.Bodies,
                    x,
                    shape.Height * 0.34f,
                    0f,
                    width * 0.92f,
                    shape.Height * 0.68f,
                    shape.Depth);
                for (int rib = -2; rib <= 2; rib++)
                    Box(
                        shape,
                        shape.Details,
                        x + rib * width * 0.16f,
                        shape.Height * 0.34f,
                        shape.Depth * 0.505f,
                        0.07f,
                        shape.Height * 0.62f,
                        0.08f);
            }
        }

        private static void BuildGantry(
            BuildingShape shape)
        {
            float top =
                shape.Height * 0.82f;
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Bodies,
                    side * shape.Width * 0.42f,
                    top * 0.5f,
                    0f,
                    shape.Width * 0.08f,
                    top,
                    shape.Depth * 0.18f);
            Box(
                shape,
                shape.Bodies,
                0f,
                top,
                0f,
                shape.Width,
                shape.Height * 0.12f,
                shape.Depth * 0.22f);
            AddCrossBraces(
                shape,
                0f,
                top * 0.52f,
                shape.Depth * 0.12f,
                shape.Width * 0.76f,
                top * 0.72f);
        }

        private static void BuildStack(
            BuildingShape shape)
        {
            float radius =
                Mathf.Min(shape.Width, shape.Depth) *
                0.42f;
            AddTaperedCylinder(
                shape.Bodies,
                shape.Center,
                radius,
                radius * 0.62f,
                shape.Height,
                shape.Yaw,
                14);
            Cylinder(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.94f,
                0f,
                radius * 0.76f,
                shape.Height * 0.08f,
                14);
        }

        private static void BuildWaterTower(
            BuildingShape shape)
        {
            float legHeight =
                shape.Height * 0.62f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    PitchedBox(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.26f,
                        legHeight * 0.5f,
                        z * shape.Depth * 0.26f,
                        0.18f,
                        legHeight,
                        0.18f,
                        z * 5f,
                        x * 5f);
            AddCrossBraces(
                shape,
                0f,
                legHeight * 0.48f,
                shape.Depth * 0.28f,
                shape.Width * 0.5f,
                legHeight * 0.55f);
            Cylinder(
                shape,
                shape.Bodies,
                0f,
                legHeight,
                0f,
                Mathf.Min(
                    shape.Width,
                    shape.Depth) * 0.43f,
                shape.Height * 0.25f,
                14);
            Cone(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.87f,
                0f,
                Mathf.Min(
                    shape.Width,
                    shape.Depth) * 0.44f,
                shape.Height * 0.13f,
                14);
        }

        private static void BuildMotorPool(
            BuildingShape shape)
        {
            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.42f,
                        shape.Height * 0.45f,
                        z * shape.Depth * 0.42f,
                        0.2f,
                        shape.Height * 0.9f,
                        0.2f);
            PitchedBox(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.92f,
                0f,
                shape.Width * 1.08f,
                0.16f,
                shape.Depth * 1.08f,
                0f,
                6f);
            Box(
                shape,
                shape.Bodies,
                0f,
                0.12f,
                0f,
                shape.Width * 0.28f,
                0.24f,
                shape.Depth * 0.72f);
        }

        private static void BuildFieldHospital(
            BuildingShape shape)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                BuildingShape tent = shape;
                tent.Center += Local(
                    side * shape.Width * 0.26f,
                    0f,
                    0f,
                    shape.Yaw);
                tent.Width = shape.Width * 0.42f;
                Box(
                    tent,
                    tent.Bodies,
                    0f,
                    tent.Height * 0.16f,
                    0f,
                    tent.Width,
                    tent.Height * 0.32f,
                    tent.Depth);
                AddGableRoof(
                    tent.Bodies,
                    tent.Center,
                    tent.Width,
                    tent.Depth,
                    tent.Height * 0.8f,
                    tent.Yaw);
            }
            Box(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.62f,
                shape.Depth * 0.51f,
                shape.Width * 0.12f,
                0.12f,
                0.08f);
            Box(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.62f,
                shape.Depth * 0.52f,
                0.12f,
                shape.Width * 0.12f,
                0.08f);
        }

        private static void BuildCheckpoint(
            BuildingShape shape)
        {
            AddFlatShell(shape, 0.72f);
            AddWindowGrid(shape, 2, 1, true);
            Box(
                shape,
                shape.Details,
                shape.Width * 0.62f,
                shape.Height * 0.32f,
                shape.Depth * 0.38f,
                shape.Width * 0.95f,
                0.12f,
                0.12f,
                -0.15f);
        }

        private static void BuildTransformerShed(
            BuildingShape shape)
        {
            AddFlatShell(shape, 0.75f);
            for (int i = -1; i <= 1; i++)
            {
                Cylinder(
                    shape,
                    shape.Details,
                    i * shape.Width * 0.22f,
                    shape.Height * 0.35f,
                    shape.Depth * 0.52f,
                    shape.Width * 0.06f,
                    shape.Height * 0.32f,
                    8);
                for (int rib = 0; rib < 4; rib++)
                    Cylinder(
                        shape,
                        shape.Roofs,
                        i * shape.Width * 0.22f,
                        shape.Height * (0.38f +
                            rib * 0.055f),
                        shape.Depth * 0.52f,
                        shape.Width * 0.085f,
                        0.06f,
                        8);
            }
        }

    }
}
