using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90MSFleetVisualTests
    {
        [Test]
        public void BuildsSourceHullLoftAndCenterGlacis()
        {
            TankView view = Create();
            try
            {
                Mesh hull = Find(
                        view,
                        "Painted-T90MS-HullLoft")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    hull.bounds,
                    new Vector3(-1.60f, 0.44f, -3.43f),
                    new Vector3(1.60f, 1.545f, 3.43f));
                Assert.That(hull.vertexCount, Is.GreaterThan(900));

                Mesh glacis = Find(
                        view,
                        "Painted-T90MS-CenterGlacis")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    glacis.bounds,
                    new Vector3(-1.06f, 0.72f, 1.75f),
                    new Vector3(1.06f, 1.46f, 3.43f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericHullWithoutClaimingLaterStages()
        {
            TankView view = Create();
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
                    view.Root.GetComponentsInChildren<Transform>(true)
                        .Count(item =>
                            item.name.StartsWith("Soviet-") ||
                            item.name.StartsWith("Painted-Soviet-")),
                    Is.GreaterThanOrEqualTo(12));
                Assert.That(
                    Find(view, "Turret")
                        .GetComponent<Renderer>().enabled,
                    Is.True);
                Assert.That(
                    Find(view, "Gun")
                        .GetComponent<Renderer>().enabled,
                    Is.True);
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90ms");
            return TankView.Create(
                new TankState(
                    "t90ms-visual-test",
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

        private static void AssertBounds(
            Bounds bounds,
            Vector3 minimum,
            Vector3 maximum)
        {
            Assert.That(bounds.min.x,
                Is.EqualTo(minimum.x).Within(0.0001f));
            Assert.That(bounds.min.y,
                Is.EqualTo(minimum.y).Within(0.0001f));
            Assert.That(bounds.min.z,
                Is.EqualTo(minimum.z).Within(0.0001f));
            Assert.That(bounds.max.x,
                Is.EqualTo(maximum.x).Within(0.0001f));
            Assert.That(bounds.max.y,
                Is.EqualTo(maximum.y).Within(0.0001f));
            Assert.That(bounds.max.z,
                Is.EqualTo(maximum.z).Within(0.0001f));
        }
    }
}
