using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class ItalianFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "carro45t",
            "ariete",
            "ariete_c1",
            "ariete_c2"
        };

        [Test]
        public void ProductionFamilyUsesCatalogArmorAndCommonFittings()
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
                            "Painted-Italian-SmokeLauncher"),
                        Is.EqualTo(
                            id == "carro45t"
                                ? 10
                                : 8),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Italian-SmokeBankShoe"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Italian-Antenna"),
                        Is.EqualTo(
                            id == "ariete_c1"
                                ? 1
                                : 2),
                        id);
                    Assert.That(
                        Count(view, "Painted-Italian-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Italian-RearServiceGrille"),
                        Is.EqualTo(
                            id == "carro45t"
                                ? 3
                                : 2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Italian-RearServiceSlat"),
                        Is.EqualTo(
                            id == "carro45t"
                                ? 12
                                : 10),
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
        public void Carro45TRetainsRoofRackAndGunPlant()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog, "carro45t");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-CommanderVision"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-LoaderVision"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-Breda-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-CrownRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-CrownRailPost"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-CrownCrossRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-RearRackRail"),
                    Is.EqualTo(8));
                AssertGunOwned(
                    view,
                    "Painted-Italian-Carro45T-GunMask");
                Assert.That(
                    Count(
                        view,
                        "Italian-Carro45T-Coax-Barrel"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ArieteMarksKeepDistinctRoofWeapons()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView baseAriete =
                Create(catalog, "ariete");
            TankView c1 =
                Create(catalog, "ariete_c1");
            TankView c2 =
                Create(catalog, "ariete_c2");
            try
            {
                foreach (TankView view in
                    new[] { baseAriete, c1, c2 })
                {
                    Assert.That(
                        Count(
                            view,
                            "Painted-Italian-Ariete-TURMS-Housing"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Italian-Ariete-PanoramicSight-Housing"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Italian-Ariete-CupolaVision"),
                        Is.EqualTo(6));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Italian-Ariete-GalixPlatform"),
                        Is.EqualTo(2));
                    Assert.That(
                        Count(
                            view,
                            "Italian-Ariete-RoofServicePanel"),
                        Is.EqualTo(5));
                }
                Assert.That(
                    Count(
                        baseAriete,
                        "Italian-Ariete-M2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        c1,
                        "Italian-ArieteC1-CommanderMAG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        c1,
                        "Italian-ArieteC1-LoaderMAG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(
                        c1,
                        "Painted-Italian-ArieteC2-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        c2,
                        "Italian-ArieteC2-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        c2,
                        "Painted-Italian-ArieteC2-APU"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        c2,
                        "Painted-Italian-ArieteC2-DriverThermal"),
                    Is.EqualTo(1));
            }
            finally
            {
                baseAriete.Destroy();
                c1.Destroy();
                c2.Destroy();
            }
        }

        [Test]
        public void C1AndC2KeepSeatedSideRackPackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            foreach (string id in
                new[] { "ariete_c1", "ariete_c2" })
            {
                VehicleDefinition definition =
                    catalog.GetVehicle(id);
                TankView view = Create(catalog, id);
                try
                {
                    Assert.That(
                        Count(
                            view,
                            "Italian-Ariete-SideRackRail"),
                        Is.EqualTo(4),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Italian-Ariete-SideRackArm"),
                        Is.EqualTo(6),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Italian-Ariete-SidePanel"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Italian-Ariete-BasketBridge"),
                        Is.EqualTo(2),
                        id);
                    Transform panel = Find(
                        view,
                        "Painted-Italian-Ariete-SidePanel");
                    Assert.That(
                        Mathf.Abs(panel.localPosition.x),
                        Is.EqualTo(
                            definition.dims.widthM *
                            0.31f)
                            .Within(0.0001f),
                        id);
                    AssertGunOwned(
                        view,
                        id == "ariete_c2"
                            ? "Painted-Italian-ArieteC2-GunMask"
                            : "Painted-Italian-ArieteC1-GunMask");
                }
                finally
                {
                    view.Destroy();
                }
            }
        }

        [Test]
        public void C2RetainsCatalogEraAndPaintedFittingsUseCamo()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("ariete_c2");
            TankView view = Create(
                catalog,
                "ariete_c2",
                "summer");
            try
            {
                Assert.That(
                    EraCount(definition),
                    Is.EqualTo(100));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(124));
                Assert.That(
                    CountPrefix(view, "ERA"),
                    Is.EqualTo(0));

                Renderer hull = view.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    view,
                    "Painted-Italian-ArieteC2-RWS-Body")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    view,
                    "Italian-ArieteC2-RWS-Lens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.SameAs(
                        hull.sharedMaterial.mainTexture));
                Assert.That(
                    lens.sharedMaterial.mainTexture,
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

        private static int EraCount(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .Concat(
                    definition.armor.turretPlates)
                .Count(plate =>
                    plate.kind == "era");
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
                    "italian-" + id,
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
