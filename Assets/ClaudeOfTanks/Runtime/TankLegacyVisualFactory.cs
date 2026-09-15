using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLegacyVisualFactory
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length,
            float trackWidth)
        {
            float trackY =
                TankRunningGearLayout.RoadWheelY(
                    definition,
                    height);
            Part(
                "Hull",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    trackY + height * 0.16f,
                    0f),
                new Vector3(
                    width * 0.84f,
                    height * 0.28f,
                    length * 0.92f),
                color);
            Part(
                "UpperHull",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    trackY + height * 0.35f,
                    length * 0.02f),
                new Vector3(
                    width * 0.72f,
                    height * 0.18f,
                    length * 0.62f),
                color * 1.08f);
            int roadWheelCount =
                TankRunningGearLayout.RoadWheelCount(
                    definition,
                    length);
            float trackCenterX =
                TankRunningGearLayout.TrackCenterX(
                    definition,
                    width);
            TankLegacyRunningGearFactory.BuildPair(
                root,
                trackCenterX,
                trackY,
                length,
                trackWidth,
                roadWheelCount,
                definition);

            bool casemate =
                definition != null &&
                (definition.role == "td" ||
                 definition.id.StartsWith("strv103"));
            float turretScale =
                definition != null &&
                definition.role == "ifv"
                    ? 0.36f
                    : 0.55f;
            Part(
                "Turret",
                casemate
                    ? PrimitiveType.Cube
                    : PrimitiveType.Cylinder,
                turret,
                casemate
                    ? new Vector3(
                        0f,
                        -height * 0.12f,
                        length * 0.08f)
                    : Vector3.zero,
                casemate
                    ? new Vector3(
                        width * 0.72f,
                        height * 0.28f,
                        length * 0.42f)
                    : new Vector3(
                        width * turretScale,
                        height * 0.2f,
                        width * turretScale),
                color * 0.92f);
            float gunLength =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    length);
            float gunRadius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);
            Transform barrel = Part(
                "Gun",
                PrimitiveType.Cube,
                turret,
                TankAuthoredDetails.ResolveGunCenter(
                    definition,
                    width * turretScale,
                    gunLength),
                new Vector3(
                    gunRadius,
                    gunRadius,
                    gunLength),
                new Color(0.12f, 0.14f, 0.12f));
            barrel.localRotation = Quaternion.identity;
            ArmorSurfaces(
                root,
                turret,
                definition,
                color);
            TankAuthoredDetails.Build(
                root,
                turret,
                definition,
                color);
            TankFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
        }

        private static void ArmorSurfaces(
            Transform hull,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (definition?.armor == null) return;
            PlateSet(
                hull,
                definition.armor.hullPlates,
                color);
            PlateSet(
                turret,
                definition.armor.turretPlates,
                color * 0.94f);
        }

        private static void PlateSet(
            Transform parent,
            ArmorPlateDefinition[] plates,
            Color baseColor)
        {
            if (plates == null) return;
            for (int i = 0; i < plates.Length; i++)
            {
                ArmorPlateDefinition plate = plates[i];
                if (plate.verts == null ||
                    plate.verts.Length < 3)
                {
                    continue;
                }
                Mesh mesh = new Mesh { name = plate.name };
                Vector3[] vertices =
                    new Vector3[plate.verts.Length];
                for (int vertex = 0;
                    vertex < vertices.Length;
                    vertex++)
                {
                    vertices[vertex] =
                        plate.verts[vertex].ToVector3();
                }
                int[] triangles =
                    new int[(vertices.Length - 2) * 3];
                int cursor = 0;
                for (int triangle = 1;
                    triangle < vertices.Length - 1;
                    triangle++)
                {
                    triangles[cursor++] = 0;
                    triangles[cursor++] = triangle;
                    triangles[cursor++] = triangle + 1;
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.uv =
                    TankCamouflageMaterialApplicator
                        .ProjectUvs(
                            vertices,
                            mesh.bounds,
                            mesh.normals.Length > 0
                                ? mesh.normals[0]
                                : Vector3.up);
                GameObject surface =
                    new GameObject(
                        "Armor-" + plate.name);
                surface.transform.SetParent(
                    parent,
                    false);
                surface.AddComponent<MeshFilter>()
                    .sharedMesh = mesh;
                MeshRenderer renderer =
                    surface.AddComponent<MeshRenderer>();
                Color color =
                    plate.kind == "era"
                        ? Color.Lerp(
                            baseColor,
                            new Color(
                                0.2f,
                                0.24f,
                                0.16f),
                            0.45f)
                        : plate.kind == "spaced"
                            ? baseColor * 0.82f
                            : baseColor * 1.04f;
                Material material =
                    new Material(
                        Shader.Find("Standard"))
                    {
                        color = color
                    };
                material.SetInt("_Cull", 0);
                renderer.sharedMaterial = material;
            }
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            GameObject part =
                GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Collider collider =
                part.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(collider);
                else
                    Object.DestroyImmediate(collider);
            }
            part.GetComponent<Renderer>()
                .sharedMaterial =
                    new Material(
                        Shader.Find("Standard"))
                    {
                        color = color
                    };
            return part.transform;
        }
    }
}
