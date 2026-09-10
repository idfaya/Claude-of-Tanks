using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class SovietFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "t80",
            "t80b",
            "t80bv",
            "t80u",
            "t90a_vladimir",
            "t90a_burlak",
            "t90sm",
            "t90m",
            "t90ms",
            "t90m_proryv",
            "ua_t80bv",
            "ua_t80u_kursk",
            "t72m1_jaguar",
            "bmpt_t90"
        };

        [Test]
        public void ProductionFamilyBuildsVariantEquipment()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            for (int i = 0;
                i < ProductionIds.Length;
                i++)
            {
                TankView view = Create(
                    catalog,
                    ProductionIds[i]);
                try
                {
                    Transform[] parts =
                        view.Root.GetComponentsInChildren<
                            Transform>();
                    int genericParts = parts.Count(item =>
                        item.name.StartsWith(
                            "Soviet-") ||
                        item.name.StartsWith(
                            "Painted-Soviet-"));
                    if (ProductionIds[i] == "t90a_vladimir" ||
                        ProductionIds[i] == "t90a_burlak" ||
                        ProductionIds[i] == "t90sm" ||
                        ProductionIds[i] == "t90ms" ||
                        ProductionIds[i] == "t90m" ||
                        ProductionIds[i] == "t90m_proryv")
                    {
                        Assert.That(genericParts, Is.EqualTo(0));
                        if (ProductionIds[i] == "t90a_vladimir")
                        {
                            Assert.That(
                                parts.Any(item =>
                                    item.name ==
                                    "T90AVladimir-PresentationRoot"),
                                Is.True);
                        }
                    }
                    else
                    {
                        Assert.That(
                            genericParts,
                            Is.GreaterThanOrEqualTo(12),
                            ProductionIds[i]);
                    }
                    Assert.That(
                        parts.Any(item =>
                            item.name == "ERA"),
                        Is.False,
                        ProductionIds[i] +
                        " must use authored armor meshes.");
                }
                finally
                {
                    view.Destroy();
                }
            }
        }

        [Test]
        public void EarlyAndModernVariantsStayDistinct()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            TankView early = Create(catalog, "t80");
            TankView reactive = Create(catalog, "t80bv");
            TankView shtora = Create(catalog, "t90a_vladimir");
            TankView modern = Create(catalog, "t90m");
            TankView burlak = Create(
                catalog,
                "t90a_burlak");
            TankView ukrainian = Create(
                catalog,
                "ua_t80u_kursk");
            try
            {
                Assert.That(
                    Count(early, "Armor-", "era"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(reactive, "Armor-", "era"),
                    Is.GreaterThan(20));
                Assert.That(
                    Count(
                        early,
                        "Soviet-TurbineGrille"),
                    Is.Zero);
                Assert.That(
                    Count(
                        early,
                        "T80-EngineDeckGrilleBacking"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        ukrainian,
                        "Soviet-TurbineGrille"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        early,
                        "Soviet-SearchlightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        shtora,
                        "T90AVladimir-ShtoraLens"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        shtora,
                        "Soviet-SearchlightLens"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        modern,
                        "Painted-T90M-KordRace"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        modern,
                        "Painted-T90M-BustleTopStore"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        burlak,
                        "Painted-Soviet-BurlakStowage"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        burlak,
                        "Painted-T90A-UpperHull"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        burlak,
                        "Painted-T90ABurlak-FenderClosure-Centre") +
                    Count(
                        burlak,
                        "Painted-T90ABurlak-FenderClosure-Forward") +
                    Count(
                        burlak,
                        "Painted-T90ABurlak-FenderClosure-Shoulder") +
                    Count(
                        burlak,
                        "Painted-T90ABurlak-FenderClosure-Bow"),
                    Is.EqualTo(8));
                Transform burlakHull = burlak.Root
                    .GetComponentsInChildren<Transform>()
                    .First(item =>
                        item.name == "T90ABurlak-HullSection");
                Assert.That(
                    burlakHull.localPosition,
                    Is.EqualTo(new Vector3(0f, 0.12f, 0f)));
                Assert.That(
                    burlakHull.localScale,
                    Is.EqualTo(new Vector3(0.94f, 0.92f, 1.02f)));
                Assert.That(
                    Count(
                        ukrainian,
                        "Painted-Soviet-UkrainianSnorkel"),
                    Is.EqualTo(1));
            }
            finally
            {
                early.Destroy();
                reactive.Destroy();
                shtora.Destroy();
                modern.Destroy();
                burlak.Destroy();
                ukrainian.Destroy();
            }
        }

        [Test]
        public void BmptUsesTwinCannonsAndEightMissiles()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "bmpt_t90");
            try
            {
                Transform[] cannons = view.Root
                    .GetComponentsInChildren<Transform>()
                    .Where(item =>
                        item.name ==
                            "Soviet-BMPT-Cannon")
                    .ToArray();
                Assert.That(
                    cannons.Length,
                    Is.EqualTo(2));
                Assert.That(
                    cannons.Select(item =>
                        Mathf.Abs(
                            item.localPosition.x)),
                    Is.All.EqualTo(0.2f)
                        .Within(0.0001f));
                Assert.That(
                    cannons.Select(item =>
                        item.localScale.z),
                    Is.All.EqualTo(2.75f)
                        .Within(0.0001f));
                Assert.That(
                    Count(
                        view,
                        "Painted-Soviet-BMPT-MissileTube"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "Painted-Soviet-BMPT-SmokeLauncher"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-Soviet-BMPT-GrenadePod"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "MissilePod"),
                    Is.EqualTo(0));
                Renderer authoritativeGun = view.Root
                    .Find("TurretRoot/Gun")
                    .GetComponent<Renderer>();
                Assert.That(
                    authoritativeGun.enabled,
                    Is.False);
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void PaintedFittingsUseCamouflageButSensorsDoNot()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "t90m",
                "summer");
            try
            {
                Renderer hull = view.Root.Find("Hull")
                    .GetComponent<Renderer>();
                Renderer housing = FindRenderer(
                    view,
                    "Painted-T90M-SosnaHousing");
                Renderer lens = FindRenderer(
                    view,
                    "T90M-SosnaLens");
                Assert.That(
                    housing.sharedMaterial.mainTexture,
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

        private static TankView Create(
            ContentCatalog catalog,
            string id,
            string camouflage = "factory")
        {
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    "soviet-" + id,
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

        private static int Count(
            TankView view,
            string prefix,
            string contains)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item =>
                    item.name.StartsWith(prefix) &&
                    item.name.ToLowerInvariant()
                        .Contains(contains));
        }

        private static Renderer FindRenderer(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Renderer>()
                .First(item => item.name == name);
        }
    }
}
