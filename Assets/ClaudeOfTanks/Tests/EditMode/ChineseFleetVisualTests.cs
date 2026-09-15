using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class ChineseFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "type59",
            "ztz85_iii",
            "type99a",
            "ztz99a2",
            "vt4a1"
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
                    string prefix = PrimaryPrefix(id);
                    Assert.That(Visible(view, "Hull"), Is.False, id);
                    Assert.That(Visible(view, "UpperHull"), Is.False, id);
                    Assert.That(Visible(view, "Turret"), Is.False, id);
                    Assert.That(Visible(view, "Gun"), Is.False, id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-" + prefix + "-HullLoft"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-" + prefix +
                            (id == "type59"
                                ? "-CastTurretShell"
                                : "-TurretShell")),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-" + prefix + "-MainGunTube"),
                        Is.EqualTo(1),
                        id);
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
                            "Painted-Chinese-EngineDeck"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Chinese-EngineFan"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Chinese-RearServiceGrille"),
                        Is.EqualTo(
                            id == "ztz85_iii"
                                ? 3
                                : 2),
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
        public void Type59KeepsCastGenerationRefit()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog, "type59");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-Type59-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Chinese-Type59-DShK-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Chinese-Type59-CommanderMG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-Type59-GlacisPanel"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-Type59-SkirtPanel"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-Type59-CheekCassette"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Chinese-Type59-Antenna"),
                    Is.EqualTo(2));
                AssertGunOwned(
                    view,
                    "Chinese-Type59-SearchlightDrum");
                AssertGunOwned(
                    view,
                    "Chinese-Type59-CoaxHousing");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Ztz85KeepsLongTurretAndCommandPackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog, "ztz85_iii");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-ZTZ85III-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Chinese-ZTZ85III-W85-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-ZTZ85III-ISFCS212-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Chinese-ZTZ85III-BustlePack"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Chinese-ZTZ85III-SideBasketRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Chinese-ZTZ85III-SideBasketPost"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        view,
                        "Chinese-ZTZ85III-RadioMast"),
                    Is.EqualTo(1));
                AssertGunOwned(
                    view,
                    "Painted-Chinese-ZTZ85III-GunMask");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Type99VariantsKeepDistinctRoofAndBustlePackages()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView type99 = Create(catalog, "type99a");
            TankView a2 = Create(catalog, "ztz99a2");
            TankView vt = Create(catalog, "vt4a1");
            try
            {
                Assert.That(
                    Count(
                        type99,
                        "Painted-Chinese-Type99A-SmokeLauncher"),
                    Is.EqualTo(20));
                Assert.That(
                    Count(
                        type99,
                        "Painted-Chinese-Type99A-GunnerTower-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        type99,
                        "Chinese-Type99A-QJC88-Receiver"),
                    Is.EqualTo(1));

                Assert.That(
                    Count(
                        a2,
                        "Painted-Chinese-ZTZ99A2-SmokeLauncher"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        a2,
                        "Painted-Chinese-ZTZ99A2-RearServiceCabinet"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        a2,
                        "Chinese-ZTZ99A2-RearServiceLouvre"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        a2,
                        "Chinese-ZTZ99A2-W85-Receiver"),
                    Is.EqualTo(1));

                Assert.That(
                    Count(
                        vt,
                        "Painted-Chinese-VT4A1-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        vt,
                        "Painted-Chinese-VT4A1-RWS-Body"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        vt,
                        "Chinese-VT4A1-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                        Count(
                            vt,
                            "Chinese-VT4A1-WarningLens"),
                        Is.EqualTo(4));
                Assert.That(
                    Count(
                        vt,
                        "Painted-Chinese-VT4A1-BustleCase"),
                    Is.EqualTo(2));

                Assert.That(
                    CountPrefix(
                        type99,
                        "Painted-Chinese-ZTZ99A2-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(
                        a2,
                        "Painted-Chinese-VT4A1-"),
                    Is.EqualTo(0));
            }
            finally
            {
                type99.Destroy();
                a2.Destroy();
                vt.Destroy();
            }
        }

        [Test]
        public void Vt4UsesCamoAndAllModernGunsStayScaled()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "vt4a1",
                "summer");
            try
            {
                Renderer hull = view.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    view,
                    "Painted-Chinese-VT4A1-RWS-Body")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    view,
                    "Chinese-VT4A1-RWS-Sensor-Lens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.SameAs(
                        hull.sharedMaterial.mainTexture));
                Assert.That(
                    lens.sharedMaterial.mainTexture,
                    Is.Null);
                foreach (string name in new[]
                {
                    "Painted-Chinese-Type99A-GunMask",
                    "Painted-Chinese-ZTZ99A2-GunMask",
                    "Painted-Chinese-VT4A1-GunMask"
                })
                {
                    string id =
                        name.Contains("Type99A")
                            ? "type99a"
                            : name.Contains("ZTZ99A2")
                                ? "ztz99a2"
                                : "vt4a1";
                    TankView modern = id == "vt4a1"
                        ? view
                        : Create(catalog, id);
                    try
                    {
                        AssertGunOwned(modern, name);
                    }
                    finally
                    {
                        if (modern != view)
                            modern.Destroy();
                    }
                }
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
                    "chinese-" + id,
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

        private static bool Visible(
            TankView view,
            string name)
        {
            Renderer renderer = view.Root
                .GetComponentsInChildren<Renderer>(true)
                .FirstOrDefault(item => item.name == name);
            return renderer != null && renderer.enabled;
        }

        private static string PrimaryPrefix(string id)
        {
            if (id == "type59") return "Chinese-Type59";
            if (id == "ztz85_iii") return "Chinese-ZTZ85III";
            if (id == "type99a") return "Chinese-Type99A";
            if (id == "ztz99a2") return "Chinese-ZTZ99A2";
            return "Chinese-VT4A1";
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
