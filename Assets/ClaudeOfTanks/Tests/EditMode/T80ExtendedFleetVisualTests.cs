using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T80ExtendedFleetVisualTests
    {
        [TestCase("t80u")]
        [TestCase("ua_t80bv")]
        [TestCase("ua_t80u_kursk")]
        [TestCase("t84")]
        [TestCase("ua_t84_oplot_m")]
        public void ReplacesDefaultHullTurretGunAndSovietFallback(
            string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Visible(view, "Hull"), Is.False, id);
                Assert.That(Visible(view, "UpperHull"), Is.False, id);
                Assert.That(Visible(view, "Turret"), Is.False, id);
                Assert.That(Visible(view, "Gun"), Is.False, id);
                Assert.That(VisibleCount(view, "SideArmor"), Is.Zero, id);
                Assert.That(VisibleCount(view, "MissilePod"), Is.Zero, id);
                Assert.That(
                    CountPrefix(view, "Painted-Soviet-") +
                    CountPrefix(view, "Soviet-"),
                    Is.Zero,
                    id);
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t80u")]
        [TestCase("ua_t80bv")]
        [TestCase("ua_t80u_kursk")]
        public void BuildsT80UCastKontakt5Presentation(
            string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Count(view, "Painted-T80U-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-T80-CastDome"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "T80-2A46M1Assembly"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-T80U-K5GlacisTile"),
                    Is.EqualTo(14));
                Assert.That(Count(view, "Painted-T80U-K5TurretChevron"),
                    Is.EqualTo(16));
                Assert.That(Count(view, "Painted-T80U-K5FlankReturn"),
                    Is.EqualTo(10));
                Assert.That(Count(view, "T80U-TurbineDeckGrille"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "T80U-RearFuelDrum"),
                    Is.EqualTo(2));
                if (id.StartsWith("ua_"))
                {
                    Assert.That(
                        Count(view, "Painted-T80U-UkrainianStowageBin"),
                        Is.EqualTo(1));
                }
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t84")]
        [TestCase("ua_t84_oplot_m")]
        public void BuildsT84WeldedDupletPresentation(
            string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Count(view, "Painted-T84-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "T84-WeldedTurretPresentationRoot"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-T84-DupletCheekCassette"),
                    Is.EqualTo(16));
                Assert.That(Count(view, "Painted-T84-DupletFlankCassette"),
                    Is.EqualTo(8));
                Assert.That(Count(view, "T84-KBA3GunAssembly"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-T84-KBA3ThermalSleeve"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "T84-SmokeBank"),
                    Is.EqualTo(2));
                Assert.That(Count(view, "VehicleMarking-T84-Right"),
                    Is.EqualTo(id == "t84" ? 1 : 0));
                Assert.That(Count(view, "VehicleMarking-T84-Left"),
                    Is.EqualTo(id == "t84" ? 1 : 0));
                if (id == "ua_t84_oplot_m")
                {
                    Assert.That(
                        Count(view, "Painted-T84-OplotM-RightFenderBin"),
                        Is.EqualTo(1));
                }
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
                    "t80-extended-" + id,
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
