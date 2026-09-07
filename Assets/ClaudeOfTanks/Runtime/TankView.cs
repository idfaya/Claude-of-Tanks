using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class TankView
    {
        private readonly Transform _root;
        private readonly Transform _turret;
        private readonly Renderer[] _renderers;
        private readonly Mesh[] _meshes;
        private readonly Material[] _materials;
        private readonly Color _aliveColor;

        private TankView(Transform root, Transform turret, Renderer[] renderers, Color aliveColor)
        {
            _root = root;
            _turret = turret;
            _renderers = renderers;
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            int generatedCount = 0;
            for (int i = 0; i < filters.Length; i++)
                if (filters[i].name.StartsWith("Armor-")) generatedCount++;
            _meshes = new Mesh[generatedCount];
            int generatedIndex = 0;
            for (int i = 0; i < filters.Length; i++)
                if (filters[i].name.StartsWith("Armor-"))
                    _meshes[generatedIndex++] = filters[i].sharedMesh;
            _materials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) _materials[i] = renderers[i].sharedMaterial;
            _aliveColor = aliveColor;
        }

        public Transform Root => _root;

        public static TankView Create(TankState tank)
        {
            return Create(tank, null);
        }

        public static TankView Create(TankState tank, VehicleDefinition definition)
        {
            Color authored = definition != null && definition.visual != null
                ? definition.visual.Color : new Color(0.28f, 0.32f, 0.24f);
            Color teamColor = tank.Team == Team.Alpha
                ? Color.Lerp(authored, new Color(0.16f, 0.55f, 0.25f), 0.35f)
                : Color.Lerp(authored, new Color(0.68f, 0.18f, 0.12f), 0.48f);
            float width = definition?.dims != null && definition.dims.widthM > 0f
                ? definition.dims.widthM : 3.4f;
            float length = definition?.dims != null && definition.dims.hullLengthM > 0f
                ? definition.dims.hullLengthM : 5.4f;
            float height = definition?.dims != null && definition.dims.heightM > 0f
                ? definition.dims.heightM : 2.8f;
            float trackWidth = definition?.visual != null && definition.visual.trackWidthM > 0f
                ? definition.visual.trackWidthM : width * 0.16f;
            GameObject root = new GameObject(tank.Id);

            float trackY = Mathf.Max(0.4f, height * 0.21f);
            CreatePart("Hull", PrimitiveType.Cube, root.transform, new Vector3(0f, trackY + height * 0.16f, 0f),
                new Vector3(width * 0.84f, height * 0.28f, length * 0.92f), teamColor);
            CreatePart("UpperHull", PrimitiveType.Cube, root.transform,
                new Vector3(0f, trackY + height * 0.35f, length * 0.02f),
                new Vector3(width * 0.72f, height * 0.18f, length * 0.62f), teamColor * 1.08f);
            CreateTrack(root.transform, -width * 0.47f, trackY, length, trackWidth);
            CreateTrack(root.transform, width * 0.47f, trackY, length, trackWidth);

            bool casemate = definition != null &&
                (definition.role == "td" || definition.id.StartsWith("strv103"));
            GameObject turretRoot = new GameObject("TurretRoot");
            turretRoot.transform.SetParent(root.transform, false);
            turretRoot.transform.localPosition = new Vector3(0f, height * 0.63f, length * 0.03f);
            float turretScale = definition != null && definition.role == "ifv" ? 0.36f : 0.55f;
            if (casemate)
            {
                CreatePart("Turret", PrimitiveType.Cube, turretRoot.transform,
                    new Vector3(0f, -height * 0.12f, length * 0.08f),
                    new Vector3(width * 0.72f, height * 0.28f, length * 0.42f), teamColor * 0.92f);
            }
            else
            {
                CreatePart("Turret", PrimitiveType.Cylinder, turretRoot.transform, Vector3.zero,
                    new Vector3(width * turretScale, height * 0.2f, width * turretScale), teamColor * 0.92f);
            }
            float gunLength = definition?.gun != null && definition.gun.caliberMm < 80f
                ? length * 0.35f : length * 0.62f;
            float gunRadius = Mathf.Clamp(
                (definition?.gun != null ? definition.gun.caliberMm : 120f) / 500f, 0.08f, 0.32f);
            Transform barrel = CreatePart("Gun", PrimitiveType.Cube, turretRoot.transform,
                new Vector3(0f, 0.05f, width * turretScale + gunLength * 0.45f),
                new Vector3(gunRadius, gunRadius, gunLength),
                new Color(0.12f, 0.14f, 0.12f));
            barrel.localRotation = Quaternion.identity;
            CreateArmorSurfaces(root.transform, turretRoot.transform, definition, teamColor);
            AddFamilyDetails(root.transform, turretRoot.transform, definition, teamColor, width, height, length);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            TankView view = new TankView(root.transform, turretRoot.transform, renderers, teamColor);
            view.Sync(tank);
            return view;
        }

        private static void CreateArmorSurfaces(
            Transform hull, Transform turret, VehicleDefinition definition, Color color)
        {
            if (definition?.armor == null) return;
            CreatePlateSet(hull, definition.armor.hullPlates, color);
            CreatePlateSet(turret, definition.armor.turretPlates, color * 0.94f);
        }

        private static void CreatePlateSet(
            Transform parent, ArmorPlateDefinition[] plates, Color baseColor)
        {
            if (plates == null) return;
            for (int i = 0; i < plates.Length; i++)
            {
                ArmorPlateDefinition plate = plates[i];
                if (plate.verts == null || plate.verts.Length < 3) continue;
                Mesh mesh = new Mesh { name = plate.name };
                Vector3[] vertices = new Vector3[plate.verts.Length];
                for (int vertex = 0; vertex < vertices.Length; vertex++)
                {
                    vertices[vertex] = plate.verts[vertex].ToVector3();
                }
                int faceTriangles = (vertices.Length - 2) * 3;
                int[] triangles = new int[faceTriangles];
                int cursor = 0;
                for (int triangle = 1; triangle < vertices.Length - 1; triangle++)
                {
                    triangles[cursor++] = 0;
                    triangles[cursor++] = triangle;
                    triangles[cursor++] = triangle + 1;
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                GameObject surface = new GameObject("Armor-" + plate.name);
                surface.transform.SetParent(parent, false);
                surface.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer renderer = surface.AddComponent<MeshRenderer>();
                Color color = plate.kind == "era"
                    ? Color.Lerp(baseColor, new Color(0.2f, 0.24f, 0.16f), 0.45f)
                    : plate.kind == "spaced" ? baseColor * 0.82f : baseColor * 1.04f;
                Material material = new Material(Shader.Find("Standard")) { color = color };
                material.SetInt("_Cull", 0);
                renderer.sharedMaterial = material;
            }
        }

        private static void AddFamilyDetails(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (definition == null) return;
            if (definition.role == "ifv")
            {
                CreatePart("MissilePod", PrimitiveType.Cube, turret,
                    new Vector3(width * 0.3f, height * 0.05f, 0f),
                    new Vector3(width * 0.22f, height * 0.18f, width * 0.38f), color * 0.78f);
            }
            if (definition.era == "modern")
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    CreatePart("SideArmor", PrimitiveType.Cube, root,
                        new Vector3(side * width * 0.48f, height * 0.46f, 0f),
                        new Vector3(width * 0.06f, height * 0.24f, length * 0.58f), color * 0.82f);
                }
            }
            if (definition.id.Contains("t90") || definition.id.Contains("t72") ||
                definition.id.Contains("t80"))
            {
                for (int i = -2; i <= 2; i++)
                {
                    CreatePart("ERA", PrimitiveType.Cube, turret,
                        new Vector3(i * width * 0.12f, 0f, width * 0.43f),
                        new Vector3(width * 0.1f, height * 0.09f, 0.16f), color * 1.12f);
                }
            }
            if (definition.id.Contains("abrams") || definition.id.StartsWith("m1a"))
            {
                CreatePart("TurretBustle", PrimitiveType.Cube, turret,
                    new Vector3(0f, 0f, -width * 0.34f),
                    new Vector3(width * 0.62f, height * 0.16f, width * 0.38f), color * 0.88f);
            }
            if (definition.id.Contains("leopard") || definition.id.StartsWith("leo2"))
            {
                CreatePart("TurretWedge", PrimitiveType.Cube, turret,
                    new Vector3(0f, 0f, width * 0.4f),
                    new Vector3(width * 0.78f, height * 0.24f, width * 0.42f), color * 1.05f);
            }
        }

        private static void CreateTrack(
            Transform root, float x, float y, float length, float width)
        {
            CreatePart(x < 0f ? "LeftTrack" : "RightTrack", PrimitiveType.Cube, root,
                new Vector3(x, y, 0f), new Vector3(width, y, length),
                new Color(0.09f, 0.09f, 0.08f));
            int wheelCount = Mathf.Clamp(Mathf.RoundToInt(length * 0.9f), 4, 8);
            for (int i = 0; i < wheelCount; i++)
            {
                float z = Mathf.Lerp(-length * 0.38f, length * 0.38f, i / (float)(wheelCount - 1));
                Transform wheel = CreatePart("RoadWheel", PrimitiveType.Cylinder, root,
                    new Vector3(x, y, z), new Vector3(y * 0.72f, width * 0.7f, y * 0.72f),
                    new Color(0.12f, 0.13f, 0.11f));
                wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        public void Sync(TankState tank)
        {
            _root.position = tank.Position.ToUnity();
            _root.rotation = Quaternion.Euler(0f, tank.Yaw * Mathf.Rad2Deg, 0f);
            _turret.localRotation = Quaternion.Euler(0f, tank.TurretYaw * Mathf.Rad2Deg, 0f);

            Color color = tank.Destroyed ? new Color(0.08f, 0.08f, 0.075f) : _aliveColor;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].gameObject.name == "Hull" ||
                    _renderers[i].gameObject.name == "UpperHull" ||
                    _renderers[i].gameObject.name == "Turret")
                {
                    _renderers[i].sharedMaterial.color = color;
                }
            }
        }

        public void Destroy()
        {
            DestroyObject(_root.gameObject);
            for (int i = 0; i < _meshes.Length; i++) DestroyObject(_meshes[i]);
            for (int i = 0; i < _materials.Length; i++) DestroyObject(_materials[i]);
        }

        private static void DestroyObject(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Object.Destroy(value);
            else Object.DestroyImmediate(value);
        }

        private static Transform CreatePart(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
            return part.transform;
        }
    }

    internal static class VectorConversion
    {
        public static Vector3 ToUnity(this Float3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Float3 ToSimulation(this Vector3 value)
        {
            return new Float3(value.x, value.y, value.z);
        }
    }
}
