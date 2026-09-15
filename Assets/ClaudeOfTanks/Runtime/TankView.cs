using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class TankView
    {
        private readonly Transform _root;
        private readonly Transform _turret;
        private readonly Transform _recipeTurret;
        private readonly Transform _recipeGun;
        private readonly TankGunPitchRig _gunPitchRig;
        private readonly Renderer[] _renderers;
        private readonly Mesh[] _meshes;
        private readonly Material[] _materials;
        private readonly Color[] _materialColors;
        private readonly Texture2D _camouflageTexture;
        private readonly Color _aliveColor;
        private TankView(
            Transform root,
            Transform turret,
            Transform recipeTurret,
            Transform recipeGun,
            TankGunPitchRig gunPitchRig,
            Renderer[] renderers,
            Color aliveColor,
            Texture2D camouflageTexture)
        {
            _root = root;
            _turret = turret;
            _recipeTurret = recipeTurret;
            _recipeGun = recipeGun;
            _gunPitchRig = gunPitchRig;
            _renderers = renderers;
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            int generatedCount = 0;
            for (int i = 0; i < filters.Length; i++)
                if (TankViewPose.IsGeneratedMesh(filters[i]))
                    generatedCount++;
            _meshes = new Mesh[generatedCount];
            int generatedIndex = 0;
            for (int i = 0; i < filters.Length; i++)
                if (TankViewPose.IsGeneratedMesh(
                        filters[i]))
                    _meshes[generatedIndex++] = filters[i].sharedMesh;
            _materials = new Material[renderers.Length];
            _materialColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                _materials[i] = renderers[i].sharedMaterial;
                _materialColors[i] =
                    _materials[i] != null
                        ? _materials[i].color
                        : Color.white;
            }
            _camouflageTexture = camouflageTexture;
            _aliveColor = aliveColor;
        }

        public Transform Root => _root;

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
            GameObject turretRoot = new GameObject("TurretRoot");
            turretRoot.transform.SetParent(root.transform, false);
            turretRoot.transform.localPosition =
                TankAuthoredDetails.ResolveTurretPivot(
                    definition,
                    height,
                    length);
            bool hasRecipe =
                TankGeometryRecipeFactory.ShouldBuild(
                    definition);
            bool buildLegacy =
                !hasRecipe ||
                !Application.isPlaying;
            if (buildLegacy)
            {
                TankLegacyVisualFactory.Build(
                    root.transform,
                    turretRoot.transform,
                    definition,
                    teamColor,
                    width,
                    height,
                    length,
                    trackWidth);
            }
            Transform recipeTurret;
            Transform recipeGun;
            if (hasRecipe)
            {
                TankGeometryRecipeFactory.TryBuild(
                    root.transform,
                    definition,
                    out recipeTurret,
                    out recipeGun);
            }
            else
            {
                recipeTurret = null;
                recipeGun = null;
            }
            TankGunPitchRig gunPitchRig =
                TankGunPitchRig.Build(
                    turretRoot.transform,
                    definition);

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
                recipeTurret,
                recipeGun,
                gunPitchRig,
                renderers,
                camouflageTexture == null
                    ? teamColor
                    : Color.white,
                camouflageTexture);
            view.Sync(tank);
            return view;
        }

        public void Sync(TankState tank)
        {
            TankViewPose.Apply(
                _root,
                _turret,
                _gunPitchRig,
                tank);
            if (_recipeTurret != null)
            {
                _recipeTurret.localRotation =
                    Quaternion.Euler(
                        0f,
                        tank.TurretYaw *
                            Mathf.Rad2Deg,
                        0f);
            }
            if (_recipeGun != null)
            {
                _recipeGun.localRotation =
                    Quaternion.Euler(
                        -tank.GunPitchRad *
                            Mathf.Rad2Deg,
                        0f,
                        0f);
            }
            Color color = tank.Destroyed ? new Color(0.08f, 0.08f, 0.075f) : _aliveColor;
            for (int i = 0; i < _renderers.Length; i++)
            {
                string name =
                    _renderers[i].gameObject.name;
                if (name.StartsWith("Recipe-") ||
                    name.StartsWith("Painted-Recipe-"))
                {
                    Color source =
                        _materialColors[i];
                    _renderers[i].sharedMaterial.color =
                        tank.Destroyed
                            ? new Color(
                                source.r * 0.2f,
                                source.g * 0.2f,
                                source.b * 0.2f,
                                source.a)
                            : source;
                }
                else if (name == "Hull" ||
                    name == "UpperHull" ||
                    name == "Turret")
                {
                    _renderers[i].sharedMaterial.color = color;
                }
            }
        }

        public void Destroy()
        {
            TankGeneratedTextureOwner.ReleaseOwnedTextures(_root);
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
