using System;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct CameraPostProfile
    {
        private const float BaseAerialDensity = 0.00145f;
        private const float BaseAerialHazeDensity = 0.00092f;

        private CameraPostProfile(
            float bloomStrength,
            float bloomThreshold,
            int blurIterations,
            int bloomDownsample,
            int aoSamples,
            int aoDownsample,
            float aoIntensity,
            float aoRadiusM,
            float aerialStrength,
            float exposure,
            float mapExposure,
            float contrast,
            float saturation,
            float vignette,
            float blackLift,
            float highlightKnee,
            float brightVignetteKeep,
            float aerialDensity,
            float aerialHazeDensity,
            float hazeLuminanceCap)
        {
            BloomStrength = bloomStrength;
            BloomThreshold = bloomThreshold;
            BlurIterations = blurIterations;
            BloomDownsample = bloomDownsample;
            AoSamples = aoSamples;
            AoDownsample = aoDownsample;
            AoIntensity = aoIntensity;
            AoRadiusM = aoRadiusM;
            AerialStrength = aerialStrength;
            Exposure = exposure;
            MapExposure = mapExposure;
            Contrast = contrast;
            Saturation = saturation;
            Vignette = vignette;
            BlackLift = blackLift;
            HighlightKnee = highlightKnee;
            BrightVignetteKeep = brightVignetteKeep;
            AerialDensity = aerialDensity;
            AerialHazeDensity = aerialHazeDensity;
            HazeLuminanceCap = hazeLuminanceCap;
        }

        public float BloomStrength { get; }
        public float BloomThreshold { get; }
        public int BlurIterations { get; }
        public int BloomDownsample { get; }
        public int AoSamples { get; }
        public int AoDownsample { get; }
        public float AoIntensity { get; }
        public float AoRadiusM { get; }
        public float AerialStrength { get; }
        public float Exposure { get; }
        public float MapExposure { get; }
        public float Contrast { get; }
        public float Saturation { get; }
        public float Vignette { get; }
        public float BlackLift { get; }
        public float HighlightKnee { get; }
        public float BrightVignetteKeep { get; }
        public float AerialDensity { get; }
        public float AerialHazeDensity { get; }
        public float HazeLuminanceCap { get; }

        public static CameraPostProfile Resolve(
            int quality,
            int trim,
            bool highContrast,
            MapSky sky = null)
        {
            float bloom;
            int iterations;
            int bloomDownsample;
            int aoSamples;
            int aoDownsample;
            float aoIntensity;
            float aoRadius;
            if (quality >= 5)
            {
                bloom = 0.24f;
                iterations = 2;
                bloomDownsample = 2;
                aoSamples = 12;
                aoDownsample = 2;
                aoIntensity = 0.62f;
                aoRadius = 2.3f;
            }
            else if (quality >= 4)
            {
                bloom = 0.2f;
                iterations = 2;
                bloomDownsample = 2;
                aoSamples = 10;
                aoDownsample = 2;
                aoIntensity = 0.58f;
                aoRadius = 2.1f;
            }
            else if (quality >= 3)
            {
                bloom = 0.16f;
                iterations = 1;
                bloomDownsample = 2;
                aoSamples = 8;
                aoDownsample = 2;
                aoIntensity = 0.52f;
                aoRadius = 1.9f;
            }
            else if (quality >= 2)
            {
                bloom = 0.1f;
                iterations = 1;
                bloomDownsample = 4;
                aoSamples = 6;
                aoDownsample = 4;
                aoIntensity = 0.44f;
                aoRadius = 1.7f;
            }
            else
            {
                bloom = 0f;
                iterations = 0;
                bloomDownsample = 4;
                aoSamples = 0;
                aoDownsample = 4;
                aoIntensity = 0f;
                aoRadius = 0f;
            }

            if (trim >= 1)
            {
                aoSamples = 0;
                aoIntensity = 0f;
            }
            if (trim == 1)
            {
                bloom *= 0.55f;
                iterations = Math.Min(iterations, 1);
                bloomDownsample =
                    Math.Max(bloomDownsample, 4);
            }
            else if (trim >= 2)
            {
                bloom = 0f;
                iterations = 0;
            }

            float mapExposure = sky != null && sky.postExposure > 0f
                ? Clamp(sky.postExposure, 0.72f, 1.08f)
                : 1f;
            float fogScale = sky != null && sky.fogDensity > 0f
                ? Clamp(sky.fogDensity / 0.00062f, 0.76f, 1.24f)
                : 1f;
            float exposureTrim = Math.Max(0f, 1f - mapExposure);
            float highlightKnee = Clamp(0.80f - exposureTrim * 0.95f, 0.70f, 0.80f);
            float brightVignetteKeep = Clamp(0.62f - exposureTrim * 1.25f, 0.48f, 0.62f);
            float hazeLuminanceCap = Clamp(0.385f - exposureTrim * 0.50f, 0.32f, 0.385f);

            return new CameraPostProfile(
                bloom,
                quality >= 4 ? 1.78f : 1.95f,
                iterations,
                bloomDownsample,
                aoSamples,
                aoDownsample,
                aoIntensity,
                aoRadius,
                quality >= 2 ? 1f : 0.72f,
                quality >= 3 ? -0.08f : -0.04f,
                mapExposure,
                highContrast ? 1.36f : 1.18f,
                quality >= 2 ? 1.045f : 1f,
                quality >= 2 ? 0.14f : 0.1f,
                0.022f,
                highlightKnee,
                brightVignetteKeep,
                BaseAerialDensity * fogScale,
                BaseAerialHazeDensity * fogScale,
                hazeLuminanceCap);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
}
