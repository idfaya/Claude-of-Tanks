using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Bmp3FleetVisualTests
    {
        [TestCase("bmp3")]
        [TestCase("bmp3_rok")]
        public void UsesCatalogArmorWithoutGenericIfvParts(
            string id)
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
        public void RussianHullKeepsRearDriveAndBoatEquipment()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "bmp3");
            try
            {
                Assert.That(
                    Count(view, "Painted-Bmp3-CenterTub"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp3-UpperGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp3-LowerProw"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp3-WaveBreakerRib"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-Bmp3-SponsonBin"),
                    Is.EqualTo(28));
                Assert.That(
                    Count(view, "Painted-Bmp3-BowMachineGunBall"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bmp3-BowPktBarrel"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bmp3-FlankCrewHatch"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bmp3-TroopHatch"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bmp3-WaterjetRim"),
                    Is.EqualTo(2));

                Transform[] wheels =
                    FindAll(view, "RoadWheel-L");
                Assert.That(
                    wheels.Select(item =>
                            item.localPosition.z)
                        .OrderBy(value => value)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            -2.15f,
                            -1.315f,
                            -0.62f,
                            0.055f,
                            1.04f,
                            1.79f
                        })
                        .Within(0.0001f));
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(item.localPosition.x + 1.32f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(item.localPosition.y - 0.37f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.32f,
                            0.72f,
                            -2.98f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.32f,
                            0.88f,
                            2.73f)));
                Assert.That(
                    FindAll(view, "Bmp3-ReturnRoller")
                        .Where(item =>
                            item.localPosition.x < 0f)
                        .Select(item =>
                            item.localPosition.z)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            1.3f,
                            -0.1f,
                            -1.5f
                        })
                        .Within(0.0001f));
                Assert.That(
                    FindAll(view, "Bmp3-ReturnRoller")
                        .All(item =>
                            Mathf.Abs(item.localPosition.y - 1.02f) <
                            0.0001f),
                    Is.True);
                Assert.That(
                    FindAll(view, "ReturnRoller-L")
                        .All(item =>
                            !item.GetComponent<Renderer>().enabled),
                    Is.True);
                Assert.That(
                    FindAll(view, "ReturnRoller-R")
                        .All(item =>
                            !item.GetComponent<Renderer>().enabled),
                    Is.True);
                Bounds trackBounds =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(
                    trackBounds.min.z,
                    Is.EqualTo(-3.419f).Within(0.04f));
                Assert.That(
                    trackBounds.max.z,
                    Is.EqualTo(3.11f).Within(0.04f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void RokVariantKeepsBmp2HullAndKoreanProtection()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "bmp3_rok");
            try
            {
                Assert.That(
                    Count(view, "Painted-Bmp2-CenterTub"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp2-SideCassette"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-Bmp3Rok-SidePanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(view, "Painted-Bmp3Rok-GlacisPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-Bmp3Rok-LightPlatform"),
                    Is.EqualTo(2));
                Assert.That(
                    FindAll(view, "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .OrderBy(value => value)
                        .ToArray(),
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
                        .localPosition.z,
                    Is.EqualTo(2.256f));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition.z,
                    Is.EqualTo(-2.44f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("bmp3", 1.85f, 0.24f, 6)]
        [TestCase("bmp3_rok", 1.66f, 0.03f, 8)]
        public void LowTurretKeepsCrewStationsAndEquipment(
            string id,
            float pivotY,
            float pivotZ,
            int smokeCount)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                id);
            try
            {
                Assert.That(
                    view.Root.Find("TurretRoot")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(0f, pivotY, pivotZ)));
                Assert.That(
                    Count(view, "Painted-Bmp3-LowTurret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp3-CrewCupola"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Bmp3-CrewHatch"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Bmp3-CupolaPeriscope"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Bmp3-CommanderSightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp3-TknSightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Bmp3-SmokeLauncher"),
                    Is.EqualTo(smokeCount));
                Assert.That(
                    Count(view, "Bmp3-RoofMgBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Bmp3-RadioAntenna"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("bmp3", 0.05f, 0.28f, 2.125f)]
        [TestCase("bmp3_rok", 0f, 0.285f, 1.81f)]
        public void TripleGunPlantFollowsGun(
            string id,
            float x,
            float y,
            float z)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                id,
                "digital");
            try
            {
                Assert.That(
                    view.Root
                        .Find("TurretRoot/Gun")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(x, y, z)));
                AssertGunOwned(
                    view,
                    "Painted-Bmp3-2A70Barrel");
                AssertGunOwned(
                    view,
                    "Bmp3-2A70MuzzleBore");
                AssertGunOwned(
                    view,
                    "Painted-Bmp3-2A72Barrel");
                AssertGunOwned(
                    view,
                    "Bmp3-2A72MuzzleBore");
                AssertGunOwned(
                    view,
                    "Bmp3-CoaxPktBarrel");
                Renderer painted = Find(
                        view,
                        "Painted-Bmp3-TripleGunCradle")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        "Bmp3-CommanderSightLens")
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
            return FindAll(view, name).Length;
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

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Where(item =>
                    item.name == name)
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return FindAll(view, name).First();
        }
    }
}
