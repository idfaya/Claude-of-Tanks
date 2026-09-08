using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class GameFlowController : MonoBehaviour
    {
        private ContentCatalog _catalog;
        private GameObject _garage;
        private TankView _preview;
        private Dropdown _vehicle;
        private Dropdown _map;
        private Dropdown _mode;
        private Camera _camera;
        private BattleController _battle;
        private NetworkBattleController _networkBattle;
        private GameSettingsPanel _settingsPanel;
        private ReplayBrowserPanel _replayBrowser;
        private PrivateRoomCoordinator _privateRoom;
        private PrivateRoomPanel _privateRoomPanel;
        private RankedCoordinator _ranked;
        private RankedPanel _rankedPanel;
        private ReplayArchive _replayArchive;
        private Material _garageFloorMaterial;

        public bool IsGarageVisible => _garage != null;
        public BattleController ActiveBattle => _battle;
        public NetworkBattleController ActiveNetworkBattle => _networkBattle;
        public PrivateRoomCoordinator PrivateRoom => _privateRoom;
        public PrivateRoomPanel PrivateRoomPanel => _privateRoomPanel;
        public RankedCoordinator Ranked => _ranked;
        public RankedPanel RankedPanel => _rankedPanel;
        public int VehicleOptionCount => _catalog.ProductionVehicleIds.Length;
        public int MapOptionCount => _catalog.Maps.Length;
        public string SelectedVehicleId => _catalog.ProductionVehicleIds[_vehicle.value];
        public string SelectedMapId => _catalog.Maps[_map.value].id;
        public GameModeId SelectedMode => (GameModeId)_mode.value;
        public int ReplayCount => _replayArchive != null ? _replayArchive.List().Length : 0;

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

            _catalog = ContentCatalog.Load();
            InitializeLoadout();
            _replayArchive = ReplayArchive.Current;
            _privateRoom = gameObject.AddComponent<PrivateRoomCoordinator>();
            _privateRoom.ConfigureContent(
                _catalog.ProductionVehicleIds,
                MapIds(),
                (vehicleId, equipmentId) =>
                    _catalog.GetVehicle(vehicleId)
                        .AllowsEquipment(equipmentId));
            _privateRoom.MatchHandoffReady += StartNetworkBattle;
            _privateRoom.Changed += OnPrivateRoomChanged;
            _ranked = gameObject.AddComponent<RankedCoordinator>();
            _ranked.MatchHandoffReady += StartRankedBattle;
            EnsureEventSystem();
            ShowGarage();
        }

        private void ShowGarage()
        {
            if (_battle != null) ReleaseObject(_battle.gameObject);
            _battle = null;
            DestroyGarage();
            _garage = new GameObject("Garage");
            _garage.transform.SetParent(transform, false);
            BuildGarageStage();
            BuildGarageUi();
            if (!SyncGarageFromRoom())
                RefreshDefaultGarageSelection();
        }

        private void StartBattle()
        {
            if (_privateRoom != null && _privateRoom.IsInLobby)
            {
                return;
            }
            if (_ranked != null && _ranked.IsBusy)
            {
                return;
            }
            string vehicleId = SelectedVehicleId;
            string mapId = SelectedMapId;
            GameModeId mode = SelectedMode;
            DestroyGarage();
            GameObject battleObject = new GameObject("Battle");
            battleObject.transform.SetParent(transform, false);
            battleObject.SetActive(false);
            _battle = battleObject.AddComponent<BattleController>();
            _battle.Configure(
                vehicleId,
                mapId,
                mode,
                ShowGarage,
                _replayArchive,
                _loadout.Equipment,
                _loadout.CamouflageId);
            battleObject.SetActive(true);
            _battle.Initialize();
        }

        public void Select(int vehicleIndex, int mapIndex, GameModeId mode)
        {
            if (_garage == null)
            {
                throw new InvalidOperationException("Selections can only change in the garage.");
            }
            if (vehicleIndex < 0 || vehicleIndex >= VehicleOptionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(vehicleIndex));
            }
            if (mapIndex < 0 || mapIndex >= MapOptionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(mapIndex));
            }
            if (!Enum.IsDefined(typeof(GameModeId), mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            _vehicle.SetValueWithoutNotify(vehicleIndex);
            _map.SetValueWithoutNotify(mapIndex);
            _mode.SetValueWithoutNotify((int)mode);
            bool vehicleChanged =
                _loadout.VehicleId !=
                _catalog.ProductionVehicleIds[vehicleIndex];
            _loadout.SelectVehicle(
                _catalog.ProductionVehicleIds[vehicleIndex]);
            if (!vehicleChanged) RefreshPreview(vehicleIndex);
        }

        public void DeploySelected()
        {
            StartBattle();
        }

        public void ReturnToGarage()
        {
            if (_networkBattle != null)
            {
                _networkBattle.ReturnToRoom();
                return;
            }
            ShowGarage();
        }

        private void StartNetworkBattle()
        {
            if (_networkBattle != null) return;
            DestroyGarage();
            GameObject battleObject = new GameObject("NetworkBattle");
            battleObject.transform.SetParent(transform, false);
            battleObject.SetActive(false);
            _networkBattle =
                battleObject.AddComponent<NetworkBattleController>();
            try
            {
                if (_privateRoom.IsHost)
                {
                    _networkBattle.ConfigureHost(
                        _privateRoom.TakeHostHandoff(),
                        ResumeHostLobby);
                }
                else
                {
                    _networkBattle.ConfigureClient(
                        _privateRoom.TakeClientHandoff(),
                        ResumeClientLobby);
                }
                battleObject.SetActive(true);
            }
            catch (Exception error)
            {
                Debug.LogError(
                    "Private room battle failed: " + error.Message);
                _privateRoom.Leave();
                _networkBattle = null;
                ReleaseObject(battleObject);
                ShowGarage();
            }
        }

        private void ResumeHostLobby(
            ClaudeOfTanks.WebRTC.PrivateRoomHostLobbyRuntime lobby)
        {
            _privateRoom.ResumeHostLobby(lobby);
            FinishNetworkBattle();
        }

        private void ResumeClientLobby(
            ClaudeOfTanks.WebRTC.PrivateRoomClientLobbyRuntime lobby)
        {
            _privateRoom.ResumeClientLobby(lobby);
            FinishNetworkBattle();
        }

        private void FinishNetworkBattle()
        {
            GameObject battleObject =
                _networkBattle != null
                    ? _networkBattle.gameObject
                    : null;
            _networkBattle = null;
            ReleaseObject(battleObject);
            ShowGarage();
            _privateRoomPanel.Open();
        }

        public void PlayReplay(string replayId)
        {
            ArchivedReplay replay = _replayArchive.Load(replayId);
            DestroyGarage();
            GameObject battleObject = new GameObject("Replay");
            battleObject.transform.SetParent(transform, false);
            battleObject.SetActive(false);
            _battle = battleObject.AddComponent<BattleController>();
            _battle.Configure(
                replay.Entry.PlayerVehicleId,
                replay.Entry.MapId,
                replay.Entry.GameMode,
                ShowGarage,
                _replayArchive);
            battleObject.SetActive(true);
            _battle.Initialize();
            _battle.LoadArchivedReplay(replay);
        }

        private void BuildGarageStage()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.SetParent(transform, false);
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }
            _camera.transform.position = new Vector3(9f, 5.5f, -10f);
            _camera.transform.rotation = Quaternion.LookRotation(new Vector3(-9f, -3.4f, 10f));
            _camera.fieldOfView = 48f;
            _camera.backgroundColor = new Color(0.1f, 0.12f, 0.13f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "GarageFloor";
            floor.transform.SetParent(_garage.transform, false);
            floor.transform.localScale = new Vector3(4f, 1f, 4f);
            _garageFloorMaterial = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.18f, 0.2f, 0.19f)
            };
            floor.GetComponent<Renderer>().sharedMaterial = _garageFloorMaterial;
            GameObject lightObject = new GameObject("GarageKey");
            lightObject.transform.SetParent(_garage.transform, false);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        }

        private void BuildGarageUi()
        {
            GameObject ui = new GameObject("GarageUI");
            ui.transform.SetParent(_garage.transform, false);
            Canvas canvas = ui.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = ui.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            ui.AddComponent<GraphicRaycaster>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text title = Text("Title", ui.transform, font, 30);
            title.text = "CLAUDE OF TANKS";
            Place(title.rectTransform, new Vector2(28f, -62f), new Vector2(420f, -18f), new Vector2(0f, 1f));
            _vehicle = Dropdown("Vehicle", ui.transform, font, new Vector2(28f, -132f));
            _map = Dropdown("Map", ui.transform, font, new Vector2(28f, -194f));
            _mode = Dropdown("Mode", ui.transform, font, new Vector2(28f, -256f));
            Fill(_vehicle, VehicleNames());
            Fill(_map, MapNames());
            Fill(_mode, new[] { "Standard", "Capture the Flag", "Zone Control", "Turbo Ball", "Endless Horde" });
            _vehicle.onValueChanged.AddListener(
                OnGarageVehicleChanged);
            _map.onValueChanged.AddListener(
                _ => OnGarageMapChanged());
            _mode.onValueChanged.AddListener(
                _ => _privateRoom.SelectMode(SelectedMode));
            Button deploy = Button("Deploy", ui.transform, font, "DEPLOY", new Vector2(28f, -328f));
            deploy.onClick.AddListener(StartBattle);
            Button settings = Button("Settings", ui.transform, font, "SETTINGS", new Vector2(220f, -328f));
            settings.onClick.AddListener(() => _settingsPanel.Open());
            Button replays = Button("Replays", ui.transform, font, "REPLAYS", new Vector2(412f, -328f));
            replays.onClick.AddListener(() => _replayBrowser.Open());
            _settingsPanel = GameSettingsPanel.Create(ui.transform, GameSettings.Current);
            _replayBrowser = ReplayBrowserPanel.Create(ui.transform, _replayArchive, PlayReplay);
            BuildGarageModeActions(ui.transform, font);
        }

        private void RefreshPreview(int index)
        {
            if (_preview != null) _preview.Destroy();
            VehicleDefinition definition = _catalog.GetVehicle(_catalog.ProductionVehicleIds[index]);
            TankState tank = new TankState("GarageVehicle", Team.Alpha, definition.ToTankSpec(), Vector3.zero.ToSimulation(), 0.55f);
            _preview = TankView.Create(
                tank,
                definition,
                _loadout.CamouflageId,
                SelectedMapId,
                _catalog);
            _preview.Root.SetParent(_garage.transform, true);
        }

        private List<string> VehicleNames()
        {
            List<string> values = new List<string>();
            foreach (string id in _catalog.ProductionVehicleIds) values.Add(_catalog.GetVehicle(id).name);
            return values;
        }

        private List<string> MapNames()
        {
            List<string> values = new List<string>();
            foreach (MapDefinition map in _catalog.Maps) values.Add(map.name);
            return values;
        }

        private IEnumerable<string> MapIds()
        {
            foreach (MapDefinition map in _catalog.Maps)
                yield return map.id;
        }

        private bool SyncGarageFromRoom()
        {
            RoomStateSnapshot state = _privateRoom?.LobbyState;
            if (_garage == null || state == null) return false;
            int mapIndex = Array.FindIndex(
                _catalog.Maps,
                map => map.id == state.MapId);
            if (mapIndex >= 0) _map.SetValueWithoutNotify(mapIndex);
            _mode.SetValueWithoutNotify((int)state.GameMode);
            RoomPlayerSnapshot local = Array.Find(
                state.Players,
                player => player.PlayerId == _privateRoom.LocalPlayerId);
            int vehicleIndex = local != null
                ? Array.IndexOf(
                    _catalog.ProductionVehicleIds,
                    local.VehicleSpecId)
                : -1;
            if (vehicleIndex >= 0)
                SyncLoadoutFromRoom(local, vehicleIndex);
            return true;
        }

        private void OnPrivateRoomChanged()
        {
            SyncGarageFromRoom();
        }

        private static void Fill(Dropdown dropdown, IEnumerable<string> values)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(values));
        }

        private static Dropdown Dropdown(string name, Transform parent, Font font, Vector2 position)
        {
            GameObject root = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), position, position + new Vector2(330f, 46f), new Vector2(0f, 1f));
            foreach (Text text in root.GetComponentsInChildren<Text>(true)) text.font = font;
            return root.GetComponent<Dropdown>();
        }

        private static Button Button(
            string name, Transform parent, Font font, string label, Vector2 position)
        {
            GameObject root = DefaultControls.CreateButton(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), position, position + new Vector2(180f, 50f), new Vector2(0f, 1f));
            Text text = root.GetComponentInChildren<Text>();
            text.font = font;
            text.text = label;
            return root.GetComponent<Button>();
        }

        private static Text Text(string name, Transform parent, Font font, int size)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            root.transform.SetParent(parent, false);
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            return text;
        }

        private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject events = new GameObject("EventSystem");
            events.transform.SetParent(transform, false);
            events.AddComponent<EventSystem>();
            events.AddComponent<StandaloneInputModule>();
        }

        private void DestroyGarage()
        {
            if (_preview != null)
            {
                _preview.Destroy();
                _preview = null;
            }
            if (_garageFloorMaterial != null)
            {
                ReleaseObject(_garageFloorMaterial);
                _garageFloorMaterial = null;
            }
            if (_garage != null)
            {
                ReleaseObject(_garage);
                _garage = null;
                _settingsPanel = null;
                _replayBrowser = null;
                _privateRoomPanel = null;
                _rankedPanel = null;
                _loadoutPanel = null;
            }
        }

        private void OnDestroy()
        {
            if (_privateRoom != null)
            {
                _privateRoom.MatchHandoffReady -= StartNetworkBattle;
                _privateRoom.Changed -= OnPrivateRoomChanged;
            }
            if (_ranked != null)
                _ranked.MatchHandoffReady -= StartRankedBattle;
            DisposeLoadout();
            if (_preview != null)
            {
                _preview.Destroy();
                _preview = null;
            }
            if (_garageFloorMaterial != null)
            {
                ReleaseObject(_garageFloorMaterial);
                _garageFloorMaterial = null;
            }
        }

        private static void ReleaseObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}
