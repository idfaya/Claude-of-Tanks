using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90AVladimirFleetVisualTests
    {
        [Test]
        public void BuildsSourceHullLoftAndRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90a_vladimir");
            TankView view = Create(catalog, definition);
            try
            {
                Transform hull = Find(
                    view,
                    "Painted-T90AVladimir-HullLoft");
                Mesh mesh = hull
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    mesh.bounds,
                    new Vector3(-1.60f, 0.42f, -4.755f),
                    new Vector3(1.60f, 1.671f, 2.10f));
                Assert.That(mesh.vertexCount, Is.GreaterThan(1000));
                Assert.That(
                    Count(view, "T90AVladimir-RoadWheelDish"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90AVladimir-RoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90AVladimir-RoadWheelHub"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90AVladimir-ReturnRoller"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90AVladimir-Sprocket"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90AVladimir-Idler"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90AVladimir-TrackPad"),
                    Is.EqualTo(248));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-Fender"),
                    Is.EqualTo(20));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-SideSkirt"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90AVladimir-ShtoraLens"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-LowerCheek"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-UpperCheek"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-Crown"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-K5Flank"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-SmokeCanister"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90AVladimir-KordTower"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90AVladimir-MuzzleBore"),
                    Is.EqualTo(1));
                Transform turret = view.Root.Find("TurretRoot");
                AssertVector(
                    Find(view, "T90AVladimir-PresentationRoot")
                        .localPosition,
                    new Vector3(0f, 0.10f, -0.90f));
                AssertVector(
                    turret.InverseTransformPoint(
                        Find(view, "T90AVladimir-GunFittings")
                            .position),
                    new Vector3(0f, 0.34f, 0.15f));
                AssertVector(
                    turret.InverseTransformPoint(
                        Find(view, "T90AVladimir-MuzzleBore")
                            .position),
                    new Vector3(0f, 0.34f, 5.486f));
                Assert.That(
                    view.Root.GetComponentsInChildren<Transform>(true)
                        .Count(item =>
                            item.name.StartsWith("Soviet-") ||
                            item.name.StartsWith("Painted-Soviet-")),
                    Is.EqualTo(0));
                Assert.That(
                    hull.GetComponent<Collider>(),
                    Is.Null);
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
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void RunningGearUsesVladimirSourceStations()
        {
            float[] expected =
            {
                1.03f, 0.28f, -0.47f,
                -1.22f, -1.97f, -2.72f
            };
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(
                    TankT90AVladimirFamilyDetails
                        .RoadWheelZ(index),
                    Is.EqualTo(expected[index])
                        .Within(0.0001f));
            }
        }

        private static TankView Create(
            ContentCatalog catalog,
            VehicleDefinition definition)
        {
            return TankView.Create(
                new TankState(
                    "t90a-vladimir-visual-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .First(item => item.name == name);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == name);
        }

        private static void AssertBounds(
            Bounds bounds,
            Vector3 minimum,
            Vector3 maximum)
        {
            Assert.That(
                bounds.min.x,
                Is.EqualTo(minimum.x).Within(0.0001f));
            Assert.That(
                bounds.min.y,
                Is.EqualTo(minimum.y).Within(0.0001f));
            Assert.That(
                bounds.min.z,
                Is.EqualTo(minimum.z).Within(0.0001f));
            Assert.That(
                bounds.max.x,
                Is.EqualTo(maximum.x).Within(0.0001f));
            Assert.That(
                bounds.max.y,
                Is.EqualTo(maximum.y).Within(0.0001f));
            Assert.That(
                bounds.max.z,
                Is.EqualTo(maximum.z).Within(0.0001f));
        }

        private static void AssertVector(
            Vector3 actual,
            Vector3 expected)
        {
            Assert.That(
                actual.x,
                Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(
                actual.y,
                Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(
                actual.z,
                Is.EqualTo(expected.z).Within(0.0001f));
        }
    }
}
