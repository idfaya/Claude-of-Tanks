using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndNativeSixWheelCourse()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90");
            TankView view =
                Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(166));
                Assert.That(
                    Count(view, "T90-TsGeneratedPresentationSchema"),
                    Is.EqualTo(1));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(definition.armor.turretPlates)
                        .Count(plate => plate.kind == "era"),
                    Is.EqualTo(141));
                Assert.That(
                    FindAll(view, "RoadWheel-L").Length,
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(view, "RoadWheel-R").Length,
                    Is.EqualTo(6));
                Assert.That(
                    CountPrefix(view, "TS-T90-gearReturnRoller"),
                    Is.GreaterThanOrEqualTo(2));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.395f,
                        0.9f,
                        -2.52f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.395f,
                        0.71f,
                        2.7f)));
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
                    Is.EqualTo(141));
                Assert.That(
                    Count(view, "Painted-Soviet-FuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Soviet-ShtoraLens"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "TS-T90-"),
                    Is.GreaterThanOrEqualTo(70));
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
        public void BuildsHullKontakt5RearAndCageIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "TS-T90-hull"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullExternalArmor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullRubber"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-gearTrackBandL"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-gearTrackBandR"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullWood"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullEquipment"),
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
                    Count(view, "TS-T90-turret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretExternalArmor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretDetail"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretEquipment"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretGlass"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "TS-T90-fitting_smokeBank"),
                    Is.GreaterThanOrEqualTo(4));
                Assert.That(
                    Count(view, "TS-T90-browningDerivedMachineGunBody"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "TS-T90-fitting_antennaWhip"),
                    Is.GreaterThanOrEqualTo(4));
                Assert.That(
                    Count(view, "TS-T90-turretDark"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsTwoA46MOnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "TS-T90-gun",
                        "TS-T90-gunDark",
                        "TS-T90-gunMount",
                        "TS-T90-muzzleBoreShadowRim",
                        "TS-T90-muzzleBoreShadowDisc"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(view, "T90-TsGunFittings"),
                    Is.EqualTo(1));
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
                catalog.GetVehicle("t90");
            return TankView.Create(
                new TankState(
                    "t90-test",
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
