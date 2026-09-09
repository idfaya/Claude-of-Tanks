using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankDetailGeometry
    {
        public static float HullRoofY(
            VehicleDefinition definition,
            float fallback)
        {
            return PlateExtreme(
                definition?.armor?.hullPlates,
                "hull_roof",
                1,
                true,
                fallback);
        }

        public static float HullRearZ(
            VehicleDefinition definition,
            float fallback)
        {
            return PlateExtreme(
                definition?.armor?.hullPlates,
                null,
                2,
                false,
                fallback);
        }

        public static float TurretRoofY(
            VehicleDefinition definition)
        {
            return PlateExtreme(
                definition?.armor?.turretPlates,
                "turret_roof",
                1,
                true,
                0.75f);
        }

        public static float TurretFrontZ(
            VehicleDefinition definition)
        {
            return PlateExtreme(
                definition?.armor?.turretPlates,
                null,
                2,
                true,
                1f);
        }

        public static float TurretRearZ(
            VehicleDefinition definition)
        {
            return PlateExtreme(
                definition?.armor?.turretPlates,
                null,
                2,
                false,
                -1.2f);
        }

        public static float TurretHalfWidth(
            VehicleDefinition definition,
            float fallback)
        {
            ArmorPlateDefinition[] plates =
                definition?.armor?.turretPlates;
            float width = fallback;
            if (plates == null) return width;
            for (int plate = 0;
                plate < plates.Length;
                plate++)
            {
                CatalogPoint[] vertices =
                    plates[plate]?.verts;
                if (vertices == null) continue;
                for (int vertex = 0;
                    vertex < vertices.Length;
                    vertex++)
                {
                    width = Mathf.Max(
                        width,
                        Mathf.Abs(vertices[vertex].x));
                }
            }
            return width;
        }

        public static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            GameObject part =
                GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Collider collider =
                part.GetComponent<Collider>();
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(collider);
            else
                UnityEngine.Object.DestroyImmediate(collider);
            part.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = color
                };
            return part.transform;
        }

        public static Transform MeshPart(
            string name,
            Transform parent,
            Vector3[] vertices,
            int[] triangles,
            Color color)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            Mesh mesh = new Mesh { name = name + "Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = color
                };
            return part.transform;
        }

        public static Transform LathePart(
            string name,
            Transform parent,
            float[] radii,
            float[] heights,
            int segments,
            float zScale,
            Color color)
        {
            if (radii == null ||
                heights == null ||
                radii.Length != heights.Length ||
                radii.Length < 2)
                throw new ArgumentException(
                    "Lathe profile requires matching radius/height arrays.");

            int rings = radii.Length;
            Vector3[] vertices =
                new Vector3[segments * rings];
            for (int segment = 0; segment < segments; segment++)
            {
                float angle =
                    segment * Mathf.PI * 2f / segments;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                for (int ring = 0; ring < rings; ring++)
                {
                    vertices[segment * rings + ring] =
                        new Vector3(
                            cosine * radii[ring],
                            heights[ring],
                            sine * radii[ring] * zScale);
                }
            }

            int[] triangles =
                new int[segments * (rings - 1) * 6];
            int cursor = 0;
            for (int segment = 0; segment < segments; segment++)
            {
                int nextSegment =
                    (segment + 1) % segments;
                for (int ring = 0; ring < rings - 1; ring++)
                {
                    int a = segment * rings + ring;
                    int b = nextSegment * rings + ring;
                    int c = nextSegment * rings + ring + 1;
                    int d = segment * rings + ring + 1;
                    triangles[cursor++] = a;
                    triangles[cursor++] = c;
                    triangles[cursor++] = b;
                    triangles[cursor++] = a;
                    triangles[cursor++] = d;
                    triangles[cursor++] = c;
                }
            }

            return MeshPart(
                name,
                parent,
                vertices,
                triangles,
                color);
        }

        public static Transform GunFittingsRoot(
            Transform gun,
            string name)
        {
            GameObject root = new GameObject(name);
            Transform transform = root.transform;
            transform.SetParent(gun, false);
            transform.localPosition =
                new Vector3(0f, 0f, -0.5f);
            transform.localRotation =
                Quaternion.identity;
            Vector3 scale = gun.localScale;
            transform.localScale =
                new Vector3(
                    scale.x == 0f
                        ? 1f
                        : 1f / scale.x,
                    scale.y == 0f
                        ? 1f
                        : 1f / scale.y,
                    scale.z == 0f
                        ? 1f
                        : 1f / scale.z);
            return transform;
        }

        private static float PlateExtreme(
            ArmorPlateDefinition[] plates,
            string name,
            int axis,
            bool maximum,
            float fallback)
        {
            if (plates == null) return fallback;
            float result = fallback;
            bool found = false;
            for (int plate = 0;
                plate < plates.Length;
                plate++)
            {
                ArmorPlateDefinition definition =
                    plates[plate];
                if (definition?.verts == null ||
                    (name != null &&
                     !definition.name.StartsWith(
                         name,
                         StringComparison.Ordinal)))
                {
                    continue;
                }
                for (int vertex = 0;
                    vertex < definition.verts.Length;
                    vertex++)
                {
                    CatalogPoint point =
                        definition.verts[vertex];
                    float value = axis == 0
                        ? point.x
                        : axis == 1
                            ? point.y
                            : point.z;
                    result = !found
                        ? value
                        : maximum
                            ? Mathf.Max(result, value)
                            : Mathf.Min(result, value);
                    found = true;
                }
            }
            return found ? result : fallback;
        }
    }
}
