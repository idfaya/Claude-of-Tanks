using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed class GameSettingsPanel : MonoBehaviour
    {
        private readonly Dictionary<GameInputAction, Button> _bindingButtons =
            new Dictionary<GameInputAction, Button>();
        private GameSettings _settings;
        private GameObject _panel;
        private Image _shade;
        private Image _surfaceImage;
        private RectTransform _surface;
        private Slider _volume;
        private GameSettingsAudioSection _audioMix;
        private Dropdown _quality;
        private Toggle _fullscreen;
        private Toggle _reducedMotion;
        private Toggle _highContrast;
        private Slider _hudScale;
        private GameInputAction? _waitingForBinding;
        private int _layoutWidth;
        private int _layoutHeight;

        public bool IsVisible => _panel != null && _panel.activeSelf;
        public GameInputAction? WaitingForBinding => _waitingForBinding;

        public static GameSettingsPanel Create(Transform parent, GameSettings settings)
        {
            GameObject root = new GameObject("Settings", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            Stretch(rect);
            GameSettingsPanel panel = root.AddComponent<GameSettingsPanel>();
            panel._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            panel.Build();
            panel.Close();
            return panel;
        }

        public void Open()
        {
            _waitingForBinding = null;
            Refresh();
            _panel.SetActive(true);
            transform.SetAsLastSibling();
            ApplyViewportLayout();
        }

        public void Close()
        {
            _waitingForBinding = null;
            if (_panel != null) _panel.SetActive(false);
        }

        public void BeginRebind(GameInputAction action)
        {
            if (!Enum.IsDefined(typeof(GameInputAction), action))
                throw new ArgumentOutOfRangeException(nameof(action));
            _waitingForBinding = action;
            RefreshBindings();
        }

        public void CaptureBinding(KeyCode key)
        {
            if (!_waitingForBinding.HasValue) return;
            _settings.SetBinding(_waitingForBinding.Value, key);
            _waitingForBinding = null;
            RefreshBindings();
        }

        private void Update()
        {
            if (IsVisible && (_layoutWidth != Screen.width || _layoutHeight != Screen.height))
                ApplyViewportLayout();
            if (!_waitingForBinding.HasValue) return;
            for (int i = 0; i < GameSettings.AllowedBindings.Length; i++)
            {
                KeyCode key = GameSettings.AllowedBindings[i];
                if (!GameSettings.WasKeyPressedThisFrame(key)) continue;
                CaptureBinding(key);
                return;
            }
        }

        private void Build()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Image shade = Image("Shade", transform, new Color(0.01f, 0.015f, 0.018f, 0.84f));
            _shade = shade;
            Stretch(shade.rectTransform);
            _panel = shade.gameObject;

            Image surface = Image("Surface", shade.transform, new Color(0.055f, 0.065f, 0.062f, 0.98f));
            _surfaceImage = surface;
            RectTransform surfaceRect = surface.rectTransform;
            _surface = surfaceRect;
            surfaceRect.anchorMin = new Vector2(0.5f, 0.5f);
            surfaceRect.anchorMax = new Vector2(0.5f, 0.5f);
            surfaceRect.sizeDelta = new Vector2(700f, 700f);
            surfaceRect.anchoredPosition = Vector2.zero;

            Text title = Label("Title", surface.transform, font, 26, TextAnchor.MiddleLeft);
            title.text = "SETTINGS";
            PlaceHorizontal(title.rectTransform, 28f, -28f, -58f, -16f);

            Text audioTitle = Label("AudioTitle", surface.transform, font, 13, TextAnchor.MiddleLeft);
            audioTitle.text = "MASTER";
            audioTitle.color = new Color(0.68f, 0.75f, 0.72f);
            Place(audioTitle.rectTransform, new Vector2(28f, -104f), new Vector2(132f, -78f));
            _volume = Slider("Volume", surface.transform);
            Place(_volume.GetComponent<RectTransform>(),
                new Vector2(140f, -100f),
                new Vector2(320f, -82f));
            _volume.minValue = 0f;
            _volume.maxValue = 1f;
            _volume.onValueChanged.AddListener(_settings.SetMasterVolume);

            Text qualityTitle = Label("QualityTitle", surface.transform, font, 13, TextAnchor.MiddleLeft);
            qualityTitle.text = "QUALITY";
            qualityTitle.color = new Color(0.68f, 0.75f, 0.72f);
            Place(qualityTitle.rectTransform, new Vector2(350f, -104f), new Vector2(424f, -78f));
            _quality = Dropdown("Quality", surface.transform, font);
            Place(_quality.GetComponent<RectTransform>(), new Vector2(424f, -108f), new Vector2(520f, -74f));
            List<string> qualityNames = new List<string>(QualitySettings.names);
            if (qualityNames.Count == 0) qualityNames.Add("Default");
            _quality.AddOptions(qualityNames);
            _quality.onValueChanged.AddListener(_settings.SetQualityLevel);

            _fullscreen = Toggle("Fullscreen", surface.transform, font, "FULLSCREEN");
            Place(_fullscreen.GetComponent<RectTransform>(),
                new Vector2(528f, -108f),
                new Vector2(672f, -74f));
            _fullscreen.onValueChanged.AddListener(_settings.SetFullscreen);
            _audioMix = new GameSettingsAudioSection(
                surface.transform,
                font,
                _settings);

            Text controlsTitle = Label("ControlsTitle", surface.transform, font, 13, TextAnchor.MiddleLeft);
            controlsTitle.text = "CONTROLS";
            controlsTitle.color = new Color(0.68f, 0.75f, 0.72f);
            PlaceHorizontal(controlsTitle.rectTransform, 28f, -28f, -232f, -204f);

            Array actions = Enum.GetValues(typeof(GameInputAction));
            for (int i = 0; i < actions.Length; i++)
            {
                GameInputAction action = (GameInputAction)actions.GetValue(i);
                int column = i / 5;
                int row = i % 5;
                float x = 28f + column * 326f;
                float y = -274f - row * 58f;
                Text label = Label(action + "Label", surface.transform, font, 13, TextAnchor.MiddleLeft);
                label.text = ActionLabel(action);
                Place(label.rectTransform, new Vector2(x, y), new Vector2(x + 150f, y + 38f));
                Button button = Button(action + "Binding", surface.transform, font, string.Empty);
                Place(button.GetComponent<RectTransform>(),
                    new Vector2(x + 158f, y),
                    new Vector2(x + 292f, y + 38f));
                GameInputAction captured = action;
                button.onClick.AddListener(() => BeginRebind(captured));
                _bindingButtons.Add(action, button);
            }

            Text accessibilityTitle = Label(
                "AccessibilityTitle",
                surface.transform,
                font,
                13,
                TextAnchor.MiddleLeft);
            accessibilityTitle.text = "ACCESSIBILITY";
            accessibilityTitle.color = new Color(0.68f, 0.75f, 0.72f);
            PlaceHorizontal(
                accessibilityTitle.rectTransform,
                28f,
                -28f,
                -550f,
                -522f);

            _reducedMotion = Toggle(
                "ReducedMotion",
                surface.transform,
                font,
                "REDUCED MOTION");
            Place(
                _reducedMotion.GetComponent<RectTransform>(),
                new Vector2(28f, -596f),
                new Vector2(224f, -562f));
            _reducedMotion.onValueChanged.AddListener(
                _settings.SetReducedMotion);

            _highContrast = Toggle(
                "HighContrast",
                surface.transform,
                font,
                "HIGH CONTRAST");
            Place(
                _highContrast.GetComponent<RectTransform>(),
                new Vector2(238f, -596f),
                new Vector2(430f, -562f));
            _highContrast.onValueChanged.AddListener(value =>
            {
                _settings.SetHighContrast(value);
                ApplyAccessibilityStyle();
            });

            Text hudScaleTitle = Label(
                "HudScaleTitle",
                surface.transform,
                font,
                13,
                TextAnchor.MiddleLeft);
            hudScaleTitle.text = "HUD SCALE";
            Place(
                hudScaleTitle.rectTransform,
                new Vector2(450f, -596f),
                new Vector2(530f, -562f));
            _hudScale = Slider("HudScale", surface.transform);
            Place(
                _hudScale.GetComponent<RectTransform>(),
                new Vector2(536f, -592f),
                new Vector2(672f, -566f));
            _hudScale.minValue = GameSettings.MinimumHudScale;
            _hudScale.maxValue = GameSettings.MaximumHudScale;
            _hudScale.onValueChanged.AddListener(_settings.SetHudScale);

            Button reset = Button("Reset", surface.transform, font, "RESET");
            Place(reset.GetComponent<RectTransform>(), new Vector2(28f, 22f), new Vector2(178f, 66f),
                Vector2.zero);
            reset.onClick.AddListener(() =>
            {
                _settings.ResetDefaults();
                Refresh();
            });
            Button close = Button("Close", surface.transform, font, "DONE");
            Place(close.GetComponent<RectTransform>(), new Vector2(-178f, 22f), new Vector2(-28f, 66f),
                new Vector2(1f, 0f));
            close.onClick.AddListener(Close);
        }

        private void Refresh()
        {
            _volume.SetValueWithoutNotify(_settings.MasterVolume);
            _audioMix.Refresh();
            _quality.SetValueWithoutNotify(_settings.QualityLevel);
            _fullscreen.SetIsOnWithoutNotify(_settings.Fullscreen);
            _reducedMotion.SetIsOnWithoutNotify(
                _settings.ReducedMotion);
            _highContrast.SetIsOnWithoutNotify(
                _settings.HighContrast);
            _hudScale.SetValueWithoutNotify(_settings.HudScale);
            ApplyAccessibilityStyle();
            RefreshBindings();
        }

        private void ApplyAccessibilityStyle()
        {
            bool highContrast = _settings.HighContrast;
            _shade.color = highContrast
                ? new Color(0f, 0f, 0f, 0.94f)
                : new Color(0.01f, 0.015f, 0.018f, 0.84f);
            _surfaceImage.color = highContrast
                ? new Color(0.015f, 0.018f, 0.016f, 1f)
                : new Color(0.055f, 0.065f, 0.062f, 0.98f);
        }

        private void RefreshBindings()
        {
            foreach (KeyValuePair<GameInputAction, Button> pair in _bindingButtons)
            {
                Text text = pair.Value.GetComponentInChildren<Text>();
                text.text = _waitingForBinding == pair.Key
                    ? "PRESS KEY"
                    : BindingLabel(_settings.GetBinding(pair.Key));
            }
        }

        private void ApplyViewportLayout()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            float canvasScale = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
            float widthScale = Mathf.Min(700f, Screen.width - 32f) / (700f * canvasScale);
            float heightScale = Mathf.Min(700f, Screen.height - 32f) / (700f * canvasScale);
            float scale = Mathf.Min(widthScale, heightScale);
            _surface.localScale = Vector3.one * Mathf.Clamp(scale, 0.65f, 1.8f);
            _layoutWidth = Screen.width;
            _layoutHeight = Screen.height;
        }

        private static string ActionLabel(GameInputAction action)
        {
            switch (action)
            {
                case GameInputAction.FirstAid: return "FIRST AID";
                case GameInputAction.Extinguisher: return "EXTINGUISHER";
                default: return action.ToString().ToUpperInvariant();
            }
        }

        private static string BindingLabel(KeyCode key)
        {
            string value = key.ToString();
            if (value.StartsWith("Alpha", StringComparison.Ordinal)) value = value.Substring(5);
            if (value == "LeftControl") value = "L CTRL";
            if (value == "LeftShift") value = "L SHIFT";
            return value.ToUpperInvariant();
        }

        private static Slider Slider(string name, Transform parent)
        {
            GameObject root = DefaultControls.CreateSlider(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Transform background = root.transform.Find("Background");
            if (background != null)
                background.GetComponent<Image>().color = new Color(0.13f, 0.16f, 0.15f);
            Transform fill = root.transform.Find("Fill Area/Fill");
            if (fill != null)
                fill.GetComponent<Image>().color = new Color(0.3f, 0.75f, 0.38f);
            return root.GetComponent<Slider>();
        }

        private static Dropdown Dropdown(string name, Transform parent, Font font)
        {
            GameObject root = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.14f);
            Transform arrow = root.transform.Find("Arrow");
            if (arrow != null)
            {
                arrow.gameObject.SetActive(false);
            }
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.color = Color.white;
            }
            Text chevron = Label(
                "Chevron",
                root.transform,
                font,
                10,
                TextAnchor.MiddleCenter);
            chevron.text = "V";
            chevron.rectTransform.anchorMin = new Vector2(1f, 0f);
            chevron.rectTransform.anchorMax = Vector2.one;
            chevron.rectTransform.offsetMin = new Vector2(-28f, 0f);
            chevron.rectTransform.offsetMax = Vector2.zero;
            return root.GetComponent<Dropdown>();
        }

        private static Toggle Toggle(string name, Transform parent, Font font, string label)
        {
            GameObject root = DefaultControls.CreateToggle(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Transform background = root.transform.Find("Background");
            if (background != null)
            {
                background.GetComponent<Image>().color =
                    new Color(0.12f, 0.15f, 0.14f);
            }
            Transform checkmark = root.transform.Find(
                "Background/Checkmark");
            if (checkmark != null)
            {
                checkmark.GetComponent<Image>().color =
                    new Color(0.3f, 0.82f, 0.38f);
            }
            Text text = root.GetComponentInChildren<Text>();
            text.font = font;
            text.fontSize = 13;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 13;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.text = label;
            text.color = Color.white;
            return root.GetComponent<Toggle>();
        }

        private static Button Button(string name, Transform parent, Font font, string label)
        {
            GameObject root = DefaultControls.CreateButton(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.14f);
            Text text = root.GetComponentInChildren<Text>();
            text.font = font;
            text.fontSize = 13;
            text.text = label;
            text.color = Color.white;
            return root.GetComponent<Button>();
        }

        private static Image Image(string name, Transform parent, Color color)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Label(
            string name,
            Transform parent,
            Font font,
            int size,
            TextAnchor alignment)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            root.transform.SetParent(parent, false);
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(
            RectTransform rect,
            Vector2 min,
            Vector2 max,
            Vector2? anchor = null)
        {
            rect.anchorMin = anchor ?? new Vector2(0f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private static void PlaceHorizontal(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float top)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }
    }
}
