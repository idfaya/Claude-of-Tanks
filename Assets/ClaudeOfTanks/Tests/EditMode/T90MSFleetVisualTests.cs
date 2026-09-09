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
        public void ReplacesGenericHullTurretAndGun()
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
                    Is.EqualTo(0));
                Assert.That(
                    Find(view, "Turret")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
                Assert.That(
                    Find(view, "Gun")
                        .GetComponent<Renderer>().enabled,
                    Is.False);
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsFinalSourceWeldedTurretCore()
        {
            TankView view = Create();
            try
            {
                Transform root =
                    Find(view, "T90MS-PresentationRoot");
                Assert.That(
                    root.localPosition,
                    Is.EqualTo(new Vector3(0f, 0.043f, -0.15f)));

                Mesh inner = Find(
                        view,
                        "Painted-T90MS-InnerWeldedShell")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    inner.bounds,
                    new Vector3(-1.434f, 0f, -1.58178f),
                    new Vector3(1.417f, 0.743f, 1.04f));
                Assert.That(inner.vertexCount, Is.EqualTo(744));

                Mesh outer = Find(
                        view,
                        "Painted-T90MS-OuterWeldedSkin")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    outer.bounds,
                    new Vector3(-1.58f, 0.03f, -1.72f),
                    new Vector3(1.58f, 0.61f, 1.34f));
                Assert.That(outer.vertexCount, Is.EqualTo(312));

                Assert.That(
                    Count(view, "T90MS-BuriedTurretRing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90MS-CrownFacet"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90MS-CrownWeld"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsSourceRunningGearAndLinkedTrackCourse()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "T90MS-RoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90MS-RoadWheelDish"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90MS-RoadWheelHub"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90MS-RoadWheelBolt"),
                    Is.EqualTo(96));
                Assert.That(
                    Count(view, "Painted-T90MS-Sprocket"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-Idler"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-ReturnRoller"),
                    Is.EqualTo(6));

                Transform[] pads = view.Root
                    .GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name == "T90MS-TrackPad")
                    .ToArray();
                Assert.That(pads.Length, Is.GreaterThan(100));
                Assert.That(pads.Length % 2, Is.EqualTo(0));
                Assert.That(
                    pads.Min(item => item.localPosition.y),
                    Is.EqualTo(0.05f).Within(0.0001f));
                Assert.That(
                    pads.Max(item => item.localPosition.y),
                    Is.GreaterThan(1.15f));
                Assert.That(
                    pads.Min(item => item.localPosition.x),
                    Is.EqualTo(-1.395f).Within(0.0001f));
                Assert.That(
                    pads.Max(item => item.localPosition.x),
                    Is.EqualTo(1.395f).Within(0.0001f));

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
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsFrontGuardsDeckReliktAndStowage()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90MS-FenderLip"),
                    Is.EqualTo(22));
                Assert.That(
                    Count(view, "Painted-T90MS-FrontGuardShoulder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-FrontGuardShell"),
                    Is.EqualTo(2));
                Mesh shoulder = Find(
                        view,
                        "Painted-T90MS-FrontGuardShoulder")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    shoulder.bounds,
                    new Vector3(-1.64f, 1.115f, 2.62f),
                    new Vector3(-0.92f, 1.445f, 3.28f));

                Assert.That(
                    Count(view, "Painted-T90MS-DriverHatch"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90MS-DriverPeriscope"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-EngineGrille"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90MS-TowEye"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-Headlight"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-BowServiceBlock"),
                    Is.EqualTo(1));

                Assert.That(
                    Count(view, "Painted-T90MS-GlacisRelikt"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90MS-GlacisReliktFace"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90MS-GlacisReliktSeam"),
                    Is.EqualTo(8));

                Assert.That(
                    Count(view, "T90MS-BowTowCable"),
                    Is.EqualTo(1));
                Assert.That(
                    Find(view, "T90MS-BowTowCable")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.vertexCount,
                    Is.EqualTo(126));
                Assert.That(
                    Count(view, "T90MS-UnditchingLog"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90MS-LogStrap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-SpareTrackLink"),
                    Is.EqualTo(4));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsSourceSkirtsTailEquipmentAndPerimeterCage()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90MS-TailRack"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-TailOuterRack"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-RearFuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-RearFuelDrumBand"),
                    Is.EqualTo(4));

                Assert.That(
                    Count(view, "Painted-T90MS-SkirtRelikt"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90MS-SkirtReliktFaceSeam"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(view, "T90MS-SkirtReliktRubberHem"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Painted-T90MS-SkirtPanel"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "T90MS-SkirtBatten"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "T90MS-SkirtBolt"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "T90MS-SkirtBottomLip"),
                    Is.EqualTo(14));

                Assert.That(
                    Count(view, "T90MS-FlankCageRail"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "T90MS-FlankCagePost"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90MS-FlankCageBracket"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90MS-TransomCageRail"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90MS-TransomCagePost"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90MS-ServiceBayBacking"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-T90MS-ServiceLouvre"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-T90MS-WidthAnchor"),
                    Is.EqualTo(2));

                Assert.That(
                    Count(view, "T90MS-RearMudFlap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90MS-FrontMudFlap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90MS-FrontMudFlapSupport"),
                    Is.EqualTo(2));
                Transform flap = Find(
                    view,
                    "T90MS-FrontMudFlap");
                Assert.That(
                    flap.localPosition,
                    Is.EqualTo(new Vector3(-1.53f, 0.73f, 3.345f)));
                AssertBounds(
                    flap.GetComponent<MeshFilter>().sharedMesh.bounds,
                    new Vector3(-0.025f, -0.36f, -0.20f),
                    new Vector3(0.025f, 0.378f, 0.20f));
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
