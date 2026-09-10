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
