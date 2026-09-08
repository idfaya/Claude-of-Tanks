using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class JapaneseFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "stb1",
            "type74",
            "type90",
            "type90a",
            "type10",
            "type10b"
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
                            "Painted-Japanese-SmokeLauncher"),
                        Is.EqualTo(
                            SmokeCount(id)),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Japanese-SmokeBankShoe"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Japanese-Antenna"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Painted-Japanese-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Japanese-RearServiceGrille"),
                        Is.EqualTo(
                            id == "stb1" ||
                            id == "type74"
                                ? 2
                                : 1),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Japanese-RearServiceSlat"),
                        Is.EqualTo(
                            RearSlatCount(id)),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Japanese-EngineDeck"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(view, "Japanese-EngineFan"),
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
        public void CastGenerationKeepsSearchlightsAndOpenBaskets()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView stb = Create(catalog, "stb1");
            TankView type74 = Create(catalog, "type74");
            try
            {
                Assert.That(
                    Count(
                        stb,
                        "Japanese-STB1-SearchlightLens"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        stb,
                        "Japanese-STB1-CupolaVision"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        stb,
                        "Japanese-STB1-FlankVent"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        stb,
                        "Japanese-STB1-CommanderMG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        stb,
                        "Japanese-STB1-BustleRail"),
                    Is.EqualTo(10));

                Assert.That(
                    Count(
                        type74,
                        "Japanese-Type74-SearchlightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        type74,
                        "Japanese-Type74-CupolaVision"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        type74,
                        "Japanese-Type74-CommanderMG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        type74,
                        "Japanese-Type74-SideBasketRail"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        type74,
                        "Painted-Japanese-Type74-BasketStowage"),
                    Is.EqualTo(2));
            }
            finally
            {
                stb.Destroy();
                type74.Destroy();
            }
        }

        [Test]
        public void Type90APackageIsVisualOnlyAndVariantScoped()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView type90 = Create(catalog, "type90");
            TankView type90a = Create(catalog, "type90a");
            try
            {
                Assert.That(
                    CountPrefix(type90, "Painted-Type90A-") +
                    CountPrefix(type90, "Type90A-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        type90a,
                        "Painted-Type90A-CheekCarrier"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        type90a,
                        "Painted-Type90A-CheekCassette"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        type90a,
                        "Painted-Type90A-HullServiceModule"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        type90a,
                        "Painted-Type90A-TurretFlankModule"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        type90a,
                        "Painted-Type90A-APSHead-Housing"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        type90a,
                        "Painted-Type90A-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(type90a, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions
                            .ValidPlateCount(
                                catalog.GetVehicle(
                                    "type90a"))));
                Assert.That(
                    Find(
                        type90a,
                        "Painted-Type90A-GunMask")
                        .parent.name,
                    Is.EqualTo("Gun"));
            }
            finally
            {
                type90.Destroy();
                type90a.Destroy();
            }
        }

        [Test]
        public void Type10BRetainsCatalogEraAndAddsOnlyKaiFittings()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("type10b");
            TankView type10 = Create(catalog, "type10");
            TankView type10b = Create(
                catalog,
                "type10b",
                "summer");
            try
            {
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(
                            definition.armor.turretPlates)
                        .Count(plate =>
                            plate.kind == "era"),
                    Is.EqualTo(52));
                Assert.That(
                    CountPrefix(type10b, "Armor-"),
                    Is.EqualTo(71));
                Assert.That(
                    CountPrefix(type10, "Painted-Type10B-") +
                    CountPrefix(type10, "Type10B-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        type10b,
                        "Painted-Type10B-LeftEO-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        type10b,
                        "Painted-Type10B-RightEO-Housing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        type10b,
                        "Painted-Type10B-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        type10b,
                        "Type10B-KaiBasketRail"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(
                        type10b,
                        "Type10B-KaiBasketJoin"),
                    Is.EqualTo(2));
                Transform mask = Find(
                    type10b,
                    "Painted-Type10B-GunMask");
                Assert.That(
                    mask.parent.name,
                    Is.EqualTo("Gun"));
                Assert.That(
                    CountPrefix(type10b, "ERA"),
                    Is.EqualTo(0));
                Renderer hull = type10b.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    type10b,
                    "Painted-Type10B-LeftEO-Housing")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    type10b,
                    "Type10B-LeftEO-Lens")
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
                type10.Destroy();
                type10b.Destroy();
            }
        }

        [Test]
        public void FittingsUseCatalogSeats()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("type90a");
            TankView view = Create(catalog, "type90a");
            try
            {
                Transform grille = Find(
                    view,
                    "Japanese-RearServiceGrille");
                Assert.That(
                    grille.localPosition.z,
                    Is.EqualTo(
                        HullRear(definition) -
                        0.051f)
                        .Within(0.0001f));
                Transform smoke = Find(
                    view,
                    "Painted-Japanese-SmokeLauncher");
                Assert.That(
                    smoke.localPosition.y,
                    Is.EqualTo(
                        TurretRoof(definition) -
                        0.19f)
                        .Within(0.0001f));
                Transform carrier = Find(
                    view,
                    "Painted-Type90A-CheekCarrier");
                Assert.That(
                    Mathf.Abs(carrier.localPosition.x),
                    Is.EqualTo(
                        TurretHalfWidth(definition) *
                        0.58f)
                        .Within(0.0001f));
                Transform module = Find(
                    view,
                    "Painted-Type90A-HullServiceModule");
                Assert.That(
                    Mathf.Abs(module.localPosition.x),
                    Is.EqualTo(
                        definition.dims.widthM *
                        0.495f)
                        .Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static int SmokeCount(string id)
        {
            if (id == "stb1" ||
                id == "type90a")
                return 10;
            if (id == "type74")
                return 6;
            if (id == "type10b")
                return 12;
            return 8;
        }

        private static int RearSlatCount(string id)
        {
            if (id == "stb1") return 10;
            if (id == "type74" ||
                id == "type90" ||
                id == "type90a")
                return 6;
            return 5;
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
                    "japanese-" + id,
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

        private static float HullRear(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .SelectMany(item => item.verts)
                .Min(item => item.z);
        }

        private static float TurretRoof(
            VehicleDefinition definition)
        {
            return definition.armor.turretPlates
                .Where(item =>
                    item.name.StartsWith(
                        "turret_roof"))
                .SelectMany(item => item.verts)
                .Max(item => item.y);
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
