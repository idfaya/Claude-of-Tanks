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
            Color color,
            float capRadius = 0f,
            float roofTiltScale = 1f)
        {
            if (radii == null ||
                heights == null ||
                radii.Length != heights.Length ||
                radii.Length < 2)
                throw new ArgumentException(
                    "Lathe profile requires matching radius/height arrays.");

            BuildLatheProfile(
                radii,
                heights,
                out float[] profileRadii,
                out float[] profileHeights,
                out float[] profileAngles);
            int rings = profileRadii.Length;
            Vector3[] vertices =
                new Vector3[segments * rings];
            Vector3[] normals =
                new Vector3[segments * rings];
            for (int segment = 0; segment < segments; segment++)
            {
                float angle =
                    segment * Mathf.PI * 2f / segments;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                for (int ring = 0; ring < rings; ring++)
                {
                    float radius = profileRadii[ring];
                    vertices[segment * rings + ring] =
                        new Vector3(
                            cosine * radius,
                            profileHeights[ring],
                            sine * radius * zScale);
                    float normalAngle = profileAngles[ring];
                    if (capRadius > 0f && normalAngle < 0.8f)
                    {
                        float capAngle = Mathf.Min(
                            0.8f,
                            Mathf.Asin(Mathf.Min(1f, radius / capRadius)));
                        if (capAngle > normalAngle)
                            normalAngle = capAngle;
                    }
                    if (roofTiltScale != 1f && normalAngle < 0.8f)
                        normalAngle *= roofTiltScale;
                    Vector3 normal = new Vector3(
                        cosine * Mathf.Sin(normalAngle),
                        Mathf.Cos(normalAngle),
                        sine * Mathf.Sin(normalAngle) / zScale);
                    normals[segment * rings + ring] =
                        normal.normalized;
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

            Transform result = MeshPart(
                name,
                parent,
                vertices,
                triangles,
                color);
            MeshFilter filter = result.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
                filter.sharedMesh.normals = normals;
            return result;
        }

        private static void BuildLatheProfile(
            float[] radii,
            float[] heights,
            out float[] profileRadii,
            out float[] profileHeights,
            out float[] profileAngles)
        {
            int sourceRings = radii.Length;
            float[] segmentAngles =
                new float[sourceRings - 1];
            int profileCount = 1;
            for (int index = 0; index < sourceRings - 1; index++)
            {
                float dr = radii[index + 1] - radii[index];
                float dy = heights[index + 1] - heights[index];
                segmentAngles[index] = Mathf.Atan2(dy, -dr);
                profileCount += Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        Mathf.Sqrt(dr * dr + dy * dy) / 0.055f));
            }

            float[] vertexAngles =
                new float[sourceRings];
            vertexAngles[0] = segmentAngles[0];
            for (int index = 1; index < sourceRings - 1; index++)
                vertexAngles[index] =
                    (segmentAngles[index - 1] + segmentAngles[index]) * 0.5f;
            vertexAngles[sourceRings - 1] =
                segmentAngles[sourceRings - 2];

            profileRadii = new float[profileCount];
            profileHeights = new float[profileCount];
            profileAngles = new float[profileCount];
            int cursor = 0;
            for (int index = 0; index < sourceRings - 1; index++)
            {
                float dr = radii[index + 1] - radii[index];
                float dy = heights[index + 1] - heights[index];
                int cuts = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        Mathf.Sqrt(dr * dr + dy * dy) / 0.055f));
                for (int cut = 0; cut < cuts; cut++)
                {
                    float t = cut / (float)cuts;
                    profileRadii[cursor] = radii[index] + dr * t;
                    profileHeights[cursor] = heights[index] + dy * t;
                    profileAngles[cursor] =
                        Mathf.Lerp(
                            vertexAngles[index],
                            vertexAngles[index + 1],
                            t);
                    cursor++;
                }
            }
            profileRadii[cursor] = radii[sourceRings - 1];
            profileHeights[cursor] = heights[sourceRings - 1];
            profileAngles[cursor] = vertexAngles[sourceRings - 1];
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
