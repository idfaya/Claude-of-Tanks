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
        public const int MaximumMeshCount = ChunksPerAxis * ChunksPerAxis * 3;
        public const float DefaultVisibleDistanceM = 620f;

        private const float WorldHalfExtentM = 500f;
        private readonly GameObject _root;
        private readonly IHeightField _heightField;
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<ChunkView> _chunks = new List<ChunkView>();

        private MapVegetationRuntime(Transform parent, MapDefinition map, IHeightField heightField)
        {
            _root = new GameObject("Vegetation");
            _root.transform.SetParent(parent, false);
            _heightField = heightField ?? throw new ArgumentNullException(nameof(heightField));
            Build(map);
        }

        public int TreeCount { get; private set; }
        public int MeshCount => _meshes.Count;
        public int VertexCount { get; private set; }
        public int ChunkCount => _chunks.Count;
        public int ActiveChunkCount { get; private set; }
        public Transform Root => _root.transform;

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

        public void Dispose()
        {
            for (int i = 0; i < _materials.Count; i++) DestroyObject(_materials[i]);
            for (int i = 0; i < _meshes.Count; i++) DestroyObject(_meshes[i]);
            DestroyObject(_root);
        }

        private void Build(MapDefinition map)
        {
            MapVegetationLayout layout = map.unityVegetation;
            MapVegetationStand[] stands =
                layout?.stands ?? Array.Empty<MapVegetationStand>();
            ChunkBucket[] buckets = new ChunkBucket[ChunksPerAxis * ChunksPerAxis];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = new ChunkBucket();

            for (int standIndex = 0; standIndex < stands.Length; standIndex++)
            {
                MapVegetationStand stand = stands[standIndex];
                DeterministicRandom random = new DeterministicRandom(
                    stand.seed == 0u ? 1u : stand.seed);
                for (int treeIndex = 0; treeIndex < stand.count; treeIndex++)
                {
                    float angle = random.NextFloat() * MathUtil.Pi * 2f;
                    float radius = stand.radius > 0f
                        ? MathF.Sqrt(random.NextFloat()) * stand.radius
                        : 0f;
                    float x = MathUtil.Clamp(
                        stand.x + MathF.Cos(angle) * radius,
                        -WorldHalfExtentM + 1f,
                        WorldHalfExtentM - 1f);
                    float z = MathUtil.Clamp(
                        stand.z + MathF.Sin(angle) * radius,
                        -WorldHalfExtentM + 1f,
                        WorldHalfExtentM - 1f);
                    float yaw = random.NextFloat() * MathUtil.Pi * 2f;
                    float scale = 0.78f + random.NextFloat() * 0.48f;
                    AddTree(
                        buckets[ChunkIndex(x, z)],
                        stand.species,
                        x,
                        z,
                        yaw,
                        scale);
                    TreeCount++;
                }
            }

            Color ground = map.unitySurface?.groundColor?.ToColor() ??
                new Color(0.28f, 0.34f, 0.22f);
            Material trunk = Material(
                Color.Lerp(new Color(0.24f, 0.15f, 0.08f), ground, 0.18f));
            Material broadleaf = Material(
                Color.Lerp(new Color(0.16f, 0.34f, 0.12f), ground, 0.2f));
            Material conifer = Material(
                Color.Lerp(new Color(0.09f, 0.24f, 0.13f), ground, 0.16f));
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
                    CreateMesh(chunk.transform, "Broadleaf", bucket.Broadleaf, broadleaf);
                    CreateMesh(chunk.transform, "Conifers", bucket.Conifers, conifer);
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
            string species,
            float x,
            float z,
            float yaw,
            float scale)
        {
            bool conifer = IsConifer(species);
            bool palm = string.Equals(species, "palm", StringComparison.Ordinal);
            bool narrow = conifer ||
                string.Equals(species, "poplar", StringComparison.Ordinal) ||
                string.Equals(species, "cypress", StringComparison.Ordinal);
            float height = (palm ? 12f : narrow ? 11f : 8.5f) * scale;
            float crownRadius = (palm ? 3.4f : narrow ? 2.25f : 3.2f) * scale;
            float trunkHeight = height * (palm ? 0.78f : 0.48f);
            float ground = _heightField.HeightAt(x, z);
            AddBox(
                bucket.Trunks,
                new Vector3(x, ground + trunkHeight * 0.5f, z),
                new Vector3(0.42f * scale, trunkHeight, 0.42f * scale),
                yaw);
            MeshBucket canopy = conifer ? bucket.Conifers : bucket.Broadleaf;
            Vector3 crownCenter =
                new Vector3(x, ground + trunkHeight + crownRadius * 0.55f, z);
            if (palm)
            {
                AddPalmCrown(canopy, crownCenter, crownRadius, yaw);
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
        }

        private Material Material(Color color)
        {
            Material material = new Material(Shader.Find("Standard")) { color = color };
            material.SetFloat("_Glossiness", 0.02f);
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

        private static bool IsConifer(string species)
        {
            return species == "pine" ||
                species == "spruce" ||
                species == "fir" ||
                species == "cedar" ||
                species == "cypress";
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
        }

        private sealed class ChunkBucket
        {
            public readonly MeshBucket Trunks = new MeshBucket();
            public readonly MeshBucket Broadleaf = new MeshBucket();
            public readonly MeshBucket Conifers = new MeshBucket();
            public int TreeCount;
        }

        private sealed class ChunkView
        {
            public GameObject Root;
            public Vector3 Center;
        }
    }
}
