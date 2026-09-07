using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleEffects : MonoBehaviour
    {
        public const int EffectPoolSize = 24;
        public const int DecalPoolSize = 48;
        public const int LightPoolSize = 2;

        private readonly Queue<EffectNode> _available = new Queue<EffectNode>();
        private readonly List<EffectNode> _active = new List<EffectNode>();
        private readonly List<EffectNode> _nodes = new List<EffectNode>();
        private readonly Queue<DecalNode> _availableDecals = new Queue<DecalNode>();
        private readonly List<DecalNode> _activeDecals = new List<DecalNode>();
        private readonly List<DecalNode> _decals = new List<DecalNode>();
        private readonly LightNode[] _lights = new LightNode[LightPoolSize];
        private AudioClip _shotLight;
        private AudioClip _shotHeavy;
        private AudioClip _penetration;
        private AudioClip _ricochet;
        private AudioClip _destroyed;
        private Material _particleMaterial;
        private Material _decalMaterial;
        private Texture2D _decalTexture;
        private MaterialPropertyBlock _decalProperties;
        private int _lightCursor;
        private uint _noise = 0x91e10da5u;

        public int ActiveEffectCount => _active.Count;
        public int ActiveDecalCount => _activeDecals.Count;

        public int ActiveLightCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _lights.Length; i++)
                {
                    if (_lights[i].Root.activeSelf) count++;
                }
                return count;
            }
        }

        public static BattleEffects Create()
        {
            GameObject root = new GameObject("BattleEffects");
            BattleEffects effects = root.AddComponent<BattleEffects>();
            effects.Initialize();
            return effects;
        }

        public void Play(BattleEvent battleEvent, Transform target = null)
        {
            EffectNode node = AcquireEffect();
            Vector3 position = battleEvent.Position.ToUnity();
            node.Root.transform.position = position;
            node.Root.SetActive(true);
            ConfigureParticles(node.Particles, battleEvent);
            ConfigureAudio(node.Audio, battleEvent);
            node.ExpiresAt = Time.unscaledTime + Lifetime(battleEvent.Type);
            _active.Add(node);

            TriggerLight(battleEvent, position);
            if (battleEvent.Type == BattleEventType.ShellHit && target != null)
            {
                StampDecal(battleEvent, target);
            }
        }

        public void ResetAll()
        {
            _active.Clear();
            _available.Clear();
            for (int i = 0; i < _nodes.Count; i++)
            {
                EffectNode node = _nodes[i];
                node.Audio.Stop();
                node.Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                node.Root.SetActive(false);
                _available.Enqueue(node);
            }

            _activeDecals.Clear();
            _availableDecals.Clear();
            for (int i = 0; i < _decals.Count; i++)
            {
                DecalNode decal = _decals[i];
                decal.Root.transform.SetParent(transform, false);
                decal.Root.SetActive(false);
                _availableDecals.Enqueue(decal);
            }

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].Root.SetActive(false);
                _lights[i].Light.intensity = 0f;
            }
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                EffectNode node = _active[i];
                if (node.ExpiresAt > now) continue;
                node.Audio.Stop();
                node.Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
                node.Light.intensity = node.Peak * Mathf.Clamp01(remaining / node.Duration);
            }
        }

        private void Initialize()
        {
            _shotLight = Tone("ShotLight", 92f, 0.16f, 0.34f, 0.32f);
            _shotHeavy = Tone("ShotHeavy", 54f, 0.34f, 0.5f, 0.5f);
            _penetration = Tone("Penetration", 145f, 0.16f, 0.3f, 0.62f);
            _ricochet = Tone("Ricochet", 620f, 0.2f, 0.2f, 0.72f);
            _destroyed = Tone("Destroyed", 42f, 0.8f, 0.58f, 0.55f);
            _decalTexture = BuildDecalTexture();
            Shader particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended") ??
                Shader.Find("Particles/Standard Unlit") ??
                Shader.Find("Standard");
            _particleMaterial = new Material(particleShader)
            {
                color = Color.white,
                mainTexture = _decalTexture
            };
            _decalMaterial = BuildDecalMaterial(_decalTexture);
            _decalProperties = new MaterialPropertyBlock();

            for (int i = 0; i < EffectPoolSize; i++)
            {
                EffectNode node = CreateEffectNode(i);
                _nodes.Add(node);
                _available.Enqueue(node);
            }
            for (int i = 0; i < DecalPoolSize; i++)
            {
                DecalNode decal = CreateDecalNode(i);
                _decals.Add(decal);
                _availableDecals.Enqueue(decal);
            }
            for (int i = 0; i < LightPoolSize; i++)
            {
                GameObject root = new GameObject("CombatLight-" + i);
                root.transform.SetParent(transform, false);
                Light light = root.AddComponent<Light>();
                light.type = LightType.Point;
                light.shadows = LightShadows.None;
                root.SetActive(false);
                _lights[i] = new LightNode { Root = root, Light = light };
            }
        }

        private EffectNode CreateEffectNode(int index)
        {
            GameObject root = new GameObject("Effect-" + index);
            root.transform.SetParent(transform, false);
            ParticleSystem particles = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 64;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;
            ParticleSystemRenderer particleRenderer = root.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.sharedMaterial = _particleMaterial;

            AudioSource audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 1f;
            audio.maxDistance = 900f;
            audio.rolloffMode = AudioRolloffMode.Logarithmic;
            root.SetActive(false);
            return new EffectNode
            {
                Root = root,
                Particles = particles,
                Audio = audio
            };
        }

        private DecalNode CreateDecalNode(int index)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Quad);
            root.name = "ImpactDecal-" + index;
            root.transform.SetParent(transform, false);
            Collider collider = root.GetComponent<Collider>();
            if (collider != null) Release(collider);
            Renderer renderer = root.GetComponent<Renderer>();
            renderer.sharedMaterial = _decalMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            root.SetActive(false);
            return new DecalNode { Root = root, Renderer = renderer };
        }

        private EffectNode AcquireEffect()
        {
            if (_available.Count == 0)
            {
                EffectNode oldest = _active[0];
                _active.RemoveAt(0);
                oldest.Audio.Stop();
                oldest.Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                oldest.Root.SetActive(false);
                _available.Enqueue(oldest);
            }
            return _available.Dequeue();
        }

        private void ConfigureParticles(ParticleSystem particles, BattleEvent battleEvent)
        {
            ParticleSystem.MainModule main = particles.main;
            bool destroyed = battleEvent.Type == BattleEventType.TankDestroyed;
            bool fired = battleEvent.Type == BattleEventType.ShellFired;
            main.startLifetime = destroyed ? 1.4f : fired ? 0.28f : 0.55f;
            main.startSize = destroyed ? 0.65f : fired ? 0.24f : 0.18f;
            main.gravityModifier = destroyed ? 0.7f : 0.28f;
            Color color = destroyed
                ? new Color(1f, 0.24f, 0.025f, 1f)
                : fired
                    ? new Color(1f, 0.72f, 0.22f, 1f)
                    : battleEvent.Penetrated
                    ? new Color(1f, 0.32f, 0.06f, 1f)
                    : new Color(0.86f, 0.8f, 0.62f, 1f);
            int count = destroyed ? 34 : fired ? 16 : battleEvent.Penetrated ? 20 : 12;
            Vector3 baseDirection = battleEvent.Direction.ToUnity();
            if (baseDirection.sqrMagnitude < 0.1f) baseDirection = Vector3.up;
            baseDirection.Normalize();
            Vector3 normal = battleEvent.Normal.ToUnity();
            if (normal.sqrMagnitude < 0.1f) normal = Vector3.up;
            normal.Normalize();
            if (!fired) baseDirection = Vector3.Reflect(baseDirection, normal);

            particles.Clear(true);
            for (int i = 0; i < count; i++)
            {
                Vector3 spread = new Vector3(NextSigned(), Mathf.Abs(NextSigned()), NextSigned()).normalized;
                float speed = destroyed ? Mathf.Lerp(4f, 12f, Next01()) : Mathf.Lerp(2f, 8f, Next01());
                ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
                {
                    position = Vector3.zero,
                    velocity = Vector3.Lerp(baseDirection, spread, destroyed ? 0.8f : 0.45f).normalized * speed,
                    startColor = color,
                    startSize = (destroyed ? 0.45f : 0.12f) * Mathf.Lerp(0.65f, 1.35f, Next01()),
                    startLifetime = destroyed ? Mathf.Lerp(0.8f, 1.5f, Next01()) : Mathf.Lerp(0.2f, 0.65f, Next01())
                };
                particles.Emit(emit, 1);
            }
            particles.Play();
        }

        private void ConfigureAudio(AudioSource audio, BattleEvent battleEvent)
        {
            if (battleEvent.Type == BattleEventType.ShellFired)
            {
                audio.clip = battleEvent.CaliberMm > 105f ? _shotHeavy : _shotLight;
                audio.volume = Mathf.Lerp(0.55f, 1f, Mathf.Clamp01(battleEvent.CaliberMm / 150f));
            }
            else if (battleEvent.Type == BattleEventType.TankDestroyed)
            {
                audio.clip = _destroyed;
                audio.volume = 1f;
            }
            else
            {
                audio.clip = battleEvent.Penetrated ? _penetration : _ricochet;
                audio.volume = battleEvent.Penetrated ? 0.82f : 0.65f;
            }
            audio.pitch = 0.97f + Next01() * 0.06f;
            if (Application.isPlaying) audio.Play();
        }

        private void TriggerLight(BattleEvent battleEvent, Vector3 position)
        {
            LightNode node = _lights[_lightCursor];
            _lightCursor = (_lightCursor + 1) % _lights.Length;
            node.Root.transform.position = position;
            node.Root.SetActive(true);
            bool destroyed = battleEvent.Type == BattleEventType.TankDestroyed;
            bool fired = battleEvent.Type == BattleEventType.ShellFired;
            node.Light.color = fired
                ? new Color(1f, 0.76f, 0.42f)
                : new Color(1f, 0.28f, 0.06f);
            node.Light.range = destroyed ? 18f : fired ? 10f : 7f;
            node.Duration = destroyed ? 0.7f : fired ? 0.09f : 0.18f;
            node.Peak = destroyed ? 7f : fired ? 4f : 2.5f;
            node.ExpiresAt = Time.unscaledTime + node.Duration;
            node.Light.intensity = node.Peak;
        }

        private void StampDecal(BattleEvent battleEvent, Transform target)
        {
            DecalNode decal;
            if (_availableDecals.Count > 0)
            {
                decal = _availableDecals.Dequeue();
            }
            else
            {
                decal = _activeDecals[0];
                _activeDecals.RemoveAt(0);
            }

            Vector3 normal = battleEvent.Normal.ToUnity();
            if (normal.sqrMagnitude < 0.1f) normal = Vector3.up;
            normal.Normalize();
            Vector3 decalUp = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f
                ? Vector3.forward
                : Vector3.up;
            decal.Root.transform.SetParent(target, true);
            decal.Root.transform.position = battleEvent.Position.ToUnity() + normal * 0.025f;
            decal.Root.transform.rotation = Quaternion.LookRotation(normal, decalUp);
            float size = Mathf.Clamp(battleEvent.CaliberMm / 180f, 0.22f, 0.85f);
            decal.Root.transform.localScale = Vector3.one * size;
            _decalProperties.SetColor(
                "_Color",
                battleEvent.Penetrated
                    ? new Color(0.055f, 0.045f, 0.035f, 0.92f)
                    : new Color(0.34f, 0.31f, 0.27f, 0.75f));
            decal.Renderer.SetPropertyBlock(_decalProperties);
            decal.Root.SetActive(true);
            _activeDecals.Add(decal);
        }

        private static float Lifetime(BattleEventType type)
        {
            return type == BattleEventType.TankDestroyed ? 1.5f
                : type == BattleEventType.ShellFired ? 0.45f
                : 0.7f;
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

        private static AudioClip Tone(
            string name, float frequency, float duration, float gain, float noiseGain)
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
                samples[i] = (Mathf.Sin(2f * Mathf.PI * frequency * i / rate) *
                    (1f - noiseGain) + random * noiseGain) * envelope * gain;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Texture2D BuildDecalTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "ImpactDecalTexture",
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

        private static Material BuildDecalMaterial(Texture texture)
        {
            Material material = new Material(Shader.Find("Standard"))
            {
                name = "ImpactDecalMaterial",
                mainTexture = texture
            };
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_Cull", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            return material;
        }

        private void OnDestroy()
        {
            Release(_shotLight);
            Release(_shotHeavy);
            Release(_penetration);
            Release(_ricochet);
            Release(_destroyed);
            Release(_particleMaterial);
            Release(_decalMaterial);
            Release(_decalTexture);
        }

        private static void Release(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private sealed class EffectNode
        {
            public GameObject Root;
            public ParticleSystem Particles;
            public AudioSource Audio;
            public float ExpiresAt;
        }

        private sealed class DecalNode
        {
            public GameObject Root;
            public Renderer Renderer;
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
