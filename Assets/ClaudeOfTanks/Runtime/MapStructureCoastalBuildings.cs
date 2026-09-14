using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void BuildCoastalStructure(
            BuildingShape shape)
        {
            if (shape.Kind == "lighthouse")
            {
                BuildLighthouse(shape);
                return;
            }
            if (shape.Kind == "netyard")
            {
                BuildNetYard(shape);
                return;
            }
            AddGableShell(shape, 0.62f);
            AddRuralFacade(shape);
            if (shape.Kind == "fishershack" ||
                shape.Kind == "stilthouse")
            {
                AddStilts(shape);
            }
            if (shape.Kind == "boatshed")
                AddBoatDoor(shape);
            if (shape.Kind == "fishery")
                AddFisheryDock(shape);
            if (shape.Kind == "fishershack")
                AddDryingRack(shape);
            if (shape.Kind == "stilthouse")
                AddStiltLadder(shape);
        }

        private static void BuildLighthouse(
            BuildingShape shape)
        {
            float radius =
                Mathf.Min(shape.Width, shape.Depth) *
                0.34f;
            float shaft =
                shape.Height * 0.68f;
            AddFoundation(shape, 0.6f);
            AddTaperedCylinder(
                shape.Bodies,
                shape.Center,
                radius,
                radius * 0.66f,
                shaft,
                shape.Yaw,
                14);
            Cylinder(
                shape,
                shape.Details,
                0f,
                shaft,
                0f,
                radius * 0.88f,
                shape.Height * 0.1f,
                12);
            Box(
                shape,
                shape.Roofs,
                0f,
                shaft + shape.Height * 0.11f,
                0f,
                radius * 2.3f,
                0.18f,
                radius * 2.3f);
            Cone(
                shape,
                shape.Roofs,
                0f,
                shaft + shape.Height * 0.18f,
                0f,
                radius * 0.95f,
                shape.Height * 0.14f,
                12);
            for (int side = -1; side <= 1; side += 2)
                Box(
                    shape,
                    shape.Details,
                    side * radius * 0.38f,
                    shaft + shape.Height * 0.05f,
                    radius * 0.72f,
                    radius * 0.34f,
                    shape.Height * 0.07f,
                    0.08f);
        }

        private static void AddStilts(
            BuildingShape shape)
        {
            float raised =
                shape.Height * 0.22f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.38f,
                        raised * 0.5f,
                        z * shape.Depth * 0.38f,
                        0.2f,
                        raised,
                        0.2f);
        }

        private static void AddBoatDoor(
            BuildingShape shape)
        {
            Box(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.28f,
                shape.Depth * 0.51f,
                shape.Width * 0.55f,
                shape.Height * 0.56f,
                0.12f);
            for (int i = -2; i <= 2; i++)
                Box(
                    shape,
                    shape.Roofs,
                    i * shape.Width * 0.1f,
                    shape.Height * 0.28f,
                    shape.Depth * 0.53f,
                    0.08f,
                    shape.Height * 0.52f,
                    0.1f);
        }

        private static void AddFisheryDock(
            BuildingShape shape)
        {
            float front =
                shape.Depth * 0.5f + 2f;
            Box(
                shape,
                shape.Bodies,
                0f,
                0.22f,
                front,
                shape.Width * 1.45f,
                0.3f,
                4f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x * shape.Width * 0.58f,
                        -0.55f,
                        front + z * 1.5f,
                        0.25f,
                        1.8f,
                        0.25f);
            PitchedBox(
                shape,
                shape.Details,
                shape.Width * 0.48f,
                shape.Height * 0.55f,
                shape.Depth * 0.56f,
                0.22f,
                shape.Height * 0.85f,
                0.22f,
                0f,
                -22f);
        }

        private static void AddDryingRack(
            BuildingShape shape)
        {
            float rear =
                -shape.Depth * 0.5f - 0.65f;
            for (int i = -1; i <= 1; i++)
                Box(
                    shape,
                    shape.Details,
                    i * shape.Width * 0.34f,
                    shape.Height * 0.26f,
                    rear,
                    0.1f,
                    shape.Height * 0.52f,
                    0.1f);
            Box(
                shape,
                shape.Details,
                0f,
                shape.Height * 0.51f,
                rear,
                shape.Width * 0.82f,
                0.1f,
                0.1f);
        }

        private static void AddStiltLadder(
            BuildingShape shape)
        {
            float x =
                shape.Width * 0.54f;
            for (int i = 0; i < 6; i++)
                Box(
                    shape,
                    shape.Details,
                    x,
                    0.25f + i * 0.32f,
                    shape.Depth * 0.4f +
                        i * 0.16f,
                    0.85f,
                    0.08f,
                    0.12f);
            PitchedBox(
                shape,
                shape.Details,
                x,
                1.05f,
                shape.Depth * 0.78f,
                0.12f,
                2.5f,
                0.12f,
                -34f);
        }

        private static void BuildNetYard(
            BuildingShape shape)
        {
            AddFoundation(shape, 0.2f);
            int racks = 4;
            for (int rack = 0; rack < racks; rack++)
            {
                float x = -shape.Width * 0.36f +
                    rack * shape.Width * 0.24f;
                for (int z = -1; z <= 1; z += 2)
                    Box(
                        shape,
                        shape.Details,
                        x,
                        shape.Height * 0.38f,
                        z * shape.Depth * 0.38f,
                        0.12f,
                        shape.Height * 0.76f,
                        0.12f);
                Box(
                    shape,
                    shape.Roofs,
                    x,
                    shape.Height * 0.72f,
                    0f,
                    0.1f,
                    shape.Height * 0.62f,
                    shape.Depth * 0.72f);
            }
            Box(
                shape,
                shape.Bodies,
                -shape.Width * 0.38f,
                shape.Height * 0.2f,
                0f,
                shape.Width * 0.22f,
                shape.Height * 0.4f,
                shape.Depth * 0.3f);
        }
    }
}
