using ClaudeOfTanks.Runtime;
using NUnit.Framework;
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
            for (int i = 0; i < catalog.Maps.Length; i++)
            {
                MapDefinition definition = catalog.Maps[i];
                AssertSurfaceData(definition);
                MapRuntime runtime = MapRuntime.Create(definition);
                try
                {
                    Assert.That(runtime.Root.name, Is.EqualTo("Map-" + definition.id));
                    Assert.That(runtime.Root.Find("Sun"), Is.Not.Null, definition.id);
                    Assert.That(runtime.Root.Find("Battlefield"), Is.Not.Null, definition.id);
                    Assert.That(runtime.Root.Find("Surface-GroundVariation"), Is.Not.Null, definition.id);
                    Assert.That(runtime.Root.Find("Surface-RoadCasing"), Is.Not.Null, definition.id);
                    Transform roads = runtime.Root.Find("Surface-Roads");
                    Assert.That(roads, Is.Not.Null, definition.id);
                    Assert.That(runtime.Root.Find("Surface-Craters"), Is.Not.Null, definition.id);
                    Assert.That(runtime.RoadPolylineCount, Is.EqualTo(definition.unitySurface.roads.Length), definition.id);
                    Assert.That(runtime.LakeCount, Is.EqualTo(definition.unitySurface.lakes.Length), definition.id);
                    Assert.That(runtime.MarshCount, Is.EqualTo(definition.unitySurface.marshes.Length), definition.id);
                    Assert.That(runtime.CraterCount, Is.EqualTo(definition.props.craters), definition.id);
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
