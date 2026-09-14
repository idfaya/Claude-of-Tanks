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
            bool foundRaisedTerrain = false;
            bool foundDepressedTerrain = false;
            for (int i = 0; i < catalog.Maps.Length; i++)
            {
                MapDefinition definition = catalog.Maps[i];
                AssertSurfaceData(definition);
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
                                StaticObstacleFlags.Vision)
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
                    Assert.That(
                        RenderSettings.sun,
                        Is.SameAs(sun.GetComponent<Light>()),
                        definition.id);
                    Transform battlefield = runtime.Root.Find("Battlefield");
                    Assert.That(battlefield, Is.Not.Null, definition.id);
                    MeshFilter[] terrainMeshes = battlefield.GetComponentsInChildren<MeshFilter>();
                    Assert.That(
                        terrainMeshes.Length,
                        Is.EqualTo(MapRuntime.TerrainChunkCount),
                        definition.id);
                    Assert.That(runtime.TerrainVertexCount, Is.GreaterThan(16000), definition.id);
                    Assert.That(runtime.TerrainTriangleCount, Is.GreaterThan(30000), definition.id);
                    Assert.That(
                        battlefield.GetComponentsInChildren<Collider>(),
                        Is.Empty,
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
                    runtime.UpdateVegetationVisibility(Vector3.zero);
                    Assert.That(
                        runtime.ActiveVegetationChunkCount,
                        Is.EqualTo(runtime.VegetationChunkCount),
                        definition.id);
                    Assert.That(runtime.Root.Find("Surface-RoadCasing"), Is.Not.Null, definition.id);
                    Transform roads = runtime.Root.Find("Surface-Roads");
                    Assert.That(roads, Is.Not.Null, definition.id);
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
            Assert.That(totalStructureTriangles, Is.GreaterThan(75000));
            Assert.That(
                totalAuthoritativeObstacles,
                Is.EqualTo(66715));
            Assert.That(totalTerrainTriangles, Is.GreaterThan(600000));
            Assert.That(totalVegetationStands, Is.EqualTo(4579));
            Assert.That(totalTrees, Is.EqualTo(65170));
            Assert.That(foundRaisedTerrain, Is.True);
            Assert.That(foundDepressedTerrain, Is.True);
        }

        private static void AssertSurfaceData(MapDefinition map)
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

        private static void AssertColor(MapColor color, string message)
        {
            Assert.That(color, Is.Not.Null, message);
            Assert.That(color.r, Is.InRange(0f, 1f), message);
            Assert.That(color.g, Is.InRange(0f, 1f), message);
            Assert.That(color.b, Is.InRange(0f, 1f), message);
        }
    }
}
