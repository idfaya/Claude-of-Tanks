using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ClaudeOfTanks.Runtime
{
    [DefaultExecutionOrder(-100)]
    public sealed class BattleController : MonoBehaviour
    {
        private readonly Dictionary<string, TankInput> _inputs = new Dictionary<string, TankInput>();
        private readonly Dictionary<string, TankView> _tankViews = new Dictionary<string, TankView>();
        private readonly Dictionary<string, VehicleDefinition> _vehicleDefinitions =
            new Dictionary<string, VehicleDefinition>();
        private readonly Dictionary<string, string> _camouflageIds =
            new Dictionary<string, string>();
        private readonly Dictionary<int, GameObject> _shellViews = new Dictionary<int, GameObject>();
        private readonly List<int> _staleShellIds = new List<int>();
        private readonly BattleCameraRig _cameraRig = new BattleCameraRig();
        private BattleSimulation _simulation;
        private BattleReplayRecorder _replayRecorder;
        private BattleReplaySession _replaySession;
        private BattleSimulation _liveSimulation;
        private TankState _livePlayer;
        private BotController _botController;
        private readonly SpottingSimulation _hudSpotting = new SpottingSimulation();
        private TankState _player;
        private Camera _camera;
        private ContentCatalog _catalog;
        private MapRuntime _mapRuntime;
        private MatchModeWorldView _modeWorldView;
        private BattleHud _hud;
        private BattleEffects _effects;
        private BattleAudio _audio;
        private ReplayArchive _replayArchive;
        [SerializeField] private string mapId = "verdant";
        [SerializeField] private GameModeId gameMode = GameModeId.Standard;
        [SerializeField] private string vehicleId = "m1a2";
        private string[] _playerEquipment = Array.Empty<string>();
        private string _playerCamouflageId = "factory";
        private Action _returnToGarage;
        private Vector3 _cameraAimPoint;
        private bool _aimHeldLastFrame;
        private bool _aimHoldOwnsSniper;
        private int _shotsFired;
        private int _hits;
        private int _penetrations;
        private float _damageDealt;
        private float _damageReceived;
        private float _accumulator;
        private float _replayStartTimeS;
        private bool _replayIsKillcam;
        private bool _archivePlayback;
        private bool _archiveWritten;
        private string _archivedReplayId;
        private string _status = "BATTLE";
        private float _statusUntil;
        private GUIStyle _labelStyle;
        private GUIStyle _statusStyle;

        public string VehicleId => vehicleId;
        public string MapId => mapId;
        public GameModeId GameMode => gameMode;
        public TankState Player => _player;
        public BattleState State => _simulation?.State;
        public MatchModeState MatchMode => _simulation?.MatchMode;
        public bool IsReplaying => _replaySession != null;
        public string ArchivedReplayId => _archivedReplayId;
        public string PlayerCamouflageId
        {
            get
            {
                if (_player == null) return null;
                return _camouflageIds.TryGetValue(
                    _player.Id,
                    out string camouflageId)
                        ? camouflageId
                        : null;
            }
        }

        public void Configure(
            string selectedVehicleId, string selectedMapId, GameModeId selectedMode,
            Action returnToGarage,
            ReplayArchive replayArchive = null,
            string[] equipment = null,
            string camouflageId = null)
        {
            vehicleId = selectedVehicleId;
            mapId = selectedMapId;
            gameMode = selectedMode;
            _returnToGarage = returnToGarage;
            _replayArchive = replayArchive ?? ReplayArchive.Current;
            _playerEquipment = equipment == null
                ? Array.Empty<string>()
                : (string[])equipment.Clone();
            _playerCamouflageId = string.IsNullOrEmpty(camouflageId)
                ? "factory"
                : camouflageId;
        }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_catalog != null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                InputSystem.AddDevice<Keyboard>();
            }
            if (Mouse.current == null)
            {
                InputSystem.AddDevice<Mouse>();
            }
#endif
            _catalog = ContentCatalog.Load();
            BuildEnvironment();
            StartBattle();
            _hud = BattleHud.Create(
                StartBattle,
                _returnToGarage,
                parent: transform);
            _hud.SetMap(_catalog.GetMap(mapId));
            _hud.ConfigureReplayActions(
                () => StartReplay(true),
                () => StartReplay(false),
                ExitReplay);
            _effects = BattleEffects.Create();
            _effects.transform.SetParent(transform, false);
            _audio = BattleAudio.Create();
            _audio.transform.SetParent(transform, false);
        }

        private void Update()
        {
            if (_simulation == null)
            {
                return;
            }
            if (_replaySession != null)
            {
                UpdateReplay();
                return;
            }

            UpdateCameraControls();
            if (!IsBattleOver())
            {
                float frameTime = Mathf.Min(Time.deltaTime, 0.25f);
                _accumulator += frameTime;
                TankInput playerInput = ReadPlayerInput();
                while (_accumulator >= BattleState.FixedDeltaTime)
                {
                    BuildInputs(playerInput);
                    _replayRecorder.Record(_inputs, BattleState.FixedDeltaTime);
                    _simulation.Step(_inputs, BattleState.FixedDeltaTime);
                    playerInput.ToggleHydropneumaticAim = false;
                    ConsumeEvents(false);
                    _accumulator -= BattleState.FixedDeltaTime;
                }
            }

            SyncViews();
            CheckResult();
            _hud.SetState(
                _player,
                _simulation.MatchMode,
                Time.unscaledTime < _statusUntil || IsBattleOver() ? _status : string.Empty,
                IsBattleOver(),
                new BattleHudStats
                {
                    ShotsFired = _shotsFired,
                    Hits = _hits,
                    Penetrations = _penetrations,
                    DamageDealt = _damageDealt,
                    DamageReceived = _damageReceived,
                    Kills = _player.Kills,
                    TimeS = _simulation.State.TimeS
                });
            _hud.SetCamera(_cameraRig.Mode, _cameraRig.Zoom);
            _hud.SetMinimap(
                _player,
                _simulation.State.Tanks,
                _simulation.MatchMode,
                _hudSpotting,
                _simulation.State.IsVisionOccluded);
            if (Input.GetKeyDown(KeyCode.Return) && IsBattleOver())
            {
                StartBattle();
            }
        }

        private void LateUpdate()
        {
            if (_camera == null || _player == null)
            {
                return;
            }

            _cameraRig.Apply(_camera, _player, _cameraAimPoint, Time.deltaTime);
            _mapRuntime?.UpdateVegetationVisibility(_camera.transform.position);
            _effects?.SyncPersistent(_simulation.State.Tanks);
            _audio?.SetKillcamDucking(
                _replaySession != null && _replayIsKillcam);
            _audio?.SyncEngines(
                _simulation.State.Tanks,
                _player.Id,
                _camera.transform.position,
                _cameraRig.Mode == BattleCameraMode.Sniper);
            _audio?.SyncAwareness(
                _simulation.State.Tanks,
                _player.Id,
                _simulation.State.IsVisionOccluded);
            TankView playerView;
            if (_tankViews.TryGetValue(_player.Id, out playerView))
            {
                bool visible = _cameraRig.Mode != BattleCameraMode.Sniper;
                if (playerView.Root.gameObject.activeSelf != visible)
                {
                    playerView.Root.gameObject.SetActive(visible);
                }
            }
        }

        private void StartBattle()
        {
            _replaySession = null;
            _liveSimulation = null;
            _livePlayer = null;
            _archivePlayback = false;
            _archiveWritten = false;
            _archivedReplayId = null;
            _hud?.SetReplayState(false, false, 0f, 0f, false);
            _effects?.ResetAll();
            _audio?.ResetAll();
            _audio?.BeginBattle();
            _modeWorldView?.Dispose();
            _modeWorldView = null;
            foreach (TankView view in _tankViews.Values)
            {
                view.Destroy();
            }

            foreach (GameObject shell in _shellViews.Values)
            {
                DestroyShellView(shell);
            }

            _tankViews.Clear();
            _shellViews.Clear();
            _inputs.Clear();
            _vehicleDefinitions.Clear();
            _camouflageIds.Clear();

            MapDefinition map = _catalog.GetMap(mapId);
            IHeightField heightField = MapSimulationAdapter.BuildHeightField(map);
            BattleState state = new BattleState(
                heightField,
                6000u,
                500f,
                MapSimulationAdapter.BuildStaticObstacles(map, heightField));
            MapPoint playerSpawn = map.spawns.player;
            AddTank(state, "player", Team.Alpha, vehicleId,
                SpawnPosition(state, playerSpawn.x, playerSpawn.z), 0f,
                _playerEquipment,
                _playerCamouflageId);
            AddTank(state, "alpha-2", Team.Alpha, "challenger2",
                SpawnPosition(state, playerSpawn.x - 13f, playerSpawn.z - 8f), 0.1f,
                Array.Empty<string>(),
                "auto");
            string[] enemies = { "t90m", "type99a", "leo2a6" };
            for (int i = 0; i < enemies.Length; i++)
            {
                MapPoint spawn = map.spawns.enemies[i];
                AddTank(state, "bravo-" + (i + 1), Team.Bravo, enemies[i],
                    SpawnPosition(state, spawn.x, spawn.z), MathUtil.Pi,
                    Array.Empty<string>(),
                    "auto");
            }
            _simulation = new BattleSimulation(state, gameMode);
            _modeWorldView = MatchModeWorldView.Create(transform);
            _replayRecorder = new BattleReplayRecorder(
                state,
                gameMode,
                _camouflageIds);
            _botController = new BotController(
                new SpottingSimulation(),
                state.IsVisionOccluded,
                state,
                _simulation.BotTarget);
            _player = state.Tanks[0];
            _cameraRig.Reset();
            _cameraAimPoint = _player.Position.ToUnity() +
                new Vector3(0f, 1.6f, 100f);
            _aimHeldLastFrame = false;
            _aimHoldOwnsSniper = false;
            _shotsFired = 0;
            _hits = 0;
            _penetrations = 0;
            _damageDealt = 0f;
            _damageReceived = 0f;
            for (int i = 0; i < state.Tanks.Count; i++)
            {
                TankState tank = state.Tanks[i];
                _tankViews.Add(
                    tank.Id,
                    TankView.Create(
                        tank,
                        _vehicleDefinitions[tank.Id],
                        _camouflageIds[tank.Id],
                        mapId,
                        _catalog));
            }

            _accumulator = 0f;
            _status = "BATTLE";
            _statusUntil = Time.unscaledTime + 1.5f;
        }

        public void StartReplay(bool killcam)
        {
            if (!IsBattleOver() || _replayRecorder == null || _replayRecorder.Recording.FrameCount == 0)
                return;
            _liveSimulation = _simulation;
            _livePlayer = _player;
            _archivePlayback = false;
            _replaySession = new BattleReplaySession(_replayRecorder.Recording);
            _replayIsKillcam = killcam;
            _replayStartTimeS = killcam
                ? Mathf.Max(0f, _replaySession.DurationS - 8f)
                : 0f;
            _replaySession.SeekTime(_replayStartTimeS);
            _simulation = _replaySession.Simulation;
            _player = FindTank(_simulation.State, _livePlayer.Id);
            _accumulator = 0f;
            _effects?.ResetAll();
            _audio?.ResetAll();
            _audio?.BeginBattle(false);
            ClearShellViews();
            _modeWorldView?.Sync(_simulation.MatchMode);
            SyncViews();
            _hud.SetReplayState(
                true,
                killcam,
                0f,
                _replaySession.DurationS - _replayStartTimeS,
                _replaySession.Complete);
        }

        public void ExitReplay()
        {
            if (_replaySession == null) return;
            if (_archivePlayback)
            {
                _returnToGarage?.Invoke();
                return;
            }
            _simulation = _liveSimulation;
            _player = _livePlayer;
            _replaySession = null;
            _liveSimulation = null;
            _livePlayer = null;
            _accumulator = 0f;
            _effects?.ResetAll();
            _audio?.ResetAll();
            _audio?.BeginBattle(false);
            ClearShellViews();
            _modeWorldView?.Sync(_simulation.MatchMode);
            SyncViews();
            _hud.SetReplayState(false, false, 0f, 0f, false);
        }

        public void LoadArchivedReplay(ArchivedReplay archived)
        {
            if (archived == null || archived.Entry == null || archived.Recording == null)
                throw new ArgumentNullException(nameof(archived));
            if (archived.Entry.MapId != mapId ||
                archived.Entry.GameMode != gameMode)
                throw new InvalidOperationException("Archived replay does not match battle configuration.");

            _effects?.ResetAll();
            _audio?.ResetAll();
            _audio?.BeginBattle(false);
            foreach (TankView view in _tankViews.Values) view.Destroy();
            _tankViews.Clear();
            ClearShellViews();
            _vehicleDefinitions.Clear();
            _camouflageIds.Clear();
            for (int i = 0; i < archived.Recording.TankCount; i++)
            {
                string entityId = archived.Recording.GetTankId(i);
                string specId = archived.Recording.GetTankSpecId(i);
                VehicleDefinition definition = _catalog.GetVehicle(specId);
                _vehicleDefinitions.Add(entityId, definition);
                _camouflageIds.Add(
                    entityId,
                    archived.Recording.GetTankCamouflageId(i));
            }

            _replaySession = new BattleReplaySession(archived.Recording);
            _simulation = _replaySession.Simulation;
            _player = FindTank(_simulation.State, archived.Entry.PlayerEntityId);
            _liveSimulation = null;
            _livePlayer = null;
            _archivePlayback = true;
            _replayIsKillcam = false;
            _replayStartTimeS = 0f;
            _accumulator = 0f;
            _cameraRig.Reset();
            _cameraAimPoint = _player.Position.ToUnity() + new Vector3(0f, 1.6f, 100f);
            if (_modeWorldView == null)
                _modeWorldView = MatchModeWorldView.Create(transform);
            for (int i = 0; i < _simulation.State.Tanks.Count; i++)
            {
                TankState tank = _simulation.State.Tanks[i];
                _tankViews.Add(
                    tank.Id,
                    TankView.Create(
                        tank,
                        _vehicleDefinitions[tank.Id],
                        _camouflageIds[tank.Id],
                        mapId,
                        _catalog));
            }
            SyncViews();
            _hud.SetReplayState(true, false, 0f, _replaySession.DurationS, false);
        }

        private void UpdateReplay()
        {
            _accumulator += Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            UpdateCameraControls();
            while (_accumulator >= BattleState.FixedDeltaTime && !_replaySession.Complete)
            {
                _replaySession.Step();
                ConsumeEvents(true);
                _accumulator -= BattleState.FixedDeltaTime;
            }
            SyncViews();
            _hud.SetState(
                _player,
                _simulation.MatchMode,
                string.Empty,
                false,
                new BattleHudStats
                {
                    ShotsFired = _shotsFired,
                    Hits = _hits,
                    Penetrations = _penetrations,
                    DamageDealt = _damageDealt,
                    DamageReceived = _damageReceived,
                    Kills = _player.Kills,
                    TimeS = _simulation.State.TimeS
                });
            _hud.SetCamera(_cameraRig.Mode, _cameraRig.Zoom);
            _hud.SetMinimap(
                _player,
                _simulation.State.Tanks,
                _simulation.MatchMode,
                _hudSpotting,
                _simulation.State.IsVisionOccluded);
            _hud.SetReplayState(
                true,
                _replayIsKillcam,
                Mathf.Max(0f, _replaySession.CurrentTimeS - _replayStartTimeS),
                _replaySession.DurationS - _replayStartTimeS,
                _replaySession.Complete);
        }

        private void ClearShellViews()
        {
            foreach (GameObject shell in _shellViews.Values) DestroyShellView(shell);
            _shellViews.Clear();
        }

        private static TankState FindTank(BattleState state, string entityId)
        {
            for (int i = 0; i < state.Tanks.Count; i++)
                if (state.Tanks[i].Id == entityId) return state.Tanks[i];
            throw new InvalidOperationException("Replay is missing player entity " + entityId + ".");
        }

        private void OnDestroy()
        {
            _mapRuntime?.Dispose();
            _modeWorldView?.Dispose();
            foreach (TankView view in _tankViews.Values) view.Destroy();
            foreach (GameObject shell in _shellViews.Values) DestroyShellView(shell);
        }

        private void AddTank(
            BattleState state,
            string entityId,
            Team team,
            string vehicleId,
            Float3 position,
            float yaw,
            string[] equipment,
            string camouflageId)
        {
            VehicleDefinition definition = _catalog.GetVehicle(vehicleId);
            TankState tank = new TankState(
                entityId,
                team,
                definition.ToTankSpec(),
                position,
                yaw);
            LoadoutSimulation.ApplyEquipment(
                tank,
                equipment ?? Array.Empty<string>());
            tank.CamouflagePaintBonus =
                CamouflageSpottingPolicy.Bonus(
                    camouflageId,
                    mapId);
            state.Tanks.Add(tank);
            _vehicleDefinitions[entityId] = definition;
            _camouflageIds[entityId] =
                string.IsNullOrEmpty(camouflageId)
                    ? "factory"
                    : camouflageId;
        }

        private TankInput ReadPlayerInput()
        {
            float throttle = 0f;
            float steer = 0f;
            GameSettings settings = GameSettings.Current;
            if (settings.IsPressed(GameInputAction.Forward)) throttle += 1f;
            if (settings.IsPressed(GameInputAction.Reverse)) throttle -= 1f;
            if (settings.IsPressed(GameInputAction.Right)) steer += 1f;
            if (settings.IsPressed(GameInputAction.Left)) steer -= 1f;
            BattleGamepadFrame gamepad = BattleGamepadInput.Read();
            steer += gamepad.Steer;
            throttle += gamepad.Throttle;
            if (_hud != null)
            {
                steer += _hud.TouchDrive.x;
                throttle += _hud.TouchDrive.y;
            }

            Float3 aimPoint = _player.Position + Float3.Forward(_player.Yaw + _player.TurretYaw) * 100f;
            if (_camera != null)
            {
                Ray ray = _camera.ScreenPointToRay(PointerPosition());
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                float distance;
                if (ground.Raycast(ray, out distance))
                {
                    aimPoint = ray.GetPoint(distance).ToSimulation();
                }
            }
            _cameraAimPoint = aimPoint.ToUnity();

            return new TankInput
            {
                Throttle = Mathf.Clamp(throttle, -1f, 1f),
                Steer = Mathf.Clamp(steer, -1f, 1f),
                Brake = gamepad.Brake || (_hud != null && _hud.BrakeHeld) ||
                    settings.IsPressed(GameInputAction.Brake),
                Fire = gamepad.Fire || (_hud != null && _hud.FireHeld) ||
                    IsPrimaryButtonPressed() || settings.IsPressed(GameInputAction.Fire),
                UseRepairKit = gamepad.RepairPressed ||
                    settings.WasPressedThisFrame(GameInputAction.Repair) ||
                    (_hud != null && _hud.ConsumeConsumable(0)),
                UseFirstAidKit = gamepad.FirstAidPressed ||
                    settings.WasPressedThisFrame(GameInputAction.FirstAid) ||
                    (_hud != null && _hud.ConsumeConsumable(1)),
                UseFireExtinguisher = gamepad.ExtinguisherPressed ||
                    settings.WasPressedThisFrame(GameInputAction.Extinguisher) ||
                    (_hud != null && _hud.ConsumeConsumable(2)),
                SpecialAction =
                    gamepad
                        .HydropneumaticAimPressed ||
                    settings.WasPressedThisFrame(
                        GameInputAction
                            .HydropneumaticAim) ||
                    (_hud != null &&
                     _hud
                         .ConsumeHydropneumaticToggle()),
                ShellSlot = _hud.ConsumeShellSlot(
                    _player.Combat.ShellSlot,
                    _player.Spec.Shells.Length),
                AimPoint = aimPoint
            };
        }

        private static bool IsPrimaryButtonPressed()
        {
            bool pressed = Input.GetMouseButton(0);
#if ENABLE_INPUT_SYSTEM
            pressed |= Mouse.current != null && Mouse.current.leftButton.isPressed;
#endif
            return pressed;
        }

        private void UpdateCameraControls()
        {
            if (BattleGamepadInput.Read().SniperPressed ||
                GameSettings.Current.WasPressedThisFrame(GameInputAction.Sniper) ||
                (_hud != null && _hud.ConsumeSniperToggle()))
            {
                _cameraRig.ToggleSniper();
                _aimHoldOwnsSniper = false;
            }

            int zoomSteps = ReadZoomSteps();
            if (zoomSteps != 0)
            {
                _cameraRig.StepZoom(zoomSteps);
                _aimHoldOwnsSniper = false;
            }

            bool aimHeld = IsSecondaryButtonPressed();
            if (aimHeld && !_aimHeldLastFrame &&
                _cameraRig.Mode == BattleCameraMode.Arcade)
            {
                _cameraRig.ToggleSniper();
                _aimHoldOwnsSniper = true;
            }
            else if (!aimHeld && _aimHeldLastFrame && _aimHoldOwnsSniper)
            {
                if (_cameraRig.Mode == BattleCameraMode.Sniper)
                {
                    _cameraRig.ToggleSniper();
                }
                _aimHoldOwnsSniper = false;
            }
            _aimHeldLastFrame = aimHeld;
        }

        private static bool IsSecondaryButtonPressed()
        {
            bool pressed = Input.GetMouseButton(1);
#if ENABLE_INPUT_SYSTEM
            pressed |= Mouse.current != null && Mouse.current.rightButton.isPressed;
#endif
            return pressed;
        }

        private static int ReadZoomSteps()
        {
            float scroll = Input.mouseScrollDelta.y;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                float inputSystemScroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(inputSystemScroll) > Mathf.Abs(scroll))
                {
                    scroll = inputSystemScroll;
                }
            }
#endif
            return scroll > 0.01f ? 1 : scroll < -0.01f ? -1 : 0;
        }

        private Vector2 PointerPosition()
        {
            Vector2 touchPosition;
            if (_hud != null && _hud.TryGetTouchAimPosition(out touchPosition))
            {
                return touchPosition;
            }
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
#endif
            return Input.mousePosition;
        }

        private void BuildInputs(TankInput playerInput)
        {
            _inputs.Clear();
            _inputs[_player.Id] = playerInput;
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 1; i < tanks.Count; i++)
            {
                TankState bot = tanks[i];
                if (bot.Destroyed ||
                    !bot.ModeActive)
                {
                    continue;
                }

                _inputs[bot.Id] = _botController.Decide(bot, tanks);
            }
        }

        private void ConsumeEvents(bool replay)
        {
            List<BattleEvent> events = _simulation.State.Events;
            for (int i = 0; i < events.Count; i++)
            {
                BattleEvent battleEvent = events[i];
                _audio?.Play(
                    battleEvent,
                    replay ? null : _player.Id,
                    replay ? null : _player);
                if (!replay &&
                    battleEvent.Type == BattleEventType.ShellFired &&
                    battleEvent.SourceId == _player.Id)
                {
                    _shotsFired++;
                }
                else if (!replay && battleEvent.Type == BattleEventType.ShellHit)
                {
                    if (battleEvent.SourceId == _player.Id)
                    {
                        _hits++;
                        if (battleEvent.Penetrated) _penetrations++;
                        _damageDealt += battleEvent.Value;
                    }
                    if (battleEvent.TargetId == _player.Id)
                    {
                        _damageReceived += battleEvent.Value;
                    }
                }
                if (battleEvent.Type == BattleEventType.ShellFired ||
                    battleEvent.Type == BattleEventType.ShellHit ||
                    battleEvent.Type == BattleEventType.StructureHit ||
                    battleEvent.Type == BattleEventType.StructureDestroyed ||
                    battleEvent.Type == BattleEventType.PropCrushed ||
                    battleEvent.Type == BattleEventType.TankDestroyed)
                {
                    TankView target;
                    Transform targetTransform =
                        !string.IsNullOrEmpty(battleEvent.TargetId) &&
                        _tankViews.TryGetValue(battleEvent.TargetId, out target)
                            ? target.Root
                            : null;
                    _effects.Play(battleEvent, targetTransform);
                }
                if (battleEvent.Type == BattleEventType.ShellHit)
                {
                    _status = battleEvent.Penetrated
                        ? Mathf.RoundToInt(battleEvent.Value).ToString()
                        : "RICOCHET";
                    _statusUntil = Time.unscaledTime + 0.8f;
                }
                else if (battleEvent.Type == BattleEventType.TankDestroyed)
                {
                    _status = "DESTROYED";
                    _statusUntil = Time.unscaledTime + 1.4f;
                }
                else if (battleEvent.Type == BattleEventType.StructureDestroyed)
                {
                    _status = "STRUCTURE DESTROYED";
                    _statusUntil = Time.unscaledTime + 1.2f;
                }
            }
        }

        private void SyncViews()
        {
            _mapRuntime?.SyncDestroyedStructures(_simulation.State);
            _modeWorldView?.Sync(_simulation.MatchMode);
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
            {
                _tankViews[tanks[i].Id].Sync(tanks[i]);
            }

            _staleShellIds.Clear();
            foreach (int id in _shellViews.Keys)
            {
                _staleShellIds.Add(id);
            }

            List<ShellState> shells = _simulation.State.Shells;
            for (int i = 0; i < shells.Count; i++)
            {
                ShellState shell = shells[i];
                GameObject view;
                if (!_shellViews.TryGetValue(shell.Id, out view))
                {
                    view = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    view.name = "Shell-" + shell.Id;
                    view.transform.localScale = Vector3.one * 0.18f;
                    Renderer renderer = view.GetComponent<Renderer>();
                    renderer.sharedMaterial = new Material(Shader.Find("Standard"))
                    {
                        color = new Color(1f, 0.72f, 0.14f)
                    };
                    _shellViews.Add(shell.Id, view);
                }

                view.transform.position = shell.Position.ToUnity();
                _staleShellIds.Remove(shell.Id);
            }

            for (int i = 0; i < _staleShellIds.Count; i++)
            {
                int id = _staleShellIds[i];
                DestroyShellView(_shellViews[id]);
                _shellViews.Remove(id);
            }
        }

        private static void DestroyShellView(GameObject shell)
        {
            Renderer renderer = shell != null ? shell.GetComponent<Renderer>() : null;
            if (renderer != null && renderer.sharedMaterial != null)
            {
                ReleaseObject(renderer.sharedMaterial);
            }
            ReleaseObject(shell);
        }

        private static void ReleaseObject(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void CheckResult()
        {
            Team? winner = _simulation.MatchMode.Winner;
            if (winner.HasValue)
            {
                _status = winner.Value == Team.Alpha ? "VICTORY" : "DEFEAT";
                _statusUntil = float.PositiveInfinity;
                if (_replaySession == null)
                    _audio?.PresentResult(_status);
                ArchiveReplay();
            }
            else if (_simulation.MatchMode.Draw)
            {
                _status = "DRAW";
                _statusUntil = float.PositiveInfinity;
                if (_replaySession == null)
                    _audio?.PresentResult(_status);
                ArchiveReplay();
            }
        }

        private void ArchiveReplay()
        {
            if (_archiveWritten || _archivePlayback || _replayArchive == null) return;
            _archiveWritten = true;
            try
            {
                ReplayArchiveEntry entry = _replayArchive.Save(
                    _replayRecorder.Recording,
                    mapId,
                    _player.Id);
                _archivedReplayId = entry.Id;
            }
            catch (Exception error)
            {
                Debug.LogError("Replay archive failed: " + error.Message);
            }
        }

        private bool IsBattleOver()
        {
            return _status == "VICTORY" || _status == "DEFEAT" || _status == "DRAW";
        }

        private void BuildEnvironment()
        {
            _mapRuntime?.Dispose();
            _mapRuntime = MapRuntime.Create(_catalog.GetMap(mapId));

            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            _camera.fieldOfView = 58f;
            _camera.nearClipPlane = 0.15f;
            _camera.farClipPlane = 500f;
            _camera.backgroundColor = new Color(0.49f, 0.61f, 0.68f);
            _camera.transform.position = new Vector3(0f, 8f, -48f);
        }

        private static Float3 SpawnPosition(BattleState state, float x, float z)
        {
            return new Float3(x, state.HeightField.HeightAt(x, z), z);
        }

        private void OnGUI()
        {
            if (_hud != null) return;
            EnsureStyles();
            float hpRatio = _player == null ? 0f : _player.Health / _player.Spec.MaxHealth;
            GUI.color = new Color(0.05f, 0.06f, 0.055f, 0.88f);
            GUI.Box(new Rect(20f, Screen.height - 92f, 286f, 66f), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(34f, Screen.height - 82f, 250f, 24f),
                _player == null ? string.Empty : _player.Spec.DisplayName, _labelStyle);
            GUI.Label(new Rect(34f, Screen.height - 56f, 250f, 24f),
                _player == null
                    ? string.Empty
                    : string.Format("HP {0:0}   AP {1:0.0}s", _player.Health, _player.ReloadRemainingS),
                _labelStyle);

            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            GUI.DrawTexture(new Rect(20f, Screen.height - 20f, 286f, 6f), Texture2D.whiteTexture);
            GUI.color = hpRatio > 0.35f ? new Color(0.36f, 0.78f, 0.28f) : new Color(0.88f, 0.18f, 0.12f);
            GUI.DrawTexture(new Rect(20f, Screen.height - 20f, 286f * hpRatio, 6f), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width * 0.5f - 8f, Screen.height * 0.5f - 16f, 20f, 30f), "+", _statusStyle);
            if (_simulation != null && gameMode != GameModeId.Standard)
            {
                MatchModeState mode = _simulation.MatchMode;
                string objective = gameMode == GameModeId.EndlessHorde
                    ? "WAVE " + mode.HordeWave
                    : string.Format("{0:0}  -  {1:0}", mode.AlphaScore, mode.BravoScore);
                GUI.Label(new Rect(0f, 8f, Screen.width, 32f), objective, _statusStyle);
            }
            if (Time.unscaledTime < _statusUntil)
            {
                GUI.Label(new Rect(0f, 30f, Screen.width, 48f), _status, _statusStyle);
            }

            if (IsBattleOver() &&
                GUI.Button(new Rect(Screen.width * 0.5f - 70f, Screen.height * 0.5f + 58f, 140f, 38f), "RESTART"))
            {
                StartBattle();
            }

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }
    }
}
