using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class MapVisualTests
    {
        [Test]
        public void EveryMapBuildsNativeUnityEnvironment()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            int totalObjects = 0;
            int totalRoadTriangles = 0;
            int totalCraters = 0;
            int mapsWithWater = 0;
            int mapsWithMarshes = 0;
            int totalBuildings = 0;
            int totalTacticalBuildings = 0;
            int totalWallRuns = 0;
            int totalRubble = 0;
            int totalSandbagLines = 0;
            int totalHedgehogs = 0;
            int totalStructureTriangles = 0;
            int totalAuthoritativeObstacles = 0;
            int totalTerrainTriangles = 0;
            int totalVegetationStands = 0;
            int totalTrees = 0;
            HashSet<string> buildingKinds =
                new HashSet<string>();
            HashSet<int> recipeSignatures =
                new HashSet<int>();
            HashSet<string> vegetationSpecies =
                new HashSet<string>();
            HashSet<string> materialRoles =
                new HashSet<string>();
            HashSet<string> normalMapRoles =
                new HashSet<string>();
            bool foundAlphaFoliage = false;
            bool foundWindWeightedFoliage = false;
            bool foundRaisedTerrain = false;
            bool foundDepressedTerrain = false;
            bool foundSplatTerrain = false;
            bool foundSplatRoad = false;
            bool foundSplatIce = false;
            for (int i = 0; i < catalog.Maps.Length; i++)
            {
                MapDefinition definition = catalog.Maps[i];
                AssertSurfaceData(
                    definition,
                    buildingKinds,
                    recipeSignatures);
                AssertVisualCatalogData(definition);
                Assert.That(definition.unityVegetation, Is.Not.Null, definition.id);
                Assert.That(
                    definition.unityVegetation.stands,
                    Is.Not.Null.And.Not.Empty,
                    definition.id);
                int manifestTreeCount = 0;
                for (int standIndex = 0;
                    standIndex < definition.unityVegetation.stands.Length;
                    standIndex++)
                {
                    MapVegetationStand stand =
                        definition.unityVegetation.stands[standIndex];
                    Assert.That(
                        stand.zone,
                        Is.EqualTo("cluster")
                            .Or.EqualTo("lone")
                            .Or.EqualTo("rim")
                            .Or.EqualTo("belt"),
                        definition.id);
                    Assert.That(stand.species, Is.Not.Empty, definition.id);
                    vegetationSpecies.Add(stand.species);
                    Assert.That(stand.count, Is.GreaterThan(0), definition.id);
                    Assert.That(Mathf.Abs(stand.x), Is.LessThan(500f), definition.id);
                    Assert.That(Mathf.Abs(stand.z), Is.LessThan(500f), definition.id);
                    manifestTreeCount += stand.count;
                }
                Assert.That(
                    manifestTreeCount,
                    Is.EqualTo(definition.unityVegetation.treeCount),
                    definition.id);
                totalVegetationStands += definition.unityVegetation.stands.Length;
                totalTrees += manifestTreeCount;
                IHeightField heightField = MapSimulationAdapter.BuildHeightField(definition);
                StaticObstacle[] obstacles =
                    MapSimulationAdapter.BuildStaticObstacles(definition, heightField);
                Assert.That(obstacles, Is.Not.Empty, definition.id);
                Assert.That(
                    obstacles.Length,
                    Is.LessThanOrEqualTo(BattleState.MaximumStaticObstacles),
                    definition.id);
                HashSet<string> obstacleIds = new HashSet<string>();
                for (int obstacleIndex = 0; obstacleIndex < obstacles.Length; obstacleIndex++)
                {
                    StaticObstacle obstacle = obstacles[obstacleIndex];
                    Assert.That(obstacleIds.Add(obstacle.Id), Is.True, obstacle.Id);
                    Assert.That(obstacle.HalfWidthM, Is.GreaterThan(0f), obstacle.Id);
                    Assert.That(obstacle.HalfLengthM, Is.GreaterThan(0f), obstacle.Id);
                    Assert.That(obstacle.HeightM, Is.GreaterThan(0f), obstacle.Id);
                    Assert.That(
                        obstacle.Flags,
                        obstacle.Crushable
                            ? Is.EqualTo(
                                StaticObstacleFlags.Movement |
                                StaticObstacleFlags.Shells |
                                StaticObstacleFlags
                                    .Concealment)
                            : Is.EqualTo(StaticObstacleFlags.All),
                        obstacle.Id);
                }
                totalAuthoritativeObstacles += obstacles.Length;
                MapRuntime runtime = MapRuntime.Create(definition);
                try
                {
                    Assert.That(runtime.Root.name, Is.EqualTo("Map-" + definition.id));
                    Transform sun = runtime.Root.Find("Sun");
                    Assert.That(sun, Is.Not.Null, definition.id);
                    Assert.That(RenderSettings.fog, Is.True, definition.id);
                    Assert.That(
                        RenderSettings.fogMode,
                        Is.EqualTo(FogMode.ExponentialSquared),
                        definition.id);
                    float expectedFogDensity = Mathf.Clamp(
                        (definition.sky != null && definition.sky.fogDensity > 0f
                            ? definition.sky.fogDensity
                            : 0.00062f) * 0.55f,
                        0.00012f,
                        0.00058f);
                    Assert.That(
                        RenderSettings.fogDensity,
                        Is.EqualTo(expectedFogDensity).Within(0.000001f),
                        definition.id);
                    float expectedReflectionIntensity = Mathf.Clamp(
                        PositiveOrDefault(definition.sky.envIntensity, 0.2f) * 2.8f,
                        0.18f,
                        1.2f);
                    Assert.That(
                        RenderSettings.reflectionIntensity,
                        Is.EqualTo(expectedReflectionIntensity).Within(0.0001f),
                        definition.id);
                    Assert.That(RenderSettings.skybox, Is.Not.Null, definition.id);
                    Assert.That(
                        RenderSettings.sun,
                        Is.SameAs(sun.GetComponent<Light>()),
                        definition.id);
                    Assert.That(
                        sun.GetComponent<Light>().intensity,
                        Is.EqualTo(
                            Mathf.Clamp(
                                PositiveOrDefault(definition.sky.sunIntensity, 1f) *
                                    0.72f *
                                    Mathf.Clamp(
                                        PositiveOrDefault(
                                            definition.sky.postExposure,
                                            1f),
                                        0.82f,
                                        1.04f),
                                0.35f,
                                3.4f))
                            .Within(0.0001f),
                        definition.id);
                    AssertStandardSkyboxPresentation(
                        RenderSettings.skybox,
                        definition.sky,
                        definition.id);
                    Transform horizon = runtime.Root.Find("Horizon");
                    Assert.That(horizon, Is.Not.Null, definition.id);
                    Assert.That(horizon.Find("Horizon-Ridge"), Is.Not.Null, definition.id);
                    AssertHorizonRidgePresentation(
                        horizon.Find("Horizon-Ridge"),
                        definition.horizon,
                        definition.sky,
                        definition.id);
                    Assert.That(runtime.HorizonMeshCount, Is.GreaterThanOrEqualTo(1), definition.id);
                    Assert.That(runtime.HorizonVertexCount, Is.GreaterThan(900), definition.id);
                    if (definition.horizon.snowline > 0f || definition.horizon.snowHex != 0)
                        Assert.That(horizon.Find("Horizon-Snowcap"), Is.Not.Null, definition.id);
                    if (definition.horizon.treeline > 0f || definition.horizon.forestHex != 0)
                        Assert.That(horizon.Find("Horizon-Treeline"), Is.Not.Null, definition.id);
                    Transform clouds = runtime.Root.Find("CloudDecks");
                    int expectedCloudDecks =
                        (definition.sky.cloudOpacity > 0.01f ? 1 : 0) +
                        (definition.sky.cloudOpacity2 > 0.01f ? 1 : 0);
                    Assert.That(runtime.CloudDeckCount, Is.EqualTo(expectedCloudDecks), definition.id);
                    if (expectedCloudDecks > 0)
                    {
                        Assert.That(clouds, Is.Not.Null, definition.id);
                        if (definition.sky.cloudOpacity > 0.01f)
                        {
                            Assert.That(clouds.Find("CloudDeck-Cumulus"), Is.Not.Null, definition.id);
                            AssertCloudDeckPresentation(
                                clouds.Find("CloudDeck-Cumulus"),
                                definition.sky,
                                definition.id,
                                false);
                        }
                        if (definition.sky.cloudOpacity2 > 0.01f)
                        {
                            Assert.That(clouds.Find("CloudDeck-Cirrus"), Is.Not.Null, definition.id);
                            AssertCloudDeckPresentation(
                                clouds.Find("CloudDeck-Cirrus"),
                                definition.sky,
                                definition.id,
                                true);
                        }
                    }
                    Transform battlefield = runtime.Root.Find("Battlefield");
                    Assert.That(battlefield, Is.Not.Null, definition.id);
                    MeshFilter[] terrainMeshes = battlefield.GetComponentsInChildren<MeshFilter>();
                    Assert.That(
                        terrainMeshes.Length,
                        Is.EqualTo(MapRuntime.TerrainChunkCount),
                        definition.id);
                    Assert.That(runtime.TerrainVertexCount, Is.GreaterThan(16000), definition.id);
                    Assert.That(runtime.TerrainTriangleCount, Is.GreaterThan(30000), definition.id);
                    AssertSplatMaterial(
                        terrainMeshes[0].GetComponent<MeshRenderer>().sharedMaterial,
                        definition.splat,
                        definition.id,
                        "Terrain");
                    AssertTerrainDetailShader(
                        terrainMeshes[0].GetComponent<MeshRenderer>().sharedMaterial,
                        0f,
                        definition.id + ":Terrain");
                    AssertTerrainSplatTexture(
                        terrainMeshes[0].GetComponent<MeshRenderer>().sharedMaterial,
                        definition.splat,
                        definition.id);
                    foundSplatTerrain = true;
                    Assert.That(
                        battlefield.GetComponentsInChildren<Collider>(),
                        Is.Empty,
                        definition.id);
                    Assert.That(
                        runtime.ActiveTerrainChunkCount,
                        Is.EqualTo(MapRuntime.TerrainChunkCount),
                        definition.id);
                    for (int terrainIndex = 0; terrainIndex < terrainMeshes.Length; terrainIndex++)
                    {
                        Vector3[] vertices = terrainMeshes[terrainIndex].sharedMesh.vertices;
                        for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex += 97)
                        {
                            Vector3 world = terrainMeshes[terrainIndex].transform.TransformPoint(
                                vertices[vertexIndex]);
                            float expected = heightField.HeightAt(world.x, world.z);
                            Assert.That(world.y, Is.EqualTo(expected).Within(0.0001f), definition.id);
                            if (world.y > 0.25f) foundRaisedTerrain = true;
                            if (world.y < -0.25f) foundDepressedTerrain = true;
                        }
                    }
                    totalTerrainTriangles += runtime.TerrainTriangleCount;
                    Assert.That(runtime.Root.Find("Surface-GroundVariation"), Is.Not.Null, definition.id);
                    Transform vegetation = runtime.Root.Find("Vegetation");
                    Assert.That(vegetation, Is.Not.Null, definition.id);
                    Assert.That(runtime.TreeCount, Is.EqualTo(manifestTreeCount), definition.id);
                    Assert.That(runtime.VegetationChunkCount, Is.InRange(1, 16), definition.id);
                    Assert.That(
                        runtime.VegetationMeshCount,
                        Is.InRange(1, MapVegetationRuntime.MaximumMeshCount),
                        definition.id);
                    Assert.That(
                        runtime.VegetationVertexCount,
                        Is.InRange(manifestTreeCount * 14, 300000),
                        definition.id);
                    Assert.That(
                        vegetation.GetComponentsInChildren<Collider>(),
                        Is.Empty,
                        definition.id);
                    runtime.UpdateVegetationVisibility(new Vector3(5000f, 0f, 5000f), 0f);
                    Assert.That(runtime.ActiveVegetationChunkCount, Is.Zero, definition.id);
                    Assert.That(runtime.ActiveTerrainChunkCount, Is.Zero, definition.id);
                    runtime.UpdateVegetationVisibility(Vector3.zero);
                    Assert.That(
                        runtime.ActiveVegetationChunkCount,
                        Is.EqualTo(runtime.VegetationChunkCount),
                        definition.id);
                    Assert.That(
                        runtime.ActiveTerrainChunkCount,
                        Is.EqualTo(MapRuntime.TerrainChunkCount),
                        definition.id);
                    Assert.That(runtime.Root.Find("Surface-RoadCasing"), Is.Not.Null, definition.id);
                    Transform roads = runtime.Root.Find("Surface-Roads");
                    Assert.That(roads, Is.Not.Null, definition.id);
                    Material roadMaterial = roads.GetComponent<MeshRenderer>().sharedMaterial;
                    AssertSplatMaterial(
                        roadMaterial,
                        definition.splat,
                        definition.id,
                        "Road");
                    AssertTerrainDetailShader(
                        roadMaterial,
                        2f,
                        definition.id + ":Road");
                    AssertSplatColor(
                        roadMaterial,
                        MapMaterialFactory.SplatRoadTintProperty,
                        SplatTripleOrDefault(
                            definition.splat.roadTint,
                            1.08f,
                            1.04f,
                            0.96f),
                        definition.id + ":roadTint");
                    foundSplatRoad = true;
                    Vector3[] roadVertices =
                        roads.GetComponent<MeshFilter>().sharedMesh.vertices;
                    for (int roadVertex = 0; roadVertex < roadVertices.Length; roadVertex += 11)
                    {
                        Vector3 world = roads.TransformPoint(roadVertices[roadVertex]);
                        Assert.That(
                            world.y,
                            Is.EqualTo(heightField.HeightAt(world.x, world.z) + 0.069f)
                                .Within(0.0001f),
                            definition.id);
                    }
                    Assert.That(runtime.Root.Find("Surface-Craters"), Is.Not.Null, definition.id);
                    Assert.That(runtime.RoadPolylineCount, Is.EqualTo(definition.unitySurface.roads.Length), definition.id);
                    Assert.That(runtime.LakeCount, Is.EqualTo(definition.unitySurface.lakes.Length), definition.id);
                    Assert.That(runtime.MarshCount, Is.EqualTo(definition.unitySurface.marshes.Length), definition.id);
                    Assert.That(runtime.CraterCount, Is.EqualTo(definition.props.craters), definition.id);
                    Assert.That(runtime.BuildingCount,
                        Is.EqualTo(definition.unityStructures.buildings.Length), definition.id);
                    Assert.That(
                        runtime.DistinctBuildingKindCount,
                        Is.EqualTo(
                            DistinctBuildingKinds(
                                definition)),
                        definition.id);
                    Assert.That(
                        runtime.MinimumBuildingTriangleCount,
                        Is.GreaterThanOrEqualTo(20),
                        definition.id);
                    Assert.That(
                        runtime.MaximumBuildingHeightRatio,
                        Is.LessThanOrEqualTo(1.65f),
                        definition.id);
                    Assert.That(runtime.TacticalBuildingCount, Is.EqualTo(3), definition.id);
                    Assert.That(runtime.WallRunCount,
                        Is.EqualTo(definition.unityStructures.walls.Length), definition.id);
                    Assert.That(runtime.RubblePileCount,
                        Is.EqualTo(definition.unityStructures.rubblePiles), definition.id);
                    Assert.That(runtime.SandbagLineCount,
                        Is.EqualTo(definition.unityStructures.sandbagLines), definition.id);
                    Assert.That(runtime.HedgehogCount,
                        Is.EqualTo(definition.unityStructures.hedgehogs), definition.id);
                    Assert.That(roads.GetComponent<Collider>(), Is.Null, definition.id);
                    totalRoadTriangles += roads.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
                    totalCraters += runtime.CraterCount;

                    if (definition.unitySurface.lakes.Length > 0)
                    {
                        string name = definition.unitySurface.frozenWater
                            ? "Surface-FrozenWater"
                            : "Surface-Water";
                        Transform water = runtime.Root.Find(name);
                        Assert.That(water, Is.Not.Null, definition.id);
                        Assert.That(water.GetComponent<Collider>(), Is.Null, definition.id);
                        Material waterMaterial =
                            water.GetComponent<MeshRenderer>().sharedMaterial;
                        AssertSplatMaterial(
                            waterMaterial,
                            definition.splat,
                            definition.id,
                            definition.unitySurface.frozenWater ? "Ice" : "Water");
                        if (definition.unitySurface.frozenWater &&
                            definition.splat.iceLake)
                        {
                            Assert.That(
                                waterMaterial.GetFloat(
                                    MapMaterialFactory.SplatIceDriftProperty),
                                Is.EqualTo(
                                    PositiveOrDefault(
                                        definition.splat.iceDrift,
                                        0.85f))
                                    .Within(0.0001f),
                                definition.id);
                            foundSplatIce = true;
                        }
                        mapsWithWater++;
                    }
                    if (definition.unitySurface.marshes.Length > 0)
                    {
                        Transform marshes = runtime.Root.Find("Surface-Marshes");
                        Assert.That(marshes, Is.Not.Null, definition.id);
                        Assert.That(marshes.GetComponent<Collider>(), Is.Null, definition.id);
                        mapsWithMarshes++;
                    }
                    Transform structures = runtime.Root.Find("Structures");
                    Assert.That(structures, Is.Not.Null, definition.id);
                    MeshFilter[] structureMeshes = structures.GetComponentsInChildren<MeshFilter>();
                    Assert.That(structureMeshes.Length, Is.InRange(4, 5), definition.id);
                    Transform structureBodies = structures.Find("Structures-Bodies");
                    Assert.That(structureBodies, Is.Not.Null, definition.id);
                    int intactBodyTriangles =
                        structureBodies.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
                    for (int meshIndex = 0; meshIndex < structureMeshes.Length; meshIndex++)
                    {
                        Assert.That(structureMeshes[meshIndex].GetComponent<Collider>(),
                            Is.Null, definition.id);
                        totalStructureTriangles +=
                            structureMeshes[meshIndex].sharedMesh.triangles.Length / 3;
                    }
                    totalBuildings += runtime.BuildingCount;
                    totalTacticalBuildings += runtime.TacticalBuildingCount;
                    totalWallRuns += runtime.WallRunCount;
                    totalRubble += runtime.RubblePileCount;
                    totalSandbagLines += runtime.SandbagLineCount;
                    totalHedgehogs += runtime.HedgehogCount;

                    BattleState destructionState = new BattleState(
                        heightField,
                        501u,
                        500f,
                        obstacles);
                    int destroyed = 0;
                    for (int obstacleIndex = 0; obstacleIndex < obstacles.Length; obstacleIndex++)
                    {
                        if (!obstacles[obstacleIndex].Destructible ||
                            obstacles[obstacleIndex].Crushable)
                        {
                            continue;
                        }
                        Assert.That(
                            destructionState.DamageStaticObstacle(obstacleIndex, 10000f),
                            Is.True,
                            obstacles[obstacleIndex].Id);
                        destroyed++;
                    }
                    runtime.SyncDestroyedStructures(destructionState);
                    Assert.That(destroyed, Is.EqualTo(3), definition.id);
                    Assert.That(runtime.DestroyedBuildingCount, Is.EqualTo(3), definition.id);
                    Assert.That(
                        structureBodies.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3,
                        Is.LessThan(intactBodyTriangles),
                        definition.id);
                    Transform destroyedMesh = structures.Find("Structures-Destroyed");
                    Assert.That(destroyedMesh, Is.Not.Null, definition.id);
                    Assert.That(
                        destroyedMesh.GetComponent<MeshFilter>().sharedMesh.triangles.Length,
                        Is.GreaterThan(0),
                        definition.id);
                    Assert.That(
                        structures.GetComponentsInChildren<MeshRenderer>().Length,
                        Is.LessThanOrEqualTo(6),
                        definition.id);

                    runtime.SyncDestroyedStructures(new BattleState(
                        heightField,
                        501u,
                        500f,
                        obstacles));
                    Assert.That(runtime.DestroyedBuildingCount, Is.Zero, definition.id);
                    Assert.That(
                        structureBodies.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3,
                        Is.EqualTo(intactBodyTriangles),
                        definition.id);
                    Assert.That(destroyedMesh.gameObject.activeSelf, Is.False, definition.id);

                    int treeObstacleIndex = -1;
                    for (int obstacleIndex = 0;
                        obstacleIndex < obstacles.Length;
                        obstacleIndex++)
                    {
                        if (obstacles[obstacleIndex].Crushable)
                        {
                            treeObstacleIndex = obstacleIndex;
                            break;
                        }
                    }
                    Assert.That(treeObstacleIndex, Is.GreaterThanOrEqualTo(0), definition.id);
                    BattleState toppledTreeState = new BattleState(
                        heightField,
                        502u,
                        500f,
                        obstacles);
                    Assert.That(
                        toppledTreeState.DamageStaticObstacle(
                            treeObstacleIndex,
                            float.MaxValue),
                        Is.True,
                        definition.id);
                    runtime.SyncDestroyedStructures(toppledTreeState);
                    Assert.That(runtime.ToppledTreeCount, Is.EqualTo(1), definition.id);
                    Transform fallenTrees = vegetation.Find("Fallen-Trees");
                    Assert.That(fallenTrees, Is.Not.Null, definition.id);
                    Assert.That(
                        fallenTrees.GetComponent<MeshFilter>().sharedMesh.triangles,
                        Is.Not.Empty,
                        definition.id);
                    CollectProceduralMaterialRoles(
                        runtime.Root,
                        materialRoles,
                        normalMapRoles,
                        ref foundAlphaFoliage,
                        ref foundWindWeightedFoliage,
                        definition.id);

                    int objects = runtime.Root.GetComponentsInChildren<UnityEngine.Transform>().Length;
                    Assert.That(objects, Is.GreaterThan(8), definition.id);
                    totalObjects += objects;
                }
                finally
                {
                    runtime.Dispose();
                }
            }

            Assert.That(catalog.Maps, Has.Length.EqualTo(20));
            Assert.That(totalObjects, Is.GreaterThan(300));
            Assert.That(totalRoadTriangles, Is.GreaterThan(4000));
            Assert.That(totalCraters, Is.GreaterThan(1400));
            Assert.That(mapsWithWater, Is.GreaterThanOrEqualTo(4));
            Assert.That(mapsWithMarshes, Is.GreaterThanOrEqualTo(10));
            Assert.That(totalBuildings, Is.EqualTo(661));
            Assert.That(totalTacticalBuildings, Is.EqualTo(60));
            Assert.That(totalWallRuns, Is.EqualTo(197));
            Assert.That(totalRubble, Is.EqualTo(894));
            Assert.That(totalSandbagLines, Is.EqualTo(361));
            Assert.That(totalHedgehogs, Is.EqualTo(346));
            Assert.That(
                totalStructureTriangles,
                Is.InRange(200000, 300000));
            Assert.That(
                totalAuthoritativeObstacles,
                Is.EqualTo(66715));
            Assert.That(totalTerrainTriangles, Is.GreaterThan(600000));
            Assert.That(totalVegetationStands, Is.EqualTo(4579));
            Assert.That(totalTrees, Is.EqualTo(65170));
            Assert.That(foundRaisedTerrain, Is.True);
            Assert.That(foundDepressedTerrain, Is.True);
            Assert.That(buildingKinds, Has.Count.EqualTo(62));
            Assert.That(
                recipeSignatures,
                Has.Count.EqualTo(buildingKinds.Count));
            Assert.That(vegetationSpecies, Has.Count.EqualTo(13));
            AssertMaterialRole(materialRoles, "Terrain");
            AssertMaterialRole(materialRoles, "GroundVariation");
            AssertMaterialRole(materialRoles, "RoadCasing");
            AssertMaterialRole(materialRoles, "Road");
            AssertMaterialRole(materialRoles, "Marsh");
            AssertMaterialRole(materialRoles, "Water");
            AssertMaterialRole(materialRoles, "Ice");
            AssertMaterialRole(materialRoles, "Crater");
            AssertMaterialRole(materialRoles, "StructureBody");
            AssertMaterialRole(materialRoles, "StructureRoof");
            AssertMaterialRole(materialRoles, "StructureDetail");
            AssertMaterialRole(materialRoles, "StructureWall");
            AssertMaterialRole(materialRoles, "StructureCover");
            AssertMaterialRole(materialRoles, "Rock");
            AssertMaterialRole(materialRoles, "Bark");
            AssertMaterialRole(materialRoles, "Broadleaf");
            AssertMaterialRole(materialRoles, "Conifer");
            AssertMaterialRole(materialRoles, "Palm");
            AssertMaterialRole(materialRoles, "Birch");
            AssertMaterialRole(materialRoles, "Horizon");
            AssertMaterialRole(materialRoles, "Cloud");
            Assert.That(foundAlphaFoliage, Is.True);
            Assert.That(foundWindWeightedFoliage, Is.True);
            Assert.That(foundSplatTerrain, Is.True);
            Assert.That(foundSplatRoad, Is.True);
            Assert.That(foundSplatIce, Is.True);
            AssertMaterialRole(normalMapRoles, "Terrain");
            AssertMaterialRole(normalMapRoles, "Road");
            AssertMaterialRole(normalMapRoles, "RoadCasing");
            AssertMaterialRole(normalMapRoles, "Water");
            AssertMaterialRole(normalMapRoles, "Ice");
            AssertMaterialRole(normalMapRoles, "StructureBody");
            AssertMaterialRole(normalMapRoles, "StructureWall");
            AssertMaterialRole(normalMapRoles, "Bark");
        }

        private static void AssertSurfaceData(
            MapDefinition map,
            HashSet<string> buildingKinds,
            HashSet<int> recipeSignatures)
        {
            Assert.That(map.unitySurface, Is.Not.Null, map.id);
            Assert.That(map.unitySurface.roads, Is.Not.Null.And.Not.Empty, map.id);
            for (int i = 0; i < map.unitySurface.roads.Length; i++)
            {
                Assert.That(map.unitySurface.roads[i].points, Has.Length.GreaterThanOrEqualTo(2), map.id);
            }
            Assert.That(map.unitySurface.lakes, Is.Not.Null, map.id);
            Assert.That(map.unitySurface.marshes, Is.Not.Null, map.id);
            Assert.That(map.unityStructures, Is.Not.Null, map.id);
            Assert.That(map.unityStructures.buildings, Is.Not.Null.And.Not.Empty, map.id);
            Assert.That(map.unityStructures.walls, Is.Not.Null.And.Not.Empty, map.id);
            AssertColor(map.unityStructures.buildingColor, map.id + " building");
            HashSet<string> planPositions = new HashSet<string>();
            int tactical = 0;
            for (int i = 0; i < map.unityStructures.buildings.Length; i++)
            {
                MapBuilding building = map.unityStructures.buildings[i];
                Assert.That(building.kind, Is.Not.Empty, map.id);
                Assert.That(
                    MapStructureRuntime.SupportsBuildingKind(
                        building.kind),
                    Is.True,
                    map.id + ":" + building.kind);
                buildingKinds.Add(building.kind);
                recipeSignatures.Add(
                    MapStructureRuntime.BuildingKindSignature(
                        building.kind));
                Assert.That(building.profile, Is.Not.Empty, map.id);
                Assert.That(building.w, Is.GreaterThan(2f), map.id);
                Assert.That(building.d, Is.GreaterThan(2f), map.id);
                Assert.That(building.h, Is.GreaterThan(2f), map.id);
                Assert.That(Mathf.Abs(building.x), Is.LessThanOrEqualTo(500f), map.id);
                Assert.That(Mathf.Abs(building.z), Is.LessThanOrEqualTo(500f), map.id);
                if (building.tactical) tactical++;
                else Assert.That(
                    planPositions.Add(building.x.ToString("R") + ":" + building.z.ToString("R")),
                    Is.True,
                    map.id + " duplicate plan placement");
            }
            Assert.That(tactical, Is.EqualTo(3), map.id);
            AssertColor(map.unitySurface.groundColor, map.id + " ground");
            AssertColor(map.unitySurface.hardColor, map.id + " hard");
            AssertColor(map.unitySurface.softColor, map.id + " soft");
            AssertColor(map.unitySurface.roadColor, map.id + " road");
            AssertColor(map.unitySurface.roadCasingColor, map.id + " road casing");
            AssertColor(map.unitySurface.waterColor, map.id + " water");
        }

        private static void AssertVisualCatalogData(MapDefinition map)
        {
            Assert.That(map.horizon, Is.Not.Null, map.id);
            Assert.That(map.horizon.baseHex, Is.Not.Zero, map.id);
            Assert.That(
                map.horizon.style,
                Is.EqualTo("rolling")
                    .Or.EqualTo("alpine")
                    .Or.EqualTo("mesa")
                    .Or.EqualTo("escarpment"),
                map.id);
            Assert.That(PositiveOrDefault(map.horizon.amp, 1f), Is.InRange(0.4f, 2.3f), map.id);
            Assert.That(map.sky, Is.Not.Null, map.id);
            Assert.That(map.sky.fogMix, Is.InRange(0.45f, 0.95f), map.id);
            Assert.That(PositiveOrDefault(map.sky.envIntensity, 0.2f), Is.InRange(0.12f, 0.65f), map.id);
            Assert.That(PositiveOrDefault(map.sky.hemiIntensity, 0.32f), Is.InRange(0.18f, 0.9f), map.id);
            Assert.That(map.sky.cloudTintHex, Is.Not.Zero, map.id);
            Assert.That(map.sky.cloudOpacity, Is.GreaterThanOrEqualTo(0f), map.id);
            Assert.That(map.sky.cloudOpacity2, Is.GreaterThanOrEqualTo(0f), map.id);
            Assert.That(map.shot, Is.Not.Null, map.id);
            Assert.That(map.shot.pos, Has.Length.EqualTo(3), map.id);
            Assert.That(map.shot.look, Has.Length.EqualTo(3), map.id);
            Assert.That(map.splat, Is.Not.Null, map.id);
            Assert.That(map.splat.tintA, Has.Length.EqualTo(3), map.id);
            Assert.That(map.splat.tintB, Has.Length.EqualTo(3), map.id);
            Assert.That(map.splat.tintC, Has.Length.EqualTo(3), map.id);
            if (map.splat.roadTint != null)
                Assert.That(map.splat.roadTint, Has.Length.EqualTo(3), map.id);
            Assert.That(PositiveOrDefault(map.splat.microAmp, 1f), Is.InRange(0.2f, 1.6f), map.id);
            Assert.That(PositiveOrDefault(map.splat.midRelief, 1f), Is.InRange(0.35f, 1.3f), map.id);
            Assert.That(PositiveOrDefault(map.splat.midReliefFar, 480f), Is.InRange(240f, 900f), map.id);
            Assert.That(map.minimap, Is.Not.Null, map.id);
            Assert.That(map.minimap.@base, Has.Length.EqualTo(3), map.id);
            Assert.That(map.minimap.hard, Has.Length.EqualTo(3), map.id);
            Assert.That(map.minimap.soft, Has.Length.EqualTo(3), map.id);
            Assert.That(map.minimap.roadFill, Is.Not.Empty, map.id);
        }

        private static int DistinctBuildingKinds(
            MapDefinition map)
        {
            HashSet<string> kinds =
                new HashSet<string>();
            for (int i = 0;
                i < map.unityStructures.buildings.Length;
                i++)
            {
                kinds.Add(
                    map.unityStructures
                        .buildings[i].kind);
            }
            return kinds.Count;
        }

        private static void AssertColor(MapColor color, string message)
        {
            Assert.That(color, Is.Not.Null, message);
            Assert.That(color.r, Is.InRange(0f, 1f), message);
            Assert.That(color.g, Is.InRange(0f, 1f), message);
            Assert.That(color.b, Is.InRange(0f, 1f), message);
        }

        private static void AssertSplatMaterial(
            Material material,
            MapSplat splat,
            string mapId,
            string role)
        {
            Assert.That(material, Is.Not.Null, mapId + ":" + role);
            Assert.That(splat, Is.Not.Null, mapId + ":" + role);
            Assert.That(material.name, Does.Contain("-splat"), mapId + ":" + role);
            Assert.That(
                material.GetFloat(MapMaterialFactory.SplatAppliedProperty),
                Is.EqualTo(1f).Within(0.0001f),
                mapId + ":" + role);
            AssertSplatColor(
                material,
                MapMaterialFactory.SplatTintAProperty,
                SplatTripleOrDefault(splat.tintA, 1.16f, 1.08f, 0.76f),
                mapId + ":" + role + ":tintA");
            AssertSplatColor(
                material,
                MapMaterialFactory.SplatTintBProperty,
                SplatTripleOrDefault(splat.tintB, 0.78f, 0.90f, 0.72f),
                mapId + ":" + role + ":tintB");
            AssertSplatColor(
                material,
                MapMaterialFactory.SplatTintCProperty,
                SplatTripleOrDefault(splat.tintC, 1.10f, 1.04f, 0.84f),
                mapId + ":" + role + ":tintC");
            Assert.That(
                material.GetFloat(MapMaterialFactory.SplatMicroAmpProperty),
                Is.EqualTo(PositiveOrDefault(splat.microAmp, 1f)).Within(0.0001f),
                mapId + ":" + role);
            Assert.That(
                material.GetFloat(MapMaterialFactory.SplatMidReliefProperty),
                Is.EqualTo(PositiveOrDefault(splat.midRelief, 1f)).Within(0.0001f),
                mapId + ":" + role);
            Assert.That(
                material.GetFloat(MapMaterialFactory.SplatMidReliefFarProperty),
                Is.EqualTo(PositiveOrDefault(splat.midReliefFar, 480f)).Within(0.0001f),
                mapId + ":" + role);
            Assert.That(
                material.GetFloat(MapMaterialFactory.SplatSandMacroProperty),
                Is.EqualTo(Mathf.Clamp01(splat.sandMacro)).Within(0.0001f),
                mapId + ":" + role);
            Assert.That(
                material.GetFloat(MapMaterialFactory.SplatRippleAmpProperty),
                Is.EqualTo(Mathf.Clamp(splat.rippleAmp, 0f, 1.2f)).Within(0.0001f),
                mapId + ":" + role);
            Assert.That(
                material.GetFloat(MapMaterialFactory.TerrainCloudShadeProperty),
                Is.InRange(splat.iceLake ? 0.08f : 0.16f, splat.iceLake ? 0.11f : 0.26f),
                mapId + ":" + role);
        }

        private static void AssertTerrainSplatTexture(
            Material material,
            MapSplat splat,
            string mapId)
        {
            Texture2D texture = material.mainTexture as Texture2D;
            Assert.That(texture, Is.Not.Null, mapId);
            Assert.That(texture.width, Is.GreaterThanOrEqualTo(128), mapId);
            Assert.That(texture.height, Is.GreaterThanOrEqualTo(128), mapId);
            Color[] pixels = texture.GetPixels(0);
            Assert.That(pixels, Is.Not.Empty, mapId);
            float minLuminance = float.PositiveInfinity;
            float maxLuminance = 0f;
            float average = 0f;
            Color first = pixels[0];
            int distinct = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                float luminance =
                    pixels[i].r * 0.2126f +
                    pixels[i].g * 0.7152f +
                    pixels[i].b * 0.0722f;
                minLuminance = Mathf.Min(minLuminance, luminance);
                maxLuminance = Mathf.Max(maxLuminance, luminance);
                average += luminance;
                float delta =
                    Mathf.Abs(pixels[i].r - first.r) +
                    Mathf.Abs(pixels[i].g - first.g) +
                    Mathf.Abs(pixels[i].b - first.b);
                if (delta > 0.08f) distinct++;
            }
            average /= pixels.Length;
            float range = maxLuminance - minLuminance;
            Assert.That(
                material.GetFloat(MapMaterialFactory.TerrainDetailRangeProperty),
                Is.EqualTo(range).Within(0.01f),
                mapId);
            Assert.That(range, Is.GreaterThan(0.09f), mapId);
            Assert.That(distinct, Is.GreaterThan(pixels.Length / 12), mapId);
            if (splat.sandMacro > 0.5f)
            {
                Assert.That(maxLuminance, Is.LessThan(0.95f), mapId);
                Assert.That(
                    range,
                    Is.GreaterThan(splat.sandMacro > 0.85f ? 0.22f : 0.17f),
                    mapId);
                Assert.That(average, Is.LessThan(0.74f), mapId);
            }
        }

        private static void AssertTerrainDetailShader(
            Material material,
            float roleCode,
            string message)
        {
            Assert.That(material.shader.name, Is.EqualTo("ClaudeOfTanks/MapTerrainDetail"), message);
            Assert.That(
                material.GetFloat(MapMaterialFactory.TerrainShaderAppliedProperty),
                Is.EqualTo(1f).Within(0.0001f),
                message);
            Assert.That(
                material.GetFloat(MapMaterialFactory.TerrainRoleProperty),
                Is.EqualTo(roleCode).Within(0.0001f),
                message);
            Assert.That(
                material.HasProperty(MapMaterialFactory.SplatSandMacroProperty),
                Is.True,
                message);
            Assert.That(
                material.HasProperty(MapMaterialFactory.TerrainCloudShadeProperty),
                Is.True,
                message);
        }

        private static void AssertSplatColor(
            Material material,
            string property,
            float[] expected,
            string message)
        {
            Assert.That(expected, Has.Length.EqualTo(3), message);
            Color actual = material.GetColor(property);
            Assert.That(actual.r, Is.EqualTo(expected[0]).Within(0.0001f), message);
            Assert.That(actual.g, Is.EqualTo(expected[1]).Within(0.0001f), message);
            Assert.That(actual.b, Is.EqualTo(expected[2]).Within(0.0001f), message);
        }

        private static float[] SplatTripleOrDefault(
            float[] values,
            float r,
            float g,
            float b)
        {
            return values != null && values.Length >= 3
                ? values
                : new[] { r, g, b };
        }

        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        private static Color HexColor(int value)
        {
            return new Color(
                ((value >> 16) & 255) / 255f,
                ((value >> 8) & 255) / 255f,
                (value & 255) / 255f);
        }

        private static float Luminance(Color color)
        {
            return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        }

        private static float ColorDistance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) +
                Mathf.Abs(a.g - b.g) +
                Mathf.Abs(a.b - b.b);
        }

        private static void CollectProceduralMaterialRoles(
            Transform root,
            HashSet<string> roles,
            HashSet<string> normalMapRoles,
            ref bool foundAlphaFoliage,
            ref bool foundWindWeightedFoliage,
            string mapId)
        {
            MeshRenderer[] renderers =
                root.GetComponentsInChildren<MeshRenderer>(true);
            Assert.That(renderers, Is.Not.Empty, mapId);
            const string prefix = "MapProcedural-";
            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                Assert.That(material, Is.Not.Null, mapId + ":" + renderers[i].name);
                Texture texture = material.mainTexture;
                Assert.That(texture, Is.Not.Null, mapId + ":" + renderers[i].name);
                Assert.That(
                    texture.name,
                    Does.StartWith(prefix),
                    mapId + ":" + renderers[i].name);
                string roleAndSeed = texture.name.Substring(prefix.Length);
                int separator = roleAndSeed.IndexOf('-');
                Assert.That(separator, Is.GreaterThan(0), texture.name);
                string role = roleAndSeed.Substring(0, separator);
                roles.Add(role);
                if (material.HasProperty("_BumpMap"))
                {
                    Texture normal = material.GetTexture("_BumpMap");
                    if (normal != null)
                    {
                        const string normalPrefix = "MapProceduralNormal-";
                        Assert.That(
                            normal.name,
                            Does.StartWith(normalPrefix),
                            mapId + ":" + renderers[i].name);
                        string normalRoleAndSeed =
                            normal.name.Substring(normalPrefix.Length);
                        int normalSeparator = normalRoleAndSeed.IndexOf('-');
                        Assert.That(normalSeparator, Is.GreaterThan(0), normal.name);
                        normalMapRoles.Add(
                            normalRoleAndSeed.Substring(0, normalSeparator));
                    }
                }
                if (role == "Broadleaf" ||
                    role == "Conifer" ||
                    role == "Palm" ||
                    role == "Birch")
                {
                    MeshFilter filter = renderers[i].GetComponent<MeshFilter>();
                    Assert.That(filter, Is.Not.Null, mapId + ":" + renderers[i].name);
                    Assert.That(
                        filter.sharedMesh.uv,
                        Has.Length.EqualTo(filter.sharedMesh.vertexCount),
                        mapId + ":" + renderers[i].name);
                    Assert.That(
                        material.renderQueue,
                        Is.EqualTo((int)UnityEngine.Rendering.RenderQueue.AlphaTest),
                        mapId + ":" + renderers[i].name);
                    Assert.That(
                        material.shader.name,
                        Is.EqualTo("ClaudeOfTanks/MapFoliageWindCutout"),
                        mapId + ":" + renderers[i].name);
                    if (material.HasProperty("_Cull"))
                    {
                        Assert.That(
                            material.GetInt("_Cull"),
                            Is.EqualTo((int)UnityEngine.Rendering.CullMode.Off),
                            mapId + ":" + renderers[i].name);
                    }
                    Assert.That(
                        material.GetFloat("_Cutoff"),
                        Is.GreaterThan(0.3f),
                        mapId + ":" + renderers[i].name);
                    Assert.That(
                        material.GetFloat("_WindStrength"),
                        Is.GreaterThan(0f),
                        mapId + ":" + renderers[i].name);
                    Color[] colors = filter.sharedMesh.colors;
                    Assert.That(
                        colors,
                        Has.Length.EqualTo(filter.sharedMesh.vertexCount),
                        mapId + ":" + renderers[i].name);
                    for (int colorIndex = 0;
                        colorIndex < colors.Length;
                        colorIndex++)
                    {
                        if (colors[colorIndex].a > 0.01f)
                        {
                            foundWindWeightedFoliage = true;
                            break;
                        }
                    }
                    foundAlphaFoliage = true;
                }
            }
        }

        private static void AssertMaterialRole(
            HashSet<string> roles,
            string role)
        {
            Assert.That(roles.Contains(role), Is.True, role);
        }

        private static void AssertHorizonRidgePresentation(
            Transform ridge,
            MapHorizon horizon,
            MapSky sky,
            string mapId)
        {
            Assert.That(ridge, Is.Not.Null, mapId);
            MeshFilter filter = ridge.GetComponent<MeshFilter>();
            Assert.That(filter, Is.Not.Null, mapId);
            Mesh mesh = filter.sharedMesh;
            Assert.That(mesh, Is.Not.Null, mapId);
            Assert.That(mesh.vertexCount, Is.GreaterThan(900), mapId);
            Color[] colors = mesh.colors;
            Assert.That(colors, Has.Length.EqualTo(mesh.vertexCount), mapId);
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float minRadius = float.PositiveInfinity;
            float maxRadius = 0f;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                minY = Mathf.Min(minY, vertices[i].y);
                maxY = Mathf.Max(maxY, vertices[i].y);
                float radius = new Vector2(vertices[i].x, vertices[i].z).magnitude;
                minRadius = Mathf.Min(minRadius, radius);
                maxRadius = Mathf.Max(maxRadius, radius);
            }
            Assert.That(maxY - minY, Is.GreaterThan(70f), mapId);
            Assert.That(maxRadius - minRadius, Is.GreaterThan(300f), mapId);
            if (horizon.style == "mesa")
            {
                Assert.That(
                    mesh.vertexCount,
                    Is.EqualTo(6 * (520 + 1)),
                    mapId);
                Assert.That(
                    mesh.triangles.Length,
                    Is.EqualTo((6 - 1) * 520 * 6),
                    mapId);
                Assert.That(
                    maxY - minY,
                    Is.LessThan(180f + PositiveOrDefault(horizon.amp, 1f) * 92f),
                    mapId);
                if (mapId == "desert")
                    Assert.That(maxY - minY, Is.LessThan(285f), mapId);
                Assert.That(maxRadius, Is.GreaterThan(1500f), mapId);
                AssertMesaSkylineBreakup(mesh, mapId);
            }
            bool foundColorVariation = false;
            float nearestFogDistance = float.PositiveInfinity;
            Color fog = HexColor(sky != null && sky.fogTintHex != 0
                ? sky.fogTintHex
                : 0x8799a0);
            Color first = colors[0];
            for (int i = 1; i < colors.Length; i++)
            {
                float delta =
                    Mathf.Abs(colors[i].r - first.r) +
                    Mathf.Abs(colors[i].g - first.g) +
                    Mathf.Abs(colors[i].b - first.b);
                if (delta > 0.08f)
                {
                    foundColorVariation = true;
                }
                if (horizon.style == "mesa")
                    nearestFogDistance = Mathf.Min(
                        nearestFogDistance,
                        ColorDistance(colors[i], fog));
            }
            Assert.That(foundColorVariation, Is.True, mapId);
            if (horizon.style == "mesa")
                Assert.That(
                    nearestFogDistance,
                    Is.LessThan(0.58f),
                    mapId);
            MeshRenderer renderer = ridge.GetComponent<MeshRenderer>();
            Assert.That(renderer, Is.Not.Null, mapId);
            Material material = renderer.sharedMaterial;
            Assert.That(material, Is.Not.Null, mapId);
            Assert.That(
                material.shader.name,
                Is.EqualTo("ClaudeOfTanks/MapHorizonDetail"),
                mapId);
            Assert.That(
                material.HasProperty(MapMaterialFactory.HorizonBandingProperty),
                Is.True,
                mapId);
            Assert.That(
                material.GetFloat(MapMaterialFactory.HorizonBandingProperty),
                Is.GreaterThan(0.01f),
                mapId);
            Assert.That(
                material.GetFloat(MapMaterialFactory.HorizonDetailStrengthProperty),
                Is.GreaterThan(0.3f),
                mapId);
            Assert.That(
                material.GetFloat(MapMaterialFactory.HorizonStyleProperty),
                Is.EqualTo(HorizonStyleCode(horizon.style)).Within(0.0001f),
                mapId);
            Assert.That(
                material.GetFloat(MapMaterialFactory.HorizonMaxHeightProperty),
                Is.GreaterThanOrEqualTo(maxY),
                mapId);
            Assert.That(
                material.GetFloat(MapMaterialFactory.HorizonTextureRangeProperty),
                Is.GreaterThan(0.025f),
                mapId);
            Assert.That(
                material.mainTexture.name,
                Does.StartWith("MapProcedural-Horizon-"),
                mapId);
        }

        private static void AssertMesaSkylineBreakup(
            Mesh mesh,
            string mapId)
        {
            const int segments = 520;
            const int stride = segments + 1;
            Vector3[] vertices = mesh.vertices;
            float minSkyline = float.PositiveInfinity;
            float maxSkyline = float.NegativeInfinity;
            float[] skyline = new float[segments];
            for (int segment = 0; segment < segments; segment++)
            {
                float top = float.NegativeInfinity;
                for (int row = 2; row < 6; row++)
                {
                    top = Mathf.Max(
                        top,
                        vertices[row * stride + segment].y);
                }
                skyline[segment] = top;
                minSkyline = Mathf.Min(minSkyline, top);
                maxSkyline = Mathf.Max(maxSkyline, top);
            }

            float threshold = Mathf.Lerp(minSkyline, maxSkyline, 0.58f);
            int transitions = 0;
            bool previousHigh = skyline[segments - 1] >= threshold;
            for (int segment = 0; segment < segments; segment++)
            {
                bool high = skyline[segment] >= threshold;
                if (high != previousHigh)
                    transitions++;
                previousHigh = high;
            }

            Assert.That(
                maxSkyline - minSkyline,
                Is.GreaterThan(32f),
                mapId + ": skyline relief");
            Assert.That(
                transitions,
                Is.GreaterThanOrEqualTo(4),
                mapId + ": skyline table/butte breakup");
        }

        private static float HorizonStyleCode(string style)
        {
            return style == "alpine" ? 1f :
                style == "mesa" ? 2f :
                style == "escarpment" ? 3f :
                0f;
        }

        private static void AssertCloudDeckPresentation(
            Transform deck,
            MapSky sky,
            string mapId,
            bool cirrus)
        {
            Assert.That(deck, Is.Not.Null, mapId);
            MeshFilter filter = deck.GetComponent<MeshFilter>();
            Assert.That(filter, Is.Not.Null, mapId + ":" + deck.name);
            Mesh mesh = filter.sharedMesh;
            Assert.That(mesh, Is.Not.Null, mapId + ":" + deck.name);
            Assert.That(mesh.vertexCount, Is.EqualTo((96 + 1) * (18 + 1)), mapId);
            Bounds bounds = mesh.bounds;
            Assert.That(bounds.extents.x, Is.GreaterThan(1200f), mapId);
            Assert.That(bounds.extents.z, Is.GreaterThan(1200f), mapId);
            Assert.That(bounds.max.y, Is.GreaterThan(1280f), mapId);
            Assert.That(bounds.min.y, Is.LessThan(0f), mapId);

            MeshRenderer renderer = deck.GetComponent<MeshRenderer>();
            Assert.That(renderer, Is.Not.Null, mapId + ":" + deck.name);
            Material material = renderer.sharedMaterial;
            Assert.That(material, Is.Not.Null, mapId + ":" + deck.name);
            Assert.That(
                material.shader.name,
                Is.EqualTo("ClaudeOfTanks/MapCloudDeck"),
                mapId + ":" + deck.name);
            Assert.That(
                material.renderQueue,
                Is.EqualTo((int)UnityEngine.Rendering.RenderQueue.Transparent),
                mapId + ":" + deck.name);

            bool overcast = sky.cloudOpacity >= 0.95f &&
                sky.cloudOpacity2 >= 0.85f &&
                PositiveOrDefault(sky.turbidity, 4f) >= 5.4f;
            float altitude = PositiveOrDefault(
                sky.cloudAltM,
                overcast ? 340f : 620f);
            float uvMeters = PositiveOrDefault(
                sky.cloudUvM,
                overcast ? 2400f : 3200f);
            float hazeK = PositiveOrDefault(
                sky.cloudHazeK,
                overcast ? 0.00015f : 0.00023f);
            if (cirrus)
            {
                altitude = Mathf.Max(altitude * 1.72f, 980f);
                uvMeters *= 1.75f;
                hazeK *= 0.45f;
            }
            Assert.That(
                material.GetFloat(MapMaterialFactory.CloudAltitudeProperty),
                Is.EqualTo(altitude).Within(0.0001f),
                mapId + ":" + deck.name);
            Assert.That(
                material.GetFloat(MapMaterialFactory.CloudScaleProperty),
                Is.EqualTo(uvMeters).Within(0.0001f),
                mapId + ":" + deck.name);
            Assert.That(
                material.GetFloat(MapMaterialFactory.CloudHazeRateProperty),
                Is.EqualTo(hazeK).Within(0.000001f),
                mapId + ":" + deck.name);
            Vector4 fade = material.GetVector(MapMaterialFactory.CloudYFadeProperty);
            Assert.That(fade.x, Is.EqualTo(0.007f).Within(0.0001f), mapId);
            Assert.That(fade.y, Is.EqualTo(0.034f).Within(0.0001f), mapId);
            Assert.That(
                material.GetFloat(MapMaterialFactory.CloudShadeStrengthProperty),
                Is.EqualTo(cirrus ? 0.18f : 0.42f).Within(0.0001f),
                mapId + ":" + deck.name);
        }

        private static void AssertStandardSkyboxPresentation(
            Material skybox,
            MapSky sky,
            string mapId)
        {
            Assert.That(skybox, Is.Not.Null, mapId);
            Assert.That(
                skybox.shader.name,
                Is.EqualTo("Skybox/Procedural"),
                mapId);
            Assert.That(
                skybox.name,
                Is.EqualTo("MapProceduralSkybox"),
                mapId);
            Assert.That(skybox.HasProperty("_SkyTint"), Is.True, mapId);
            Assert.That(skybox.HasProperty("_GroundColor"), Is.True, mapId);
            Assert.That(skybox.HasProperty("_Exposure"), Is.True, mapId);
            Assert.That(skybox.HasProperty("_AtmosphereThickness"), Is.True, mapId);
            Assert.That(skybox.HasProperty("_SunSize"), Is.True, mapId);
            Assert.That(skybox.HasProperty("_SunSizeConvergence"), Is.True, mapId);
            Assert.That(skybox.HasProperty("_SunDisk"), Is.True, mapId);
            float expectedExposure = Mathf.Clamp(
                0.96f * PositiveOrDefault(sky?.postExposure ?? 0f, 1f) +
                    PositiveOrDefault(sky?.envIntensity ?? 0f, 0.2f) * 0.16f,
                0.78f,
                1.16f);
            Assert.That(
                skybox.GetFloat("_Exposure"),
                Is.EqualTo(expectedExposure).Within(0.0001f),
                mapId);
            float density = sky != null ? sky.fogDensity : 0.00062f;
            float turbidity = PositiveOrDefault(sky?.turbidity ?? 0f, 4f);
            float rayleigh = PositiveOrDefault(sky?.rayleigh ?? 0f, 1.2f);
            float mie = PositiveOrDefault(sky?.mieCoefficient ?? 0f, 0.006f);
            float mieG = PositiveOrDefault(sky?.mieDirectionalG ?? 0f, 0.82f);
            float expectedThickness = Mathf.Clamp(
                Mathf.Lerp(
                    0.34f,
                    0.92f,
                    Mathf.InverseLerp(0.0003f, 0.001f, density)) +
                (turbidity - 4f) * 0.055f +
                (rayleigh - 1.2f) * 0.075f,
                0.24f,
                1.22f);
            Assert.That(
                skybox.GetFloat("_AtmosphereThickness"),
                Is.EqualTo(expectedThickness).Within(0.0001f),
                mapId);
            Assert.That(skybox.GetFloat("_SunDisk"), Is.EqualTo(2f).Within(0.0001f), mapId);
            Assert.That(
                skybox.GetFloat("_SunSize"),
                Is.EqualTo(Mathf.Clamp(
                    0.018f + mie * 4.2f + (mieG - 0.78f) * 0.045f,
                    0.018f,
                    0.075f)).Within(0.0001f),
                mapId);
            Assert.That(
                skybox.GetFloat("_SunSizeConvergence"),
                Is.EqualTo(Mathf.Clamp(
                    2.4f + (mieG - 0.78f) * 8.0f + mie * 90f,
                    2.0f,
                    8.0f)).Within(0.0001f),
                mapId);
        }
    }
}
