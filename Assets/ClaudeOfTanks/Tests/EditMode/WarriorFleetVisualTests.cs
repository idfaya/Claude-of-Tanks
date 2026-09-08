using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class WarriorFleetVisualTests
    {
        [TestCase("fv510")]
        [TestCase("fv510_milan")]
        public void UsesCatalogArmorAndExactWarriorCourse(
            string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            TankView view = Create(catalog, id);
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
                        new Vector3(-1.276f, 0.64f, 2.6412f)));
                Assert.That(
                    Find(view, "Idler-L").localPosition,
                    Is.EqualTo(
                        new Vector3(-1.276f, 0.62f, -2.6093f)));
                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.529f).Within(0.002f));
                Assert.That(track.max.x, Is.EqualTo(-1.023f).Within(0.002f));
                Assert.That(track.min.y, Is.EqualTo(0.0302f).Within(0.02f));
                Assert.That(track.max.y, Is.EqualTo(1.1902f).Within(0.02f));
                Assert.That(track.min.z, Is.EqualTo(-3.1597f).Within(0.03f));
                Assert.That(track.max.z, Is.EqualTo(3.2147f).Within(0.03f));
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
        public void BaseWarriorKeepsWrapHullAndRardenStation()
        {
            TankView view =
                Create(ContentCatalog.Load(), "fv510");
            try
            {
                Assert.That(
                    Count(view, "Painted-Warrior-WrapPanel"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Warrior-WrapStrake"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "Painted-Warrior-LowerDrop"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "Painted-Warrior-RearDoor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Warrior-LeftExhaustCowl"),
                    Is.EqualTo(1));
                Assert.That(
                    view.Root.Find("TurretRoot").localPosition,
                    Is.EqualTo(
                        new Vector3(
                            0f,
                            1.7805369f,
                            -0.4355725f)));
                Assert.That(
                    Count(view, "Painted-Warrior-WeldedTurret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-Warrior-SmokeLauncher"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-Warrior-RardenBarrel"),
                    Is.EqualTo(1));
                AssertGunOwned(
                    view,
                    "Painted-Warrior-RardenBarrel");
                Assert.That(
                    Count(view, "WarriorMilan-LauncherTube"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void MilanVariantAddsProtectionAndThreeTubesOnly()
        {
            TankView view =
                Create(ContentCatalog.Load(), "fv510_milan");
            try
            {
                Assert.That(
                    Count(view, "Painted-WarriorMilan-GlacisTile"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-WarriorMilan-SideArmor"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "WarriorMilan-LauncherTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "WarriorMilan-SpareTube"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "WarriorMilan-SightLens"),
                    Is.EqualTo(1));
                Renderer painted = Find(
                        view,
                        "Painted-WarriorMilan-SideArmor")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                        view,
                        "WarriorMilan-SightLens")
                    .GetComponent<Renderer>();
                Assert.That(painted.sharedMaterial.mainTexture, Is.Not.Null);
                Assert.That(lens.sharedMaterial.mainTexture, Is.Null);
            }
            finally
            {
                view.Destroy();
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
