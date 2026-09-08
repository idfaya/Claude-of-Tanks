using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleCrewVoice : MonoBehaviour
    {
        public const int QueueLimit = 2;
        public const float GlobalGapS = 0.22f;
        private const float BaseVolume = 0.92f;

        private readonly Dictionary<string, AudioClip> _clips =
            new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        private readonly Dictionary<CrewVoiceId, float> _lastPlay =
            new Dictionary<CrewVoiceId, float>();
        private readonly Dictionary<CrewVoiceGroup, GroupPlay> _lastGroup =
            new Dictionary<CrewVoiceGroup, GroupPlay>();
        private readonly Dictionary<CrewVoiceId, string> _lastVariant =
            new Dictionary<CrewVoiceId, string>();
        private readonly List<VoiceRequest> _queue =
            new List<VoiceRequest>(QueueLimit);
        private GameSettings _settings;
        private float _currentEnd = -1f;
        private int _currentPriority = -1;
        private CrewVoiceGroup? _currentGroup;
        private uint _variantState = 0xc0ffeeu;

        public AudioSource Source { get; private set; }
        public CrewVoiceId? CurrentLine { get; private set; }
        public int PendingCount => _queue.Count;
        public int PlayCount { get; private set; }
        public int LoadedClipCount => _clips.Count;
        public int MaximumSimultaneous { get; private set; }
        public string LastClipName { get; private set; }

        public static BattleCrewVoice Create(
            Transform parent,
            GameSettings settings,
            Func<string, AudioClip> resolver = null)
        {
            GameObject root = new GameObject("CrewVoice");
            if (parent != null) root.transform.SetParent(parent, false);
            BattleCrewVoice voice =
                root.AddComponent<BattleCrewVoice>();
            voice.Initialize(
                settings ?? throw new ArgumentNullException(nameof(settings)),
                resolver ?? LoadClip);
            return voice;
        }

        public bool Request(
            CrewVoiceId id,
            float now,
            float delayS = 0f,
            float staleS = -1f,
            bool force = false)
        {
            BattleCrewVoiceLine line = BattleCrewVoiceCatalog.Get(id);
            if (!HasPlayableClip(line)) return false;
            if (force)
            {
                _queue.Clear();
                StopCurrent(now);
                return PlayNow(id, now);
            }
            if (CooldownActive(line, now)) return false;
            float delay = Mathf.Max(0f, delayS);
            float busyUntil = _currentEnd + GlobalGapS;
            if (_currentGroup == line.Group &&
                CurrentLine.HasValue &&
                now < _currentEnd &&
                line.Priority <= _currentPriority)
            {
                return false;
            }
            if (delay <= 0f && now >= busyUntil)
                return PlayNow(id, now);
            if (delay <= 0f && CanInterrupt(line.Priority, now))
            {
                StopCurrent(now);
                RemoveLowerPriority(line.Priority);
                return PlayNow(id, now);
            }
            if (line.Priority == 0 && now < busyUntil)
                return false;
            return Enqueue(
                line,
                now,
                delay,
                staleS >= 0f ? staleS : line.StaleS);
        }

        public void TickAt(float now)
        {
            if (CurrentLine.HasValue && now >= _currentEnd)
                ClearCurrent();
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if (now > _queue[i].ExpiresAt)
                    _queue.RemoveAt(i);
            }
            int best = BestReady(now);
            if (best < 0) return;
            if (now < _currentEnd + GlobalGapS)
            {
                if (!CanInterrupt(_queue[best].Priority, now)) return;
                StopCurrent(now);
            }
            CrewVoiceId id = _queue[best].Id;
            _queue.RemoveAt(best);
            BattleCrewVoiceLine line = BattleCrewVoiceCatalog.Get(id);
            if (!CooldownActive(line, now)) PlayNow(id, now);
        }

        public bool HasPending(CrewVoiceId id)
        {
            for (int i = 0; i < _queue.Count; i++)
                if (_queue[i].Id == id) return true;
            return false;
        }

        public void CancelPending(
            CrewVoiceGroup? keepGroup = null,
            bool stopObsoleteActive = false,
            float now = float.NaN)
        {
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if (!keepGroup.HasValue ||
                    _queue[i].Group != keepGroup.Value)
                {
                    _queue.RemoveAt(i);
                }
            }
            if (stopObsoleteActive &&
                CurrentLine.HasValue &&
                (!keepGroup.HasValue ||
                 _currentGroup != keepGroup.Value))
            {
                StopCurrent(float.IsNaN(now) ? Time.unscaledTime : now);
            }
        }

        public void ResetAll()
        {
            _queue.Clear();
            Source.Stop();
            CurrentLine = null;
            _currentGroup = null;
            _currentPriority = -1;
            _currentEnd = -1f;
            _lastPlay.Clear();
            _lastGroup.Clear();
            _lastVariant.Clear();
            PlayCount = 0;
            MaximumSimultaneous = 0;
            LastClipName = null;
        }

        public void Silence(float now = float.NaN)
        {
            _queue.Clear();
            StopCurrent(float.IsNaN(now) ? Time.unscaledTime : now);
        }

        private void Update()
        {
            TickAt(Time.unscaledTime);
        }

        private void Initialize(
            GameSettings settings,
            Func<string, AudioClip> resolver)
        {
            _settings = settings;
            Source = gameObject.AddComponent<AudioSource>();
            Source.playOnAwake = false;
            Source.loop = false;
            Source.spatialBlend = 0f;
            Source.dopplerLevel = 0f;
            for (int i = 0; i < BattleCrewVoiceCatalog.LineCount; i++)
            {
                BattleCrewVoiceLine line =
                    BattleCrewVoiceCatalog.Get((CrewVoiceId)i);
                for (int fileIndex = 0;
                    fileIndex < line.Files.Length;
                    fileIndex++)
                {
                    string file = line.Files[fileIndex];
                    if (_clips.ContainsKey(file)) continue;
                    AudioClip clip = resolver(file);
                    if (clip != null) _clips.Add(file, clip);
                }
            }
            if (_clips.Count != BattleCrewVoiceCatalog.VariantCount)
            {
                Debug.LogWarning(
                    "Crew voice payload incomplete: loaded " +
                    _clips.Count + " of " +
                    BattleCrewVoiceCatalog.VariantCount + " clips.");
            }
            _settings.AudioChanged += ApplyMix;
            ApplyMix();
        }

        private bool PlayNow(CrewVoiceId id, float now)
        {
            BattleCrewVoiceLine line = BattleCrewVoiceCatalog.Get(id);
            AudioClip clip = SelectClip(line);
            if (clip == null) return false;
            Source.Stop();
            Source.clip = clip;
            Source.pitch = 0.985f + Next01() * 0.03f;
            ApplyMix();
            if (Application.isPlaying) Source.Play();
            CurrentLine = id;
            _currentPriority = line.Priority;
            _currentGroup = line.Group;
            _currentEnd = now + clip.length / Source.pitch;
            _lastPlay[id] = now;
            _lastGroup[line.Group] =
                new GroupPlay(now, line.Priority);
            _lastVariant[id] = clip.name;
            LastClipName = clip.name;
            PlayCount++;
            MaximumSimultaneous = Math.Max(MaximumSimultaneous, 1);
            return true;
        }

        private AudioClip SelectClip(BattleCrewVoiceLine line)
        {
            int start = (int)(Next01() * line.Files.Length);
            string previous;
            _lastVariant.TryGetValue(line.Id, out previous);
            for (int offset = 0; offset < line.Files.Length; offset++)
            {
                string name = line.Files[(start + offset) % line.Files.Length];
                AudioClip clip;
                if (!_clips.TryGetValue(name, out clip)) continue;
                if (line.Files.Length > 1 && clip.name == previous) continue;
                return clip;
            }
            for (int i = 0; i < line.Files.Length; i++)
            {
                AudioClip clip;
                if (_clips.TryGetValue(line.Files[i], out clip)) return clip;
            }
            return null;
        }

        private bool Enqueue(
            BattleCrewVoiceLine line,
            float now,
            float delayS,
            float staleS)
        {
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if (_queue[i].Group != line.Group) continue;
                if (_queue[i].Priority > line.Priority) return false;
                _queue.RemoveAt(i);
            }
            if (line.Priority >= 3) RemoveLowerPriority(line.Priority);
            if (_queue.Count >= QueueLimit)
            {
                int worst = WorstRequest();
                if (_queue[worst].Priority >= line.Priority) return false;
                _queue.RemoveAt(worst);
            }
            float readyAt = now + delayS;
            _queue.Add(new VoiceRequest(
                line.Id,
                line.Priority,
                line.Group,
                now,
                readyAt,
                readyAt + staleS));
            return true;
        }

        private int BestReady(float now)
        {
            int best = -1;
            for (int i = 0; i < _queue.Count; i++)
            {
                VoiceRequest request = _queue[i];
                if (request.ReadyAt > now) continue;
                if (best < 0 ||
                    request.Priority > _queue[best].Priority ||
                    (request.Priority == _queue[best].Priority &&
                     request.RequestedAt < _queue[best].RequestedAt))
                {
                    best = i;
                }
            }
            return best;
        }

        private int WorstRequest()
        {
            int worst = 0;
            for (int i = 1; i < _queue.Count; i++)
            {
                if (_queue[i].Priority < _queue[worst].Priority ||
                    (_queue[i].Priority == _queue[worst].Priority &&
                     _queue[i].RequestedAt < _queue[worst].RequestedAt))
                {
                    worst = i;
                }
            }
            return worst;
        }

        private bool CooldownActive(
            BattleCrewVoiceLine line,
            float now)
        {
            float last;
            if (_lastPlay.TryGetValue(line.Id, out last) &&
                now - last < line.CooldownS)
            {
                return true;
            }
            GroupPlay group;
            return line.GroupCooldownS > 0f &&
                _lastGroup.TryGetValue(line.Group, out group) &&
                group.Priority >= line.Priority &&
                now - group.Time < line.GroupCooldownS;
        }

        private bool CanInterrupt(int priority, float now)
        {
            return CurrentLine.HasValue &&
                _currentEnd - now > 0.12f &&
                ((priority >= 4 && _currentPriority < 4) ||
                 (priority >= 3 && _currentPriority <= 1));
        }

        private void RemoveLowerPriority(int priority)
        {
            for (int i = _queue.Count - 1; i >= 0; i--)
                if (_queue[i].Priority < priority) _queue.RemoveAt(i);
        }

        private void StopCurrent(float now)
        {
            Source.Stop();
            _currentEnd = now;
            ClearCurrent();
        }

        private void ClearCurrent()
        {
            CurrentLine = null;
            _currentPriority = -1;
            _currentGroup = null;
        }

        private void ApplyMix()
        {
            if (Source != null)
                Source.volume = BaseVolume * _settings.VoiceVolume;
        }

        private bool HasPlayableClip(BattleCrewVoiceLine line)
        {
            for (int i = 0; i < line.Files.Length; i++)
                if (_clips.ContainsKey(line.Files[i])) return true;
            return false;
        }

        private float Next01()
        {
            _variantState = _variantState * 1664525u + 1013904223u;
            return (_variantState >> 8) / 16777216f;
        }

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.AudioChanged -= ApplyMix;
        }

        private static AudioClip LoadClip(string name)
        {
            return Resources.Load<AudioClip>("Audio/Voice/" + name);
        }

        private struct GroupPlay
        {
            public readonly float Time;
            public readonly int Priority;

            public GroupPlay(float time, int priority)
            {
                Time = time;
                Priority = priority;
            }
        }

        private struct VoiceRequest
        {
            public readonly CrewVoiceId Id;
            public readonly int Priority;
            public readonly CrewVoiceGroup Group;
            public readonly float RequestedAt;
            public readonly float ReadyAt;
            public readonly float ExpiresAt;

            public VoiceRequest(
                CrewVoiceId id,
                int priority,
                CrewVoiceGroup group,
                float requestedAt,
                float readyAt,
                float expiresAt)
            {
                Id = id;
                Priority = priority;
                Group = group;
                RequestedAt = requestedAt;
                ReadyAt = readyAt;
                ExpiresAt = expiresAt;
            }
        }
    }
}
