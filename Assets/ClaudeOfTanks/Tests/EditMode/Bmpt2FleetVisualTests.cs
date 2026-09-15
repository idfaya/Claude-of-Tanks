using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Bmpt2FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndOffsetT72RunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("bmpt_terminator2");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(153));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(Count(view, "MissilePod"), Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(12));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));

                float[] stations =
                    view.Root
                        .GetComponentsInChildren<Transform>()
                        .Where(item =>
                            item.name == "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .OrderBy(value => value)
                        .ToArray();
                Assert.That(
                    stations,
                    Is.EqualTo(
                        new[]
                        {
                            -2.997f,
                            -2.221f,
                            -1.445f,
                            -0.669f,
                            0.107f,
                            0.883f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.33f, 0.75f, -3.417f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.33f, 0.62f, 1.463f)));

                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(
                    track.min.x,
                    Is.EqualTo(-1.62f).Within(0.001f));
                Assert.That(
                    track.max.x,
                    Is.EqualTo(-1.04f).Within(0.001f));
                Assert.That(
                    track.min.y,
                    Is.EqualTo(0.0025f).Within(0.015f));
                Assert.That(
                    track.max.y,
                    Is.EqualTo(1.0899f).Within(0.015f));
                Assert.That(
                    track.min.z,
                    Is.EqualTo(-3.8053f).Within(0.03f));
                Assert.That(
                    track.max.z,
                    Is.EqualTo(1.6499f).Within(0.03f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void RetainsDonorHullAndAddsBmptProtection()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
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
                    Count(view, "Painted-Bmpt2-SidePanel"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-Bmpt2-GlacisTile"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Bmpt2-FenderNotchBridge"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bmpt2-FuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bmpt2-UnditchingLog"),
                    Is.EqualTo(1));
                Assert.That(
                    Find(view, "Turret")
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
        public void DedicatedStationKeepsTwinCannonsAtakaAndRoofKit()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Find(view, "Bmpt2-Station")
                        .localPosition.z,
                    Is.EqualTo(-0.32f).Within(0.0001f));
                Assert.That(
                    Count(view, "Painted-Bmpt2-Turntable"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmpt2-2A42Barrel"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bmpt2-MuzzleBore"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bmpt2-AtakaTube"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Bmpt2-AtakaMouth"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-Bmpt2-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Bmpt2-RadioAntenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bmpt2-RoofMgBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmpt2-PanoramaLens"),
                    Is.EqualTo(1));
                AssertGunOwned(
                    view,
                    "Painted-Bmpt2-2A42Barrel");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void TerminatorTwoDoesNotReplaceBmptT90Owner()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView bmpt2 = Create(catalog);
            TankView bmptT90 = Create(catalog, "bmpt_t90");
            try
            {
                Assert.That(
                    Count(bmpt2, "Painted-Bmpt2-WeaponTower"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        bmpt2,
                        "Painted-BmptT90-WeaponStation"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        bmptT90,
                        "Painted-BmptT90-WeaponStation"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        bmptT90,
                        "Painted-Bmpt2-WeaponTower"),
                    Is.EqualTo(0));
            }
            finally
            {
                bmpt2.Destroy();
                bmptT90.Destroy();
            }
        }

        private static TankView Create(
            ContentCatalog catalog,
            string id = "bmpt_terminator2")
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

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.parent.name,
                Is.EqualTo("Gun"));
            Vector3 product = Vector3.Scale(
                part.parent.parent.localScale,
                part.parent.parent.parent.localScale);
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
