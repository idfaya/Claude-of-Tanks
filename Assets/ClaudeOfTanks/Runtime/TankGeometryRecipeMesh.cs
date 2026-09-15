using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankGeometryMaterialKey :
        IEquatable<TankGeometryMaterialKey>
    {
        public readonly int Owner;
        public readonly int Role;
        private readonly Color32 _color;

        public TankGeometryMaterialKey(
            int owner,
            int role,
            Color color)
        {
            Owner = owner;
            Role = role;
            _color = color;
        }

        public bool Equals(
            TankGeometryMaterialKey other)
        {
            return Owner == other.Owner &&
                Role == other.Role &&
                _color.Equals(other._color);
        }

        public override bool Equals(object value)
        {
            return value is
                    TankGeometryMaterialKey &&
                Equals(
                    (TankGeometryMaterialKey)value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return
                    (Owner * 397) ^
                    (Role * 31) ^
                    _color.GetHashCode();
            }
        }

        public Color Color => _color;
    }

    internal sealed class
        TankGeometryMeshAccumulator
    {
        private readonly string _name;
        private readonly List<Vector3> _vertices =
            new List<Vector3>();
        private readonly List<Vector3> _normals =
            new List<Vector3>();
        private readonly List<Vector2> _uvs =
            new List<Vector2>();
        private readonly List<int> _triangles =
            new List<int>();

        public TankGeometryMeshAccumulator(
            string name)
        {
            _name = name;
        }

        public void Append(
            Vector3[] positions,
            Vector3[] normals,
            Vector2[] uvs,
            int[] triangles,
            Matrix4x4 matrix)
        {
            int offset = _vertices.Count;
            Matrix4x4 normalMatrix =
                matrix.inverse.transpose;
            for (int index = 0;
                index < positions.Length;
                index++)
            {
                _vertices.Add(
                    matrix.MultiplyPoint3x4(
                        positions[index]));
                _normals.Add(
                    normals == null
                        ? Vector3.zero
                        : normalMatrix
                            .MultiplyVector(
                                normals[index])
                            .normalized);
                _uvs.Add(
                    uvs == null
                        ? Vector2.zero
                        : uvs[index]);
            }
            for (int index = 0;
                index < triangles.Length;
                index += 3)
            {
                _triangles.Add(
                    offset +
                    triangles[index]);
                _triangles.Add(
                    offset +
                    triangles[index + 2]);
                _triangles.Add(
                    offset +
                    triangles[index + 1]);
            }
        }

        public void Build(
            TankGeometryMaterialKey key,
            Transform parent)
        {
            string role = RoleName(key.Role);
            string prefix =
                IsPaintedRole(key.Role)
                    ? "Painted-Recipe-"
                    : "Recipe-";
            GameObject obj =
                new GameObject(
                    prefix + role + "-" +
                    _name);
            obj.transform.SetParent(
                parent,
                false);
            Mesh mesh =
                new Mesh
                {
                    name = obj.name + "Mesh",
                    indexFormat =
                        _vertices.Count >
                            ushort.MaxValue
                            ? IndexFormat.UInt32
                            : IndexFormat.UInt16
                };
            mesh.SetVertices(_vertices);
            bool hasNormals = false;
            for (int index = 0;
                index < _normals.Count;
                index++)
            {
                if (_normals[index]
                    .sqrMagnitude > 0.25f)
                {
                    hasNormals = true;
                    break;
                }
            }
            if (hasNormals)
                mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(
                _triangles,
                0,
                true);
            if (!hasNormals)
                mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            obj.AddComponent<MeshFilter>()
                .sharedMesh = mesh;
            MeshRenderer renderer =
                obj.AddComponent<MeshRenderer>();
            Material material =
                new Material(
                    Shader.Find("Standard"))
                {
                    color = key.Color
                };
            if (key.Role == 5)
                ConfigureTransparency(material);
            renderer.sharedMaterial = material;
        }

        private static void ConfigureTransparency(
            Material material)
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt(
                "_SrcBlend",
                (int)BlendMode.SrcAlpha);
            material.SetInt(
                "_DstBlend",
                (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
        }

        private static bool IsPaintedRole(int role)
        {
            return role == 0 ||
                role == 1 ||
                role == 8;
        }

        private static string RoleName(int role)
        {
            switch (role)
            {
                case 0: return "Armor";
                case 1: return "Wheel";
                case 2: return "Rubber";
                case 3: return "Track";
                case 4: return "Gunmetal";
                case 5: return "Optic";
                case 6: return "Canvas";
                case 7: return "Wood";
                case 8: return "Fitting";
                case 9: return "Marking";
                default: return "Other";
            }
        }
    }
}
