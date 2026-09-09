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
                    Count(view, "Painted-T90-BustleCargoBox"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-OpvtBaseCollar"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-OpvtRackStay"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-CannonBaseBoot"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90-GunMountCheekPlate"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90-TrackPad"),
                    Is.EqualTo(156));
                Assert.That(
                    Count(view, "T90-GearRoadWheelTire"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelDisc"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-GearRoadWheelInset"),
                    Is.EqualTo(12));
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
                AssertMeshVertexCountAtLeast(
                    view,
                    "Painted-T90-CSharpCastDomeMesh",
                    1000);
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
        }

        private static void AssertMeshVertexCountAtLeast(
            TankView view,
            string name,
            int expected)
        {
            Transform transform = Find(view, name);
            Assert.That(transform, Is.Not.Null, name);
            MeshFilter filter =
                transform.GetComponent<MeshFilter>();
            Assert.That(filter, Is.Not.Null, name);
            Assert.That(
                filter.sharedMesh.vertexCount,
                Is.GreaterThanOrEqualTo(expected));
            Assert.That(
                filter.sharedMesh.normals.Length,
                Is.EqualTo(filter.sharedMesh.vertexCount));
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
    }
}
