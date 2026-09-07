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
    }
}
