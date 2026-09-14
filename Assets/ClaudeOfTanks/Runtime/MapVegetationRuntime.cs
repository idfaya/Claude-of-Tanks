using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class MapVegetationRuntime : IDisposable
    {
        public const int ChunksPerAxis = 4;
        public const int MaximumMeshCount = ChunksPerAxis * ChunksPerAxis * 6 + 1;
        public const float DefaultVisibleDistanceM = 620f;

        private const float WorldHalfExtentM = 500f;
        private readonly GameObject _root;
        private readonly IHeightField _heightField;
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<ChunkView> _chunks = new List<ChunkView>();
        private readonly List<MeshBinding> _bindings = new List<MeshBinding>();
        private readonly VegetationTreePlacement[] _trees;
        private readonly string _treeObstaclePrefix;
        private readonly bool[] _destroyedTrees;
        private readonly bool[] _desiredDestroyedTrees;
        private Mesh _fallenMesh;
        private BattleState _syncedState;
        private uint _syncedRevision = uint.MaxValue;

        private MapVegetationRuntime(Transform parent, MapDefinition map, IHeightField heightField)
        {
            _root = new GameObject("Vegetation");
            _root.transform.SetParent(parent, false);
            _heightField = heightField ?? throw new ArgumentNullException(nameof(heightField));
            _trees = MapVegetationPlacementBuilder.Expand(map);
            _treeObstaclePrefix = (map.id ?? "map") + "-tree-";
            _destroyedTrees = new bool[_trees.Length];
            _desiredDestroyedTrees = new bool[_trees.Length];
            Build(map);
        }

        public int TreeCount { get; private set; }
        public int MeshCount => _meshes.Count;
        public int VertexCount { get; private set; }
        public int ChunkCount => _chunks.Count;
        public int ActiveChunkCount { get; private set; }
        public Transform Root => _root.transform;
        public int ToppledTreeCount { get; private set; }

        public static MapVegetationRuntime Create(
            Transform parent,
            MapDefinition map,
            IHeightField heightField)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (map == null) throw new ArgumentNullException(nameof(map));
            return new MapVegetationRuntime(parent, map, heightField);
        }

        public void UpdateVisibility(
            Vector3 cameraPosition,
            float visibleDistanceM = DefaultVisibleDistanceM)
        {
            float distance = Mathf.Max(0f, visibleDistanceM);
            float chunkSize = WorldHalfExtentM * 2f / ChunksPerAxis;
            float reach = distance + chunkSize * 0.72f;
            float reachSquared = reach * reach;
            int active = 0;
            for (int i = 0; i < _chunks.Count; i++)
            {
                ChunkView chunk = _chunks[i];
                float dx = cameraPosition.x - chunk.Center.x;
                float dz = cameraPosition.z - chunk.Center.z;
                bool visible = dx * dx + dz * dz <= reachSquared;
                if (chunk.Root.activeSelf != visible) chunk.Root.SetActive(visible);
                if (visible) active++;
            }
            ActiveChunkCount = active;
        }

        public void SyncDestroyedTrees(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (ReferenceEquals(state, _syncedState) &&
                state.StaticObstacleRevision == _syncedRevision)
            {
                return;
            }
            Array.Clear(_desiredDestroyedTrees, 0, _desiredDestroyedTrees.Length);
            int toppled = 0;
            for (int i = 0; i < state.StaticObstacles.Length; i++)
            {
                if (!state.IsStaticObstacleDestroyed(i)) continue;
                string id = state.StaticObstacles[i].Id;
                if (!id.StartsWith(_treeObstaclePrefix, StringComparison.Ordinal)) continue;
                int treeIndex;
                if (!int.TryParse(
                        id.Substring(_treeObstaclePrefix.Length),
                        out treeIndex) ||
                    treeIndex < 0 ||
                    treeIndex >= _desiredDestroyedTrees.Length)
                {
                    continue;
                }
                if (!_desiredDestroyedTrees[treeIndex])
                {
                    _desiredDestroyedTrees[treeIndex] = true;
                    toppled++;
                }
            }
            bool changed = false;
            for (int i = 0; i < _destroyedTrees.Length; i++)
            {
                if (_destroyedTrees[i] == _desiredDestroyedTrees[i]) continue;
                changed = true;
                break;
            }
            _syncedState = state;
            _syncedRevision = state.StaticObstacleRevision;
            if (!changed) return;
            Array.Copy(
                _desiredDestroyedTrees,
                _destroyedTrees,
                _destroyedTrees.Length);
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Apply(_destroyedTrees);
            RebuildFallenMesh();
            ToppledTreeCount = toppled;
        }

        public void Dispose()
        {
            for (int i = 0; i < _materials.Count; i++)
                MapMaterialFactory.Destroy(_materials[i]);
            for (int i = 0; i < _meshes.Count; i++) DestroyObject(_meshes[i]);
            DestroyObject(_root);
        }

        private void Build(MapDefinition map)
        {
            ChunkBucket[] buckets = new ChunkBucket[ChunksPerAxis * ChunksPerAxis];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = new ChunkBucket();

            for (int treeIndex = 0; treeIndex < _trees.Length; treeIndex++)
            {
                VegetationTreePlacement tree = _trees[treeIndex];
                AddTree(buckets[ChunkIndex(tree.X, tree.Z)], tree);
                TreeCount++;
            }

            Color ground = map.unitySurface?.groundColor?.ToColor() ??
                new Color(0.28f, 0.34f, 0.22f);
            string seed = map.id ?? "map";
            Material trunk = Material(
                Color.Lerp(new Color(0.24f, 0.15f, 0.08f), ground, 0.18f),
                MapMaterialRole.Bark,
                seed + "-bark");
            Material birchTrunk = Material(
                Color.Lerp(new Color(0.82f, 0.79f, 0.69f), ground, 0.08f),
                MapMaterialRole.Birch,
                seed + "-birch-bark");
            Material broadleaf = Material(
                Color.Lerp(new Color(0.16f, 0.34f, 0.12f), ground, 0.2f),
                MapMaterialRole.Broadleaf,
                seed + "-broadleaf");
            Material conifer = Material(
                Color.Lerp(new Color(0.09f, 0.24f, 0.13f), ground, 0.16f),
                MapMaterialRole.Conifer,
                seed + "-conifer");
            Material palm = Material(
                Color.Lerp(new Color(0.18f, 0.38f, 0.15f), ground, 0.12f),
                MapMaterialRole.Palm,
                seed + "-palm");
            Material birch = Material(
                Color.Lerp(new Color(0.36f, 0.40f, 0.28f), ground, 0.34f),
                MapMaterialRole.Birch,
                seed + "-birch-crown");
            CreateFallenMesh(trunk);
            float chunkSize = WorldHalfExtentM * 2f / ChunksPerAxis;
            for (int z = 0; z < ChunksPerAxis; z++)
            {
                for (int x = 0; x < ChunksPerAxis; x++)
                {
                    ChunkBucket bucket = buckets[z * ChunksPerAxis + x];
                    if (bucket.TreeCount == 0) continue;
                    GameObject chunk = new GameObject("Vegetation-" + x + "-" + z);
                    chunk.transform.SetParent(_root.transform, false);
                    CreateMesh(chunk.transform, "Trunks", bucket.Trunks, trunk);
                    CreateMesh(chunk.transform, "Birch-Trunks", bucket.BirchTrunks, birchTrunk);
                    CreateMesh(chunk.transform, "Broadleaf", bucket.Broadleaf, broadleaf);
                    CreateMesh(chunk.transform, "Conifers", bucket.Conifers, conifer);
                    CreateMesh(chunk.transform, "Palms", bucket.Palms, palm);
                    CreateMesh(chunk.transform, "Birch-Crowns", bucket.BirchCrowns, birch);
                    _chunks.Add(new ChunkView
                    {
                        Root = chunk,
                        Center = new Vector3(
                            -WorldHalfExtentM + (x + 0.5f) * chunkSize,
                            0f,
                            -WorldHalfExtentM + (z + 0.5f) * chunkSize)
                    });
                }
            }
            ActiveChunkCount = _chunks.Count;
        }

        private void AddTree(
            ChunkBucket bucket,
            VegetationTreePlacement tree)
        {
            string species = tree.Species;
            float x = tree.X;
            float z = tree.Z;
            float yaw = tree.Yaw;
            float scale = tree.Scale;
            bool conifer = MapVegetationPlacementBuilder.IsConifer(species);
            bool palm = string.Equals(species, "palm", StringComparison.Ordinal);
            bool birch = MapVegetationPlacementBuilder.IsBirchFamily(species);
            bool narrow = conifer ||
                string.Equals(species, "poplar", StringComparison.Ordinal) ||
                string.Equals(species, "cypress", StringComparison.Ordinal) ||
                string.Equals(species, "eucalyptus", StringComparison.Ordinal);
            float height = tree.Height;
            float crownRadius = tree.CrownRadius;
            float trunkHeight = tree.TrunkHeight;
            float ground = _heightField.HeightAt(x, z);
            MeshBucket trunkBucket = birch ? bucket.BirchTrunks : bucket.Trunks;
            int trunkStart = trunkBucket.Triangles.Count;
            AddBox(
                trunkBucket,
                new Vector3(x, ground + trunkHeight * 0.5f, z),
                new Vector3(tree.TrunkRadius * 2f, trunkHeight, tree.TrunkRadius * 2f),
                yaw);
            MeshBucket canopy = palm
                ? bucket.Palms
                : birch
                    ? bucket.BirchCrowns
                    : conifer
                        ? bucket.Conifers
                        : bucket.Broadleaf;
            int canopyStart = canopy.Triangles.Count;
            Vector3 crownCenter =
                new Vector3(x, ground + tree.CanopyCenterHeight, z);
            if (palm)
            {
                AddPalmCrown(canopy, crownCenter, crownRadius, yaw);
            }
            else if (birch)
            {
                AddBirchCrown(canopy, crownCenter, crownRadius, height, yaw);
            }
            else if (conifer)
            {
                AddCone(
                    canopy,
                    new Vector3(x, ground + trunkHeight * 0.72f, z),
                    crownRadius,
                    height * 0.58f,
                    yaw);
                AddCone(
                    canopy,
                    new Vector3(x, ground + trunkHeight * 0.98f, z),
                    crownRadius * 0.76f,
                    height * 0.48f,
                    yaw + 0.35f);
            }
            else if (narrow)
            {
                AddOctahedron(
                    canopy,
                    crownCenter + Vector3.up * crownRadius * 0.45f,
                    crownRadius * 0.72f,
                    crownRadius * 1.9f);
                AddOctahedron(
                    canopy,
                    crownCenter - Vector3.up * crownRadius * 0.45f,
                    crownRadius * 0.88f,
                    crownRadius * 1.65f);
            }
            else
            {
                AddOctahedron(
                    canopy,
                    crownCenter,
                    crownRadius,
                    crownRadius * 1.15f);
                AddOctahedron(
                    canopy,
                    crownCenter + Local(
                        crownRadius * 0.48f,
                        crownRadius * 0.08f,
                        crownRadius * 0.12f,
                        yaw),
                    crownRadius * 0.68f,
                    crownRadius * 0.85f);
                AddOctahedron(
                    canopy,
                    crownCenter + Local(
                        -crownRadius * 0.38f,
                        -crownRadius * 0.05f,
                        crownRadius * 0.3f,
                        yaw),
                    crownRadius * 0.62f,
                    crownRadius * 0.82f);
            }
            trunkBucket.AddOwner(
                tree.Index,
                trunkStart,
                trunkBucket.Triangles.Count - trunkStart);
            canopy.AddOwner(
                tree.Index,
                canopyStart,
                canopy.Triangles.Count - canopyStart);
            bucket.TreeCount++;
        }

        private void CreateMesh(
            Transform parent,
            string name,
            MeshBucket bucket,
            Material material)
        {
            if (bucket.Vertices.Count == 0) return;
            Mesh mesh = new Mesh { name = name + "-Mesh" };
            if (bucket.Vertices.Count > ushort.MaxValue)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(bucket.Vertices);
            mesh.SetTriangles(bucket.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshes.Add(mesh);
            VertexCount += bucket.Vertices.Count;
            GameObject node = new GameObject(name);
            node.transform.SetParent(parent, false);
            node.AddComponent<MeshFilter>().sharedMesh = mesh;
            node.AddComponent<MeshRenderer>().sharedMaterial = material;
            _bindings.Add(new MeshBinding(mesh, bucket));
        }

        private void CreateFallenMesh(Material trunkMaterial)
        {
            _fallenMesh = new Mesh { name = "Fallen-Trees-Mesh" };
            _fallenMesh.indexFormat = IndexFormat.UInt32;
            _meshes.Add(_fallenMesh);
            GameObject node = new GameObject("Fallen-Trees");
            node.transform.SetParent(_root.transform, false);
            node.AddComponent<MeshFilter>().sharedMesh = _fallenMesh;
            node.AddComponent<MeshRenderer>().sharedMaterial = trunkMaterial;
        }

        private void RebuildFallenMesh()
        {
            MeshBucket fallen = new MeshBucket();
            for (int i = 0; i < _trees.Length; i++)
            {
                if (!_destroyedTrees[i]) continue;
                VegetationTreePlacement tree = _trees[i];
                float ground = _heightField.HeightAt(tree.X, tree.Z);
                AddFallenBox(fallen, tree, ground);
            }
            _fallenMesh.Clear();
            _fallenMesh.SetVertices(fallen.Vertices);
            _fallenMesh.SetTriangles(fallen.Triangles, 0);
            _fallenMesh.RecalculateNormals();
            _fallenMesh.RecalculateBounds();
        }

        private Material Material(
            Color color,
            MapMaterialRole role,
            string seed)
        {
            Material material = MapMaterialFactory.Create(color, role, seed);
            _materials.Add(material);
            return material;
        }

        private static int ChunkIndex(float x, float z)
        {
            float size = WorldHalfExtentM * 2f / ChunksPerAxis;
            int chunkX = Math.Min(
                ChunksPerAxis - 1,
                Math.Max(0, (int)((x + WorldHalfExtentM) / size)));
            int chunkZ = Math.Min(
                ChunksPerAxis - 1,
                Math.Max(0, (int)((z + WorldHalfExtentM) / size)));
            return chunkZ * ChunksPerAxis + chunkX;
        }

        private static void AddOctahedron(
            MeshBucket bucket,
            Vector3 center,
            float radius,
            float height)
        {
            int start = bucket.Vertices.Count;
            float halfHeight = height * 0.5f;
            bucket.Vertices.Add(center + Vector3.up * halfHeight);
            bucket.Vertices.Add(center - Vector3.up * halfHeight);
            bucket.Vertices.Add(center + Vector3.right * radius);
            bucket.Vertices.Add(center - Vector3.right * radius);
            bucket.Vertices.Add(center + Vector3.forward * radius);
            bucket.Vertices.Add(center - Vector3.forward * radius);
            int[] triangles =
            {
                0, 4, 2, 0, 3, 4, 0, 5, 3, 0, 2, 5,
                1, 2, 4, 1, 4, 3, 1, 3, 5, 1, 5, 2
            };
            for (int i = 0; i < triangles.Length; i++)
                bucket.Triangles.Add(start + triangles[i]);
        }

        private static void AddCone(
            MeshBucket bucket,
            Vector3 baseCenter,
            float radius,
            float height,
            float yaw)
        {
            const int segments = 6;
            int start = bucket.Vertices.Count;
            bucket.Vertices.Add(baseCenter + Vector3.up * height);
            bucket.Vertices.Add(baseCenter);
            for (int i = 0; i < segments; i++)
            {
                float angle = yaw + i * MathUtil.Pi * 2f / segments;
                bucket.Vertices.Add(baseCenter + new Vector3(
                    MathF.Cos(angle) * radius,
                    0f,
                    MathF.Sin(angle) * radius));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                bucket.Triangles.Add(start);
                bucket.Triangles.Add(start + 2 + i);
                bucket.Triangles.Add(start + 2 + next);
                bucket.Triangles.Add(start + 1);
                bucket.Triangles.Add(start + 2 + next);
                bucket.Triangles.Add(start + 2 + i);
            }
        }

        private static void AddPalmCrown(
            MeshBucket bucket,
            Vector3 center,
            float radius,
            float yaw)
        {
            for (int frond = 0; frond < 7; frond++)
            {
                float angle = yaw + frond * MathUtil.Pi * 2f / 7f;
                Vector3 direction = new Vector3(
                    MathF.Sin(angle),
                    -0.16f,
                    MathF.Cos(angle));
                Vector3 frondCenter = center + direction * radius * 0.55f;
                AddBox(
                    bucket,
                    frondCenter,
                    new Vector3(radius * 0.28f, 0.12f, radius * 1.25f),
                    angle);
            }
        }

        private static void AddBirchCrown(
            MeshBucket bucket,
            Vector3 center,
            float radius,
            float height,
            float yaw)
        {
            AddOctahedron(bucket, center, radius * 0.82f, height * 0.38f);
            AddOctahedron(
                bucket,
                center + Local(radius * 0.42f, radius * 0.38f, radius * 0.18f, yaw),
                radius * 0.48f,
                height * 0.34f);
            AddOctahedron(
                bucket,
                center + Local(-radius * 0.34f, radius * 0.22f, radius * 0.28f, yaw),
                radius * 0.42f,
                height * 0.3f);
            for (int branch = 0; branch < 4; branch++)
            {
                float angle = yaw + branch * MathUtil.Pi * 0.5f;
                Vector3 offset = new Vector3(
                    MathF.Sin(angle) * radius * 0.38f,
                    radius * 0.2f,
                    MathF.Cos(angle) * radius * 0.38f);
                AddBox(
                    bucket,
                    center + offset,
                    new Vector3(radius * 0.08f, height * 0.32f, radius * 0.08f),
                    angle);
            }
        }

        private static Vector3 Local(float x, float y, float z, float yaw)
        {
            float cos = MathF.Cos(yaw);
            float sin = MathF.Sin(yaw);
            return new Vector3(
                x * cos + z * sin,
                y,
                -x * sin + z * cos);
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
            int[] triangles =
            {
                0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
                0, 4, 7, 0, 7, 3, 1, 2, 6, 1, 6, 5,
                3, 7, 6, 3, 6, 2, 0, 1, 5, 0, 5, 4
            };
            for (int i = 0; i < triangles.Length; i++)
                bucket.Triangles.Add(start + triangles[i]);
        }

        private static void AddFallenBox(
            MeshBucket bucket,
            VegetationTreePlacement tree,
            float ground)
        {
            float directionX = MathF.Sin(tree.Yaw);
            float directionZ = MathF.Cos(tree.Yaw);
            float rightX = directionZ;
            float rightZ = -directionX;
            float halfLength = tree.TrunkHeight * 0.5f;
            float halfWidth = MathF.Max(0.16f, tree.TrunkRadius);
            Vector3 center = new Vector3(
                tree.X + directionX * halfLength,
                ground + halfWidth,
                tree.Z + directionZ * halfLength);
            int start = bucket.Vertices.Count;
            for (int y = -1; y <= 1; y += 2)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int end = -1; end <= 1; end += 2)
                    {
                        bucket.Vertices.Add(center + new Vector3(
                            rightX * halfWidth * side + directionX * halfLength * end,
                            halfWidth * y,
                            rightZ * halfWidth * side + directionZ * halfLength * end));
                    }
                }
            }
            int[] triangles =
            {
                0, 1, 3, 0, 3, 2, 4, 6, 7, 4, 7, 5,
                0, 4, 5, 0, 5, 1, 2, 3, 7, 2, 7, 6,
                0, 2, 6, 0, 6, 4, 1, 5, 7, 1, 7, 3
            };
            for (int i = 0; i < triangles.Length; i++)
                bucket.Triangles.Add(start + triangles[i]);
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }

        private sealed class MeshBucket
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int> Triangles = new List<int>();
            public readonly List<TreeTriangleRange> Owners =
                new List<TreeTriangleRange>();

            public void AddOwner(int treeIndex, int start, int count)
            {
                Owners.Add(new TreeTriangleRange
                {
                    TreeIndex = treeIndex,
                    Start = start,
                    Count = count
                });
            }
        }

        private struct TreeTriangleRange
        {
            public int TreeIndex;
            public int Start;
            public int Count;
        }

        private sealed class MeshBinding
        {
            private readonly Mesh _mesh;
            private readonly int[] _triangles;
            private readonly TreeTriangleRange[] _owners;
            private readonly List<int> _active = new List<int>();

            public MeshBinding(Mesh mesh, MeshBucket bucket)
            {
                _mesh = mesh;
                _triangles = bucket.Triangles.ToArray();
                _owners = bucket.Owners.ToArray();
                _active.Capacity = _triangles.Length;
            }

            public void Apply(bool[] destroyedTrees)
            {
                _active.Clear();
                for (int i = 0; i < _owners.Length; i++)
                {
                    TreeTriangleRange owner = _owners[i];
                    if (destroyedTrees[owner.TreeIndex]) continue;
                    for (int triangle = owner.Start;
                        triangle < owner.Start + owner.Count;
                        triangle++)
                    {
                        _active.Add(_triangles[triangle]);
                    }
                }
                _mesh.SetTriangles(_active, 0);
            }
        }

        private sealed class ChunkBucket
        {
            public readonly MeshBucket Trunks = new MeshBucket();
            public readonly MeshBucket BirchTrunks = new MeshBucket();
            public readonly MeshBucket Broadleaf = new MeshBucket();
            public readonly MeshBucket Conifers = new MeshBucket();
            public readonly MeshBucket Palms = new MeshBucket();
            public readonly MeshBucket BirchCrowns = new MeshBucket();
            public int TreeCount;
        }

        private sealed class ChunkView
        {
            public GameObject Root;
            public Vector3 Center;
        }
    }
}
