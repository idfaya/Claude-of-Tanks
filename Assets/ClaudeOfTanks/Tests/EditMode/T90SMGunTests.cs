using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90SMGunTests
    {
        [Test]
        public void BuildsSourceTwoA46M5AndReplacesGenericGun()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Find(view, "Gun")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
                Transform assembly =
                    Find(view, "T90SM-2A46M5Assembly");
                AssertVector(
                    assembly.localPosition,
                    new Vector3(0f, 0.288f, 1.17f));
                Assert.That(
                    Count(view, "Painted-T90SM-2A46M5Saddle"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-2A46M5RootCone"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-MantletPlug"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-CanvasCover"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-CanvasSideStrap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-BootCrease"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-CoaxPort"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-2A46M5ThermalSleeve"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-2A46M5ForwardTube"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-2A46M5SleeveRing"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-T90SM-2A46M5FumeExtractor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-2A46M5FumeExtractorBand"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-2A46M5MuzzleBore"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-2A46M5MuzzleBoreDisc"),
                    Is.EqualTo(1));

                Transform bore =
                    Find(view, "T90SM-2A46M5MuzzleBore");
                AssertVector(
                    assembly.InverseTransformPoint(bore.position),
                    new Vector3(0f, -0.012f, 4.986f));
                Assert.That(
                    Find(
                            view,
                            "Painted-T90SM-2A46M5ForwardTube")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds.extents.x,
                    Is.EqualTo(0.102f).Within(0.0001f));
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
                    "t90sm-gun-test",
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

        private static void AssertVector(
            Vector3 actual,
            Vector3 expected)
        {
            Assert.That(actual.x,
                Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(actual.y,
                Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(actual.z,
                Is.EqualTo(expected.z).Within(0.0001f));
        }
    }
}
