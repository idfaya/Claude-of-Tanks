using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleOneShotEffects : MonoBehaviour
    {
        public const int PoolSize = 24;
        public const int LightPoolSize = 2;

        private readonly Queue<EffectNode> _available =
            new Queue<EffectNode>();
        private readonly List<EffectNode> _active =
            new List<EffectNode>();
        private readonly List<EffectNode> _nodes =
            new List<EffectNode>();
        private readonly LightNode[] _lights =
            new LightNode[LightPoolSize];
        private GameSettings _settings;
        private Material _material;
        private int _lightCursor;
        private uint _noise = 0x91e10da5u;

        public int ActiveCount => _active.Count;

        public int ActiveLightCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _lights.Length; i++)
                    if (_lights[i].Root.activeSelf) count++;
                return count;
            }
        }

        public static BattleOneShotEffects Create(
            Transform parent,
            Material material,
            GameSettings settings)
        {
            GameObject root = new GameObject("OneShotEffects");
            root.transform.SetParent(parent, false);
            BattleOneShotEffects effects =
                root.AddComponent<BattleOneShotEffects>();
            effects.Initialize(
                material,
                settings ?? throw new ArgumentNullException(
                    nameof(settings)));
            return effects;
        }

        public bool Play(BattleEvent battleEvent)
        {
            if (!IsVisualEvent(battleEvent.Type)) return false;
            EffectNode node = Acquire();
            node.Root.transform.position =
                battleEvent.Position.ToUnity();
            node.Root.SetActive(true);
            BattleOneShotProfile profile =
                BattleOneShotProfile.Resolve(
                    battleEvent,
                    _settings.ReducedMotion);
            EmitCore(node.Core, battleEvent, profile);
            EmitSparks(node.Sparks, battleEvent, profile);
            EmitCloud(node.Cloud, battleEvent, profile);
            node.ExpiresAt =
                Time.unscaledTime + profile.LifetimeS;
            _active.Add(node);
            TriggerLight(battleEvent, profile);
            return true;
        }

        public void ResetAll()
        {
            _active.Clear();
            _available.Clear();
            for (int i = 0; i < _nodes.Count; i++)
            {
                EffectNode node = _nodes[i];
                StopAndClear(node.Core);
                StopAndClear(node.Sparks);
                StopAndClear(node.Cloud);
                node.Root.SetActive(false);
                _available.Enqueue(node);
            }
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].Root.SetActive(false);
                _lights[i].Light.intensity = 0f;
            }
        }

        private void Initialize(
            Material material,
            GameSettings settings)
        {
            _material = material;
            _settings = settings;
            for (int i = 0; i < PoolSize; i++)
            {
                EffectNode node = CreateNode(i);
                _nodes.Add(node);
                _available.Enqueue(node);
            }
            for (int i = 0; i < LightPoolSize; i++)
            {
                GameObject root =
                    new GameObject("CombatLight-" + i);
                root.transform.SetParent(transform, false);
                Light light = root.AddComponent<Light>();
                light.type = LightType.Point;
                light.shadows = LightShadows.None;
                root.SetActive(false);
                _lights[i] =
                    new LightNode { Root = root, Light = light };
            }
        }

        private EffectNode CreateNode(int index)
        {
            GameObject root =
                new GameObject("OneShot-" + index);
            root.transform.SetParent(transform, false);
            EffectNode node = new EffectNode
            {
                Root = root,
                Core = CreateSystem(
                    "FlashCore",
                    root.transform,
                    ParticleSystemRenderMode.Billboard,
                    16),
                Sparks = CreateSystem(
                    "Sparks",
                    root.transform,
                    ParticleSystemRenderMode.Stretch,
                    48),
                Cloud = CreateSystem(
                    "Cloud",
                    root.transform,
                    ParticleSystemRenderMode.Billboard,
                    48)
            };
            Seed(node.Core, index, 0);
            Seed(node.Sparks, index, 1);
            Seed(node.Cloud, index, 2);
            root.SetActive(false);
            return node;
        }

        private ParticleSystem CreateSystem(
            string name,
            Transform parent,
            ParticleSystemRenderMode renderMode,
            int maximum)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            ParticleSystem particles =
                root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;
            main.maxParticles = maximum;
            main.gravityModifier =
                name == "Sparks" ? 0.72f : 0f;
            ParticleSystem.EmissionModule emission =
                particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;
            ParticleSystem.ColorOverLifetimeModule fade =
                particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = FadeGradient();
            ParticleSystem.SizeOverLifetimeModule size =
                particles.sizeOverLifetime;
            size.enabled = true;
            float endScale = name == "Cloud"
                ? 2.4f
                : name == "FlashCore" ? 0.45f : 0.2f;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                AnimationCurve.Linear(0f, 1f, 1f, endScale));
            ParticleSystemRenderer renderer =
                root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = renderMode;
            renderer.sharedMaterial = _material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (renderMode == ParticleSystemRenderMode.Stretch)
            {
                renderer.velocityScale = 0.12f;
                renderer.lengthScale = 2.6f;
            }
            return particles;
        }

        private static ParticleSystem.MinMaxGradient FadeGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.72f, 0.62f),
                    new GradientAlphaKey(0f, 1f)
                });
            return new ParticleSystem.MinMaxGradient(gradient);
        }

        private void EmitCore(
            ParticleSystem particles,
            BattleEvent battleEvent,
            BattleOneShotProfile profile)
        {
            particles.Clear(true);
            Vector3 direction = Direction(battleEvent);
            for (int i = 0; i < profile.CoreCount; i++)
            {
                ParticleSystem.EmitParams emit =
                    new ParticleSystem.EmitParams
                    {
                        position =
                            battleEvent.Position.ToUnity(),
                        velocity =
                            direction * Mathf.Lerp(
                                0.3f,
                                1.8f,
                                Next01()),
                        startColor = profile.CoreColor,
                        startSize =
                            profile.CoreSize *
                            Mathf.Lerp(0.75f, 1.25f, Next01()),
                        startLifetime =
                            Mathf.Lerp(0.06f, 0.18f, Next01())
                    };
                particles.Emit(emit, 1);
            }
            if (profile.CoreCount > 0) particles.Play();
        }

        private void EmitSparks(
            ParticleSystem particles,
            BattleEvent battleEvent,
            BattleOneShotProfile profile)
        {
            particles.Clear(true);
            Vector3 direction = OutwardDirection(battleEvent);
            for (int i = 0; i < profile.SparkCount; i++)
            {
                Vector3 spread = RandomHemisphere(direction);
                ParticleSystem.EmitParams emit =
                    new ParticleSystem.EmitParams
                    {
                        position =
                            battleEvent.Position.ToUnity(),
                        velocity =
                            Vector3.Lerp(
                                direction,
                                spread,
                                profile.SparkSpread)
                            .normalized *
                            Mathf.Lerp(
                                profile.SparkSpeed * 0.55f,
                                profile.SparkSpeed,
                                Next01()),
                        startColor = profile.SparkColor,
                        startSize =
                            Mathf.Lerp(0.025f, 0.07f, Next01()),
                        startLifetime =
                            Mathf.Lerp(0.18f, 0.62f, Next01())
                    };
                particles.Emit(emit, 1);
            }
            if (profile.SparkCount > 0) particles.Play();
        }

        private void EmitCloud(
            ParticleSystem particles,
            BattleEvent battleEvent,
            BattleOneShotProfile profile)
        {
            particles.Clear(true);
            Vector3 outward = OutwardDirection(battleEvent);
            for (int i = 0; i < profile.CloudCount; i++)
            {
                Vector3 spread = RandomHemisphere(outward);
                ParticleSystem.EmitParams emit =
                    new ParticleSystem.EmitParams
                    {
                        position =
                            battleEvent.Position.ToUnity(),
                        velocity =
                            Vector3.Lerp(outward, spread, 0.72f)
                            .normalized *
                            Mathf.Lerp(0.5f, 3.5f, Next01()) +
                            Vector3.up *
                            Mathf.Lerp(0.4f, 2.2f, Next01()),
                        startColor = profile.CloudColor,
                        startSize =
                            profile.CloudSize *
                            Mathf.Lerp(0.65f, 1.4f, Next01()),
                        startLifetime =
                            Mathf.Lerp(
                                profile.LifetimeS * 0.45f,
                                profile.LifetimeS,
                                Next01())
                    };
                particles.Emit(emit, 1);
            }
            if (profile.CloudCount > 0) particles.Play();
        }

        private void TriggerLight(
            BattleEvent battleEvent,
            BattleOneShotProfile profile)
        {
            if (_settings.ReducedMotion ||
                profile.CoreCount == 0)
            {
                return;
            }
            LightNode node = _lights[_lightCursor];
            _lightCursor =
                (_lightCursor + 1) % _lights.Length;
            node.Root.transform.position =
                battleEvent.Position.ToUnity();
            node.Root.SetActive(true);
            bool destroyed =
                battleEvent.Type == BattleEventType.TankDestroyed ||
                battleEvent.Type ==
                    BattleEventType.StructureDestroyed;
            node.Light.color = profile.SparkColor;
            node.Light.range = destroyed ? 18f : 8f;
            node.Duration = destroyed ? 0.65f : 0.12f;
            node.Peak = destroyed ? 7f : 3.2f;
            node.ExpiresAt =
                Time.unscaledTime + node.Duration;
            node.Light.intensity = node.Peak;
        }

        private EffectNode Acquire()
        {
            if (_available.Count == 0)
            {
                EffectNode oldest = _active[0];
                _active.RemoveAt(0);
                StopAndClear(oldest.Core);
                StopAndClear(oldest.Sparks);
                StopAndClear(oldest.Cloud);
                oldest.Root.SetActive(false);
                _available.Enqueue(oldest);
            }
            return _available.Dequeue();
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                EffectNode node = _active[i];
                if (node.ExpiresAt > now) continue;
                StopAndClear(node.Core);
                StopAndClear(node.Sparks);
                StopAndClear(node.Cloud);
                node.Root.SetActive(false);
                _active.RemoveAt(i);
                _available.Enqueue(node);
            }
            for (int i = 0; i < _lights.Length; i++)
            {
                LightNode node = _lights[i];
                if (!node.Root.activeSelf) continue;
                float remaining = node.ExpiresAt - now;
                if (remaining <= 0f)
                {
                    node.Light.intensity = 0f;
                    node.Root.SetActive(false);
                    continue;
                }
                node.Light.intensity =
                    node.Peak *
                    Mathf.Clamp01(
                        remaining / node.Duration);
            }
        }

        private Vector3 RandomHemisphere(Vector3 normal)
        {
            Vector3 value = new Vector3(
                NextSigned(),
                NextSigned(),
                NextSigned());
            if (value.sqrMagnitude < 0.001f)
                value = Vector3.up;
            value.Normalize();
            if (Vector3.Dot(value, normal) < 0f)
                value = -value;
            return value;
        }

        private static Vector3 Direction(BattleEvent battleEvent)
        {
            Vector3 direction =
                battleEvent.Direction.ToUnity();
            return direction.sqrMagnitude < 0.01f
                ? Vector3.up
                : direction.normalized;
        }

        private static Vector3 OutwardDirection(
            BattleEvent battleEvent)
        {
            Vector3 normal = battleEvent.Normal.ToUnity();
            if (normal.sqrMagnitude >= 0.01f)
                return normal.normalized;
            return Direction(battleEvent);
        }

        private static bool IsVisualEvent(BattleEventType type)
        {
            return type == BattleEventType.ShellFired ||
                type == BattleEventType.ShellHit ||
                type == BattleEventType.TankDestroyed ||
                type == BattleEventType.StructureHit ||
                type == BattleEventType.StructureDestroyed ||
                type == BattleEventType.PropCrushed;
        }

        private float Next01()
        {
            _noise = _noise * 1664525u + 1013904223u;
            return (_noise >> 8) / 16777216f;
        }

        private float NextSigned()
        {
            return Next01() * 2f - 1f;
        }

        private static void Seed(
            ParticleSystem particles,
            int index,
            int channel)
        {
            StopAndClear(particles);
            particles.useAutoRandomSeed = false;
            particles.randomSeed =
                0x9e3779b9u +
                (uint)(index * 3 + channel + 1) *
                2654435761u;
        }

        private static void StopAndClear(
            ParticleSystem particles)
        {
            particles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private sealed class EffectNode
        {
            public GameObject Root;
            public ParticleSystem Core;
            public ParticleSystem Sparks;
            public ParticleSystem Cloud;
            public float ExpiresAt;
        }

        private sealed class LightNode
        {
            public GameObject Root;
            public Light Light;
            public float ExpiresAt;
            public float Duration;
            public float Peak;
        }

    }
}
