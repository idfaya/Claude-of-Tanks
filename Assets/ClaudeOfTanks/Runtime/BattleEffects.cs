using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleEffects : MonoBehaviour
    {
        public const int EffectPoolSize =
            BattleOneShotEffects.PoolSize;
        public const int DecalPoolSize = 48;
        public const int LightPoolSize =
            BattleOneShotEffects.LightPoolSize;

        private readonly Queue<DecalNode> _availableDecals = new Queue<DecalNode>();
        private readonly List<DecalNode> _activeDecals = new List<DecalNode>();
        private readonly List<DecalNode> _decals = new List<DecalNode>();
        private Material _particleMaterial;
        private Material _decalMaterial;
        private Texture2D _decalTexture;
        private MaterialPropertyBlock _decalProperties;
        private BattleOneShotEffects _oneShots;
        private BattlePersistentEffects _persistent;
        private GameSettings _settings;
        private uint _noise = 0x91e10da5u;

        public int ActiveEffectCount => _oneShots.ActiveCount;
        public int ActiveDecalCount => _activeDecals.Count;
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
                StampDecal(battleEvent, target);
            }
            if (battleEvent.Type == BattleEventType.TankDestroyed)
                StampGroundScorch(battleEvent);
        }

        public void SyncPersistent(IList<TankState> tanks)
        {
            _persistent.Sync(tanks);
        }

        public void ResetAll()
        {
            _oneShots.ResetAll();

            _activeDecals.Clear();
            _availableDecals.Clear();
            for (int i = 0; i < _decals.Count; i++)
            {
                DecalNode decal = _decals[i];
                decal.Root.transform.SetParent(transform, false);
                decal.Root.SetActive(false);
                _availableDecals.Enqueue(decal);
            }

            _persistent.ResetAll();
        }

        private void Initialize()
        {
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
            _oneShots = BattleOneShotEffects.Create(
                transform,
                _particleMaterial,
                _settings);
            _persistent = BattlePersistentEffects.Create(
                transform,
                _particleMaterial,
                _settings);

            for (int i = 0; i < DecalPoolSize; i++)
            {
                DecalNode decal = CreateDecalNode(i);
                _decals.Add(decal);
                _availableDecals.Enqueue(decal);
            }
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

        private void StampDecal(BattleEvent battleEvent, Transform target)
        {
            DecalNode decal = AcquireDecal();

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

        private void StampGroundScorch(BattleEvent battleEvent)
        {
            DecalNode decal = AcquireDecal();
            decal.Root.transform.SetParent(transform, true);
            decal.Root.transform.position =
                battleEvent.Position.ToUnity() + Vector3.up * 0.035f;
            decal.Root.transform.rotation =
                Quaternion.Euler(-90f, Next01() * 360f, 0f);
            float size = Mathf.Clamp(
                battleEvent.CaliberMm / 22f,
                4.4f,
                7.2f);
            decal.Root.transform.localScale =
                new Vector3(size, size * 0.82f, 1f);
            _decalProperties.SetColor(
                "_Color",
                new Color(0.035f, 0.028f, 0.022f, 0.82f));
            decal.Renderer.SetPropertyBlock(_decalProperties);
            decal.Root.SetActive(true);
            _activeDecals.Add(decal);
        }

        private DecalNode AcquireDecal()
        {
            if (_availableDecals.Count > 0)
                return _availableDecals.Dequeue();
            DecalNode oldest = _activeDecals[0];
            _activeDecals.RemoveAt(0);
            return oldest;
        }

        private float Next01()
        {
            _noise = _noise * 1664525u + 1013904223u;
            return (_noise >> 8) / 16777216f;
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

        private sealed class DecalNode
        {
            public GameObject Root;
            public Renderer Renderer;
        }

    }
}
