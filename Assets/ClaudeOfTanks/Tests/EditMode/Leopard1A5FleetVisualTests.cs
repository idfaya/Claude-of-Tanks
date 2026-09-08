using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Leopard1A5FleetVisualTests
    {
        [Test]
        public void UsesOnlyCatalogArmorAndNoGenericSideArmor()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("leo1a5");
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
                    Is.EqualTo(39));
                Assert.That(
                    Count(view, "SideArmor"),
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

        [Test]
        public void HullKeepsFenderApronDeckAndFuelCanIdentity()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-ContinuousFender"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-RubberApron"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-FenderLocker"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-RearFuelCan"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-FuelCanCarrier"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-EngineIntake"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-IntakeLouvre"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-EngineFan"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-RearLouvre"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-DriverPeriscopeLens"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-SpareTrackLink"),
                    Is.EqualTo(7));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void TurretKeepsA5OpticsWeaponsAndLoadedBasket()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-BVCheekFront"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-BVCheekReturn"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-EMES18-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-EMES18-Aperture"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-CommanderVisionBlock"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-SmokeLauncher"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-MG3-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-MG3-Shield"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-BustlePost"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-BustleCargo"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-SideRackRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-Antenna"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void L7A3FittingsFollowGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "digital");
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-Leopard1A5-MantletRearPad");
                AssertGunOwned(
                    view,
                    "Painted-Leopard1A5-MantletCore");
                AssertGunOwned(
                    view,
                    "Painted-Leopard1A5-L7A3-ThermalSleeve");
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-MantletWing"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-L7A3-FumeExtractor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Leopard1A5-L7A3-Cinch"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-Leopard1A5-L7A3-MRS"),
                    Is.EqualTo(1));
                Assert.That(
                    Find(
                            view,
                            "Painted-Leopard1A5-MantletReceiver")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));

                Renderer painted = Find(
                        view,
                        "Painted-Leopard1A5-BVCheekFront")
                    .GetComponent<Renderer>();
                Renderer weapon = Find(
                        view,
                        "Leopard1A5-MG3-Receiver")
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
                catalog.GetVehicle("leo1a5");
            return TankView.Create(
                new TankState(
                    "leopard1a5-test",
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
