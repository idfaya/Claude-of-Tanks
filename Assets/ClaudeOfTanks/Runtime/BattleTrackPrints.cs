using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleTrackPrints : MonoBehaviour
    {
        private readonly Queue<TrackPrint> _available =
            new Queue<TrackPrint>();
        private readonly List<TrackPrint> _active =
            new List<TrackPrint>();
        private readonly List<TrackPrint> _prints =
            new List<TrackPrint>();
        private Material _material;
        private Texture2D _texture;
        private Mesh _mesh;
        private MaterialPropertyBlock _properties;

        public int ActiveCount => _active.Count;

        public static BattleTrackPrints Create(Transform parent)
        {
            GameObject root = new GameObject("TrackPrints");
            root.transform.SetParent(parent, false);
            BattleTrackPrints prints =
                root.AddComponent<BattleTrackPrints>();
            prints.Initialize();
            return prints;
        }

        public void Stamp(string ownerId, Vector3 position, float yaw)
        {
            TrackPrint print;
            if (_available.Count > 0)
            {
                print = _available.Dequeue();
            }
            else
            {
                print = _active[0];
                _active.RemoveAt(0);
            }
            print.OwnerId = ownerId;
            print.BornAt = Time.unscaledTime;
            print.Root.transform.SetPositionAndRotation(
                position + Vector3.up * 0.035f,
                Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f));
            print.Root.SetActive(true);
            SetAlpha(print, 0.48f);
            _active.Add(print);
        }

        public void ReleaseOwner(string ownerId)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].OwnerId == ownerId)
                    ReleaseAt(i);
            }
        }

        public void ResetAll()
        {
            _active.Clear();
            _available.Clear();
            for (int i = 0; i < _prints.Count; i++)
            {
                TrackPrint print = _prints[i];
                print.OwnerId = null;
                print.Root.SetActive(false);
                _available.Enqueue(print);
            }
        }

        private void Initialize()
        {
            _texture = BuildTexture();
            _material = BuildMaterial(_texture);
            _mesh = BuildMesh();
            _properties = new MaterialPropertyBlock();
            for (int i = 0;
                i < BattlePersistentEffects.TrackPrintLimit;
                i++)
            {
                TrackPrint print = CreatePrint(i);
                _prints.Add(print);
                _available.Enqueue(print);
            }
        }

        private TrackPrint CreatePrint(int index)
        {
            GameObject root = new GameObject("TrackPrint-" + index);
            root.transform.SetParent(transform, false);
            MeshFilter filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = _mesh;
            MeshRenderer renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            root.SetActive(false);
            return new TrackPrint { Root = root, Renderer = renderer };
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                TrackPrint print = _active[i];
                float age = now - print.BornAt;
                if (age >= BattlePersistentEffects.TrackPrintLifetimeS)
                {
                    ReleaseAt(i);
                    continue;
                }
                SetAlpha(
                    print,
                    0.48f *
                    (1f - age /
                     BattlePersistentEffects.TrackPrintLifetimeS));
            }
        }

        private void ReleaseAt(int index)
        {
            TrackPrint print = _active[index];
            _active.RemoveAt(index);
            print.OwnerId = null;
            print.Root.SetActive(false);
            _available.Enqueue(print);
        }

        private void SetAlpha(TrackPrint print, float alpha)
        {
            _properties.SetColor(
                "_Color",
                new Color(0.13f, 0.11f, 0.08f, alpha));
            print.Renderer.SetPropertyBlock(_properties);
        }

        private static Texture2D BuildTexture()
        {
            const int width = 32;
            const int height = 64;
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false)
            {
                name = "TrackPrintTexture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float edgeY =
                    Mathf.Min(y + 0.5f, height - y - 0.5f) / 7f;
                for (int x = 0; x < width; x++)
                {
                    float edgeX =
                        Mathf.Min(x + 0.5f, width - x - 0.5f) / 5f;
                    float tread = ((y / 7) & 1) == 0 ? 1f : 0.72f;
                    byte alpha = (byte)Mathf.RoundToInt(
                        255f *
                        Mathf.Clamp01(Mathf.Min(edgeX, edgeY)) *
                        tread);
                    pixels[y * width + x] =
                        new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Material BuildMaterial(Texture texture)
        {
            Shader shader = Shader.Find("Unlit/Transparent") ??
                Shader.Find("Standard");
            Material material = new Material(shader)
            {
                name = "TrackPrintMaterial",
                mainTexture = texture,
                color = new Color(0.13f, 0.11f, 0.08f, 0.48f),
                renderQueue = 3000
            };
            material.SetInt("_ZWrite", 0);
            material.SetInt("_Cull", 0);
            return material;
        }

        private static Mesh BuildMesh()
        {
            Mesh mesh = new Mesh { name = "TrackPrintMesh" };
            mesh.vertices = new[]
            {
                new Vector3(-1.5f, 0f, -0.65f),
                new Vector3(-0.9f, 0f, -0.65f),
                new Vector3(-0.9f, 0f, 0.65f),
                new Vector3(-1.5f, 0f, 0.65f),
                new Vector3(0.9f, 0f, -0.65f),
                new Vector3(1.5f, 0f, -0.65f),
                new Vector3(1.5f, 0f, 0.65f),
                new Vector3(0.9f, 0f, 0.65f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f)
            };
            mesh.triangles = new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 6, 5, 4, 7, 6
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            Release(_material);
            Release(_texture);
            Release(_mesh);
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private sealed class TrackPrint
        {
            public string OwnerId;
            public GameObject Root;
            public Renderer Renderer;
            public float BornAt;
        }
    }
}
