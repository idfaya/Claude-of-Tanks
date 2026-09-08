using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattleCrewVoiceTests
    {
        private readonly List<AudioClip> _clips = new List<AudioClip>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _clips.Count; i++)
                Object.DestroyImmediate(_clips[i]);
            _clips.Clear();
        }

        [Test]
        public void SchedulerBoundsQueueInterruptsAndDropsStaleCalls()
        {
            GameSettings settings = Settings();
            BattleCrewVoice voice = BattleCrewVoice.Create(
                null,
                settings,
                ResolveClip);
            try
            {
                Assert.That(BattleCrewVoiceCatalog.LineCount, Is.EqualTo(36));

                Assert.That(
                    voice.Request(
                        CrewVoiceId.BattleStart,
                        0f,
                        delayS: 0.5f),
                    Is.True);
                voice.TickAt(0.49f);
                Assert.That(voice.PlayCount, Is.Zero);
                voice.TickAt(0.5f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.BattleStart));

                voice.TickAt(2f);
                Assert.That(
                    voice.Request(CrewVoiceId.Firing, 2f),
                    Is.True);
                Assert.That(
                    voice.Request(CrewVoiceId.Fire, 2.1f),
                    Is.True);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.Fire));
                Assert.That(voice.MaximumSimultaneous, Is.EqualTo(1));

                voice.Request(
                    CrewVoiceId.Penetration,
                    4f,
                    delayS: 0.2f);
                voice.Request(
                    CrewVoiceId.EnemyCrit,
                    4f,
                    delayS: 0.2f);
                voice.Request(
                    CrewVoiceId.EnemySpotted,
                    4f,
                    delayS: 1f);
                Assert.That(
                    voice.PendingCount,
                    Is.LessThanOrEqualTo(BattleCrewVoice.QueueLimit));
                Assert.That(
                    voice.HasPending(CrewVoiceId.Penetration),
                    Is.False);
                Assert.That(
                    voice.HasPending(CrewVoiceId.EnemyCrit),
                    Is.True);

                voice.TickAt(6f);
                Assert.That(
                    voice.HasPending(CrewVoiceId.EnemySpotted),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        [Test]
        public void UnityPayloadContainsEveryCatalogVariant()
        {
            BattleCrewVoice voice =
                BattleCrewVoice.Create(null, Settings());
            try
            {
                Assert.That(
                    BattleCrewVoiceCatalog.VariantCount,
                    Is.EqualTo(82));
                Assert.That(
                    voice.LoadedClipCount,
                    Is.EqualTo(BattleCrewVoiceCatalog.VariantCount));
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        [Test]
        public void SchedulerRotatesVariantsAndTracksLiveVoiceMix()
        {
            GameSettings settings = Settings();
            BattleCrewVoice voice = BattleCrewVoice.Create(
                null,
                settings,
                ResolveClip);
            try
            {
                voice.Request(CrewVoiceId.Reloaded, 0f, force: true);
                string first = voice.LastClipName;
                float initialVolume = voice.Source.volume;
                voice.Request(CrewVoiceId.Reloaded, 4f, force: true);

                Assert.That(voice.LastClipName, Is.Not.EqualTo(first));
                settings.SetVoiceVolume(0.25f);
                Assert.That(
                    voice.Source.volume,
                    Is.EqualTo(initialVolume * 0.25f).Within(0.001f));

                voice.ResetAll();
                Assert.That(voice.PendingCount, Is.Zero);
                Assert.That(voice.CurrentLine, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        [Test]
        public void SchedulerEnforcesLineAndGroupCooldowns()
        {
            BattleCrewVoice voice = BattleCrewVoice.Create(
                null,
                Settings(),
                ResolveClip);
            try
            {
                Assert.That(
                    voice.Request(CrewVoiceId.Fire, 0f),
                    Is.True);
                voice.TickAt(2f);
                Assert.That(
                    voice.Request(CrewVoiceId.Fire, 2f),
                    Is.False);

                Assert.That(
                    voice.Request(CrewVoiceId.Penetration, 12f),
                    Is.True);
                voice.TickAt(14f);
                Assert.That(
                    voice.Request(CrewVoiceId.Ricochet, 14f),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        [Test]
        public void DirectorChoosesSpecificDamageKillAndRecoveryCalls()
        {
            BattleCrewVoice voice = BattleCrewVoice.Create(
                null,
                Settings(),
                ResolveClip);
            BattleCrewVoiceDirector director =
                new BattleCrewVoiceDirector(voice);
            TankState player = new TankState(
                "player",
                Team.Alpha,
                TankSpec.Heavy(),
                Float3.Zero,
                0f);
            try
            {
                director.Sync(player, 0f);
                DamageSimulation.DamageModule(
                    player.Combat,
                    "engine",
                    10000f,
                    () => 1f);
                director.Handle(
                    new BattleEvent
                    {
                        Type = BattleEventType.ShellHit,
                        SourceId = "enemy",
                        TargetId = player.Id,
                        Penetrated = true,
                        Value = 200f
                    },
                    player.Id,
                    player,
                    1f);
                voice.TickAt(1.12f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.EngineDamaged));

                director.Handle(
                    new BattleEvent
                    {
                        Type = BattleEventType.ShellHit,
                        SourceId = player.Id,
                        TargetId = "enemy",
                        Penetrated = true,
                        Value = 300f
                    },
                    player.Id,
                    player,
                    3f);
                director.Handle(
                    new BattleEvent
                    {
                        Type = BattleEventType.TankDestroyed,
                        SourceId = player.Id,
                        TargetId = "enemy"
                    },
                    player.Id,
                    player,
                    3.1f);
                voice.TickAt(3.4f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.TargetDestroyed));
                Assert.That(
                    voice.HasPending(CrewVoiceId.Penetration),
                    Is.False);

                DamageSimulation.RepairAllModules(player.Combat);
                director.Sync(player, 5f);
                voice.TickAt(5.2f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.EngineRepaired));
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        [Test]
        public void AwarenessCallsNewContactAndHonorsThreeSecondFuse()
        {
            BattleCrewVoice voice = BattleCrewVoice.Create(
                null,
                Settings(),
                ResolveClip);
            BattleCrewVoiceDirector director =
                new BattleCrewVoiceDirector(voice);
            TankState player = new TankState(
                "player",
                Team.Alpha,
                TankSpec.Medium(),
                Float3.Zero,
                0f);
            TankState enemy = new TankState(
                "enemy",
                Team.Bravo,
                TankSpec.Medium(),
                new Float3(0f, 0f, 100f),
                MathUtil.Pi);
            List<TankState> visible =
                new List<TankState> { player };
            try
            {
                director.SyncAwareness(
                    visible,
                    player.Id,
                    null,
                    false,
                    0f);
                visible.Add(enemy);
                director.SyncAwareness(
                    visible,
                    player.Id,
                    null,
                    true,
                    1f);
                voice.TickAt(1.1f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.EnemySpotted));

                director.SyncAwareness(
                    visible,
                    player.Id,
                    null,
                    true,
                    4f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.SixthSense));
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        [Test]
        public void ResultCallPreemptsObsoleteBattleChatter()
        {
            BattleCrewVoice voice = BattleCrewVoice.Create(
                null,
                Settings(),
                ResolveClip);
            BattleCrewVoiceDirector director =
                new BattleCrewVoiceDirector(voice);
            try
            {
                director.BeginBattle(0f);
                voice.TickAt(0.48f);
                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.BattleStart));

                director.PresentResult("VICTORY", 0.6f);
                voice.TickAt(0.83f);

                Assert.That(
                    voice.CurrentLine,
                    Is.EqualTo(CrewVoiceId.Victory));
                Assert.That(voice.PendingCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(voice.gameObject);
            }
        }

        private AudioClip ResolveClip(string name)
        {
            AudioClip clip = AudioClip.Create(name, 24000, 1, 24000, false);
            _clips.Add(clip);
            return clip;
        }

        private static GameSettings Settings()
        {
            return new GameSettings(
                new MemorySettingsStore(),
                new SettingsTarget());
        }

        private sealed class MemorySettingsStore : ISettingsStore
        {
            private readonly Dictionary<string, int> _ints =
                new Dictionary<string, int>();
            private readonly Dictionary<string, float> _floats =
                new Dictionary<string, float>();

            public int GetInt(string key, int fallback)
            {
                int value;
                return _ints.TryGetValue(key, out value) ? value : fallback;
            }

            public float GetFloat(string key, float fallback)
            {
                float value;
                return _floats.TryGetValue(key, out value) ? value : fallback;
            }

            public void SetInt(string key, int value) { _ints[key] = value; }
            public void SetFloat(string key, float value) { _floats[key] = value; }
            public void Save() { }
        }

        private sealed class SettingsTarget : ISettingsTarget
        {
            public int QualityLevelCount => 3;
            public void ApplyVolume(float value) { }
            public void ApplyQuality(int value) { }
            public void ApplyFullscreen(bool value) { }
        }
    }
}
