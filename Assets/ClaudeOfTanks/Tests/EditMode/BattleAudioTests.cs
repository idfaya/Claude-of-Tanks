using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattleAudioTests
    {
        [Test]
        public void CombatVoicesStayBoundedAndReset()
        {
            BattleAudio audio = BattleAudio.Create(Settings());
            BattleEvent fired = new BattleEvent
            {
                Type = BattleEventType.ShellFired,
                Position = Float3.Zero,
                CaliberMm = 120f
            };

            try
            {
                for (int i = 0; i < 40; i++)
                {
                    audio.Play(fired);
                }

                Assert.That(
                    audio.ActiveOneShotCount,
                    Is.EqualTo(BattleAudio.OneShotVoiceLimit));
                Assert.That(audio.AmbienceActive, Is.True);

                audio.ResetAll();

                Assert.That(audio.ActiveOneShotCount, Is.Zero);
                Assert.That(audio.ActiveEngineCount, Is.Zero);
                Assert.That(audio.AmbienceActive, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(audio.gameObject);
            }
        }

        [Test]
        public void EngineVoicesPreferOwnerThenNearestAndUseLiveMix()
        {
            GameSettings settings = Settings();
            BattleAudio audio = BattleAudio.Create(settings);
            List<TankState> tanks = new List<TankState>();
            for (int i = 0; i < 14; i++)
            {
                tanks.Add(new TankState(
                    "tank-" + i,
                    Team.Alpha,
                    TankSpec.Medium(),
                    new Float3(i * 20f, 0f, 0f),
                    0f));
            }
            tanks[13].Position = new Float3(1400f, 0f, 0f);

            try
            {
                audio.SyncEngines(
                    tanks,
                    "tank-13",
                    Vector3.zero,
                    false);

                Assert.That(
                    audio.ActiveEngineCount,
                    Is.EqualTo(BattleAudio.EngineVoiceLimit));
                Assert.That(audio.HasEngineVoice("tank-13"), Is.True);
                Assert.That(audio.HasEngineVoice("tank-0"), Is.True);
                Assert.That(audio.HasEngineVoice("tank-11"), Is.False);
                AudioSource ownerEngine = audio.transform
                    .Find("Engine-0")
                    .GetComponent<AudioSource>();
                float engineVolume = ownerEngine.volume;
                Assert.That(ownerEngine.spatialBlend, Is.Zero);

                audio.Play(new BattleEvent
                {
                    Type = BattleEventType.ShellFired,
                    Position = Float3.Zero,
                    CaliberMm = 120f
                });
                AudioSource combat = audio.transform
                    .Find("OneShot-0")
                    .GetComponent<AudioSource>();
                float combatVolume = combat.volume;

                settings.SetEngineVolume(0.25f);
                settings.SetCombatVolume(0.4f);
                settings.SetAmbienceVolume(0.5f);

                Assert.That(audio.EngineGain, Is.EqualTo(0.25f).Within(0.001f));
                Assert.That(audio.CombatGain, Is.EqualTo(0.4f).Within(0.001f));
                Assert.That(audio.AmbienceGain, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(
                    ownerEngine.volume,
                    Is.EqualTo(engineVolume * 0.25f).Within(0.001f));
                Assert.That(
                    combat.volume,
                    Is.EqualTo(combatVolume * 0.4f).Within(0.001f));
                Assert.That(
                    audio.transform.Find("Ambience")
                        .GetComponent<AudioSource>().volume,
                    Is.EqualTo(0.08f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(audio.gameObject);
            }
        }

        [Test]
        public void PlayerReloadCuesAreBoundedMixedAndReset()
        {
            GameSettings settings = Settings();
            BattleAudio audio = BattleAudio.Create(settings);
            TankState tank = new TankState(
                "player",
                Team.Alpha,
                TankSpec.Heavy(),
                Float3.Zero,
                0f);
            List<TankState> tanks = new List<TankState> { tank };

            try
            {
                DamageSimulation.StartPostShotReload(
                    tank.Combat,
                    tank.DamageSpec);
                tank.ReloadRemainingS = tank.Combat.Reload.RemainingS;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);

                tank.Combat.Reload.RemainingS =
                    tank.Combat.Reload.TotalS * 0.5f;
                tank.ReloadRemainingS = tank.Combat.Reload.RemainingS;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);

                Assert.That(audio.ReloadCueCount, Is.EqualTo(3));
                Assert.That(
                    audio.ActiveReloadVoiceCount,
                    Is.LessThanOrEqualTo(BattleReloadAudio.VoiceLimit));
                AudioSource cue = audio.transform
                    .Find("ReloadAudio/Reload-0")
                    .GetComponent<AudioSource>();
                float volume = cue.volume;

                audio.SetKillcamDucking(true);
                Assert.That(
                    cue.volume,
                    Is.EqualTo(volume * 0.35f).Within(0.001f));
                settings.SetCombatVolume(0.4f);

                Assert.That(
                    cue.volume,
                    Is.EqualTo(volume * 0.35f * 0.4f).Within(0.001f));
                audio.SetKillcamDucking(false);
                audio.SetPauseDucking(true);
                Assert.That(
                    cue.volume,
                    Is.EqualTo(volume * 0.04f * 0.4f).Within(0.001f));

                tank.Combat.Reload.RemainingS = 0f;
                tank.Combat.Reload.Kind = DamageReloadKind.Ready;
                tank.ReloadRemainingS = 0f;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);

                Assert.That(audio.ReloadReadyCount, Is.EqualTo(1));

                audio.ResetAll();

                Assert.That(audio.ReloadCueCount, Is.Zero);
                Assert.That(audio.ReloadReadyCount, Is.Zero);
                Assert.That(audio.ActiveReloadVoiceCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(audio.gameObject);
            }
        }

        [Test]
        public void NetworkReloadStateInfersMagazineWithoutCombatInternals()
        {
            BattleAudio audio = BattleAudio.Create(Settings());
            TankSpec spec = TankSpec.Medium();
            spec.MagazineSize = 4;
            spec.MagazineReloadS = 18f;
            spec.IntraClipS = 2.5f;
            TankState tank = new TankState(
                "player",
                Team.Alpha,
                spec,
                Float3.Zero,
                0f);
            List<TankState> tanks = new List<TankState> { tank };

            try
            {
                tank.ReloadRemainingS = 17.4f;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);
                Assert.That(audio.ReloadCueCount, Is.EqualTo(1));

                tank.ReloadRemainingS = 8f;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);
                Assert.That(audio.ReloadCueCount, Is.EqualTo(3));

                tank.ReloadRemainingS = 1f;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);
                Assert.That(
                    audio.ReloadProfile,
                    Is.EqualTo(BattleReloadProfile.Magazine));
                Assert.That(audio.ReloadCueCount, Is.EqualTo(5));

                tank.ReloadRemainingS = 0f;
                audio.SyncEngines(tanks, tank.Id, Vector3.zero, false);
                Assert.That(audio.ReloadReadyCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(audio.gameObject);
            }
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
