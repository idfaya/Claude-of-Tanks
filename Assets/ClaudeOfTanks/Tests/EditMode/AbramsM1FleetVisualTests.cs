using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class AbramsM1FleetVisualTests
    {
        [TestCase("m1a1")]
        [TestCase("m1a1ha")]
        public void UsesCatalogArmorAndSevenWheelRearDriveCourse(
            string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            TankView view = Create(catalog, id);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(32));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(14));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.425f, 1.1f, -3.28f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.425f, 0.85f, 3.02f)));
                Assert.That(
                    FindAll(view, "RoadWheel-L")
                        .Select(item => item.localPosition.z)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            2.19f,
                            1.46f,
                            0.73f,
                            0f,
                            -0.73f,
                            -1.46f,
                            -2.19f
                        })
                        .Within(0.0001f));
                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.715f).Within(0.002f));
                Assert.That(track.max.x, Is.EqualTo(-1.135f).Within(0.002f));
                Assert.That(track.min.y, Is.EqualTo(-0.002f).Within(0.03f));
                Assert.That(track.max.y, Is.EqualTo(1.5097f).Within(0.03f));
                Assert.That(track.min.z, Is.EqualTo(-3.6879f).Within(0.002f));
                Assert.That(track.max.z, Is.EqualTo(3.4494f).Within(0.002f));
                foreach (string name in new[]
                    { "Hull", "UpperHull", "Turret", "Gun",
                      "Armor-track_L", "Armor-track_R" })
                    AssertHidden(view, name);
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void FamilyKeepsTejasHullTurretAndEquipment()
        {
            TankView view =
                Create(ContentCatalog.Load(), "m1a1");
            try
            {
                Assert.That(
                    Count(view, "Painted-AbramsM1-LongGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-AbramsM1-SideSkirt"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "AbramsM1-EngineDeckGrille"),
                    Is.EqualTo(9));
                Assert.That(
                    Count(view, "Painted-AbramsM1-DeepBustle"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "AbramsM1-BlowoffSeam"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-Abrams-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "AbramsM1-GunnerSightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "AbramsM1-CommanderMgReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "AbramsM1-LoaderMgReceiver"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void HeavyArmorKeepsDistinctProtectionOpticAndSearchlight()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView baseView = Create(catalog, "m1a1");
            TankView heavy = Create(catalog, "m1a1ha");
            try
            {
                Assert.That(
                    Count(baseView, "Painted-AbramsM1-SideSkirt"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(baseView, "Painted-AbramsM1Ha-SideModule"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(heavy, "Painted-AbramsM1Ha-SideModule"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(baseView, "AbramsM1-CommanderWindow"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(heavy, "AbramsM1Ha-CommanderWindow"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(heavy, "Painted-AbramsM1Ha-MgShield"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(baseView, "AbramsM1-TowCable"),
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(baseView, "AbramsM1-TowCable")
                        .All(item => item.localPosition.x < 0f),
                    Is.True);
                Assert.That(
                    Count(heavy, "AbramsM1Ha-TowCable"),
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(heavy, "AbramsM1Ha-TowCable")
                        .All(item => item.localPosition.x > 0f),
                    Is.True);
                AssertGunOwned(
                    heavy,
                    "Painted-AbramsM1Ha-Searchlight");
                AssertGunOwned(
                    heavy,
                    "AbramsM1Ha-SearchlightLens");
            }
            finally
            {
                baseView.Destroy();
                heavy.Destroy();
            }
        }

        [TestCase("m1a1", 0.3f)]
        [TestCase("m1a1ha", 0.3442623f)]
        public void M256PlantFollowsGun(
            string id,
            float gunY)
        {
            TankView view =
                Create(ContentCatalog.Load(), id);
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-AbramsM1-M256Barrel");
                AssertGunOwned(
                    view,
                    "AbramsM1-MuzzleBore");
                AssertGunOwned(
                    view,
                    "AbramsM1-CoaxBarrel");
                Vector3 gun =
                    Find(view, "TurretRoot/Gun")
                        .localPosition;
                Assert.That(gun.x, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(gun.y, Is.EqualTo(gunY).Within(0.0001f));
                Assert.That(gun.z, Is.EqualTo(3.39f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(
            ContentCatalog catalog,
            string id)
        {
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-test",
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
            Assert.That(part.parent.parent.name, Is.EqualTo("Gun"));
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
