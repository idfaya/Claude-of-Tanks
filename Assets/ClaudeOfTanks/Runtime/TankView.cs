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
        private readonly Texture2D _camouflageTexture;
        private readonly Color _aliveColor;

        private TankView(
            Transform root,
            Transform turret,
            Renderer[] renderers,
            Color aliveColor,
            Texture2D camouflageTexture)
        {
            _root = root;
            _turret = turret;
            _renderers = renderers;
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            int generatedCount = 0;
            for (int i = 0; i < filters.Length; i++)
                if (IsGeneratedMesh(filters[i])) generatedCount++;
            _meshes = new Mesh[generatedCount];
            int generatedIndex = 0;
            for (int i = 0; i < filters.Length; i++)
                if (IsGeneratedMesh(filters[i]))
                    _meshes[generatedIndex++] = filters[i].sharedMesh;
            _materials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) _materials[i] = renderers[i].sharedMaterial;
            _camouflageTexture = camouflageTexture;
            _aliveColor = aliveColor;
        }

        public Transform Root => _root;

        private static bool IsGeneratedMesh(MeshFilter filter)
        {
            return filter.name.StartsWith("Armor-") ||
                filter.name.StartsWith("TrackLinks-") ||
                (filter.sharedMesh != null &&
                 filter.sharedMesh.name == filter.name + "Mesh");
        }

        public static TankView Create(TankState tank)
        {
            return Create(tank, null, "factory", null);
        }

        public static TankView Create(TankState tank, VehicleDefinition definition)
        {
            return Create(tank, definition, "factory", null);
        }

        public static TankView Create(
            TankState tank,
            VehicleDefinition definition,
            string camouflageId,
            string mapId)
        {
            return Create(
                tank,
                definition,
                camouflageId,
                mapId,
                null);
        }

        public static TankView Create(
            TankState tank,
            VehicleDefinition definition,
            string camouflageId,
            string mapId,
            ContentCatalog catalog)
        {
            Color authored = definition != null && definition.visual != null
                ? TankCamouflage.ResolveColor(
                    definition,
                    camouflageId,
                    mapId)
                : new Color(0.28f, 0.32f, 0.24f);
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
            trackWidth = TankRunningGearLayout.TrackWidth(definition, trackWidth);
            GameObject root = new GameObject(tank.Id);

            float trackY = TankRunningGearLayout.RoadWheelY(definition, height);
            CreatePart("Hull", PrimitiveType.Cube, root.transform, new Vector3(0f, trackY + height * 0.16f, 0f),
                new Vector3(width * 0.84f, height * 0.28f, length * 0.92f), teamColor);
            CreatePart("UpperHull", PrimitiveType.Cube, root.transform,
                new Vector3(0f, trackY + height * 0.35f, length * 0.02f),
                new Vector3(width * 0.72f, height * 0.18f, length * 0.62f), teamColor * 1.08f);
            int roadWheelCount =
                TankRunningGearLayout.RoadWheelCount(
                    definition,
                    length);
            float trackCenterX = TankRunningGearLayout.TrackCenterX(definition, width);
            CreateTrack(root.transform, -trackCenterX, trackY, length, trackWidth, roadWheelCount, definition);
            CreateTrack(root.transform, trackCenterX, trackY, length, trackWidth, roadWheelCount, definition);

            bool casemate = definition != null &&
                (definition.role == "td" || definition.id.StartsWith("strv103"));
            GameObject turretRoot = new GameObject("TurretRoot");
            turretRoot.transform.SetParent(root.transform, false);
            turretRoot.transform.localPosition =
                TankAuthoredDetails.ResolveTurretPivot(
                    definition,
                    height,
                    length);
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
            float gunLength =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    length);
            float gunRadius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);
            Transform barrel = CreatePart("Gun", PrimitiveType.Cube, turretRoot.transform,
                TankAuthoredDetails.ResolveGunCenter(
                    definition,
                    width * turretScale,
                    gunLength),
                new Vector3(gunRadius, gunRadius, gunLength),
                new Color(0.12f, 0.14f, 0.12f));
            barrel.localRotation = Quaternion.identity;
            CreateArmorSurfaces(root.transform, turretRoot.transform, definition, teamColor);
            TankAuthoredDetails.Build(
                root.transform,
                turretRoot.transform,
                definition,
                teamColor);
            TankFamilyDetails.Build(
                root.transform,
                turretRoot.transform,
                definition,
                teamColor,
                width,
                height,
                length);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            Texture2D camouflageTexture =
                TankCamouflageMaterialApplicator.CreateAndApply(
                    catalog,
                    definition,
                    camouflageId,
                    mapId,
                    tank.Team,
                    renderers,
                    teamColor);
            TankView view = new TankView(
                root.transform,
                turretRoot.transform,
                renderers,
                camouflageTexture == null
                    ? teamColor
                    : Color.white,
                camouflageTexture);
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
                mesh.uv =
                    TankCamouflageMaterialApplicator.ProjectUvs(
                    vertices,
                    mesh.bounds,
                    mesh.normals.Length > 0
                        ? mesh.normals[0]
                        : Vector3.up);
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

        private static void CreateTrack(
            Transform root,
            float x,
            float y,
            float length,
            float width,
            int wheelCount,
            VehicleDefinition definition)
        {
            string side = x < 0f ? "L" : "R";
            GameObject gear = new GameObject("RunningGear-" + side);
            gear.transform.SetParent(root, false);
            Color trackColor = new Color(0.09f, 0.09f, 0.08f);
            Color wheelColor = new Color(0.15f, 0.16f, 0.13f);
            Color hubColor = new Color(0.23f, 0.25f, 0.2f);
            float roadRadius = TankRunningGearLayout.RoadWheelRadius(definition, y);
            float wheelThickness = width * 0.72f;
            for (int i = 0; i < wheelCount; i++)
            {
                float z = TankRunningGearLayout.RoadWheelZ(
                    definition, i, wheelCount, length);
                Transform arm = CreatePart("SuspensionArm-" + side, PrimitiveType.Cube, gear.transform,
                    new Vector3(x * 0.82f, y * 1.02f, z - roadRadius * 0.18f),
                    new Vector3(width * 0.2f, roadRadius * 0.16f, roadRadius * 0.9f),
                    wheelColor * 0.7f);
                arm.localRotation = Quaternion.Euler(18f, 0f, 0f);
                Transform joint = CreatePart("SuspensionJoint-" + side, PrimitiveType.Cylinder,
                    gear.transform, new Vector3(x * 0.82f, y * 1.18f, z - roadRadius * 0.46f),
                    new Vector3(roadRadius * 0.2f, width * 0.22f, roadRadius * 0.2f),
                    wheelColor * 0.72f);
                joint.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Transform wheel = CreatePart("RoadWheel-" + side, PrimitiveType.Cylinder, gear.transform,
                    new Vector3(x, y, z),
                    new Vector3(roadRadius, wheelThickness, roadRadius), wheelColor);
                wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Transform hub = CreatePart("WheelHub-" + side, PrimitiveType.Cylinder, gear.transform,
                    new Vector3(x + Mathf.Sign(x) * width * 0.38f, y, z),
                    new Vector3(roadRadius * 0.34f, width * 0.09f, roadRadius * 0.34f),
                    hubColor);
                hub.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            float sprocketRadius =
                TankRunningGearLayout.SprocketRadius(
                    definition,
                    roadRadius);
            float idlerRadius =
                TankRunningGearLayout.IdlerRadius(
                    definition,
                    roadRadius);
            Vector2 sprocket = TankRunningGearLayout.SprocketPosition(
                definition, length, y, roadRadius);
            Vector2 idler = TankRunningGearLayout.IdlerPosition(
                definition, length, y, roadRadius);
            CreateEndWheel("Sprocket-" + side, gear.transform, x, sprocket.y,
                sprocket.x, sprocketRadius, wheelThickness, hubColor);
            CreateEndWheel("Idler-" + side, gear.transform, x, idler.y,
                idler.x, idlerRadius, wheelThickness, wheelColor);

            int returnCount = Mathf.Clamp(wheelCount / 2, 2, 4);
            float topY = y + roadRadius * 0.92f;
            for (int i = 0; i < returnCount; i++)
            {
                float z = Mathf.Lerp(-length * 0.27f, length * 0.27f,
                    returnCount == 1 ? 0.5f : i / (float)(returnCount - 1));
                Transform roller = CreatePart("ReturnRoller-" + side, PrimitiveType.Cylinder,
                    gear.transform, new Vector3(x, topY - roadRadius * 0.08f, z),
                    new Vector3(roadRadius * 0.3f, wheelThickness * 0.72f, roadRadius * 0.3f),
                    wheelColor * 0.9f);
                roller.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            CreateTrackLinks(gear.transform, side, x, y, length, width, roadRadius, trackColor, definition);
        }

        private static void CreateEndWheel(
            string name,
            Transform parent,
            float x,
            float y,
            float z,
            float radius,
            float thickness,
            Color color)
        {
            Transform wheel = CreatePart(name, PrimitiveType.Cylinder, parent,
                new Vector3(x, y, z), new Vector3(radius, thickness, radius), color);
            wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Transform hub = CreatePart(name + "-Hub", PrimitiveType.Cylinder, parent,
                new Vector3(x + Mathf.Sign(x) * thickness * 0.52f, y, z),
                new Vector3(radius * 0.3f, thickness * 0.12f, radius * 0.3f), color * 1.2f);
            hub.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private static void CreateTrackLinks(
            Transform parent,
            string side,
            float x,
            float y,
            float length,
            float width,
            float radius,
            Color color,
            VehicleDefinition definition)
        {
            float frontZ = TankRunningGearLayout.TrackFrontZ(definition, length);
            float rearZ = TankRunningGearLayout.TrackRearZ(definition, length);
            float bottomY = TankRunningGearLayout.TrackBottomY(definition, y, radius);
            float topY = TankRunningGearLayout.TrackTopY(definition, y, radius);
            float pitch = Mathf.Clamp(width * 0.34f, 0.11f, 0.24f);
            int straightCount = Mathf.Clamp(Mathf.CeilToInt((frontZ - rearZ) / pitch), 18, 54);
            int arcCount = 7;
            int totalLinks = straightCount * 2 + arcCount * 2;
            Vector3[] vertices = new Vector3[totalLinks * 8];
            int[] triangles = new int[totalLinks * 36];
            int link = 0;
            for (int i = 0; i < straightCount; i++)
            {
                float t = (i + 0.5f) / straightCount;
                float z = Mathf.Lerp(rearZ, frontZ, t);
                AppendLink(vertices, triangles, link++, new Vector3(x, bottomY, z),
                    width, 0.09f, pitch * 0.78f, 0f);
                AppendLink(vertices, triangles, link++, new Vector3(x, topY,
                    Mathf.Lerp(frontZ, rearZ, t)),
                    width, 0.09f, pitch * 0.78f, 0f);
            }
            float centerY = (bottomY + topY) * 0.5f;
            float arcRadius = (topY - bottomY) * 0.5f;
            for (int end = -1; end <= 1; end += 2)
            {
                float centerZ = end < 0 ? rearZ : frontZ;
                for (int i = 0; i < arcCount; i++)
                {
                    float angle = -Mathf.PI * 0.5f +
                        (i + 0.5f) / arcCount * Mathf.PI;
                    float z = centerZ + end * Mathf.Cos(angle) * arcRadius;
                    float py = centerY + Mathf.Sin(angle) * arcRadius;
                    AppendLink(vertices, triangles, link++, new Vector3(x, py, z),
                        width, 0.09f, pitch * 0.72f, -end * angle * Mathf.Rad2Deg);
                }
            }
            Mesh mesh = new Mesh { name = "TrackLinks-" + side };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GameObject links = new GameObject("TrackLinks-" + side);
            links.transform.SetParent(parent, false);
            links.AddComponent<MeshFilter>().sharedMesh = mesh;
            links.AddComponent<MeshRenderer>().sharedMaterial =
                new Material(Shader.Find("Standard")) { color = color };
        }

        private static void AppendLink(
            Vector3[] vertices,
            int[] triangles,
            int index,
            Vector3 center,
            float width,
            float height,
            float length,
            float rotationX)
        {
            int vertex = index * 8;
            Quaternion rotation = Quaternion.Euler(rotationX, 0f, 0f);
            Vector3 half = new Vector3(width, height, length) * 0.5f;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 local = new Vector3(
                    (corner & 1) == 0 ? -half.x : half.x,
                    (corner & 2) == 0 ? -half.y : half.y,
                    (corner & 4) == 0 ? -half.z : half.z);
                vertices[vertex + corner] = center + rotation * local;
            }
            int triangle = index * 36;
            int[] faces =
            {
                0, 2, 3, 0, 3, 1, 4, 5, 7, 4, 7, 6,
                0, 1, 5, 0, 5, 4, 2, 6, 7, 2, 7, 3,
                0, 4, 6, 0, 6, 2, 1, 3, 7, 1, 7, 5
            };
            for (int i = 0; i < faces.Length; i++) triangles[triangle + i] = vertex + faces[i];
        }

        public void Sync(TankState tank)
        {
            _root.position = tank.Position.ToUnity();
            _root.rotation = Quaternion.Euler(
                -tank.HullPitchRad * Mathf.Rad2Deg,
                tank.Yaw * Mathf.Rad2Deg,
                0f);
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
            if (_root != null) DestroyObject(_root.gameObject);
            for (int i = 0; i < _meshes.Length; i++) DestroyObject(_meshes[i]);
            for (int i = 0; i < _materials.Length; i++) DestroyObject(_materials[i]);
            DestroyObject(_camouflageTexture);
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
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying) Object.Destroy(collider);
                else Object.DestroyImmediate(collider);
            }
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
