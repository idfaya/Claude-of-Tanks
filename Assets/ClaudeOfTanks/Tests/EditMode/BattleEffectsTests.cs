using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattleEffectsTests
    {
        [Test]
        public void EffectsLightsAndVehicleDecalsStayBoundedAndReset()
        {
            BattleEffects effects = BattleEffects.Create();
            GameObject target = new GameObject("Target");
            BattleEvent hit = new BattleEvent
            {
                Type = BattleEventType.ShellHit,
                TargetId = "target",
                Position = new Float3(0f, 1f, 0f),
                Direction = new Float3(0f, 0f, 1f),
                Normal = new Float3(0f, 0f, -1f),
                CaliberMm = 120f,
                ShellType = "APFSDS",
                Penetrated = true,
                Value = 400f
            };

            try
            {
                Assert.That(
                    effects.GetComponentsInChildren<AudioSource>(true),
                    Is.Empty);
                for (int i = 0; i < 60; i++)
                {
                    hit.Position = new Float3(i * 0.01f, 1f, 0f);
                    effects.Play(hit, target.transform);
                }

                Assert.That(effects.ActiveEffectCount, Is.EqualTo(BattleEffects.EffectPoolSize));
                Assert.That(effects.ActiveDecalCount, Is.EqualTo(BattleEffects.DecalPoolSize));
                Assert.That(effects.ActiveLightCount, Is.LessThanOrEqualTo(BattleEffects.LightPoolSize));

                effects.ResetAll();

                Assert.That(effects.ActiveEffectCount, Is.Zero);
                Assert.That(effects.ActiveDecalCount, Is.Zero);
                Assert.That(effects.ActiveLightCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(effects.gameObject);
            }
        }

        [Test]
        public void ReducedMotionKeepsFeedbackButSuppressesDynamicFlashes()
        {
            MemorySettingsStore store = new MemorySettingsStore();
            GameSettings settings = new GameSettings(
                store,
                new SettingsTarget());
            settings.SetReducedMotion(true);
            BattleEffects effects = BattleEffects.Create(settings);
            BattleEvent fired = new BattleEvent
            {
                Type = BattleEventType.ShellFired,
                Position = new Float3(0f, 1f, 0f),
                Direction = new Float3(0f, 0f, 1f),
                Normal = new Float3(0f, 1f, 0f),
                CaliberMm = 120f
            };

            try
            {
                effects.Play(fired);

                Assert.That(effects.ActiveEffectCount, Is.EqualTo(1));
                Assert.That(effects.ActiveLightCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(effects.gameObject);
            }
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
