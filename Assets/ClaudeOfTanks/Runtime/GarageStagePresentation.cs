using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class GarageStagePresentation : IDisposable
    {
        public const float PlatformTopY = 0.36f;

        private readonly List<Material> _materials =
            new List<Material>();
        private readonly Transform _root;

        private GarageStagePresentation(
            Transform parent,
            Camera camera)
        {
            GameObject root = new GameObject("WorkshopStage");
            root.transform.SetParent(parent, false);
            _root = root.transform;
            ConfigureCamera(camera);
            Build();
        }

        public static GarageStagePresentation Create(
            Transform parent,
            Camera camera)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));
            return new GarageStagePresentation(parent, camera);
        }

        public void Dispose()
        {
            for (int i = 0; i < _materials.Count; i++)
                ReleaseObject(_materials[i]);
            _materials.Clear();
        }

        private static void ConfigureCamera(Camera camera)
        {
            camera.transform.position =
                new Vector3(11.1f, 4.1f, 12f);
            camera.transform.rotation = Quaternion.LookRotation(
                new Vector3(0f, 1.6f, 0f) -
                camera.transform.position);
            camera.fieldOfView = 42f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(0.025f, 0.032f, 0.035f);
        }

        private void Build()
        {
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight =
                new Color(0.11f, 0.14f, 0.135f);

            Material floor = Material(
                "Garage Concrete",
                new Color(0.19f, 0.205f, 0.20f),
                0.03f,
                0.22f);
            Material steel = Material(
                "Workshop Steel",
                new Color(0.105f, 0.125f, 0.13f),
                0.55f,
                0.24f);
            Material darkSteel = Material(
                "Workshop Dark Steel",
                new Color(0.045f, 0.055f, 0.058f),
                0.7f,
                0.3f);
            Material deck = Material(
                "Turntable Deck",
                new Color(0.16f, 0.18f, 0.175f),
                0.62f,
                0.28f);
            Material accent = Material(
                "Safety Yellow",
                new Color(0.86f, 0.62f, 0.16f),
                0.2f,
                0.32f);
            Material service = Material(
                "Service Green",
                new Color(0.18f, 0.31f, 0.23f),
                0.32f,
                0.23f);
            Material lamp = Material(
                "Warm Lamp",
                new Color(1f, 0.82f, 0.56f),
                0f,
                0.55f,
                new Color(1f, 0.58f, 0.2f) * 2.2f);

            BuildFloor(floor, darkSteel);
            BuildTurntable(darkSteel, deck, accent);
            BuildShell(steel, darkSteel);
            BuildTrusses(darkSteel);
            BuildHighBayLights(darkSteel, lamp);
            BuildServiceProps(darkSteel, service, accent);
            BuildKeyLight();
        }

        private void BuildFloor(
            Material floor,
            Material darkSteel)
        {
            Transform group = Group("Floor");
            Primitive(
                "GarageFloor",
                PrimitiveType.Cube,
                group,
                new Vector3(0f, -0.12f, 0f),
                new Vector3(46f, 0.24f, 46f),
                floor);

            for (int i = -2; i <= 2; i++)
            {
                Primitive(
                    "ExpansionJoint-" + (i + 2),
                    PrimitiveType.Cube,
                    group,
                    new Vector3(i * 7.2f, 0.006f, 0f),
                    new Vector3(0.035f, 0.012f, 42f),
                    darkSteel,
                    false);
            }
        }

        private void BuildTurntable(
            Material darkSteel,
            Material deck,
            Material accent)
        {
            Transform group = Group("Turntable");
            Primitive(
                "TurntableBase",
                PrimitiveType.Cylinder,
                group,
                new Vector3(0f, 0.15f, 0f),
                new Vector3(6.35f, 0.15f, 6.35f),
                darkSteel);
            Primitive(
                "TurntableDeck",
                PrimitiveType.Cylinder,
                group,
                new Vector3(0f, 0.31f, 0f),
                new Vector3(6f, 0.05f, 6f),
                deck);

            Transform rim = Group("HazardRim", group);
            const int segmentCount = 24;
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f /
                    segmentCount;
                Primitive(
                    "HazardSegment-" + i.ToString("00"),
                    PrimitiveType.Cube,
                    rim,
                    new Vector3(
                        Mathf.Sin(angle) * 6.08f,
                        0.38f,
                        Mathf.Cos(angle) * 6.08f),
                    new Vector3(1.5f, 0.09f, 0.48f),
                    (i & 1) == 0 ? accent : darkSteel,
                    false,
                    Quaternion.Euler(
                        0f,
                        angle * Mathf.Rad2Deg,
                        0f));
            }
        }

        private void BuildShell(
            Material steel,
            Material darkSteel)
        {
            Transform shell = Group("WorkshopShell");
            Primitive(
                "RearWall",
                PrimitiveType.Cube,
                shell,
                new Vector3(0f, 4.1f, -13f),
                new Vector3(28f, 8.2f, 0.28f),
                steel);
            Primitive(
                "LeftWall",
                PrimitiveType.Cube,
                shell,
                new Vector3(-14f, 4.1f, 0f),
                new Vector3(0.28f, 8.2f, 26f),
                steel);
            Primitive(
                "RightWall",
                PrimitiveType.Cube,
                shell,
                new Vector3(14f, 4.1f, 0f),
                new Vector3(0.28f, 8.2f, 26f),
                steel);
            Primitive(
                "Ceiling",
                PrimitiveType.Cube,
                shell,
                new Vector3(0f, 8.2f, 0f),
                new Vector3(28f, 0.24f, 26f),
                darkSteel);

            for (int i = 0; i < 7; i++)
            {
                float x = -12f + i * 4f;
                Primitive(
                    "RearWallRib-" + i,
                    PrimitiveType.Cube,
                    shell,
                    new Vector3(x, 4.1f, -12.8f),
                    new Vector3(0.18f, 8f, 0.18f),
                    darkSteel,
                    false);
            }
        }

        private void BuildTrusses(Material material)
        {
            Transform trusses = Group("RoofTrusses");
            for (int i = 0; i < 3; i++)
            {
                float z = -8f + i * 7f;
                Primitive(
                    "CrossBeam-" + i,
                    PrimitiveType.Cube,
                    trusses,
                    new Vector3(0f, 7.65f, z),
                    new Vector3(27f, 0.22f, 0.24f),
                    material,
                    false);
                Primitive(
                    "BraceLeft-" + i,
                    PrimitiveType.Cube,
                    trusses,
                    new Vector3(-7f, 7.1f, z),
                    new Vector3(10f, 0.16f, 0.18f),
                    material,
                    false,
                    Quaternion.Euler(0f, 0f, 8f));
                Primitive(
                    "BraceRight-" + i,
                    PrimitiveType.Cube,
                    trusses,
                    new Vector3(7f, 7.1f, z),
                    new Vector3(10f, 0.16f, 0.18f),
                    material,
                    false,
                    Quaternion.Euler(0f, 0f, -8f));
            }
        }

        private void BuildHighBayLights(
            Material housing,
            Material lamp)
        {
            Transform lights = Group("HighBayLights");
            Vector3[] positions =
            {
                new Vector3(-5.5f, 7.2f, 1.5f),
                new Vector3(0f, 7.55f, -1.5f),
                new Vector3(5.5f, 7.2f, 1.5f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                Transform fixture =
                    Group("HighBay-" + (i + 1), lights);
                Primitive(
                    "Housing",
                    PrimitiveType.Cylinder,
                    fixture,
                    positions[i],
                    new Vector3(0.62f, 0.13f, 0.62f),
                    housing,
                    false);
                Primitive(
                    "Lens",
                    PrimitiveType.Cylinder,
                    fixture,
                    positions[i] + Vector3.down * 0.17f,
                    new Vector3(0.48f, 0.035f, 0.48f),
                    lamp,
                    false);
                GameObject lightObject =
                    new GameObject("WarmWorkshopLight");
                lightObject.transform.SetParent(fixture, false);
                lightObject.transform.position =
                    positions[i] + Vector3.down * 0.24f;
                lightObject.transform.rotation =
                    Quaternion.LookRotation(
                        new Vector3(0f, 0.25f, 0f) -
                        lightObject.transform.position);
                Light light =
                    lightObject.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = new Color(1f, 0.82f, 0.63f);
                light.intensity = i == 1 ? 2.1f : 1.55f;
                light.range = 18f;
                light.spotAngle = 74f;
                light.shadows = i == 1
                    ? LightShadows.Soft
                    : LightShadows.None;
            }
        }

        private void BuildServiceProps(
            Material darkSteel,
            Material service,
            Material accent)
        {
            Transform props = Group("ServiceProps");
            for (int i = 0; i < 4; i++)
            {
                Primitive(
                    "ServiceCabinet-" + i,
                    PrimitiveType.Cube,
                    props,
                    new Vector3(-11.8f + i * 1.45f, 1.15f, -12.1f),
                    new Vector3(1.15f, 2.3f, 0.65f),
                    i == 1 ? service : darkSteel);
            }
            Primitive(
                "ToolBench",
                PrimitiveType.Cube,
                props,
                new Vector3(9.6f, 0.72f, -11.7f),
                new Vector3(4.4f, 1.35f, 1.1f),
                service);
            Primitive(
                "SafetyRail",
                PrimitiveType.Cube,
                props,
                new Vector3(9.6f, 1.55f, -11.15f),
                new Vector3(4.5f, 0.12f, 0.12f),
                accent,
                false);
            for (int i = 0; i < 3; i++)
            {
                Primitive(
                    "SupplyCrate-" + i,
                    PrimitiveType.Cube,
                    props,
                    new Vector3(
                        -11.6f + i * 1.35f,
                        0.42f,
                        7.4f + (i & 1) * 1.1f),
                    new Vector3(1.15f, 0.82f, 1.15f),
                    service);
            }
        }

        private void BuildKeyLight()
        {
            GameObject keyObject = new GameObject("GarageKey");
            keyObject.transform.SetParent(_root, false);
            keyObject.transform.rotation =
                Quaternion.Euler(38f, 145f, 0f);
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.78f, 0.85f, 0.82f);
            key.intensity = 0.78f;
            key.shadows = LightShadows.Soft;

            GameObject fillObject =
                new GameObject("GarageFill");
            fillObject.transform.SetParent(_root, false);
            fillObject.transform.rotation =
                Quaternion.Euler(28f, -35f, 0f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.50f, 0.63f, 0.68f);
            fill.intensity = 0.28f;
            fill.shadows = LightShadows.None;
        }

        private Transform Group(
            string name,
            Transform parent = null)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent ?? _root, false);
            return group.transform;
        }

        private Material Material(
            string name,
            Color color,
            float metallic,
            float glossiness,
            Color? emission = null)
        {
            Material material = new Material(
                Shader.Find("Standard"))
            {
                name = name,
                color = color
            };
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", glossiness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor(
                    "_EmissionColor",
                    emission.Value);
            }
            _materials.Add(material);
            return material;
        }

        private static Transform Primitive(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool castsShadows = true,
            Quaternion? rotation = null)
        {
            GameObject value = GameObject.CreatePrimitive(type);
            value.name = name;
            value.transform.SetParent(parent, false);
            value.transform.localPosition = position;
            value.transform.localRotation =
                rotation ?? Quaternion.identity;
            value.transform.localScale = scale;
            Renderer renderer = value.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castsShadows
                ? ShadowCastingMode.On
                : ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            Collider collider = value.GetComponent<Collider>();
            if (collider != null) ReleaseObject(collider);
            return value.transform;
        }

        private static void ReleaseObject(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
