using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Type89FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndBradleyShapedCourse()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("type89");
            TankView view = Create(catalog, "type89");
            try
            {
                Assert.That(CountPrefix(view, "Armor-"), Is.EqualTo(19));
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
                Assert.That(
                    Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.25f, 0.6f, 2.62f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.25f, 0.7f, -2.62f)));
                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.47f).Within(0.002f));
                Assert.That(track.max.x, Is.EqualTo(-1.03f).Within(0.002f));
                Assert.That(track.min.y, Is.EqualTo(0.01f).Within(0.02f));
                Assert.That(track.max.y, Is.EqualTo(1.3943f).Within(0.02f));
                Assert.That(track.min.z, Is.EqualTo(-3.19f).Within(0.03f));
                Assert.That(track.max.z, Is.EqualTo(3.0833f).Within(0.03f));
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
        public void LongGlacisKeepsJapaneseHullIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load(), "type89");
            try
            {
                Assert.That(
                    Count(view, "Painted-Type89-LongGlacis"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Type89-ProwWall"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Type89-ThinSkirt"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Type89-DriverHatch"),
                    Is.EqualTo(1));
                Assert.That(
                    Find(view, "Painted-Type89-DriverHatch")
                        .localPosition.x,
                    Is.GreaterThan(0f));
                Assert.That(
                    Count(view, "Type89-PowerpackLouvre"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "Type89-FiringPort"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Type89-FiringPortVision"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-Type89-RearDoor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Type89-LeftExhaust"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void OffsetTurretKeepsTwinJyuMatAndNoRoofMachineGun()
        {
            TankView view =
                Create(ContentCatalog.Load(), "type89");
            try
            {
                Assert.That(
                    view.Root.Find("TurretRoot")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(0.25f, 1.8f, -0.1f)));
                Assert.That(
                    Count(view, "Painted-Type89-WeldedTurret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Type89-JyuMatBox"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Type89-JyuMatMouth"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Type89-SmokeLauncher"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-Type89-CommanderCupola"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Type89-GunnerSightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "Type89-RoofMg"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KdePlantFollowsGunWithoutClaimingLightTiger()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView type89 = Create(catalog, "type89");
            TankView lightTiger =
                Create(catalog, "type89_light_tiger");
            try
            {
                Assert.That(
                    type89.Root.Find("TurretRoot/Gun")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(-0.05f, 0.3f, 2.275f)));
                AssertGunOwned(
                    type89,
                    "Painted-Type89-KdeBarrel");
                AssertGunOwned(
                    type89,
                    "Type89-MuzzleBore");
                Assert.That(
                    Count(type89, "Type89-FlashVentRing"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(type89, "Type89-CoaxBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        lightTiger,
                        "Painted-Type89-WeldedTurret"),
                    Is.EqualTo(0));
            }
            finally
            {
                type89.Destroy();
                lightTiger.Destroy();
            }
        }

        private static TankView Create(
            ContentCatalog catalog,
            string id)
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
            Assert.That(part.parent.parent.name, Is.EqualTo("Gun"));
            Vector3 product = Vector3.Scale(
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
                .Count(item => item.name.StartsWith(prefix));
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
