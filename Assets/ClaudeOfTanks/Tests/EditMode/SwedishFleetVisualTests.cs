using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class SwedishFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "strv81",
            "udes03",
            "strv103a",
            "strv103",
            "cv90",
            "strv122",
            "cv90_mkiv"
        };

        [Test]
        public void ProductionFamilyUsesCatalogArmorAndOwnedPackages()
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
                            "Swedish-RearServiceGrille"),
                        Is.EqualTo(
                            id == "strv122"
                                ? 3
                                : 2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Swedish-EngineDeck"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(view, "Swedish-EngineFan"),
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
        public void SiegeLineKeepsHullOwnedDistinctPackages()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView udes = Create(catalog, "udes03");
            TankView a = Create(catalog, "strv103a");
            TankView b = Create(catalog, "strv103");
            try
            {
                Assert.That(
                    Count(
                        udes,
                        "Painted-Swedish-UDES03-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a,
                        "Painted-Swedish-Strv103A-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        b,
                        "Painted-Swedish-Strv103B-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        udes,
                        "Swedish-UDES03-FixedGunAssembly"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a,
                        "Swedish-Strv103A-FixedGunAssembly"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        b,
                        "Swedish-Strv103B-FixedGunAssembly"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        udes,
                        "Painted-Swedish-UDES03-HydraulicRam"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        udes,
                        "Swedish-UDES03-HydraulicRod"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        udes,
                        "Swedish-UDES03-GlacisLouvre"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        udes,
                        "Swedish-UDES03-Antenna"),
                    Is.EqualTo(1));

                Assert.That(
                    Count(
                        a,
                        "Swedish-Strv103A-GlacisLouvre"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        a,
                        "Swedish-Strv103A-Antenna"),
                    Is.EqualTo(2));
                Assert.That(
                    CountPrefix(
                        a,
                        "Painted-Swedish-Strv103B-"),
                    Is.EqualTo(0));

                Assert.That(
                    Count(
                        b,
                        "Painted-Swedish-Strv103B-DozerBlade"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        b,
                        "Swedish-Strv103B-DozerArm"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        b,
                        "Swedish-Strv103B-NoseFenceRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        b,
                        "Swedish-Strv103B-NoseFencePost"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(
                        b,
                        "Swedish-Strv103B-Antenna"),
                    Is.EqualTo(2));

                foreach (TankView view in
                    new[] { udes, a, b })
                {
                    Assert.That(
                        Visible(view, "Hull"),
                        Is.False);
                    Assert.That(
                        Visible(view, "UpperHull"),
                        Is.False);
                    Assert.That(
                        Visible(view, "Turret"),
                        Is.False);
                    Assert.That(
                        Visible(view, "Gun"),
                        Is.False);
                    Transform collar = Find(
                        view,
                        "Painted-Swedish-FixedGunCollar");
                    Assert.That(
                        collar.parent,
                        Is.SameAs(view.Root));
                    Transform turret = view.Root.Find(
                        "TurretRoot");
                    Assert.That(
                        turret.GetComponentsInChildren<Transform>()
                            .Count(item =>
                                item.name.StartsWith(
                                    "Swedish-")),
                        Is.EqualTo(0));
                }
            }
            finally
            {
                udes.Destroy();
                a.Destroy();
                b.Destroy();
            }
        }

        [Test]
        public void TurretedTanksKeepSwedishAndDonorIdentity()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView strv81 = Create(catalog, "strv81");
            TankView strv122 = Create(catalog, "strv122");
            try
            {
                Assert.That(Visible(strv81, "Hull"), Is.False);
                Assert.That(Visible(strv81, "UpperHull"), Is.False);
                Assert.That(Visible(strv81, "Turret"), Is.False);
                Assert.That(Visible(strv81, "Gun"), Is.False);
                Assert.That(
                    Count(
                        strv81,
                        "Painted-Swedish-Strv81-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        strv81,
                        "Painted-Swedish-Strv81-CastTurretShell"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        strv81,
                        "Painted-Swedish-Strv81-20PdrGunTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        strv81,
                        "Painted-Swedish-Strv81-SmokeLauncher"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        strv81,
                        "Swedish-Strv81-Antenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        strv81,
                        "Swedish-Strv81-Ksp58-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        strv81,
                        "Swedish-Strv81-VentilatorVane"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        strv81,
                        "Swedish-Strv81-RearBasketRail"),
                    Is.EqualTo(10));
                AssertGunOwned(
                    strv81,
                    "Painted-Swedish-Strv81-GunMask");

                Assert.That(
                    Count(
                        strv122,
                        "Painted-Leopard-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        strv122,
                        "Painted-Leopard-Hatch"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        strv122,
                        "Swedish-Strv122-Ksp58-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        strv122,
                        "Swedish-Strv122-CupolaVision"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        strv122,
                        "Swedish-Strv122-RearBasketRail"),
                    Is.EqualTo(13));
                AssertGunOwned(
                    strv122,
                    "Painted-Swedish-Strv122-GunMask");
            }
            finally
            {
                strv81.Destroy();
                strv122.Destroy();
            }
        }

        [Test]
        public void Cv90VariantsOwnRwsAndOnlyMkivOwnsMissiles()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView cv90 = Create(catalog, "cv90");
            TankView mkiv = Create(catalog, "cv90_mkiv");
            try
            {
                foreach (TankView view in new[] { cv90, mkiv })
                {
                    Assert.That(Visible(view, "Hull"), Is.False);
                    Assert.That(Visible(view, "UpperHull"), Is.False);
                    Assert.That(Visible(view, "Turret"), Is.False);
                    Assert.That(Visible(view, "Gun"), Is.False);
                }
                Assert.That(
                    Count(
                        cv90,
                        "Painted-Swedish-CV90-MissionCell"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        cv90,
                        "Painted-Swedish-CV90-TurretShell"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        cv90,
                        "Painted-Swedish-CV90-MainGunTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-MissionCell"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-TurretShell"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-MainGunTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(cv90, "MissilePod"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(mkiv, "MissilePod"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        cv90,
                        "Painted-Swedish-CV90-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        cv90,
                        "Painted-Swedish-CV90-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        cv90,
                        "Swedish-CV90-RWS-YokeArm"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        mkiv,
                        "Swedish-CV90MkIV-RWS-FeedBelt"),
                    Is.EqualTo(4));
                Assert.That(
                    CountPrefix(
                        cv90,
                        "Painted-Swedish-CV90MkIV-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-MissilePod"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        mkiv,
                        "Swedish-CV90MkIV-MissileCell"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-FuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        mkiv,
                        "Painted-Swedish-CV90MkIV-SensorHead-Housing"),
                    Is.EqualTo(4));
            }
            finally
            {
                cv90.Destroy();
                mkiv.Destroy();
            }
        }

        [Test]
        public void Cv90MkivUsesCamoAndScaledGunOwnership()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "cv90_mkiv",
                "summer");
            try
            {
                Renderer hull = view.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    view,
                    "Painted-Swedish-CV90MkIV-RWS-Receiver")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    view,
                    "Swedish-CV90MkIV-RWS-Sensor-Lens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.SameAs(
                        hull.sharedMaterial.mainTexture));
                Assert.That(
                    lens.sharedMaterial.mainTexture,
                    Is.Null);
                AssertGunOwned(
                    view,
                    "Painted-Swedish-CV90MkIV-GunShroud");
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
                    "swedish-" + id,
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

        private static bool Visible(
            TankView view,
            string name)
        {
            Renderer renderer = view.Root
                .GetComponentsInChildren<Renderer>(true)
                .FirstOrDefault(item => item.name == name);
            return renderer != null && renderer.enabled;
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
