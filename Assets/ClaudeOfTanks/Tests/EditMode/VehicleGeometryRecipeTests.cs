using System;
using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class VehicleGeometryRecipeTests
    {
        private const string ManifestResource =
            "Content/VehicleGeometry/manifest";
        private const float DimensionTolerance = 0.03f;
        private const float CenterToleranceM = 0.08f;

        [SetUp]
        public void EnableRecipes()
        {
            TankGeometryRecipeFactory
                .EnableEditModeRecipes = true;
        }

        [TearDown]
        public void DisableRecipes()
        {
            TankGeometryRecipeFactory
                .EnableEditModeRecipes = false;
        }

        [Test]
        public void EveryCatalogVehicleMatchesTypeScriptGeometryRecipe()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleGeometryManifest manifest =
                LoadManifest();
            Assert.That(
                manifest.vehicles,
                Has.Length.EqualTo(
                    catalog.Vehicles.Length));

            for (int index = 0;
                index < manifest.vehicles.Length;
                index++)
            {
                VehicleGeometryReceipt receipt =
                    manifest.vehicles[index];
                VehicleDefinition definition =
                    catalog.GetVehicle(receipt.id);
                TankView view = TankView.Create(
                    new TankState(
                        "recipe-" + receipt.id,
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
                    Transform recipe =
                        view.Root.Find(
                            "AuthoritativeVisualRecipe");
                    Assert.That(
                        recipe,
                        Is.Not.Null,
                        receipt.id);
                    Renderer[] renderers =
                        recipe.GetComponentsInChildren<
                            Renderer>(true);
                    Assert.That(
                        renderers,
                        Is.Not.Empty,
                        receipt.id);
                    Assert.That(
                        renderers.All(item =>
                            !item.forceRenderingOff),
                        Is.True,
                        receipt.id);
                    Bounds actual = renderers[0].bounds;
                    for (int renderer = 1;
                        renderer < renderers.Length;
                        renderer++)
                    {
                        actual.Encapsulate(
                            renderers[renderer].bounds);
                    }
                    Vector3 expectedMinimum =
                        ToVector3(
                            receipt.bounds.min);
                    Vector3 expectedMaximum =
                        ToVector3(
                            receipt.bounds.max);
                    Bounds expected = new Bounds(
                        (expectedMinimum +
                         expectedMaximum) * 0.5f,
                        expectedMaximum -
                        expectedMinimum);
                    AssertRelative(
                        receipt.id + "/width",
                        actual.size.x,
                        expected.size.x);
                    AssertRelative(
                        receipt.id + "/height",
                        actual.size.y,
                        expected.size.y);
                    AssertRelative(
                        receipt.id + "/length",
                        actual.size.z,
                        expected.size.z);
                    Assert.That(
                        Vector3.Distance(
                            actual.center,
                            expected.center),
                        Is.LessThanOrEqualTo(
                            CenterToleranceM),
                        receipt.id);

                    MeshFilter[] filters =
                        recipe.GetComponentsInChildren<
                            MeshFilter>(true);
                    int vertices = filters.Sum(item =>
                        item.sharedMesh != null
                            ? item.sharedMesh.vertexCount
                            : 0);
                    Assert.That(
                        vertices,
                        Is.EqualTo(
                            receipt.vertexCount),
                        receipt.id);
                    Assert.That(
                        recipe.GetComponentsInChildren<
                                Transform>(true)
                            .Any(item =>
                                item.name.StartsWith(
                                    "Armor-") ||
                                item.name.StartsWith(
                                    "Module-")),
                        Is.False,
                        receipt.id);
                }
                finally
                {
                    view.Destroy();
                }
            }
        }

        [Test]
        public void RecipeUsesIndependentArticulationRigs()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("m1a1ha");
            TankState tank = new TankState(
                "recipe-articulation",
                Team.Alpha,
                definition.ToTankSpec(),
                Float3.Zero,
                0f);
            TankView view = TankView.Create(
                tank,
                definition,
                "factory",
                "forest",
                catalog);
            try
            {
                Transform recipe =
                    view.Root.Find(
                        "AuthoritativeVisualRecipe");
                Transform turret =
                    recipe.Find(
                        "RecipeTurretRoot");
                Transform gun =
                    turret.Find(
                        "RecipeGunAssembly");
                tank.TurretYaw = 0.4f;
                tank.GunPitchRad = 0.2f;
                view.Sync(tank);
                Assert.That(
                    turret.localEulerAngles.y,
                    Is.EqualTo(
                        0.4f * Mathf.Rad2Deg)
                        .Within(0.01f));
                Assert.That(
                    Mathf.DeltaAngle(
                        gun.localEulerAngles.x,
                        -0.2f * Mathf.Rad2Deg),
                    Is.EqualTo(0f)
                        .Within(0.01f));
                Renderer painted =
                    recipe.GetComponentsInChildren<
                            Renderer>(true)
                        .First(item =>
                            item.name.StartsWith(
                                "Painted-Recipe-"));
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    painted.sharedMaterial
                        .mainTextureScale,
                    Is.EqualTo(Vector2.one));
                Color alive =
                    painted.sharedMaterial.color;
                tank.Destroyed = true;
                view.Sync(tank);
                Assert.That(
                    painted.sharedMaterial.color
                        .maxColorComponent,
                    Is.LessThan(
                        alive.maxColorComponent));
                tank.Destroyed = false;
                view.Sync(tank);
                Assert.That(
                    painted.sharedMaterial.color,
                    Is.EqualTo(alive));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static VehicleGeometryManifest
            LoadManifest()
        {
            TextAsset asset =
                Resources.Load<TextAsset>(
                    ManifestResource);
            Assert.That(asset, Is.Not.Null);
            VehicleGeometryManifest manifest =
                JsonUtility.FromJson<
                    VehicleGeometryManifest>(
                    asset.text);
            Assert.That(manifest, Is.Not.Null);
            Assert.That(
                manifest.schemaVersion,
                Is.EqualTo(1));
            return manifest;
        }

        private static void AssertRelative(
            string label,
            float actual,
            float expected)
        {
            float error =
                Mathf.Abs(actual - expected) /
                Mathf.Max(
                    Mathf.Abs(expected),
                    0.0001f);
            Assert.That(
                error,
                Is.LessThanOrEqualTo(
                    DimensionTolerance),
                label);
        }

        private static Vector3 ToVector3(
            float[] values)
        {
            Assert.That(
                values,
                Has.Length.EqualTo(3));
            return new Vector3(
                values[0],
                values[1],
                values[2]);
        }

        [Serializable]
        private sealed class VehicleGeometryManifest
        {
            public int schemaVersion;
            public VehicleGeometryReceipt[] vehicles;
        }

        [Serializable]
        private sealed class VehicleGeometryReceipt
        {
            public string id;
            public int sourceCount;
            public int vertexCount;
            public VehicleGeometryBounds bounds;
            public int compressedBytes;
        }

        [Serializable]
        private sealed class VehicleGeometryBounds
        {
            public float[] min;
            public float[] max;
        }
    }
}
