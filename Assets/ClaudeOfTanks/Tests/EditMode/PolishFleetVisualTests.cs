using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class PolishFleetVisualTests
    {
        [TestCase("t72m1_jaguar", "T72M1Jaguar")]
        [TestCase("pt91m", "PT91M")]
        [TestCase("pt91_twardy", "PT91Twardy")]
        [TestCase("pl01", "PL01")]
        [TestCase("pl01_105", "PL01")]
        public void ReplacesDefaultHullTurretGunAndSideArmor(
            string id,
            string prefix)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Visible(view, "Hull"), Is.False, id);
                Assert.That(Visible(view, "UpperHull"), Is.False, id);
                Assert.That(Visible(view, "Turret"), Is.False, id);
                Assert.That(Visible(view, "Gun"), Is.False, id);
                Assert.That(VisibleCount(view, "SideArmor"), Is.Zero, id);
                Assert.That(
                    CountPrefix(view, "Painted-" + prefix),
                    Is.GreaterThan(8),
                    id);
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t72m1_jaguar", "T72M1Jaguar", 24, 14)]
        [TestCase("pt91m", "PT91M", 18, 8)]
        [TestCase("pt91_twardy", "PT91Twardy", 24, 6)]
        public void BuildsPt91ErawaAndPolishRoofSuite(
            string id,
            string prefix,
            int glacisTiles,
            int skirtTiles)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(
                    Count(view, "Painted-" + prefix + "-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-" + prefix + "-CastDome"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, prefix + "-2A46MSGunAssembly"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-" + prefix + "-ERAWAGlacisTile"),
                    Is.EqualTo(glacisTiles));
                Assert.That(
                    Count(view, "Painted-" + prefix + "-ERAWASkirtCassette"),
                    Is.EqualTo(skirtTiles));
                Assert.That(
                    Count(view, "Painted-" + prefix + "-ERAWATurretCheek"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, prefix + "-WkmB-Bearing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, prefix + "-RearFuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, prefix + "-PCODrawaSightLens"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("pl01", 0)]
        [TestCase("pl01_105", 6)]
        public void BuildsPl01FacetedStealthPresentation(
            string id,
            int glacisEra)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(
                    Count(view, "Painted-PL01-FacetedHullLoft"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-PL01-FacetedTurretShell"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "PL01-GunAssembly"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-PL01-FullHeightSkirtFacet"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(view, "Painted-PL01-StealthTurretSidePanel"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "PL01-Left-SmokeBank") +
                    Count(view, "PL01-Right-SmokeBank"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-PL01-105GlacisEraCassette"),
                    Is.EqualTo(glacisEra));
                Assert.That(
                    Count(view, "Painted-PL01-105CrowsBasePlate"),
                    Is.EqualTo(id == "pl01_105" ? 1 : 0));
                Assert.That(
                    Count(view, "Painted-PL01-LowObservableRwsTower"),
                    Is.EqualTo(id == "pl01" ? 1 : 0));
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
                    "polish-" + id,
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

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Count(item => item.name.StartsWith(prefix));
        }
    }
}
