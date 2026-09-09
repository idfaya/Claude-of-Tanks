using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T72BUFleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndNativeSixWheelCourse()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t72bu");
            TankView view =
                Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(180));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(definition.armor.turretPlates)
                        .Count(plate => plate.kind == "era"),
                    Is.EqualTo(160));
                Assert.That(
                    FindAll(view, "RoadWheel-L").Length,
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(view, "RoadWheel-R").Length,
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T72BU-ReturnRoller"),
                    Is.EqualTo(6));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.38f,
                        0.61f,
                        -2.46f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.38f,
                        0.58f,
                        2.58f)));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellAndKeepsKontakt5Visible()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    VisibleArmor(view),
                    Is.EqualTo(160));
                Assert.That(
                    Count(view, "Painted-Soviet-FuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Soviet-SearchlightLens"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Soviet-ShtoraLens"),
                    Is.EqualTo(0));
                Assert.That(
                    view.Root.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsHullKontakt5AndRearServiceIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T72BU-UpperHull"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T72BU-K5GlacisWedge"),
                    Is.EqualTo(25));
                Assert.That(
                    Count(view, "Painted-T72BU-K5SkirtPanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(view, "T72BU-EngineLouvre"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(view, "T72BU-WadingMast"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T72BU-RearFuelDrum"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T72BU-UnditchingLog"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsCastTurretShtoraAndRoofStation()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T72BU-CastDome"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T72BU-K5CheekWedge"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "Painted-T72BU-ShtoraHousing"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T72BU-ShtoraLens"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T72BU-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T72BU-NsvtReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T72BU-BustleRail"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T72BU-RadioWhip"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsTwoA46M4OnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "Painted-T72BU-2A46M4Saddle",
                        "Painted-T72BU-2A46M4Root",
                        "Painted-T72BU-2A46M4Evacuator",
                        "Painted-T72BU-2A46M4ForwardTube",
                        "T72BU-MuzzleBore"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(view, "T72BU-2A46M4SleeveRing"),
                    Is.EqualTo(4));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(
            ContentCatalog catalog)
        {
            VehicleDefinition definition =
                catalog.GetVehicle("t72bu");
            return TankView.Create(
                new TankState(
                    "t72bu-test",
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
        }

        private static int VisibleArmor(
            TankView view)
        {
            return FindAllByPrefix(view, "Armor-")
                .Count(item =>
                    item.GetComponent<Renderer>().enabled);
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
            return FindAllByPrefix(view, prefix).Length;
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name == name)
                .ToArray();
        }

        private static Transform[] FindAllByPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name.StartsWith(prefix))
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return FindAll(view, name).First();
        }
    }
}
