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
        public void T90MSBuildsSolvedRussianMarkingsAndReleasesTextures()
        {
            TankView view = Create();
            Texture2D designationTexture = null;
            Texture2D insigniaTexture = null;
            try
            {
                Transform root = Find(
                    view,
                    "T90MS-TacticalMarkings");
                Transform insignia = Find(
                    view,
                    "VehicleMarking-T90MS-Insignia");
                Transform designation = Find(
                    view,
                    "VehicleMarking-T90MS-Designation");

                Assert.That(root.parent.name,
                    Is.EqualTo("T90MS-PresentationRoot"));
                AssertVector(
                    insignia.localPosition,
                    new Vector3(
                        0.9610577f,
                        0.6983223f,
                        -0.8166105f));
                AssertVector(
                    designation.localPosition,
                    new Vector3(
                        1.0750685f,
                        0.5469447f,
                        -1.2616064f));
                AssertVector(
                    insignia.localScale,
                    Vector3.one * 0.24f);
                AssertVector(
                    designation.localScale,
                    Vector3.one * 0.24f);
                AssertQuaternion(
                    insignia.localRotation,
                    new Quaternion(
                        -0.3780458f,
                        0.6207669f,
                        0.3572425f,
                        0.586607f));
                AssertQuaternion(
                    designation.localRotation,
                    new Quaternion(
                        -0.1696276f,
                        0.7701519f,
                        0.1322612f,
                        0.6004993f));

                Renderer insigniaRenderer =
                    insignia.GetComponent<Renderer>();
                Renderer designationRenderer =
                    designation.GetComponent<Renderer>();
                insigniaTexture =
                    insigniaRenderer.sharedMaterial.mainTexture
                    as Texture2D;
                designationTexture =
                    designationRenderer.sharedMaterial.mainTexture
                    as Texture2D;
                Assert.That(insigniaTexture, Is.Not.Null);
                Assert.That(designationTexture, Is.Not.Null);
                Assert.That(
                    insigniaTexture,
                    Is.Not.SameAs(designationTexture));
                Assert.That(
                    designationTexture.width,
                    Is.EqualTo(128));
                Assert.That(
                    designationTexture.height,
                    Is.EqualTo(128));
                Assert.That(
                    designationTexture.name,
                    Is.EqualTo(
                        "TankTacticalNumberTexture-T90MS-340"));
                Assert.That(
                    insigniaTexture.name,
                    Is.EqualTo(
                        "TankTacticalInsigniaTexture-T90MS-RU"));
                Assert.That(
                    designationTexture.GetPixels32()
                        .Count(pixel => pixel.a > 0),
                    Is.GreaterThan(1000));
                Assert.That(
                    insigniaTexture.GetPixels32()
                        .Count(pixel => pixel.a > 0),
                    Is.GreaterThan(1000));
                Assert.That(
                    designationTexture.GetPixels32()
                        .Count(pixel =>
                            pixel.Equals(
                                new Color32(
                                    216,
                                    213,
                                    201,
                                    255))),
                    Is.GreaterThan(1000));
                Assert.That(
                    insigniaTexture.GetPixels32()
                        .Count(pixel =>
                            pixel.Equals(
                                new Color32(
                                    182,
                                    50,
                                    46,
                                    255))),
                    Is.GreaterThan(1000));
                Assert.That(
                    designation.GetComponent<MeshFilter>()
                        .sharedMesh.vertexCount,
                    Is.EqualTo(4));
                Vector2[] uv =
                    designation.GetComponent<MeshFilter>()
                        .sharedMesh.uv;
                Assert.That(uv[0], Is.EqualTo(new Vector2(1f, 0f)));
                Assert.That(uv[1], Is.EqualTo(new Vector2(0f, 0f)));
            }
            finally
            {
                view.Destroy();
            }
            Assert.That(designationTexture == null, Is.True);
            Assert.That(insigniaTexture == null, Is.True);
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

        private static void AssertQuaternion(
            Quaternion actual,
            Quaternion expected)
        {
            Assert.That(
                Mathf.Abs(Quaternion.Dot(actual, expected)),
                Is.EqualTo(1f).Within(0.0001f));
        }
    }
}
