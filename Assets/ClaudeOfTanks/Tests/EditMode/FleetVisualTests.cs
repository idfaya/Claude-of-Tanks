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
            int trackTriangleCount = 0;
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
                        Is.GreaterThan(30), ids[i]);
                    Transform[] transforms = view.Root.GetComponentsInChildren<Transform>();
                    int roadWheels = 0;
                    int suspensionArms = 0;
                    int suspensionJoints = 0;
                    int returnRollers = 0;
                    int sprockets = 0;
                    int idlers = 0;
                    int trackRuns = 0;
                    for (int child = 0; child < transforms.Length; child++)
                    {
                        if (transforms[child].name.StartsWith("Armor-")) armorSurfaceCount++;
                        if (transforms[child].name.StartsWith("RoadWheel-")) roadWheels++;
                        if (transforms[child].name.StartsWith("SuspensionArm-")) suspensionArms++;
                        if (transforms[child].name.StartsWith("SuspensionJoint-")) suspensionJoints++;
                        if (transforms[child].name.StartsWith("ReturnRoller-")) returnRollers++;
                        if (transforms[child].name.StartsWith("Sprocket-") &&
                            !transforms[child].name.EndsWith("-Hub")) sprockets++;
                        if (transforms[child].name.StartsWith("Idler-") &&
                            !transforms[child].name.EndsWith("-Hub")) idlers++;
                        if (transforms[child].name.StartsWith("TrackLinks-"))
                        {
                            trackRuns++;
                            MeshFilter filter = transforms[child].GetComponent<MeshFilter>();
                            trackTriangleCount += filter.sharedMesh.triangles.Length / 3;
                        }
                    }
                    Assert.That(roadWheels, Is.GreaterThanOrEqualTo(8), ids[i]);
                    Assert.That(suspensionArms, Is.EqualTo(roadWheels), ids[i]);
                    Assert.That(suspensionJoints, Is.EqualTo(roadWheels), ids[i]);
                    Assert.That(returnRollers, Is.GreaterThanOrEqualTo(4), ids[i]);
                    Assert.That(sprockets, Is.EqualTo(2), ids[i]);
                    Assert.That(idlers, Is.EqualTo(2), ids[i]);
                    Assert.That(trackRuns, Is.EqualTo(2), ids[i]);
                    Assert.That(view.Root.GetComponentsInChildren<Collider>(), Is.Empty, ids[i]);
                }
                finally
                {
                    view.Destroy();
                }
            }

            Assert.That(ids, Has.Length.EqualTo(126));
            Assert.That(armorSurfaceCount, Is.GreaterThan(1000));
            Assert.That(trackTriangleCount, Is.GreaterThan(150000));
        }

        [Test]
        public void TankViewCleanupToleratesDestroyedParentHierarchy()
        {
            VehicleDefinition definition = ContentCatalog.Load().GetVehicle("m1a2");
            TankView view = TankView.Create(
                new TankState(
                    "cleanup",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition);
            Object.DestroyImmediate(view.Root.gameObject);

            Assert.DoesNotThrow(view.Destroy);
        }
    }
}
