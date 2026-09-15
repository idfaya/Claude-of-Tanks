using System.Collections;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    public sealed class
        VehicleGeometryRecipePlayModeTests
    {
        [UnityTest]
        public IEnumerator CatalogUsesOnlyRecipeGeometryAtRuntime()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition[] vehicles =
                catalog.Vehicles;
            for (int index = 0;
                index < vehicles.Length;
                index++)
            {
                VehicleDefinition definition =
                    vehicles[index];
                TankView view = TankView.Create(
                    new TankState(
                        "runtime-recipe-" +
                        definition.id,
                        Team.Alpha,
                        definition.ToTankSpec(),
                        Float3.Zero,
                        0f),
                    definition,
                    "factory",
                    "forest",
                    catalog);
                Transform recipe =
                    view.Root.Find(
                        "AuthoritativeVisualRecipe");
                Assert.That(
                    recipe,
                    Is.Not.Null,
                    definition.id);
                Assert.That(
                    recipe.GetComponentsInChildren<
                        Renderer>(true).Length,
                    Is.InRange(1, 72),
                    definition.id);

                Transform[] transforms =
                    view.Root.GetComponentsInChildren<
                        Transform>(true);
                for (int part = 0;
                    part < transforms.Length;
                    part++)
                {
                    string name =
                        transforms[part].name;
                    Assert.That(
                        name.StartsWith("Armor-") ||
                        name.StartsWith("Module-") ||
                        name == "Hull" ||
                        name == "UpperHull" ||
                        name == "Turret" ||
                        name == "Gun",
                        Is.False,
                        definition.id + "/" + name);
                }
                view.Destroy();
                yield return null;
            }
        }
    }
}
