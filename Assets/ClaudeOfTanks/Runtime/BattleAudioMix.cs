using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class BattleAudio
    {
        private float _battleDuck = 1f;
        private bool _killcamDucked, _pauseDucked;

        public float EngineGain => _settings.EngineVolume;
        public float CombatGain => _settings.CombatVolume;
        public float AmbienceGain => _settings.AmbienceVolume;
        public float UiGain => _settings.UiVolume;
        public float VoiceGain => _settings.VoiceVolume;

        public void SetKillcamDucking(bool active)
        {
            if (_killcamDucked == active) return;
            _killcamDucked = active;
            RefreshDucking();
        }

        public void SetPauseDucking(bool active)
        {
            if (_pauseDucked == active) return;
            _pauseDucked = active;
            RefreshDucking();
        }

        private void ApplyMix()
        {
            for (int i = 0; i < _activeOneShots.Count; i++)
            {
                BattleOneShotVoice voice = _activeOneShots[i];
                voice.Source.volume =
                    voice.BaseVolume * CombatGain * _battleDuck;
            }
            foreach (BattleEngineVoice voice in _activeEngines.Values)
            {
                voice.Source.volume =
                    voice.BaseVolume * EngineGain * _battleDuck;
            }
            if (_ambience != null)
                _ambience.volume = 0.16f * AmbienceGain;
        }

        private void RefreshDucking()
        {
            _battleDuck = (_killcamDucked ? 0.35f : 1f) *
                (_pauseDucked ? 0.04f : 1f);
            _reload.SetBattleDucking(_battleDuck);
            ApplyMix();
        }
    }
}
