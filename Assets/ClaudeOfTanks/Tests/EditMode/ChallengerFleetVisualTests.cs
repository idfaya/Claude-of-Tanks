using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class ChallengerFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "fv4034",
            "challenger2",
            "challenger2e",
            "ua_challenger2",
            "challenger_3",
            "challenger_3x"
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
                        Count(
                            view,
                            "Challenger-EngineGrille"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Challenger-BustleRackBar"),
                        Is.EqualTo(15),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Challenger-Antenna"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        CountPrefix(
                            view,
                            "Painted-Challenger2-Hatch") +
                        CountPrefix(
                            view,
                            "Painted-Challenger3-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        SmokeCount(view),
                        Is.EqualTo(ExpectedSmokeCount(id)),
                        id);
                    Assert.That(
                        CountPrefix(view, "Armor-"),
                        Is.EqualTo(
                            FleetVisualAssertions
                                .ValidPlateCount(
                                    definition)),
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
        public void Challenger2VariantsKeepDistinctRoofAndFieldKits()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView fv4034 = Create(catalog, "fv4034");
            TankView challenger2 =
                Create(catalog, "challenger2");
            TankView challenger2e =
                Create(catalog, "challenger2e");
            TankView ukrainian =
                Create(catalog, "ua_challenger2");
            try
            {
                Assert.That(
                    Count(fv4034, "FV4034-Periscope"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        fv4034,
                        "FV4034-MAG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        fv4034,
                        "FV4034-M2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(
                        fv4034,
                        "Painted-Challenger2-RWS"),
                    Is.EqualTo(0));

                Assert.That(
                    Count(
                        challenger2,
                        "Painted-Challenger2-GPSHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        challenger2,
                        "Painted-Challenger2-SightHead"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        challenger2,
                        "Painted-Challenger2-RWS-Receiver"),
                    Is.EqualTo(1));

                AssertEnhancedChallenger2(challenger2e);
                AssertEnhancedChallenger2(ukrainian);
                Assert.That(
                    Count(
                        ukrainian,
                        "Challenger2UA-HullCage"),
                    Is.EqualTo(22));
                Assert.That(
                    Count(
                        ukrainian,
                        "Challenger2UA-TurretCage"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(
                        ukrainian,
                        "Challenger2UA-RearCage"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(
                        ukrainian,
                        "Challenger2UA-Canopy"),
                    Is.EqualTo(8));
                Assert.That(
                    CountPrefix(
                        challenger2e,
                        "Challenger2UA-"),
                    Is.EqualTo(0));
            }
            finally
            {
                fv4034.Destroy();
                challenger2.Destroy();
                challenger2e.Destroy();
                ukrainian.Destroy();
            }
        }

        [Test]
        public void Challenger3XAddsOnlyItsAuthoredEquipmentPackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView challenger3 =
                Create(catalog, "challenger_3");
            TankView challenger3x =
                Create(catalog, "challenger_3x");
            try
            {
                foreach (TankView view in new[]
                    {
                        challenger3,
                        challenger3x
                    })
                {
                    Assert.That(
                        Count(
                            view,
                            "Painted-Challenger3-Protector-Receiver"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Painted-Challenger3-PanoramicSight"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            view,
                            "Challenger3-TrophyRadar"),
                        Is.EqualTo(4));
                }

                Assert.That(
                    CountPrefix(
                        challenger3,
                        "Challenger3X-") +
                    CountPrefix(
                        challenger3,
                        "Painted-Challenger3X-"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        challenger3x,
                        "Painted-Challenger3X-Autocannon-Receiver"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        challenger3x,
                        "Challenger3X-Autocannon-Barrel"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        challenger3x,
                        "Challenger3X-RadarArray"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        challenger3x,
                        "Painted-Challenger3X-Searchlight"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        challenger3x,
                        "Challenger3X-BustleCage"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(
                        challenger3x,
                        "Painted-Challenger3X-Stowage"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        challenger3x,
                        "Challenger3X-SkirtHanger"),
                    Is.EqualTo(18));
            }
            finally
            {
                challenger3.Destroy();
                challenger3x.Destroy();
            }
        }

        [Test]
        public void FittingsUseCatalogSeatsAndCamouflageSemantics()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView challenger3x =
                Create(
                    catalog,
                    "challenger_3x",
                    "summer");
            TankView challenger2e =
                Create(
                    catalog,
                    "challenger2e",
                    "summer");
            try
            {
                VehicleDefinition definition =
                    catalog.GetVehicle(
                        "challenger_3x");
                Transform grille = Find(
                    challenger3x,
                    "Challenger-EngineGrille");
                Assert.That(
                    Mathf.Abs(grille.localPosition.x),
                    Is.EqualTo(
                        definition.dims.widthM *
                        0.18f)
                        .Within(0.0001f));
                Assert.That(
                    grille.localPosition.y,
                    Is.EqualTo(
                        HullRoof(definition) +
                        0.022f)
                        .Within(0.0001f));
                Assert.That(
                    grille.localPosition.z,
                    Is.EqualTo(
                        HullRear(definition) +
                        definition.dims.hullLengthM *
                        0.2f)
                        .Within(0.0001f));

                Transform rws = Find(
                    challenger3x,
                    "Painted-Challenger3-Protector-Base");
                Assert.That(
                    rws.localPosition.y,
                    Is.EqualTo(
                        TurretRoof(definition) +
                        0.04f)
                        .Within(0.0001f));
                Transform radarMast = Find(
                    challenger3x,
                    "Painted-Challenger3X-RadarMast");
                Assert.That(
                    radarMast.localPosition.y - 0.36f,
                    Is.EqualTo(
                        TurretRoof(definition) -
                        0.02f)
                        .Within(0.0001f));

                Renderer hull = challenger3x.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    challenger3x,
                    "Painted-Challenger3X-Autocannon-Receiver")
                    .GetComponent<Renderer>();
                Renderer sensor = Find(
                    challenger3x,
                    "Challenger3X-RadarArray")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.SameAs(
                        hull.sharedMaterial.mainTexture));
                Assert.That(
                    sensor.sharedMaterial.mainTexture,
                    Is.Null);

                VehicleDefinition enhanced =
                    catalog.GetVehicle("challenger2e");
                Assert.That(
                    Mathf.Abs(
                        Find(
                            challenger2e,
                            "Painted-Challenger2E-SkirtPanel")
                            .localPosition.x),
                    Is.EqualTo(
                        enhanced.dims.widthM *
                        0.49f)
                        .Within(0.0001f));
            }
            finally
            {
                challenger3x.Destroy();
                challenger2e.Destroy();
            }
        }

        private static void AssertEnhancedChallenger2(
            TankView view)
        {
            Assert.That(
                Count(
                    view,
                    "Painted-Challenger2E-SkirtPanel"),
                Is.EqualTo(16));
            Assert.That(
                Count(
                    view,
                    "Challenger2E-FuelBarrel"),
                Is.EqualTo(2));
            Assert.That(
                Count(
                    view,
                    "Painted-Challenger2E-SightHead"),
                Is.EqualTo(2));
            Assert.That(
                Count(
                    view,
                    "Challenger2E-LoaderMAG-Receiver"),
                Is.EqualTo(1));
            Assert.That(
                Count(
                    view,
                    "Challenger2E-CommanderM2-Receiver"),
                Is.EqualTo(1));
            Assert.That(
                Count(
                    view,
                    "Challenger2E-RearMAG-Receiver"),
                Is.EqualTo(1));
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
                    "challenger-" + id,
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                camouflage,
                "forest",
                catalog);
        }

        private static int SmokeCount(TankView view)
        {
            return CountPrefix(
                    view,
                    "Painted-Challenger2-SmokeLauncher") +
                CountPrefix(
                    view,
                    "Painted-Challenger3-SmokeLauncher");
        }

        private static int ExpectedSmokeCount(string id)
        {
            return id == "challenger2" ||
                id == "challenger_3" ||
                id == "challenger_3x"
                    ? 10
                    : 8;
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

        private static float HullRoof(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .SelectMany(item => item.verts)
                .Max(item => item.y);
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
    }
}
