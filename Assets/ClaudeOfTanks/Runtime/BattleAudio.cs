using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleAudio : MonoBehaviour
    {
        public const int OneShotVoiceLimit = 24;
        public const int EngineVoiceLimit = 10;
        public const float EngineHearInM = 900f;
        public const float EngineHearOutM = 1000f;

        private readonly Queue<BattleOneShotVoice> _availableOneShots =
            new Queue<BattleOneShotVoice>();
        private readonly List<BattleOneShotVoice> _activeOneShots =
            new List<BattleOneShotVoice>();
        private readonly List<BattleOneShotVoice> _oneShots =
            new List<BattleOneShotVoice>();
        private readonly Queue<BattleEngineVoice> _availableEngines =
            new Queue<BattleEngineVoice>();
        private readonly Dictionary<string, BattleEngineVoice> _activeEngines =
            new Dictionary<string, BattleEngineVoice>(StringComparer.Ordinal);
        private readonly List<string> _staleEngineIds =
            new List<string>(EngineVoiceLimit);
        private readonly TankState[] _engineCandidates =
            new TankState[EngineVoiceLimit];
        private readonly float[] _engineScores =
            new float[EngineVoiceLimit];
        private GameSettings _settings;
        private AudioClip _shotLight;
        private AudioClip _shotHeavy;
        private AudioClip _penetration;
        private AudioClip _ricochet;
        private AudioClip _destroyed;
        private AudioClip _engineLoop;
        private AudioClip _ambienceLoop;
        private AudioSource _ambience;
        private BattleReloadAudio _reload;
        private TankState _occupiedTank;
        private int _engineCandidateCount;
        private float _battleDuck = 1f;
        private uint _noise = 0x91e10da5u;

        public int ActiveOneShotCount => _activeOneShots.Count;
        public int ActiveEngineCount => _activeEngines.Count;
        public bool AmbienceActive =>
            _ambience != null && _ambience.gameObject.activeSelf;
        public float EngineGain => _settings.EngineVolume;
        public float CombatGain => _settings.CombatVolume;
        public float AmbienceGain => _settings.AmbienceVolume;
        public float UiGain => _settings.UiVolume;
        public float VoiceGain => _settings.VoiceVolume;
        public int ActiveReloadVoiceCount => _reload.ActiveVoiceCount;
        public int ReloadCueCount => _reload.CueCount;
        public int ReloadReadyCount => _reload.ReadyCount;
        public BattleReloadProfile ReloadProfile => _reload.Profile;

        public static BattleAudio Create(GameSettings settings = null)
        {
            GameObject root = new GameObject("BattleAudio");
            BattleAudio audio = root.AddComponent<BattleAudio>();
            audio._settings = settings ?? GameSettings.Current;
            audio.Initialize();
            audio.BeginBattle();
            return audio;
        }

        public void BeginBattle()
        {
            if (_ambience == null) return;
            _ambience.gameObject.SetActive(true);
            _ambience.volume = 0.16f * AmbienceGain;
            if (Application.isPlaying && !_ambience.isPlaying)
                _ambience.Play();
        }

        public void Play(BattleEvent battleEvent)
        {
            AudioClip clip;
            float volume;
            if (!TryResolveOneShot(battleEvent, out clip, out volume))
                return;

            BattleOneShotVoice voice = AcquireOneShot();
            voice.Root.transform.position =
                battleEvent.Position.ToUnity();
            voice.Source.clip = clip;
            voice.BaseVolume = volume;
            voice.Source.volume = volume * CombatGain * _battleDuck;
            voice.Source.pitch = 0.97f + Next01() * 0.06f;
            voice.ExpiresAt = Time.unscaledTime + clip.length + 0.1f;
            voice.Root.SetActive(true);
            if (Application.isPlaying) voice.Source.Play();
            _activeOneShots.Add(voice);
        }

        public void SyncEngines(
            IList<TankState> tanks,
            string listenerOwnerId,
            Vector3 listenerPosition,
            bool scoped)
        {
            BuildEngineCandidates(
                tanks,
                listenerOwnerId,
                listenerPosition);
            _reload.Sync(_occupiedTank);
            ReleaseStaleEngines();
            for (int i = 0; i < _engineCandidateCount; i++)
            {
                TankState tank = _engineCandidates[i];
                BattleEngineVoice voice;
                if (!_activeEngines.TryGetValue(tank.Id, out voice))
                {
                    voice = _availableEngines.Dequeue();
                    voice.Root.SetActive(true);
                    if (Application.isPlaying) voice.Source.Play();
                    _activeEngines.Add(tank.Id, voice);
                }
                UpdateEngine(
                    voice,
                    tank,
                    tank.Id == listenerOwnerId,
                    listenerPosition,
                    scoped);
            }
        }

        public void SetKillcamDucking(bool active)
        {
            float next = active ? 0.35f : 1f;
            if (Mathf.Approximately(_battleDuck, next)) return;
            _battleDuck = next;
            _reload.SetBattleDucking(next);
            ApplyMix();
        }

        public bool HasEngineVoice(string entityId)
        {
            return entityId != null &&
                _activeEngines.ContainsKey(entityId);
        }

        public void ResetAll()
        {
            _activeOneShots.Clear();
            _availableOneShots.Clear();
            for (int i = 0; i < _oneShots.Count; i++)
            {
                BattleOneShotVoice voice = _oneShots[i];
                voice.Source.Stop();
                voice.Root.SetActive(false);
                _availableOneShots.Enqueue(voice);
            }

            _staleEngineIds.Clear();
            foreach (string id in _activeEngines.Keys)
                _staleEngineIds.Add(id);
            for (int i = 0; i < _staleEngineIds.Count; i++)
                ReleaseEngine(_staleEngineIds[i]);

            if (_ambience != null)
            {
                _ambience.Stop();
                _ambience.gameObject.SetActive(false);
            }
            _reload.ResetAll();
            _battleDuck = 1f;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _activeOneShots.Count - 1; i >= 0; i--)
            {
                BattleOneShotVoice voice = _activeOneShots[i];
                if (voice.ExpiresAt > now) continue;
                voice.Source.Stop();
                voice.Root.SetActive(false);
                _activeOneShots.RemoveAt(i);
                _availableOneShots.Enqueue(voice);
            }
        }

        private void Initialize()
        {
            _shotLight = BattleAudioClips.Tone(
                "ShotLight",
                92f,
                0.16f,
                0.34f,
                0.32f);
            _shotHeavy = BattleAudioClips.Tone(
                "ShotHeavy",
                54f,
                0.34f,
                0.5f,
                0.5f);
            _penetration = BattleAudioClips.Tone(
                "Penetration",
                145f,
                0.16f,
                0.3f,
                0.62f);
            _ricochet = BattleAudioClips.Tone(
                "Ricochet",
                620f,
                0.2f,
                0.2f,
                0.72f);
            _destroyed = BattleAudioClips.Tone(
                "Destroyed",
                42f,
                0.8f,
                0.58f,
                0.55f);
            _engineLoop = BattleAudioClips.EngineLoop();
            _ambienceLoop = BattleAudioClips.AmbienceLoop();

            for (int i = 0; i < OneShotVoiceLimit; i++)
            {
                BattleOneShotVoice voice = CreateOneShot(i);
                _oneShots.Add(voice);
                _availableOneShots.Enqueue(voice);
            }
            for (int i = 0; i < EngineVoiceLimit; i++)
                _availableEngines.Enqueue(CreateEngine(i));
            _reload = BattleReloadAudio.Create(transform, _settings);

            GameObject ambienceRoot = new GameObject("Ambience");
            ambienceRoot.transform.SetParent(transform, false);
            _ambience = ambienceRoot.AddComponent<AudioSource>();
            _ambience.playOnAwake = false;
            _ambience.loop = true;
            _ambience.spatialBlend = 0f;
            _ambience.clip = _ambienceLoop;
            _settings.AudioChanged += ApplyMix;
            ApplyMix();
        }

        private BattleOneShotVoice CreateOneShot(int index)
        {
            GameObject root = new GameObject("OneShot-" + index);
            root.transform.SetParent(transform, false);
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 22f;
            source.maxDistance = 900f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0f;
            root.SetActive(false);
            return new BattleOneShotVoice { Root = root, Source = source };
        }

        private BattleEngineVoice CreateEngine(int index)
        {
            GameObject root = new GameObject("Engine-" + index);
            root.transform.SetParent(transform, false);
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 1f;
            source.minDistance = 22f;
            source.maxDistance = EngineHearOutM;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0f;
            source.clip = _engineLoop;
            AudioLowPassFilter lowPass =
                root.AddComponent<AudioLowPassFilter>();
            root.SetActive(false);
            return new BattleEngineVoice
            {
                Root = root,
                Source = source,
                LowPass = lowPass
            };
        }

        private BattleOneShotVoice AcquireOneShot()
        {
            if (_availableOneShots.Count == 0)
            {
                BattleOneShotVoice oldest = _activeOneShots[0];
                _activeOneShots.RemoveAt(0);
                oldest.Source.Stop();
                oldest.Root.SetActive(false);
                _availableOneShots.Enqueue(oldest);
            }
            return _availableOneShots.Dequeue();
        }

        private bool TryResolveOneShot(
            BattleEvent battleEvent,
            out AudioClip clip,
            out float volume)
        {
            if (battleEvent.Type == BattleEventType.ShellFired)
            {
                clip = battleEvent.CaliberMm > 105f
                    ? _shotHeavy
                    : _shotLight;
                volume = Mathf.Lerp(
                    0.55f,
                    1f,
                    Mathf.Clamp01(battleEvent.CaliberMm / 150f));
                return true;
            }
            if (battleEvent.Type == BattleEventType.TankDestroyed ||
                battleEvent.Type == BattleEventType.StructureDestroyed)
            {
                clip = _destroyed;
                volume = 1f;
                return true;
            }
            if (battleEvent.Type == BattleEventType.ShellHit ||
                battleEvent.Type == BattleEventType.StructureHit ||
                battleEvent.Type == BattleEventType.PropCrushed)
            {
                clip = battleEvent.Penetrated
                    ? _penetration
                    : _ricochet;
                volume = battleEvent.Penetrated ? 0.82f : 0.65f;
                return true;
            }
            clip = null;
            volume = 0f;
            return false;
        }

        private void BuildEngineCandidates(
            IList<TankState> tanks,
            string ownerId,
            Vector3 listenerPosition)
        {
            _occupiedTank = null;
            for (int i = 0; i < _engineCandidateCount; i++)
                _engineCandidates[i] = null;
            _engineCandidateCount = 0;
            if (tanks == null) return;

            for (int i = 0; i < tanks.Count; i++)
            {
                TankState tank = tanks[i];
                if (tank == null || tank.Destroyed) continue;
                bool own = tank.Id == ownerId;
                if (own) _occupiedTank = tank;
                float distance = Vector3.Distance(
                    listenerPosition,
                    tank.Position.ToUnity());
                bool active = _activeEngines.ContainsKey(tank.Id);
                float limit = active ? EngineHearOutM : EngineHearInM;
                if (!own && distance > limit) continue;
                float score = own
                    ? float.MinValue
                    : distance - (active ? 24f : 0f);
                InsertEngineCandidate(tank, score);
            }
        }

        private void InsertEngineCandidate(TankState tank, float score)
        {
            int index = _engineCandidateCount;
            if (index < EngineVoiceLimit)
            {
                _engineCandidateCount++;
            }
            else
            {
                index = EngineVoiceLimit - 1;
                if (score >= _engineScores[index]) return;
            }
            while (index > 0 && score < _engineScores[index - 1])
            {
                _engineCandidates[index] =
                    _engineCandidates[index - 1];
                _engineScores[index] = _engineScores[index - 1];
                index--;
            }
            _engineCandidates[index] = tank;
            _engineScores[index] = score;
        }

        private void ReleaseStaleEngines()
        {
            _staleEngineIds.Clear();
            foreach (string id in _activeEngines.Keys)
            {
                bool selected = false;
                for (int i = 0; i < _engineCandidateCount; i++)
                {
                    if (_engineCandidates[i].Id != id) continue;
                    selected = true;
                    break;
                }
                if (!selected) _staleEngineIds.Add(id);
            }
            for (int i = 0; i < _staleEngineIds.Count; i++)
                ReleaseEngine(_staleEngineIds[i]);
        }

        private void ReleaseEngine(string id)
        {
            BattleEngineVoice voice;
            if (!_activeEngines.TryGetValue(id, out voice)) return;
            _activeEngines.Remove(id);
            voice.Source.Stop();
            voice.Root.SetActive(false);
            _availableEngines.Enqueue(voice);
        }

        private void UpdateEngine(
            BattleEngineVoice voice,
            TankState tank,
            bool own,
            Vector3 listenerPosition,
            bool scoped)
        {
            Vector3 position = tank.Position.ToUnity();
            float distance = own
                ? 0f
                : Vector3.Distance(listenerPosition, position);
            float topSpeed =
                Mathf.Max(1f, tank.Spec.TopSpeedKmh / 3.6f);
            float speed = Mathf.Clamp01(
                Mathf.Abs(tank.SpeedMps) / topSpeed);
            voice.Root.transform.position = position;
            voice.Source.spatialBlend = own ? 0f : 1f;
            voice.Source.pitch = 0.8f + speed * 0.6f;
            float load = 0.44f + speed * 0.56f;
            voice.BaseVolume = load * (own && scoped ? 1.18f : 1f);
            voice.Source.volume =
                voice.BaseVolume * EngineGain * _battleDuck;
            float cutoff = Mathf.Clamp(
                18000f * (40f / (40f + distance)),
                450f,
                18000f);
            voice.LowPass.cutoffFrequency =
                own && scoped ? Mathf.Min(650f, cutoff) : cutoff;
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

        private float Next01()
        {
            _noise = _noise * 1664525u + 1013904223u;
            return (_noise >> 8) / 16777216f;
        }

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.AudioChanged -= ApplyMix;
            Release(_shotLight);
            Release(_shotHeavy);
            Release(_penetration);
            Release(_ricochet);
            Release(_destroyed);
            Release(_engineLoop);
            Release(_ambienceLoop);
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

    }
}
