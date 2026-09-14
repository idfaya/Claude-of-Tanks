using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class MapRuntime : IDisposable
    {
        private const float GroundSurfaceY = 0.025f;
        private const int DiscSegments = 24;
        private const int TerrainChunksPerAxis = 4;
        private const int TerrainQuadsPerChunk = 32;
        private const float TerrainHalfExtentM = 500f;

        private readonly GameObject _root;
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private IHeightField _heightField;
        private MapStructureRuntime _structures;
        private MapVegetationRuntime _vegetation;

        private MapRuntime(GameObject root)
        {
            _root = root;
        }

        public Transform Root => _root.transform;
        public int RoadPolylineCount { get; private set; }
        public int LakeCount { get; private set; }
        public int MarshCount { get; private set; }
        public int CraterCount { get; private set; }
        public int BuildingCount => _structures != null ? _structures.BuildingCount : 0;
        public int TacticalBuildingCount =>
            _structures != null ? _structures.TacticalBuildingCount : 0;
        public int WallRunCount => _structures != null ? _structures.WallRunCount : 0;
        public int RubblePileCount => _structures != null ? _structures.RubblePileCount : 0;
        public int SandbagLineCount => _structures != null ? _structures.SandbagLineCount : 0;
        public int HedgehogCount => _structures != null ? _structures.HedgehogCount : 0;
        public int DestroyedBuildingCount =>
            _structures != null ? _structures.DestroyedBuildingCount : 0;
        public int DistinctBuildingKindCount =>
            _structures != null
                ? _structures.DistinctBuildingKindCount
                : 0;
        public int MinimumBuildingTriangleCount =>
            _structures != null
                ? _structures.MinimumBuildingTriangleCount
                : 0;
        public float MaximumBuildingHeightRatio =>
            _structures != null
                ? _structures.MaximumBuildingHeightRatio
                : 0f;
        public int TreeCount => _vegetation != null ? _vegetation.TreeCount : 0;
        public int VegetationMeshCount =>
            _vegetation != null ? _vegetation.MeshCount : 0;
        public int VegetationVertexCount =>
            _vegetation != null ? _vegetation.VertexCount : 0;
        public int VegetationChunkCount =>
            _vegetation != null ? _vegetation.ChunkCount : 0;
        public int ActiveVegetationChunkCount =>
            _vegetation != null ? _vegetation.ActiveChunkCount : 0;
        public int ToppledTreeCount =>
            _vegetation != null ? _vegetation.ToppledTreeCount : 0;
        public const int TerrainChunkCount =
            TerrainChunksPerAxis * TerrainChunksPerAxis;
        public int TerrainVertexCount { get; private set; }
        public int TerrainTriangleCount { get; private set; }

        public static MapRuntime Create(MapDefinition map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            GameObject root = new GameObject("Map-" + map.id);
            MapRuntime runtime = new MapRuntime(root);
            runtime.Build(map);
            return runtime;
        }

        public void SyncDestroyedStructures(BattleState state)
        {
            _structures?.SyncDestroyedStructures(state);
            _vegetation?.SyncDestroyedTrees(state);
        }

        public void UpdateVegetationVisibility(
            Vector3 cameraPosition,
            float visibleDistanceM = MapVegetationRuntime.DefaultVisibleDistanceM)
        {
            _vegetation?.UpdateVisibility(cameraPosition, visibleDistanceM);
        }

        public void Dispose()
        {
            _structures?.Dispose();
            _vegetation?.Dispose();
            for (int i = 0; i < _materials.Count; i++) DestroyObject(_materials[i]);
            for (int i = 0; i < _meshes.Count; i++) DestroyObject(_meshes[i]);
            DestroyObject(_root);
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }

        private void Build(MapDefinition map)
        {
            _heightField = MapSimulationAdapter.BuildHeightField(map);
            Color fog = HexColor(map.sky != null ? map.sky.fogTintHex : 0x8799a0);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = Mathf.Max(0.0004f, (map.sky?.fogDensity ?? 0.0007f) * 6f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(Color.black, fog, 0.42f);

            GameObject sun = new GameObject("Sun");
            sun.transform.SetParent(_root.transform, false);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = HexColor(map.sky != null ? map.sky.sunColorHex : 0xfff2cc);
            light.intensity = Mathf.Clamp(map.sky?.sunIntensity ?? 1.0f, 0.4f, 1.05f);
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(
                map.sky?.sunElevationDeg ?? 48f, -(map.sky?.sunAzimuthDeg ?? 32f), 0f);
            RenderSettings.sun = light;

            MapSurface surface = map.unitySurface;
            Color groundColor = surface != null
                ? surface.groundColor.ToColor()
                : GroundColor(map.id);
            CreateTerrain(groundColor);
            CreateGroundVariation(map.id, surface, groundColor);
            CreateWetGround(surface);
            CreateRoads(surface);
            CreateCraters(map.id, map.props?.craters ?? 0, groundColor);
            _structures = MapStructureRuntime.Create(_root.transform, map);
            _vegetation = MapVegetationRuntime.Create(
                _root.transform,
                map,
                _heightField);

            int rocks = Mathf.Clamp((map.props?.rocks ?? 0) / 18, 3, 18);
            DeterministicScatter(map.id, rocks, groundColor);
        }

        private void CreateTerrain(Color color)
        {
            GameObject root = new GameObject("Battlefield");
            root.transform.SetParent(_root.transform, false);
            Material material = CreateMaterial(color);
            float chunkSize = TerrainHalfExtentM * 2f / TerrainChunksPerAxis;
            float step = chunkSize / TerrainQuadsPerChunk;
            ITerrainSurface terrainSurface = _heightField as ITerrainSurface;
            for (int chunkZ = 0; chunkZ < TerrainChunksPerAxis; chunkZ++)
            {
                for (int chunkX = 0; chunkX < TerrainChunksPerAxis; chunkX++)
                {
                    int vertexAxis = TerrainQuadsPerChunk + 1;
                    Vector3[] vertices = new Vector3[vertexAxis * vertexAxis];
                    Vector3[] normals = new Vector3[vertices.Length];
                    Vector2[] uvs = new Vector2[vertices.Length];
                    int[] triangles = new int[
                        TerrainQuadsPerChunk * TerrainQuadsPerChunk * 6];
                    float originX = -TerrainHalfExtentM + chunkX * chunkSize;
                    float originZ = -TerrainHalfExtentM + chunkZ * chunkSize;
                    for (int z = 0; z < vertexAxis; z++)
                    {
                        for (int x = 0; x < vertexAxis; x++)
                        {
                            int index = z * vertexAxis + x;
                            float worldX = originX + x * step;
                            float worldZ = originZ + z * step;
                            vertices[index] = new Vector3(
                                worldX,
                                _heightField.HeightAt(worldX, worldZ),
                                worldZ);
                            Float3 normal = terrainSurface != null
                                ? terrainSurface.NormalAt(worldX, worldZ)
                                : new Float3(0f, 1f, 0f);
                            normals[index] = new Vector3(normal.X, normal.Y, normal.Z);
                            uvs[index] = new Vector2(
                                (worldX + TerrainHalfExtentM) / (TerrainHalfExtentM * 2f),
                                (worldZ + TerrainHalfExtentM) / (TerrainHalfExtentM * 2f));
                        }
                    }
                    int triangle = 0;
                    for (int z = 0; z < TerrainQuadsPerChunk; z++)
                    {
                        for (int x = 0; x < TerrainQuadsPerChunk; x++)
                        {
                            int current = z * vertexAxis + x;
                            triangles[triangle++] = current;
                            triangles[triangle++] = current + vertexAxis;
                            triangles[triangle++] = current + 1;
                            triangles[triangle++] = current + 1;
                            triangles[triangle++] = current + vertexAxis;
                            triangles[triangle++] = current + vertexAxis + 1;
                        }
                    }

                    Mesh mesh = new Mesh
                    {
                        name = "Battlefield-" + chunkX + "-" + chunkZ + "-Mesh"
                    };
                    mesh.vertices = vertices;
                    mesh.normals = normals;
                    mesh.uv = uvs;
                    mesh.triangles = triangles;
                    mesh.RecalculateBounds();
                    _meshes.Add(mesh);
                    TerrainVertexCount += vertices.Length;
                    TerrainTriangleCount += triangles.Length / 3;

                    GameObject chunk = new GameObject(
                        "Terrain-" + chunkX + "-" + chunkZ);
                    chunk.transform.SetParent(root.transform, false);
                    chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                    chunk.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
        }

        private void CreateGroundVariation(string mapId, MapSurface surface, Color groundColor)
        {
            Color hardColor = surface != null
                ? surface.hardColor.ToColor()
                : Color.Lerp(groundColor, Color.gray, 0.2f);
            Color variation = Color.Lerp(groundColor, hardColor, 0.28f);
            System.Random random = new System.Random(StableHash(mapId + "-ground"));
            List<Vector3> vertices = new List<Vector3>(18 * (DiscSegments + 1));
            List<int> triangles = new List<int>(18 * DiscSegments * 3);
            for (int i = 0; i < 18; i++)
            {
                float radius = Mathf.Lerp(22f, 68f, (float)random.NextDouble());
                Vector3 center = Position(random, GroundSurfaceY);
                AddTerrainDisc(
                    vertices,
                    triangles,
                    center.x,
                    center.z,
                    radius,
                    DiscSegments,
                    GroundSurfaceY);
            }
            CreateSurfaceMesh("Surface-GroundVariation", vertices, triangles, variation);
        }

        private void CreateWetGround(MapSurface surface)
        {
            if (surface == null) return;

            MapDisc[] marshes = surface.marshes ?? Array.Empty<MapDisc>();
            MarshCount = marshes.Length;
            if (marshes.Length > 0)
            {
                Color marshColor = Color.Lerp(
                    surface.softColor.ToColor(),
                    surface.waterColor.ToColor(),
                    surface.frozenWater ? 0.38f : 0.16f);
                CreateDiscSurface(
                    "Surface-Marshes",
                    marshes,
                    GroundSurfaceY + 0.012f,
                    marshColor,
                    true);
            }

            MapDisc[] lakes = surface.lakes ?? Array.Empty<MapDisc>();
            LakeCount = lakes.Length;
            if (lakes.Length > 0)
            {
                Color waterColor = surface.waterColor.ToColor();
                if (surface.frozenWater)
                {
                    waterColor = Color.Lerp(waterColor, new Color(0.72f, 0.84f, 0.88f), 0.58f);
                }
                Material material = CreateMaterial(waterColor);
                material.SetFloat("_Metallic", surface.frozenWater ? 0.08f : 0.18f);
                material.SetFloat("_Glossiness", surface.frozenWater ? 0.72f : 0.86f);
                CreateDiscSurface(
                    surface.frozenWater ? "Surface-FrozenWater" : "Surface-Water",
                    lakes,
                    GroundSurfaceY + 0.024f,
                    material,
                    false);
            }
        }

        private void CreateRoads(MapSurface surface)
        {
            if (surface == null) return;
            MapPolyline[] roads = surface.roads ?? Array.Empty<MapPolyline>();
            RoadPolylineCount = roads.Length;
            if (roads.Length == 0) return;

            CreateRoadSurface(
                "Surface-RoadCasing",
                roads,
                11.5f,
                GroundSurfaceY + 0.032f,
                surface.roadCasingColor.ToColor());
            CreateRoadSurface(
                "Surface-Roads",
                roads,
                7.5f,
                GroundSurfaceY + 0.044f,
                surface.roadColor.ToColor());
        }

        private void CreateRoadSurface(
            string name,
            MapPolyline[] roads,
            float width,
            float height,
            Color color)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            float halfWidth = width * 0.5f;
            for (int roadIndex = 0; roadIndex < roads.Length; roadIndex++)
            {
                MapPoint[] points = roads[roadIndex]?.points;
                if (points == null || points.Length < 2) continue;
                int start = vertices.Count;
                for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
                {
                    MapPoint previous = points[Mathf.Max(0, pointIndex - 1)];
                    MapPoint next = points[Mathf.Min(points.Length - 1, pointIndex + 1)];
                    Vector2 tangent = new Vector2(next.x - previous.x, next.z - previous.z);
                    if (tangent.sqrMagnitude < 0.0001f) tangent = Vector2.up;
                    tangent.Normalize();
                    Vector2 perpendicular = new Vector2(-tangent.y, tangent.x) * halfWidth;
                    MapPoint point = points[pointIndex];
                    float leftX = point.x + perpendicular.x;
                    float leftZ = point.z + perpendicular.y;
                    float rightX = point.x - perpendicular.x;
                    float rightZ = point.z - perpendicular.y;
                    vertices.Add(new Vector3(
                        leftX,
                        _heightField.HeightAt(leftX, leftZ) + height,
                        leftZ));
                    vertices.Add(new Vector3(
                        rightX,
                        _heightField.HeightAt(rightX, rightZ) + height,
                        rightZ));
                }
                for (int pointIndex = 0; pointIndex < points.Length - 1; pointIndex++)
                {
                    int current = start + pointIndex * 2;
                    triangles.Add(current);
                    triangles.Add(current + 2);
                    triangles.Add(current + 1);
                    triangles.Add(current + 1);
                    triangles.Add(current + 2);
                    triangles.Add(current + 3);
                }
            }
            CreateSurfaceMesh(name, vertices, triangles, color);
        }

        private void CreateDiscSurface(
            string name,
            MapDisc[] discs,
            float baseHeight,
            Color color,
            bool conformToTerrain)
        {
            CreateDiscSurface(
                name,
                discs,
                baseHeight,
                CreateMaterial(color),
                conformToTerrain);
        }

        private void CreateDiscSurface(
            string name,
            MapDisc[] discs,
            float baseHeight,
            Material material,
            bool conformToTerrain)
        {
            List<Vector3> vertices = new List<Vector3>(discs.Length * (DiscSegments + 1));
            List<int> triangles = new List<int>(discs.Length * DiscSegments * 3);
            for (int i = 0; i < discs.Length; i++)
            {
                MapDisc disc = discs[i];
                float height = baseHeight + (conformToTerrain
                    ? Mathf.Clamp(disc.level, -0.01f, 0.08f)
                    : disc.level);
                if (conformToTerrain)
                {
                    AddTerrainDisc(
                        vertices,
                        triangles,
                        disc.x,
                        disc.z,
                        Mathf.Max(1f, disc.r),
                        DiscSegments,
                        height);
                }
                else
                {
                    AddDisc(
                        vertices,
                        triangles,
                        new Vector3(disc.x, height, disc.z),
                        Mathf.Max(1f, disc.r),
                        DiscSegments);
                }
            }
            CreateSurfaceMesh(name, vertices, triangles, material);
        }

        private void CreateCraters(string mapId, int count, Color groundColor)
        {
            CraterCount = Mathf.Max(0, count);
            if (CraterCount == 0) return;

            System.Random random = new System.Random(StableHash(mapId + "-craters"));
            List<Vector3> vertices = new List<Vector3>(CraterCount * (DiscSegments * 3 + 1));
            List<int> triangles = new List<int>(CraterCount * DiscSegments * 9);
            for (int craterIndex = 0; craterIndex < CraterCount; craterIndex++)
            {
                Vector3 center = Position(random, GroundSurfaceY + 0.01f);
                float radius = Mathf.Lerp(2.4f, 7.2f, (float)random.NextDouble());
                int start = vertices.Count;
                vertices.Add(center);
                for (int ring = 0; ring < 3; ring++)
                {
                    float ringRadius = radius * (0.42f + ring * 0.29f);
                    float ringHeight = ring == 1 ? 0.13f : 0.015f;
                    for (int segment = 0; segment < DiscSegments; segment++)
                    {
                        float angle = segment * Mathf.PI * 2f / DiscSegments;
                        float x = center.x + Mathf.Cos(angle) * ringRadius;
                        float z = center.z + Mathf.Sin(angle) * ringRadius;
                        vertices.Add(new Vector3(
                            x,
                            _heightField.HeightAt(x, z) +
                                GroundSurfaceY + 0.01f + ringHeight,
                            z));
                    }
                }
                for (int segment = 0; segment < DiscSegments; segment++)
                {
                    int next = (segment + 1) % DiscSegments;
                    triangles.Add(start);
                    triangles.Add(start + 1 + next);
                    triangles.Add(start + 1 + segment);
                    ConnectRings(triangles, start + 1, start + 1 + DiscSegments, segment, next);
                    ConnectRings(
                        triangles,
                        start + 1 + DiscSegments,
                        start + 1 + DiscSegments * 2,
                        segment,
                        next);
                }
            }
            Color craterColor = Color.Lerp(groundColor, new Color(0.12f, 0.1f, 0.08f), 0.58f);
            CreateSurfaceMesh("Surface-Craters", vertices, triangles, craterColor);
        }

        private static void ConnectRings(
            List<int> triangles,
            int innerStart,
            int outerStart,
            int segment,
            int next)
        {
            int inner = innerStart + segment;
            int innerNext = innerStart + next;
            int outer = outerStart + segment;
            int outerNext = outerStart + next;
            triangles.Add(inner);
            triangles.Add(outerNext);
            triangles.Add(outer);
            triangles.Add(inner);
            triangles.Add(innerNext);
            triangles.Add(outerNext);
        }

        private static void AddDisc(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 center,
            float radius,
            int segments)
        {
            int start = vertices.Count;
            vertices.Add(center);
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment * Mathf.PI * 2f / segments;
                vertices.Add(center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius));
            }
            for (int segment = 0; segment < segments; segment++)
            {
                triangles.Add(start);
                triangles.Add(start + 1 + (segment + 1) % segments);
                triangles.Add(start + 1 + segment);
            }
        }

        private void AddTerrainDisc(
            List<Vector3> vertices,
            List<int> triangles,
            float centerX,
            float centerZ,
            float radius,
            int segments,
            float heightOffset)
        {
            int start = vertices.Count;
            vertices.Add(new Vector3(
                centerX,
                _heightField.HeightAt(centerX, centerZ) + heightOffset,
                centerZ));
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment * Mathf.PI * 2f / segments;
                float x = centerX + Mathf.Cos(angle) * radius;
                float z = centerZ + Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(
                    x,
                    _heightField.HeightAt(x, z) + heightOffset,
                    z));
            }
            for (int segment = 0; segment < segments; segment++)
            {
                triangles.Add(start);
                triangles.Add(start + 1 + (segment + 1) % segments);
                triangles.Add(start + 1 + segment);
            }
        }

        private GameObject CreateSurfaceMesh(
            string name,
            List<Vector3> vertices,
            List<int> triangles,
            Color color)
        {
            return CreateSurfaceMesh(name, vertices, triangles, CreateMaterial(color));
        }

        private GameObject CreateSurfaceMesh(
            string name,
            List<Vector3> vertices,
            List<int> triangles,
            Material material)
        {
            if (vertices.Count == 0 || triangles.Count == 0) return null;
            Mesh mesh = new Mesh { name = name + "-Mesh" };
            if (vertices.Count > ushort.MaxValue) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshes.Add(mesh);

            GameObject result = new GameObject(name);
            result.transform.SetParent(_root.transform, false);
            result.AddComponent<MeshFilter>().sharedMesh = mesh;
            result.AddComponent<MeshRenderer>().sharedMaterial = material;
            return result;
        }

        private Material CreateMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Standard")) { color = color };
            material.SetFloat("_Glossiness", 0.08f);
            _materials.Add(material);
            return material;
        }

        private void DeterministicScatter(
            string mapId, int rockCount, Color groundColor)
        {
            System.Random random = new System.Random(StableHash(mapId));
            for (int i = 0; i < rockCount; i++)
            {
                float scale = Mathf.Lerp(1.2f, 4.5f, (float)random.NextDouble());
                CreatePrimitive("Rock", PrimitiveType.Sphere,
                    Position(random, scale * 0.35f), new Vector3(scale, scale * 0.7f, scale),
                    Color.Lerp(groundColor, new Color(0.3f, 0.3f, 0.28f), 0.65f));
            }
        }

        private GameObject CreatePrimitive(
            string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color,
            Transform parent = null)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.SetParent(parent ?? _root.transform, true);
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color);
            return result;
        }

        private Vector3 Position(System.Random random, float heightOffset)
        {
            float x = Mathf.Lerp(-430f, 430f, (float)random.NextDouble());
            float z = Mathf.Lerp(-430f, 430f, (float)random.NextDouble());
            return new Vector3(
                x,
                _heightField.HeightAt(x, z) + heightOffset,
                z);
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

        private static Color HexColor(int value)
        {
            return new Color(
                ((value >> 16) & 255) / 255f,
                ((value >> 8) & 255) / 255f,
                (value & 255) / 255f);
        }

        private static Color GroundColor(string id)
        {
            if (id == "desert" || id == "badlands") return new Color(0.48f, 0.39f, 0.25f);
            if (id == "winter" || id == "alpine") return new Color(0.66f, 0.72f, 0.72f);
            if (id == "caldera" || id == "blackglass") return new Color(0.16f, 0.16f, 0.15f);
            if (id == "urban" || id == "foundry") return new Color(0.28f, 0.29f, 0.27f);
            return new Color(0.28f, 0.34f, 0.22f);
        }
    }
}
