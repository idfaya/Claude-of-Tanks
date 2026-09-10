using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T80FleetVisualTests
    {
        [TestCase("t80", 1.05f)]
        [TestCase("t80b", 1.05f)]
        [TestCase("t80bv", 1.02f)]
        public void BuildsFinalSharedPressureHull(
            string id,
            float rearLowerHalfWidth)
        {
            TankView view = Create(id);
            try
            {
                Transform presentation =
                    Find(view, "T80-HullPresentationRoot");
                Assert.That(
                    presentation.localPosition,
                    Is.EqualTo(Vector3.zero));
                Mesh hull = Find(view, "Painted-T80-HullLoft")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    hull.bounds,
                    new Vector3(-1.28f, 0.44f, -3.26f),
                    new Vector3(1.28f, 1.503f, 3.05f));
                Assert.That(hull.vertexCount, Is.GreaterThan(900));
                float rearLower = hull.vertices
                    .Where(point =>
                        Mathf.Abs(point.z + 3.26f) < 0.0001f &&
                        point.y < 1.40f)
                    .Max(point => Mathf.Abs(point.x));
                Assert.That(
                    rearLower,
                    Is.EqualTo(rearLowerHalfWidth).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t80")]
        [TestCase("t80b")]
        [TestCase("t80bv")]
        public void ReplacesGenericHullButKeepsPendingFallbacks(
            string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(
                    Find(view, "Hull")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
                Assert.That(
                    Find(view, "UpperHull")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
                Assert.That(
                    view.Root.GetComponentsInChildren<Renderer>(true)
                        .Where(item =>
                            item.name.StartsWith("Armor-"))
                        .All(item => !item.enabled),
                    Is.True);
                Assert.That(
                    Count(view, "Painted-Soviet-FuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-Soviet-SmokeLauncher"),
                    Is.GreaterThan(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t80")]
        [TestCase("t80b")]
        [TestCase("t80bv")]
        public void BuildsPressedSixWheelLinkedCourse(string id)
        {
            TankView view = Create(id);
            try
            {
                Assert.That(Count(view, "T80-RoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "T80-RoadWheelTireShoulder"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "Painted-T80-RoadWheelDisc"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "Painted-T80-RoadWheelRib"),
                    Is.EqualTo(72));
                Assert.That(Count(view, "T80-RoadWheelDishWell"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "Painted-T80-RoadWheelHub"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "Painted-T80-RoadWheelHubCap"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "T80-RoadWheelBolt"),
                    Is.EqualTo(72));

                Assert.That(Count(view, "T80-SuspensionArm"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "T80-SuspensionJoint"),
                    Is.EqualTo(24));
                Assert.That(Count(view, "T80-SuspensionJointStep"),
                    Is.EqualTo(24));
                Assert.That(Count(view, "T80-SuspensionJointCap"),
                    Is.EqualTo(24));

                Assert.That(Count(view, "Painted-T80-SprocketBody"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T80-SprocketCarrierRing"),
                    Is.EqualTo(4));
                Assert.That(Count(view, "T80-SprocketTooth"),
                    Is.EqualTo(40));
                Assert.That(Count(view, "T80-SprocketBolt"),
                    Is.EqualTo(16));
                Assert.That(Count(view, "Painted-T80-IdlerBody"),
                    Is.EqualTo(2));
                Assert.That(Count(view, "Painted-T80-IdlerDish"),
                    Is.EqualTo(4));
                Assert.That(Count(view, "T80-IdlerLighteningHole"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "T80-IdlerBolt"),
                    Is.EqualTo(16));
                Assert.That(Count(view, "Painted-T80-ReturnRollerDisc"),
                    Is.EqualTo(10));

                Transform[] pads = view.Root
                    .GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name == "T80-TrackPad")
                    .ToArray();
                Assert.That(pads.Length, Is.GreaterThan(100));
                Assert.That(
                    pads[0].GetComponent<MeshFilter>()
                        .sharedMesh.bounds.size.x,
                    Is.EqualTo(0.58f).Within(0.0001f));
                Assert.That(
                    view.Root.GetComponentsInChildren<Renderer>(true)
                        .Where(IsGenericGear)
                        .All(item => !item.enabled),
                    Is.True);
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-visual-test",
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

        private static bool IsGenericGear(Renderer renderer)
        {
            string name = renderer.name;
            return name.StartsWith("RoadWheel-") ||
                name.StartsWith("Sprocket-") ||
                name.StartsWith("Idler-") ||
                name.StartsWith("ReturnRoller-") ||
                name.StartsWith("TrackLinks-");
        }

        private static void AssertBounds(
            Bounds actual,
            Vector3 minimum,
            Vector3 maximum)
        {
            const float tolerance = 0.0001f;
            Assert.That(actual.min.x,
                Is.EqualTo(minimum.x).Within(tolerance));
            Assert.That(actual.min.y,
                Is.EqualTo(minimum.y).Within(tolerance));
            Assert.That(actual.min.z,
                Is.EqualTo(minimum.z).Within(tolerance));
            Assert.That(actual.max.x,
                Is.EqualTo(maximum.x).Within(tolerance));
            Assert.That(actual.max.y,
                Is.EqualTo(maximum.y).Within(tolerance));
            Assert.That(actual.max.z,
                Is.EqualTo(maximum.z).Within(tolerance));
        }
    }
}
