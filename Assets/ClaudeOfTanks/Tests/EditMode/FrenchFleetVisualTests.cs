using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class FrenchFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "amx30",
            "amx30b2",
            "amx40",
            "leclerc",
            "leclerc_xlr",
            "amx56"
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
                            "Painted-French-SmokeLauncher"),
                        Is.EqualTo(
                            SmokeCount(id)),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-French-SmokeBankShoe"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "French-Antenna"),
                        Is.EqualTo(
                            AntennaCount(id)),
                        id);
                    Assert.That(
                        Count(view, "Painted-French-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "French-RearServiceGrille"),
                        Is.EqualTo(
                            IsAmx30(id) ? 2 : 3),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "French-RearServiceSlat"),
                        Is.EqualTo(
                            IsAmx30(id) ? 8 : 15),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-French-EngineDeck"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(view, "French-EngineFan"),
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
        public void Amx30VariantsKeepCastRoofAndGunOwnedOptics()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView amx30 = Create(catalog, "amx30");
            TankView b2 = Create(catalog, "amx30b2");
            try
            {
                foreach (TankView view in
                    new[] { amx30, b2 })
                {
                    Assert.That(
                        Count(
                            view,
                            "French-AMX30-CupolaVision"),
                        Is.EqualTo(10));
                    Assert.That(
                        Count(
                            view,
                            "French-AMX30-AANF1-Receiver"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "French-AMX30-M693-Barrel"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Painted-French-AMX30-PH8B-Housing"),
                        Is.EqualTo(1));
                    Assert.That(
                        Find(
                            view,
                            "Painted-French-AMX30-GunMask")
                            .parent.parent.name,
                        Is.EqualTo("Gun"));
                }
                Assert.That(
                    CountPrefix(
                        amx30,
                        "Painted-French-AMX30B2-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        b2,
                        "Painted-French-AMX30B2-COTAC-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        b2,
                        "Painted-French-AMX30B2-LLLTV-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(amx30, "Armor-"),
                    Is.EqualTo(19));
                Assert.That(
                    CountPrefix(b2, "Armor-"),
                    Is.EqualTo(91));
            }
            finally
            {
                amx30.Destroy();
                b2.Destroy();
            }
        }

        [Test]
        public void Amx40KeepsSeatedExternalPackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("amx40");
            TankView view = Create(catalog, "amx40");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-AMX40-FlankPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "AMX40-FlankPanelSupport"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-AMX40-CheekTie"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "French-AMX40-CupolaVision"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        view,
                        "French-AMX40-AANF1-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "French-AMX40-Coax20-Barrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "French-AMX40-RearRackRail"),
                    Is.EqualTo(5));
                Transform panel = Find(
                    view,
                    "Painted-AMX40-FlankPanel");
                Assert.That(
                    Mathf.Abs(panel.localPosition.x),
                    Is.EqualTo(
                        TurretHalfWidth(definition) *
                        0.98f)
                        .Within(0.0001f));
                Assert.That(
                    Find(
                        view,
                        "Painted-French-AMX40-GunMask")
                        .parent.parent.name,
                    Is.EqualTo("Gun"));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void LeclercVariantsKeepRoofAndVariantPackages()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView leclerc = Create(catalog, "leclerc");
            TankView xlr = Create(catalog, "leclerc_xlr");
            TankView amx56 = Create(catalog, "amx56");
            try
            {
                foreach (TankView view in
                    new[] { leclerc, xlr, amx56 })
                {
                    Assert.That(
                        Count(
                            view,
                            "Painted-French-Leclerc-HL70Head"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "French-Leclerc-HL70Lens"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "French-Leclerc-ANF1-Receiver"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "French-Leclerc-M2-Receiver"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "French-Leclerc-SideBasketRail"),
                        Is.EqualTo(14));
                    Assert.That(
                        Count(
                            view,
                            "Painted-French-Leclerc-StowageDrum"),
                        Is.EqualTo(1));
                }
                Assert.That(
                    CountPrefix(
                        leclerc,
                        "Painted-French-LeclercXLR-RWS"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(
                        leclerc,
                        "Painted-French-AMX56-RWS"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        xlr,
                        "French-LeclercXLR-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        amx56,
                        "French-AMX56-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        amx56,
                        "French-AMX56-ThermalClamp"),
                    Is.EqualTo(5));
                Assert.That(
                    CountPrefix(leclerc, "Armor-"),
                    Is.EqualTo(19));
                Assert.That(
                    CountPrefix(xlr, "Armor-"),
                    Is.EqualTo(34));
                Assert.That(
                    CountPrefix(amx56, "Armor-"),
                    Is.EqualTo(87));
                Assert.That(
                    Find(
                        xlr,
                        "Painted-French-LeclercXLR-GunMask")
                        .parent.parent.name,
                    Is.EqualTo("Gun"));
                Assert.That(
                    Find(
                        amx56,
                        "Painted-French-AMX56-GunMask")
                        .parent.parent.name,
                    Is.EqualTo("Gun"));
            }
            finally
            {
                leclerc.Destroy();
                xlr.Destroy();
                amx56.Destroy();
            }
        }

        [Test]
        public void CatalogEraRemainsSoleEraOwnerAndPaintedFittingsUseCamo()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition b2Definition =
                catalog.GetVehicle("amx30b2");
            VehicleDefinition amx56Definition =
                catalog.GetVehicle("amx56");
            TankView b2 = Create(
                catalog,
                "amx30b2",
                "summer");
            TankView amx56 = Create(
                catalog,
                "amx56",
                "summer");
            try
            {
                Assert.That(
                    EraCount(b2Definition),
                    Is.EqualTo(72));
                Assert.That(
                    EraCount(amx56Definition),
                    Is.EqualTo(58));
                Assert.That(
                    CountPrefix(b2, "ERA"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(amx56, "ERA"),
                    Is.EqualTo(0));

                Renderer hull = amx56.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    amx56,
                    "Painted-French-AMX56-RWS-Body")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    amx56,
                    "French-AMX56-RWS-Lens")
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
                b2.Destroy();
                amx56.Destroy();
            }
        }

        private static int SmokeCount(string id)
        {
            if (id == "amx30") return 4;
            if (id == "amx30b2") return 6;
            if (id == "amx40") return 12;
            return 18;
        }

        private static int AntennaCount(string id)
        {
            if (id == "amx30") return 1;
            if (id == "amx40") return 3;
            return 2;
        }

        private static bool IsAmx30(string id)
        {
            return id == "amx30" ||
                id == "amx30b2";
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
                    "french-" + id,
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

        private static float TurretHalfWidth(
            VehicleDefinition definition)
        {
            return definition.armor.turretPlates
                .SelectMany(item => item.verts)
                .Max(item => Mathf.Abs(item.x));
        }
    }
}
