using System;
using System.Text;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public struct BattleHudStats
    {
        public int ShotsFired;
        public int Hits;
        public int Penetrations;
        public int Kills;
        public float DamageDealt;
        public float DamageReceived;
        public float TimeS;
    }

    public sealed partial class BattleHud : MonoBehaviour
    {
        private Text _vehicle;
        private Text _stats;
        private Text _objective;
        private Text _status;
        private Image _healthFill;
        private Image _vehiclePanel;
        private Image _damagePanel;
        private GameObject _touchRoot;
        private GameObject _scopeRoot;
        private Text _scopeZoom;
        private Text _damageDetails;
        private BattleMinimap _minimap;
        private GameSettingsPanel _settingsPanel;
        private BattleHudPause _pause;
        private readonly RectTransform[] _consumableButtons = new RectTransform[3];
        private readonly Text[] _consumableLabels = new Text[3];
        private BattleHudHydropneumaticControl _hydropneumatic;
        private GameSettings _settings;
        private BattleHudAccessibilityPresentation _accessibility;
        private GameObject _resultRoot;
        private GameObject _replayRoot;
        private Text _resultTitle;
        private Text _resultStats;
        private Text _replayStatus;
        private Button _killcamButton;
        private Button _fullReplayButton;
        private readonly StringBuilder _damageText = new StringBuilder(160);
        private Vector2 _touchDrive;
        private Vector2 _touchAimPosition;
        private readonly bool[] _driveHeld = new bool[4];
        private bool _fireHeld;
        private bool _brakeHeld;
        private bool _touchAimActive;
        private bool _sniperToggleQueued;
        private int _touchShellSlot = -1;
        private int _touchLayoutWidth;
        private int _touchLayoutHeight;
        private readonly bool[] _consumables = new bool[3];
        private BattleCameraMode _cameraMode = (BattleCameraMode)(-1);
        private float _cameraZoom = -1f;
        private bool _resultShown;
        private Action _killcam;
        private Action _fullReplay;
        private Action _exitReplay;
        private Action _restart;
        private Action _garage;

        public Vector2 TouchDrive => _touchDrive;
        public bool FireHeld => _fireHeld;
        public bool BrakeHeld => _brakeHeld;
        public string DamageSummary => _damageDetails != null ? _damageDetails.text : string.Empty;
        public string ResultSummary => _resultStats != null ? _resultStats.text : string.Empty;
        public bool ResultVisible => _resultRoot != null && _resultRoot.activeSelf;
        public bool ReplayVisible => _replayRoot != null && _replayRoot.activeSelf;
        public bool IsPaused => _pause != null && _pause.IsPaused;
        public int MinimapTankMarkers => _minimap != null ? _minimap.VisibleTankMarkerCount : 0;
        public int MinimapEnemyMarkers => _minimap != null ? _minimap.VisibleEnemyMarkerCount : 0;
        public int MinimapObjectiveMarkers =>
            _minimap != null ? _minimap.VisibleObjectiveMarkerCount : 0;

        public static BattleHud Create(
            Action restart,
            Action garage = null,
            GameSettings settings = null,
            Transform parent = null)
        {
            GameObject root = new GameObject("BattleHUD");
            if (parent != null) root.transform.SetParent(parent, false);
            BattleHud hud = root.AddComponent<BattleHud>();
            hud._restart = restart;
            hud._garage = garage;
            hud._settings = settings ?? GameSettings.Current;
            hud.Build();
            return hud;
        }

        public void SetInputDevice(BattleInputDevice device)
        {
            _accessibility.SetInputDevice(device);
        }

        public void SetPaused(bool paused)
        {
            _pause.SetPaused(paused, ResultVisible || ReplayVisible);
        }

        public bool ConsumeConsumable(int slot)
        {
            bool value = _consumables[slot];
            _consumables[slot] = false;
            return value;
        }

        public bool ConsumeSniperToggle()
        {
            bool value = _sniperToggleQueued;
            _sniperToggleQueued = false;
            return value;
        }

        public bool ConsumeHydropneumaticToggle()
        {
            return _hydropneumatic.Consume();
        }

        public int ConsumeShellSlot(
            int current,
            int shellCount)
        {
            int requested = _touchShellSlot;
            _touchShellSlot = -1;
            if (shellCount > 0 &&
                _settings.WasPressedThisFrame(
                    GameInputAction.Shell1))
                requested = 0;
            else if (shellCount > 1 &&
                _settings.WasPressedThisFrame(
                    GameInputAction.Shell2))
                requested = 1;
            else if (shellCount > 2 &&
                _settings.WasPressedThisFrame(
                    GameInputAction.Shell3))
                requested = 2;
            return Mathf.Clamp(
                requested >= 0 ? requested : current,
                0,
                Mathf.Max(0, shellCount - 1));
        }

        public bool TryGetTouchAimPosition(out Vector2 position)
        {
            position = _touchAimPosition;
            return _touchAimActive;
        }

        public void SetState(
            TankState player, MatchModeState mode, string status, bool battleOver,
            BattleHudStats stats)
        {
            if (player == null) return;
            _vehicle.text = player.Spec.DisplayName;
            int shellSlot = Mathf.Clamp(
                player.Combat.ShellSlot,
                0,
                player.Spec.Shells.Length - 1);
            _stats.text = string.Format(
                "HP {0:0}/{1:0}    {2}    RELOAD {3:0.0}s",
                player.Health,
                player.Spec.MaxHealth,
                player.Spec.Shells[shellSlot].Type,
                player.ReloadRemainingS);
            _healthFill.fillAmount = Mathf.Clamp01(player.Health / player.Spec.MaxHealth);
            _healthFill.color =
                _accessibility.HealthColor(_healthFill.fillAmount);
            _objective.text = mode.Id == GameModeId.Standard ? "STANDARD"
                : mode.Id == GameModeId.EndlessHorde ? "WAVE " + mode.HordeWave
                : string.Format("{0:0}  {1}  {2:0}", mode.AlphaScore, ModeLabel(mode.Id), mode.BravoScore);
            _status.text = status;
            _hydropneumatic.SetState(player);
            UpdateDamagePanel(player.Combat);
            UpdateResult(status, battleOver, stats);
        }

        public void SetTouchVisible(bool visible)
        {
            _touchRoot.SetActive(visible);
            _minimap?.SetTouchLayout(visible);
            if (visible) SetTouchLayoutForViewport(Screen.width, Screen.height);
        }

        public void ConfigureReplayActions(Action killcam, Action fullReplay, Action exitReplay)
        {
            _killcam = killcam;
            _fullReplay = fullReplay;
            _exitReplay = exitReplay;
        }

        public void SetReplayActionsAvailable(bool available)
        {
            _killcamButton?.gameObject.SetActive(available);
            _fullReplayButton?.gameObject.SetActive(available);
        }

        public void SetReplayState(
            bool visible,
            bool killcam,
            float currentTimeS,
            float durationS,
            bool complete)
        {
            _replayRoot.SetActive(visible);
            if (!visible)
            {
                _resultShown = false;
                return;
            }
            _resultRoot.SetActive(false);
            _status.text = string.Empty;
            int current = Mathf.FloorToInt(currentTimeS);
            int duration = Mathf.CeilToInt(durationS);
            _replayStatus.text = string.Format(
                "{0}   {1:00}:{2:00} / {3:00}:{4:00}{5}",
                killcam ? "KILLCAM" : "BATTLE REPLAY",
                current / 60,
                current % 60,
                duration / 60,
                duration % 60,
                complete ? "   COMPLETE" : string.Empty);
        }

        public void SetTouchLayoutForViewport(int width, int height)
        {
            if (width < 1 || height < 1) throw new ArgumentOutOfRangeException();
            _touchLayoutWidth = width;
            _touchLayoutHeight = height;
            bool portrait = height > width;
            for (int i = 0; i < _consumableButtons.Length; i++)
            {
                Vector2 position = portrait
                    ? new Vector2(420f + i * 56f, 250f)
                    : new Vector2(430f + i * 56f, 20f);
                Rect(
                    _consumableButtons[i],
                    position,
                    position + new Vector2(48f, 48f),
                    Vector2.zero);
            }
            _hydropneumatic.SetLayout(portrait);
        }
        public void SetMap(MapDefinition map)
        {
            _minimap.SetMap(map);
        }

        public void SetMinimap(
            TankState player,
            System.Collections.Generic.IList<TankState> tanks,
            MatchModeState mode,
            SpottingSimulation spotting,
            System.Func<Float3, Float3, bool> isOccluded = null)
        {
            _minimap.Update(player, tanks, mode, spotting, isOccluded);
        }

        public void SetCamera(BattleCameraMode mode, float zoom)
        {
            if (_cameraMode == mode && Mathf.Approximately(_cameraZoom, zoom))
            {
                return;
            }

            _cameraMode = mode;
            _cameraZoom = zoom;
            bool scoped = mode == BattleCameraMode.Sniper;
            _scopeRoot.SetActive(scoped);
            _scopeZoom.text = scoped ? string.Format("x{0:0}", zoom) : string.Empty;
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject events = new GameObject("EventSystem");
                events.transform.SetParent(transform, false);
                events.AddComponent<EventSystem>();
                events.AddComponent<StandaloneInputModule>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Image panel = Image("VehiclePanel", transform, new Color(0.035f, 0.045f, 0.04f, 0.88f));
            _vehiclePanel = panel;
            Rect(panel.rectTransform, new Vector2(20f, 20f), new Vector2(390f, 92f), new Vector2(0f, 0f));
            _vehicle = Label("Vehicle", panel.transform, font, 18, TextAnchor.UpperLeft);
            Rect(_vehicle.rectTransform, new Vector2(16f, -36f), new Vector2(-16f, -8f),
                new Vector2(0f, 1f), new Vector2(1f, 1f));
            _stats = Label("Stats", panel.transform, font, 14, TextAnchor.MiddleLeft);
            _stats.resizeTextForBestFit = true;
            _stats.resizeTextMinSize = 11;
            _stats.resizeTextMaxSize = 18;
            Rect(_stats.rectTransform, new Vector2(16f, 24f), new Vector2(-16f, 48f),
                Vector2.zero, new Vector2(1f, 0f));
            Image healthBack = Image("Health", panel.transform, new Color(0.08f, 0.09f, 0.08f, 1f));
            Rect(healthBack.rectTransform, new Vector2(16f, 10f), new Vector2(-16f, 17f),
                Vector2.zero, new Vector2(1f, 0f));
            _healthFill = Image("Fill", healthBack.transform, Color.green);
            _healthFill.type = UnityEngine.UI.Image.Type.Filled;
            _healthFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            Rect(_healthFill.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

            _objective = Label("Objective", transform, font, 18, TextAnchor.MiddleCenter);
            Rect(_objective.rectTransform, new Vector2(-260f, -42f), new Vector2(260f, -8f), new Vector2(0.5f, 1f));
            _status = Label("Status", transform, font, 28, TextAnchor.MiddleCenter);
            Rect(_status.rectTransform, new Vector2(-260f, -88f), new Vector2(260f, -44f), new Vector2(0.5f, 1f));
            Text reticle = Label("Reticle", transform, font, 28, TextAnchor.MiddleCenter);
            reticle.text = "+";
            Rect(reticle.rectTransform, new Vector2(-20f, -20f), new Vector2(20f, 20f), new Vector2(0.5f, 0.5f));
            BuildScopeOverlay(font);

            Button repair = CreateButton(
                "Repair", "4", new Vector2(430f, 20f),
                () => _consumables[0] = true);
            Button firstAid = CreateButton(
                "FirstAid", "5", new Vector2(486f, 20f),
                () => _consumables[1] = true);
            Button extinguisher = CreateButton(
                "Extinguish", "6", new Vector2(542f, 20f),
                () => _consumables[2] = true);
            Button[] consumables = { repair, firstAid, extinguisher };
            for (int i = 0; i < consumables.Length; i++)
            {
                _consumableButtons[i] =
                    consumables[i].GetComponent<RectTransform>();
                _consumableLabels[i] =
                    consumables[i].GetComponentInChildren<Text>();
            }
            Button hydropneumatic = CreateButton(
                "HydropneumaticAim",
                "E",
                new Vector2(430f, 76f),
                null,
                new Vector2(64f, 40f));
            _hydropneumatic =
                new BattleHudHydropneumaticControl(
                    hydropneumatic);
            CreateButton("Garage", "GARAGE", new Vector2(20f, -48f), _garage,
                new Vector2(90f, 34f), new Vector2(0f, 1f));
            CreateButton("Settings", "SETTINGS", new Vector2(120f, -48f),
                () => _settingsPanel.Open(),
                new Vector2(96f, 34f), new Vector2(0f, 1f));
            BuildDamagePanel(font);
            _minimap = BattleMinimap.Create(transform, font);
            BuildTouchControls(font);
            BuildResultScreen(font);
            BuildReplayOverlay(font);
            _settingsPanel = GameSettingsPanel.Create(transform, _settings);
            _pause = new BattleHudPause(
                transform,
                _settingsPanel,
                ClearHeldInput);
            _accessibility = new BattleHudAccessibilityPresentation(
                transform,
                font,
                new[] { _vehiclePanel, _damagePanel },
                _consumableLabels,
                _settings);
            UiAudioFeedback.BindTree(transform, _settings);
        }
        private void OnDestroy()
        {
            _pause?.Dispose();
            _accessibility?.Dispose();
            _minimap?.Dispose();
        }

        private void Update()
        {
            _accessibility?.DetectActiveDevice();
            _pause?.Tick(ResultVisible || ReplayVisible);
            if (_touchRoot != null &&
                _touchRoot.activeSelf &&
                (_touchLayoutWidth != Screen.width || _touchLayoutHeight != Screen.height))
            {
                SetTouchLayoutForViewport(Screen.width, Screen.height);
            }
        }

        private void ClearHeldInput()
        {
            Array.Clear(_driveHeld, 0, _driveHeld.Length);
            _touchDrive = Vector2.zero;
            _fireHeld = false;
            _brakeHeld = false;
            _touchAimActive = false;
            _sniperToggleQueued = false;
            _touchShellSlot = -1;
            _hydropneumatic.Clear();
        }

        private void BuildDamagePanel(Font font)
        {
            Image panel = Image("DamagePanel", transform, new Color(0.035f, 0.045f, 0.04f, 0.88f));
            _damagePanel = panel;
            Rect(panel.rectTransform, new Vector2(-248f, -146f), new Vector2(-20f, -20f),
                Vector2.one);
            Text title = Label("Title", panel.transform, font, 13, TextAnchor.UpperLeft);
            title.text = "VEHICLE STATUS";
            title.color = new Color(0.72f, 0.78f, 0.8f);
            Rect(title.rectTransform, new Vector2(12f, -30f), new Vector2(-12f, -8f),
                new Vector2(0f, 1f), new Vector2(1f, 1f));
            _damageDetails = Label("DamageDetails", panel.transform, font, 12, TextAnchor.UpperLeft);
            _damageDetails.supportRichText = true;
            Rect(_damageDetails.rectTransform, new Vector2(12f, 10f), new Vector2(-12f, -36f),
                Vector2.zero, Vector2.one);
        }

        private void UpdateDamagePanel(DamageCombatState combat)
        {
            _damageText.Clear();
            if (combat.Fire.Burning)
            {
                _damageText.Append("<color=#ff633f>FIRE</color>");
            }

            AppendModule(combat, "gun", "GUN");
            AppendModule(combat, "turretRing", "TURRET");
            AppendModule(combat, "engine", "ENGINE");
            AppendModule(combat, "transmission", "TRANS");
            AppendModule(combat, "fuelTank", "FUEL");
            AppendModule(combat, "ammoRack", "AMMO");
            AppendModule(combat, "optics", "OPTICS");
            AppendModule(combat, "radio", "RADIO");
            AppendModule(combat, "trackL", "TRACK L");
            AppendModule(combat, "trackR", "TRACK R");

            foreach (var crew in combat.Crew)
            {
                if (crew.Value) continue;
                AppendLine("<color=#f05a5a>" + crew.Key.ToUpperInvariant() + " OUT</color>");
            }

            if (_damageText.Length == 0)
            {
                _damageText.Append("<color=#8fa29a>SYSTEMS NOMINAL</color>");
            }
            _damageDetails.text = _damageText.ToString();
        }

        private void AppendModule(DamageCombatState combat, string id, string label)
        {
            DamageModuleState module;
            if (!combat.Modules.TryGetValue(id, out module) ||
                module.Condition == DamageModuleCondition.Ok)
            {
                return;
            }

            string color = module.Condition == DamageModuleCondition.Red
                ? "#f05a5a"
                : "#f0b04a";
            AppendLine("<color=" + color + ">" + label + " " +
                module.Condition.ToString().ToUpperInvariant() + "</color>");
        }

        private void AppendLine(string value)
        {
            if (_damageText.Length > 0) _damageText.Append("   ");
            _damageText.Append(value);
        }

        private void UpdateResult(string status, bool battleOver, BattleHudStats stats)
        {
            if (!battleOver)
            {
                if (_resultShown)
                {
                    _resultShown = false;
                    _resultRoot.SetActive(false);
                }
                return;
            }
            if (_resultShown)
            {
                return;
            }

            _resultShown = true;
            _resultRoot.SetActive(true);
            _resultTitle.text = status;
            _resultTitle.color = status == "VICTORY"
                ? new Color(0.5f, 0.9f, 0.55f)
                : status == "DEFEAT"
                    ? new Color(0.95f, 0.38f, 0.34f)
                    : new Color(0.78f, 0.82f, 0.85f);
            float accuracy = stats.ShotsFired > 0
                ? stats.Hits * 100f / stats.ShotsFired
                : 0f;
            int minutes = Mathf.FloorToInt(stats.TimeS / 60f);
            int seconds = Mathf.FloorToInt(stats.TimeS) % 60;
            _resultStats.text = string.Format(
                "DAMAGE {0:0}     KILLS {1}     HITS {2}/{3} ({4:0}%)\n" +
                "PENETRATIONS {5}     RECEIVED {6:0}     TIME {7:00}:{8:00}",
                stats.DamageDealt,
                stats.Kills,
                stats.Hits,
                stats.ShotsFired,
                accuracy,
                stats.Penetrations,
                stats.DamageReceived,
                minutes,
                seconds);
        }

        private static string ModeLabel(GameModeId id)
        {
            if (id == GameModeId.CaptureTheFlag) return "CTF";
            if (id == GameModeId.ZoneControl) return "ZONES";
            if (id == GameModeId.TurboBall) return "TURBO";
            return id.ToString().ToUpperInvariant();
        }
    }
}
