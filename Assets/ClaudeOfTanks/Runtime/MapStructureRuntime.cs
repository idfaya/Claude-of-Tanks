using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class MapStructureRuntime : IDisposable
    {
        private readonly GameObject _root;
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Material> _materials = new List<Material>();
        private readonly LandformHeightField _heightField;

        private MapStructureRuntime(Transform parent, MapDefinition map)
        {
            _root = new GameObject("Structures");
            _root.transform.SetParent(parent, false);
            _heightField = BuildHeightField(map);
            Build(map);
        }

        public Transform Root => _root.transform;
        public int BuildingCount { get; private set; }
        public int TacticalBuildingCount { get; private set; }
        public int WallRunCount { get; private set; }
        public int RubblePileCount { get; private set; }
        public int SandbagLineCount { get; private set; }
        public int HedgehogCount { get; private set; }

        public static MapStructureRuntime Create(Transform parent, MapDefinition map)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (map == null) throw new ArgumentNullException(nameof(map));
            return new MapStructureRuntime(parent, map);
        }

        public void Dispose()
        {
            for (int i = 0; i < _materials.Count; i++) DestroyObject(_materials[i]);
            for (int i = 0; i < _meshes.Count; i++) DestroyObject(_meshes[i]);
            DestroyObject(_root);
        }

        private void Build(MapDefinition map)
        {
            MapStructures source = map.unityStructures;
            if (source == null) return;
            MapBuilding[] buildings = source.buildings ?? Array.Empty<MapBuilding>();
            MapWall[] walls = source.walls ?? Array.Empty<MapWall>();
            BuildingCount = buildings.Length;
            WallRunCount = walls.Length;
            RubblePileCount = Mathf.Max(0, source.rubblePiles);
            SandbagLineCount = Mathf.Max(0, source.sandbagLines);
            HedgehogCount = Mathf.Max(0, source.hedgehogs);

            MeshBucket bodies = new MeshBucket();
            MeshBucket roofs = new MeshBucket();
            MeshBucket details = new MeshBucket();
            MeshBucket wallMesh = new MeshBucket();
            MeshBucket cover = new MeshBucket();
            for (int i = 0; i < buildings.Length; i++)
            {
                AddBuilding(buildings[i], bodies, roofs, details);
                if (buildings[i].tactical) TacticalBuildingCount++;
            }
            for (int i = 0; i < walls.Length; i++) AddWall(walls[i], wallMesh);
            AddRubble(map.id, buildings, RubblePileCount, cover);
            AddSandbags(map.id, buildings, SandbagLineCount, cover);
            AddHedgehogs(map.id, HedgehogCount, details);

            Color building = source.buildingColor != null
                ? source.buildingColor.ToColor()
                : new Color(0.62f, 0.61f, 0.57f);
            Color roof = Color.Lerp(building, new Color(0.15f, 0.16f, 0.16f), 0.58f);
            Color dark = new Color(0.12f, 0.14f, 0.14f);
            CreateMesh("Structures-Bodies", bodies, building);
            CreateMesh("Structures-Roofs", roofs, roof);
            CreateMesh("Structures-Details", details, dark);
            CreateMesh("Structures-Walls", wallMesh, Color.Lerp(building, Color.gray, 0.3f));
            CreateMesh("Structures-Cover", cover, Color.Lerp(building, new Color(0.2f, 0.17f, 0.13f), 0.55f));
        }

        private void AddBuilding(
            MapBuilding building,
            MeshBucket bodies,
            MeshBucket roofs,
            MeshBucket details)
        {
            float y = _heightField.HeightAt(building.x, building.z);
            Vector3 center = new Vector3(building.x, y, building.z);
            float yaw = building.yawDeg * Mathf.Deg2Rad;
            float width = Mathf.Max(3f, building.w);
            float depth = Mathf.Max(3f, building.d);
            float height = Mathf.Max(3f, building.h);
            string profile = building.profile ?? "rural";

            if (profile == "ruin")
            {
                AddBox(bodies, center + Local(0f, height * 0.36f, -depth * 0.45f, yaw),
                    new Vector3(width, height * 0.72f, 0.65f), yaw);
                AddBox(bodies, center + Local(-width * 0.45f, height * 0.24f, 0f, yaw),
                    new Vector3(0.65f, height * 0.48f, depth), yaw);
                AddBox(bodies, center + Local(width * 0.45f, height * 0.16f, depth * 0.12f, yaw),
                    new Vector3(0.65f, height * 0.32f, depth * 0.76f), yaw);
                return;
            }
            if (profile == "tent")
            {
                AddGableRoof(roofs, center + Vector3.up * 0.1f, width, depth, height, yaw);
                AddBox(details, center + Local(0f, height * 0.35f, depth * 0.49f, yaw),
                    new Vector3(width * 0.12f, height * 0.7f, 0.08f), yaw);
                return;
            }
            if (profile == "tower")
            {
                bool round = building.kind == "lighthouse" ||
                    building.kind == "watertower" ||
                    building.kind == "stack";
                if (round)
                {
                    AddCylinder(bodies, center, width * 0.5f, height, yaw, 12);
                }
                else
                {
                    AddBox(bodies, center + Vector3.up * (height * 0.5f),
                        new Vector3(width, height, depth), yaw);
                    AddBox(roofs, center + Vector3.up * (height + 0.45f),
                        new Vector3(width * 1.12f, 0.9f, depth * 1.12f), yaw);
                }
                AddBox(details, center + Vector3.up * (height * 0.68f) +
                    Local(0f, 0f, depth * 0.51f, yaw),
                    new Vector3(width * 0.5f, height * 0.08f, 0.1f), yaw);
                return;
            }

            float wallHeight = profile == "industrial" ? height * 0.82f : height * 0.68f;
            AddBox(bodies, center + Vector3.up * (wallHeight * 0.5f),
                new Vector3(width, wallHeight, depth), yaw);
            if (profile == "industrial")
            {
                AddBox(roofs, center + Vector3.up * (wallHeight + 0.28f),
                    new Vector3(width * 1.03f, 0.55f, depth * 1.03f), yaw);
                if ((StableHash(building.kind) & 1) == 0)
                {
                    AddCylinder(details,
                        center + Local(width * 0.28f, wallHeight, -depth * 0.24f, yaw),
                        Mathf.Min(width, depth) * 0.08f,
                        Mathf.Max(2.5f, height * 0.55f),
                        yaw,
                        8);
                }
            }
            else if (profile == "urban")
            {
                AddBox(roofs, center + Vector3.up * (wallHeight + 0.24f),
                    new Vector3(width * 1.03f, 0.48f, depth * 1.03f), yaw);
            }
            else
            {
                AddGableRoof(roofs, center + Vector3.up * wallHeight,
                    width * 1.08f, depth * 1.08f, height - wallHeight, yaw);
            }

            int floors = Mathf.Clamp(Mathf.RoundToInt(wallHeight / 3f), 1, 8);
            for (int floor = 0; floor < floors; floor++)
            {
                float windowY = 1.6f + floor * 2.8f;
                AddBox(details,
                    center + Local(0f, windowY, depth * 0.505f, yaw),
                    new Vector3(width * 0.46f, 1.1f, 0.08f), yaw);
            }
        }

        private void AddWall(MapWall wall, MeshBucket bucket)
        {
            Vector2 from = new Vector2(wall.x1, wall.z1);
            Vector2 to = new Vector2(wall.x2, wall.z2);
            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length < 1f) return;
            float yaw = Mathf.Atan2(delta.x, delta.y);
            int pieces = Mathf.Max(1, Mathf.CeilToInt(length / 18f));
            float pieceLength = length / pieces;
            for (int i = 0; i < pieces; i++)
            {
                if (pieces > 2 && i == Mathf.Abs(wall.variant) % pieces) continue;
                Vector2 point = Vector2.Lerp(from, to, (i + 0.5f) / pieces);
                float y = _heightField.HeightAt(point.x, point.y);
                AddBox(bucket, new Vector3(point.x, y + 0.75f, point.y),
                    new Vector3(0.8f, 1.5f, pieceLength * 0.9f), yaw);
            }
        }

        private void AddRubble(
            string mapId,
            MapBuilding[] buildings,
            int count,
            MeshBucket bucket)
        {
            if (count == 0) return;
            System.Random random = new System.Random(StableHash(mapId + "-rubble"));
            for (int i = 0; i < count; i++)
            {
                Vector3 center = FeaturePosition(buildings, random, i, 7f);
                for (int chunk = 0; chunk < 3; chunk++)
                {
                    float size = Mathf.Lerp(0.45f, 1.35f, (float)random.NextDouble());
                    float yaw = (float)random.NextDouble() * Mathf.PI * 2f;
                    Vector3 offset = new Vector3(
                        Mathf.Lerp(-2.2f, 2.2f, (float)random.NextDouble()),
                        size * 0.25f,
                        Mathf.Lerp(-2.2f, 2.2f, (float)random.NextDouble()));
                    AddBox(bucket, center + offset,
                        new Vector3(size, size * 0.55f, size * 0.8f), yaw);
                }
            }
        }

        private void AddSandbags(
            string mapId,
            MapBuilding[] buildings,
            int count,
            MeshBucket bucket)
        {
            if (count == 0) return;
            System.Random random = new System.Random(StableHash(mapId + "-sandbags"));
            for (int line = 0; line < count; line++)
            {
                Vector3 center = FeaturePosition(buildings, random, line, 12f);
                float yaw = (float)random.NextDouble() * Mathf.PI * 2f;
                for (int bag = 0; bag < 6; bag++)
                {
                    Vector3 offset = Local((bag - 2.5f) * 0.85f, 0.28f, 0f, yaw);
                    AddBox(bucket, center + offset, new Vector3(0.78f, 0.52f, 0.45f), yaw);
                }
            }
        }

        private void AddHedgehogs(string mapId, int count, MeshBucket bucket)
        {
            System.Random random = new System.Random(StableHash(mapId + "-hedgehogs"));
            for (int i = 0; i < count; i++)
            {
                Vector3 center = new Vector3(
                    Mathf.Lerp(-340f, 340f, (float)random.NextDouble()),
                    0f,
                    Mathf.Lerp(-340f, 340f, (float)random.NextDouble()));
                center.y = _heightField.HeightAt(center.x, center.z) + 0.9f;
                float yaw = (float)random.NextDouble() * Mathf.PI;
                AddBox(bucket, center, new Vector3(0.22f, 2.1f, 0.22f), yaw + 0.78f);
                AddBox(bucket, center, new Vector3(0.22f, 2.1f, 0.22f), yaw - 0.78f);
                AddBox(bucket, center, new Vector3(0.22f, 0.22f, 2.1f), yaw);
            }
        }

        private Vector3 FeaturePosition(
            MapBuilding[] buildings,
            System.Random random,
            int index,
            float radius)
        {
            Vector3 center;
            if (buildings.Length > 0)
            {
                MapBuilding building = buildings[index % buildings.Length];
                center = new Vector3(building.x, 0f, building.z);
            }
            else
            {
                center = new Vector3(
                    Mathf.Lerp(-340f, 340f, (float)random.NextDouble()),
                    0f,
                    Mathf.Lerp(-340f, 340f, (float)random.NextDouble()));
            }
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            center.x += Mathf.Cos(angle) * radius;
            center.z += Mathf.Sin(angle) * radius;
            center.y = _heightField.HeightAt(center.x, center.z);
            return center;
        }

        private void CreateMesh(string name, MeshBucket bucket, Color color)
        {
            if (bucket.Vertices.Count == 0) return;
            Mesh mesh = new Mesh { name = name + "-Mesh" };
            if (bucket.Vertices.Count > ushort.MaxValue) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(bucket.Vertices);
            mesh.SetTriangles(bucket.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshes.Add(mesh);
            Material material = new Material(Shader.Find("Standard")) { color = color };
            material.SetFloat("_Glossiness", 0.08f);
            _materials.Add(material);
            GameObject node = new GameObject(name);
            node.transform.SetParent(_root.transform, false);
            node.AddComponent<MeshFilter>().sharedMesh = mesh;
            node.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void AddBox(
            MeshBucket bucket,
            Vector3 center,
            Vector3 size,
            float yaw)
        {
            int start = bucket.Vertices.Count;
            float hx = size.x * 0.5f;
            float hy = size.y * 0.5f;
            float hz = size.z * 0.5f;
            Vector3[] corners =
            {
                new Vector3(-hx, -hy, -hz), new Vector3(hx, -hy, -hz),
                new Vector3(hx, hy, -hz), new Vector3(-hx, hy, -hz),
                new Vector3(-hx, -hy, hz), new Vector3(hx, -hy, hz),
                new Vector3(hx, hy, hz), new Vector3(-hx, hy, hz)
            };
            float cos = Mathf.Cos(yaw);
            float sin = Mathf.Sin(yaw);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 value = corners[i];
                bucket.Vertices.Add(center + new Vector3(
                    value.x * cos + value.z * sin,
                    value.y,
                    -value.x * sin + value.z * cos));
            }
            int[] indices =
            {
                0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
                0, 4, 7, 0, 7, 3, 1, 2, 6, 1, 6, 5,
                3, 7, 6, 3, 6, 2, 0, 1, 5, 0, 5, 4
            };
            for (int i = 0; i < indices.Length; i++) bucket.Triangles.Add(start + indices[i]);
        }

        private static void AddGableRoof(
            MeshBucket bucket,
            Vector3 baseCenter,
            float width,
            float depth,
            float height,
            float yaw)
        {
            int start = bucket.Vertices.Count;
            float hx = width * 0.5f;
            float hz = depth * 0.5f;
            Vector3[] local =
            {
                new Vector3(-hx, 0f, -hz), new Vector3(hx, 0f, -hz),
                new Vector3(-hx, 0f, hz), new Vector3(hx, 0f, hz),
                new Vector3(0f, height, -hz), new Vector3(0f, height, hz)
            };
            for (int i = 0; i < local.Length; i++)
                bucket.Vertices.Add(baseCenter + Local(local[i].x, local[i].y, local[i].z, yaw));
            int[] indices =
            {
                0, 1, 4, 2, 5, 3,
                0, 4, 5, 0, 5, 2,
                1, 3, 5, 1, 5, 4,
                0, 2, 3, 0, 3, 1
            };
            for (int i = 0; i < indices.Length; i++) bucket.Triangles.Add(start + indices[i]);
        }

        private static void AddCylinder(
            MeshBucket bucket,
            Vector3 baseCenter,
            float radius,
            float height,
            float yaw,
            int segments)
        {
            int start = bucket.Vertices.Count;
            bucket.Vertices.Add(baseCenter);
            bucket.Vertices.Add(baseCenter + Vector3.up * height);
            for (int i = 0; i < segments; i++)
            {
                float angle = yaw + i * Mathf.PI * 2f / segments;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                bucket.Vertices.Add(baseCenter + offset);
                bucket.Vertices.Add(baseCenter + offset + Vector3.up * height);
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int bottom = start + 2 + i * 2;
                int top = bottom + 1;
                int nextBottom = start + 2 + next * 2;
                int nextTop = nextBottom + 1;
                bucket.Triangles.Add(start);
                bucket.Triangles.Add(nextBottom);
                bucket.Triangles.Add(bottom);
                bucket.Triangles.Add(start + 1);
                bucket.Triangles.Add(top);
                bucket.Triangles.Add(nextTop);
                bucket.Triangles.Add(bottom);
                bucket.Triangles.Add(nextBottom);
                bucket.Triangles.Add(top);
                bucket.Triangles.Add(top);
                bucket.Triangles.Add(nextBottom);
                bucket.Triangles.Add(nextTop);
            }
        }

        private static Vector3 Local(float x, float y, float z, float yaw)
        {
            float cos = Mathf.Cos(yaw);
            float sin = Mathf.Sin(yaw);
            return new Vector3(x * cos + z * sin, y, -x * sin + z * cos);
        }

        private static LandformHeightField BuildHeightField(MapDefinition map)
        {
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

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }

        private sealed class MeshBucket
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int> Triangles = new List<int>();
        }
    }
}
