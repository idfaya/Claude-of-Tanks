using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class SceneStudioController :
        MonoBehaviour
    {
        public const int MaximumActors = 12;
        private readonly List<StudioActor>
            _actors =
                new List<StudioActor>();
        private ContentCatalog _catalog;
        private Action _exit;
        private Camera _camera;
        private MapRuntime _map;
        private IHeightField _heightField;
        private SceneStudioPanel _panel;
        private string _mapId;
        private int _selectedIndex = -1;
        private int _nextActorId = 1;

        public int ActorCount => _actors.Count;
        public int SelectedIndex => _selectedIndex;
        public string MapId => _mapId;

        public void Configure(
            ContentCatalog catalog,
            string vehicleId,
            string mapId,
            Action exit)
        {
            if (_catalog != null)
                return;
            _catalog = catalog ??
                throw new ArgumentNullException(
                    nameof(catalog));
            _exit = exit;
            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject =
                    new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                _camera =
                    cameraObject.AddComponent<
                        Camera>();
                cameraObject.AddComponent<
                    AudioListener>();
            }
            SetMap(mapId);
            ConfigureCamera();
            AddActor(
                string.IsNullOrEmpty(vehicleId)
                    ? _catalog
                        .ProductionVehicleIds[0]
                    : vehicleId);
            _panel = SceneStudioPanel.Create(
                transform,
                this,
                _catalog);
        }

        public StudioActorSnapshot ActorAt(int index)
        {
            if (index < 0 ||
                index >= _actors.Count)
            {
                return null;
            }
            StudioActor actor = _actors[index];
            return new StudioActorSnapshot
            {
                Id = actor.Id,
                VehicleId = actor.VehicleId,
                CamouflageId =
                    actor.CamouflageId,
                Position = actor.State.Position,
                HullYawDeg =
                    actor.State.Yaw /
                    MathUtil.Deg2Rad,
                TurretYawDeg =
                    actor.State.TurretYaw /
                    MathUtil.Deg2Rad,
                GunPitchDeg =
                    actor.State.GunPitchRad /
                    MathUtil.Deg2Rad,
                Destroyed =
                    actor.State.Destroyed
            };
        }

        public void SelectActor(int index)
        {
            _selectedIndex =
                _actors.Count == 0
                    ? -1
                    : Math.Max(
                        0,
                        Math.Min(
                            index,
                            _actors.Count - 1));
            _panel?.Refresh();
        }

        public bool AddActor(string vehicleId)
        {
            if (_actors.Count >=
                    MaximumActors ||
                !_catalog.ContainsVehicle(
                    vehicleId))
            {
                return false;
            }
            VehicleDefinition definition =
                _catalog.GetVehicle(vehicleId);
            int column =
                _actors.Count % 4;
            int row =
                _actors.Count / 4;
            Float3 position = new Float3(
                (column - 1.5f) * 8f,
                0f,
                row * 10f);
            position.Y =
                _heightField.HeightAt(
                    position.X,
                    position.Z);
            TankState state = new TankState(
                "studio-" + _nextActorId++,
                Team.Alpha,
                definition.ToTankSpec(),
                position,
                0f);
            StudioActor actor =
                new StudioActor
                {
                    Id = state.Id,
                    VehicleId = vehicleId,
                    CamouflageId = "factory",
                    State = state,
                    Definition = definition,
                    View = TankView.Create(
                        state,
                        definition,
                        "factory",
                        _mapId,
                        _catalog)
                };
            actor.View.Root.SetParent(
                transform,
                true);
            _actors.Add(actor);
            _selectedIndex =
                _actors.Count - 1;
            _panel?.Refresh();
            return true;
        }

        public bool RemoveSelectedActor()
        {
            if (_selectedIndex < 0 ||
                _selectedIndex >= _actors.Count)
            {
                return false;
            }
            _actors[_selectedIndex]
                .View.Destroy();
            _actors.RemoveAt(_selectedIndex);
            _selectedIndex =
                Math.Min(
                    _selectedIndex,
                    _actors.Count - 1);
            _panel?.Refresh();
            return true;
        }

        public void SetSelectedPose(
            float x,
            float z,
            float hullYawDeg,
            float turretYawDeg,
            float gunPitchDeg)
        {
            StudioActor actor =
                Selected();
            if (actor == null) return;
            actor.State.Position =
                new Float3(
                    x,
                    _heightField.HeightAt(x, z),
                    z);
            actor.State.Yaw =
                hullYawDeg *
                MathUtil.Deg2Rad;
            actor.State.TurretYaw =
                turretYawDeg *
                MathUtil.Deg2Rad;
            actor.State.GunPitchRad =
                MathUtil.Clamp(
                    gunPitchDeg,
                    -actor.State.Spec
                        .GunDepressionDeg,
                    actor.State.Spec
                        .GunElevationDeg) *
                MathUtil.Deg2Rad;
            actor.View.Sync(actor.State);
        }

        public void SetSelectedCamouflage(
            string camouflageId)
        {
            StudioActor actor = Selected();
            if (actor == null) return;
            actor.CamouflageId =
                string.IsNullOrEmpty(
                    camouflageId)
                    ? "factory"
                    : camouflageId;
            actor.View.Destroy();
            actor.View = TankView.Create(
                actor.State,
                actor.Definition,
                actor.CamouflageId,
                _mapId,
                _catalog);
            actor.View.Root.SetParent(
                transform,
                true);
        }

        public void SetSelectedDestroyed(
            bool destroyed)
        {
            StudioActor actor = Selected();
            if (actor == null) return;
            actor.State.Destroyed = destroyed;
            actor.State.Combat.Destroyed =
                destroyed;
            actor.View.Sync(actor.State);
        }

        public void SetMap(string mapId)
        {
            MapDefinition definition =
                _catalog.GetMap(mapId);
            _map?.Dispose();
            _map = MapRuntime.Create(definition);
            _map.Root.SetParent(transform, false);
            _heightField =
                MapSimulationAdapter
                    .BuildHeightField(
                        definition);
            CameraPostProcessing.Ensure(
                    _camera,
                    GameSettings.Current)
                .ApplyMap(definition);
            _mapId = mapId;
            for (int i = 0;
                i < _actors.Count;
                i++)
            {
                StudioActor actor = _actors[i];
                Float3 position =
                    actor.State.Position;
                position.Y =
                    _heightField.HeightAt(
                        position.X,
                        position.Z);
                actor.State.Position = position;
                actor.View.Sync(actor.State);
            }
        }

        public void SetCamera(
            float distance,
            float height,
            float orbitDeg,
            float fieldOfView)
        {
            float radians =
                orbitDeg *
                MathUtil.Deg2Rad;
            Vector3 focus =
                Selected()?.State.Position
                    .ToUnity() ??
                Vector3.zero;
            _camera.transform.position =
                focus +
                new Vector3(
                    Mathf.Sin(radians) *
                        distance,
                    height,
                    Mathf.Cos(radians) *
                        distance);
            _camera.transform.rotation =
                Quaternion.LookRotation(
                    focus +
                    Vector3.up * 1.5f -
                    _camera.transform
                        .position);
            _camera.fieldOfView =
                Mathf.Clamp(
                    fieldOfView,
                    15f,
                    80f);
        }

        public void Exit()
        {
            _exit?.Invoke();
        }

        private void Update()
        {
            if (_map != null &&
                _camera != null)
            {
                _map.UpdateWorldStreaming(
                    _camera.transform.position);
            }
        }

        private StudioActor Selected()
        {
            return _selectedIndex >= 0 &&
                _selectedIndex <
                    _actors.Count
                    ? _actors[_selectedIndex]
                    : null;
        }

        private void ConfigureCamera()
        {
            RenderSettings.fog = true;
            _camera.nearClipPlane = 0.15f;
            _camera.farClipPlane = 1400f;
            _camera.clearFlags = CameraClearFlags.Skybox;
            SetCamera(22f, 8f, 35f, 42f);
        }

        private void OnDestroy()
        {
            for (int i = 0;
                i < _actors.Count;
                i++)
            {
                _actors[i].View.Destroy();
            }
            _actors.Clear();
            _map?.Dispose();
            _map = null;
        }

        private sealed class StudioActor
        {
            public string Id;
            public string VehicleId;
            public string CamouflageId;
            public VehicleDefinition Definition;
            public TankState State;
            public TankView View;
        }

    }

    public sealed class StudioActorSnapshot
    {
        public string Id;
        public string VehicleId;
        public string CamouflageId;
        public Float3 Position;
        public float HullYawDeg;
        public float TurretYawDeg;
        public float GunPitchDeg;
        public bool Destroyed;
    }
}
