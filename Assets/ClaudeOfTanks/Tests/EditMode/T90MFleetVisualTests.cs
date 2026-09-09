using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90MFleetVisualTests
    {
        [TestCase("t90m")]
        [TestCase("t90m_proryv")]
        public void BuildsFinalSharedPressureHullCore(string id)
        {
            TankView view = Create(id);
            try
            {
                Transform root =
                    Find(view, "T90M-HullPresentationRoot");
                Assert.That(root.localPosition, Is.EqualTo(Vector3.zero));

                Mesh hull = Find(view, "Painted-T90M-HullLoft")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    hull.bounds,
                    new Vector3(-1.60f, 0.40f, -3.43f),
                    new Vector3(1.60f, 1.505f, 3.43f));
                Assert.That(hull.vertexCount, Is.GreaterThan(900));

                Mesh glacis =
                    Find(view, "Painted-T90M-CenterGlacis")
                        .GetComponent<MeshFilter>()
                        .sharedMesh;
                AssertBounds(
                    glacis.bounds,
                    new Vector3(-1.42f, 0.78f, 2.58f),
                    new Vector3(1.42f, 1.43f, 3.16f));
                Assert.That(
                    Count(view, "Painted-T90M-ShoulderBridge"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90M-ShoulderBridgeSeam"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("t90m")]
        [TestCase("t90m_proryv")]
        public void ReplacesGenericHullAndRearFallback(string id)
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
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Painted-Soviet-FuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Soviet-UnditchingLog"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition = catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-hull-test",
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
            Vector3 expectedMin,
            Vector3 expectedMax)
        {
            Assert.That(bounds.min.x,
                Is.EqualTo(expectedMin.x).Within(0.001f));
            Assert.That(bounds.min.y,
                Is.EqualTo(expectedMin.y).Within(0.001f));
            Assert.That(bounds.min.z,
                Is.EqualTo(expectedMin.z).Within(0.001f));
            Assert.That(bounds.max.x,
                Is.EqualTo(expectedMax.x).Within(0.001f));
            Assert.That(bounds.max.y,
                Is.EqualTo(expectedMax.y).Within(0.001f));
            Assert.That(bounds.max.z,
                Is.EqualTo(expectedMax.z).Within(0.001f));
        }
    }
}
