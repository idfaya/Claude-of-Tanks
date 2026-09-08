using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Bwp1FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndBmp2RunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("bwp1");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(17));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "MissilePod"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(12));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));

                Transform[] wheels =
                    view.Root
                        .GetComponentsInChildren<Transform>()
                        .Where(item =>
                            item.name.StartsWith(
                                "RoadWheel-"))
                        .ToArray();
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(
                            Mathf.Abs(item.localPosition.x) -
                            1.205f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(item.localPosition.y - 0.3f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.205f,
                            0.8f,
                            2.256f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.205f,
                            0.6f,
                            -2.44f)));
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                AssertHidden(view, "Armor-track_L");
                AssertHidden(view, "Armor-track_R");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void PolishHullRetainsDonorAndAddsProtection()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    Count(view, "Painted-Bmp2-CenterTub"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-FiringPort"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(view, "Painted-Bmp2-RearDoor"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bwp1-SidePanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(view, "Painted-Bwp1-GlacisPanel"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "Painted-Bwp1-LightPlatform"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bwp1-Headlight"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void DedicatedStationKeepsSensorsCrewAndEquipment()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    view.Root.Find("TurretRoot")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(0f, 1.66f, 0.03f)));
                Assert.That(
                    Count(view, "Painted-Bwp1-LowTurret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bwp1-SensorHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bwp1-SensorLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bwp1-CrewCupola"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bwp1-CrewHatch"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bwp1-TurretSidePanel"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-Bwp1-SmokeLauncher"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Bwp1-RadioAntenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bwp1-RoofMgBarrel"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Mk30FollowsGunAndOnlyPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "digital");
            try
            {
                Assert.That(
                    view.Root
                        .Find("TurretRoot/Gun")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            0f,
                            0.285f,
                            1.81f)));
                AssertGunOwned(
                    view,
                    "Painted-Bwp1-Mk30Barrel");
                AssertGunOwned(
                    view,
                    "Bwp1-MuzzleBore");
                Renderer painted = Find(
                        view,
                        "Painted-Bwp1-SidePanel")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        "Bwp1-SensorLens")
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

        private static TankView Create(
            ContentCatalog catalog,
            string camouflage = "factory")
        {
            VehicleDefinition definition =
                catalog.GetVehicle("bwp1");
            return TankView.Create(
                new TankState(
                    "bwp1-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                camouflage,
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
