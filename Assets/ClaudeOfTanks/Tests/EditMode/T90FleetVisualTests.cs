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
        public void UsesCatalogArmorAndTranslatedSixWheelCourse()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90");
            TankView view = Create(catalog);
            try
            {
                Assert.That(CountPrefix(view, "Armor-"), Is.EqualTo(166));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(definition.armor.turretPlates)
                        .Count(plate => plate.kind == "era"),
                    Is.EqualTo(141));
                Assert.That(
                    Count(view, "T90-PresentationSchema"),
                    Is.EqualTo(1));
                Assert.That(CountPrefix(view, "TS-T90-"), Is.EqualTo(0));
                Assert.That(
                    Count(view, "T90-GearRoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelDisc"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelInset"),
                    Is.EqualTo(12));
                Assert.That(
                    Find(view, "T90-Sprocket").localPosition,
                    Is.EqualTo(new Vector3(-1.395f, 0.9f, -2.52f)));
                Assert.That(
                    Find(view, "T90-Idler").localPosition,
                    Is.EqualTo(new Vector3(-1.395f, 0.71f, 2.7f)));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellAndKeepsKontakt5Visible()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(VisibleArmor(view), Is.EqualTo(141));
                Assert.That(
                    CountPrefix(view, "Painted-T90-"),
                    Is.GreaterThanOrEqualTo(60));
                Assert.That(
                    view.Root.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsHullGearRearAndCageIdentity()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(Count(view, "T90-TrackPad"), Is.EqualTo(156));
                Assert.That(
                    Count(view, "T90-GearSuspensionLink"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearSuspensionJointBoss"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "Painted-T90-K5SkirtPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-T90-RubberSkirt"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "T90-RearQuarterSlat"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-SplitUnditchingLog"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsCastTurretShtoraAndRoofStation()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T90-CSharpCastDomeMesh"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-K5CheekLeaf"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90-ShtoraLens"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90-RadioWhip"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "Painted-T90-BustleRack"),
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
            TankView view = Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "Painted-T90-2A46MSaddle",
                        "Painted-T90-2A46MRoot",
                        "Painted-T90-2A46MEvacuator",
                        "Painted-T90-2A46MForwardTube",
                        "T90-MuzzleBore"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(view, "T90-GunFittings"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(ContentCatalog catalog)
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
                Find(view, name).GetComponent<Renderer>().enabled,
                Is.False);
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(part.parent.parent.name, Is.EqualTo("Gun"));
        }

        private static int VisibleArmor(TankView view)
        {
            return FindAllByPrefix(view, "Armor-")
                .Count(item => item.GetComponent<Renderer>().enabled);
        }

        private static int Count(TankView view, string name)
        {
            return FindAll(view, name).Length;
        }

        private static int CountPrefix(TankView view, string prefix)
        {
            return FindAllByPrefix(view, prefix).Length;
        }

        private static Transform Find(TankView view, string name)
        {
            Transform[] matches = FindAll(view, name);
            Assert.That(matches, Is.Not.Empty, name);
            return matches[0];
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
    }
}
