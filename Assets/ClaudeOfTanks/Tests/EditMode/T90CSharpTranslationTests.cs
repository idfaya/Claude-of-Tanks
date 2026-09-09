using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public class T90CSharpTranslationTests
    {
        [Test]
        public void AddsTranslatedMeshHullAndTurretSurfaces()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90-CSharpUpperHullWedge"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-CSharpSweptGlacisMesh"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-CSharpRearDeckStep"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-CSharpTurretCheekWedge-L"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-CSharpTurretCheekWedge-R"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-CastDome"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Painted-T90-CSharpCastDomeMesh"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-TurretRingCollar"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Painted-T90-CastSeat"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-CastSeatSeam"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-BustleCargoBox"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-OpvtBaseCollar"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-OpvtRackStay"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-ShtoraDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-ShtoraRim"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-ShtoraVentFin"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90-CommanderCupolaRim"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90-GunnerHatch"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-SmokeLauncherCap"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90-AntennaBase"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90-RadioWhip"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "Painted-T90-RearFuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-UnditchingLogEnd"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90-BustleCableCoil"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-RearTowEye"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-RecoveryEye"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-CannonBaseBoot"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-GunMountCheekPlate"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Painted-T90-GunBootSection"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90-GunBootCrease"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-GunBootClamp"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-2A46MSleeveRing"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90-MuzzleBoreDisc"),
                    Is.EqualTo(1));
                Assert.That(
                    Find(view, "T90-MuzzleBore").localPosition.z,
                    Is.EqualTo(5.146f).Within(0.0001f));
                Vector3 gunOrigin = view.Root.Find("TurretRoot")
                    .InverseTransformPoint(
                        Find(view, "T90-GunFittings").position);
                AssertVector(
                    gunOrigin,
                    new Vector3(0f, 0.36f, 0.88f));
                Assert.That(
                    Count(view, "T90-TrackPad"),
                    Is.EqualTo(156));
                Assert.That(
                    Count(view, "T90-GearRoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelShoulder"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelDisc"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelInset"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelHub"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelHubCap"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-Sprocket"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-Idler"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-ReturnRoller"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90-ReturnRollerDisc"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90-GearSuspensionLink"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearSuspensionJointBoss"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(view, "T90-K5RoofVerticalSeam"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-K5RoofEdgeSeam"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90-K5InnerLeafCap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90-K5OuterLeafCap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-K5LeafOuterEdgeSeam"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90-K5LeafTopSeam"),
                    Is.EqualTo(4));
                AssertMeshVertexCount(
                    view,
                    "Painted-T90-CSharpCastDomeMesh",
                    468);
                AssertMeshVertexCount(
                    view,
                    "Painted-T90-CastSeat",
                    108);
                Bounds castBounds = Find(
                        view,
                        "Painted-T90-CSharpCastDomeMesh")
                    .GetComponent<MeshFilter>()
                    .sharedMesh.bounds;
                AssertVector(
                    castBounds.min,
                    new Vector3(-1.62f, -0.06f, -1.46f));
                AssertVector(
                    castBounds.max,
                    new Vector3(1.64f, 0.71f, 1.38f));
                AssertMeshVertexCount(
                    view,
                    "Painted-T90-CSharpUpperHullWedge",
                    36);
                AssertMeshVertexCount(
                    view,
                    "Painted-T90-CSharpTurretCheekWedge-L",
                    36);
                AssertMeshVertexCount(
                    view,
                    "Painted-T90-LowerTub",
                    900);
                Assert.That(
                    Find(view, "Painted-T90-LowerTub").localScale,
                    Is.EqualTo(Vector3.one));
                Mesh wheelMesh = Find(
                        view,
                        "T90-GearRoadWheelTire")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                Assert.That(
                    wheelMesh.bounds.size.x,
                    Is.EqualTo(0.22f).Within(0.0001f));
                Assert.That(
                    MaximumRadiusX(wheelMesh.vertices),
                    Is.EqualTo(0.385f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void DestroysTranslatedRuntimeMeshes()
        {
            TankView view = Create();
            Mesh mesh = Find(
                    view,
                    "Painted-T90-CSharpCastDomeMesh")
                .GetComponent<MeshFilter>()
                .sharedMesh;

            view.Destroy();

            Assert.That(mesh == null, Is.True);
        }

        private static TankView Create()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90");
            return TankView.Create(
                new TankState(
                    "t90-csharp-translation-test",
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
            Transform[] transforms =
                view.Root.GetComponentsInChildren<Transform>(true);
            int count = 0;
            for (int index = 0;
                index < transforms.Length;
                index++)
            {
                if (transforms[index].name == name)
                    count++;
            }
            return count;
        }

        private static void AssertMeshVertexCount(
            TankView view,
            string name,
            int expected)
        {
            Transform transform = Find(view, name);
            Assert.That(transform, Is.Not.Null, name);
            MeshFilter filter =
                transform.GetComponent<MeshFilter>();
            Assert.That(filter, Is.Not.Null, name);
            Assert.That(filter.sharedMesh.vertexCount, Is.EqualTo(expected));
            Assert.That(
                filter.sharedMesh.normals.Length,
                Is.EqualTo(expected));
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            Transform[] transforms =
                view.Root.GetComponentsInChildren<Transform>(true);
            for (int index = 0;
                index < transforms.Length;
                index++)
            {
                if (transforms[index].name == name)
                    return transforms[index];
            }
            return null;
        }

        private static void AssertVector(
            Vector3 actual,
            Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
        }

        private static float MaximumRadiusX(Vector3[] vertices)
        {
            float radius = 0f;
            for (int index = 0; index < vertices.Length; index++)
            {
                radius = Mathf.Max(
                    radius,
                    Mathf.Sqrt(
                        vertices[index].y * vertices[index].y +
                        vertices[index].z * vertices[index].z));
            }
            return radius;
        }
    }
}
