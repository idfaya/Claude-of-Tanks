using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleEffects : MonoBehaviour
    {
        public const int EffectPoolSize =
            BattleOneShotEffects.PoolSize;
        public const int DecalPoolSize =
            BattleImpactDecals.PoolSize;
        public const int LightPoolSize =
            BattleOneShotEffects.LightPoolSize;

        private Material _particleMaterial;
        private Texture2D _particleTexture;
        private BattleImpactDecals _impactDecals;
        private BattleOneShotEffects _oneShots;
        private BattlePersistentEffects _persistent;
        private GameSettings _settings;

        public int ActiveEffectCount => _oneShots.ActiveCount;
        public int ActiveDecalCount =>
            _impactDecals.ActiveCount;
        public int ActivePersistentTankCount =>
            _persistent != null ? _persistent.ActiveTankCount : 0;
        public int ActiveTrackPrintCount =>
            _persistent != null ? _persistent.ActiveTrackPrintCount : 0;
        public int ActivePersistentSystemCount =>
            _persistent != null ? _persistent.ActiveSystemCount : 0;

        public int ActiveLightCount
        {
            get
            {
                return _oneShots.ActiveLightCount;
            }
        }

        public static BattleEffects Create(GameSettings settings = null)
        {
            GameObject root = new GameObject("BattleEffects");
            BattleEffects effects = root.AddComponent<BattleEffects>();
            effects._settings = settings ?? GameSettings.Current;
            effects.Initialize();
            return effects;
        }

        public void Play(BattleEvent battleEvent, Transform target = null)
        {
            _oneShots.Play(battleEvent);
            if (battleEvent.Type == BattleEventType.ShellHit && target != null)
            {
                _impactDecals.StampImpact(
                    battleEvent,
                    target);
            }
            if (battleEvent.Type == BattleEventType.TankDestroyed)
            {
                _impactDecals.ClearTarget(target);
                _impactDecals.StampGroundScorch(battleEvent);
            }
        }

        public void SyncPersistent(IList<TankState> tanks)
        {
            _persistent.Sync(tanks);
        }

        public void ResetAll()
        {
            _oneShots.ResetAll();

            _impactDecals.ResetAll();
            _persistent.ResetAll();
        }

        private void Initialize()
        {
            _particleTexture = BuildParticleTexture();
            Shader particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended") ??
                Shader.Find("Particles/Standard Unlit") ??
                Shader.Find("Standard");
            _particleMaterial = new Material(particleShader)
            {
                color = Color.white,
                mainTexture = _particleTexture
            };
            _impactDecals =
                BattleImpactDecals.Create(transform);
            _oneShots = BattleOneShotEffects.Create(
                transform,
                _particleMaterial,
                _settings);
            _persistent = BattlePersistentEffects.Create(
                transform,
                _particleMaterial,
                _settings);

        }

        private static Texture2D BuildParticleTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "CombatParticleTexture",
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f) / size * 2f - 1f;
                    float ny = (y + 0.5f) / size * 2f - 1f;
                    float radius = Mathf.Sqrt(nx * nx + ny * ny);
                    float irregular = Mathf.Sin(Mathf.Atan2(ny, nx) * 7f) * 0.06f;
                    byte alpha = (byte)Mathf.RoundToInt(
                        Mathf.Clamp01((1f + irregular - radius) * 5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private void OnDestroy()
        {
            Release(_particleMaterial);
            Release(_particleTexture);
        }

        private static void Release(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

    }
}
