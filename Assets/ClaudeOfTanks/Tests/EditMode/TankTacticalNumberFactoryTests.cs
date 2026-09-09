using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankTacticalNumberFactoryTests
    {
        [Test]
        public void T90MSBuildsMirroredRuntimeNumberAndReleasesTexture()
        {
            TankView view = Create();
            Texture2D texture = null;
            try
            {
                Transform root = Find(
                    view,
                    "T90MS-TacticalNumbers");
                Transform right = Find(
                    view,
                    "VehicleMarking-T90MS-Right");
                Transform left = Find(
                    view,
                    "VehicleMarking-T90MS-Left");

                Assert.That(root.parent.name,
                    Is.EqualTo("T90MS-PresentationRoot"));
                AssertVector(
                    right.localPosition,
                    new Vector3(1.67f, 0.30f, -0.38f));
                AssertVector(
                    left.localPosition,
                    new Vector3(-1.67f, 0.30f, -0.38f));
                AssertVector(
                    right.localScale,
                    Vector3.one * 0.25f);
                Assert.That(
                    Mathf.DeltaAngle(
                        right.localEulerAngles.y,
                        90f),
                    Is.EqualTo(0f).Within(0.001f));
                Assert.That(
                    Mathf.DeltaAngle(
                        left.localEulerAngles.y,
                        -90f),
                    Is.EqualTo(0f).Within(0.001f));

                Renderer rightRenderer =
                    right.GetComponent<Renderer>();
                Renderer leftRenderer =
                    left.GetComponent<Renderer>();
                texture =
                    rightRenderer.sharedMaterial.mainTexture
                    as Texture2D;
                Assert.That(texture, Is.Not.Null);
                Assert.That(
                    leftRenderer.sharedMaterial.mainTexture,
                    Is.SameAs(texture));
                Assert.That(texture.width, Is.EqualTo(128));
                Assert.That(texture.height, Is.EqualTo(64));
                Assert.That(
                    texture.name,
                    Is.EqualTo(
                        "TankTacticalNumberTexture-T90MS-340"));
                Assert.That(
                    texture.GetPixels32().Count(pixel => pixel.a > 0),
                    Is.GreaterThan(1000));
                Assert.That(
                    right.GetComponent<MeshFilter>()
                        .sharedMesh.vertexCount,
                    Is.EqualTo(4));
            }
            finally
            {
                view.Destroy();
            }
            Assert.That(texture == null, Is.True);
        }

        private static TankView Create()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90ms");
            return TankView.Create(
                new TankState(
                    "t90ms-marking-test",
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
