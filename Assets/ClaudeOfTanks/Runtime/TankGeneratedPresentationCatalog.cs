using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankGeneratedPresentationCatalog
    {
        private const string ResourcePath =
            "Generated/tank-presentation-schemas";
        private const int SupportedSchemaVersion = 1;
        private static GeneratedPresentationData _data;

        public static bool TryBuild(
            string id,
            Transform root,
            Transform turret,
            VehicleDefinition definition)
        {
            GeneratedPresentationVehicle schema = Find(id);
            if (schema == null) return false;

            HideTargets(root, schema);
            if (!string.IsNullOrEmpty(schema.markerName))
            {
                GameObject marker = new GameObject(schema.markerName);
                marker.transform.SetParent(root, false);
            }

            Transform gunFittings = null;
            for (int i = 0; i < schema.meshes.Length; i++)
            {
                BuildMesh(
                    schema,
                    schema.meshes[i],
                    root,
                    turret,
                    ref gunFittings);
            }
            return true;
        }

        private static GeneratedPresentationVehicle Find(string id)
        {
            GeneratedPresentationData data = Load();
            if (data?.vehicles == null) return null;
            for (int i = 0; i < data.vehicles.Length; i++)
            {
                if (data.vehicles[i].id == id)
                    return data.vehicles[i];
            }
            return null;
        }

        private static GeneratedPresentationData Load()
        {
            if (_data != null) return _data;
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null) return null;
            GeneratedPresentationData data =
                JsonUtility.FromJson<GeneratedPresentationData>(
                    asset.text);
            if (data == null ||
                data.schemaVersion != SupportedSchemaVersion)
            {
                throw new InvalidOperationException(
                    "Unsupported tank presentation schema.");
            }
            _data = data;
            return _data;
        }

        private static void BuildMesh(
            GeneratedPresentationVehicle schema,
            GeneratedPresentationMesh mesh,
            Transform root,
            Transform turret,
            ref Transform gunFittings)
        {
            if (mesh?.vertices == null ||
                mesh.triangles == null ||
                mesh.vertices.Length < 9 ||
                mesh.triangles.Length < 3)
                return;

            Transform parent = ResolveParent(
                schema,
                mesh.target,
                root,
                turret,
                ref gunFittings);
            if (parent == null) return;

            Mesh unityMesh = new Mesh
            {
                name = mesh.name
            };
            if (mesh.vertices.Length / 3 > 65535)
                unityMesh.indexFormat =
                    UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices =
                new Vector3[mesh.vertices.Length / 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                int offset = i * 3;
                vertices[i] = new Vector3(
                    mesh.vertices[offset],
                    mesh.vertices[offset + 1],
                    mesh.vertices[offset + 2]);
            }
            unityMesh.vertices = vertices;
            unityMesh.triangles = mesh.triangles;
            unityMesh.RecalculateNormals();
            unityMesh.RecalculateBounds();

            GameObject part = new GameObject(mesh.name);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = unityMesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = mesh.color == null
                        ? Color.white
                        : new Color(
                            mesh.color.r,
                            mesh.color.g,
                            mesh.color.b)
                };
        }

        private static Transform ResolveParent(
            GeneratedPresentationVehicle schema,
            string target,
            Transform root,
            Transform turret,
            ref Transform gunFittings)
        {
            if (target == "root") return root;
            if (target == "turret") return turret;
            if (target != "gunFittings") return root;

            if (gunFittings != null) return gunFittings;
            Transform gun = turret.Find("Gun");
            if (gun == null) return null;
            gunFittings = TankDetailGeometry.GunFittingsRoot(
                gun,
                schema.gunFittingsRootName);
            return gunFittings;
        }

        private static void HideTargets(
            Transform root,
            GeneratedPresentationVehicle schema)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < parts.Length; i++)
            {
                if (ShouldHide(parts[i].name, schema))
                    Hide(parts[i]);
            }
        }

        private static bool ShouldHide(
            string name,
            GeneratedPresentationVehicle schema)
        {
            for (int i = 0; i < schema.hiddenNames.Length; i++)
            {
                if (string.Equals(
                    name,
                    schema.hiddenNames[i],
                    StringComparison.Ordinal))
                    return true;
            }
            for (int i = 0; i < schema.hiddenPrefixes.Length; i++)
            {
                if (name.StartsWith(
                    schema.hiddenPrefixes[i],
                    StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static void Hide(Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }

    [Serializable]
    internal sealed class GeneratedPresentationData
    {
        public int schemaVersion;
        public GeneratedPresentationVehicle[] vehicles;
    }

    [Serializable]
    internal sealed class GeneratedPresentationVehicle
    {
        public string id;
        public string markerName;
        public string gunFittingsRootName;
        public string[] hiddenNames;
        public string[] hiddenPrefixes;
        public int meshCount;
        public int vertexCount;
        public int triangleCount;
        public GeneratedPresentationMesh[] meshes;
    }

    [Serializable]
    internal sealed class GeneratedPresentationMesh
    {
        public string name;
        public string target;
        public GeneratedPresentationColor color;
        public float[] vertices;
        public int[] triangles;
    }

    [Serializable]
    internal sealed class GeneratedPresentationColor
    {
        public float r;
        public float g;
        public float b;
    }
}
