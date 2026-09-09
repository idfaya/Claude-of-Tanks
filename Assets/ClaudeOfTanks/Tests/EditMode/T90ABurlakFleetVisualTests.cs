using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90ABurlakFleetVisualTests
    {
        [Test]
        public void BuildsSourceCoreShouldersAndBustle()
        {
            TankView view = Create();
            try
            {
                Transform presentation =
                    Find(view, "T90ABurlak-PresentationRoot");
                AssertVector(
                    presentation.localPosition,
                    new Vector3(0f, 0.11f, 0f));
                AssertVector(
                    presentation.localScale,
                    new Vector3(1f, 0.85f, 1f));
                Assert.That(
                    VertexCount(view, "Painted-T90ABurlak-Core"),
                    Is.EqualTo(324));
                Assert.That(
                    VertexCount(view, "Painted-T90ABurlak-RingApron"),
                    Is.EqualTo(90));
                Assert.That(
                    Count(view, "Painted-T90ABurlak-ShoulderCarrier"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90ABurlak-ShoulderK5"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90ABurlak-ShoulderK5Seam"),
                    Is.EqualTo(8));
                Assert.That(
                    VertexCount(view, "Painted-T90ABurlak-BustleLoft"),
                    Is.EqualTo(168));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsFullSourceBustleEnvelopeAndServiceGrid()
        {
            TankView view = Create();
            try
            {
                Mesh bustle = Find(
                        view,
                        "Painted-T90ABurlak-BustleLoft")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                Assert.That(
                    bustle.bounds.min.z,
                    Is.EqualTo(-3.30f).Within(0.0001f));
                Assert.That(
                    bustle.bounds.max.z,
                    Is.EqualTo(-1.08f).Within(0.0001f));
                Assert.That(
                    bustle.bounds.min.x,
                    Is.EqualTo(-1.10f).Within(0.0001f));
                Assert.That(
                    bustle.bounds.max.x,
                    Is.EqualTo(1.10f).Within(0.0001f));
                Assert.That(
                    Count(view, "T90ABurlak-BustleLid"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "Painted-T90ABurlak-BustleSidePod"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90ABurlak-BustleRearRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90ABurlak-BustleRearPost"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90ABurlak-BustleServiceLatch"),
                    Is.EqualTo(9));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericTurretAndSovietFallback()
        {
            TankView view = Create();
            try
            {
                Renderer generic = view.Root
                    .Find("TurretRoot/Turret")
                    .GetComponent<Renderer>();
                Assert.That(generic.enabled, Is.False);
                Assert.That(
                    view.Root.GetComponentsInChildren<Transform>(true)
                        .Count(item =>
                            item.name.StartsWith("Soviet-") ||
                            item.name.StartsWith("Painted-Soviet-")),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsRoofEquipmentAndAsymmetricSmokeProgram()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90ABurlak-PanoramaHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90ABurlak-Hatch"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-PeriscopeBody"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90ABurlak-AutoloaderFeedLid"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90ABurlak-AutoloaderRubRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-AmmoCan"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-Shield"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-CradleFork"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-FeedLink"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-ShieldFoldedEdge"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-Nsvt-ShieldFastener"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(view, "T90ABurlak-Detail-SmokeLauncher"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(view, "T90ABurlak-SmokeLauncherCap"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(view, "T90ABurlak-SmokeBankBase"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-AntennaBasePot"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90ABurlak-AntennaCollar"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90ABurlak-Detail-RadioWhip"),
                    Is.EqualTo(1));
                Transform whip =
                    Find(view, "T90ABurlak-Detail-RadioWhip");
                Assert.That(
                    whip.GetComponent<MeshFilter>()
                        .sharedMesh.bounds.size.y,
                    Is.EqualTo(2.67f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsScaledTwoA46AssemblyOnSourcePivot()
        {
            TankView view = Create();
            try
            {
                Renderer generic = view.Root
                    .Find("TurretRoot/Gun")
                    .GetComponent<Renderer>();
                Assert.That(generic.enabled, Is.False);
                Transform assembly =
                    Find(view, "T90ABurlak-GunAssembly");
                AssertVector(
                    assembly.localPosition,
                    new Vector3(0f, 0.225f, 0.615f));
                AssertVector(
                    assembly.localScale,
                    new Vector3(1f, 1.318f, 0.955f));
                Assert.That(
                    Count(view, "Painted-T90ABurlak-BarrelCourse"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90ABurlak-BarrelRing"),
                    Is.EqualTo(5));
                Transform turret = view.Root.Find("TurretRoot");
                Transform muzzle = Find(view, "T90A-MuzzleBore");
                AssertVector(
                    turret.InverseTransformPoint(muzzle.position),
                    new Vector3(0f, 0.3068515f, 5.32888f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void AppliesT90AMaterialRolesToBurlak()
        {
            TankView view = Create();
            try
            {
                Renderer hull =
                    FindRenderer(view, "Painted-T90A-UpperHull");
                Renderer barrel =
                    FindRenderer(view, "Painted-T90ABurlak-BarrelCourse");
                Renderer track =
                    FindRenderer(view, "TrackLinks-L");
                Renderer glass =
                    FindRenderer(view, "T90ABurlak-PanoramaLens");
                Assert.That(hull.sharedMaterial.mainTexture, Is.Not.Null);
                Assert.That(barrel.sharedMaterial.mainTexture, Is.Not.Null);
                Assert.That(track.sharedMaterial.mainTexture, Is.Null);
                Assert.That(glass.sharedMaterial.mainTexture, Is.Null);
                Assert.That(
                    track.sharedMaterial.color.r,
                    Is.EqualTo(0x35 / 255f).Within(0.002f));
                Assert.That(
                    glass.sharedMaterial.color.b,
                    Is.EqualTo(0x40 / 255f).Within(0.002f));
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
                catalog.GetVehicle("t90a_burlak");
            return TankView.Create(
                new TankState(
                    "t90a-burlak-visual-test",
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

        private static int VertexCount(
            TankView view,
            string name)
        {
            return Find(view, name)
                .GetComponent<MeshFilter>()
                .sharedMesh.vertexCount;
        }

        private static Renderer FindRenderer(
            TankView view,
            string name)
        {
            return Find(view, name).GetComponent<Renderer>();
        }

        private static void AssertVector(
            Vector3 actual,
            Vector3 expected)
        {
            Assert.That(
                actual.x,
                Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(
                actual.y,
                Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(
                actual.z,
                Is.EqualTo(expected.z).Within(0.0001f));
        }
    }
}
