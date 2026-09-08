using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class BattleOneShotVoice
    {
        public GameObject Root;
        public AudioSource Source;
        public float BaseVolume;
        public float ExpiresAt;
    }

    internal sealed class BattleEngineVoice
    {
        public GameObject Root;
        public AudioSource Source;
        public AudioLowPassFilter LowPass;
        public float BaseVolume;
    }
}
