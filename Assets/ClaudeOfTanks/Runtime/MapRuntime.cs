using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class MapRuntime : IDisposable
    {
        private readonly GameObject _root;
        private readonly List<Material> _materials = new List<Material>();

        private MapRuntime(GameObject root)
        {
            _root = root;
        }

        public Transform Root => _root.transform;

        public static MapRuntime Create(MapDefinition map)
        {
            GameObject root = new GameObject("Map-" + map.id);
            MapRuntime runtime = new MapRuntime(root);
            runtime.Build(map);
            return runtime;
        }

        public void Dispose()
        {
            for (int i = 0; i < _materials.Count; i++) DestroyObject(_materials[i]);
            DestroyObject(_root);
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }

        private void Build(MapDefinition map)
        {
            Color fog = HexColor(map.sky != null ? map.sky.fogTintHex : 0x8799a0);
            RenderSettings.fog = true;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = Mathf.Max(0.0004f, (map.sky?.fogDensity ?? 0.0007f) * 6f);
            RenderSettings.ambientLight = Color.Lerp(fog, Color.gray, 0.5f);

            GameObject sun = new GameObject("Sun");
            sun.transform.SetParent(_root.transform, false);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = HexColor(map.sky != null ? map.sky.sunColorHex : 0xfff2cc);
            light.intensity = Mathf.Clamp(map.sky?.sunIntensity ?? 1.25f, 0.5f, 2.2f);
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(
                map.sky?.sunElevationDeg ?? 48f, -(map.sky?.sunAzimuthDeg ?? 32f), 0f);

            Color groundColor = GroundColor(map.id);
            CreatePrimitive("Battlefield", PrimitiveType.Plane, Vector3.zero,
                new Vector3(100f, 1f, 100f), groundColor);
            LandformDefinition[] landforms = map.terrain?.landforms ?? Array.Empty<LandformDefinition>();
            for (int i = 0; i < landforms.Length; i++) CreateLandform(landforms[i], groundColor);

            int rocks = Mathf.Clamp((map.props?.rocks ?? 0) / 18, 3, 18);
            int trees = Mathf.Clamp((map.vegetation?.loneCount ?? 0) / 24, 0, 14);
            DeterministicScatter(map.id, rocks, trees, groundColor);
        }

        private void CreateLandform(LandformDefinition landform, Color color)
        {
            float width = landform.width > 0f ? landform.width : landform.rx * 2f;
            float length = landform.length > 0f ? landform.length : landform.rz * 2f;
            GameObject form = CreatePrimitive(
                "Landform-" + landform.kind,
                PrimitiveType.Sphere,
                new Vector3(landform.x, landform.height * 0.4f - 2f, landform.z),
                new Vector3(
                    Mathf.Max(8f, width),
                    Mathf.Max(1f, Mathf.Abs(landform.height)),
                    Mathf.Max(8f, length)),
                Color.Lerp(color, Color.gray, 0.12f));
            form.transform.rotation = Quaternion.Euler(0f, landform.yawDeg, 0f);
        }

        private void DeterministicScatter(
            string mapId, int rockCount, int treeCount, Color groundColor)
        {
            System.Random random = new System.Random(StableHash(mapId));
            for (int i = 0; i < rockCount; i++)
            {
                float scale = Mathf.Lerp(1.2f, 4.5f, (float)random.NextDouble());
                CreatePrimitive("Rock", PrimitiveType.Sphere,
                    Position(random, scale * 0.35f), new Vector3(scale, scale * 0.7f, scale),
                    Color.Lerp(groundColor, new Color(0.3f, 0.3f, 0.28f), 0.65f));
            }
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 position = Position(random, 0f);
                GameObject tree = new GameObject("Tree");
                tree.transform.SetParent(_root.transform, false);
                tree.transform.position = position;
                CreatePrimitive("Trunk", PrimitiveType.Cylinder, position + Vector3.up * 2.5f,
                    new Vector3(0.5f, 2.5f, 0.5f), new Color(0.25f, 0.16f, 0.09f), tree.transform);
                CreatePrimitive("Crown", PrimitiveType.Sphere, position + Vector3.up * 6f,
                    new Vector3(4f, 5f, 4f), new Color(0.16f, 0.28f, 0.12f), tree.transform);
            }
        }

        private GameObject CreatePrimitive(
            string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color,
            Transform parent = null)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.SetParent(parent ?? _root.transform, true);
            result.transform.position = position;
            result.transform.localScale = scale;
            Material material = new Material(Shader.Find("Standard")) { color = color };
            _materials.Add(material);
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }

        private static Vector3 Position(System.Random random, float y)
        {
            return new Vector3(
                Mathf.Lerp(-430f, 430f, (float)random.NextDouble()),
                y,
                Mathf.Lerp(-430f, 430f, (float)random.NextDouble()));
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }

        private static Color HexColor(int value)
        {
            return new Color(
                ((value >> 16) & 255) / 255f,
                ((value >> 8) & 255) / 255f,
                (value & 255) / 255f);
        }

        private static Color GroundColor(string id)
        {
            if (id == "desert" || id == "badlands") return new Color(0.48f, 0.39f, 0.25f);
            if (id == "winter" || id == "alpine") return new Color(0.66f, 0.72f, 0.72f);
            if (id == "caldera" || id == "blackglass") return new Color(0.16f, 0.16f, 0.15f);
            if (id == "urban" || id == "foundry") return new Color(0.28f, 0.29f, 0.27f);
            return new Color(0.28f, 0.34f, 0.22f);
        }
    }
}
