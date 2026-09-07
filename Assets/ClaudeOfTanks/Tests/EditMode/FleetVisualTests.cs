using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class FleetVisualTests
    {
        [Test]
        public void EveryProductionVehicleBuildsNativeUnityVisual()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            string[] ids = catalog.ProductionVehicleIds;
            int armorSurfaceCount = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                VehicleDefinition definition = catalog.GetVehicle(ids[i]);
                TankState tank = new TankState(
                    "visual-" + ids[i], Team.Alpha, definition.ToTankSpec(), Float3.Zero, 0f);
                TankView view = TankView.Create(tank, definition);
                try
                {
                    Assert.That(view.Root.Find("TurretRoot"), Is.Not.Null, ids[i]);
                    Assert.That(view.Root.GetComponentsInChildren<Renderer>().Length,
                        Is.GreaterThan(5), ids[i]);
                    Transform[] transforms = view.Root.GetComponentsInChildren<Transform>();
                    for (int child = 0; child < transforms.Length; child++)
                    {
                        if (transforms[child].name.StartsWith("Armor-")) armorSurfaceCount++;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(view.Root.gameObject);
                }
            }

            Assert.That(ids, Has.Length.EqualTo(126));
            Assert.That(armorSurfaceCount, Is.GreaterThan(1000));
        }
    }
}
