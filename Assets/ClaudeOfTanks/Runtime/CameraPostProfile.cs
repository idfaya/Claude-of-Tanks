using System;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct CameraPostProfile
    {
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
            float contrast,
            float saturation,
            float vignette)
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
            Contrast = contrast;
            Saturation = saturation;
            Vignette = vignette;
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
        public float Contrast { get; }
        public float Saturation { get; }
        public float Vignette { get; }

        public static CameraPostProfile Resolve(
            int quality,
            int trim,
            bool highContrast)
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

            return new CameraPostProfile(
                bloom,
                quality >= 4 ? 0.92f : 1.02f,
                iterations,
                bloomDownsample,
                aoSamples,
                aoDownsample,
                aoIntensity,
                aoRadius,
                quality >= 2 ? 1f : 0.72f,
                quality >= 3 ? -0.08f : -0.04f,
                highContrast ? 1.13f : 1.055f,
                quality >= 2 ? 1.035f : 1f,
                quality >= 2 ? 0.16f : 0.1f);
        }
    }
}
