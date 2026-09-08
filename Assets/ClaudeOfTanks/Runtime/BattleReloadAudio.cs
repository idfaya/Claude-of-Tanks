using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleReloadAudio : MonoBehaviour
    {
        public const int VoiceLimit = 4;

        private readonly Queue<ReloadVoice> _available =
            new Queue<ReloadVoice>();
        private readonly List<ReloadVoice> _active =
            new List<ReloadVoice>();
        private readonly List<ReloadVoice> _voices =
            new List<ReloadVoice>();
        private GameSettings _settings;
        private AudioClip[] _clips;
        private AudioClip _readyClip;
        private BattleReloadCuePlan _plan;
        private DamageReloadKind _kind;
        private float _totalS;
        private float _lastRemainingS;
        private float _caliberMm;
        private float _battleDuck = 1f;
        private int _nextCue;
        private bool _cycleActive;

        public int ActiveVoiceCount => _active.Count;
        public int CueCount { get; private set; }
        public int ReadyCount { get; private set; }
        public BattleReloadProfile Profile =>
            _plan != null ? _plan.Profile : BattleReloadProfile.Rapid;

        public static BattleReloadAudio Create(
            Transform parent,
            GameSettings settings)
        {
            GameObject root = new GameObject("ReloadAudio");
            root.transform.SetParent(parent, false);
            BattleReloadAudio audio =
                root.AddComponent<BattleReloadAudio>();
            audio._settings = settings ??
                throw new ArgumentNullException(nameof(settings));
            audio.Initialize();
            return audio;
        }

        public void Sync(TankState tank)
        {
            if (tank == null || tank.Destroyed)
            {
                _cycleActive = false;
                return;
            }

            float remainingS;
            float totalS;
            float caliberMm;
            DamageReloadKind kind;
            if (!TryResolveCycle(
                tank,
                out remainingS,
                out totalS,
                out caliberMm,
                out kind))
            {
                CompleteCycle();
                return;
            }

            bool restarted = !_cycleActive ||
                kind != _kind ||
                Math.Abs(totalS - _totalS) > 0.01f ||
                remainingS > _lastRemainingS + 0.04f;
            if (restarted)
                BeginCycle(totalS, kind, caliberMm);

            float progress = Mathf.Clamp01(1f - remainingS / _totalS);
            PlayCuesThrough(progress);
            _lastRemainingS = remainingS;
        }

        public void SetBattleDucking(float gain)
        {
            float next = Mathf.Clamp01(gain);
            if (Mathf.Approximately(_battleDuck, next)) return;
            _battleDuck = next;
            ApplyMix();
        }

        public void ResetAll()
        {
            _cycleActive = false;
            _plan = null;
            _nextCue = 0;
            _lastRemainingS = 0f;
            CueCount = 0;
            ReadyCount = 0;
            StopVoices();
            _battleDuck = 1f;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ReloadVoice voice = _active[i];
                if (voice.ExpiresAt > now) continue;
                ReleaseVoiceAt(i);
            }
        }

        private void Initialize()
        {
            _clips = new AudioClip[7];
            _clips[(int)BattleReloadCueType.Motor] =
                BattleAudioClips.Tone("ReloadMotor", 88f, 0.28f, 0.34f, 0.12f);
            _clips[(int)BattleReloadCueType.Index] =
                BattleAudioClips.Tone("ReloadIndex", 420f, 0.09f, 0.5f, 0.18f);
            _clips[(int)BattleReloadCueType.BreechOpen] =
                BattleAudioClips.Tone("BreechOpen", 190f, 0.14f, 0.46f, 0.24f);
            _clips[(int)BattleReloadCueType.Extract] =
                BattleAudioClips.Tone("ShellExtract", 310f, 0.11f, 0.4f, 0.3f);
            _clips[(int)BattleReloadCueType.ShellLift] =
                BattleAudioClips.Tone("ShellLift", 120f, 0.2f, 0.38f, 0.2f);
            _clips[(int)BattleReloadCueType.Ram] =
                BattleAudioClips.Tone("ShellRam", 72f, 0.16f, 0.55f, 0.28f);
            _clips[(int)BattleReloadCueType.BreechClose] =
                BattleAudioClips.Tone("BreechClose", 250f, 0.13f, 0.58f, 0.22f);
            _readyClip =
                BattleAudioClips.Tone("ReloadReady", 760f, 0.1f, 0.28f, 0.05f);

            for (int i = 0; i < VoiceLimit; i++)
            {
                ReloadVoice voice = CreateVoice(i);
                _voices.Add(voice);
                _available.Enqueue(voice);
            }
            _settings.AudioChanged += ApplyMix;
        }

        private ReloadVoice CreateVoice(int index)
        {
            GameObject root = new GameObject("Reload-" + index);
            root.transform.SetParent(transform, false);
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            root.SetActive(false);
            return new ReloadVoice { Root = root, Source = source };
        }

        private bool TryResolveCycle(
            TankState tank,
            out float remainingS,
            out float totalS,
            out float caliberMm,
            out DamageReloadKind kind)
        {
            remainingS = Math.Max(0f, tank.ReloadRemainingS);
            caliberMm = tank.Spec != null && tank.Spec.Shell != null
                ? Math.Max(12f, tank.Spec.Shell.CaliberMm)
                : 100f;
            DamageReloadState authoritative =
                tank.Combat != null ? tank.Combat.Reload : null;
            if (authoritative != null &&
                authoritative.Kind != DamageReloadKind.Ready &&
                authoritative.TotalS > 0f)
            {
                remainingS = Math.Max(remainingS, authoritative.RemainingS);
                totalS = Math.Max(0.05f, authoritative.TotalS);
                kind = authoritative.Kind;
                return remainingS > 0f;
            }
            if (remainingS <= 0f)
            {
                totalS = 0f;
                kind = DamageReloadKind.Ready;
                return false;
            }
            if (_cycleActive &&
                remainingS <= _lastRemainingS + 0.04f)
            {
                totalS = _totalS;
                kind = _kind;
                return true;
            }
            kind = BattleReloadAudioPolicy.InferKind(
                tank.Spec,
                remainingS);
            totalS = BattleReloadAudioPolicy.InferTotal(tank.Spec, kind);
            return true;
        }

        private void BeginCycle(
            float totalS,
            DamageReloadKind kind,
            float caliberMm)
        {
            StopVoices();
            _cycleActive = true;
            _totalS = Math.Max(0.05f, totalS);
            _lastRemainingS = _totalS;
            _kind = kind;
            _caliberMm = caliberMm;
            _nextCue = 0;
            _plan = BattleReloadAudioPolicy.Resolve(
                _totalS,
                kind,
                caliberMm);
        }

        private void CompleteCycle()
        {
            if (!_cycleActive) return;
            PlayCuesThrough(1f);
            if (_plan != null && _plan.Ready)
            {
                PlayClip(_readyClip, 0.44f, _caliberMm);
                ReadyCount++;
            }
            _cycleActive = false;
        }

        private void PlayCuesThrough(float progress)
        {
            if (_plan == null) return;
            while (_nextCue < _plan.Cues.Length &&
                progress + 0.000001f >= _plan.Cues[_nextCue].At)
            {
                BattleReloadCue cue = _plan.Cues[_nextCue++];
                PlayClip(
                    _clips[(int)cue.Type],
                    VolumeFor(cue.Type),
                    _caliberMm);
                CueCount++;
            }
        }

        private void PlayClip(
            AudioClip clip,
            float volume,
            float caliberMm)
        {
            ReloadVoice voice = AcquireVoice();
            voice.Source.clip = clip;
            voice.BaseVolume = volume;
            voice.Source.volume =
                volume * _settings.CombatVolume * _battleDuck;
            voice.Source.pitch = Mathf.Clamp(
                1.12f - caliberMm / 600f,
                0.82f,
                1.08f);
            voice.ExpiresAt = Time.unscaledTime + clip.length + 0.05f;
            voice.Root.SetActive(true);
            if (Application.isPlaying) voice.Source.Play();
            _active.Add(voice);
        }

        private ReloadVoice AcquireVoice()
        {
            if (_available.Count == 0) ReleaseVoiceAt(0);
            return _available.Dequeue();
        }

        private void ReleaseVoiceAt(int index)
        {
            ReloadVoice voice = _active[index];
            voice.Source.Stop();
            voice.Root.SetActive(false);
            _active.RemoveAt(index);
            _available.Enqueue(voice);
        }

        private void StopVoices()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                ReleaseVoiceAt(i);
        }

        private void ApplyMix()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                ReloadVoice voice = _active[i];
                voice.Source.volume =
                    voice.BaseVolume *
                    _settings.CombatVolume *
                    _battleDuck;
            }
        }

        private static float VolumeFor(BattleReloadCueType type)
        {
            if (type == BattleReloadCueType.Motor) return 0.34f;
            if (type == BattleReloadCueType.Index) return 0.48f;
            if (type == BattleReloadCueType.Ram) return 0.58f;
            if (type == BattleReloadCueType.BreechClose) return 0.62f;
            return 0.46f;
        }

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.AudioChanged -= ApplyMix;
            if (_clips != null)
            {
                for (int i = 0; i < _clips.Length; i++)
                    Release(_clips[i]);
            }
            Release(_readyClip);
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private sealed class ReloadVoice
        {
            public GameObject Root;
            public AudioSource Source;
            public float BaseVolume;
            public float ExpiresAt;
        }
    }
}
