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

        [TestCase("t90m")]
        [TestCase("t90m_proryv")]
        public void BuildsFinalRoofEquipmentAndRemovesFallback(string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(
                    Count(view, "Painted-T90M-CommanderCupola"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90M-GunnerCupola"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90M-SosnaHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90M-SosnaLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90M-RoofPeriscopeSlot"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90M-RoofPeriscopeGlass"),
                    Is.EqualTo(5));

                Assert.That(
                    Count(view, "Painted-T90M-KordRace"),
                    Is.EqualTo(1));
                Transform kord = Find(view, "T90M-RemoteKord");
                Assert.That(
                    kord.localScale.y,
                    Is.EqualTo(1f / 0.65f).Within(0.0001f));
                Assert.That(
                    Count(view, "T90M-Left-Detail-SmokeLauncher") +
                    Count(view, "T90M-Right-Detail-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90M-0-AntennaWhip") +
                    Count(view, "T90M-1-AntennaWhip"),
                    Is.EqualTo(2));

                Assert.That(
                    Count(view, "Painted-T90M-SearchlightHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90M-SearchlightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90M-CommanderRoofCollar"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90M-GunnerRoofCollar"),
                    Is.EqualTo(2));
                Assert.That(
                    FindAll(view, "Painted-T90M-CommanderRoofCollar")
                        .Select(item => item.localPosition.y),
                    Is.All.EqualTo(0.995f).Within(0.0001f));
                Assert.That(
                    FindAll(view, "Painted-T90M-GunnerRoofCollar")
                        .Select(item => item.localPosition.y),
                    Is.All.EqualTo(0.985f).Within(0.0001f));
                Assert.That(
                    Count(view, "T90M-FinalRoofPeriscopeSlot"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-T90M-RoofEquipmentBox"),
                    Is.EqualTo(6));

                Assert.That(
                    view.Root.GetComponentsInChildren<Transform>(true)
                        .Count(item =>
                            item.name.StartsWith("Soviet-") ||
                            item.name.StartsWith("Painted-Soviet-")),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t90m")]
        [TestCase("t90m_proryv")]
        public void BuildsFinalTwoA46M5Gun(string id)
        {
            TankView view = Create(id);
            try
            {
                Transform assembly =
                    Find(view, "T90M-2A46M5Assembly");
                Assert.That(
                    assembly.localPosition,
                    Is.EqualTo(new Vector3(0f, 0.21f, 1.15f)));
                Assert.That(
                    assembly.localScale,
                    Is.EqualTo(new Vector3(
                        1f / 0.95f,
                        1f / 0.65f,
                        1f)));
                Assert.That(
                    Find(view, "Gun")
                        .GetComponent<Renderer>().enabled,
                    Is.False);

                Assert.That(
                    Count(view, "Painted-T90M-2A46M5Saddle"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90M-2A46M5RootCone"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90M-GunBootSection"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90M-GunBootCrease"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90M-GunBootClamp"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90M-CoaxPort"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90M-CoaxPortWasher"),
                    Is.EqualTo(1));

                Assert.That(
                    Count(view, "Painted-T90M-2A46M5ThermalSleeve"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-T90M-2A46M5ForwardTube"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(view, "T90M-2A46M5SleeveRing"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-T90M-2A46M5EvacuatorCrest"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90M-2A46M5MuzzleCollar"),
                    Is.EqualTo(1));
                Transform rim =
                    Find(view, "Painted-T90M-2A46M5MuzzleRim");
                Assert.That(
                    rim.localPosition,
                    Is.EqualTo(new Vector3(0f, 0f, 4.9145f)));
                Assert.That(
                    Count(view, "T90M-2A46M5MuzzleBoreDisc"),
                    Is.EqualTo(1));
                Assert.That(
                    Find(
                            view,
                            "Painted-T90M-2A46M5ThermalSleeve")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds.extents.x,
                    Is.EqualTo(0.108f).Within(0.0001f));
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
            return FindAll(view, name).Count();
        }

        private static System.Collections.Generic.IEnumerable<Transform>
            FindAll(TankView view, string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name == name);
        }
    }
}
