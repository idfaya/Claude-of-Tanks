using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Bmp2FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactSixWheelRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("bmp2");
            TankView view = Create(
                catalog,
                "bmp2");
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
                    Count(view, "MissilePod"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "SideArmor"),
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
                    wheels.All(item =>
                        Mathf.Abs(item.localScale.x - 0.3f) <
                        0.0001f),
                    Is.True);
                float[] stations =
                    wheels
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
                            -2.094f,
                            -1.374f,
                            -0.654f,
                            0.066f,
                            0.786f,
                            1.506f
                        })
                        .Within(0.0001f));
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
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localScale.x,
                    Is.EqualTo(0.26f));
                Assert.That(
                    Find(view, "Idler-L")
                        .localScale.x,
                    Is.EqualTo(0.24f));
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
        public void BoatHullKeepsProwPassengerKitAndModernizedProtection()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "bmp2");
            try
            {
                Assert.That(
                    Count(view, "Painted-Bmp2-CenterTub"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp2-Sponson"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bmp2-UpperGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp2-LowerProw"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-WaveBreakerRib"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Bmp2-FiringPort"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        view,
                        "Bmp2-FiringPortVisionBlock"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(view, "Painted-Bmp2-SideCassette"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bmp2-GlacisCassette"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-Bmp2-RearDoor"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bmp2-RearDoorBulge"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ConicalTurretKeepsKonkursCrewKitAndModernSensors()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "bmp2");
            try
            {
                Assert.That(
                    view.Root.Find("TurretRoot")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(0f, 1.66f, 0.03f)));
                Assert.That(
                    Count(view, "Painted-Bmp2-ConicalTurret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-KonkursTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-Tkn3Lens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-BpkSightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp2-SmokeLauncher"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bmp2-TurretCheekPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bmp2-TurretSidePanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bmp2-LaserWarningHead"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bmp2-CommanderThermalHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-RoofPktBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp2-RadioAntenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Find(view, "Bmp2-KonkursTube")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void TwoA42FollowsGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "bmp2",
                "digital");
            try
            {
                Assert.That(
                    view.Root
                        .Find("TurretRoot/Gun")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(0f, 0.285f, 1.81f)));
                AssertGunOwned(
                    view,
                    "Painted-Bmp2-2A42Barrel");
                AssertGunOwned(
                    view,
                    "Bmp2-2A42MuzzleBore");
                AssertGunOwned(
                    view,
                    "Bmp2-CoaxPktBarrel");
                Assert.That(
                    Count(view, "Bmp2-2A42GuideRail"),
                    Is.EqualTo(2));
                Renderer painted = Find(
                        view,
                        "Painted-Bmp2-SideCassette")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        "Bmp2-Tkn3Lens")
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
