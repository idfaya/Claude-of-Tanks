using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90SMFleetVisualTests
    {
        [Test]
        public void BuildsSourceHullLoftAndFinalCoreReceipts()
        {
            TankView view = Create();
            try
            {
                Transform hull = Find(view, "Painted-T90SM-HullLoft");
                Mesh mesh = hull
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    mesh.bounds,
                    new Vector3(-1.60f, 0.44f, -2.92f),
                    new Vector3(1.60f, 1.45f, 3.02f));
                Assert.That(mesh.vertexCount, Is.GreaterThan(900));
                Assert.That(
                    Count(view, "Painted-T90SM-BellyEdgeChannel"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-CenterKeel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-RightInnerSkirtLug"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-RearDeckModule"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-HullModuleFoot"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-T90SM-BowModuleLap"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericGearAndSharedSovietFallback()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    view.Root.GetComponentsInChildren<Transform>(true)
                        .Count(item =>
                            item.name.StartsWith("Soviet-") ||
                            item.name.StartsWith("Painted-Soviet-")),
                    Is.EqualTo(0));
                Assert.That(
                    Find(view, "Hull")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
                Assert.That(
                    Find(view, "UpperHull")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
                Assert.That(
                    Count(view, "Painted-T90SM-RoadWheel"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90SM-RoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90SM-Sprocket"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-Idler"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-ReturnRoller"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90SM-TrackPad"),
                    Is.GreaterThan(100));
                Renderer[] genericGear = view.Root
                    .GetComponentsInChildren<Renderer>(true)
                    .Where(item =>
                        item.name.StartsWith("RoadWheel-") ||
                        item.name.StartsWith("Sprocket-") ||
                        item.name.StartsWith("Idler-") ||
                        item.name.StartsWith("ReturnRoller-") ||
                        item.name.StartsWith("TrackLinks-"))
                    .ToArray();
                Assert.That(genericGear.Length, Is.GreaterThan(0));
                Assert.That(
                    genericGear.All(item => !item.enabled),
                    Is.True);
                Assert.That(
                    Find(view, "Turret")
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
                catalog.GetVehicle("t90sm");
            return TankView.Create(
                new TankState(
                    "t90sm-visual-test",
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
