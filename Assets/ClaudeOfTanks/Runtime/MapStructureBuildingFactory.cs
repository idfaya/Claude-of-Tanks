using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        internal static bool SupportsBuildingKind(
            string kind)
        {
            switch (kind)
            {
                case "ruin":
                case "cottage":
                case "farmhouse":
                case "barn":
                case "granary":
                case "chapel":
                case "logcabin":
                case "woodshed":
                case "shed":
                case "mill":
                case "tavern":
                case "schoolhouse":
                case "rangerlodge":
                case "alpine":
                case "alpinerefuge":
                case "huntingblind":
                case "fieldhut":
                case "longhouse":
                case "saunahut":
                case "boatshed":
                case "fishershack":
                case "fishery":
                case "netyard":
                case "stilthouse":
                case "lighthouse":
                case "adobe":
                case "market":
                case "marketRow":
                case "compound":
                case "compoundSouk":
                case "caravanserai":
                case "minaret":
                case "deserttent":
                case "depot":
                case "warehouse":
                case "containerRow":
                case "factory":
                case "gantry":
                case "foundryoffice":
                case "stack":
                case "watertower":
                case "motorpool":
                case "fieldhospital":
                case "checkpointhut":
                case "transformershed":
                case "commandtent":
                case "guardpost":
                case "quonsethut":
                case "firestation":
                case "rowhouse":
                case "cornershop":
                case "civichall":
                case "church":
                case "onionchurch":
                case "bathhouse":
                case "parkingdeck":
                case "megatower":
                case "needletower":
                case "broadcasttower":
                case "terracetower":
                case "arcology":
                case "tower":
                    return true;
                default:
                    return false;
            }
        }

        internal static int BuildingKindSignature(
            string kind)
        {
            if (!SupportsBuildingKind(kind)) return 0;
            return StableHash("structure-recipe:" + kind);
        }

        private void AddBuilding(
            MapBuilding building,
            MeshBucket bodies,
            MeshBucket roofs,
            MeshBucket details)
        {
            if (!SupportsBuildingKind(building.kind))
                throw new InvalidOperationException(
                    "Missing Unity structure recipe: " +
                    building.kind);
            BuildingShape shape = new BuildingShape
            {
                Kind = building.kind,
                Center = new Vector3(
                    building.x,
                    _heightField.HeightAt(
                        building.x,
                        building.z),
                    building.z),
                Yaw = building.yawDeg *
                    Mathf.Deg2Rad,
                Width = Mathf.Max(3f, building.w),
                Depth = Mathf.Max(3f, building.d),
                Height = Mathf.Max(3f, building.h),
                Bodies = bodies,
                Roofs = roofs,
                Details = details
            };
            if (building.kind == "ruin")
                BuildRuin(shape);
            else if (IsRuralKind(building.kind))
                BuildRuralStructure(shape);
            else if (IsCoastalKind(building.kind))
                BuildCoastalStructure(shape);
            else if (IsDesertKind(building.kind))
                BuildDesertStructure(shape);
            else if (IsIndustrialKind(building.kind))
                BuildIndustrialStructure(shape);
            else
                BuildUrbanStructure(shape);
        }

        private static bool IsRuralKind(string kind)
        {
            switch (kind)
            {
                case "cottage":
                case "farmhouse":
                case "barn":
                case "granary":
                case "chapel":
                case "logcabin":
                case "woodshed":
                case "shed":
                case "mill":
                case "tavern":
                case "schoolhouse":
                case "rangerlodge":
                case "alpine":
                case "alpinerefuge":
                case "huntingblind":
                case "fieldhut":
                case "longhouse":
                case "saunahut":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsCoastalKind(string kind)
        {
            return kind == "boatshed" ||
                kind == "fishershack" ||
                kind == "fishery" ||
                kind == "netyard" ||
                kind == "stilthouse" ||
                kind == "lighthouse";
        }

        private static bool IsDesertKind(string kind)
        {
            return kind == "adobe" ||
                kind == "market" ||
                kind == "marketRow" ||
                kind == "compound" ||
                kind == "compoundSouk" ||
                kind == "caravanserai" ||
                kind == "minaret" ||
                kind == "deserttent";
        }

        private static bool IsIndustrialKind(string kind)
        {
            switch (kind)
            {
                case "depot":
                case "warehouse":
                case "containerRow":
                case "factory":
                case "gantry":
                case "foundryoffice":
                case "stack":
                case "watertower":
                case "motorpool":
                case "fieldhospital":
                case "checkpointhut":
                case "transformershed":
                case "commandtent":
                case "guardpost":
                case "quonsethut":
                case "firestation":
                    return true;
                default:
                    return false;
            }
        }

        private static void BuildRuin(
            BuildingShape shape)
        {
            Box(
                shape,
                shape.Bodies,
                0f,
                shape.Height * 0.36f,
                -shape.Depth * 0.45f,
                shape.Width,
                shape.Height * 0.72f,
                0.65f);
            Box(
                shape,
                shape.Bodies,
                -shape.Width * 0.45f,
                shape.Height * 0.24f,
                0f,
                0.65f,
                shape.Height * 0.48f,
                shape.Depth);
            Box(
                shape,
                shape.Bodies,
                shape.Width * 0.45f,
                shape.Height * 0.16f,
                shape.Depth * 0.12f,
                0.65f,
                shape.Height * 0.32f,
                shape.Depth * 0.76f);
            for (int i = 0; i < 3; i++)
                Box(
                    shape,
                    shape.Details,
                    -shape.Width * 0.25f + i *
                        shape.Width * 0.25f,
                    0.22f + i * 0.08f,
                    shape.Depth * 0.12f,
                    shape.Width * 0.18f,
                    0.32f,
                    shape.Depth * 0.22f,
                    i * 0.31f);
        }

        private static void AddFoundation(
            BuildingShape shape,
            float margin = 0.35f)
        {
            Box(
                shape,
                shape.Bodies,
                0f,
                0.22f,
                0f,
                shape.Width + margin,
                0.44f,
                shape.Depth + margin);
        }

        private static void AddGableShell(
            BuildingShape shape,
            float wallRatio = 0.68f,
            float overhang = 1.08f)
        {
            float wallHeight =
                shape.Height * wallRatio;
            AddFoundation(shape);
            Box(
                shape,
                shape.Bodies,
                0f,
                wallHeight * 0.5f,
                0f,
                shape.Width,
                wallHeight,
                shape.Depth);
            AddGableRoof(
                shape.Roofs,
                shape.Center +
                    Vector3.up * wallHeight,
                shape.Width * overhang,
                shape.Depth * 1.05f,
                shape.Height - wallHeight,
                shape.Yaw);
        }

        private static void AddFlatShell(
            BuildingShape shape,
            float wallRatio = 0.82f)
        {
            float wallHeight =
                shape.Height * wallRatio;
            AddFoundation(shape);
            Box(
                shape,
                shape.Bodies,
                0f,
                wallHeight * 0.5f,
                0f,
                shape.Width,
                wallHeight,
                shape.Depth);
            Box(
                shape,
                shape.Roofs,
                0f,
                wallHeight + 0.18f,
                0f,
                shape.Width * 1.04f,
                0.36f,
                shape.Depth * 1.04f);
        }

        private static void AddWindowGrid(
            BuildingShape shape,
            int columns,
            int floors,
            bool sides = false)
        {
            columns = Mathf.Max(1, columns);
            floors = Mathf.Max(1, floors);
            for (int floor = 0; floor < floors; floor++)
            {
                float y =
                    (floor + 0.6f) *
                    shape.Height /
                    (floors + 0.5f);
                for (int column = 0;
                    column < columns;
                    column++)
                {
                    float x = -shape.Width * 0.4f +
                        shape.Width * 0.8f *
                        (column + 0.5f) / columns;
                    Box(
                        shape,
                        shape.Details,
                        x,
                        y,
                        shape.Depth * 0.505f,
                        shape.Width * 0.52f /
                            columns,
                        Mathf.Min(1.45f, shape.Height /
                            (floors + 2f)),
                        0.1f);
                }
                if (!sides) continue;
                for (int side = -1; side <= 1; side += 2)
                    Box(
                        shape,
                        shape.Details,
                        side * shape.Width * 0.505f,
                        y,
                        0f,
                        0.1f,
                        Mathf.Min(1.45f, shape.Height /
                            (floors + 2f)),
                        shape.Depth * 0.48f);
            }
        }

        private static void Box(
            BuildingShape shape,
            MeshBucket bucket,
            float x,
            float y,
            float z,
            float width,
            float height,
            float depth,
            float localYaw = 0f)
        {
            AddBox(
                bucket,
                shape.Center +
                    Local(x, y, z, shape.Yaw),
                new Vector3(width, height, depth),
                shape.Yaw + localYaw);
        }

        private struct BuildingShape
        {
            public string Kind;
            public Vector3 Center;
            public float Yaw;
            public float Width;
            public float Depth;
            public float Height;
            public MeshBucket Bodies;
            public MeshBucket Roofs;
            public MeshBucket Details;
        }
    }
}
