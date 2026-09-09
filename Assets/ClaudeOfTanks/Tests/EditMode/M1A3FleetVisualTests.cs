using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class M1A3FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactHybridAbramsRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("m1a3");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(69));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(14));
                Assert.That(
                    FindAll(view, "RoadWheel-L")
                        .Select(item => item.localPosition.z)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            2.25f,
                            1.5f,
                            0.75f,
                            0f,
                            -0.75f,
                            -1.5f,
                            -2.25f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.46f, 0.96f, -3.42f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.46f, 0.88f, 3.27f)));
                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.78f).Within(0.0002f));
                Assert.That(track.max.x, Is.EqualTo(-1.14f).Within(0.0002f));
                Assert.That(track.min.y, Is.EqualTo(-0.0075f).Within(0.0002f));
                Assert.That(track.max.y, Is.EqualTo(1.4024f).Within(0.0002f));
                Assert.That(track.min.z, Is.EqualTo(-3.8624f).Within(0.0002f));
                Assert.That(track.max.z, Is.EqualTo(3.7124f).Within(0.0002f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsFacetedHybridHullAndCrewCapsule()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-M1A3-IntegratedGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A3-GlacisShoulder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-M1A3-SkirtCassette"),
                    Is.EqualTo(22));
                Assert.That(
                    Count(view, "Painted-M1A3-CrewCapsuleHatch"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-M1A3-HybridPlenum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "M1A3-HybridLouvre"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-M1A3-HullCageRail"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-M1A3-HullCagePost"),
                    Is.EqualTo(14));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsUnmannedTurretProtectionAndSensorSuite()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-M1A3-LowTurretCore"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A3-IntegratedCheek"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-M1A3-IsolatedBustle"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A3-BlowoffPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-M1A3-TurretSideModule"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "Painted-M1A3-HardKillLauncher"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "M1A3-RadarFace"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-M1A3-EoTower"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-M1A3-PanoramicHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A3-RwsYoke"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "M1A3-RwsReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "M1A3-NetworkAntenna"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-M1A3-SmokeLauncher"),
                    Is.EqualTo(12));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void GunPlantFollowsAuthored130MillimeterAxis()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Vector3 gun =
                    Find(view, "TurretRoot/Gun")
                        .localPosition;
                Assert.That(gun.x, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(gun.y, Is.EqualTo(0.28f).Within(0.0001f));
                Assert.That(gun.z, Is.EqualTo(3.605f).Within(0.0001f));
                AssertGunOwned(
                    view,
                    "Painted-M1A3-ThermalShroud");
                AssertGunOwned(
                    view,
                    "Painted-M1A3-BoreEvacuator");
                AssertGunOwned(
                    view,
                    "M1A3-MuzzleBore");
                AssertGunOwned(
                    view,
                    "M1A3-CoaxBarrel");
                Assert.That(
                    Find(view, "M1A3-MuzzleBore")
                        .localPosition.z,
                    Is.EqualTo(5.648f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellWithoutPresentationColliders()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    { "Hull", "UpperHull", "Turret", "Gun",
                      "Armor-track_L", "Armor-track_R" })
                    AssertHidden(view, name);
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-Abrams-"),
                    Is.EqualTo(0));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(
            ContentCatalog catalog)
        {
            VehicleDefinition definition =
                catalog.GetVehicle("m1a3");
            return TankView.Create(
                new TankState(
                    "m1a3-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static void AssertHidden(
            TankView view,
            string name)
        {
            Assert.That(
                Find(view, name)
                    .GetComponent<Renderer>()
                    .enabled,
                Is.False);
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
            Vector3 product =
                Vector3.Scale(
                    part.parent.localScale,
                    part.parent.parent.localScale);
            Assert.That(product.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(product.y, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(product.z, Is.EqualTo(1f).Within(0.0001f));
        }

        private static int Count(
            TankView view,
            string name)
        {
            return FindAll(view, name).Length;
        }

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item => item.name.StartsWith(prefix));
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Where(item => item.name == name)
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            if (name.Contains("/"))
                return view.Root.Find(name);
            return FindAll(view, name).First();
        }
    }
}
