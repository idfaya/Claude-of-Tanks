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
                    Is.False);
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

        [Test]
        public void BuildsBowDeckReliktAndSegmentedSkirts()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90SM-BowProng"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-FenderLip"),
                    Is.EqualTo(20));
                Assert.That(
                    Count(view, "Painted-T90SM-LowBowFenderLip"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-DriverHatch"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-DriverPeriscope"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-EngineGrille"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90SM-TowEye"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-Headlight"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-GlacisRelikt"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90SM-GlacisReliktSeam"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T90SM-SkirtPanel"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90SM-SkirtBatten"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90SM-SkirtBottomLip"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90SM-BowSkirtCap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-FrontMudFlap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-OuterFenderHorn"),
                    Is.EqualTo(2));
                Renderer[] hullArmor = view.Root
                    .GetComponentsInChildren<Renderer>(true)
                    .Where(item =>
                        item.name.StartsWith("Painted-T90SM-Bow") ||
                        item.name.StartsWith("Painted-T90SM-Outer"))
                    .ToArray();
                Assert.That(
                    hullArmor.Max(item => item.bounds.max.z),
                    Is.EqualTo(3.465f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsTailRacksScallopsCageAndServiceField()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90SM-InnerTailRack"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-OuterTailRack"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-CornerBin"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-UnditchingLog"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-SpareTrackLink"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-T90SM-ScallopedSkirt"),
                    Is.EqualTo(20));
                Assert.That(
                    Count(view, "T90SM-ScallopedSkirtBatten"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "Painted-T90SM-CageRail"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "Painted-T90SM-CageStile"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "Painted-T90SM-CageStub"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(view, "Painted-T90SM-CagePost"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T90SM-CageAnchor"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-RearServiceLouvre"),
                    Is.EqualTo(13));
                Assert.That(
                    Count(view, "T90SM-RearTowEye"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-RearMudFlap"),
                    Is.EqualTo(2));
                Renderer toe = Find(
                        view,
                        "Painted-T90SM-OuterRackToe")
                    .GetComponent<Renderer>();
                Assert.That(
                    toe.bounds.min.z,
                    Is.EqualTo(-3.435f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsSourceWeldedTurretFoundationRingAndCupolas()
        {
            TankView view = Create();
            try
            {
                Transform root =
                    Find(view, "T90SM-PresentationRoot");
                Assert.That(
                    root.localPosition.x,
                    Is.EqualTo(0f).Within(0.000001f));
                Assert.That(
                    root.localPosition.y,
                    Is.EqualTo(0.09372385f).Within(0.000001f));
                Assert.That(
                    root.localPosition.z,
                    Is.EqualTo(-0.06f).Within(0.000001f));

                Mesh foundation = Find(
                        view,
                        "Painted-T90SM-WeldedFoundation")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    foundation.bounds,
                    new Vector3(
                        -1.581f,
                        0f,
                        -0.8317378f),
                    new Vector3(
                        1.581f,
                        0.515f,
                        1.4530622f));
                Assert.That(
                    foundation.vertexCount,
                    Is.EqualTo(198));
                Assert.That(
                    Count(view, "Painted-T90SM-RearCastingShelf"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-CrownPlate"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-TurretRing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-CommanderCupolaDrum"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-GunnerCupolaDrum"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-CommanderCupolaLid"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-GunnerCupolaLid"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsBroadCheeksNoseWedgesAndFlushSideCassettes()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90SM-BroadCheek"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-NoseWedgeOuter"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-NoseWedgeMiddle"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-NoseWedgeInner"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-ForwardCheekWedge"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-MainCheekCassette"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-FlankTransitionAft"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-FlankTransitionFront"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-RightCassetteCrown"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-LeftFlankBin"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-LeftRoofEdgeCassette"),
                    Is.EqualTo(1));

                Mesh cheek = Find(
                        view,
                        "Painted-T90SM-BroadCheek")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    cheek.bounds,
                    new Vector3(-1.55f, 0.08f, -0.30f),
                    new Vector3(-0.2325f, 0.4635f, 1.40f));
                Mesh cassette = Find(
                        view,
                        "Painted-T90SM-MainCheekCassette")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    cassette.bounds,
                    new Vector3(-1.735f, 0.08f, 0.10f),
                    new Vector3(-1.10f, 0.50f, 1.08f));
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
