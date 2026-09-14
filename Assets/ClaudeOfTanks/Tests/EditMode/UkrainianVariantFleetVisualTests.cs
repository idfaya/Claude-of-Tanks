using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class UkrainianVariantFleetVisualTests
    {
        [TestCase("ua_t64bv")]
        [TestCase("ua_m1a1")]
        public void ReplacesDefaultHullTurretGunAndSideArmor(string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Visible(view, "Hull"), Is.False, id);
                Assert.That(Visible(view, "UpperHull"), Is.False, id);
                Assert.That(Visible(view, "Turret"), Is.False, id);
                Assert.That(Visible(view, "Gun"), Is.False, id);
                Assert.That(VisibleCount(view, "SideArmor"), Is.Zero, id);
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsDonbasT64K1AndFieldStowage()
        {
            TankView view = Create("ua_t64bv");
            try
            {
                Assert.That(
                    Count(view, "Painted-T64-CastDome"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-UAT64BV-K1SideCassette"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-UAT64BV-K1GlacisTile"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "Painted-UAT64BV-K1TurretHorseshoe"),
                    Is.EqualTo(28));
                Assert.That(
                    Count(view, "Painted-UAT64BV-RightSnorkelRack"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "UAT64BV-AkmProp"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "VehicleMarking-UAT64BV-Right"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsUkrainianAbramsDroneCage()
        {
            TankView view = Create("ua_m1a1");
            try
            {
                Assert.That(
                    Count(view, "Painted-AbramsM1Ha-SideModule"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "UAM1A1-DroneCageRoot"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-UAM1A1-CageFoot"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "UAM1A1-CagePost"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "UAM1A1-CageCrossRib"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "UAM1A1-DroneCageJammerBox"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(string id)
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    "ukrainian-variant-" + id,
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
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

        private static int VisibleCount(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Renderer>(true)
                .Count(item => item.name == name && item.enabled);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == name);
        }
    }
}
