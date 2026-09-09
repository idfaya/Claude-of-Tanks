using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90MaterialParityTests
    {
        [Test]
        public void AppliesTsPbrRolesWithoutReplacingCamouflage()
        {
            TankView view = Create();
            try
            {
                AssertRole(
                    view,
                    "Painted-T90-LowerTub",
                    0.05f,
                    0.12f,
                    true);
                AssertRole(
                    view,
                    "Painted-T90-2A46MForwardTube",
                    0.08f,
                    0.2f,
                    true);
                AssertRole(
                    view,
                    "T90-GearRoadWheelDisc",
                    0.08f,
                    0.08f);
                AssertRole(
                    view,
                    "T90-GearRoadWheelTire",
                    0f,
                    0.04f);
                AssertRole(
                    view,
                    "T90-TrackPad",
                    0.08f,
                    0.05f);
                AssertRole(
                    view,
                    "T90-1G46Lens",
                    0.85f,
                    0.88f);
                AssertRole(
                    view,
                    "T90-SplitUnditchingLog",
                    0f,
                    0.12f);
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void AppliesShtoraAndDarkEmissionFloors()
        {
            TankView view = Create();
            try
            {
                Material shtora = Material(view, "T90-ShtoraLens");
                Assert.That(shtora.IsKeywordEnabled("_EMISSION"), Is.True);
                AssertColor(
                    shtora.GetColor("_EmissionColor"),
                    new Color(
                        0x7c / 255f,
                        0x24 / 255f,
                        0x10 / 255f));

                Material dark = Material(view, "T90-ShtoraDrum");
                Assert.That(dark.IsKeywordEnabled("_EMISSION"), Is.True);
                AssertColor(
                    dark.GetColor("_EmissionColor"),
                    new Color(
                        0x0c / 255f,
                        0x10 / 255f,
                        0x0a / 255f));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static void AssertRole(
            TankView view,
            string name,
            float metallic,
            float glossiness,
            bool requiresTexture = false)
        {
            Material material = Material(view, name);
            Assert.That(
                material.GetFloat("_Metallic"),
                Is.EqualTo(metallic).Within(0.0001f),
                name + " metallic");
            Assert.That(
                material.GetFloat("_Glossiness"),
                Is.EqualTo(glossiness).Within(0.0001f),
                name + " glossiness");
            if (requiresTexture)
                Assert.That(material.mainTexture, Is.Not.Null, name);
        }

        private static Material Material(
            TankView view,
            string name)
        {
            Transform transform = Find(view, name);
            Assert.That(transform, Is.Not.Null, name);
            Renderer renderer = transform.GetComponent<Renderer>();
            Assert.That(renderer, Is.Not.Null, name);
            Assert.That(renderer.sharedMaterial, Is.Not.Null, name);
            return renderer.sharedMaterial;
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            Transform[] transforms =
                view.Root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                    return transforms[index];
            }
            return null;
        }

        private static TankView Create()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90");
            return TankView.Create(
                new TankState(
                    "t90-material-parity-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static void AssertColor(
            Color actual,
            Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.0001f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.0001f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.0001f));
        }
    }
}
