using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class PattonFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "m46_patton",
            "m47_patton",
            "m48",
            "m60a1",
            "m60a2",
            "m60a3"
        };

        [Test]
        public void ProductionFamilyUsesCatalogArmorAndCommonServiceDetails()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            foreach (string id in ProductionIds)
            {
                VehicleDefinition definition =
                    catalog.GetVehicle(id);
                TankView view = Create(catalog, id);
                try
                {
                    Assert.That(
                        Count(view, "SideArmor"),
                        Is.EqualTo(0),
                        id);
                    Assert.That(
                        CountPrefix(view, "Armor-"),
                        Is.EqualTo(
                            FleetVisualAssertions
                                .ValidPlateCount(
                                    definition)),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Patton-EngineDeck"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Patton-EngineGrille"),
                        Is.EqualTo(4),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Patton-RearServiceLouvre"),
                        Is.EqualTo(id == "m48" ? 9 : 6),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Patton-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        view.Root
                            .GetComponentsInChildren<Collider>()
                            .Length,
                        Is.EqualTo(0),
                        id);
                }
                finally
                {
                    view.Destroy();
                }
            }
        }

        [Test]
        public void M46AndM47KeepDistinctCastTurretPackages()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView m46 = Create(catalog, "m46_patton");
            TankView m47 = Create(catalog, "m47_patton");
            try
            {
                Assert.That(
                    Count(m46, "Patton-M46-FenderMuffler"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        m46,
                        "Painted-Patton-M46-MufflerStrap"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(m46, "Patton-M46-M2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(m46, "Patton-M46-Antenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        m47,
                        "Painted-Patton-M47-RangefinderBlister"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(m47, "Patton-M47-FenderMuffler"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(m47, "Patton-M47-M2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(m47, "Patton-M47-Antenna"),
                    Is.EqualTo(2));
                AssertGunOwned(
                    m46,
                    "Painted-Patton-M46-SingleBaffleBrake");
                AssertGunOwned(
                    m47,
                    "Painted-Patton-M47-BlastDeflector");
            }
            finally
            {
                m46.Destroy();
                m47.Destroy();
            }
        }

        [Test]
        public void M48KeepsDualWeaponsAndBusyA5ServicePackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog, "m48");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M48-FenderBox"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Patton-M48-CommanderM2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Patton-M48-LoaderM2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Patton-M48-Antenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M48-Searchlight"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Patton-M48-BustleSideRail"),
                    Is.EqualTo(2));
                AssertGunOwned(
                    view,
                    "Painted-Patton-M48-MantletSightHousing");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void M60A1AndA3KeepTheirOwnModernizationPackages()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView a1 = Create(catalog, "m60a1");
            TankView a3 = Create(catalog, "m60a3");
            try
            {
                Assert.That(
                    Count(
                        a1,
                        "Painted-Patton-M60A1-SideCassette"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        a1,
                        "Painted-Patton-M60A1-GlacisCassette"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        a1,
                        "Painted-Patton-M60A1-CheekCassette"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        a1,
                        "Patton-M60A1-M19VisionBlock"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        a1,
                        "Patton-M60A1-M85-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a1,
                        "Patton-M60A1-M2-Receiver"),
                    Is.EqualTo(1));

                Assert.That(
                    Count(
                        a3,
                        "Painted-Patton-M60A3-SideCassette"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(
                        a3,
                        "Painted-Patton-M60A3-GlacisCassette"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        a3,
                        "Painted-Patton-M60A3-CheekCassette"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        a3,
                        "Painted-Patton-M60A3-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        a3,
                        "Painted-Patton-M60A3-TTS-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a3,
                        "Painted-Patton-M60A3-ThermalSleeveClamp"),
                    Is.EqualTo(3));
                Assert.That(
                    CountPrefix(
                        a3,
                        "Armor-m60a3_turret_era_"),
                    Is.EqualTo(66));
                AssertGunOwned(
                    a1,
                    "Painted-Patton-M60A1-Searchlight");
                AssertGunOwned(
                    a3,
                    "Painted-Patton-M60A3-Searchlight");
            }
            finally
            {
                a1.Destroy();
                a3.Destroy();
            }
        }

        [Test]
        public void M60A2KeepsStarshipLauncherAndHunterStation()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "m60a2",
                "digital");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-SideCassette"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-GlacisCassette"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-CheekCassette"),
                    Is.EqualTo(20));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-RaisedShoulder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-RWS-Body"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        view,
                        "Painted-Patton-M60A2-LauncherCollar"),
                    Is.EqualTo(3));
                AssertGunOwned(
                    view,
                    "Painted-Patton-M60A2-GunMask");
                AssertGunOwned(
                    view,
                    "Painted-Patton-M60A2-Searchlight");
                Renderer cassette =
                    Find(
                        view,
                        "Painted-Patton-M60A2-CheekCassette")
                    .GetComponent<Renderer>();
                Renderer weapon =
                    Find(
                        view,
                        "Patton-M60A2-RWS-M2-Receiver")
                    .GetComponent<Renderer>();
                Assert.That(
                    cassette.sharedMaterial.mainTexture,
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
            string id,
            string camouflage = "factory")
        {
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    "patton-" + id,
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
