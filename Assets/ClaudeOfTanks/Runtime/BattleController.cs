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
        private BattleSimulation _simulation;
        private BotController _botController;
        private TankState _player;
        private Camera _camera;
        private ContentCatalog _catalog;
        private MapRuntime _mapRuntime;
        [SerializeField] private string mapId = "verdant";
        [SerializeField] private GameModeId gameMode = GameModeId.Standard;
        private float _accumulator;
        private string _status = "BATTLE";
        private float _statusUntil;
        private GUIStyle _labelStyle;
        private GUIStyle _statusStyle;

        private void Awake()
        {
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
        }

        private void Update()
        {
            if (_simulation == null)
            {
                return;
            }

            float frameTime = Mathf.Min(Time.deltaTime, 0.25f);
            _accumulator += frameTime;
            TankInput playerInput = ReadPlayerInput();
            while (_accumulator >= BattleState.FixedDeltaTime)
            {
                BuildInputs(playerInput);
                _simulation.Step(_inputs, BattleState.FixedDeltaTime);
                ConsumeEvents();
                _accumulator -= BattleState.FixedDeltaTime;
            }

            SyncViews();
            CheckResult();
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

            Vector3 target = _player.Position.ToUnity() + Vector3.up * 1.8f;
            Vector3 forward = new Vector3(Mathf.Sin(_player.Yaw), 0f, Mathf.Cos(_player.Yaw));
            Vector3 desired = target - forward * 12f + Vector3.up * 7f;
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                desired,
                1f - Mathf.Exp(-7f * Time.deltaTime));
            _camera.transform.rotation = Quaternion.LookRotation(
                target + forward * 8f - _camera.transform.position,
                Vector3.up);
        }

        private void StartBattle()
        {
            foreach (TankView view in _tankViews.Values)
            {
                view.Destroy();
            }

            foreach (GameObject shell in _shellViews.Values)
            {
                Destroy(shell);
            }

            _tankViews.Clear();
            _shellViews.Clear();
            _inputs.Clear();
            _vehicleDefinitions.Clear();

            MapDefinition map = _catalog.GetMap(mapId);
            BattleState state = new BattleState(BuildHeightField(map), 6000u);
            MapPoint playerSpawn = map.spawns.player;
            AddTank(state, "player", Team.Alpha, "m1a2",
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
            _botController = new BotController(new SpottingSimulation());
            _player = state.Tanks[0];
            for (int i = 0; i < state.Tanks.Count; i++)
            {
                TankState tank = state.Tanks[i];
                _tankViews.Add(tank.Id, TankView.Create(tank, _vehicleDefinitions[tank.Id]));
            }

            _accumulator = 0f;
            _status = "BATTLE";
            _statusUntil = Time.unscaledTime + 1.5f;
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

            return new TankInput
            {
                Throttle = throttle,
                Steer = steer,
                Brake = IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift),
                Fire = IsPrimaryButtonPressed() || IsKeyPressed(KeyCode.Space),
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

        private static Vector2 PointerPosition()
        {
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
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = new Color(1f, 0.72f, 0.14f);
                    _shellViews.Add(shell.Id, view);
                }

                view.transform.position = shell.Position.ToUnity();
                _staleShellIds.Remove(shell.Id);
            }

            for (int i = 0; i < _staleShellIds.Count; i++)
            {
                int id = _staleShellIds[i];
                Destroy(_shellViews[id]);
                _shellViews.Remove(id);
            }
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
