using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClaudeOfTanks.Editor
{
    public static class TankPresentationPrefabBaker
    {
        private const int SupportedSchemaVersion = 1;
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
            mesh.triangles = source.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material MaterialFor(
            PresentationMesh source,
            Dictionary<string, Material> materials)
        {
            PresentationColor color = source.color;
            string key = color == null
                ? "1_1_1"
                : $"{color.r:0.####}_{color.g:0.####}_{color.b:0.####}";
            if (materials.TryGetValue(key, out Material existing))
                return existing;

            Material material =
                new Material(Shader.Find("Standard"))
                {
                    name = "Mat-" + key,
                    color = color == null
                        ? Color.white
                        : new Color(color.r, color.g, color.b)
                };
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
        public PresentationColor color;
        public float[] vertices;
        public int[] triangles;
    }

    [Serializable]
    internal sealed class PresentationColor
    {
        public float r;
        public float g;
        public float b;
    }
}
