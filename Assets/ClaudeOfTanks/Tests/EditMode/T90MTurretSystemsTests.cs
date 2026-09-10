using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90MTurretSystemsTests
    {
        [TestCase("t90m")]
        [TestCase("t90m_proryv")]
        public void BuildsFinalAttachedBustle(string id)
        {
            TankView view = Create(id);
            try
            {
                Mesh magazine =
                    Find(view, "Painted-T90M-BustleMagazine")
                        .GetComponent<MeshFilter>()
                        .sharedMesh;
                Assert.That(magazine.vertexCount, Is.EqualTo(132));
                Assert.That(
                    Count(view, "T90M-BustleServiceLid"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-T90M-BustleSideBin"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90M-BustleSideRail"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90M-BustleSidePost"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90M-BustleRearRail"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90M-BustleRearPost"),
                    Is.EqualTo(5));

                Assert.That(
                    Count(view, "T90M-BustleRearLouvre"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90M-BustleRearReturn"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90M-BustleRearCylinder"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90M-BustleRearCylinderStrap"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90M-BustleRearCylinderCradle"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90M-BustleRearCylinderReturn"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90M-BustleRearUpperCradle"),
                    Is.EqualTo(2));

                Assert.That(
                    Count(view, "Painted-T90M-BustleShoulder"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-T90M-BustleShoulderPanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(view, "T90M-BustleCageSideRail"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90M-BustleCageSidePost"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90M-BustleCageRearRail"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-T90M-BustleTopStore"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90M-BustleCanvasRoll"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90M-BustleCanvasStrap"),
                    Is.EqualTo(6));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition = catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-turret-systems-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .First(item => item.name == name);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == name);
        }
    }
}
