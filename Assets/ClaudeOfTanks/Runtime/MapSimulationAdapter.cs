using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public static class MapSimulationAdapter
    {
        public static string BuildingObstacleId(string mapId, int buildingIndex)
        {
            return (mapId ?? "map") + "-building-" + buildingIndex;
        }

        public static string WallObstacleId(string mapId, int wallIndex, int pieceIndex)
        {
            return (mapId ?? "map") + "-wall-" + wallIndex + "-" + pieceIndex;
        }

        public static string TreeObstacleId(string mapId, int treeIndex)
        {
            return (mapId ?? "map") + "-tree-" + treeIndex;
        }

        public static IHeightField BuildHeightField(MapDefinition map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            LandformDefinition[] source =
                map.terrain?.landforms ?? Array.Empty<LandformDefinition>();
            TerrainLandform[] landforms = new TerrainLandform[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                landforms[i] = new TerrainLandform
                {
                    Kind = source[i].kind,
                    X = source[i].x,
                    Z = source[i].z,
                    Height = source[i].height,
                    Length = source[i].length,
                    Width = source[i].width,
                    RadiusX = source[i].rx,
                    RadiusZ = source[i].rz,
                    YawRad = source[i].yawDeg * MathUtil.Deg2Rad
                };
            }
            return new LandformHeightField(landforms);
        }

        public static StaticObstacle[] BuildStaticObstacles(
            MapDefinition map,
            IHeightField heightField)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (heightField == null) throw new ArgumentNullException(nameof(heightField));
            MapStructures structures = map.unityStructures;

            List<StaticObstacle> result = new List<StaticObstacle>();
            MapBuilding[] buildings =
                structures?.buildings ?? Array.Empty<MapBuilding>();
            for (int i = 0; i < buildings.Length; i++)
                AddBuilding(result, map.id, i, buildings[i], heightField);

            MapWall[] walls = structures?.walls ?? Array.Empty<MapWall>();
            for (int i = 0; i < walls.Length; i++)
                AddWall(result, map.id, i, walls[i], heightField);

            VegetationTreePlacement[] trees = MapVegetationPlacementBuilder.Expand(map);
            for (int i = 0; i < trees.Length; i++)
            {
                VegetationTreePlacement tree = trees[i];
                result.Add(new StaticObstacle(
                    TreeObstacleId(map.id, tree.Index),
                    new Float3(
                        tree.X,
                        heightField.HeightAt(tree.X, tree.Z),
                        tree.Z),
                    MathF.Max(0.16f, tree.TrunkRadius),
                    MathF.Max(0.16f, tree.TrunkRadius),
                    tree.TrunkHeight,
                    tree.Yaw,
                    StaticObstacleFlags.Movement |
                    StaticObstacleFlags.Shells |
                    StaticObstacleFlags.Vision,
                    true,
                    true,
                    1f));
            }

            if (result.Count > BattleState.MaximumStaticObstacles)
                throw new InvalidOperationException("Map static obstacle count exceeds its bound.");
            return result.ToArray();
        }

        private static void AddBuilding(
            List<StaticObstacle> result,
            string mapId,
            int index,
            MapBuilding building,
            IHeightField heightField)
        {
            float width = MathF.Max(3f, building.w);
            float depth = MathF.Max(3f, building.d);
            float height = MathF.Max(3f, building.h);
            float yaw = building.yawDeg * MathUtil.Deg2Rad;
            float ground = heightField.HeightAt(building.x, building.z);
            string id = BuildingObstacleId(mapId, index);
            if (string.Equals(building.profile, "ruin", StringComparison.Ordinal))
            {
                AddLocalBox(
                    result, id + "-rear", building, ground, yaw,
                    0f, -depth * 0.45f, width, 0.65f, height * 0.72f);
                AddLocalBox(
                    result, id + "-left", building, ground, yaw,
                    -width * 0.45f, 0f, 0.65f, depth, height * 0.48f);
                AddLocalBox(
                    result, id + "-right", building, ground, yaw,
                    width * 0.45f, depth * 0.12f, 0.65f, depth * 0.76f, height * 0.32f);
                return;
            }

            result.Add(new StaticObstacle(
                id,
                new Float3(building.x, ground, building.z),
                width * 0.5f,
                depth * 0.5f,
                height,
                yaw,
                StaticObstacleFlags.All,
                building.destructible));
        }

        private static void AddLocalBox(
            List<StaticObstacle> result,
            string id,
            MapBuilding building,
            float ground,
            float yaw,
            float localX,
            float localZ,
            float width,
            float depth,
            float height)
        {
            float cos = MathF.Cos(yaw);
            float sin = MathF.Sin(yaw);
            result.Add(new StaticObstacle(
                id,
                new Float3(
                    building.x + localX * cos + localZ * sin,
                    ground,
                    building.z - localX * sin + localZ * cos),
                width * 0.5f,
                depth * 0.5f,
                height,
                yaw,
                StaticObstacleFlags.All,
                building.destructible));
        }

        private static void AddWall(
            List<StaticObstacle> result,
            string mapId,
            int wallIndex,
            MapWall wall,
            IHeightField heightField)
        {
            float dx = wall.x2 - wall.x1;
            float dz = wall.z2 - wall.z1;
            float length = MathF.Sqrt(dx * dx + dz * dz);
            if (length < 1f) return;
            float yaw = MathF.Atan2(dx, dz);
            int pieces = Math.Max(1, (int)MathF.Ceiling(length / 18f));
            float pieceLength = length / pieces;
            int omitted = Math.Abs(wall.variant) % pieces;
            for (int i = 0; i < pieces; i++)
            {
                if (pieces > 2 && i == omitted) continue;
                float t = (i + 0.5f) / pieces;
                float x = wall.x1 + dx * t;
                float z = wall.z1 + dz * t;
                result.Add(new StaticObstacle(
                    WallObstacleId(mapId, wallIndex, i),
                    new Float3(x, heightField.HeightAt(x, z), z),
                    0.4f,
                    pieceLength * 0.45f,
                    1.5f,
                    yaw,
                    StaticObstacleFlags.All));
            }
        }
    }
}
