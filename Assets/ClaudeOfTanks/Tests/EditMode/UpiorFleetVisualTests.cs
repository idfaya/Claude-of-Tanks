using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class UpiorFleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactNarrowRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("upior");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(19));
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
                            0.94f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(item.localPosition.y - 0.29f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(item.localScale.x - 0.235f) <
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
                            -1.628f,
                            -1.032f,
                            -0.435f,
                            0.345f,
                            0.978f,
                            1.577f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-0.94f, 0.5f, -2.1f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-0.94f, 0.58f, 2.2f)));

                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(
                    track.min.x,
                    Is.EqualTo(-1.12f).Within(0.001f));
                Assert.That(
                    track.max.x,
                    Is.EqualTo(-0.76f).Within(0.001f));
                Assert.That(
                    track.min.y,
                    Is.EqualTo(0f).Within(0.015f));
                Assert.That(
                    track.max.y,
                    Is.EqualTo(0.865f).Within(0.015f));
                Assert.That(
                    track.min.z,
                    Is.EqualTo(-2.37f).Within(0.03f));
                Assert.That(
                    track.max.z,
                    Is.EqualTo(2.4699f).Within(0.03f));
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
        public void FacetedHullKeepsCorrectBowSternAndOpenGear()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-Upior-CrownFacet"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Upior-RakedGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Upior-TowShackle"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Upior-RearDoor"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Upior-Waterjet"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Upior-SkirtPanel"),
                    Is.EqualTo(26));
                Assert.That(
                    Count(view, "Painted-Upior-SponsonClosure"),
                    Is.EqualTo(20));
                Assert.That(
                    Count(view, "Upior-ReturnRoller"),
                    Is.EqualTo(6));
                Assert.That(
                    Find(view, "Painted-Upior-NoseBeam")
                        .localPosition.z,
                    Is.GreaterThan(0f));
                Assert.That(
                    Find(view, "Painted-Upior-RearDoor")
                        .localPosition.z,
                    Is.LessThan(0f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void RearTurretKeepsDrumSensorTowerAndRoofKit()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    view.Root.Find("TurretRoot")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(-0.1f, 1.47f, -0.74f)));
                Assert.That(
                    Count(view, "Painted-Upior-FacetedDrum"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Upior-SensorHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Upior-SensorAperture"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Upior-AtgmTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Upior-CommanderCupola"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Upior-GunnerHatch"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Upior-SmokeLauncher"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Upior-RadioAntenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Upior-RoofMgBarrel"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ThirtyMillimeterPlantFollowsGunAndKeepsOpticsNeutral()
        {
            TankView view = Create(
                ContentCatalog.Load(),
                "digital");
            try
            {
                Assert.That(
                    view.Root
                        .Find("TurretRoot/Gun")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(0f, 0.21f, 1.75f)));
                AssertGunOwned(
                    view,
                    "Painted-Upior-30mmBarrel");
                AssertGunOwned(
                    view,
                    "Upior-MuzzleBore");
                AssertGunOwned(
                    view,
                    "Upior-CoaxBarrel");
                Renderer painted = Find(
                        view,
                        "Painted-Upior-SkirtPanel")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        "Upior-SensorAperture")
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
                catalog.GetVehicle("upior");
            return TankView.Create(
                new TankState(
                    "upior-test",
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
