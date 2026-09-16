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
        private const int HorizonSegments = 520;
        private const int CloudDomeSegments = 96;
        private const int CloudDomeRings = 18;
        private const int TerrainChunksPerAxis = 4;
        private const int TerrainQuadsPerChunk = 32;
        private const float TerrainHalfExtentM = 500f;
        private const float HorizonRadiusM = 980f;
        private const float HorizonRimHalfExtentM = 512f;
        private const float CloudDomeRadiusM = 1320f;
        private const float CumulusDefaultAltitudeM = 620f;
        private const float OvercastCloudAltitudeM = 340f;
        private const float CumulusDefaultUvM = 3200f;
        private const float OvercastCloudUvM = 2400f;
        private const float CumulusDefaultHazeK = 0.00023f;
        private const float OvercastCloudHazeK = 0.00015f;
        private const float FogExtinctionShare = 0.55f;

        private readonly GameObject _root;
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<MapChunkView> _terrainChunks =
            new List<MapChunkView>();
        private IHeightField _heightField;
        private MapStructureRuntime _structures;
        private MapVegetationRuntime _vegetation;
        private Material _skyboxMaterial;

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
        public int ActiveTerrainChunkCount { get; private set; }
        public int HorizonMeshCount { get; private set; }
        public int HorizonVertexCount { get; private set; }
        public int CloudDeckCount { get; private set; }
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
            UpdateWorldStreaming(cameraPosition, visibleDistanceM, visibleDistanceM);
        }

        public void UpdateWorldStreaming(
            Vector3 cameraPosition,
            float terrainVisibleDistanceM = MapVegetationRuntime.DefaultVisibleDistanceM,
            float vegetationVisibleDistanceM = MapVegetationRuntime.DefaultVisibleDistanceM)
        {
            _vegetation?.UpdateVisibility(cameraPosition, vegetationVisibleDistanceM);
            UpdateTerrainVisibility(cameraPosition, terrainVisibleDistanceM);
        }

        public void Dispose()
        {
            _structures?.Dispose();
            _vegetation?.Dispose();
            if (RenderSettings.skybox == _skyboxMaterial)
                RenderSettings.skybox = null;
            DestroyObject(_skyboxMaterial);
            for (int i = 0; i < _materials.Count; i++)
                MapMaterialFactory.Destroy(_materials[i]);
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
            MapSurface surface = map.unitySurface;
            Color groundColor = surface != null
                ? surface.groundColor.ToColor()
                : GroundColor(map.id);
            ConfigureEnvironment(map, groundColor);
            CreateHorizon(map, groundColor);
            CreateCloudDecks(map);
            CreateTerrain(groundColor, map.splat);
            CreateGroundVariation(map.id, surface, groundColor, map.splat);
            CreateWetGround(surface, map.splat);
            CreateRoads(surface, map.splat);
            CreateCraters(map.id, map.props?.craters ?? 0, groundColor);
            _structures = MapStructureRuntime.Create(_root.transform, map);
            _vegetation = MapVegetationRuntime.Create(
                _root.transform,
                map,
                _heightField);

            int rocks = Mathf.Clamp((map.props?.rocks ?? 0) / 18, 3, 18);
            DeterministicScatter(map.id, rocks, groundColor);
        }

        private void ConfigureEnvironment(MapDefinition map, Color groundColor)
        {
            Color sunColor = HexColor(
                map.sky != null && map.sky.sunColorHex != 0
                    ? map.sky.sunColorHex
                    : 0xfff2cc);
            Color fog = FogColor(map.sky, groundColor, sunColor);
            float envIntensity = PositiveOrDefault(map.sky?.envIntensity ?? 0f, 0.2f);
            float hemiIntensity = PositiveOrDefault(map.sky?.hemiIntensity ?? 0f, 0.32f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = FogDensity(map.sky);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(
                    Color.Lerp(groundColor, fog, 0.58f),
                    sunColor,
                    Mathf.Clamp01(0.08f + envIntensity * 0.18f))
                * Mathf.Lerp(
                    0.72f,
                    1.28f,
                    Mathf.InverseLerp(0.18f, 0.9f, hemiIntensity + envIntensity * 0.35f));
            RenderSettings.reflectionIntensity = Mathf.Clamp(envIntensity * 2.8f, 0.18f, 1.2f);

            _skyboxMaterial = CreateSkyboxMaterial(fog, groundColor, sunColor, map.sky);
            if (_skyboxMaterial != null)
                RenderSettings.skybox = _skyboxMaterial;

            GameObject sun = new GameObject("Sun");
            sun.transform.SetParent(_root.transform, false);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = sunColor;
            light.intensity = SunIntensity(map.sky);
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(
                map.sky?.sunElevationDeg ?? 48f, -(map.sky?.sunAzimuthDeg ?? 32f), 0f);
            RenderSettings.sun = light;
        }

        private static float SunIntensity(MapSky sky)
        {
            float authored = PositiveOrDefault(sky?.sunIntensity ?? 0f, 1.0f);
            float postExposure = PositiveOrDefault(sky?.postExposure ?? 0f, 1.0f);
            return Mathf.Clamp(
                authored * 0.72f * Mathf.Clamp(postExposure, 0.82f, 1.04f),
                0.35f,
                3.4f);
        }

        private static Color FogColor(MapSky sky, Color groundColor, Color sunColor)
        {
            Color authored = HexColor(
                sky != null && sky.fogTintHex != 0 ? sky.fogTintHex : 0x8799a0);
            Color atmosphericBlue = HexColor(0x7e97b8);
            Color sampledHorizon = Color.Lerp(
                Color.Lerp(groundColor, atmosphericBlue, 0.62f),
                sunColor,
                0.08f);
            float fogMix = Mathf.Clamp01(PositiveOrDefault(sky?.fogMix ?? 0f, 0.55f));
            return Color.Lerp(sampledHorizon, authored, fogMix);
        }

        private static float FogDensity(MapSky sky)
        {
            float density = sky != null && sky.fogDensity > 0f
                ? sky.fogDensity
                : 0.00062f;
            return Mathf.Clamp(density * FogExtinctionShare, 0.00012f, 0.00058f);
        }

        private static Color AuthoredFogTint(MapSky sky)
        {
            return HexColor(
                sky != null && sky.fogTintHex != 0 ? sky.fogTintHex : 0x8799a0);
        }

        private static Material CreateSkyboxMaterial(
            Color fog,
            Color groundColor,
            Color sunColor,
            MapSky sky)
        {
            Shader shader = Shader.Find("Skybox/Procedural");
            if (shader == null) return null;
            Material material = new Material(shader)
            {
                name = "MapProceduralSkybox"
            };
            float rayleigh = PositiveOrDefault(sky?.rayleigh ?? 0f, 1.2f);
            float turbidity = PositiveOrDefault(sky?.turbidity ?? 0f, 4f);
            float density = sky != null ? sky.fogDensity : 0.00062f;
            float mie = PositiveOrDefault(sky?.mieCoefficient ?? 0f, 0.006f);
            float mieG = PositiveOrDefault(sky?.mieDirectionalG ?? 0f, 0.82f);
            float postExposure = sky != null && sky.postExposure > 0f
                ? sky.postExposure
                : 1f;
            float env = PositiveOrDefault(sky?.envIntensity ?? 0f, 0.2f);
            if (material.HasProperty("_SkyTint"))
            {
                Color clearSky = Color.Lerp(
                    HexColor(0x4f82ba),
                    HexColor(0x84b9df),
                    Mathf.Clamp01((rayleigh - 0.55f) * 0.48f));
                Color aerialSky = Color.Lerp(
                    clearSky,
                    fog,
                    Mathf.Clamp01((turbidity - 4f) * 0.055f + density * 110f));
                float sunWarmth = Mathf.Clamp01(mie * 22f + (mieG - 0.78f) * 0.45f);
                material.SetColor(
                    "_SkyTint",
                    Color.Lerp(aerialSky, sunColor, 0.04f + sunWarmth * 0.18f));
            }
            if (material.HasProperty("_GroundColor"))
            {
                Color authoredFog = AuthoredFogTint(sky);
                material.SetColor(
                    "_GroundColor",
                    Color.Lerp(
                        Color.Lerp(groundColor, fog, 0.62f),
                        authoredFog,
                        0.48f));
            }
            if (material.HasProperty("_Exposure"))
            {
                material.SetFloat(
                    "_Exposure",
                    Mathf.Clamp(0.96f * postExposure + env * 0.16f, 0.78f, 1.16f));
            }
            if (material.HasProperty("_AtmosphereThickness"))
            {
                material.SetFloat(
                    "_AtmosphereThickness",
                    Mathf.Clamp(
                        Mathf.Lerp(0.34f, 0.92f, Mathf.InverseLerp(0.0003f, 0.001f, density)) +
                        (turbidity - 4f) * 0.055f +
                        (rayleigh - 1.2f) * 0.075f,
                        0.24f,
                        1.22f));
            }
            if (material.HasProperty("_SunDisk"))
                material.SetFloat("_SunDisk", 2f);
            if (material.HasProperty("_SunSize"))
            {
                material.SetFloat(
                    "_SunSize",
                    Mathf.Clamp(0.018f + mie * 4.2f + (mieG - 0.78f) * 0.045f, 0.018f, 0.075f));
            }
            if (material.HasProperty("_SunSizeConvergence"))
            {
                material.SetFloat(
                    "_SunSizeConvergence",
                    Mathf.Clamp(2.4f + (mieG - 0.78f) * 8.0f + mie * 90f, 2.0f, 8.0f));
            }
            return material;
        }

        private void CreateHorizon(MapDefinition map, Color groundColor)
        {
            MapHorizon horizon = map.horizon;
            if (horizon == null) return;

            GameObject root = new GameObject("Horizon");
            root.transform.SetParent(_root.transform, false);

            Color fog = RenderSettings.fogColor;
            Color horizonFog = AuthoredFogTint(map.sky);
            Color baseColor = horizon.baseHex != 0
                ? HexColor(horizon.baseHex)
                : Color.Lerp(groundColor, fog, 0.45f);
            Color rockColor = horizon.rockHex != 0
                ? HexColor(horizon.rockHex)
                : baseColor;
            float haze = PositiveOrDefault(horizon.haze, 1f);
            CreateHorizonRidge(
                root.transform,
                map.id,
                horizon,
                baseColor,
                rockColor,
                horizonFog);

            if (horizon.treeline > 0f || horizon.forestHex != 0)
            {
                int layers = Mathf.Clamp(horizon.treelineLayers > 0
                    ? horizon.treelineLayers
                    : 1, 1, 3);
                Color forest = horizon.forestHex != 0
                    ? HexColor(horizon.forestHex)
                    : Color.Lerp(baseColor, new Color(0.12f, 0.22f, 0.12f), 0.62f);
                for (int layer = 0; layer < layers; layer++)
                {
                    CreateHorizonBand(
                        root.transform,
                        layer == 0 ? "Horizon-Treeline" : "Horizon-Treeline-" + layer,
                        map.id,
                        horizon,
                        0.08f,
                        Mathf.Clamp(horizon.treeline > 0f ? horizon.treeline : 0.78f, 0.22f, 0.96f),
                        HorizonRadiusM - 8f - layer * 6f,
                        -16f + layer * 2f,
                        Color.Lerp(forest, horizonFog, Mathf.Clamp01(0.18f + haze * 0.16f + layer * 0.08f)),
                        "treeline-" + layer);
                }
            }

            if (horizon.snowline > 0f || horizon.snowHex != 0)
            {
                Color snow = horizon.snowHex != 0
                    ? HexColor(horizon.snowHex)
                    : new Color(0.86f, 0.9f, 0.92f);
                CreateHorizonBand(
                    root.transform,
                    "Horizon-Snowcap",
                    map.id,
                    horizon,
                    Mathf.Clamp(horizon.snowline > 0f ? horizon.snowline : 0.62f, 0.12f, 0.92f),
                    1f,
                    HorizonRadiusM - 4f,
                    -16f,
                    Color.Lerp(snow, horizonFog, Mathf.Clamp01(0.1f + haze * 0.12f)),
                    "snow");
            }
        }

        private void CreateHorizonBand(
            Transform parent,
            string name,
            string mapId,
            MapHorizon horizon,
            float bottomFraction,
            float topFraction,
            float radius,
            float baseY,
            Color color,
            string seed)
        {
            int vertexCount = (HorizonSegments + 1) * 2;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[HorizonSegments * 12];
            int hash = StableHash(mapId + ":" + seed);
            float haze = PositiveOrDefault(horizon.haze, 1f);
            for (int i = 0; i <= HorizonSegments; i++)
            {
                float u = i / (float)HorizonSegments;
                float angle = u * Mathf.PI * 2f;
                float sampleAngle = i == HorizonSegments ? 0f : angle;
                float height = HorizonHeight(sampleAngle, horizon, hash);
                float bottomY = Mathf.Lerp(baseY, height, bottomFraction);
                float topY = Mathf.Lerp(baseY, height, topFraction);
                float rim = SquareRimRadius(angle);
                float effectiveRadius = Mathf.Max(radius, rim + 86f);
                float radialNoise = 1f + (Wave01(sampleAngle, 4.7f, hash, 109) - 0.5f) * 0.025f;
                float x = Mathf.Cos(angle) * effectiveRadius * radialNoise;
                float z = Mathf.Sin(angle) * effectiveRadius * radialNoise;
                int index = i * 2;
                vertices[index] = new Vector3(x, bottomY, z);
                vertices[index + 1] = new Vector3(x, topY, z);
                uvs[index] = new Vector2(u, bottomFraction);
                uvs[index + 1] = new Vector2(u, topFraction);
                Color bottomColor = Color.Lerp(color, RenderSettings.fogColor, Mathf.Clamp01(0.18f + haze * 0.18f));
                Color topColor = Color.Lerp(color, RenderSettings.fogColor, Mathf.Clamp01(0.08f + haze * 0.1f));
                colors[index] = bottomColor;
                colors[index + 1] = topColor;
            }
            int triangle = 0;
            for (int i = 0; i < HorizonSegments; i++)
            {
                int a = i * 2;
                int b = a + 1;
                int c = a + 2;
                int d = a + 3;
                triangles[triangle++] = a;
                triangles[triangle++] = c;
                triangles[triangle++] = b;
                triangles[triangle++] = b;
                triangles[triangle++] = c;
                triangles[triangle++] = d;
                triangles[triangle++] = a;
                triangles[triangle++] = b;
                triangles[triangle++] = c;
                triangles[triangle++] = b;
                triangles[triangle++] = d;
                triangles[triangle++] = c;
            }

            Mesh mesh = new Mesh { name = name + "-Mesh" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshes.Add(mesh);
            HorizonMeshCount++;
            HorizonVertexCount += vertices.Length;

            GameObject band = new GameObject(name);
            band.transform.SetParent(parent, false);
            band.AddComponent<MeshFilter>().sharedMesh = mesh;
            band.AddComponent<MeshRenderer>().sharedMaterial = CreateHorizonMaterial(
                color,
                mapId + "-" + seed,
                horizon,
                false,
                260f);
        }

        private void CreateHorizonRidge(
            Transform parent,
            string mapId,
            MapHorizon horizon,
            Color baseColor,
            Color rockColor,
            Color fog)
        {
            HorizonRow[] rows = HorizonRows(horizon.style ?? "rolling");
            int rowCount = rows.Length;
            int vertexCount = (HorizonSegments + 1) * rowCount;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[HorizonSegments * (rowCount - 1) * 6];
            int hash = StableHash(mapId + ":ridge");
            float amp = PositiveOrDefault(horizon.amp, 1f);
            float haze = PositiveOrDefault(horizon.haze, 1f);
            float grain = PositiveOrDefault(horizon.grain, 1f);
            Color forest = horizon.forestHex != 0
                ? HexColor(horizon.forestHex)
                : Color.Lerp(baseColor, new Color(0.16f, 0.28f, 0.13f), 0.58f);
            Color snow = horizon.snowHex != 0
                ? HexColor(horizon.snowHex)
                : new Color(0.86f, 0.9f, 0.93f);
            float snowline = horizon.snowline > 0f
                ? horizon.snowline
                : (horizon.style == "alpine" ? 0.42f : 2f);
            float treeline = horizon.treeline > 0f
                ? horizon.treeline
                : (horizon.style == "rolling" ? 0.9f :
                    horizon.style == "escarpment" ? 0.88f : 0f);
            float maxHeight = 1f;
            float[] heights = new float[vertexCount];
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                HorizonRow row = rows[rowIndex];
                for (int i = 0; i <= HorizonSegments; i++)
                {
                    float u = i / (float)HorizonSegments;
                    float angle = u * Mathf.PI * 2f;
                    float sampleAngle = i == HorizonSegments ? 0f : angle;
                    float rim = SquareRimRadius(sampleAngle);
                    float rowMargin = HorizonRowMargin(rowIndex, rowCount);
                    float effectiveRadius = Mathf.Max(
                        row.Radius,
                        rim + rowMargin);
                    float radialNoise = 1f +
                        (Wave01(sampleAngle, 4.1f, hash, 83 + rowIndex) - 0.5f) * 0.03f;
                    float height = row.Skirt
                        ? (row.Base + Wave01(sampleAngle, row.F0, hash, 17 + rowIndex) * row.Amp)
                        : HorizonHeight(
                            sampleAngle,
                            horizon,
                            hash + rowIndex * 53,
                            row.Base,
                            row.Amp,
                            row.F0,
                            row.F1);
                    height *= row.Skirt ? 1f : amp;
                    int index = rowIndex * (HorizonSegments + 1) + i;
                    vertices[index] = new Vector3(
                        Mathf.Cos(angle) * effectiveRadius * radialNoise,
                        height,
                        Mathf.Sin(angle) * effectiveRadius * radialNoise);
                    heights[index] = height;
                    if (!row.Skirt && height > maxHeight) maxHeight = height;
                }
            }

            float[] shadedHeights = SmoothedHorizonHeights(heights, rowCount);
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                HorizonRow row = rows[rowIndex];
                for (int i = 0; i <= HorizonSegments; i++)
                {
                    int index = rowIndex * (HorizonSegments + 1) + i;
                    float u = i / (float)HorizonSegments;
                    float angle = (i / (float)HorizonSegments) * Mathf.PI * 2f;
                    float height01 = Mathf.Clamp01(heights[index] / maxHeight);
                    uvs[index] = new Vector2(u * 10f, height01);
                    int prev = rowIndex * (HorizonSegments + 1) +
                        (i == 0 ? HorizonSegments - 1 : i - 1);
                    int next = rowIndex * (HorizonSegments + 1) +
                        (i == HorizonSegments ? 1 : i + 1);
                    float tangentSlope =
                        Mathf.Abs(shadedHeights[next] - shadedHeights[prev]) *
                        0.010f;
                    float radialSlope = 0f;
                    if (rowIndex > 0 && rowIndex < rowCount - 1)
                    {
                        int inner = (rowIndex - 1) * (HorizonSegments + 1) + i;
                        int outer = (rowIndex + 1) * (HorizonSegments + 1) + i;
                        float radialSpan =
                            rows[rowIndex + 1].Radius - rows[rowIndex - 1].Radius;
                        radialSlope = Mathf.Abs(
                            shadedHeights[outer] - shadedHeights[inner]) /
                            Mathf.Max(1f, radialSpan) *
                            2.2f;
                    }
                    float slope = Mathf.Clamp01(
                        Mathf.Sqrt(tangentSlope * tangentSlope +
                            radialSlope * radialSlope) *
                        1.6f);
                    float noise = Wave01(angle, 11.3f, hash, 131 + rowIndex);
                    Color color = baseColor * Mathf.Lerp(0.82f, 1.16f, height01);
                    float rockWeight = Mathf.SmoothStep(0.34f, 0.8f, slope) *
                        (row.Skirt ? 0.22f : StyleRockWeight(horizon.style));
                    color = Color.Lerp(color, rockColor, rockWeight);
                    if (treeline > 0f && height01 <= treeline)
                    {
                        float forestWeight = Mathf.Clamp01((treeline - height01) / Mathf.Max(treeline, 0.01f));
                        color = Color.Lerp(color, forest, forestWeight * 0.52f);
                    }
                    if (snowline <= 1f && height01 >= snowline)
                    {
                        float snowWeight = Mathf.SmoothStep(
                            snowline,
                            Mathf.Min(1f, snowline + 0.18f),
                            height01) *
                            (1f - slope * 0.45f);
                        color = Color.Lerp(color, snow, snowWeight);
                    }
                    float sun = 0.82f + Mathf.Sin(
                        angle + (horizon.banding + 0.35f) * Mathf.PI) * 0.1f;
                    float strata = 1f;
                    if (horizon.style == "mesa")
                    {
                        float banding = Mathf.Clamp01(
                            PositiveOrDefault(horizon.banding, 0.16f));
                        strata += Mathf.Sin(heights[index] * 0.34f + noise * 2f) *
                            banding * 0.28f;
                    }
                    float grainValue = Mathf.Lerp(
                        0.88f,
                        1.12f,
                        noise) *
                        Mathf.Lerp(1f, 1.08f, grain - 1f);
                    color *= sun * strata * grainValue;
                    float mesaNearDust = 0f;
                    if (horizon.style == "mesa" && !row.Skirt)
                    {
                        float rowDepth =
                            rowIndex / Mathf.Max(1f, rowCount - 1f);
                        float lowerWall =
                            1f - Mathf.SmoothStep(0.34f, 0.72f, height01);
                        mesaNearDust = lowerWall * (1f - rowDepth);
                        Color dustColor = Color.Lerp(baseColor, fog, 0.54f);
                        dustColor *= Mathf.Lerp(1.08f, 0.96f, height01);
                        color = Color.Lerp(
                            color,
                            dustColor,
                            Mathf.Clamp01(lowerWall * 0.56f + mesaNearDust * 0.18f));
                    }
                    float hazeWeight = row.Aerial * haze;
                    if (!row.Skirt)
                    {
                        hazeWeight += (1f - height01) * 0.1f;
                        if (horizon.style == "mesa")
                        {
                            float depth = rowIndex / Mathf.Max(1f, rowCount - 1f);
                            hazeWeight +=
                                0.34f +
                                depth * 0.22f +
                                (1f - height01) * 0.18f;
                            hazeWeight += mesaNearDust * 0.22f;
                        }
                    }
                    hazeWeight = Mathf.Clamp01(hazeWeight);
                    color = Color.Lerp(color, fog, hazeWeight);
                    color.a = 1f;
                    colors[index] = color;
                }
            }

            int triangle = 0;
            int stride = HorizonSegments + 1;
            for (int rowIndex = 0; rowIndex < rowCount - 1; rowIndex++)
            {
                for (int i = 0; i < HorizonSegments; i++)
                {
                    int a = rowIndex * stride + i;
                    int b = a + 1;
                    int c = (rowIndex + 1) * stride + i;
                    int d = c + 1;
                    triangles[triangle++] = a;
                    triangles[triangle++] = b;
                    triangles[triangle++] = c;
                    triangles[triangle++] = b;
                    triangles[triangle++] = d;
                    triangles[triangle++] = c;
                }
            }

            Mesh mesh = new Mesh { name = "Horizon-Ridge-Mesh" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshes.Add(mesh);
            HorizonMeshCount++;
            HorizonVertexCount += vertices.Length;

            GameObject ridge = new GameObject("Horizon-Ridge");
            ridge.transform.SetParent(parent, false);
            ridge.AddComponent<MeshFilter>().sharedMesh = mesh;
            ridge.AddComponent<MeshRenderer>().sharedMaterial = CreateHorizonMaterial(
                Color.white,
                mapId + "-ridge",
                horizon,
                true,
                maxHeight);
        }

        private static float[] SmoothedHorizonHeights(float[] heights, int rowCount)
        {
            float[] result = new float[heights.Length];
            Array.Copy(heights, result, heights.Length);
            float[] scratch = new float[HorizonSegments + 1];
            for (int pass = 0; pass < 3; pass++)
            {
                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    int rowBase = rowIndex * (HorizonSegments + 1);
                    for (int i = 0; i <= HorizonSegments; i++)
                    {
                        int prev = i == 0 ? HorizonSegments - 1 : i - 1;
                        int next = i == HorizonSegments ? 1 : i + 1;
                        scratch[i] =
                            result[rowBase + prev] * 0.27f +
                            result[rowBase + i] * 0.46f +
                            result[rowBase + next] * 0.27f;
                    }
                    for (int i = 0; i <= HorizonSegments; i++)
                        result[rowBase + i] = scratch[i];
                }
            }
            return result;
        }

        private static float HorizonHeight(
            float angle,
            MapHorizon horizon,
            int seed)
        {
            return HorizonHeight(angle, horizon, seed, 0f, 1f, 1.3f, 2.7f);
        }

        private static float HorizonHeight(
            float angle,
            MapHorizon horizon,
            int seed,
            float baseHeight,
            float heightRange,
            float frequency0,
            float frequency1)
        {
            string style = horizon.style ?? "rolling";
            float n0 = Wave01(angle, frequency0, seed, 17);
            float n1 = Wave01(angle, frequency1, seed, 41);
            float n2 = Wave01(angle, 6.1f, seed, 73);
            float grain = PositiveOrDefault(horizon.grain, 1f);
            if (style == "alpine")
            {
                float massif = Mathf.SmoothStep(0.12f, 0.92f, n0) * 0.66f +
                    Mathf.SmoothStep(0.18f, 0.88f, n1) * 0.26f +
                    (n2 - 0.5f) * 0.08f * grain;
                return baseHeight + Mathf.Clamp01(massif) * heightRange;
            }
            if (style == "mesa")
            {
                float warp =
                    NoiseSigned(
                        Mathf.Cos(angle) * 1.15f + 55f,
                        Mathf.Sin(angle) * 1.15f - 41f,
                        seed,
                        53) * 0.13f;
                float warpedAngle = angle + warp;
                float mx = Mathf.Cos(warpedAngle);
                float mz = Mathf.Sin(warpedAngle);
                n0 = Noise01(
                    mx * frequency0 + 41f,
                    mz * frequency0 - 27f,
                    seed,
                    17);
                n0 = Contrast01(n0, 1.72f);
                float butteField = Noise01(
                    Mathf.Cos(angle) * frequency1 * 2.3f - 13f,
                    Mathf.Sin(angle) * frequency1 * 2.3f + 33f,
                    seed,
                    41);
                butteField = Contrast01(butteField, 1.55f);
                float cren =
                    NoiseSigned(
                        Mathf.Cos(angle) * frequency0 * 4.6f - 71f,
                        Mathf.Sin(angle) * frequency0 * 4.6f + 15f,
                        seed,
                        91) * 0.042f +
                    NoiseSigned(
                        Mathf.Cos(angle) * frequency0 * 9.7f + 133f,
                        Mathf.Sin(angle) * frequency0 * 9.7f - 55f,
                        seed,
                        137) * 0.018f;
                float terraceField = n0 + cren;
                float table1 = Mathf.SmoothStep(0.36f, 0.45f, terraceField);
                float table2 = Mathf.SmoothStep(0.62f, 0.7f, terraceField);
                float butte =
                    Mathf.SmoothStep(0.8f, 0.86f, butteField) *
                    (1f - table2);
                float pedestal =
                    Mathf.SmoothStep(0.14f, 0.52f, terraceField) * 0.17f;
                float embayment = Mathf.SmoothStep(
                    0.28f,
                    0.74f,
                    Noise01(
                        Mathf.Cos(angle) * frequency0 * 0.62f + 5.7f,
                        Mathf.Sin(angle) * frequency0 * 0.62f - 9.2f,
                        seed,
                        223));
                float capWobble =
                    1f +
                    NoiseSigned(
                        Mathf.Cos(angle) * 9f + 3f,
                        Mathf.Sin(angle) * 9f - 8f,
                        seed,
                        173) * 0.05f +
                    NoiseSigned(
                        Mathf.Cos(angle) * 23f - 17f,
                        Mathf.Sin(angle) * 23f + 41f,
                        seed,
                        211) * 0.028f;
                float profile =
                    pedestal + table1 * 0.45f + table2 * 0.34f + butte * 0.3f;
                profile *= Mathf.Lerp(0.30f, 1.10f, embayment);
                return baseHeight +
                    profile * heightRange * capWobble;
            }
            if (style == "escarpment")
            {
                float bench = Mathf.SmoothStep(0.3f, 0.62f, n0);
                return baseHeight + (bench * 0.62f + Mathf.Pow(n1, 2.2f) * 0.38f) * heightRange;
            }
            float billow = Mathf.Pow(n0, 1.4f) * 0.74f + n1 * 0.26f;
            return baseHeight + billow * heightRange;
        }

        private static HorizonRow[] HorizonRows(string style)
        {
            if (style == "alpine")
            {
                return new[]
                {
                    new HorizonRow(428f, -22f, 0f, 6f, 11f, 0.1f, true),
                    new HorizonRow(470f, 26f, 14f, 6f, 11f, 0.1f, true),
                    new HorizonRow(585f, 50f, 52f, 3.1f, 6.2f, 0.1f),
                    new HorizonRow(650f, 52f, 64f, 2.8f, 5.7f, 0.14f),
                    new HorizonRow(720f, 56f, 76f, 2.6f, 5.2f, 0.18f),
                    new HorizonRow(870f, 66f, 102f, 1.9f, 4f, 0.3f),
                    new HorizonRow(1040f, 82f, 128f, 1.5f, 3.3f, 0.44f),
                    new HorizonRow(1240f, 88f, 100f, 1.1f, 2.4f, 0.6f)
                };
            }
            if (style == "rolling")
            {
                return new[]
                {
                    new HorizonRow(428f, -22f, 0f, 6f, 11f, 0.1f, true),
                    new HorizonRow(470f, 22f, 12f, 6f, 11f, 0.1f, true),
                    new HorizonRow(600f, 32f, 38f, 3f, 6.4f, 0.12f),
                    new HorizonRow(800f, 45f, 72f, 2f, 4.4f, 0.32f),
                    new HorizonRow(1050f, 60f, 112f, 1.4f, 3.1f, 0.54f),
                    new HorizonRow(1240f, 72f, 120f, 1f, 2.2f, 0.68f)
                };
            }
            if (style == "escarpment")
            {
                return new[]
                {
                    new HorizonRow(428f, -22f, 0f, 6f, 11f, 0.1f, true),
                    new HorizonRow(470f, 24f, 12f, 6f, 11f, 0.1f, true),
                    new HorizonRow(600f, 36f, 44f, 2.8f, 6f, 0.14f),
                    new HorizonRow(800f, 50f, 82f, 2f, 4.4f, 0.34f),
                    new HorizonRow(1050f, 66f, 116f, 1.4f, 3.1f, 0.54f),
                    new HorizonRow(1240f, 76f, 106f, 1f, 2.2f, 0.7f)
                };
            }
            if (style == "mesa")
            {
                return new[]
                {
                    new HorizonRow(428f, -22f, 0f, 6f, 11f, 0.1f, true),
                    new HorizonRow(470f, 26f, 14f, 6f, 11f, 0.1f, true),
                    new HorizonRow(585f, 50f, 52f, 3.1f, 6.2f, 0.12f),
                    new HorizonRow(760f, 62f, 96f, 2.1f, 4.6f, 0.24f),
                    new HorizonRow(990f, 84f, 128f, 1.5f, 3.3f, 0.42f),
                    new HorizonRow(1240f, 88f, 96f, 1.1f, 2.4f, 0.6f)
                };
            }
            return new[]
            {
                new HorizonRow(428f, -22f, 0f, 6f, 11f, 0.1f, true),
                new HorizonRow(470f, 26f, 14f, 6f, 11f, 0.1f, true),
                new HorizonRow(585f, 50f, 52f, 3.1f, 6.2f, 0.12f),
                new HorizonRow(760f, 62f, 96f, 2.1f, 4.6f, 0.24f),
                new HorizonRow(990f, 84f, 128f, 1.5f, 3.3f, 0.42f),
                new HorizonRow(1240f, 88f, 96f, 1.1f, 2.4f, 0.6f)
            };
        }

        private static float HorizonRowMargin(int rowIndex, int rowCount)
        {
            if (rowCount >= 8)
            {
                float[] alpineMargins =
                {
                    -34f, 22f, 95f, 150f, 200f, 340f, 540f, 800f
                };
                return alpineMargins[
                    Mathf.Clamp(rowIndex, 0, alpineMargins.Length - 1)];
            }
            float[] margins =
            {
                -34f, 22f, 95f, 280f, 520f, 800f
            };
            return margins[
                Mathf.Clamp(rowIndex, 0, margins.Length - 1)];
        }

        private static float SquareRimRadius(float angle)
        {
            return HorizonRimHalfExtentM /
                Mathf.Max(
                    0.001f,
                    Mathf.Max(
                        Mathf.Abs(Mathf.Cos(angle)),
                        Mathf.Abs(Mathf.Sin(angle))));
        }

        private static float StyleRockWeight(string style)
        {
            if (style == "rolling") return 0.22f;
            if (style == "escarpment") return 0.3f;
            return 0.78f;
        }

        private static float Wave01(float angle, float frequency, int seed, int salt)
        {
            float phase = ((seed >> (salt % 13)) ^ (seed * salt)) * 0.000173f;
            return Mathf.Clamp01(
                0.5f +
                Mathf.Sin(angle * frequency + phase) * 0.31f +
                Mathf.Sin(angle * (frequency * 2.17f + 0.3f) - phase * 0.73f) * 0.14f +
                Mathf.Sin(angle * (frequency * 4.63f + 1.1f) + phase * 1.31f) * 0.05f);
        }

        private static float WaveSigned(float angle, float frequency, int seed, int salt)
        {
            return Wave01(angle, frequency, seed, salt) * 2f - 1f;
        }

        private static float Noise01(float x, float y, int seed, int salt)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = x - x0;
            float ty = y - y0;
            float sx = tx * tx * (3f - 2f * tx);
            float sy = ty * ty * (3f - 2f * ty);
            float a = Hash01(x0, y0, seed, salt);
            float b = Hash01(x0 + 1, y0, seed, salt);
            float c = Hash01(x0, y0 + 1, seed, salt);
            float d = Hash01(x0 + 1, y0 + 1, seed, salt);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, sx),
                Mathf.Lerp(c, d, sx),
                sy);
        }

        private static float NoiseSigned(float x, float y, int seed, int salt)
        {
            return Noise01(x, y, seed, salt) * 2f - 1f;
        }

        private static float Contrast01(float value, float contrast)
        {
            return Mathf.Clamp01((value - 0.5f) * contrast + 0.5f);
        }

        private static float Hash01(int x, int y, int seed, int salt)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)(x * 374761393);
                h = (h << 13) | (h >> 19);
                h ^= (uint)(y * 668265263);
                h *= 1274126177u;
                h ^= (uint)(salt * 2246822519u);
                h ^= h >> 16;
                return (h & 0x00ffffff) / 16777215f;
            }
        }

        private void CreateCloudDecks(MapDefinition map)
        {
            MapSky sky = map.sky;
            if (sky == null) return;
            float nearOpacity = Mathf.Clamp(sky.cloudOpacity, 0f, 1.4f);
            float farOpacity = Mathf.Clamp(sky.cloudOpacity2, 0f, 1.4f);
            if (nearOpacity <= 0.01f && farOpacity <= 0.01f) return;

            GameObject root = new GameObject("CloudDecks");
            root.transform.SetParent(_root.transform, false);
            Color tint = HexColor(sky.cloudTintHex != 0 ? sky.cloudTintHex : 0xffffff);
            bool overcast = sky.cloudOpacity >= 0.95f &&
                sky.cloudOpacity2 >= 0.85f &&
                PositiveOrDefault(sky.turbidity, 4f) >= 5.4f;
            float altitude = PositiveOrDefault(
                sky.cloudAltM,
                overcast ? OvercastCloudAltitudeM : CumulusDefaultAltitudeM);
            float uvMeters = PositiveOrDefault(
                sky.cloudUvM,
                overcast ? OvercastCloudUvM : CumulusDefaultUvM);
            float hazeK = PositiveOrDefault(
                sky.cloudHazeK,
                overcast ? OvercastCloudHazeK : CumulusDefaultHazeK);
            if (nearOpacity > 0.01f)
            {
                CreateCloudDeck(
                    root.transform,
                    "CloudDeck-Cumulus",
                    map.id + "-clouds-a",
                    altitude,
                    uvMeters,
                    Mathf.Clamp01(nearOpacity * 0.86f),
                    tint,
                    hazeK,
                    CloudDomeRadiusM,
                    0.42f);
            }
            if (farOpacity > 0.01f)
            {
                CreateCloudDeck(
                    root.transform,
                    "CloudDeck-Cirrus",
                    map.id + "-clouds-b",
                    Mathf.Max(altitude * 1.72f, 980f),
                    uvMeters * 1.75f,
                    Mathf.Clamp01(farOpacity * 0.52f),
                    Color.Lerp(tint, RenderSettings.fogColor, 0.18f),
                    hazeK * 0.45f,
                    CloudDomeRadiusM + 40f,
                    0.18f);
            }
        }

        private void CreateCloudDeck(
            Transform parent,
            string name,
            string seed,
            float altitude,
            float size,
            float alpha,
            Color tint,
            float hazeK,
            float radius,
            float shadeStrength)
        {
            Vector3[] vertices = new Vector3[
                (CloudDomeSegments + 1) * (CloudDomeRings + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[CloudDomeSegments * CloudDomeRings * 6];
            const float maxPhi = Mathf.PI * 0.64f;
            for (int ring = 0; ring <= CloudDomeRings; ring++)
            {
                float v = ring / (float)CloudDomeRings;
                float phi = v * maxPhi;
                float y = Mathf.Cos(phi) * radius;
                float r = Mathf.Sin(phi) * radius;
                for (int i = 0; i <= CloudDomeSegments; i++)
                {
                    float u = i / (float)CloudDomeSegments;
                    float angle = u * Mathf.PI * 2f;
                    int index = ring * (CloudDomeSegments + 1) + i;
                    vertices[index] = new Vector3(
                        Mathf.Cos(angle) * r,
                        y,
                        Mathf.Sin(angle) * r);
                    uvs[index] = new Vector2(u, v);
                }
            }
            int triangle = 0;
            int stride = CloudDomeSegments + 1;
            for (int ring = 0; ring < CloudDomeRings; ring++)
            {
                for (int i = 0; i < CloudDomeSegments; i++)
                {
                    int a = ring * stride + i;
                    int b = a + 1;
                    int c = (ring + 1) * stride + i;
                    int d = c + 1;
                    triangles[triangle++] = a;
                    triangles[triangle++] = c;
                    triangles[triangle++] = b;
                    triangles[triangle++] = b;
                    triangles[triangle++] = c;
                    triangles[triangle++] = d;
                }
            }
            Mesh mesh = new Mesh { name = name + "-Mesh" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshes.Add(mesh);

            Color color = new Color(tint.r, tint.g, tint.b, alpha);
            Material material = CreateMaterial(color, MapMaterialRole.Cloud, seed);
            ConfigureCloudMaterial(material, altitude, size, hazeK, shadeStrength);

            GameObject deck = new GameObject(name);
            deck.transform.SetParent(parent, false);
            deck.AddComponent<MeshFilter>().sharedMesh = mesh;
            deck.AddComponent<MeshRenderer>().sharedMaterial = material;
            CloudDeckCount++;
        }

        private static void ConfigureCloudMaterial(
            Material material,
            float altitude,
            float scale,
            float hazeK,
            float shadeStrength)
        {
            if (material.HasProperty(MapMaterialFactory.CloudAltitudeProperty))
                material.SetFloat(MapMaterialFactory.CloudAltitudeProperty, altitude);
            if (material.HasProperty(MapMaterialFactory.CloudScaleProperty))
                material.SetFloat(MapMaterialFactory.CloudScaleProperty, scale);
            if (material.HasProperty(MapMaterialFactory.CloudHazeColorProperty))
                material.SetColor(
                    MapMaterialFactory.CloudHazeColorProperty,
                    RenderSettings.fogColor);
            if (material.HasProperty(MapMaterialFactory.CloudHazeRateProperty))
                material.SetFloat(MapMaterialFactory.CloudHazeRateProperty, hazeK);
            if (material.HasProperty(MapMaterialFactory.CloudYFadeProperty))
                material.SetVector(
                    MapMaterialFactory.CloudYFadeProperty,
                    new Vector4(0.007f, 0.034f, 0f, 0f));
            if (material.HasProperty(MapMaterialFactory.CloudShadeStrengthProperty))
                material.SetFloat(
                    MapMaterialFactory.CloudShadeStrengthProperty,
                    shadeStrength);
            if (material.HasProperty(MapMaterialFactory.CloudSunRotationProperty))
            {
                Vector3 sunDirection =
                    RenderSettings.sun != null
                        ? -RenderSettings.sun.transform.forward
                        : Vector3.forward;
                Vector2 horizontal = new Vector2(sunDirection.x, sunDirection.z);
                if (horizontal.sqrMagnitude < 0.0001f)
                    horizontal = Vector2.up;
                horizontal.Normalize();
                material.SetVector(
                    MapMaterialFactory.CloudSunRotationProperty,
                    new Vector4(-horizontal.y, -horizontal.x, 0f, 0f));
            }
            if (material.HasProperty("_Mode"))
                material.SetFloat("_Mode", 3f);
            if (material.HasProperty("_SrcBlend"))
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite"))
                material.SetInt("_ZWrite", 0);
            if (material.HasProperty("_Cull"))
                material.SetInt("_Cull", (int)CullMode.Off);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        private void CreateTerrain(Color color, MapSplat splat)
        {
            GameObject root = new GameObject("Battlefield");
            root.transform.SetParent(_root.transform, false);
            Material material = CreateMaterial(
                color,
                MapMaterialRole.Terrain,
                "terrain",
                splat);
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
                    _terrainChunks.Add(new MapChunkView
                    {
                        Root = chunk,
                        Center = new Vector3(
                            originX + chunkSize * 0.5f,
                            0f,
                            originZ + chunkSize * 0.5f)
                    });
                }
            }
            ActiveTerrainChunkCount = _terrainChunks.Count;
        }

        private void UpdateTerrainVisibility(
            Vector3 cameraPosition,
            float visibleDistanceM)
        {
            float distance = Mathf.Max(0f, visibleDistanceM);
            float chunkSize = TerrainHalfExtentM * 2f / TerrainChunksPerAxis;
            float reach = distance + chunkSize * 0.72f;
            float reachSquared = reach * reach;
            int active = 0;
            for (int i = 0; i < _terrainChunks.Count; i++)
            {
                MapChunkView chunk = _terrainChunks[i];
                float dx = cameraPosition.x - chunk.Center.x;
                float dz = cameraPosition.z - chunk.Center.z;
                bool visible = dx * dx + dz * dz <= reachSquared;
                if (chunk.Root.activeSelf != visible) chunk.Root.SetActive(visible);
                if (visible) active++;
            }
            ActiveTerrainChunkCount = active;
        }

        private void CreateGroundVariation(
            string mapId,
            MapSurface surface,
            Color groundColor,
            MapSplat splat)
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
            CreateSurfaceMesh(
                "Surface-GroundVariation",
                vertices,
                triangles,
                variation,
                MapMaterialRole.GroundVariation,
                mapId + "-ground",
                splat);
        }

        private void CreateWetGround(MapSurface surface, MapSplat splat)
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
                    MapMaterialRole.Marsh,
                    "marsh",
                    splat,
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
                MapMaterialRole role = surface.frozenWater
                    ? MapMaterialRole.Ice
                    : MapMaterialRole.Water;
                Material material = CreateMaterial(waterColor, role, role.ToString(), splat);
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

        private void CreateRoads(MapSurface surface, MapSplat splat)
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
                surface.roadCasingColor.ToColor(),
                MapMaterialRole.RoadCasing,
                splat);
            CreateRoadSurface(
                "Surface-Roads",
                roads,
                7.5f,
                GroundSurfaceY + 0.044f,
                surface.roadColor.ToColor(),
                MapMaterialRole.Road,
                splat);
        }

        private void CreateRoadSurface(
            string name,
            MapPolyline[] roads,
            float width,
            float height,
            Color color,
            MapMaterialRole role,
            MapSplat splat)
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
            CreateSurfaceMesh(name, vertices, triangles, color, role, name, splat);
        }

        private void CreateDiscSurface(
            string name,
            MapDisc[] discs,
            float baseHeight,
            Color color,
            MapMaterialRole role,
            string seed,
            MapSplat splat,
            bool conformToTerrain)
        {
            CreateDiscSurface(
                name,
                discs,
                baseHeight,
                CreateMaterial(color, role, seed, splat),
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
            CreateSurfaceMesh(
                "Surface-Craters",
                vertices,
                triangles,
                craterColor,
                MapMaterialRole.Crater,
                mapId + "-craters");
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
            return CreateSurfaceMesh(
                name,
                vertices,
                triangles,
                color,
                MapMaterialRole.Terrain,
                name);
        }

        private GameObject CreateSurfaceMesh(
            string name,
            List<Vector3> vertices,
            List<int> triangles,
            Color color,
            MapMaterialRole role,
            string seed)
        {
            return CreateSurfaceMesh(
                name,
                vertices,
                triangles,
                color,
                role,
                seed,
                null);
        }

        private GameObject CreateSurfaceMesh(
            string name,
            List<Vector3> vertices,
            List<int> triangles,
            Color color,
            MapMaterialRole role,
            string seed,
            MapSplat splat)
        {
            return CreateSurfaceMesh(
                name,
                vertices,
                triangles,
                CreateMaterial(color, role, seed, splat));
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

        private Material CreateMaterial(
            Color color,
            MapMaterialRole role,
            string seed)
        {
            return CreateMaterial(color, role, seed, null);
        }

        private Material CreateMaterial(
            Color color,
            MapMaterialRole role,
            string seed,
            MapSplat splat)
        {
            Material material = MapMaterialFactory.Create(color, role, seed, splat);
            _materials.Add(material);
            return material;
        }

        private Material CreateHorizonMaterial(
            Color color,
            string seed,
            MapHorizon horizon,
            bool ridge,
            float maxHeight)
        {
            Material material = CreateMaterial(color, MapMaterialRole.Horizon, seed);
            if (material.HasProperty(MapMaterialFactory.HorizonBandingProperty))
            {
                float banding = horizon != null
                    ? PositiveOrDefault(horizon.banding, StyleDefaultBanding(horizon.style))
                    : 0.1f;
                float grain = horizon != null
                    ? PositiveOrDefault(horizon.grain, 1f)
                    : 1f;
                material.SetFloat(
                    MapMaterialFactory.HorizonBandingProperty,
                    ridge ? Mathf.Clamp(banding, 0.02f, 0.16f) : Mathf.Clamp(banding * 0.28f, 0.01f, 0.08f));
                material.SetFloat(
                    MapMaterialFactory.HorizonDetailStrengthProperty,
                    ridge
                        ? Mathf.Clamp(0.36f + (grain - 1f) * 0.14f, 0.32f, 0.54f)
                        : 0.24f);
                material.SetFloat(
                    MapMaterialFactory.HorizonStyleProperty,
                    HorizonStyleCode(horizon != null ? horizon.style : null));
                material.SetFloat(
                    MapMaterialFactory.HorizonMaxHeightProperty,
                    Mathf.Max(80f, maxHeight));
            }
            return material;
        }

        private static float StyleDefaultBanding(string style)
        {
            return style == "mesa" ? 0.16f :
                style == "alpine" ? 0.06f :
                style == "escarpment" ? 0.1f :
                0.04f;
        }

        private static float HorizonStyleCode(string style)
        {
            return style == "alpine" ? 1f :
                style == "mesa" ? 2f :
                style == "escarpment" ? 3f :
                0f;
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
            result.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                color,
                MapMaterialRole.Rock,
                name);
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

        private sealed class MapChunkView
        {
            public GameObject Root;
            public Vector3 Center;
        }

        private readonly struct HorizonRow
        {
            public HorizonRow(
                float radius,
                float baseHeight,
                float amp,
                float f0,
                float f1,
                float aerial,
                bool skirt = false)
            {
                Radius = radius;
                Base = baseHeight;
                Amp = amp;
                F0 = f0;
                F1 = f1;
                Aerial = aerial;
                Skirt = skirt;
            }

            public readonly float Radius;
            public readonly float Base;
            public readonly float Amp;
            public readonly float F0;
            public readonly float F1;
            public readonly float Aerial;
            public readonly bool Skirt;
        }
    }
}
