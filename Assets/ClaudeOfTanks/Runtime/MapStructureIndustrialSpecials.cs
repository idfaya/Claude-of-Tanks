using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildCommandTent(
            BuildingShape shape)
        {
            AddGableRoof(
                shape.Bodies,
                shape.Center,
                shape.Width,
                shape.Depth,
                shape.Height * 0.72f,
                shape.Yaw);
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.16f,
                0f,
                shape.Width,
                shape.Height * 0.32f,
                shape.Depth);
            Box(
                shape,
                shape.Details,
                shape.Width * 0.42f,
                shape.Height * 0.65f,
                -shape.Depth * 0.42f,
                0.1f,
                shape.Height * 0.7f,
                0.1f);
            PitchedBox(
                shape,
                shape.Details,
                shape.Width * 0.48f,
                shape.Height * 0.96f,
                -shape.Depth * 0.42f,
                shape.Width * 0.32f,
                0.08f,
                0.08f,
                0f,
                -14f);
        }

        private static void BuildGuardPost(
            BuildingShape shape)
        {
            float raised =
                shape.Height * 0.25f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.34f,
                        raised * 0.5f,
                        z * shape.Depth * 0.34f,
                        0.16f,
                        raised,
                        0.16f);
            Box(
                shape,
                shape.Bodies,
                0f,
                raised + shape.Height * 0.28f,
                0f,
                shape.Width * 0.76f,
                shape.Height * 0.48f,
                shape.Depth * 0.76f);
            Box(
                shape,
                shape.Roofs,
                0f,
                shape.Height * 0.78f,
                0f,
                shape.Width * 0.9f,
                0.16f,
                shape.Depth * 0.9f);
            AddWindowGrid(shape, 2, 1, true);
        }

        private static void BuildQuonset(
            BuildingShape shape)
        {
            int ribs = 8;
            for (int rib = 0; rib < ribs; rib++)
            {
                float z = -shape.Depth * 0.46f +
                    rib * shape.Depth * 0.92f /
                    (ribs - 1f);
                for (int segment = 0;
                    segment < 7;
                    segment++)
                {
                    float angle =
                        Mathf.PI * segment / 6f;
                    float x =
                        Mathf.Cos(angle) *
                        shape.Width * 0.48f;
                    float y =
                        Mathf.Sin(angle) *
                        shape.Height * 0.88f;
                    Box(
                        shape,
                        rib == 0 || rib == ribs - 1
                            ? shape.Bodies
                            : shape.Roofs,
                        x,
                        y,
                        z,
                        shape.Width * 0.17f,
                        0.14f,
                        shape.Depth * 0.15f,
                        -angle);
                }
            }
            Box(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.34f,
                shape.Depth * 0.51f,
                shape.Width * 0.45f,
                shape.Height * 0.62f,
                0.12f);
        }

        private static void BuildFireStation(
            BuildingShape shape)
        {
            AddFlatShell(shape, 0.62f);
            for (int bay = -1; bay <= 1; bay += 2)
            {
                float x = bay *
                    shape.Width * 0.22f;
                Box(
                    shape,
                    shape.Details,
                    x,
                    shape.Height * 0.25f,
                    shape.Depth * 0.51f,
                    shape.Width * 0.3f,
                    shape.Height * 0.5f,
                    0.12f);
                for (int stripe = 1;
                    stripe < 6;
                    stripe++)
                    Box(
                        shape,
                        shape.Roofs,
                        x,
                        stripe *
                            shape.Height * 0.085f,
                        shape.Depth * 0.53f,
                        shape.Width * 0.28f,
                        0.06f,
                        0.08f);
            }
            float towerHeight =
                shape.Height * 0.78f;
            Box(
                shape,
                shape.Bodies,
                shape.Width * 0.34f,
                towerHeight * 0.5f,
                -shape.Depth * 0.25f,
                shape.Width * 0.24f,
                towerHeight,
                shape.Width * 0.24f);
            Cone(
                shape,
                shape.Roofs,
                shape.Width * 0.34f,
                towerHeight,
                -shape.Depth * 0.25f,
                shape.Width * 0.18f,
                shape.Height * 0.22f,
                4);
        }
    }
}
