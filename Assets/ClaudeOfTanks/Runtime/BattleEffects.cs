using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleEffects : MonoBehaviour
    {
        private const int PoolSize = 24;
        private readonly Queue<EffectNode> _available = new Queue<EffectNode>();
        private readonly List<EffectNode> _active = new List<EffectNode>();
        private AudioClip _shot;
        private AudioClip _impact;
        private AudioClip _destroyed;

        public static BattleEffects Create()
        {
            GameObject root = new GameObject("BattleEffects");
            BattleEffects effects = root.AddComponent<BattleEffects>();
            effects.Initialize();
            return effects;
        }

        public void Play(BattleEvent battleEvent)
        {
            if (_available.Count == 0)
            {
                EffectNode oldest = _active[0];
                _active.RemoveAt(0);
                oldest.Root.SetActive(false);
                _available.Enqueue(oldest);
            }
            EffectNode node = _available.Dequeue();
            node.Root.SetActive(true);
            node.Root.transform.position = battleEvent.Position.ToUnity();
            node.ExpiresAt = Time.unscaledTime + (battleEvent.Type == BattleEventType.TankDestroyed ? 1.4f : 0.45f);
            node.Light.color = battleEvent.Type == BattleEventType.ShellFired
                ? new Color(1f, 0.72f, 0.25f) : new Color(1f, 0.3f, 0.08f);
            node.Light.intensity = battleEvent.Type == BattleEventType.TankDestroyed ? 7f : 3f;
            node.Audio.clip = battleEvent.Type == BattleEventType.ShellFired ? _shot
                : battleEvent.Type == BattleEventType.TankDestroyed ? _destroyed : _impact;
            node.Audio.Play();
            _active.Add(node);
        }

        private void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                EffectNode node = _active[i];
                float remaining = node.ExpiresAt - Time.unscaledTime;
                node.Light.intensity *= Mathf.Clamp01(remaining * 8f);
                if (remaining > 0f) continue;
                node.Root.SetActive(false);
                _active.RemoveAt(i);
                _available.Enqueue(node);
            }
        }

        private void Initialize()
        {
            _shot = Tone("Shot", 72f, 0.18f, 0.4f);
            _impact = Tone("Impact", 120f, 0.12f, 0.25f);
            _destroyed = Tone("Destroyed", 48f, 0.65f, 0.5f);
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject root = new GameObject("Effect-" + i);
                root.transform.SetParent(transform, false);
                Light light = root.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 14f;
                AudioSource audio = root.AddComponent<AudioSource>();
                audio.spatialBlend = 1f;
                audio.maxDistance = 180f;
                audio.rolloffMode = AudioRolloffMode.Logarithmic;
                root.SetActive(false);
                _available.Enqueue(new EffectNode { Root = root, Light = light, Audio = audio });
            }
        }

        private static AudioClip Tone(string name, float frequency, float duration, float gain)
        {
            const int rate = 22050;
            int count = Mathf.CeilToInt(duration * rate);
            float[] samples = new float[count];
            uint noise = 0x91e10da5u;
            for (int i = 0; i < count; i++)
            {
                noise = noise * 1664525u + 1013904223u;
                float random = ((noise >> 8) / 16777216f) * 2f - 1f;
                float envelope = Mathf.Exp(-5f * i / (float)count);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * 0.55f +
                    random * 0.45f) * envelope * gain;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private sealed class EffectNode
        {
            public GameObject Root;
            public Light Light;
            public AudioSource Audio;
            public float ExpiresAt;
        }
    }
}
