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

        [Test]
        public void BuildsJoinedBustleMagazineAndThreeSidedCage()
        {
            TankView view = Create();
            try
            {
                Mesh shoulder = Find(
                        view,
                        "Painted-T90MS-BustleShoulder")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    shoulder.bounds,
                    new Vector3(-1.18f, 0.36f, -2.39f),
                    new Vector3(1.18f, 0.72f, -0.46f));
                Assert.That(shoulder.vertexCount, Is.EqualTo(240));

                Mesh magazine = Find(
                        view,
                        "Painted-T90MS-BustleMagazine")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    magazine.bounds,
                    new Vector3(-1.15f, 0.18f, -2.39f),
                    new Vector3(1.15f, 0.70f, -1.18f));
                Assert.That(magazine.vertexCount, Is.EqualTo(204));

                Assert.That(
                    Count(view, "T90MS-BustleServiceLid"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90MS-BustleServiceLatch"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90MS-BustleRearPost"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90MS-BustleRearRail"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "Painted-T90MS-BustleSideShoulder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-BustleSideStrut"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-BustleCageRearRail"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90MS-BustleCageRearPost"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90MS-BustleCageSideRail"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90MS-BustleCageSidePost"),
                    Is.EqualTo(8));
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
