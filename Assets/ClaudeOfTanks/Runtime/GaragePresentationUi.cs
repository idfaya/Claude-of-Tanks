using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class GaragePresentationUi : MonoBehaviour
    {
        private static readonly Color Surface =
            new Color(0.025f, 0.032f, 0.034f, 0.94f);
        private static readonly Color SurfaceRaised =
            new Color(0.055f, 0.068f, 0.069f, 0.98f);
        private static readonly Color TextPrimary =
            new Color(0.91f, 0.94f, 0.93f);
        private static readonly Color TextMuted =
            new Color(0.48f, 0.56f, 0.55f);
        private static readonly Color Accent =
            new Color(0.86f, 0.62f, 0.16f);
        private static readonly Color Tactical =
            new Color(0.32f, 0.69f, 0.43f);

        private Font _font;
        private RectTransform _topRail;
        private RectTransform _selectionRail;
        private RectTransform _statusRail;
        private RectTransform _commandBand;
        private CanvasGroup _topGroup;
        private CanvasGroup _selectionGroup;
        private CanvasGroup _statusGroup;
        private CanvasGroup _commandGroup;
        private Text _vehicleName;
        private Text _vehicleMeta;
        private Text _firepower;
        private Text _mobility;
        private Text _survivability;
        private Text _battleContext;
        private float _entrance;
        private float _statusPulse = 1f;

        public Dropdown Vehicle { get; private set; }
        public Dropdown Map { get; private set; }
        public Dropdown Mode { get; private set; }
        public Transform ModalRoot => transform;

        public static GaragePresentationUi Create(
            Transform parent,
            Font font)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            GameObject root = new GameObject(
                "GarageUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1280f, 720f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GaragePresentationUi ui =
                root.AddComponent<GaragePresentationUi>();
            ui._font = font;
            ui.Build();
            return ui;
        }

        public Button CreateActionButton(
            string name,
            string label)
        {
            return GarageUiPrimitives.ActionButton(
                name,
                label,
                _font,
                _topRail,
                _selectionRail,
                _commandBand,
                SurfaceRaised,
                TextPrimary,
                Accent);
        }

        public void UpdateVehicle(
            VehicleDefinition vehicle,
            MapDefinition map,
            string mode,
            string camouflage,
            int equipmentCount)
        {
            if (vehicle == null || map == null) return;
            VehicleShell shell =
                vehicle.gun != null &&
                vehicle.gun.shells != null &&
                vehicle.gun.shells.Length > 0
                    ? vehicle.gun.shells[0]
                    : null;
            float powerToWeight =
                vehicle.weightTons > 0f
                    ? vehicle.enginePowerHp /
                        vehicle.weightTons
                    : 0f;

            _vehicleName.text =
                vehicle.name.ToUpperInvariant();
            _vehicleMeta.text = string.Format(
                "{0}  //  {1}  //  {2}",
                Upper(vehicle.nation),
                Upper(vehicle.role),
                Upper(vehicle.era));
            _firepower.text = string.Format(
                "FIREPOWER\n{0:0} MM  /  {1:0} DAMAGE  /  {2:0.0} S",
                vehicle.gun != null
                    ? vehicle.gun.caliberMm
                    : 0f,
                shell != null ? shell.dmg : 0f,
                vehicle.gun != null
                    ? vehicle.gun.reloadS
                    : 0f);
            _mobility.text = string.Format(
                "MOBILITY\n{0:0} KM/H  /  {1:0.0} HP/T",
                vehicle.topSpeedKmh,
                powerToWeight);
            _survivability.text = string.Format(
                "SURVIVABILITY\n{0:0} HP  /  {1:0.0} T",
                vehicle.hp,
                vehicle.weightTons);
            _battleContext.text = string.Format(
                "STAGING\n{0}  //  {1}\nPAINT  {2}\nLOADOUT  {3}/3",
                Upper(map.id),
                mode,
                camouflage,
                equipmentCount);
            _statusPulse = 0.72f;
        }

        private void Build()
        {
            _topRail = Panel(
                "TopBrandRail",
                Surface,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -72f),
                Vector2.zero);
            _selectionRail = Panel(
                "SelectionRail",
                Surface,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, 96f),
                new Vector2(340f, -72f));
            _statusRail = Panel(
                "VehicleStatus",
                Surface,
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(-310f, 112f),
                new Vector2(0f, -72f));
            _commandBand = Panel(
                "CommandBand",
                new Color(0.018f, 0.024f, 0.025f, 0.96f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                Vector2.zero,
                new Vector2(0f, 96f));

            _topGroup =
                _topRail.gameObject.AddComponent<CanvasGroup>();
            _selectionGroup =
                _selectionRail.gameObject.AddComponent<CanvasGroup>();
            _statusGroup =
                _statusRail.gameObject.AddComponent<CanvasGroup>();
            _commandGroup =
                _commandBand.gameObject.AddComponent<CanvasGroup>();
            _topGroup.alpha = 0f;
            _selectionGroup.alpha = 0f;
            _statusGroup.alpha = 0f;
            _commandGroup.alpha = 0f;

            BuildBrand();
            BuildSelection();
            BuildStatus();
            BuildCommandBand();
        }

        private void BuildBrand()
        {
            Text title = Label(
                "Title",
                _topRail,
                22,
                TextAnchor.MiddleLeft,
                TextPrimary);
            title.text = "CLAUDE OF TANKS";
            Place(
                title.rectTransform,
                new Vector2(22f, -56f),
                new Vector2(330f, -14f),
                new Vector2(0f, 1f));

            Text context = Label(
                "Context",
                _topRail,
                9,
                TextAnchor.MiddleLeft,
                Accent);
            context.text =
                "ARMORED COMBAT SYSTEMS  /  GARAGE";
            Place(
                context.rectTransform,
                new Vector2(344f, -51f),
                new Vector2(650f, -19f),
                new Vector2(0f, 1f));
        }

        private void BuildSelection()
        {
            Text heading = Label(
                "Heading",
                _selectionRail,
                11,
                TextAnchor.MiddleLeft,
                Accent);
            heading.text = "DEPLOYMENT CONFIGURATION";
            Place(
                heading.rectTransform,
                new Vector2(24f, -48f),
                new Vector2(316f, -18f),
                new Vector2(0f, 1f));

            Vehicle = Selector(
                "Vehicle",
                "VEHICLE",
                -70f);
            Map = Selector(
                "Map",
                "BATTLEFIELD",
                -154f);
            Mode = Selector(
                "Mode",
                "COMBAT MODE",
                -238f);

            Image divider = Image(
                "ActionDivider",
                _selectionRail,
                new Color(
                    Accent.r,
                    Accent.g,
                    Accent.b,
                    0.52f));
            Place(
                divider.rectTransform,
                new Vector2(24f, 178f),
                new Vector2(316f, 180f),
                Vector2.zero);
        }

        private void BuildStatus()
        {
            Text caption = Label(
                "Caption",
                _statusRail,
                10,
                TextAnchor.MiddleLeft,
                Tactical);
            caption.text = "SELECTED VEHICLE";
            Place(
                caption.rectTransform,
                new Vector2(22f, -43f),
                new Vector2(288f, -17f),
                new Vector2(0f, 1f));

            _vehicleName = Label(
                "VehicleName",
                _statusRail,
                21,
                TextAnchor.MiddleLeft,
                TextPrimary);
            _vehicleName.resizeTextForBestFit = true;
            _vehicleName.resizeTextMinSize = 14;
            _vehicleName.resizeTextMaxSize = 21;
            Place(
                _vehicleName.rectTransform,
                new Vector2(22f, -86f),
                new Vector2(288f, -46f),
                new Vector2(0f, 1f));

            _vehicleMeta = Label(
                "VehicleMeta",
                _statusRail,
                9,
                TextAnchor.MiddleLeft,
                TextMuted);
            Place(
                _vehicleMeta.rectTransform,
                new Vector2(22f, -111f),
                new Vector2(288f, -87f),
                new Vector2(0f, 1f));

            _firepower = Metric(
                "Firepower",
                -142f);
            _mobility = Metric(
                "Mobility",
                -208f);
            _survivability = Metric(
                "Survivability",
                -274f);

            _battleContext = Label(
                "BattleContext",
                _statusRail,
                10,
                TextAnchor.UpperLeft,
                TextMuted);
            _battleContext.lineSpacing = 1.25f;
            Place(
                _battleContext.rectTransform,
                new Vector2(22f, -440f),
                new Vector2(288f, -348f),
                new Vector2(0f, 1f));
        }

        private void BuildCommandBand()
        {
            Text hint = Label(
                "Readiness",
                _commandBand,
                10,
                TextAnchor.MiddleLeft,
                TextMuted);
            hint.text =
                "SYSTEMS READY  //  60 HZ AUTHORITY";
            Place(
                hint.rectTransform,
                new Vector2(24f, 28f),
                new Vector2(340f, 68f),
                Vector2.zero);

            Text roster = Label(
                "Roster",
                _commandBand,
                10,
                TextAnchor.MiddleRight,
                TextMuted);
            roster.text =
                "126 VEHICLES  //  20 BATTLEFIELDS";
            Place(
                roster.rectTransform,
                new Vector2(-350f, 28f),
                new Vector2(-24f, 68f),
                new Vector2(1f, 0f));
        }

        private Dropdown Selector(
            string name,
            string caption,
            float top)
        {
            return GarageUiPrimitives.Selector(
                name,
                caption,
                top,
                _selectionRail,
                _font,
                SurfaceRaised,
                TextMuted,
                TextPrimary);
        }

        private Text Metric(string name, float top)
        {
            Image line = Image(
                name + "Line",
                _statusRail,
                new Color(1f, 1f, 1f, 0.075f));
            Place(
                line.rectTransform,
                new Vector2(22f, top - 1f),
                new Vector2(288f, top),
                new Vector2(0f, 1f));
            Text value = Label(
                name,
                _statusRail,
                11,
                TextAnchor.MiddleLeft,
                TextPrimary);
            value.lineSpacing = 1.25f;
            Place(
                value.rectTransform,
                new Vector2(22f, top - 58f),
                new Vector2(288f, top - 4f),
                new Vector2(0f, 1f));
            return value;
        }

        private RectTransform Panel(
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            return GarageUiPrimitives.Panel(
                name,
                transform,
                color,
                anchorMin,
                anchorMax,
                offsetMin,
                offsetMax);
        }

        private Text Label(
            string name,
            Transform parent,
            int size,
            TextAnchor alignment,
            Color color)
        {
            return GarageUiPrimitives.Label(
                name,
                parent,
                _font,
                size,
                alignment,
                color);
        }

        private static Image Image(
            string name,
            Transform parent,
            Color color)
        {
            return GarageUiPrimitives.Image(
                name,
                parent,
                color);
        }

        private static void Place(
            RectTransform rect,
            Vector2 min,
            Vector2 max,
            Vector2 anchor)
        {
            GarageUiPrimitives.Place(
                rect,
                min,
                max,
                anchor);
        }

        private static string Upper(string value)
        {
            return string.IsNullOrEmpty(value)
                ? "N/A"
                : value.Replace('-', ' ').ToUpperInvariant();
        }

        private void Update()
        {
            _entrance = Mathf.Min(
                1f,
                _entrance + Time.unscaledDeltaTime * 4.5f);
            _topGroup.alpha =
                Smooth(Mathf.InverseLerp(0f, 0.45f, _entrance));
            _selectionGroup.alpha =
                Smooth(Mathf.InverseLerp(0.12f, 0.78f, _entrance));
            _statusGroup.alpha =
                Smooth(Mathf.InverseLerp(0.25f, 0.92f, _entrance)) *
                _statusPulse;
            _commandGroup.alpha =
                Smooth(Mathf.InverseLerp(0.38f, 1f, _entrance));
            _statusPulse = Mathf.MoveTowards(
                _statusPulse,
                1f,
                Time.unscaledDeltaTime * 3.8f);
        }

        private static float Smooth(float value)
        {
            return value * value * (3f - 2f * value);
        }
    }
}
