using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class BritishLegacyFleetVisualTests
    {
        [TestCase("chieftain5", "Chieftain")]
        [TestCase("chieftain_mk10", "Chieftain")]
        [TestCase("challenger1", "Challenger1")]
        [TestCase("vickers_mk1", "Vickers")]
        [TestCase("centurion3", "Centurion")]
        [TestCase("centurion5", "Centurion")]
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
                Assert.That(CountPrefix(view, "Painted-" + prefix),
                    Is.GreaterThan(8), id);
                Assert.That(
                    Count(view, "VehicleMarking-" + prefix + "-Right"),
                    Is.EqualTo(1),
                    id);
                Assert.That(
                    Count(view, "VehicleMarking-" + prefix + "-Left"),
                    Is.EqualTo(1),
                    id);
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("chieftain5", 0)]
        [TestCase("chieftain_mk10", 16)]
        public void BuildsChieftainLowTurretAndStillbrew(
            string id,
            int stillbrewTiles)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Count(view, "Painted-Chieftain-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Chieftain-TurretPresentationRoot"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-Chieftain-CheekSlope"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-Chieftain-L11A5ThermalSleeve"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-ChieftainMk10-StillbrewBrowTile"),
                    Is.EqualTo(stillbrewTiles));
                Assert.That(Count(view, "Painted-ChieftainMk10-StillbrewTurretCheek"),
                    Is.EqualTo(stillbrewTiles == 0 ? 0 : 2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsChallenger1BurlingtonPresentation()
        {
            TankView view = Create("challenger1");
            try
            {
                Assert.That(Count(view, "Painted-Challenger1-HullLoft"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Challenger1-TurretPresentationRoot"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-Challenger1-BurlingtonSidePack"),
                    Is.EqualTo(10));
                Assert.That(Count(view, "Challenger1-Detail-SmokeLauncher"),
                    Is.EqualTo(10));
                Assert.That(Count(view, "Painted-Challenger1-L11A5ForwardTube"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("vickers_mk1", "Vickers", "Painted-Vickers-FoldedBowLock")]
        [TestCase("centurion3", "Centurion", "Painted-Centurion-GlacisPlate")]
        [TestCase("centurion5", "Centurion", "Painted-CenturionMk5-MantletDustCoverStowage")]
        public void BuildsCenturionPatternVehicles(
            string id,
            string prefix,
            string expectedPart)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Count(view, expectedPart), Is.EqualTo(1));
                Assert.That(Count(view, prefix + "-TurretPresentationRoot"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-" + prefix + "-CastTurretShell"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-" + prefix + "-L7ForwardTube"),
                    Is.EqualTo(1));
                Assert.That(Count(view, prefix + "-RoofMG-Bearing"),
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
                    "british-legacy-" + id,
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
