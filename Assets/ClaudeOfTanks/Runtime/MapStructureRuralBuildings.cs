using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildRuralStructure(
            BuildingShape shape)
        {
            switch (shape.Kind)
            {
                case "huntingblind":
                    BuildHuntingBlind(shape);
                    return;
                case "woodshed":
                case "shed":
                    BuildOpenShed(shape);
                    return;
                case "mill":
                    BuildMill(shape);
                    return;
            }
            AddGableShell(
                shape,
                shape.Kind == "alpine" ||
                shape.Kind == "alpinerefuge"
                    ? 0.55f
                    : 0.66f);
            AddRuralFacade(shape);
            switch (shape.Kind)
            {
                case "farmhouse":
                    AddPorch(shape, 0.92f);
                    AddChimney(shape, -0.26f, -0.2f);
                    AddSideWindows(shape, 3);
                    break;
                case "barn":
                    AddBarnDetails(shape);
                    break;
                case "granary":
                    AddGranaryDetails(shape);
                    break;
                case "chapel":
                    AddChapelDetails(shape);
                    break;
                case "logcabin":
                    AddLogCourses(shape);
                    AddChimney(shape, 0.24f, -0.18f);
                    break;
                case "tavern":
                    AddBalcony(shape);
                    AddChimney(shape, 0.28f, -0.22f);
                    break;
                case "schoolhouse":
                    AddSchoolCupola(shape);
                    AddPorch(shape, 0.55f);
                    break;
                case "rangerlodge":
                    AddPorch(shape, 1f);
                    AddSchoolCupola(shape);
                    AddChimney(shape, -0.28f, -0.18f);
                    break;
                case "alpine":
                case "alpinerefuge":
                    AddAlpineBalcony(shape);
                    AddChimney(shape, -0.24f, -0.2f);
                    break;
                case "fieldhut":
                    AddPorch(shape, 0.72f);
                    AddChimney(shape, -0.24f, -0.2f);
                    break;
                case "longhouse":
                    AddLonghouseDetails(shape);
                    break;
                case "saunahut":
                    AddLogCourses(shape);
                    AddChimney(shape, 0.26f, -0.2f);
                    break;
                default:
                    AddChimney(shape, 0.22f, 0.18f);
                    break;
            }
        }

        private static void AddRuralFacade(
            BuildingShape shape)
        {
            Box(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.24f,
                shape.Depth * 0.505f,
                shape.Width * 0.22f,
                shape.Height * 0.34f,
                0.1f);
            int windows = Mathf.Clamp(
                Mathf.RoundToInt(shape.Width / 2.8f),
                2,
                5);
            for (int i = 0; i < windows; i++)
            {
                float x = -shape.Width * 0.38f +
                    shape.Width * 0.76f *
                    (i + 0.5f) / windows;
                if (Mathf.Abs(x) <
                    shape.Width * 0.14f)
                    continue;
                AddFramedWindow(
                    shape,
                    x,
                    shape.Height * 0.38f,
                    shape.Depth * 0.51f,
                    shape.Width * 0.12f,
                    shape.Height * 0.18f);
            }
        }

        private static void AddPorch(
            BuildingShape shape,
            float widthRatio)
        {
            float porchWidth =
                shape.Width * widthRatio;
            float front =
                shape.Depth * 0.5f + 1.1f;
            Box(
                shape,
                shape.Bodies,
                0f,
                0.16f,
                front,
                porchWidth,
                0.22f,
                2.2f);
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Details,
                    side * porchWidth * 0.42f,
                    shape.Height * 0.25f,
                    front + 0.7f,
                    0.16f,
                    shape.Height * 0.5f,
                    0.16f);
            PitchedBox(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.52f,
                front + 0.1f,
                porchWidth * 1.06f,
                0.14f,
                2.6f,
                -8f);
        }

        private static void AddChimney(
            BuildingShape shape,
            float xRatio,
            float zRatio)
        {
            Box(
                shape,
                shape.Bodies,
                shape.Width * xRatio,
                shape.Height * 0.82f,
                shape.Depth * zRatio,
                0.65f,
                shape.Height * 0.46f,
                0.65f);
            Box(
                shape,
                shape.Roofs,
                shape.Width * xRatio,
                shape.Height * 1.04f,
                shape.Depth * zRatio,
                0.82f,
                0.14f,
                0.82f);
        }

        private static void AddBarnDetails(
            BuildingShape shape)
        {
            float z = shape.Depth * 0.51f;
            Box(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.24f,
                z,
                shape.Width * 0.42f,
                shape.Height * 0.48f,
                0.12f);
            AddCrossBraces(
                shape,
                0f,
                shape.Height * 0.24f,
                z + 0.08f,
                shape.Width * 0.38f,
                shape.Height * 0.42f);
            int battens = Mathf.Clamp(
                Mathf.RoundToInt(shape.Depth / 1.2f),
                6,
                14);
            for (int i = 0; i < battens; i++)
                for (int side = -1; side <= 1; side += 2)
                    Box(
                        shape,
                        shape.Details,
                        side * shape.Width * 0.505f,
                        shape.Height * 0.28f,
                        -shape.Depth * 0.45f +
                            shape.Depth * 0.9f *
                            i / (battens - 1f),
                        0.08f,
                        shape.Height * 0.5f,
                        0.12f);
        }

        private static void AddGranaryDetails(
            BuildingShape shape)
        {
            float radius =
                Mathf.Min(shape.Width, shape.Depth) *
                0.18f;
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder(
                    shape,
                    shape.Bodies,
                    side * shape.Width * 0.3f,
                    0f,
                    -shape.Depth * 0.14f,
                    radius,
                    shape.Height * 0.62f,
                    10);
                Cone(
                    shape,
                    shape.Roofs,
                    side * shape.Width * 0.3f,
                    shape.Height * 0.62f,
                    -shape.Depth * 0.14f,
                    radius * 1.08f,
                    shape.Height * 0.22f,
                    10);
            }
        }

        private static void AddChapelDetails(
            BuildingShape shape)
        {
            float front =
                shape.Depth * 0.36f;
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.55f,
                front,
                shape.Width * 0.42f,
                shape.Height * 0.62f,
                shape.Width * 0.42f);
            Cone(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.86f,
                front,
                shape.Width * 0.32f,
                shape.Height * 0.14f);
        }

        private static void AddLogCourses(
            BuildingShape shape)
        {
            int courses = Mathf.Clamp(
                Mathf.RoundToInt(shape.Height / 0.48f),
                5,
                10);
            for (int i = 0; i < courses; i++)
                Box(
                    shape,
                    shape.Details,
                    0f,
                    0.3f + i * shape.Height *
                        0.55f / courses,
                    shape.Depth * 0.507f,
                    shape.Width * 1.01f,
                    0.08f,
                    0.1f);
        }

        private static void AddBalcony(
            BuildingShape shape)
        {
            float front =
                shape.Depth * 0.5f + 0.75f;
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.48f,
                front,
                shape.Width * 0.8f,
                0.2f,
                1.55f);
            for (int i = -2; i <= 2; i++)
                Box(
                    shape,
                    shape.Details,
                    i * shape.Width * 0.17f,
                    shape.Height * 0.58f,
                    front + 0.55f,
                    0.1f,
                    shape.Height * 0.22f,
                    0.1f);
        }

        private static void AddSchoolCupola(
            BuildingShape shape)
        {
            Box(
                shape,
                shape.Bodies,
                shape.Width * 0.16f,
                shape.Height * 0.78f,
                -shape.Depth * 0.08f,
                shape.Width * 0.28f,
                shape.Height * 0.16f,
                shape.Width * 0.28f);
            Cone(
                shape,
                shape.Roofs,
                shape.Width * 0.16f,
                shape.Height * 0.86f,
                -shape.Depth * 0.08f,
                shape.Width * 0.21f,
                shape.Height * 0.14f,
                4);
        }

        private static void AddAlpineBalcony(
            BuildingShape shape)
        {
            float z =
                shape.Depth * 0.5f + 0.45f;
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.52f,
                z,
                shape.Width * 0.88f,
                0.16f,
                0.9f);
            for (int i = -3; i <= 3; i++)
                Box(
                    shape,
                    shape.Details,
                    i * shape.Width * 0.12f,
                    shape.Height * 0.59f,
                    z + 0.35f,
                    0.09f,
                    shape.Height * 0.16f,
                    0.09f);
        }

        private static void BuildHuntingBlind(
            BuildingShape shape)
        {
            float cabinBase =
                shape.Height * 0.48f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.3f,
                        cabinBase * 0.5f,
                        z * shape.Depth * 0.3f,
                        0.16f,
                        cabinBase,
                        0.16f);
            Box(
                shape,
                shape.Bodies,
                0f,
                cabinBase + shape.Height * 0.2f,
                0f,
                shape.Width * 0.78f,
                shape.Height * 0.36f,
                shape.Depth * 0.78f);
            PitchedBox(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.91f,
                0f,
                shape.Width * 0.92f,
                0.14f,
                shape.Depth * 0.92f,
                0f,
                5f);
            for (int i = 0; i < 6; i++)
                Box(
                    shape,
                    shape.Details,
                    shape.Width * 0.46f,
                    0.28f + i * 0.42f,
                    shape.Depth * 0.34f,
                    0.65f,
                    0.08f,
                    0.11f);
        }

        private static void BuildOpenShed(
            BuildingShape shape)
        {
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.42f,
                        shape.Height * 0.4f,
                        z * shape.Depth * 0.42f,
                        0.18f,
                        shape.Height * 0.8f,
                        0.18f);
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.36f,
                -shape.Depth * 0.46f,
                shape.Width,
                shape.Height * 0.72f,
                0.2f);
            PitchedBox(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.84f,
                0f,
                shape.Width * 1.08f,
                0.14f,
                shape.Depth * 1.08f,
                0f,
                8f);
        }

        private static void BuildMill(
            BuildingShape shape)
        {
            AddGableShell(shape, 0.62f);
            float z =
                shape.Depth * 0.53f;
            Cylinder(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.5f,
                z,
                shape.Width * 0.07f,
                0.35f,
                10);
            for (int blade = 0; blade < 4; blade++)
                PitchedBox(
                    shape,
                    shape.Roofs,
                    0f,
                    shape.Height * 0.66f,
                    z + 0.18f,
                    0.18f,
                    shape.Height * 0.72f,
                    shape.Width * 0.15f,
                    0f,
                    blade * 90f);
        }
    }
}
