using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class CameraPostProcessingTests
    {
        [Test]
        public void AdaptivePolicyRelievesAndRecoversInStableOrder()
        {
            AdaptiveQualityPolicy policy =
                new AdaptiveQualityPolicy(5);

            Assert.That(
                Evaluate(policy, 1f, 30f, 0.8f, 2),
                Is.EqualTo(AdaptiveQualityAction.None));
            Assert.That(policy.PerformanceTrim, Is.Zero);

            Assert.That(
                Evaluate(policy, 7f, 30f, 0.8f, 2),
                Is.EqualTo(AdaptiveQualityAction.TrimDown));
            Assert.That(policy.PerformanceTrim, Is.EqualTo(1));
            Assert.That(
                Evaluate(policy, 9f, 30f, 0.8f, 2),
                Is.EqualTo(AdaptiveQualityAction.TrimDown));
            Assert.That(policy.PerformanceTrim, Is.EqualTo(2));

            Assert.That(
                Evaluate(policy, 11f, 30f, 0.8f, 2),
                Is.EqualTo(AdaptiveQualityAction.ResolutionDown));
            Assert.That(policy.RenderScale, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(
                Evaluate(policy, 13f, 30f, 0.8f, 2),
                Is.EqualTo(AdaptiveQualityAction.ResolutionDown));
            Assert.That(policy.RenderScale, Is.EqualTo(0.8f).Within(0.001f));

            AdaptiveQualityAction tierAction =
                Evaluate(policy, 15f, 30f, 0.8f, 6);
            Assert.That(
                tierAction,
                Is.EqualTo(AdaptiveQualityAction.TierDown));
            Assert.That(policy.EffectiveQuality, Is.EqualTo(4));
            Assert.That(policy.RequestedQuality, Is.EqualTo(5));

            Assert.That(
                Evaluate(policy, 31f, 10f, 0f, 8),
                Is.EqualTo(AdaptiveQualityAction.ResolutionUp));
            Assert.That(
                Evaluate(policy, 39f, 10f, 0f, 8),
                Is.EqualTo(AdaptiveQualityAction.ResolutionUp));
            Assert.That(
                Evaluate(policy, 47f, 10f, 0f, 8),
                Is.EqualTo(AdaptiveQualityAction.TrimUp));
            Assert.That(
                Evaluate(policy, 55f, 10f, 0f, 8),
                Is.EqualTo(AdaptiveQualityAction.TrimUp));
            Assert.That(
                Evaluate(policy, 63f, 10f, 0f, 8),
                Is.EqualTo(AdaptiveQualityAction.TierUp));
            Assert.That(policy.EffectiveQuality, Is.EqualTo(5));
        }

        [Test]
        public void LowestQualityNeverDropsResolutionOrTier()
        {
            AdaptiveQualityPolicy policy =
                new AdaptiveQualityPolicy(0);

            AdaptiveQualityAction action =
                Evaluate(policy, 7f, 40f, 1f, 24);

            Assert.That(policy.EffectiveQuality, Is.Zero);
            Assert.That(policy.RenderScale, Is.EqualTo(1f));
            Assert.That(policy.PerformanceTrim, Is.EqualTo(2));
            Assert.That(action, Is.EqualTo(AdaptiveQualityAction.None));
        }

        [Test]
        public void SharedCameraEffectIsUniqueAndTracksUserQuality()
        {
            int originalQuality = QualitySettings.GetQualityLevel();
            bool originalFog = RenderSettings.fog;
            GameObject root = new GameObject(
                "PostProcessingTest",
                typeof(Camera));
            MemoryStore store = new MemoryStore();
            RecordingTarget target =
                new RecordingTarget { QualityLevelCount = 6 };
            GameSettings settings = new GameSettings(store, target);
            try
            {
                Camera camera = root.GetComponent<Camera>();
                CameraPostProcessing first =
                    CameraPostProcessing.Ensure(camera, settings);
                CameraPostProcessing second =
                    CameraPostProcessing.Ensure(camera, settings);

                Assert.That(second, Is.SameAs(first));
                Assert.That(
                    root.GetComponents<CameraPostProcessing>(),
                    Has.Length.EqualTo(1));
                Assert.That(first.ShaderAvailable, Is.True);
                Assert.That(first.RequestedQuality, Is.EqualTo(5));
                Assert.That(first.BloomEnabled, Is.True);
                Assert.That(
                    first.AmbientOcclusionEnabled,
                    Is.True);
                Assert.That(camera.allowHDR, Is.True);
                Assert.That(camera.allowDynamicResolution, Is.True);
                Assert.That(
                    camera.depthTextureMode &
                        DepthTextureMode.Depth,
                    Is.EqualTo(DepthTextureMode.Depth));
                Assert.That(
                    camera.depthTextureMode &
                        DepthTextureMode.DepthNormals,
                    Is.EqualTo(DepthTextureMode.DepthNormals));

                RenderSettings.fog = false;
                Assert.That(
                    first.AerialPerspectiveEnabled,
                    Is.False);
                RenderSettings.fog = true;
                Assert.That(
                    first.AerialPerspectiveEnabled,
                    Is.True);

                float clock = Time.unscaledTime + 7f;
                first.EvaluateWindow(
                    new AdaptiveQualityWindow(
                        clock,
                        30f,
                        1000f / 60f,
                        0.8f));
                first.EvaluateWindow(
                    new AdaptiveQualityWindow(
                        clock + 1f,
                        30f,
                        1000f / 60f,
                        0.8f));
                Assert.That(first.PerformanceTrim, Is.EqualTo(1));
                Assert.That(
                    first.AmbientOcclusionEnabled,
                    Is.False,
                    "AO is the first adaptive quality relief.");
                Assert.That(
                    camera.depthTextureMode &
                        DepthTextureMode.DepthNormals,
                    Is.EqualTo(DepthTextureMode.None));
                Assert.That(first.BloomEnabled, Is.True);

                settings.SetQualityLevel(0);
                Assert.That(first.RequestedQuality, Is.Zero);
                Assert.That(first.EffectiveQuality, Is.Zero);
                Assert.That(first.BloomEnabled, Is.False);
                Assert.That(
                    first.AmbientOcclusionEnabled,
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
                RenderSettings.fog = originalFog;
                QualitySettings.SetQualityLevel(
                    originalQuality,
                    false);
                ScalableBufferManager.ResizeBuffers(1f, 1f);
            }
        }

        [Test]
        public void MapSkyDrivesExposureAndAerialProfile()
        {
            int originalQuality = QualitySettings.GetQualityLevel();
            GameObject root = new GameObject(
                "PostProcessingMapSkyTest",
                typeof(Camera));
            MemoryStore store = new MemoryStore();
            RecordingTarget target =
                new RecordingTarget { QualityLevelCount = 6 };
            GameSettings settings = new GameSettings(store, target);
            ContentCatalog catalog = ContentCatalog.Load();
            try
            {
                CameraPostProcessing post =
                    CameraPostProcessing.Ensure(
                        root.GetComponent<Camera>(),
                        settings);
                post.ApplyMap(catalog.GetMap("desert"));

                Assert.That(
                    post.ActiveMapExposure,
                    Is.EqualTo(0.90f).Within(0.001f));
                Assert.That(
                    post.ActiveAerialDensity,
                    Is.LessThan(0.00145f));
                Assert.That(
                    post.ActiveBloomThreshold,
                    Is.EqualTo(1.78f).Within(0.001f));
                Assert.That(
                    post.ActiveHighlightKnee,
                    Is.LessThan(0.72f));
                Assert.That(
                    post.ActiveBrightVignetteKeep,
                    Is.LessThan(0.52f));
                Assert.That(
                    post.ActiveHazeLuminanceCap,
                    Is.LessThan(0.34f));

                post.ApplyMap(catalog.GetMap("winter"));
                Assert.That(
                    post.ActiveMapExposure,
                    Is.EqualTo(0.94f).Within(0.001f));
                Assert.That(
                    post.ActiveAerialHazeDensity,
                    Is.GreaterThan(0.00082f));
                Assert.That(
                    post.ActiveHighlightKnee,
                    Is.GreaterThan(post.ActiveMapExposure * 0.76f));

                post.ApplyMap(null);
                Assert.That(
                    post.ActiveMapExposure,
                    Is.EqualTo(1f).Within(0.001f));
                Assert.That(
                    post.ActiveAerialDensity,
                    Is.EqualTo(0.00145f).Within(0.00001f));
                Assert.That(
                    post.ActiveHighlightKnee,
                    Is.EqualTo(0.80f).Within(0.001f));
                Assert.That(
                    post.ActiveHazeLuminanceCap,
                    Is.EqualTo(0.385f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
                QualitySettings.SetQualityLevel(
                    originalQuality,
                    false);
                ScalableBufferManager.ResizeBuffers(1f, 1f);
            }
        }

        private static AdaptiveQualityAction Evaluate(
            AdaptiveQualityPolicy policy,
            float firstClock,
            float frameMs,
            float missedRatio,
            int count)
        {
            AdaptiveQualityAction action =
                AdaptiveQualityAction.None;
            for (int i = 0; i < count; i++)
            {
                action = policy.Evaluate(
                    new AdaptiveQualityWindow(
                        firstClock + i,
                        frameMs,
                        1000f / 60f,
                        missedRatio));
            }
            return action;
        }

        private sealed class MemoryStore : ISettingsStore
        {
            private readonly Dictionary<string, int> _ints =
                new Dictionary<string, int>();
            public int GetInt(string key, int fallback)
            {
                int value;
                return _ints.TryGetValue(key, out value)
                    ? value
                    : fallback;
            }
            public float GetFloat(string key, float fallback)
            {
                return fallback;
            }
            public void SetInt(string key, int value)
            {
                _ints[key] = value;
            }
            public void SetFloat(string key, float value) { }
            public void Save() { }
        }

        private sealed class RecordingTarget : ISettingsTarget
        {
            public int QualityLevelCount { get; set; }
            public void ApplyVolume(float value) { }
            public void ApplyQuality(int value) { }
            public void ApplyFullscreen(bool value) { }
        }
    }
}
