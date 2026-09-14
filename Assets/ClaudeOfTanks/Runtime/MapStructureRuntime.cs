using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime : IDisposable
    {
        private readonly GameObject _root;
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Material> _materials = new List<Material>();
        private readonly IHeightField _heightField;
        private readonly Dictionary<string, MapBuilding> _destructibleBuildings =
            new Dictionary<string, MapBuilding>(StringComparer.Ordinal);
        private readonly List<string> _destructibleOrder = new List<string>();
        private readonly HashSet<string> _destroyedBuildings =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<MeshBucket> _ownedBuckets = new List<MeshBucket>();
        private readonly HashSet<string> _builtKinds =
            new HashSet<string>(StringComparer.Ordinal);
        private BattleState _syncedState;
        private uint _syncedRevision = uint.MaxValue;
        private Mesh _debrisMesh;
        private GameObject _debrisNode;
        private Material _debrisMaterial;

        private MapStructureRuntime(Transform parent, MapDefinition map)
        {
            _root = new GameObject("Structures");
            _root.transform.SetParent(parent, false);
            _heightField = MapSimulationAdapter.BuildHeightField(map);
            Build(map);
        }

        public Transform Root => _root.transform;
        public int BuildingCount { get; private set; }
        public int TacticalBuildingCount { get; private set; }
        public int WallRunCount { get; private set; }
        public int RubblePileCount { get; private set; }
        public int SandbagLineCount { get; private set; }
        public int HedgehogCount { get; private set; }
        public int DestroyedBuildingCount => _destroyedBuildings.Count;
        public int DistinctBuildingKindCount => _builtKinds.Count;
        public int MinimumBuildingTriangleCount { get; private set; } =
            int.MaxValue;
        public float MaximumBuildingHeightRatio { get; private set; }

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
                string ownerId = buildings[i].destructible
                    ? MapSimulationAdapter.BuildingObstacleId(map.id, i)
                    : null;
                int bodyStart = bodies.Triangles.Count;
                int roofStart = roofs.Triangles.Count;
                int detailStart = details.Triangles.Count;
                int bodyVertexStart = bodies.Vertices.Count;
                int roofVertexStart = roofs.Vertices.Count;
                int detailVertexStart = details.Vertices.Count;
                AddBuilding(buildings[i], bodies, roofs, details);
                int triangleCount =
                    (bodies.Triangles.Count - bodyStart +
                     roofs.Triangles.Count - roofStart +
                     details.Triangles.Count - detailStart) / 3;
                MinimumBuildingTriangleCount = Mathf.Min(
                    MinimumBuildingTriangleCount,
                    triangleCount);
                _builtKinds.Add(buildings[i].kind);
                float ground = _heightField.HeightAt(
                    buildings[i].x,
                    buildings[i].z);
                float maximumY = Mathf.Max(
                    MaximumY(bodies, bodyVertexStart),
                    MaximumY(roofs, roofVertexStart),
                    MaximumY(details, detailVertexStart));
                MaximumBuildingHeightRatio = Mathf.Max(
                    MaximumBuildingHeightRatio,
                    (maximumY - ground) /
                    Mathf.Max(0.01f, buildings[i].h));
                RecordOwnedRange(bodies, ownerId, bodyStart);
                RecordOwnedRange(roofs, ownerId, roofStart);
                RecordOwnedRange(details, ownerId, detailStart);
                if (ownerId != null)
                {
                    _destructibleBuildings.Add(ownerId, buildings[i]);
                    _destructibleOrder.Add(ownerId);
                }
                if (buildings[i].tactical) TacticalBuildingCount++;
            }
            for (int i = 0; i < walls.Length; i++) AddWall(walls[i], wallMesh);
            if (buildings.Length == 0)
                MinimumBuildingTriangleCount = 0;
            AddRubble(map.id, buildings, RubblePileCount, cover);
            AddSandbags(map.id, buildings, SandbagLineCount, cover);
            AddHedgehogs(map.id, HedgehogCount, details);

            Color building = source.buildingColor != null
                ? source.buildingColor.ToColor()
                : new Color(0.62f, 0.61f, 0.57f);
            Color roof = Color.Lerp(building, new Color(0.15f, 0.16f, 0.16f), 0.58f);
            Color dark = new Color(0.12f, 0.14f, 0.14f);
            _debrisMaterial = CreateMaterial(
                Color.Lerp(building, new Color(0.15f, 0.12f, 0.1f), 0.72f));
            CreateMesh("Structures-Bodies", bodies, building);
            CreateMesh("Structures-Roofs", roofs, roof);
            CreateMesh("Structures-Details", details, dark);
            CreateMesh("Structures-Walls", wallMesh, Color.Lerp(building, Color.gray, 0.3f));
            CreateMesh("Structures-Cover", cover, Color.Lerp(building, new Color(0.2f, 0.17f, 0.13f), 0.55f));
        }

        public void SyncDestroyedStructures(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (ReferenceEquals(state, _syncedState) &&
                state.StaticObstacleRevision == _syncedRevision)
            {
                return;
            }

            _syncedState = state;
            _syncedRevision = state.StaticObstacleRevision;
            HashSet<string> desired = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < state.StaticObstacles.Length; i++)
            {
                StaticObstacle obstacle = state.StaticObstacles[i];
                if (obstacle.Destructible &&
                    state.IsStaticObstacleDestroyed(i) &&
                    _destructibleBuildings.ContainsKey(obstacle.Id))
                {
                    desired.Add(obstacle.Id);
                }
            }
            if (_destroyedBuildings.SetEquals(desired)) return;
            _destroyedBuildings.Clear();
            foreach (string id in desired) _destroyedBuildings.Add(id);
            for (int i = 0; i < _ownedBuckets.Count; i++)
                _ownedBuckets[i].ApplyDestroyed(_destroyedBuildings);
            RebuildDebris();
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

        private static void RecordOwnedRange(
            MeshBucket bucket,
            string ownerId,
            int triangleStart)
        {
            if (ownerId == null || triangleStart == bucket.Triangles.Count) return;
            bucket.OwnedRanges.Add(new OwnedTriangleRange
            {
                OwnerId = ownerId,
                Start = triangleStart,
                Count = bucket.Triangles.Count - triangleStart
            });
        }

        private static float MaximumY(
            MeshBucket bucket,
            int start)
        {
            float maximum = float.MinValue;
            for (int i = start;
                i < bucket.Vertices.Count;
                i++)
            {
                maximum = Mathf.Max(
                    maximum,
                    bucket.Vertices[i].y);
            }
            return maximum;
        }

        private void RebuildDebris()
        {
            if (_destroyedBuildings.Count == 0)
            {
                if (_debrisMesh != null) _debrisMesh.Clear();
                if (_debrisNode != null) _debrisNode.SetActive(false);
                return;
            }

            MeshBucket debris = new MeshBucket();
            for (int ownerIndex = 0; ownerIndex < _destructibleOrder.Count; ownerIndex++)
            {
                string ownerId = _destructibleOrder[ownerIndex];
                if (!_destroyedBuildings.Contains(ownerId)) continue;
                MapBuilding building = _destructibleBuildings[ownerId];
                System.Random random = new System.Random(StableHash(ownerId + "-destroyed"));
                float yaw = building.yawDeg * Mathf.Deg2Rad;
                float width = Mathf.Max(3f, building.w);
                float depth = Mathf.Max(3f, building.d);
                float ground = _heightField.HeightAt(building.x, building.z);
                for (int chunk = 0; chunk < 9; chunk++)
                {
                    float chunkWidth = Mathf.Lerp(
                        width * 0.12f,
                        width * 0.28f,
                        (float)random.NextDouble());
                    float chunkDepth = Mathf.Lerp(
                        depth * 0.12f,
                        depth * 0.28f,
                        (float)random.NextDouble());
                    float chunkHeight = Mathf.Lerp(
                        0.35f,
                        1.25f,
                        (float)random.NextDouble());
                    float localX = Mathf.Lerp(
                        -width * 0.38f,
                        width * 0.38f,
                        (float)random.NextDouble());
                    float localZ = Mathf.Lerp(
                        -depth * 0.38f,
                        depth * 0.38f,
                        (float)random.NextDouble());
                    Vector3 center = new Vector3(building.x, ground, building.z) +
                        Local(localX, chunkHeight * 0.5f, localZ, yaw);
                    AddBox(
                        debris,
                        center,
                        new Vector3(chunkWidth, chunkHeight, chunkDepth),
                        yaw + Mathf.Lerp(-0.35f, 0.35f, (float)random.NextDouble()));
                }
            }

            if (_debrisMesh == null)
            {
                _debrisMesh = new Mesh { name = "Structures-Destroyed-Mesh" };
                _meshes.Add(_debrisMesh);
                _debrisNode = new GameObject("Structures-Destroyed");
                _debrisNode.transform.SetParent(_root.transform, false);
                _debrisNode.AddComponent<MeshFilter>().sharedMesh = _debrisMesh;
                _debrisNode.AddComponent<MeshRenderer>().sharedMaterial = _debrisMaterial;
            }
            _debrisMesh.Clear();
            _debrisMesh.SetVertices(debris.Vertices);
            _debrisMesh.SetTriangles(debris.Triangles, 0);
            _debrisMesh.RecalculateNormals();
            _debrisMesh.RecalculateBounds();
            _debrisNode.SetActive(true);
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
            bucket.Mesh = mesh;
            if (bucket.OwnedRanges.Count > 0) _ownedBuckets.Add(bucket);
            Material material = CreateMaterial(color);
            GameObject node = new GameObject(name);
            node.transform.SetParent(_root.transform, false);
            node.AddComponent<MeshFilter>().sharedMesh = mesh;
            node.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private Material CreateMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Standard")) { color = color };
            material.SetFloat("_Glossiness", 0.08f);
            _materials.Add(material);
            return material;
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
            public readonly List<OwnedTriangleRange> OwnedRanges =
                new List<OwnedTriangleRange>();
            private readonly List<int> _activeTriangles = new List<int>();
            public Mesh Mesh;

            public void ApplyDestroyed(HashSet<string> destroyedOwners)
            {
                if (Mesh == null) return;
                _activeTriangles.Clear();
                int rangeIndex = 0;
                for (int triangleIndex = 0;
                    triangleIndex < Triangles.Count;
                    triangleIndex += 3)
                {
                    while (rangeIndex < OwnedRanges.Count &&
                        triangleIndex >= OwnedRanges[rangeIndex].Start +
                            OwnedRanges[rangeIndex].Count)
                    {
                        rangeIndex++;
                    }
                    bool removed =
                        rangeIndex < OwnedRanges.Count &&
                        triangleIndex >= OwnedRanges[rangeIndex].Start &&
                        destroyedOwners.Contains(OwnedRanges[rangeIndex].OwnerId);
                    if (removed) continue;
                    _activeTriangles.Add(Triangles[triangleIndex]);
                    _activeTriangles.Add(Triangles[triangleIndex + 1]);
                    _activeTriangles.Add(Triangles[triangleIndex + 2]);
                }
                Mesh.SetTriangles(_activeTriangles, 0);
            }
        }

        private sealed class OwnedTriangleRange
        {
            public string OwnerId;
            public int Start;
            public int Count;
        }
    }
}
