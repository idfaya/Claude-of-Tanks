using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class PumaFleetVisualTests
    {
        [TestCase("spz_puma", 19)]
        [TestCase("spz_puma_s1", 24)]
        public void UsesCatalogArmorAndExactSixWheelRig(
            string id,
            int armorCount)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            TankView view = Create(
                catalog,
                id);
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
                    Count(view, "MissilePod"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(12));
                float expectedTrackCenter =
                    id == "spz_puma"
                        ? 1.25f
                        : 1.323f;
                Transform[] roadWheels =
                    view.Root
                        .GetComponentsInChildren<Transform>()
                        .Where(item =>
                            item.name.StartsWith(
                                "RoadWheel-"))
                        .ToArray();
                Assert.That(
                    roadWheels.Length,
                    Is.EqualTo(12));
                Assert.That(
                    roadWheels.All(item =>
                        Mathf.Abs(
                            Mathf.Abs(
                                item.localPosition.x) -
                            expectedTrackCenter) <
                        0.0001f),
                    Is.True);
                float expectedWheelY =
                    id == "spz_puma"
                        ? 0.43f
                        : 0.378f;
                Assert.That(
                    roadWheels.All(item =>
                        Mathf.Abs(
                            item.localPosition.y -
                            expectedWheelY) <
                        0.0001f),
                    Is.True);
                float[] expectedStations =
                    id == "spz_puma"
                        ? new[]
                        {
                            -2.173f,
                            -1.43f,
                            -0.68f,
                            0.247f,
                            1.009f,
                            1.791f
                        }
                        : new[]
                        {
                            -1.98f,
                            -1.152f,
                            -0.306f,
                            0.531f,
                            1.359f,
                            2.178f
                        };
                float[] actualStations =
                    roadWheels
                        .Where(item =>
                            item.name ==
                            "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .OrderBy(value => value)
                        .ToArray();
                Assert.That(
                    actualStations,
                    Is.EqualTo(
                        expectedStations)
                        .Within(0.0001f));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));
                Assert.That(
                    Find(view, "Turret")
                        .GetComponent<Renderer>()
                        .enabled,
                    Is.False);
                Assert.That(
                    Find(view, "Hull")
                        .GetComponent<Renderer>()
                        .enabled,
                    Is.False);
                Assert.That(
                    Find(view, "UpperHull")
                        .GetComponent<Renderer>()
                        .enabled,
                    Is.False);
                Assert.That(
                    Find(view, "Armor-track_L")
                        .GetComponent<Renderer>()
                        .enabled,
                    Is.False);
                Assert.That(
                    Find(view, "Armor-track_R")
                        .GetComponent<Renderer>()
                        .enabled,
                    Is.False);
                Assert.That(
                    Find(view, "Gun")
                        .GetComponent<Renderer>()
                        .enabled,
                    Is.False);
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ProductionPumaKeepsHeavyModulesMussAndPitchingSpikePod()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "spz_puma");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Puma-UpperSideModule"),
                    Is.EqualTo(22));
                Assert.That(
                    Count(
                        view,
                        "Painted-Puma-LowerSideModule"),
                    Is.EqualTo(22));
                Assert.That(
                    Count(
                        view,
                        "Puma-MussSensorHead"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Painted-Puma-RosyLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Puma-PeriHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Puma-SpikePod"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Puma-SpikeTubeMouth"),
                    Is.EqualTo(2));
                AssertGunOwned(
                    view,
                    "Painted-Puma-SpikePod");
                AssertGunOwned(
                    view,
                    "Painted-Puma-Mk30-Barrel");
                Assert.That(
                    Count(
                        view,
                        "Painted-PumaS1-MellsSquareCell"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void PumaS1KeepsAmapMellsPanoramicAndIndependentRws()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "spz_puma_s1");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-PumaS1-AmapCassette"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Painted-PumaS1-AmapLid"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Painted-PumaS1-AllAroundCamera"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Painted-Puma-RosyLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        view,
                        "Painted-PumaS1-MellsSquareCell"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "PumaS1-MellsSquareMouth"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Puma-SpikeTubeMouth"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        view,
                        "PumaS1-PanoramicStation"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "PumaS1-CompactRws"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "PumaS1-RwsMachineGunBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "PumaS1-RwsFeedLink"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        view,
                        "Painted-PumaS1-CradleSideWeb"),
                    Is.EqualTo(8));
                Assert.That(
                    Find(
                        view,
                        "PumaS1-PanoramicStation")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));
                Assert.That(
                    Find(
                        view,
                        "PumaS1-CompactRws")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));
                Assert.That(
                    Find(
                        view,
                        "Painted-PumaS1-MellsSquareCell")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));
                Assert.That(
                    Find(
                        view,
                        "PumaS1-PanoramicStation")
                        .GetComponentsInChildren<Transform>()
                        .Any(item =>
                            item.name.Contains(
                                "MachineGun")),
                    Is.False);
                AssertGunOwned(
                    view,
                    "Painted-PumaS1-OpenCradleTop");
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("spz_puma")]
        [TestCase("spz_puma_s1")]
        public void PaintedProtectionUsesCamouflageAndOpticsStayNeutral(
            string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                id,
                "digital");
            try
            {
                Renderer painted = Find(
                        view,
                        "Painted-Puma-HighGlacis")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        "Puma-GunSideOpticLens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    optic.sharedMaterial.mainTexture,
                    Is.Null);
            }
            finally
            {
                view.Destroy();
            }
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
            Vector3 product = Vector3.Scale(
                part.parent.localScale,
                part.parent.parent.localScale);
            Assert.That(
                product.x,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                product.y,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                product.z,
                Is.EqualTo(1f).Within(0.0001f));
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
                    id + "-test",
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
    }
}
