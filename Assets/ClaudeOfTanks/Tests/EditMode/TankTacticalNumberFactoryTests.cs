using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankTacticalNumberFactoryTests
    {
        [TestCase(
            "t90", "T90", "TurretRoot",
            -1.0333622f, 0.3992434f, -1.3922033f,
            -0.0676667f, -0.7611755f, -0.0571143f, 0.6424726f,
            -1.4004122f, 0.3985853f, 0.3175893f,
            -0.0211688f, -0.6511015f, -0.0246539f, 0.7582947f)]
        [TestCase(
            "t90a", "T90A", "T90A-PresentationRoot",
            1.6185539f, 0.10675f, -1.5836288f,
            0f, 0.7176335f, 0f, 0.6964209f,
            1.2155189f, 0.4222885f, 0.0083077f,
            -0.2304606f, 0.7025491f, 0.2098583f, 0.6397438f)]
        [TestCase(
            "t90a_vladimir", "T90AVladimir",
            "T90AVladimir-PresentationRoot",
            -1.3826577f, 0.2355f, -0.691368f,
            0f, -0.6782645f, 0f, 0.7348178f,
            -1.1937633f, 0.4791295f, -0.8909543f,
            -0.1116375f, -0.714071f, -0.1067518f, 0.6828204f)]
        [TestCase(
            "t90a_burlak", "T90ABurlak",
            "T90ABurlak-PresentationRoot",
            0.9859912f, 0.4226153f, -1.5825224f,
            -0.0709102f, 0.7976612f, 0.0530336f, 0.59657f,
            1.0533363f, 0.252985f, -1.2382426f,
            0.1494883f, 0.7627854f, -0.1209944f, 0.6173913f)]
        public void CompletedT90FamilyBuildsGeneratedPair(
            string id,
            string prefix,
            string parentName,
            float insigniaX,
            float insigniaY,
            float insigniaZ,
            float insigniaQx,
            float insigniaQy,
            float insigniaQz,
            float insigniaQw,
            float designationX,
            float designationY,
            float designationZ,
            float designationQx,
            float designationQy,
            float designationQz,
            float designationQw)
        {
            TankView view = Create(id);
            try
            {
                Transform root = Find(
                    view,
                    prefix + "-TacticalMarkings");
                Assert.That(root.parent.name, Is.EqualTo(parentName));
                Assert.That(root.childCount, Is.EqualTo(2));
                AssertSeat(
                    Find(view,
                        "VehicleMarking-" +
                        prefix +
                        "-Insignia"),
                    0.23f,
                    new Vector3(
                        insigniaX,
                        insigniaY,
                        insigniaZ),
                    new Quaternion(
                        insigniaQx,
                        insigniaQy,
                        insigniaQz,
                        insigniaQw));
                AssertSeat(
                    Find(view,
                        "VehicleMarking-" +
                        prefix +
                        "-Designation"),
                    0.23f,
                    new Vector3(
                        designationX,
                        designationY,
                        designationZ),
                    new Quaternion(
                        designationQx,
                        designationQy,
                        designationQz,
                        designationQw));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void T90SMBuildsGeneratedThreeSeatSet()
        {
            TankView view = Create("t90sm");
            try
            {
                Transform root =
                    Find(view, "T90SM-TacticalMarkings");
                Assert.That(
                    root.parent.name,
                    Is.EqualTo("T90SM-PresentationRoot"));
                Assert.That(root.childCount, Is.EqualTo(3));
                AssertSeat(
                    Find(view,
                        "VehicleMarking-T90SM-Designation-Right"),
                    0.26f,
                    new Vector3(
                        1.4863963f,
                        0.3038142f,
                        -0.05f),
                    new Quaternion(
                        -0.2387832f,
                        0.6655694f,
                        0.2387832f,
                        0.6655694f));
                AssertSeat(
                    Find(view,
                        "VehicleMarking-T90SM-Designation-Left"),
                    0.26f,
                    new Vector3(
                        -1.4863963f,
                        0.3038142f,
                        -0.05f),
                    new Quaternion(
                        -0.2387832f,
                        -0.6655694f,
                        -0.2387832f,
                        0.6655694f));
                AssertSeat(
                    Find(view,
                        "VehicleMarking-T90SM-Insignia"),
                    0.24f,
                    new Vector3(
                        -1.141f,
                        0.3824f,
                        -1.33472f),
                    new Quaternion(
                        0f,
                        -0.7071068f,
                        0f,
                        0.7071068f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void T90MSBuildsSolvedRussianMarkingsAndReleasesTextures()
        {
            TankView view = Create("t90ms");
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

        private static TankView Create(string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-marking-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static void AssertSeat(
            Transform seat,
            float size,
            Vector3 position,
            Quaternion rotation)
        {
            AssertVector(seat.localPosition, position);
            AssertVector(seat.localScale, Vector3.one * size);
            AssertQuaternion(seat.localRotation, rotation);
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
