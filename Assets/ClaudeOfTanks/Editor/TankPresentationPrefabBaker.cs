using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClaudeOfTanks.Editor
{
    public static class TankPresentationPrefabBaker
    {
        private const int SupportedSchemaVersion = 2;
        private const string SourcePath =
            "Assets/ClaudeOfTanks/Generated/PresentationSource/tank-presentation-schemas.json";
        private const string OutputRoot =
            "Assets/ClaudeOfTanks/Resources/Generated/TankPresentation";

        public static void BakeAll()
        {
            if (!File.Exists(SourcePath))
                throw new FileNotFoundException(
                    "Generated presentation source is missing.",
                    SourcePath);

            PresentationData data =
                JsonUtility.FromJson<PresentationData>(
                    File.ReadAllText(SourcePath));
            if (data == null ||
                data.schemaVersion != SupportedSchemaVersion)
            {
                throw new InvalidOperationException(
                    "Unsupported presentation source schema.");
            }

            EnsureFolder("Assets/ClaudeOfTanks/Resources/Generated");
            EnsureFolder(OutputRoot);
            for (int i = 0; i < data.vehicles.Length; i++)
                BakeVehicle(data.vehicles[i]);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BakeVehicle(
            PresentationVehicle vehicle)
        {
            string id = vehicle.id;
            string assetPath = $"{OutputRoot}/{id}.asset";
            string prefabPath = $"{OutputRoot}/{id}.prefab";
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.DeleteAsset(prefabPath);

            GameObject prefabRoot =
                new GameObject($"BakedTankPresentation-{id}");
            GameObject rootSection =
                Section(prefabRoot.transform, "Root");
            GameObject turretSection =
                Section(prefabRoot.transform, "Turret");
            GameObject gunSection =
                Section(prefabRoot.transform, "GunFittings");

            Dictionary<string, Material> materials =
                new Dictionary<string, Material>();
            bool assetCreated = false;
            for (int i = 0; i < vehicle.meshes.Length; i++)
            {
                PresentationMesh source = vehicle.meshes[i];
                Mesh mesh = BuildMesh(source);
                if (!assetCreated)
                {
                    AssetDatabase.CreateAsset(mesh, assetPath);
                    assetCreated = true;
                }
                else
                {
                    AssetDatabase.AddObjectToAsset(mesh, assetPath);
                }

                Material material = MaterialFor(source, materials);
                if (!AssetDatabase.Contains(material))
                    AssetDatabase.AddObjectToAsset(material, assetPath);

                GameObject part = new GameObject(source.name);
                part.transform.SetParent(
                    TargetSection(
                        source.target,
                        rootSection,
                        turretSection,
                        gunSection).transform,
                    false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = material;
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);
            Debug.Log(
                $"Baked tank presentation prefab {id}: " +
                $"{vehicle.meshCount} meshes, " +
                $"{vehicle.vertexCount} vertices, " +
                $"{vehicle.triangleCount} triangles.");
        }

        private static Mesh BuildMesh(
            PresentationMesh source)
        {
            Mesh mesh = new Mesh
            {
                name = source.name
            };
            if (source.vertices.Length / 3 > 65535)
                mesh.indexFormat =
                    UnityEngine.Rendering.IndexFormat.UInt32;

            Vector3[] vertices =
                new Vector3[source.vertices.Length / 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                int offset = i * 3;
                vertices[i] = new Vector3(
                    source.vertices[offset],
                    source.vertices[offset + 1],
                    source.vertices[offset + 2]);
            }
            mesh.vertices = vertices;
            if (source.normals != null &&
                source.normals.Length == source.vertices.Length)
            {
                Vector3[] normals =
                    new Vector3[source.normals.Length / 3];
                for (int i = 0; i < normals.Length; i++)
                {
                    int offset = i * 3;
                    normals[i] = new Vector3(
                        source.normals[offset],
                        source.normals[offset + 1],
                        source.normals[offset + 2]);
                }
                mesh.normals = normals;
            }
            if (source.uvs != null &&
                source.uvs.Length == vertices.Length * 2)
            {
                Vector2[] uvs = new Vector2[source.uvs.Length / 2];
                for (int i = 0; i < uvs.Length; i++)
                {
                    int offset = i * 2;
                    uvs[i] = new Vector2(
                        source.uvs[offset],
                        source.uvs[offset + 1]);
                }
                mesh.uv = uvs;
            }
            if (source.colors != null &&
                source.colors.Length == source.vertices.Length)
            {
                Color[] colors =
                    new Color[source.colors.Length / 3];
                for (int i = 0; i < colors.Length; i++)
                {
                    int offset = i * 3;
                    colors[i] = new Color(
                        source.colors[offset],
                        source.colors[offset + 1],
                        source.colors[offset + 2],
                        1f);
                }
                mesh.colors = colors;
            }
            mesh.triangles = source.triangles;
            if (mesh.normals == null ||
                mesh.normals.Length != vertices.Length)
                mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material MaterialFor(
            PresentationMesh source,
            Dictionary<string, Material> materials)
        {
            PresentationMaterial sourceMaterial = source.material;
            PresentationColor color = sourceMaterial?.color;
            PresentationColor emissive = sourceMaterial?.emissive;
            string key = color == null
                ? "1_1_1"
                : $"{sourceMaterial?.type}_{sourceMaterial?.name}_" +
                  $"{color.r:0.####}_{color.g:0.####}_{color.b:0.####}_" +
                  $"{emissive?.r:0.####}_{emissive?.g:0.####}_{emissive?.b:0.####}_" +
                  $"{sourceMaterial?.roughness:0.####}_{sourceMaterial?.metalness:0.####}_" +
                  $"{sourceMaterial?.opacity:0.####}_{sourceMaterial?.transparent}_{sourceMaterial?.side}";
            if (materials.TryGetValue(key, out Material existing))
                return existing;

            Shader shader =
                Shader.Find("ClaudeOfTanks/TankBakedPresentation");
            if (shader == null)
                shader = Shader.Find("Standard");
            Material material =
                new Material(shader)
                {
                    name = "Mat-" + key,
                    color = color == null
                        ? Color.white
                        : new Color(
                            color.r,
                            color.g,
                            color.b,
                            sourceMaterial?.opacity ?? 1f)
                };
            if (material.HasProperty("_EmissionColor") &&
                emissive != null)
            {
                material.SetColor(
                    "_EmissionColor",
                    new Color(emissive.r, emissive.g, emissive.b));
                if (emissive.r > 0f || emissive.g > 0f || emissive.b > 0f)
                    material.EnableKeyword("_EMISSION");
            }
            if (material.HasProperty("_Metallic"))
                material.SetFloat(
                    "_Metallic",
                    sourceMaterial?.metalness ?? 0f);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat(
                    "_Glossiness",
                    1f - (sourceMaterial?.roughness ?? 0.5f));
            if (material.HasProperty("_Cull"))
                material.SetFloat(
                    "_Cull",
                    sourceMaterial?.side == 2 ? 0f : 2f);
            float opacity = sourceMaterial?.opacity ?? 1f;
            if (sourceMaterial?.transparent == true ||
                opacity < 0.999f)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt(
                    "_SrcBlend",
                    (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt(
                    "_DstBlend",
                    (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue =
                    (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            materials.Add(key, material);
            return material;
        }

        private static GameObject Section(
            Transform parent,
            string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject TargetSection(
            string target,
            GameObject root,
            GameObject turret,
            GameObject gun)
        {
            if (target == "turret") return turret;
            if (target == "gunFittings") return gun;
            return root;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)
                ?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    [Serializable]
    internal sealed class PresentationData
    {
        public int schemaVersion;
        public PresentationVehicle[] vehicles;
    }

    [Serializable]
    internal sealed class PresentationVehicle
    {
        public string id;
        public int meshCount;
        public int vertexCount;
        public int triangleCount;
        public PresentationMesh[] meshes;
    }

    [Serializable]
    internal sealed class PresentationMesh
    {
        public string name;
        public string target;
        public PresentationMaterial material;
        public float[] vertices;
        public float[] normals;
        public float[] uvs;
        public float[] colors;
        public int[] triangles;
    }

    [Serializable]
    internal sealed class PresentationMaterial
    {
        public string name;
        public string type;
        public PresentationColor color;
        public PresentationColor emissive;
        public float roughness;
        public float metalness;
        public float opacity;
        public bool transparent;
        public int side;
        public bool vertexColors;
        public bool hasMap;
    }

    [Serializable]
    internal sealed class PresentationColor
    {
        public float r;
        public float g;
        public float b;
    }
}
