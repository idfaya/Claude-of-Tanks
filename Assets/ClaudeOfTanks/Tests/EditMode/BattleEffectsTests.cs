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
        public void PersistentTankEffectsAndTrackPrintsStayBounded()
        {
            BattleEffects effects = BattleEffects.Create();
            List<TankState> tanks = new List<TankState>();
            for (int i = 0;
                i < BattlePersistentEffects.TankPoolSize + 4;
                i++)
            {
                TankState tank = Tank("tank-" + i, i);
                tank.SpeedMps = 12f;
                if (i == 0) tank.Combat.Fire.Burning = true;
                if (i == 1)
                {
                    tank.Destroyed = true;
                    tank.Combat.Destroyed = true;
                }
                tanks.Add(tank);
            }

            try
            {
                effects.SyncPersistent(tanks);
                Assert.That(
                    effects.ActivePersistentTankCount,
                    Is.EqualTo(BattlePersistentEffects.TankPoolSize));
                Assert.That(
                    effects.ActivePersistentSystemCount,
                    Is.GreaterThan(0).And.LessThanOrEqualTo(
                        BattlePersistentEffects.TankPoolSize * 4));

                for (int i = 0;
                    i < BattlePersistentEffects.TankPoolSize;
                    i++)
                {
                    tanks[i].Position = new Float3(i, 0f, 2f);
                }
                effects.SyncPersistent(tanks);
                Assert.That(
                    effects.ActiveTrackPrintCount,
                    Is.EqualTo(BattlePersistentEffects.TankPoolSize - 1));
                Assert.That(
                    effects.ActiveTrackPrintCount,
                    Is.LessThanOrEqualTo(
                        BattlePersistentEffects.TrackPrintLimit));

                effects.SyncPersistent(new[] { tanks[0] });
                Assert.That(effects.ActivePersistentTankCount, Is.EqualTo(1));
                Assert.That(effects.ActiveTrackPrintCount, Is.EqualTo(1));

                effects.ResetAll();
                Assert.That(effects.ActivePersistentTankCount, Is.Zero);
                Assert.That(effects.ActivePersistentSystemCount, Is.Zero);
                Assert.That(effects.ActiveTrackPrintCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(effects.gameObject);
            }
        }

        [Test]
        public void TankDestructionStampsGroundScorch()
        {
            BattleEffects effects = BattleEffects.Create();
            BattleEvent destroyed = new BattleEvent
            {
                Type = BattleEventType.TankDestroyed,
                Position = new Float3(4f, 0f, 7f),
                Direction = new Float3(0f, 0f, 1f),
                Normal = new Float3(0f, 1f, 0f),
                CaliberMm = 120f
            };

            try
            {
                effects.Play(destroyed);
                Assert.That(effects.ActiveDecalCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(effects.gameObject);
            }
        }

        [Test]
        public void OneShotProfilesLayerImpactsAndIgnoreNonVisualEvents()
        {
            BattleEffects effects = BattleEffects.Create();
            BattleEvent ricochet = Hit(
                "APFSDS",
                false);
            BattleEvent penetration = Hit(
                "APFSDS",
                true);
            BattleEvent highExplosive = Hit(
                "HE",
                false);
            BattleOneShotProfile ricochetProfile =
                BattleOneShotProfile.Resolve(ricochet, false);
            BattleOneShotProfile penetrationProfile =
                BattleOneShotProfile.Resolve(penetration, false);
            BattleOneShotProfile heProfile =
                BattleOneShotProfile.Resolve(highExplosive, false);

            try
            {
                Assert.That(
                    penetrationProfile.CoreCount,
                    Is.GreaterThan(ricochetProfile.CoreCount));
                Assert.That(
                    penetrationProfile.CloudCount,
                    Is.GreaterThan(ricochetProfile.CloudCount));
                Assert.That(
                    ricochetProfile.SparkSpeed,
                    Is.GreaterThan(penetrationProfile.SparkSpeed));
                Assert.That(
                    heProfile.CloudCount,
                    Is.GreaterThan(penetrationProfile.CloudCount));

                effects.Play(new BattleEvent
                {
                    Type = BattleEventType.ConsumableUsed
                });
                Assert.That(effects.ActiveEffectCount, Is.Zero);

                effects.Play(penetration);
                Assert.That(effects.ActiveEffectCount, Is.EqualTo(1));
                Transform oneShots =
                    effects.transform.Find("OneShotEffects");
                Assert.That(oneShots, Is.Not.Null);
                Assert.That(
                    oneShots.GetComponentsInChildren<
                        ParticleSystem>(true),
                    Has.Length.EqualTo(
                        BattleOneShotEffects.PoolSize * 3));
            }
            finally
            {
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

        private static TankState Tank(string id, int index)
        {
            TankSpec spec = TankSpec.Medium();
            spec.Id = "medium-" + index;
            return new TankState(
                id,
                index % 2 == 0 ? Team.Alpha : Team.Bravo,
                spec,
                new Float3(index, 0f, 0f),
                0f);
        }

        private static BattleEvent Hit(
            string shellType,
            bool penetrated)
        {
            return new BattleEvent
            {
                Type = BattleEventType.ShellHit,
                Position = new Float3(0f, 1f, 0f),
                Direction = new Float3(0f, 0f, 1f),
                Normal = new Float3(0f, 0f, -1f),
                ShellType = shellType,
                CaliberMm = 120f,
                Penetrated = penetrated
            };
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
