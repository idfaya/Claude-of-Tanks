using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90AFleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndNativeSixWheelCourse()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90a");
            TankView view =
                Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(145));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(definition.armor.turretPlates)
                        .Count(plate => plate.kind == "era"),
                    Is.EqualTo(105));
                Assert.That(
                    FindAll(view, "RoadWheel-L").Length,
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(view, "RoadWheel-R").Length,
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90A-ReturnRoller"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90A-RoadWheelDish"),
                    Is.EqualTo(12));
                Transform dish =
                    Find(view, "T90A-RoadWheelDish");
                Assert.That(
                    Mathf.Abs(dish.localPosition.x),
                    Is.EqualTo(1.395f).Within(0.0001f));
                Assert.That(
                    MaximumRadiusX(
                        dish.GetComponent<MeshFilter>()
                            .sharedMesh.vertices),
                    Is.EqualTo(0.3234f).Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.395f,
                        0.95f,
                        -2.42f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.395f,
                        0.66f,
                        2.83f)));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesSharedSovietFallbackAndKeepsEraVisible()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    VisibleArmor(view),
                    Is.EqualTo(105));
                Assert.That(
                    Count(view, "Soviet-ShtoraLens"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Painted-Soviet-RearFuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    view.Root.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsT90AHullAndServiceIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T90A-UpperHull"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90A-K5GlacisCassette"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90A-K5GlacisSeam"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T90A-K5SkirtCassette"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T90A-RubberSkirtBand"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-T90A-RearFuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90A-SplitUnditchingLog"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90A-RearFuelDrumFrontBand"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90A-RearFuelDrumStrap"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90A-UnditchingLogStrap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90A-TowHook"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsChevronShtoraEssaAndRoofStations()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T90A-WeldedCheekFoundation"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90A-K5ChevronUpper"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90A-K5ChevronLower"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90A-ShtoraHousing"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90A-ShtoraLens"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90A-ESSAHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90A-RoofCupola"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90A-RemoteNsvtReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90A-SmokeLauncher"),
                    Is.EqualTo(12));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsTwoA46M2OnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "Painted-T90A-CastGunCollar",
                        "Painted-T90A-2A46M2RootSleeve",
                        "Painted-T90A-RecoilCover",
                        "Painted-T90A-2A46M2Evacuator",
                        "Painted-T90A-2A46M2ForwardTube",
                        "T90A-MuzzleBore"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(view, "T90A-2A46M2SleeveRing"),
                    Is.EqualTo(5));
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
                catalog.GetVehicle("t90a");
            return TankView.Create(
                new TankState(
                    "t90a-test",
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
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
        }

        private static int VisibleArmor(
            TankView view)
        {
            return FindAllByPrefix(view, "Armor-")
                .Count(item =>
                    item.GetComponent<Renderer>().enabled);
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
            return FindAllByPrefix(view, prefix).Length;
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name == name)
                .ToArray();
        }

        private static Transform[] FindAllByPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name.StartsWith(prefix))
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return FindAll(view, name).First();
        }

        private static float MaximumRadiusX(Vector3[] vertices)
        {
            float radius = 0f;
            for (int index = 0; index < vertices.Length; index++)
            {
                radius = Mathf.Max(
                    radius,
                    Mathf.Sqrt(
                        vertices[index].y * vertices[index].y +
                        vertices[index].z * vertices[index].z));
            }
            return radius;
        }
    }
}
