using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class AbramsXFleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactAbramsXRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("abramsx");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(22));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
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
                            2.1674f,
                            1.3713f,
                            0.6533f,
                            -0.0648f,
                            -0.7828f,
                            -1.5012f,
                            -2.2189f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.375f,
                            0.869f,
                            -3.0399f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.375f,
                            0.8653f,
                            3.0078f)));
                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.66f).Within(0.0002f));
                Assert.That(track.max.x, Is.EqualTo(-1.09f).Within(0.0002f));
                Assert.That(track.min.y, Is.EqualTo(0.01f).Within(0.0002f));
                Assert.That(track.max.y, Is.EqualTo(1.2898f).Within(0.0002f));
                Assert.That(track.min.z, Is.EqualTo(-3.4598f).Within(0.0002f));
                Assert.That(track.max.z, Is.EqualTo(3.4207f).Within(0.0002f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsKnifeHullCrewAndHybridDriveIdentity()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-AbramsX-KnifeGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-AbramsX-BowShoulder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-AbramsX-CrewHatch"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-AbramsX-KneedSkirt"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "AbramsX-SkirtBolt"),
                    Is.EqualTo(72));
                Assert.That(
                    Count(view, "AbramsX-HybridLouvre"),
                    Is.EqualTo(28));
                Assert.That(
                    Count(view, "Painted-AbramsX-HybridPlenum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "AbramsX-HybridChevronVane"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-AbramsX-SternServiceBox"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsCenteredTurretSensorsAndOpenXm914Station()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Transform turret =
                    Find(view, "TurretRoot");
                Assert.That(
                    turret.localPosition,
                    Is.EqualTo(
                        new Vector3(0f, 1.95f, -0.04f)));
                Assert.That(
                    Count(view, "Painted-AbramsX-LowTurretCore"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-AbramsX-FacetedCheek"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-AbramsX-GunTunnelJamb"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-AbramsX-PanoramaDRear"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "AbramsX-PanoramaLens"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-AbramsX-XM914Yoke"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "AbramsX-XM914Cartridge"),
                    Is.EqualTo(28));
                Assert.That(
                    Count(view, "AbramsX-XM914FeedReturn"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-AbramsX-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "AbramsX-NetworkMast"),
                    Is.EqualTo(2));
                Bounds ammo =
                    Find(view, "Painted-AbramsX-XM914AmmoBox")
                        .GetComponent<Renderer>()
                        .bounds;
                Bounds mouth =
                    Find(view, "AbramsX-XM914FeedMouth")
                        .GetComponent<Renderer>()
                        .bounds;
                Assert.That(
                    mouth.min.y,
                    Is.LessThanOrEqualTo(ammo.max.y));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Xm360UsesSlimGunOwnedPlantWithoutEvacuator()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-AbramsX-XM360Tube");
                AssertGunOwned(
                    view,
                    "Painted-AbramsX-XM360VentilatedShroud");
                AssertGunOwned(
                    view,
                    "AbramsX-XM360Bore");
                AssertGunOwned(
                    view,
                    "AbramsX-CoaxBarrel");
                Assert.That(
                    Count(view, "AbramsX-XM360Vent"),
                    Is.EqualTo(20));
                Assert.That(
                    CountPrefix(view, "Painted-AbramsX-BoreEvacuator"),
                    Is.EqualTo(0));
                Assert.That(
                    Find(view, "AbramsX-XM360Bore")
                        .localPosition.z,
                    Is.EqualTo(3.6905f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellWithoutPresentationColliders()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    { "Hull", "UpperHull", "Turret", "Gun",
                      "Armor-track_L", "Armor-track_R" })
                    AssertHidden(view, name);
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-Abrams-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-AbramsX-"),
                    Is.GreaterThan(50));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));
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
                catalog.GetVehicle("abramsx");
            return TankView.Create(
                new TankState(
                    "abramsx-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "sig_abramsx",
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
