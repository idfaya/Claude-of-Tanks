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
            bool foundRaisedTerrain = false;
            bool foundDepressedTerrain = false;
            for (int i = 0; i < catalog.Maps.Length; i++)
            {
                MapDefinition definition = catalog.Maps[i];
                AssertSurfaceData(definition);
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
                        Is.EqualTo(StaticObstacleFlags.All),
                        obstacle.Id);
                }
                totalAuthoritativeObstacles += obstacles.Length;
                MapRuntime runtime = MapRuntime.Create(definition);
                try
                {
                    Assert.That(runtime.Root.name, Is.EqualTo("Map-" + definition.id));
                    Assert.That(runtime.Root.Find("Sun"), Is.Not.Null, definition.id);
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
                Is.GreaterThan(totalBuildings + totalWallRuns));
            Assert.That(totalTerrainTriangles, Is.GreaterThan(600000));
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
