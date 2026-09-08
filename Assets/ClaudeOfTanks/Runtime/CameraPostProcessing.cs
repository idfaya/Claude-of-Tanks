using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraPostProcessing : MonoBehaviour
    {
        private const string ShaderName = "Hidden/ClaudeOfTanks/Post";
        private const float SampleWindowS = 1f;
        private Camera _camera;
        private GameSettings _settings;
        private AdaptiveQualityPolicy _policy;
        private Material _material;
        private RenderTexture _bloomA;
        private RenderTexture _bloomB;
        private int _bloomWidth;
        private int _bloomHeight;
        private RenderTextureFormat _bloomFormat;
        private float _frameEmaMs;
        private float _sampleElapsed;
        private int _sampleFrames;
        private int _missedFrames;
        private float _appliedScale = 1f;
        private bool _warnedMissingShader;

        public int RequestedQuality =>
            _policy != null ? _policy.RequestedQuality : 0;
        public int EffectiveQuality =>
            _policy != null ? _policy.EffectiveQuality : 0;
        public int PerformanceTrim =>
            _policy != null ? _policy.PerformanceTrim : 0;
        public float RenderScale =>
            _policy != null ? _policy.RenderScale : 1f;
        public bool BloomEnabled =>
            _material != null && ResolveProfile().BloomStrength > 0f;
        public bool ShaderAvailable => _material != null;

        public static CameraPostProcessing Ensure(
            Camera camera,
            GameSettings settings = null)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));
            CameraPostProcessing effects =
                camera.GetComponent<CameraPostProcessing>();
            if (effects == null)
                effects = camera.gameObject
                    .AddComponent<CameraPostProcessing>();
            effects.Configure(settings ?? GameSettings.Current);
            return effects;
        }

        public void Configure(GameSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (_settings == settings && _policy != null)
                return;
            if (_settings != null)
                _settings.PresentationChanged -= SyncRequestedQuality;
            _settings = settings;
            _settings.PresentationChanged += SyncRequestedQuality;
            _camera = GetComponent<Camera>();
            _camera.allowHDR = true;
            _camera.allowDynamicResolution = true;
            EnsureMaterial();
            SyncRequestedQuality();
        }

        public AdaptiveQualityAction EvaluateWindow(
            AdaptiveQualityWindow window)
        {
            if (_policy == null)
                throw new InvalidOperationException(
                    "Post processing is not configured.");
            AdaptiveQualityAction action = _policy.Evaluate(window);
            if (action != AdaptiveQualityAction.None)
                ApplyPolicyState();
            return action;
        }

        private void EnsureMaterial()
        {
            if (_material != null) return;
            Shader shader = Shader.Find(ShaderName);
            if (shader == null || !shader.isSupported)
            {
                if (!_warnedMissingShader)
                {
                    Debug.LogWarning(
                        "Post-processing shader unavailable; " +
                        "using passthrough rendering.");
                    _warnedMissingShader = true;
                }
                return;
            }
            _material = new Material(shader)
            {
                name = "ClaudeOfTanksPostMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void SyncRequestedQuality()
        {
            int requested = _settings.QualityLevel;
            if (_policy == null)
                _policy = new AdaptiveQualityPolicy(requested);
            else
                _policy.Reset(requested, Time.unscaledTime);
            ResetSampling();
            ApplyPolicyState();
        }

        private void Update()
        {
            if (_policy == null) return;
            if (!Application.isFocused)
            {
                ResetSampling();
                return;
            }
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f || dt > 0.25f) return;
            float frameMs = dt * 1000f;
            _frameEmaMs = _sampleFrames == 0
                ? frameMs
                : Mathf.Lerp(_frameEmaMs, frameMs, 0.08f);
            _sampleElapsed += dt;
            _sampleFrames++;
            float budgetMs = FrameBudgetMs();
            if (frameMs > budgetMs * 1.1f) _missedFrames++;
            if (_sampleElapsed < SampleWindowS) return;
            EvaluateWindow(new AdaptiveQualityWindow(
                Time.unscaledTime,
                _frameEmaMs,
                budgetMs,
                _sampleFrames > 0
                    ? (float)_missedFrames / _sampleFrames
                    : 0f));
            _sampleElapsed = 0f;
            _sampleFrames = 0;
            _missedFrames = 0;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            ResetSampling();
        }

        private void OnApplicationPause(bool paused)
        {
            ResetSampling();
        }

        private void ApplyPolicyState()
        {
            int effective = _policy.EffectiveQuality;
            if (QualitySettings.GetQualityLevel() != effective)
                QualitySettings.SetQualityLevel(effective, false);
            float scale = _policy.RenderScale;
            if (Mathf.Abs(scale - _appliedScale) > 0.001f)
            {
                ScalableBufferManager.ResizeBuffers(scale, scale);
                _appliedScale = scale;
            }
        }

        private void OnRenderImage(
            RenderTexture source,
            RenderTexture destination)
        {
            EnsureMaterial();
            if (_material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Profile profile = ResolveProfile();
            ApplyMaterial(profile);
            if (profile.BloomStrength <= 0f)
            {
                _material.SetTexture("_BloomTex", Texture2D.blackTexture);
                Graphics.Blit(source, destination, _material, 2);
                return;
            }

            EnsureBloomTargets(source, profile.Downsample);
            Graphics.Blit(source, _bloomA, _material, 0);
            for (int i = 0; i < profile.BlurIterations; i++)
            {
                _material.SetVector(
                    "_BlurDirection",
                    new Vector2(1f, 0f));
                Graphics.Blit(_bloomA, _bloomB, _material, 1);
                _material.SetVector(
                    "_BlurDirection",
                    new Vector2(0f, 1f));
                Graphics.Blit(_bloomB, _bloomA, _material, 1);
            }
            _material.SetTexture("_BloomTex", _bloomA);
            Graphics.Blit(source, destination, _material, 2);
        }

        private Profile ResolveProfile()
        {
            int quality = EffectiveQuality;
            int trim = PerformanceTrim;
            float bloom;
            int iterations;
            int downsample;
            if (quality >= 5)
            {
                bloom = 0.24f;
                iterations = 2;
                downsample = 2;
            }
            else if (quality >= 4)
            {
                bloom = 0.2f;
                iterations = 2;
                downsample = 2;
            }
            else if (quality >= 3)
            {
                bloom = 0.16f;
                iterations = 1;
                downsample = 2;
            }
            else if (quality >= 2)
            {
                bloom = 0.1f;
                iterations = 1;
                downsample = 4;
            }
            else
            {
                bloom = 0f;
                iterations = 0;
                downsample = 4;
            }
            if (trim == 1)
            {
                bloom *= 0.55f;
                iterations = Math.Min(iterations, 1);
                downsample = Math.Max(downsample, 4);
            }
            else if (trim >= 2)
            {
                bloom = 0f;
                iterations = 0;
            }
            return new Profile
            {
                BloomStrength = bloom,
                BloomThreshold = quality >= 4 ? 0.92f : 1.02f,
                BlurIterations = iterations,
                Downsample = downsample,
                Exposure = quality >= 3 ? -0.08f : -0.04f,
                Contrast = (_settings != null &&
                    _settings.HighContrast) ? 1.13f : 1.055f,
                Saturation = quality >= 2 ? 1.035f : 1f,
                Vignette = quality >= 2 ? 0.16f : 0.1f
            };
        }

        private void ApplyMaterial(Profile profile)
        {
            _material.SetFloat(
                "_BloomThreshold",
                profile.BloomThreshold);
            _material.SetFloat(
                "_BloomStrength",
                profile.BloomStrength);
            _material.SetFloat("_Exposure", profile.Exposure);
            _material.SetFloat("_Contrast", profile.Contrast);
            _material.SetFloat("_Saturation", profile.Saturation);
            _material.SetFloat("_Vignette", profile.Vignette);
        }

        private void EnsureBloomTargets(
            RenderTexture source,
            int downsample)
        {
            int width = Mathf.Max(1, source.width / downsample);
            int height = Mathf.Max(1, source.height / downsample);
            if (_bloomA != null &&
                width == _bloomWidth &&
                height == _bloomHeight &&
                source.format == _bloomFormat)
            {
                return;
            }
            ReleaseBloomTargets();
            RenderTextureDescriptor descriptor = source.descriptor;
            descriptor.width = width;
            descriptor.height = height;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            _bloomA = new RenderTexture(descriptor)
            {
                name = "PostBloomA",
                hideFlags = HideFlags.HideAndDontSave
            };
            _bloomB = new RenderTexture(descriptor)
            {
                name = "PostBloomB",
                hideFlags = HideFlags.HideAndDontSave
            };
            _bloomA.Create();
            _bloomB.Create();
            _bloomWidth = width;
            _bloomHeight = height;
            _bloomFormat = source.format;
        }

        private static float FrameBudgetMs()
        {
            int target = Application.targetFrameRate;
            return target > 0 ? 1000f / target : 1000f / 60f;
        }

        private void ResetSampling()
        {
            _frameEmaMs = 0f;
            _sampleElapsed = 0f;
            _sampleFrames = 0;
            _missedFrames = 0;
        }

        private void ReleaseBloomTargets()
        {
            Release(_bloomA);
            Release(_bloomB);
            _bloomA = null;
            _bloomB = null;
            _bloomWidth = 0;
            _bloomHeight = 0;
        }

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.PresentationChanged -= SyncRequestedQuality;
            ReleaseBloomTargets();
            Release(_material);
            if (_appliedScale != 1f)
                ScalableBufferManager.ResizeBuffers(1f, 1f);
            if (_settings != null &&
                QualitySettings.GetQualityLevel() !=
                    _settings.QualityLevel)
            {
                QualitySettings.SetQualityLevel(
                    _settings.QualityLevel,
                    false);
            }
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private struct Profile
        {
            public float BloomStrength;
            public float BloomThreshold;
            public int BlurIterations;
            public int Downsample;
            public float Exposure;
            public float Contrast;
            public float Saturation;
            public float Vignette;
        }
    }
}
