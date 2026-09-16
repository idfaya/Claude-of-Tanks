using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class SceneStudioTests
    {
        [Test]
        public void StudioAddsPosesAndRoundTripsActors()
        {
            GameObject root =
                new GameObject("StudioTest");
            SceneStudioController studio =
                root.AddComponent<
                    SceneStudioController>();
            try
            {
                studio.Configure(
                    ContentCatalog.Load(),
                    "m1a2",
                    "verdant",
                    null);
                Assert.That(
                    Camera.main.clearFlags,
                    Is.EqualTo(CameraClearFlags.Skybox));
                Assert.That(
                    studio.ActorCount,
                    Is.EqualTo(1));
                studio.SetSelectedPose(
                    12f,
                    -8f,
                    45f,
                    -20f,
                    7f);
                StudioActorSnapshot actor =
                    studio.ActorAt(0);
                Assert.That(
                    actor.Position.X,
                    Is.EqualTo(12f));
                Assert.That(
                    actor.GunPitchDeg,
                    Is.EqualTo(7f)
                        .Within(0.001f));

                string json =
                    studio.ExportSceneJson();
                Assert.That(
                    studio.AddActor("t90m"),
                    Is.True);
                Assert.That(
                    studio.LoadSceneJson(json),
                    Is.True);
                Assert.That(
                    studio.ActorCount,
                    Is.EqualTo(1));
                Assert.That(
                    studio.ActorAt(0)
                        .VehicleId,
                    Is.EqualTo("m1a2"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CustomCamouflagePersistsSanitizedRecipe()
        {
            CustomCamouflageProfile previous =
                CustomCamouflageStore.Load();
            try
            {
                CustomCamouflageStore.Save(
                    new CustomCamouflageProfile
                    {
                        scheme = "digital",
                        baseColor = "#123456",
                        weatherColor = "#234567",
                        patchColorA = "#345678",
                        patchColorB = "#456789",
                        scale = 0.72f
                    });
                CustomCamouflageProfile loaded =
                    CustomCamouflageStore.Load();
                Assert.That(
                    loaded.scheme,
                    Is.EqualTo("digital"));
                Assert.That(
                    loaded.baseColor,
                    Is.EqualTo("#123456"));
                Assert.That(
                    loaded.scale,
                    Is.EqualTo(0.72f));
                Assert.That(
                    loaded.ToRecipe()
                        .patchColors,
                    Is.EqualTo(
                        new[]
                        {
                            "#345678",
                            "#456789"
                        }));
            }
            finally
            {
                CustomCamouflageStore.Save(
                    previous);
            }
        }
    }
}
