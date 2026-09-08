using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using ClaudeOfTanks.WebRTC;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ClaudeOfTanks.Runtime
{
    [DefaultExecutionOrder(-100)]
    public sealed partial class NetworkBattleController : MonoBehaviour
    {
        private readonly BattleCameraRig _cameraRig =
            new BattleCameraRig();
        private readonly SpottingSimulation _hudSpotting =
            new SpottingSimulation();
        private PrivateRoomAuthoritativeHostRuntime _host;
        private INetworkBattleClientRuntime _client;
        private PrivateRoomNetworkClientRuntime _privateClient;
        private Action<PrivateRoomHostLobbyRuntime> _hostReturned;
        private Action<PrivateRoomClientLobbyRuntime> _clientReturned;
        private Action _rankedReturned;
        private IDisposable _pendingHandoff;
        private NetworkBattlePresenter _presenter;
        private LocalTankPredictor _predictor;
        private ContentCatalog _catalog;
        private RoomMatchPlan _plan;
        private BattleHud _hud;
        private Camera _camera;
        private TankState _cameraTarget;
        private string _localPlayerId;
        private string _localEntityId;
        private float _accumulator;
        private long _commandTick;
        private uint _sequence;
        private uint _actionSequence;
        private Vector3 _cameraAimPoint;
        private bool _aimHeldLastFrame;
        private bool _aimHoldOwnsSniper;
        private bool _transitioned;
        private bool _initialized;

        public bool IsHost => _host != null;
        public bool IsSpectator { get; private set; }
        public int Round => _plan?.Round ?? 0;
        public string MapId => _plan?.MapId;
        public GameModeId GameMode =>
            _plan != null ? _plan.GameMode : GameModeId.Standard;
        public string[] LocalEquipment
        {
            get
            {
                RoomMatchSeat seat =
                    FindSeat(_plan, _localPlayerId);
                return seat?.Equipment == null
                    ? Array.Empty<string>()
                    : (string[])seat.Equipment.Clone();
            }
        }
        public string LocalCamouflageId =>
            FindSeat(_plan, _localPlayerId)?.CamoId;
        public NetworkWorldSnapshot LatestSnapshot =>
            _host != null
                ? _host.LocalClient.LatestSnapshot
                : _client?.LatestSnapshot;
        public TankState PresentedPlayer => _cameraTarget;
        public int VisibleTankCount =>
            _presenter?.VisibleTanks.Count ?? 0;

        public void ConfigureHost(
            PrivateRoomHostMatchHandoff handoff,
            Action<PrivateRoomHostLobbyRuntime> returned)
        {
            if (_initialized) throw new InvalidOperationException(
                "Network battle is already configured.");
            if (handoff == null) throw new ArgumentNullException(
                nameof(handoff));
            _pendingHandoff = handoff;
            _catalog = ContentCatalog.Load();
            _plan = handoff.Plan;
            _localPlayerId = handoff.Room.HostPlayerId;
            RoomMatchSeat seat = FindSeat(_plan, _localPlayerId);
            IsSpectator = seat == null;
            _localEntityId = seat?.EntityId;
            _predictor = CreatePredictor(seat);
            AuthoritativeMatchHost authority =
                new AuthoritativeMatchHost(
                    RoomMatchBattleFactory.Create(_catalog, _plan));
            _host = handoff.CreateMatchRuntime(
                authority,
                _predictor);
            _pendingHandoff = null;
            _hostReturned = returned ??
                throw new ArgumentNullException(nameof(returned));
            InitializePresentation();
        }

        public void ConfigureClient(
            PrivateRoomClientMatchHandoff handoff,
            Action<PrivateRoomClientLobbyRuntime> returned)
        {
            if (_initialized) throw new InvalidOperationException(
                "Network battle is already configured.");
            if (handoff == null) throw new ArgumentNullException(
                nameof(handoff));
            _pendingHandoff = handoff;
            _catalog = ContentCatalog.Load();
            _plan = handoff.Plan;
            _localPlayerId = handoff.PlayerId;
            _localEntityId = handoff.EntityId;
            IsSpectator = handoff.IsSpectator;
            _predictor = CreatePredictor(
                FindSeat(_plan, _localPlayerId));
            _privateClient =
                handoff.CreateMatchRuntime(_predictor);
            _client = _privateClient;
            _pendingHandoff = null;
            _clientReturned = returned ??
                throw new ArgumentNullException(nameof(returned));
            InitializePresentation();
        }

        public void ReturnToRoom()
        {
            if (_transitioned) return;
            _transitioned = true;
            if (_host != null)
            {
                string result = ResultCode(_host.LocalClient.LatestSnapshot);
                PrivateRoomAuthoritativeHostRuntime runtime = _host;
                _host = null;
                _hostReturned(runtime.FinishToLobby(
                    result,
                    "returned_from_battle"));
            }
            else if (_privateClient != null)
            {
                PrivateRoomNetworkClientRuntime runtime =
                    _privateClient;
                _client = null;
                _privateClient = null;
                _clientReturned(runtime.ReturnToLobby());
            }
            else if (_client != null)
            {
                INetworkBattleClientRuntime runtime = _client;
                _client = null;
                runtime.Dispose();
                _rankedReturned();
            }
        }

        private void InitializePresentation()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) InputSystem.AddDevice<Keyboard>();
            if (Mouse.current == null) InputSystem.AddDevice<Mouse>();
#endif
            _presenter = new NetworkBattlePresenter(
                _catalog,
                _plan,
                transform);
            BuildCamera();
            _hud = BattleHud.Create(ReturnToRoom, ReturnToRoom, parent: transform);
            _hud.SetMap(_catalog.GetMap(_plan.MapId));
            _hud.SetReplayActionsAvailable(false);
            _cameraRig.Reset();
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized || _transitioned) return;
            _client?.Pump();
            UpdateCameraControls();
            float frameTime = Mathf.Min(Time.deltaTime, 0.25f);
            _accumulator += frameTime;
            while (_accumulator >= BattleState.FixedDeltaTime)
            {
                if (!IsSpectator)
                {
                    NetworkInputCommand command = ReadInput();
                    if (_host != null) _host.SendLocalInput(command);
                    else _client?.SendInput(command);
                }
                if (_host != null) _host.Update(1);
                _commandTick++;
                _accumulator -= BattleState.FixedDeltaTime;
            }
            _client?.Pump();
            _predictor?.AdvancePresentation(frameTime);
            NetworkWorldSnapshot snapshot = LatestSnapshot;
            if (snapshot == null) return;
            _commandTick = Math.Max(_commandTick, snapshot.Tick);
            _cameraTarget = _presenter.Apply(
                snapshot,
                _host != null
                    ? _host.LocalClient.Buffer
                    : _client.Buffer,
                _predictor,
                _localEntityId);
            UpdateHud(snapshot);
        }

        private void LateUpdate()
        {
            if (_camera == null || _cameraTarget == null) return;
            _cameraRig.Apply(
                _camera,
                _cameraTarget,
                _cameraAimPoint,
                Time.deltaTime);
            _presenter.UpdateVisibility(_camera.transform.position);
            _presenter.UpdateAudio(
                _camera.transform.position,
                _localEntityId,
                _cameraRig.Mode == BattleCameraMode.Sniper);
            _presenter.SetTankVisible(
                _cameraTarget.Id,
                _cameraRig.Mode != BattleCameraMode.Sniper);
        }

        private NetworkInputCommand ReadInput()
        {
            if (_hud.IsPaused) return ReadPausedInput();
            float throttle = 0f;
            float steer = 0f;
            GameSettings settings = GameSettings.Current;
            if (settings.IsPressed(GameInputAction.Forward)) throttle++;
            if (settings.IsPressed(GameInputAction.Reverse)) throttle--;
            if (settings.IsPressed(GameInputAction.Right)) steer++;
            if (settings.IsPressed(GameInputAction.Left)) steer--;
            BattleGamepadFrame gamepad = BattleGamepadInput.Read();
            steer += gamepad.Steer;
            throttle += gamepad.Throttle;
            steer += _hud.TouchDrive.x;
            throttle += _hud.TouchDrive.y;
            NetworkActionBits actions = NetworkActionBits.None;
            if (gamepad.Fire || _hud.FireHeld || IsPrimaryPressed() ||
                settings.IsPressed(GameInputAction.Fire))
            {
                actions |= NetworkActionBits.Fire;
            }
            if (gamepad.RepairPressed ||
                settings.WasPressedThisFrame(GameInputAction.Repair) ||
                _hud.ConsumeConsumable(0))
                actions |= NetworkActionBits.RepairKit;
            if (gamepad.FirstAidPressed ||
                settings.WasPressedThisFrame(GameInputAction.FirstAid) ||
                _hud.ConsumeConsumable(1))
                actions |= NetworkActionBits.FirstAidKit;
            if (gamepad.ExtinguisherPressed ||
                settings.WasPressedThisFrame(GameInputAction.Extinguisher) ||
                _hud.ConsumeConsumable(2))
                actions |= NetworkActionBits.FireExtinguisher;

            Float3 origin = _predictor.State.Position +
                new Float3(0f, 1.65f, 0f);
            Float3 aim = AimPoint(origin);
            Float3 direction = (aim - origin).Normalized;
            float distance = (aim - origin).Magnitude;
            if (actions != NetworkActionBits.None) _actionSequence++;
            return new NetworkInputCommand
            {
                Sequence = ++_sequence,
                ActionSequence = _actionSequence,
                ClientTick = _commandTick,
                SnapshotAckTick = -1,
                Throttle = Mathf.Clamp(throttle, -1f, 1f),
                Steer = Mathf.Clamp(steer, -1f, 1f),
                Brake = gamepad.Brake || _hud.BrakeHeld ||
                    settings.IsPressed(GameInputAction.Brake),
                AimYawRad = MathF.Atan2(direction.X, direction.Z),
                AimPitchRad = MathF.Asin(
                    MathUtil.Clamp(direction.Y, -1f, 1f)),
                AimDistanceM = MathUtil.Clamp(
                    distance,
                    NetworkProtocol.MinimumAimDistanceM,
                    NetworkProtocol.MaximumAimDistanceM),
                Actions = actions
            };
        }

        private Float3 AimPoint(Float3 origin)
        {
            Float3 fallback = origin +
                Float3.Forward(
                    _predictor.State.Yaw +
                    _predictor.State.TurretYaw) * 100f;
            Vector2 pointer;
            if (_hud.TryGetTouchAimPosition(out pointer) ||
                TryPointer(out pointer))
            {
                Ray ray = _camera.ScreenPointToRay(pointer);
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                float distance;
                if (ground.Raycast(ray, out distance))
                    fallback = ray.GetPoint(distance).ToSimulation();
            }
            _cameraAimPoint = fallback.ToUnity();
            return fallback;
        }

        private void UpdateHud(NetworkWorldSnapshot snapshot)
        {
            if (_cameraTarget == null) return;
            bool over = snapshot.Winner.HasValue || snapshot.Draw;
            string status = _client != null &&
                    !_client.IsConnected
                ? "RECONNECTING" :
                IsSpectator ? "SPECTATING" :
                over ? Verdict(snapshot) :
                Time.unscaledTime < _presenter.StatusUntil
                    ? _presenter.Status
                    : string.Empty;
            _hud.SetState(
                _cameraTarget,
                _presenter.MatchMode,
                status,
                over,
                new BattleHudStats
                {
                    ShotsFired = _presenter.ShotsFired,
                    Hits = _presenter.Hits,
                    Penetrations = _presenter.Penetrations,
                    DamageDealt = _presenter.DamageDealt,
                    DamageReceived = _presenter.DamageReceived,
                    Kills = _cameraTarget.Kills,
                    TimeS = _presenter.State.TimeS
                });
            _hud.SetCamera(_cameraRig.Mode, _cameraRig.Zoom);
            _hud.SetMinimap(
                _cameraTarget,
                _presenter.VisibleTanks,
                _presenter.MatchMode,
                _hudSpotting,
                _presenter.State.IsVisionOccluded);
        }

        private LocalTankPredictor CreatePredictor(RoomMatchSeat seat)
        {
            if (seat == null) return null;
            VehicleDefinition definition =
                _catalog.GetVehicle(seat.VehicleSpecId);
            return new LocalTankPredictor(
                seat.PlayerId,
                seat.EntityId,
                seat.Team,
                definition.ToTankSpec(),
                MapSimulationAdapter.BuildHeightField(
                    _catalog.GetMap(_plan.MapId)),
                seat.Equipment);
        }

        private void BuildCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject root = new GameObject("Main Camera");
                root.tag = "MainCamera";
                _camera = root.AddComponent<Camera>();
                root.AddComponent<AudioListener>();
            }
            _camera.fieldOfView = 58f;
            _camera.nearClipPlane = 0.15f;
            _camera.farClipPlane = 500f;
            _camera.backgroundColor =
                new Color(0.49f, 0.61f, 0.68f);
        }

        private void UpdateCameraControls()
        {
            if (_hud.IsPaused) return;
            if (BattleGamepadInput.Read().SniperPressed ||
                GameSettings.Current.WasPressedThisFrame(
                    GameInputAction.Sniper) ||
                _hud.ConsumeSniperToggle())
            {
                _cameraRig.ToggleSniper();
                _aimHoldOwnsSniper = false;
            }
            int zoom = ReadZoom();
            if (zoom != 0)
            {
                _cameraRig.StepZoom(zoom);
                _aimHoldOwnsSniper = false;
            }
            bool held = IsSecondaryPressed();
            if (held && !_aimHeldLastFrame &&
                _cameraRig.Mode == BattleCameraMode.Arcade)
            {
                _cameraRig.ToggleSniper();
                _aimHoldOwnsSniper = true;
            }
            else if (!held && _aimHeldLastFrame &&
                _aimHoldOwnsSniper)
            {
                if (_cameraRig.Mode == BattleCameraMode.Sniper)
                    _cameraRig.ToggleSniper();
                _aimHoldOwnsSniper = false;
            }
            _aimHeldLastFrame = held;
        }

        private static RoomMatchSeat FindSeat(
            RoomMatchPlan plan,
            string playerId)
        {
            for (int i = 0; i < plan.Seats.Length; i++)
                if (plan.Seats[i].PlayerId == playerId)
                    return plan.Seats[i];
            return null;
        }

        private static bool TryPointer(out Vector2 position)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }
#endif
            position = Input.mousePosition;
            return true;
        }

        private static bool IsPrimaryPressed()
        {
            bool pressed = Input.GetMouseButton(0);
#if ENABLE_INPUT_SYSTEM
            pressed |= Mouse.current != null &&
                Mouse.current.leftButton.isPressed;
#endif
            return pressed;
        }

        private static bool IsSecondaryPressed()
        {
            bool pressed = Input.GetMouseButton(1);
#if ENABLE_INPUT_SYSTEM
            pressed |= Mouse.current != null &&
                Mouse.current.rightButton.isPressed;
#endif
            return pressed;
        }

        private static int ReadZoom()
        {
            float value = Input.mouseScrollDelta.y;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                float current = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(current) > Mathf.Abs(value))
                    value = current;
            }
#endif
            return value > 0.01f ? 1 : value < -0.01f ? -1 : 0;
        }

        private string Verdict(NetworkWorldSnapshot snapshot)
        {
            if (snapshot.Draw) return "DRAW";
            RoomMatchSeat seat = FindSeat(_plan, _localPlayerId);
            if (seat == null)
                return snapshot.Winner == Team.Alpha
                    ? "ALPHA VICTORY"
                    : "BRAVO VICTORY";
            return snapshot.Winner == seat.Team
                ? "VICTORY"
                : "DEFEAT";
        }

        private static string ResultCode(
            NetworkWorldSnapshot snapshot)
        {
            if (snapshot == null) return "aborted";
            if (snapshot.Draw) return "draw";
            return snapshot.Winner.HasValue
                ? snapshot.Winner.Value.ToString().ToLowerInvariant()
                : "aborted";
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
            _pendingHandoff?.Dispose();
            if (!_transitioned)
            {
                _client?.Dispose();
                _host?.Dispose();
            }
        }
    }
}
