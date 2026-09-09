using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class FleetVisualTests
    {
        [Test]
        public void EveryProductionVehicleBuildsNativeUnityVisual()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            string[] ids = catalog.ProductionVehicleIds;
            int armorSurfaceCount = 0;
            int trackTriangleCount = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                VehicleDefinition definition = catalog.GetVehicle(ids[i]);
                TankState tank = new TankState(
                    "visual-" + ids[i], Team.Alpha, definition.ToTankSpec(), Float3.Zero, 0f);
                TankView view = TankView.Create(
                    tank,
                    definition,
                    "factory",
                    "forest",
                    catalog);
                try
                {
                    Transform turret =
                        view.Root.Find("TurretRoot");
                    Assert.That(turret, Is.Not.Null, ids[i]);
                    Assert.That(view.Root.GetComponentsInChildren<Renderer>().Length,
                        Is.GreaterThan(30), ids[i]);
                    Transform[] transforms = view.Root.GetComponentsInChildren<Transform>();
                    int roadWheels = 0;
                    int suspensionArms = 0;
                    int suspensionJoints = 0;
                    int returnRollers = 0;
                    int sprockets = 0;
                    int idlers = 0;
                    int trackRuns = 0;
                    int authoredArmor = 0;
                    int authoredModules = 0;
                    for (int child = 0; child < transforms.Length; child++)
                    {
                        if (transforms[child].name.StartsWith("Armor-"))
                        {
                            armorSurfaceCount++;
                            authoredArmor++;
                        }
                        if (transforms[child].name.StartsWith("Module-"))
                            authoredModules++;
                        if (transforms[child].name.StartsWith("RoadWheel-")) roadWheels++;
                        if (transforms[child].name.StartsWith("SuspensionArm-")) suspensionArms++;
                        if (transforms[child].name.StartsWith("SuspensionJoint-")) suspensionJoints++;
                        if (transforms[child].name.StartsWith("ReturnRoller-")) returnRollers++;
                        if (transforms[child].name.StartsWith("Sprocket-") &&
                            !transforms[child].name.EndsWith("-Hub")) sprockets++;
                        if (transforms[child].name.StartsWith("Idler-") &&
                            !transforms[child].name.EndsWith("-Hub")) idlers++;
                        if (transforms[child].name.StartsWith("TrackLinks-"))
                        {
                            trackRuns++;
                            MeshFilter filter = transforms[child].GetComponent<MeshFilter>();
                            trackTriangleCount += filter.sharedMesh.triangles.Length / 3;
                        }
                    }
                    Assert.That(roadWheels, Is.GreaterThanOrEqualTo(8), ids[i]);
                    Assert.That(suspensionArms, Is.EqualTo(roadWheels), ids[i]);
                    Assert.That(suspensionJoints, Is.EqualTo(roadWheels), ids[i]);
                    Assert.That(returnRollers, Is.GreaterThanOrEqualTo(4), ids[i]);
                    Assert.That(sprockets, Is.EqualTo(2), ids[i]);
                    Assert.That(idlers, Is.EqualTo(2), ids[i]);
                    Assert.That(trackRuns, Is.EqualTo(2), ids[i]);
                    Assert.That(
                        authoredArmor,
                        Is.EqualTo(
                            FleetVisualAssertions.ValidPlateCount(
                                definition)),
                        ids[i]);
                    Assert.That(
                        authoredModules,
                        Is.EqualTo(
                            FleetVisualAssertions.ValidModulePartCount(
                                definition)),
                        ids[i]);
                    Assert.That(
                        authoredModules,
                        Is.GreaterThan(0),
                        ids[i] +
                        " must carry authored visible module geometry.");
                    Assert.That(
                        definition.armor.gunBarrel,
                        Is.Not.Null,
                        ids[i]);
                    Assert.That(
                        definition.armor.gunBarrel.lengthM,
                        Is.GreaterThan(0.1f),
                        ids[i]);
                    Assert.That(
                        definition.armor.gunBarrel.radiusM,
                        Is.GreaterThan(0.01f),
                        ids[i]);
                    FleetVisualAssertions.AssertAuthoredRig(
                        definition,
                        view.Root,
                        turret);
                    FleetVisualAssertions.AssertRenderableMeshes(
                        ids[i],
                        view.Root);
                    Assert.That(view.Root.GetComponentsInChildren<Collider>(), Is.Empty, ids[i]);
                }
                finally
                {
                    view.Destroy();
                }
            }

            Assert.That(ids, Has.Length.EqualTo(126));
            Assert.That(armorSurfaceCount, Is.GreaterThan(1000));
            Assert.That(trackTriangleCount, Is.GreaterThan(150000));
        }

        [Test]
        public void AbramsProductionFamilyHasDistinctAuthoredEquipment()
        {
            string[] ids =
            {
                "m1a1",
                "m1a1ha",
                "m1a2",
                "m1a2_tusk",
                "m1a2_sepv2",
                "m1a2_sepv3",
                "m1a3",
                "abramsx"
            };
            ContentCatalog catalog = ContentCatalog.Load();
            for (int i = 0; i < ids.Length; i++)
            {
                VehicleDefinition definition =
                    catalog.GetVehicle(ids[i]);
                TankView view = TankView.Create(
                    new TankState(
                        "abrams-" + ids[i],
                        Team.Alpha,
                        definition.ToTankSpec(),
                        Float3.Zero,
                        0f),
                    definition,
                    "factory",
                    "forest",
                    catalog);
                try
                {
                    Transform[] parts =
                        view.Root.GetComponentsInChildren<Transform>();
                    bool m1a2Family =
                        ids[i] == "m1a2" ||
                        ids[i] == "m1a2_tusk" ||
                        ids[i] == "m1a2_sepv2" ||
                        ids[i] == "m1a2_sepv3";
                    string familyToken =
                        ids[i] == "abramsx"
                            ? "AbramsX-"
                            : ids[i] == "m1a3"
                                ? "M1A3-"
                                : m1a2Family
                                    ? "M1A2-"
                                : "Abrams-";
                    Assert.That(
                        parts.Count(item =>
                            item.name.Contains(familyToken)),
                        Is.GreaterThanOrEqualTo(20),
                        ids[i]);
                    Assert.That(
                        parts.Count(item =>
                            item.name.StartsWith("Module-optics-")),
                        Is.EqualTo(
                            FleetVisualAssertions.ValidModulePartCount(
                                definition,
                                "optics")),
                        ids[i]);
                    if (m1a2Family)
                    {
                        string stationName =
                            ids[i] == "m1a2"
                                ? "Painted-M1A2-CrowsStandardHead"
                                : ids[i] == "m1a2_tusk"
                                    ? "Painted-M1A2-CrowsCompactHead"
                                    : ids[i] == "m1a2_sepv2"
                                        ? "Painted-M1A2-CrowsArmoredHead"
                                        : "Painted-M1A2-CrowsLowProfileHead";
                        Transform station = parts.FirstOrDefault(
                            item => item.name == stationName);
                        Assert.That(
                            station,
                            Is.Not.Null,
                            ids[i]);
                        Assert.That(
                            station.parent.name,
                            Is.EqualTo("TurretRoot"),
                            ids[i]);
                    }
                    Assert.That(
                        parts.Any(item =>
                            item.name ==
                                (ids[i] == "abramsx"
                                    ? "AbramsX-HybridLouvre"
                                    : ids[i] == "m1a3"
                                        ? "M1A3-HybridLouvre"
                                        : m1a2Family
                                            ? "AbramsM1-EngineDeckGrille"
                                        : "Abrams-EngineGrille")),
                        Is.True,
                        ids[i]);
                }
                finally
                {
                    view.Destroy();
                }
            }
        }

        [Test]
        public void TankViewCleanupToleratesDestroyedParentHierarchy()
        {
            VehicleDefinition definition = ContentCatalog.Load().GetVehicle("m1a2");
            TankView view = TankView.Create(
                new TankState(
                    "cleanup",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition);
            Object.DestroyImmediate(view.Root.gameObject);

            Assert.DoesNotThrow(view.Destroy);
        }

        [Test]
        public void CamouflageTexturesAreDeterministicAndPatterned()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90m");
            Texture2D first = TankCamouflage.CreateTexture(
                catalog,
                definition,
                "digital",
                "forest",
                Team.Alpha,
                64);
            Texture2D second = TankCamouflage.CreateTexture(
                catalog,
                definition,
                "digital",
                "forest",
                Team.Alpha,
                64);
            Texture2D winter = TankCamouflage.CreateTexture(
                catalog,
                definition,
                "winter",
                "frosthollow",
                Team.Alpha,
                64);
            try
            {
                Color32[] firstPixels = first.GetPixels32();
                Assert.That(
                    firstPixels,
                    Is.EqualTo(second.GetPixels32()));
                Assert.That(
                    firstPixels.Distinct().Count(),
                    Is.GreaterThan(4));
                Assert.That(
                    firstPixels,
                    Is.Not.EqualTo(winter.GetPixels32()));
                Assert.That(
                    first.name,
                    Does.Contain("digital"));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(winter);
            }
        }

        [Test]
        public void EveryCanonicalCamouflageBuildsPatternedTexture()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90m");
            for (int i = 0; i < catalog.Camouflage.Length; i++)
            {
                CamouflageDefinition camouflage =
                    catalog.Camouflage[i];
                Texture2D texture =
                    TankCamouflage.CreateTexture(
                        catalog,
                        definition,
                        camouflage.id,
                        "forest",
                        Team.Alpha,
                        32);
                try
                {
                    Assert.That(
                        texture.GetPixels32()
                            .Distinct()
                            .Count(),
                        Is.GreaterThan(3),
                        camouflage.id);
                }
                finally
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }

        [Test]
        public void TankViewAppliesPatternOnlyToPaintedSurfaces()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("m1a2");
            TankView view = TankView.Create(
                new TankState(
                    "patterned",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "summer",
                "forest",
                catalog);
            try
            {
                Renderer hull =
                    view.Root.Find("Hull").GetComponent<Renderer>();
                Renderer gun = view.Root
                    .Find("TurretRoot/Gun")
                    .GetComponent<Renderer>();
                Transform armor = view.Root
                    .GetComponentsInChildren<Transform>()
                    .First(item =>
                        item.name.StartsWith("Armor-"));
                Renderer bustle = view.Root
                    .GetComponentsInChildren<Renderer>()
                    .First(item =>
                        item.name ==
                            "Painted-AbramsM1-DeepBustle");
                Renderer optics = view.Root
                    .GetComponentsInChildren<Renderer>()
                    .First(item =>
                        item.name.StartsWith("Module-optics-"));
                Mesh armorMesh =
                    armor.GetComponent<MeshFilter>().sharedMesh;

                Assert.That(
                    hull.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    gun.sharedMaterial.mainTexture,
                    Is.Null);
                Assert.That(
                    armor.GetComponent<Renderer>()
                        .sharedMaterial.mainTexture,
                    Is.SameAs(hull.sharedMaterial.mainTexture));
                Assert.That(
                    bustle.sharedMaterial.mainTexture,
                    Is.SameAs(hull.sharedMaterial.mainTexture));
                Assert.That(
                    optics.sharedMaterial.mainTexture,
                    Is.Null);
                Assert.That(
                    armorMesh.uv,
                    Has.Length.EqualTo(
                        armorMesh.vertexCount));
                Assert.That(
                    armorMesh.uv.Distinct().Count(),
                    Is.GreaterThan(1));
            }
            finally
            {
                view.Destroy();
            }
        }

    }
}
