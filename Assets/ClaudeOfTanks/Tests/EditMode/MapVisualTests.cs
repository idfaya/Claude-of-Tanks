using ClaudeOfTanks.Runtime;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class MapVisualTests
    {
        [Test]
        public void EveryMapBuildsNativeUnityEnvironment()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            int totalObjects = 0;
            for (int i = 0; i < catalog.Maps.Length; i++)
            {
                MapDefinition definition = catalog.Maps[i];
                MapRuntime runtime = MapRuntime.Create(definition);
                try
                {
                    Assert.That(runtime.Root.name, Is.EqualTo("Map-" + definition.id));
                    Assert.That(runtime.Root.Find("Sun"), Is.Not.Null, definition.id);
                    Assert.That(runtime.Root.Find("Battlefield"), Is.Not.Null, definition.id);
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
        }
    }
}
