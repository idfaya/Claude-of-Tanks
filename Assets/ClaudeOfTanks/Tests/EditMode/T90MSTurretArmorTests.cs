using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90MSTurretArmorTests
    {
        [Test]
        public void BuildsChevronTilesOpticsAndProjectedRelikt()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90MS-ChevronCarrier"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90MS-NoseReliktBacking"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "Painted-T90MS-NoseReliktFace"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "T90MS-ChevronGapPlate"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90MS-FrontalOpticCradle"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-FrontalOpticRim"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-FrontalOpticLens"),
                    Is.EqualTo(2));

                Assert.That(
                    Count(view, "T90MS-FlankReliktBacking"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-T90MS-FlankRelikt"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90MS-FlankReliktInsert"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-T90MS-LowerFlankRelikt"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90MS-LowerFlankReliktInsert"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90MS-ShoulderReliktBacking"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T90MS-ShoulderRelikt"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90MS-ShoulderReliktInsert"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T90MS-RoofRelikt"),
                    Is.EqualTo(5));

                Transform[] projected = view.Root
                    .GetComponentsInChildren<Transform>(true)
                    .Where(item =>
                        item.name == "T90MS-FlankReliktBacking" ||
                        item.name == "Painted-T90MS-FlankRelikt" ||
                        item.name == "T90MS-FlankReliktInsert" ||
                        item.name == "Painted-T90MS-LowerFlankRelikt" ||
                        item.name == "T90MS-LowerFlankReliktInsert" ||
                        item.name == "T90MS-ShoulderReliktBacking" ||
                        item.name == "Painted-T90MS-ShoulderRelikt" ||
                        item.name == "T90MS-ShoulderReliktInsert" ||
                        item.name == "Painted-T90MS-RoofRelikt")
                    .ToArray();
                Assert.That(projected.Length, Is.EqualTo(54));
                Assert.That(
                    projected.All(item =>
                        item.GetComponent<MeshFilter>()
                            .sharedMesh.vertexCount == 36),
                    Is.True);
                Assert.That(
                    projected.All(item =>
                        HasFiniteBounds(
                            item.GetComponent<MeshFilter>()
                                .sharedMesh.bounds)),
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
                    "t90ms-turret-armor-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == name);
        }

        private static bool HasFiniteBounds(Bounds bounds)
        {
            return IsFinite(bounds.min.x) &&
                IsFinite(bounds.min.y) &&
                IsFinite(bounds.min.z) &&
                IsFinite(bounds.max.x) &&
                IsFinite(bounds.max.y) &&
                IsFinite(bounds.max.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}
