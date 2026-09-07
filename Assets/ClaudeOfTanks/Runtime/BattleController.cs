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
        private readonly Dictionary<int, GameObject> _shellViews = new Dictionary<int, GameObject>();
        private readonly List<int> _staleShellIds = new List<int>();
        private readonly BattleCameraRig _cameraRig = new BattleCameraRig();
        private BattleSimulation _simulation;
        private BattleReplayRecorder _replayRecorder;
        private BotController _botController;
        private readonly SpottingSimulation _hudSpotting = new SpottingSimulation();
        private TankState _player;
        private Camera _camera;
        private ContentCatalog _catalog;
        private MapRuntime _mapRuntime;
        private BattleHud _hud;
        private BattleEffects _effects;
        [SerializeField] private string mapId = "verdant";
        [SerializeField] private GameModeId gameMode = GameModeId.Standard;
        [SerializeField] private string vehicleId = "m1a2";
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

        public void Configure(
            string selectedVehicleId, string selectedMapId, GameModeId selectedMode,
            Action returnToGarage)
        {
            vehicleId = selectedVehicleId;
            mapId = selectedMapId;
            gameMode = selectedMode;
            _returnToGarage = returnToGarage;
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
            _hud = BattleHud.Create(StartBattle, _returnToGarage);
            _hud.transform.SetParent(transform, false);
            _hud.SetMap(_catalog.GetMap(mapId));
            _effects = BattleEffects.Create();
            _effects.transform.SetParent(transform, false);
        }

        private void Update()
        {
            if (_simulation == null)
            {
                return;
            }

            float frameTime = Mathf.Min(Time.deltaTime, 0.25f);
            _accumulator += frameTime;
            UpdateCameraControls();
            TankInput playerInput = ReadPlayerInput();
            while (_accumulator >= BattleState.FixedDeltaTime)
            {
                BuildInputs(playerInput);
                _replayRecorder.Record(_inputs, BattleState.FixedDeltaTime);
                _simulation.Step(_inputs, BattleState.FixedDeltaTime);
                ConsumeEvents();
                _accumulator -= BattleState.FixedDeltaTime;
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
                _hudSpotting);
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
            _effects?.ResetAll();
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

            MapDefinition map = _catalog.GetMap(mapId);
            BattleState state = new BattleState(BuildHeightField(map), 6000u);
            MapPoint playerSpawn = map.spawns.player;
            AddTank(state, "player", Team.Alpha, vehicleId,
                SpawnPosition(state, playerSpawn.x, playerSpawn.z), 0f);
            AddTank(state, "alpha-2", Team.Alpha, "challenger2",
                SpawnPosition(state, playerSpawn.x - 13f, playerSpawn.z - 8f), 0.1f);
            string[] enemies = { "t90m", "type99a", "leo2a6" };
            for (int i = 0; i < enemies.Length; i++)
            {
                MapPoint spawn = map.spawns.enemies[i];
                AddTank(state, "bravo-" + (i + 1), Team.Bravo, enemies[i],
                    SpawnPosition(state, spawn.x, spawn.z), MathUtil.Pi);
            }
            _simulation = new BattleSimulation(state, gameMode);
            _replayRecorder = new BattleReplayRecorder(state, gameMode);
            _botController = new BotController(new SpottingSimulation());
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
                _tankViews.Add(tank.Id, TankView.Create(tank, _vehicleDefinitions[tank.Id]));
            }

            _accumulator = 0f;
            _status = "BATTLE";
            _statusUntil = Time.unscaledTime + 1.5f;
        }

        private void OnDestroy()
        {
            _mapRuntime?.Dispose();
            foreach (TankView view in _tankViews.Values) view.Destroy();
            foreach (GameObject shell in _shellViews.Values) DestroyShellView(shell);
        }

        private void AddTank(
            BattleState state, string entityId, Team team, string vehicleId, Float3 position, float yaw)
        {
            VehicleDefinition definition = _catalog.GetVehicle(vehicleId);
            state.Tanks.Add(new TankState(entityId, team, definition.ToTankSpec(), position, yaw));
            _vehicleDefinitions[entityId] = definition;
        }

        private TankInput ReadPlayerInput()
        {
            float throttle = 0f;
            float steer = 0f;
            if (IsKeyPressed(KeyCode.W) || IsKeyPressed(KeyCode.UpArrow)) throttle += 1f;
            if (IsKeyPressed(KeyCode.S) || IsKeyPressed(KeyCode.DownArrow)) throttle -= 1f;
            if (IsKeyPressed(KeyCode.D) || IsKeyPressed(KeyCode.RightArrow)) steer += 1f;
            if (IsKeyPressed(KeyCode.A) || IsKeyPressed(KeyCode.LeftArrow)) steer -= 1f;
            bool gamepadFire = false;
            bool gamepadBrake = false;
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                steer += stick.x;
                throttle += stick.y;
                gamepadFire = Gamepad.current.rightTrigger.isPressed;
                gamepadBrake = Gamepad.current.buttonSouth.isPressed;
            }
#endif
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
                Brake = gamepadBrake || (_hud != null && _hud.BrakeHeld) ||
                    IsKeyPressed(KeyCode.LeftControl) || IsKeyPressed(KeyCode.RightControl),
                Fire = gamepadFire || (_hud != null && _hud.FireHeld) ||
                    IsPrimaryButtonPressed() || IsKeyPressed(KeyCode.Space),
                UseRepairKit = Input.GetKeyDown(KeyCode.Alpha4) ||
                    (_hud != null && _hud.ConsumeConsumable(0)),
                UseFirstAidKit = Input.GetKeyDown(KeyCode.Alpha5) ||
                    (_hud != null && _hud.ConsumeConsumable(1)),
                UseFireExtinguisher = Input.GetKeyDown(KeyCode.Alpha6) ||
                    (_hud != null && _hud.ConsumeConsumable(2)),
                AimPoint = aimPoint
            };
        }

        private static bool IsKeyPressed(KeyCode keyCode)
        {
            bool pressed = Input.GetKey(keyCode);
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return pressed;
            }

            switch (keyCode)
            {
                case KeyCode.W: return pressed || keyboard.wKey.isPressed;
                case KeyCode.A: return pressed || keyboard.aKey.isPressed;
                case KeyCode.S: return pressed || keyboard.sKey.isPressed;
                case KeyCode.D: return pressed || keyboard.dKey.isPressed;
                case KeyCode.UpArrow: return pressed || keyboard.upArrowKey.isPressed;
                case KeyCode.DownArrow: return pressed || keyboard.downArrowKey.isPressed;
                case KeyCode.LeftArrow: return pressed || keyboard.leftArrowKey.isPressed;
                case KeyCode.RightArrow: return pressed || keyboard.rightArrowKey.isPressed;
                case KeyCode.LeftShift: return pressed || keyboard.leftShiftKey.isPressed;
                case KeyCode.RightShift: return pressed || keyboard.rightShiftKey.isPressed;
                case KeyCode.LeftControl: return pressed || keyboard.leftCtrlKey.isPressed;
                case KeyCode.RightControl: return pressed || keyboard.rightCtrlKey.isPressed;
                case KeyCode.Space: return pressed || keyboard.spaceKey.isPressed;
            }
#endif
            return pressed;
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
            if (IsKeyPressedThisFrame(KeyCode.LeftShift) ||
                IsKeyPressedThisFrame(KeyCode.RightShift) ||
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

        private static bool IsKeyPressedThisFrame(KeyCode keyCode)
        {
            bool pressed = Input.GetKeyDown(keyCode);
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return pressed;
            }
            if (keyCode == KeyCode.LeftShift) return pressed || keyboard.leftShiftKey.wasPressedThisFrame;
            if (keyCode == KeyCode.RightShift) return pressed || keyboard.rightShiftKey.wasPressedThisFrame;
#endif
            return pressed;
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
                if (bot.Destroyed)
                {
                    continue;
                }

                _inputs[bot.Id] = _botController.Decide(bot, tanks);
            }
        }

        private void ConsumeEvents()
        {
            List<BattleEvent> events = _simulation.State.Events;
            for (int i = 0; i < events.Count; i++)
            {
                BattleEvent battleEvent = events[i];
                if (battleEvent.Type == BattleEventType.ShellFired &&
                    battleEvent.SourceId == _player.Id)
                {
                    _shotsFired++;
                }
                else if (battleEvent.Type == BattleEventType.ShellHit)
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
            }
        }

        private void SyncViews()
        {
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
            }
            else if (_simulation.MatchMode.Draw)
            {
                _status = "DRAW";
                _statusUntil = float.PositiveInfinity;
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

        private static IHeightField BuildHeightField(MapDefinition map)
        {
            LandformDefinition[] source = map.terrain?.landforms ?? Array.Empty<LandformDefinition>();
            TerrainLandform[] landforms = new TerrainLandform[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                landforms[i] = new TerrainLandform
                {
                    Kind = source[i].kind,
                    X = source[i].x,
                    Z = source[i].z,
                    Height = source[i].height,
                    Length = source[i].length,
                    Width = source[i].width,
                    RadiusX = source[i].rx,
                    RadiusZ = source[i].rz,
                    YawRad = source[i].yawDeg * MathUtil.Deg2Rad
                };
            }
            return new LandformHeightField(landforms);
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
