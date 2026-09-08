using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class KoreanFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "k2",
            "k1a1",
            "k2b"
        };

        [Test]
        public void ProductionFamilyUsesAuthoredArmorAndCommonFittings()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            for (int index = 0;
                index < ProductionIds.Length;
                index++)
            {
                string id = ProductionIds[index];
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
                            "Korean-RearServiceGrille"),
                        Is.EqualTo(
                            id == "k1a1" ? 2 : 3),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Korean-RearServiceSlat"),
                        Is.EqualTo(
                            id == "k1a1" ? 8 : 13),
                        id);
                    Assert.That(
                        Count(view, "Korean-EngineDeck"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Korean-EngineDeckSlat"),
                        Is.EqualTo(5),
                        id);
                    Assert.That(
                        Count(view, "Korean-EngineFan"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-SmokeLauncher"),
                        Is.EqualTo(12),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-SmokeBankShoe"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Korean-Antenna"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        view.Root
                            .GetComponentsInChildren<
                                Collider>()
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
        public void K1KeepsLowRoofWeaponsAndSeatedSideCages()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView k1 = Create(catalog, "k1a1");
            try
            {
                Assert.That(
                    Count(
                        k1,
                        "Painted-Korean-K1-GunnerDoghouse"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(k1, "Korean-K1-GunnerLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-CupolaVision"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(k1, "Korean-K1-Periscope"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(k1, "Korean-K1-K6-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-LoaderMAG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-RearRackRail"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-SideCageRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-SideCagePost"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-CageBracket"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-CageWeldFoot"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-CageCornerRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        k1,
                        "Painted-Korean-K1-CageStowage"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        k1,
                        "Korean-K1-CageHardCase"),
                    Is.EqualTo(2));
                Assert.That(
                    CountPrefix(k1, "Painted-K2B-") +
                    CountPrefix(k1, "K2B-"),
                    Is.EqualTo(0));
            }
            finally
            {
                k1.Destroy();
            }
        }

        [Test]
        public void K2BAddsOnlyItsStealthAndTwinRwsPackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView k2 = Create(catalog, "k2");
            TankView k2b = Create(catalog, "k2b");
            try
            {
                foreach (TankView view in new[]
                    {
                        k2,
                        k2b
                    })
                {
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-K2-KGPSHousing"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-K2-KCPSHousing"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-K2-KAPSRoofHead"),
                        Is.EqualTo(2));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-K2-CheekRadar"),
                        Is.EqualTo(2));
                    Assert.That(
                        Count(
                            view,
                            "Korean-K2-BustleRail"),
                        Is.EqualTo(17));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Korean-K2-BustleStowage"),
                        Is.EqualTo(6));
                    Assert.That(
                        Count(
                            view,
                            "Korean-K2-K6-Receiver"),
                        Is.EqualTo(1));
                }

                Assert.That(
                    CountPrefix(k2, "Painted-K2B-") +
                    CountPrefix(k2, "K2B-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        k2b,
                        "Painted-K2B-StealthSideCassette"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        k2b,
                        "K2B-StealthCassetteSeam"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        k2b,
                        "Painted-K2B-BowShoulder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        k2b,
                        "Painted-K2B-StealthTurretPanel"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        k2b,
                        "Painted-K2B-RoofRWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        k2b,
                        "Painted-K2B-AuxOpenYokeRWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        k2b,
                        "K2B-AuxOpenYokeRWS-YokeArm"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        k2b,
                        "K2B-AuxOpenYokeRWS-FeedBelt"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(k2b, "Painted-K2B-EOHead"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(k2b, "K2B-EOLens"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(k2b, "K2B-Antenna"),
                    Is.EqualTo(2));
            }
            finally
            {
                k2.Destroy();
                k2b.Destroy();
            }
        }

        [Test]
        public void FittingsUseCatalogSeatsOwnershipAndCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView k1 = Create(
                catalog,
                "k1a1",
                "summer");
            TankView k2b = Create(
                catalog,
                "k2b",
                "summer");
            try
            {
                VehicleDefinition k2bDefinition =
                    catalog.GetVehicle("k2b");
                Transform grille = Find(
                    k2b,
                    "Korean-RearServiceGrille");
                Assert.That(
                    grille.localPosition.z,
                    Is.EqualTo(
                        HullRear(k2bDefinition) -
                        0.025f)
                        .Within(0.0001f));
                Transform smoke = Find(
                    k2b,
                    "Painted-Korean-SmokeLauncher");
                Assert.That(
                    smoke.localPosition.y,
                    Is.EqualTo(
                        TurretRoof(k2bDefinition) -
                        0.28f)
                        .Within(0.0001f));
                Transform cassette = Find(
                    k2b,
                    "Painted-K2B-StealthSideCassette");
                Assert.That(
                    Mathf.Abs(cassette.localPosition.x),
                    Is.EqualTo(
                        k2bDefinition.dims.widthM *
                        0.49f)
                        .Within(0.0001f));
                Transform shoulder = Find(
                    k2b,
                    "Painted-K2B-BowShoulder");
                Assert.That(
                    shoulder.localScale.x,
                    Is.EqualTo(
                        k2bDefinition.dims.widthM *
                        0.426f)
                        .Within(0.0001f));
                Assert.That(
                    shoulder.localScale.y,
                    Is.EqualTo(0.5f)
                        .Within(0.0001f));
                Assert.That(
                    shoulder.localScale.z,
                    Is.EqualTo(
                        k2bDefinition.dims.hullLengthM *
                        0.117f)
                        .Within(0.0001f));

                VehicleDefinition k1Definition =
                    catalog.GetVehicle("k1a1");
                Transform weldFoot = Find(
                    k1,
                    "Korean-K1-CageWeldFoot");
                Assert.That(
                    Mathf.Abs(weldFoot.localPosition.x),
                    Is.EqualTo(
                        TurretHalfWidth(k1Definition) *
                        0.88f)
                        .Within(0.0001f));

                Transform mask = Find(
                    k2b,
                    "Painted-K2B-GunMask");
                Assert.That(
                    mask.parent.name,
                    Is.EqualTo("Gun"));
                Renderer hull = k2b.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = cassette
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    k2b,
                    "K2B-EOLens")
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
                k1.Destroy();
                k2b.Destroy();
            }
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
                    "korean-" + id,
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
