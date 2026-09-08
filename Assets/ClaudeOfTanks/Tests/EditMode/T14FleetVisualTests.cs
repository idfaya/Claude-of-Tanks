using System.Collections.Generic;
using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T14FleetVisualTests
    {
        [Test]
        public void UsesOnlyCatalogArmorAndExactSevenWheelRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t14");
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
                    Is.EqualTo(101));
                Assert.That(
                    CountArmorKind(
                        view,
                        definition,
                        "era"),
                    Is.EqualTo(82));
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
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
        public void HullKeepsCrewCapsuleFullSkirtsAndRearServiceField()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-CapsuleHatch"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "T14-CapsulePeriscopeLens"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-FrontSkirtPanel"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "T14-RearScreenSlat"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-RearScreenSupport"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-EngineGrille"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T14-EngineLouvre"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-UnditchingLog"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T14-FrontMudguard"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void UnmannedTurretKeepsSensorsApsAndBothRoofWeapons()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-MoldedCrownCourse"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        view,
                        "T14-CheekSensorLens"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "T14-AfganitLauncher"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "T14-PrimaryRws30mmBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T14-RoofMachineGunBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T14-PanoramicWhip"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T14-MeteoMast"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T14-RearAntenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Find(
                        view,
                        "T14-PrimaryRws30mmBarrel")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Clean2A82FollowsGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "digital");
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-T14-2A82-ShroudChin");
                AssertGunOwned(
                    view,
                    "Painted-T14-2A82-ThermalSleeve");
                AssertGunOwned(
                    view,
                    "T14-2A82-MuzzleBore");
                Assert.That(
                    Count(
                        view,
                        "Painted-T14-2A82-ThermalSleeve"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "T14-2A82-ThermalClamp"),
                    Is.EqualTo(4));
                Assert.That(
                    CountPrefix(
                        view,
                        "T14-2A82-FumeExtractor"),
                    Is.EqualTo(0));

                Renderer painted = Find(
                        view,
                        "Painted-T14-FrontSkirtPanel")
                    .GetComponent<Renderer>();
                Renderer sensor = Find(
                        view,
                        "T14-CheekSensorLens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    sensor.sharedMaterial.mainTexture,
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

        private static int CountArmorKind(
            TankView view,
            VehicleDefinition definition,
            string kind)
        {
            HashSet<string> names =
                new HashSet<string>();
            AddPlateNames(
                names,
                definition.armor?.hullPlates,
                kind);
            AddPlateNames(
                names,
                definition.armor?.turretPlates,
                kind);
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item =>
                    item.name.StartsWith("Armor-") &&
                    names.Contains(
                        item.name.Substring(
                            "Armor-".Length)));
        }

        private static void AddPlateNames(
            HashSet<string> names,
            ArmorPlateDefinition[] plates,
            string kind)
        {
            if (plates == null) return;
            for (int index = 0;
                index < plates.Length;
                index++)
            {
                ArmorPlateDefinition plate =
                    plates[index];
                if (plate?.kind == kind)
                    names.Add(plate.name);
            }
        }

        private static TankView Create(
            ContentCatalog catalog,
            string camouflage = "factory")
        {
            VehicleDefinition definition =
                catalog.GetVehicle("t14");
            return TankView.Create(
                new TankState(
                    "t14-test",
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
