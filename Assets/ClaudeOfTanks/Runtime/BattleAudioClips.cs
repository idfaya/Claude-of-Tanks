using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class BattleAudioClips
    {
        public static AudioClip Tone(
            string name,
            float frequency,
            float duration,
            float gain,
            float noiseGain)
        {
            const int rate = 22050;
            int count = Mathf.CeilToInt(duration * rate);
            float[] samples = new float[count];
            uint noise = 0x91e10da5u;
            for (int i = 0; i < count; i++)
            {
                noise = noise * 1664525u + 1013904223u;
                float random =
                    ((noise >> 8) / 16777216f) * 2f - 1f;
                float envelope =
                    Mathf.Exp(-5f * i / (float)count);
                samples[i] =
                    (Mathf.Sin(
                        2f * Mathf.PI * frequency * i / rate) *
                    (1f - noiseGain) + random * noiseGain) *
                    envelope * gain;
            }
            return Clip(name, samples, rate);
        }

        public static AudioClip EngineLoop()
        {
            const int rate = 22050;
            float[] samples = new float[rate];
            uint noise = 0x5f3759dfu;
            for (int i = 0; i < samples.Length; i++)
            {
                noise = noise * 1664525u + 1013904223u;
                float random =
                    ((noise >> 8) / 16777216f) * 2f - 1f;
                float phase = 2f * Mathf.PI * i / rate;
                samples[i] =
                    Mathf.Sin(phase * 46f) * 0.18f +
                    Mathf.Sin(phase * 92f) * 0.07f +
                    random * 0.025f;
            }
            return Clip("EngineLoop", samples, rate);
        }

        public static AudioClip AmbienceLoop()
        {
            const int rate = 22050;
            float[] samples = new float[rate * 2];
            uint noise = 0xc0ffeeu;
            float filtered = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                noise = noise * 1664525u + 1013904223u;
                float random =
                    ((noise >> 8) / 16777216f) * 2f - 1f;
                filtered += (random - filtered) * 0.025f;
                samples[i] = filtered * 0.42f;
            }
            return Clip("BattleAmbience", samples, rate);
        }

        private static AudioClip Clip(
            string name,
            float[] samples,
            int rate)
        {
            AudioClip clip = AudioClip.Create(
                name,
                samples.Length,
                1,
                rate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
