using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class M1A2FleetVisualTests
    {
        [TestCase("m1a2", 32, 0)]
        [TestCase("m1a2_tusk", 133, 101)]
        [TestCase("m1a2_sepv2", 129, 97)]
        [TestCase("m1a2_sepv3", 155, 123)]
        public void UsesCatalogArmorAndExactTejasRunningGear(
            string id,
            int armorCount,
            int reactiveCount)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            TankView view = Create(catalog, id);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(armorCount));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(
                    CountReactive(definition),
                    Is.EqualTo(reactiveCount));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(14));
                Assert.That(
                    FindAll(view, "RoadWheel-L")
                        .Select(item => item.localPosition.z)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            2.19f,
                            1.46f,
                            0.73f,
                            0f,
                            -0.73f,
                            -1.46f,
                            -2.19f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.425f, 1.1f, -3.28f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.425f, 0.85f, 3.02f)));
                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.715f).Within(0.002f));
                Assert.That(track.max.x, Is.EqualTo(-1.135f).Within(0.002f));
                Assert.That(track.min.y, Is.EqualTo(-0.002f).Within(0.03f));
                Assert.That(track.max.y, Is.EqualTo(1.5097f).Within(0.03f));
                Assert.That(track.min.z, Is.EqualTo(-3.6879f).Within(0.002f));
                Assert.That(track.max.z, Is.EqualTo(3.4494f).Within(0.002f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("m1a2")]
        [TestCase("m1a2_tusk")]
        [TestCase("m1a2_sepv2")]
        [TestCase("m1a2_sepv3")]
        public void ReplacesGenericShellAndKeepsM256GunOwnership(
            string id)
        {
            TankView view =
                Create(ContentCatalog.Load(), id);
            try
            {
                foreach (string name in new[]
                    { "Hull", "UpperHull", "Turret", "Gun",
                      "Armor-track_L", "Armor-track_R" })
                    AssertHidden(view, name);
                foreach (string name in new[]
                    { "Painted-AbramsM1-CommanderOptic",
                      "Painted-AbramsM1-CommanderMgMount",
                      "Painted-AbramsM1-LoaderSkate" })
                    AssertHidden(view, name);
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-Abrams-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-M1A2-"),
                    Is.GreaterThan(20));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));
                AssertGunOwned(
                    view,
                    "Painted-AbramsM1-M256Barrel");
                AssertGunOwned(
                    view,
                    "AbramsM1-MuzzleBore");
                AssertGunOwned(
                    view,
                    "AbramsM1-CoaxBarrel");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BaseM1A2KeepsCleanStandardStation()
        {
            TankView view =
                Create(ContentCatalog.Load(), "m1a2");
            try
            {
                Assert.That(
                    Count(view, "Painted-M1A2-CrowsStandardHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-LoaderSplitShield"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "M1A2-SpareTrackLink"),
                    Is.EqualTo(4));
                Assert.That(
                    CountPrefix(view, "Painted-M1A2-Tusk"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-M1A2-SepV2-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-M1A2-SepV3-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "M1A2-SepV3-Ghillie"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void TuskKeepsUrbanSurvivalIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load(), "m1a2_tusk");
            try
            {
                Assert.That(
                    Count(view, "Painted-M1A2-CrowsCompactHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-TuskCrowsArmorWing"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-M1A2-TuskLagsFront"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-TuskSlatPost"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(view, "Painted-M1A2-TuskSlatRow"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-M1A2-TankInfantryPhone"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-TuskUrbanLightGuard"),
                    Is.EqualTo(2));
                Assert.That(
                    CountPrefix(view, "M1A2-SepV3-Ghillie"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void SepV2KeepsTallArmoredStationAndSupportKit()
        {
            TankView view =
                Create(ContentCatalog.Load(), "m1a2_sepv2");
            try
            {
                Assert.That(
                    Count(view, "Painted-M1A2-CrowsArmoredHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV2-CrowsCrown"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "M1A2-SepV2-LoaderM2Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV2-LoaderShield"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV2-CipPanel"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV2-AmmunitionCrate"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "M1A2-SepV2-UaapuOutlet"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void SepV3KeepsLowCrowsTrophyAdlIffAndGhillie()
        {
            TankView view =
                Create(ContentCatalog.Load(), "m1a2_sepv3");
            try
            {
                Assert.That(
                    Count(view, "Painted-M1A2-CrowsLowProfileHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV3-TrophyLauncher"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "M1A2-SepV3-TrophyRadar"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "M1A2-SepV3-AdlBox"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV3-IffPanel"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "Painted-M1A2-SepV3-UaapuHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "M1A2-SepV3-GhillieLeaf"),
                    Is.EqualTo(90));
                Assert.That(
                    Count(view, "M1A2-SepV3-GhillieCrowsVine"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static int CountReactive(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .Concat(definition.armor.turretPlates)
                .Count(plate => plate.kind == "era");
        }

        private static TankView Create(
            ContentCatalog catalog,
            string id)
        {
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static void AssertHidden(
            TankView view,
            string name)
        {
            Assert.That(
                Find(view, name)
                    .GetComponent<Renderer>()
                    .enabled,
                Is.False);
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
            Vector3 product =
                Vector3.Scale(
                    part.parent.localScale,
                    part.parent.parent.localScale);
            Assert.That(product.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(product.y, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(product.z, Is.EqualTo(1f).Within(0.0001f));
        }

        private static int Count(
            TankView view,
            string name)
        {
            return FindAll(view, name).Length;
        }

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item => item.name.StartsWith(prefix));
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Where(item => item.name == name)
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            if (name.Contains("/"))
                return view.Root.Find(name);
            return FindAll(view, name).First();
        }
    }
}
