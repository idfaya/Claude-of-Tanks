using System;
using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndNativeSixWheelCourse()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90");
            TankView view =
                Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(166));
                Assert.That(
                    Count(view, "T90-BakedPresentationPrefab"),
                    Is.EqualTo(1));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(definition.armor.turretPlates)
                        .Count(plate => plate.kind == "era"),
                    Is.EqualTo(141));
                Assert.That(
                    FindAll(view, "RoadWheel-L").Length,
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(view, "RoadWheel-R").Length,
                    Is.EqualTo(6));
                Assert.That(
                    CountPrefix(view, "TS-T90-gearReturnRoller"),
                    Is.GreaterThanOrEqualTo(2));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.395f,
                        0.9f,
                        -2.52f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.395f,
                        0.71f,
                        2.7f)));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellAndKeepsKontakt5Visible()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    VisibleArmor(view),
                    Is.EqualTo(141));
                Assert.That(
                    Count(view, "Painted-Soviet-FuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Soviet-ShtoraLens"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "TS-T90-"),
                    Is.GreaterThanOrEqualTo(72));
                Assert.That(
                    view.Root.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void PreservesTsBakeMeshAndMaterialData()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    CountPrefix(view, "TS-T90-vehicleMarking_"),
                    Is.EqualTo(2));

                Mesh bakedMesh =
                    Find(view, "TS-T90-hull")
                        .GetComponent<MeshFilter>()
                        .sharedMesh;
                Assert.That(
                    bakedMesh.normals.Length,
                    Is.EqualTo(bakedMesh.vertexCount));
                Assert.That(
                    bakedMesh.uv.Length,
                    Is.EqualTo(bakedMesh.vertexCount));
                Assert.That(
                    bakedMesh.colors.Length,
                    Is.EqualTo(bakedMesh.vertexCount));

                Renderer renderer =
                    Find(view, "TS-T90-hull")
                        .GetComponent<Renderer>();
                Assert.That(
                    renderer.sharedMaterial.shader.name,
                    Is.EqualTo("ClaudeOfTanks/TankBakedPresentation"));
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_UseCamo"),
                    Is.EqualTo(1f));
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_HasMainTex"),
                    Is.EqualTo(1f));
                Assert.That(
                    renderer.sharedMaterial.GetTexture("_MainTex"),
                    Is.Not.Null);
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_HasNormalMap"),
                    Is.EqualTo(1f));
                Assert.That(
                    renderer.sharedMaterial.GetTexture("_NormalMap"),
                    Is.Not.Null);
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_HasRoughnessMap"),
                    Is.EqualTo(1f));
                Assert.That(
                    renderer.sharedMaterial.GetTexture("_RoughnessMap"),
                    Is.Not.Null);
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_NormalScale"),
                    Is.GreaterThan(0f));
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_EmissionIntensity"),
                    Is.EqualTo(1f));
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_SpecularIntensity"),
                    Is.EqualTo(0.55f)
                        .Within(0.001f));
                Assert.That(
                    view.Root.GetComponentsInChildren<Renderer>(true)
                        .Any((candidate) =>
                            candidate.sharedMaterial != null &&
                            candidate.sharedMaterial.HasProperty("_HasBumpMap") &&
                            candidate.sharedMaterial.GetFloat("_HasBumpMap") > 0.5f &&
                            candidate.sharedMaterial.GetTexture("_BumpMap") != null),
                    Is.True);
                Assert.That(
                    renderer.sharedMaterial.GetFloat("_ZWrite"),
                    Is.EqualTo(1f));
                Assert.That(
                    Find(view, "TS-T90-turretGlass")
                        .GetComponent<Renderer>()
                        .sharedMaterial
                        .color
                        .a,
                    Is.EqualTo(1f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsHullKontakt5RearAndCageIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "TS-T90-hull"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullExternalArmor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullRubber"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-gearTrackBandL"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-gearTrackBandR"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullWood"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-hullEquipment"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsCastTurretShtoraAndRoofStation()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "TS-T90-turret"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretExternalArmor"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretDetail"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretEquipment"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "TS-T90-turretGlass"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "TS-T90-fitting_smokeBank"),
                    Is.GreaterThanOrEqualTo(4));
                Assert.That(
                    Count(view, "TS-T90-browningDerivedMachineGunBody"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "TS-T90-fitting_antennaWhip"),
                    Is.GreaterThanOrEqualTo(4));
                Assert.That(
                    Count(view, "TS-T90-turretDark"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsTwoA46MOnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "TS-T90-gun",
                        "TS-T90-gunDark",
                        "TS-T90-gunMount",
                        "TS-T90-muzzleBoreShadowRim",
                        "TS-T90-muzzleBoreShadowDisc"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(view, "T90-BakedGunFittings"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsCSharpTranslatedFallbackWhenBakeIsDisabled()
        {
            string previous =
                Environment.GetEnvironmentVariable(
                    "COT_DISABLE_T90_BAKED_PRESENTATION");
            Environment.SetEnvironmentVariable(
                "COT_DISABLE_T90_BAKED_PRESENTATION",
                "1");
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "T90-BakedPresentationPrefab"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "T90-PresentationSchema"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "TS-T90-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "Painted-T90-"),
                    Is.GreaterThanOrEqualTo(60));
                Assert.That(
                    Count(view, "T90-RoadWheelInset"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T90-ReturnRoller"),
                    Is.EqualTo(6));
                AssertHidden(view, "RoadWheel-L");
                AssertHidden(view, "RoadWheel-R");
                AssertHidden(view, "Sprocket-L");
                AssertHidden(view, "Idler-L");
                AssertGunOwned(view, "Painted-T90-2A46MForwardTube");
            }
            finally
            {
                view.Destroy();
                Environment.SetEnvironmentVariable(
                    "COT_DISABLE_T90_BAKED_PRESENTATION",
                    previous);
            }
        }

        private static TankView Create(
            ContentCatalog catalog)
        {
            VehicleDefinition definition =
                catalog.GetVehicle("t90");
            return TankView.Create(
                new TankState(
                    "t90-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static void AssertHidden(
            TankView view,
            string name)
        {
            Assert.That(
                Find(view, name)
                    .GetComponent<Renderer>()
                    .enabled,
                Is.False);
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
        }

        private static int VisibleArmor(
            TankView view)
        {
            return FindAllByPrefix(view, "Armor-")
                .Count(item =>
                    item.GetComponent<Renderer>().enabled);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return FindAll(view, name).Length;
        }

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return FindAllByPrefix(view, prefix).Length;
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name == name)
                .ToArray();
        }

        private static Transform[] FindAllByPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name.StartsWith(prefix))
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return FindAll(view, name).First();
        }
    }
}
