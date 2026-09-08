using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Mbt70FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactSevenWheelRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("mbt70");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(29));
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-HullEraCassette"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-TurretEraCassette"),
                    Is.EqualTo(6));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(14));
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

        [Test]
        public void HullKeepsSkirtlessFendersClosuresAndDonorDeck()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-FrontFender"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-RearFender"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "MBT70-RubRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-RearShoulderWall"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-RearShoulderShelf"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "MBT70-RearClosureStrut"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "MBT70-EngineGrille"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-EngineLouvre"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        view,
                        "MBT70-DriverPeriscopeLens"),
                    Is.EqualTo(3));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void TurretKeepsOpticsWeaponsAndLoadedBustle()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-BustleRoofDoor"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "MBT70-CommanderVisionBlock"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-GunnerSightHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "MBT70-M2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-SignatureSmokeCanister"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "MBT70-BasketPost"),
                    Is.EqualTo(13));
                Assert.That(
                    Count(
                        view,
                        "MBT70-BustleRackRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-BustleJerryCan"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "MBT70-BustleTowCable"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "MBT70-SpareTrackLink"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "MBT70-Antenna"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Xm150FittingsFollowGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "digital");
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-MBT70-ParabolicShieldCourse");
                AssertGunOwned(
                    view,
                    "Painted-MBT70-XM150-ThermalSleeve");
                AssertGunOwned(
                    view,
                    "MBT70-XM150-SensorClamp");
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-ParabolicShieldCourse"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-XM150-FumeExtractor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-MBT70-XM150-SensorHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "MBT70-XM150-MuzzleBore"),
                    Is.EqualTo(1));

                Renderer painted = Find(
                        view,
                        "Painted-MBT70-TurretEraCassette")
                    .GetComponent<Renderer>();
                Renderer weapon = Find(
                        view,
                        "MBT70-M2-Receiver")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    weapon.sharedMaterial.mainTexture,
                    Is.Null);
            }
            finally
            {
                view.Destroy();
            }
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
            Vector3 product = Vector3.Scale(
                part.parent.localScale,
                part.parent.parent.localScale);
            Assert.That(
                product.x,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                product.y,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                product.z,
                Is.EqualTo(1f).Within(0.0001f));
        }

        private static TankView Create(
            ContentCatalog catalog,
            string camouflage = "factory")
        {
            VehicleDefinition definition =
                catalog.GetVehicle("mbt70");
            return TankView.Create(
                new TankState(
                    "mbt70-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                camouflage,
                "forest",
                catalog);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item => item.name == name);
        }

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item =>
                    item.name.StartsWith(prefix));
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .First(item => item.name == name);
        }
    }
}
