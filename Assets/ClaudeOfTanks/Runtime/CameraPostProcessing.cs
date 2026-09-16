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
        private readonly CameraPostBuffers _buffers =
            new CameraPostBuffers();
        private MapSky _mapSky;
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
            _material != null && CurrentProfile().BloomStrength > 0f;
        public bool AmbientOcclusionEnabled =>
            _material != null && CurrentProfile().AoSamples > 0;
        public bool AerialPerspectiveEnabled =>
            _material != null && RenderSettings.fog;
        public bool ShaderAvailable => _material != null;
        public float ActiveMapExposure => CurrentProfile().MapExposure;
        public float ActiveAerialDensity => CurrentProfile().AerialDensity;
        public float ActiveAerialHazeDensity =>
            CurrentProfile().AerialHazeDensity;
        public float ActiveBloomThreshold =>
            CurrentProfile().BloomThreshold;
        public float ActiveHighlightKnee =>
            CurrentProfile().HighlightKnee;
        public float ActiveBrightVignetteKeep =>
            CurrentProfile().BrightVignetteKeep;
        public float ActiveHazeLuminanceCap =>
            CurrentProfile().HazeLuminanceCap;

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

        public void ApplyMap(MapDefinition map)
        {
            _mapSky = map?.sky;
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
            _camera.depthTextureMode |= DepthTextureMode.Depth;
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
            _camera.depthTextureMode |= DepthTextureMode.Depth;
            if (CurrentProfile().AoSamples > 0)
            {
                _camera.depthTextureMode |=
                    DepthTextureMode.DepthNormals;
            }
            else
            {
                _camera.depthTextureMode &=
                    ~DepthTextureMode.DepthNormals;
            }
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

            CameraPostProfile profile = CurrentProfile();
            ApplyMaterial(profile);
            RenderAmbientOcclusion(source, profile);
            if (profile.BloomStrength <= 0f)
            {
                _material.SetTexture("_BloomTex", Texture2D.blackTexture);
                Graphics.Blit(source, destination, _material, 2);
                return;
            }

            _buffers.EnsureBloom(
                source,
                profile.BloomDownsample);
            Graphics.Blit(
                source,
                _buffers.BloomA,
                _material,
                0);
            for (int i = 0; i < profile.BlurIterations; i++)
            {
                _material.SetVector(
                    "_BlurDirection",
                    new Vector2(1f, 0f));
                Graphics.Blit(
                    _buffers.BloomA,
                    _buffers.BloomB,
                    _material,
                    1);
                _material.SetVector(
                    "_BlurDirection",
                    new Vector2(0f, 1f));
                Graphics.Blit(
                    _buffers.BloomB,
                    _buffers.BloomA,
                    _material,
                    1);
            }
            _material.SetTexture(
                "_BloomTex",
                _buffers.BloomA);
            Graphics.Blit(source, destination, _material, 2);
        }

        private void RenderAmbientOcclusion(
            RenderTexture source,
            CameraPostProfile profile)
        {
            if (profile.AoSamples <= 0)
            {
                _material.SetTexture(
                    "_AoTex",
                    Texture2D.whiteTexture);
                return;
            }
            _buffers.EnsureAmbientOcclusion(
                source,
                profile.AoDownsample);
            Graphics.Blit(
                source,
                _buffers.AoA,
                _material,
                3);
            _material.SetVector(
                "_BlurDirection",
                Vector2.right);
            Graphics.Blit(
                _buffers.AoA,
                _buffers.AoB,
                _material,
                4);
            _material.SetVector(
                "_BlurDirection",
                Vector2.up);
            Graphics.Blit(
                _buffers.AoB,
                _buffers.AoA,
                _material,
                4);
            _material.SetTexture(
                "_AoTex",
                _buffers.AoA);
        }

        private CameraPostProfile CurrentProfile()
        {
            return CameraPostProfile.Resolve(
                EffectiveQuality,
                PerformanceTrim,
                _settings != null &&
                    _settings.HighContrast,
                _mapSky);
        }

        private void ApplyMaterial(
            CameraPostProfile profile)
        {
            _material.SetFloat(
                "_BloomThreshold",
                profile.BloomThreshold);
            _material.SetFloat(
                "_BloomStrength",
                profile.BloomStrength);
            _material.SetFloat("_Exposure", profile.Exposure);
            _material.SetFloat("_MapExposure", profile.MapExposure);
            _material.SetFloat("_Contrast", profile.Contrast);
            _material.SetFloat("_Saturation", profile.Saturation);
            _material.SetFloat("_Vignette", profile.Vignette);
            _material.SetFloat("_BlackLift", profile.BlackLift);
            _material.SetFloat("_HighlightKnee", profile.HighlightKnee);
            _material.SetFloat(
                "_BrightVignetteKeep",
                profile.BrightVignetteKeep);
            _material.SetFloat(
                "_HazeLuminanceCap",
                profile.HazeLuminanceCap);
            _material.SetFloat(
                "_AoSampleCount",
                profile.AoSamples);
            _material.SetFloat(
                "_AoIntensity",
                profile.AoIntensity);
            _material.SetFloat(
                "_AoRadiusM",
                profile.AoRadiusM);
            float halfFov = Mathf.Tan(
                _camera.fieldOfView *
                Mathf.Deg2Rad *
                0.5f);
            _material.SetFloat(
                "_TanHalfFov",
                Mathf.Max(0.01f, halfFov));
            _material.SetFloat(
                "_CameraAspect",
                Mathf.Max(0.01f, _camera.aspect));

            float zoomScale = _camera.fieldOfView < 15f
                ? Mathf.Max(
                    0.26f,
                    Mathf.Pow(
                        _camera.fieldOfView / 15f,
                        1.5f))
                : 1f;
            _material.SetFloat(
                "_AerialDensity",
                RenderSettings.fog
                    ? profile.AerialDensity * zoomScale
                    : 0f);
            _material.SetFloat(
                "_AerialHazeDensity",
                RenderSettings.fog
                    ? profile.AerialHazeDensity * zoomScale
                    : 0f);
            _material.SetFloat(
                "_AerialStrength",
                RenderSettings.fog
                    ? profile.AerialStrength
                    : 0f);
            _material.SetColor(
                "_FogColor",
                RenderSettings.fogColor);
            Vector3 sunDirection =
                RenderSettings.sun != null
                    ? -RenderSettings.sun.transform.forward
                    : Vector3.up;
            _material.SetVector(
                "_SunDirectionVS",
                _camera.transform.InverseTransformDirection(
                    sunDirection).normalized);
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

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.PresentationChanged -= SyncRequestedQuality;
            _buffers.Dispose();
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

    }
}
