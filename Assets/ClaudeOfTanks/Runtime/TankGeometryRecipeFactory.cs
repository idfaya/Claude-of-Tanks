using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankGeometryRecipeFactory
    {
        private const string ResourceRoot =
            "Content/VehicleGeometry/";
        private const ushort SupportedVersion = 3;
        private const int MaximumSources = 4096;
        private const int MaximumVertices = 2_000_000;
        private const int MaximumInstances = 4096;

        internal static bool EnableEditModeRecipes
        {
            get;
            set;
        }

        public static bool ShouldBuild(
            VehicleDefinition definition)
        {
            return
                (Application.isPlaying ||
                 EnableEditModeRecipes) &&
                HasRecipe(definition);
        }

        public static bool HasRecipe(
            VehicleDefinition definition)
        {
            return definition != null &&
                !string.IsNullOrEmpty(definition.id) &&
                Resources.Load<TextAsset>(
                    ResourceRoot + definition.id) != null;
        }

        public static bool TryBuild(
            Transform root,
            VehicleDefinition definition,
            out Transform recipeTurret,
            out Transform recipeGun)
        {
            recipeTurret = null;
            recipeGun = null;
            if (root == null ||
                definition == null ||
                string.IsNullOrEmpty(definition.id))
            {
                return false;
            }
            TextAsset asset =
                Resources.Load<TextAsset>(
                    ResourceRoot + definition.id);
            if (asset == null) return false;

            Renderer[] legacy =
                root.GetComponentsInChildren<Renderer>(true);
            Transform recipeRoot = null;
            try
            {
                recipeRoot =
                    new GameObject(
                        "AuthoritativeVisualRecipe")
                        .transform;
                recipeRoot.SetParent(root, false);
                Build(
                    asset.bytes,
                    definition.id,
                    recipeRoot,
                    out recipeTurret,
                    out recipeGun);
                for (int index = 0;
                    index < legacy.Length;
                    index++)
                {
                    legacy[index]
                        .forceRenderingOff = true;
                }
                return true;
            }
            catch
            {
                Release(
                    recipeRoot != null
                        ? recipeRoot.gameObject
                        : null);
                recipeTurret = null;
                recipeGun = null;
                throw;
            }
        }

        private static void Build(
            byte[] compressed,
            string expectedId,
            Transform recipeRoot,
            out Transform recipeTurret,
            out Transform recipeGun)
        {
            recipeTurret = null;
            recipeGun = null;
            using (MemoryStream source =
                new MemoryStream(compressed, false))
            using (GZipStream gzip =
                new GZipStream(
                    source,
                    CompressionMode.Decompress))
            using (BinaryReader reader =
                new BinaryReader(
                    gzip,
                    Encoding.UTF8,
                    false))
            {
                string magic =
                    Encoding.ASCII.GetString(
                        reader.ReadBytes(4));
                if (magic != "CTG3")
                    throw new InvalidDataException(
                        "Unsupported vehicle geometry recipe.");
                ushort version = reader.ReadUInt16();
                if (version != SupportedVersion)
                    throw new InvalidDataException(
                        "Unsupported vehicle geometry version.");
                int sourceCount = reader.ReadUInt16();
                if (sourceCount <= 0 ||
                    sourceCount > MaximumSources)
                {
                    throw new InvalidDataException(
                        "Invalid vehicle geometry source count.");
                }
                string id = ReadString(reader);
                if (id != expectedId)
                    throw new InvalidDataException(
                        "Vehicle geometry id mismatch.");

                Vector3 turretPosition =
                    ReadVector3(reader);
                Vector3 gunPosition =
                    ReadVector3(reader);
                Vector3 hullScale =
                    ReadVector3(reader);
                Vector3 turretScale =
                    ReadVector3(reader);
                Vector3 gunScale =
                    ReadVector3(reader);
                Transform recipeHull =
                    CreateRig(
                        recipeRoot,
                        "RecipeHullRoot",
                        Vector3.zero);
                recipeHull.localScale = hullScale;
                recipeTurret =
                    CreateRig(
                        recipeRoot,
                        "RecipeTurretRoot",
                        turretPosition);
                recipeTurret.localScale =
                    turretScale;
                recipeGun =
                    CreateRig(
                        recipeTurret,
                        "RecipeGunAssembly",
                        gunPosition);
                recipeGun.localScale = gunScale;
                Transform recoil =
                    CreateRig(
                        recipeGun,
                        "RecipeRecoilRoot",
                        Vector3.zero);
                Transform[] owners =
                {
                    recipeHull,
                    recipeTurret,
                    recipeGun,
                    recoil,
                    CreateRig(
                        recoil,
                        "RecipeBarrelRoot-0",
                        Vector3.zero),
                    CreateRig(
                        recoil,
                        "RecipeBarrelRoot-1",
                        Vector3.zero)
                };

                Dictionary<
                    TankGeometryMaterialKey,
                    TankGeometryMeshAccumulator>
                    accumulators =
                        new Dictionary<
                            TankGeometryMaterialKey,
                            TankGeometryMeshAccumulator>();
                for (int index = 0;
                    index < sourceCount;
                    index++)
                {
                    ReadSource(
                        reader,
                        owners,
                        accumulators);
                }
                foreach (KeyValuePair<
                    TankGeometryMaterialKey,
                    TankGeometryMeshAccumulator> pair
                    in accumulators)
                {
                    pair.Value.Build(
                        pair.Key,
                        owners[pair.Key.Owner]);
                }
            }
        }

        private static void ReadSource(
            BinaryReader reader,
            Transform[] owners,
            Dictionary<
                TankGeometryMaterialKey,
                TankGeometryMeshAccumulator>
                accumulators)
        {
            int owner = reader.ReadByte();
            int role = reader.ReadByte();
            int flags = reader.ReadByte();
            reader.ReadByte();
            string name = ReadString(reader);
            int vertexCount =
                checked((int)reader.ReadUInt32());
            int indexCount =
                checked((int)reader.ReadUInt32());
            int matrixCount = reader.ReadUInt16();
            reader.ReadUInt16();
            if (owner < 0 ||
                owner >= owners.Length ||
                vertexCount < 3 ||
                vertexCount > MaximumVertices ||
                indexCount < 0 ||
                indexCount > MaximumVertices * 6 ||
                matrixCount <= 0 ||
                matrixCount > MaximumInstances)
            {
                throw new InvalidDataException(
                    "Invalid vehicle geometry source.");
            }
            Color color = new Color(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
            Vector3[] positions =
                new Vector3[vertexCount];
            for (int vertex = 0;
                vertex < vertexCount;
                vertex++)
            {
                positions[vertex] =
                    ReadVector3(reader);
            }
            Vector3[] normals = null;
            if ((flags & 1) != 0)
            {
                normals =
                    new Vector3[vertexCount];
                for (int vertex = 0;
                    vertex < vertexCount;
                    vertex++)
                {
                    normals[vertex] =
                        ReadVector3(reader);
                }
            }
            Vector2[] uvs = null;
            if ((flags & 2) != 0)
            {
                uvs = new Vector2[vertexCount];
                for (int vertex = 0;
                    vertex < vertexCount;
                    vertex++)
                {
                    uvs[vertex] =
                        new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle());
                }
            }
            int[] triangles;
            if ((flags & 4) != 0)
            {
                triangles =
                    new int[indexCount];
                for (int index = 0;
                    index < indexCount;
                    index++)
                {
                    triangles[index] =
                        checked((int)
                            reader.ReadUInt32());
                }
            }
            else
            {
                triangles =
                    new int[vertexCount];
                for (int index = 0;
                    index < vertexCount;
                    index++)
                {
                    triangles[index] = index;
                }
            }
            if (triangles.Length < 3 ||
                triangles.Length % 3 != 0)
            {
                throw new InvalidDataException(
                    "Invalid vehicle geometry triangles.");
            }

            TankGeometryMaterialKey key =
                new TankGeometryMaterialKey(
                    owner,
                    role,
                    color);
            TankGeometryMeshAccumulator accumulator;
            if (!accumulators.TryGetValue(
                    key,
                    out accumulator))
            {
                accumulator =
                    new TankGeometryMeshAccumulator(name);
                accumulators.Add(
                    key,
                    accumulator);
            }
            for (int instance = 0;
                instance < matrixCount;
                instance++)
            {
                Matrix4x4 matrix =
                    ReadMatrix(reader);
                accumulator.Append(
                    positions,
                    normals,
                    uvs,
                    triangles,
                    matrix);
            }
        }

        private static Transform CreateRig(
            Transform parent,
            string name,
            Vector3 position)
        {
            Transform created =
                new GameObject(name).transform;
            created.SetParent(parent, false);
            created.localPosition = position;
            return created;
        }

        private static Vector3 ReadVector3(
            BinaryReader reader)
        {
            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        private static Matrix4x4 ReadMatrix(
            BinaryReader reader)
        {
            Vector4 column0 = new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
            Vector4 column1 = new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
            Vector4 column2 = new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
            Vector4 column3 = new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
            return new Matrix4x4(
                column0,
                column1,
                column2,
                column3);
        }

        private static string ReadString(
            BinaryReader reader)
        {
            int length = reader.ReadUInt16();
            byte[] bytes =
                reader.ReadBytes(length);
            if (bytes.Length != length)
                throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }

        private static void Release(
            UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }

    }
}
