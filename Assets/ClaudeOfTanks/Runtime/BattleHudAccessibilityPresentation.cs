using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ClaudeOfTanks.Runtime
{
    public enum BattleInputDevice
    {
        KeyboardMouse,
        Gamepad
    }

    internal sealed class BattleHudAccessibilityPresentation : IDisposable
    {
        private readonly GameSettings _settings;
        private readonly Image[] _contrastPanels;
        private readonly Color[] _panelColors;
        private readonly List<Outline> _textOutlines =
            new List<Outline>();
        private readonly List<Text> _scaledText =
            new List<Text>();
        private readonly List<int> _baseFontSizes =
            new List<int>();
        private readonly Text[] _consumableLabels;
        private readonly Text _fire;
        private readonly Text _scope;
        private readonly Text _brake;
        private BattleInputDevice _device;

        public BattleHudAccessibilityPresentation(
            Transform root,
            Font font,
            Image[] contrastPanels,
            Text[] consumableLabels,
            GameSettings settings = null)
        {
            _settings = settings ?? GameSettings.Current;
            _consumableLabels = consumableLabels;
            _contrastPanels = new Image[contrastPanels.Length + 1];
            _panelColors = new Color[contrastPanels.Length + 1];
            for (int i = 0; i < contrastPanels.Length; i++)
            {
                _contrastPanels[i] = contrastPanels[i];
                _panelColors[i] = contrastPanels[i].color;
            }

            Image promptPanel = CreateImage(
                "InputPrompts",
                root,
                new Color(0.035f, 0.045f, 0.04f, 0.88f));
            RectTransform promptRect = promptPanel.rectTransform;
            promptRect.anchorMin = new Vector2(1f, 0f);
            promptRect.anchorMax = new Vector2(1f, 0f);
            promptRect.offsetMin = new Vector2(-670f, 20f);
            promptRect.offsetMax = new Vector2(-250f, 58f);
            _fire = CreatePrompt("Fire", promptPanel.transform, font,
                new Vector2(10f, 0f), new Vector2(120f, 38f));
            _scope = CreatePrompt("Scope", promptPanel.transform, font,
                new Vector2(124f, 0f), new Vector2(260f, 38f));
            _brake = CreatePrompt("Brake", promptPanel.transform, font,
                new Vector2(264f, 0f), new Vector2(410f, 38f));

            _contrastPanels[_contrastPanels.Length - 1] = promptPanel;
            _panelColors[_panelColors.Length - 1] = promptPanel.color;
            AddTextOutlines(root);
            _settings.PresentationChanged += OnSettingsChanged;
            Apply();
            RefreshPrompts();
        }

        public void SetInputDevice(BattleInputDevice device)
        {
            if (_device == device)
            {
                return;
            }
            _device = device;
            RefreshPrompts();
        }

        public void DetectActiveDevice()
        {
#if ENABLE_INPUT_SYSTEM
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null &&
                (gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f ||
                gamepad.rightStick.ReadValue().sqrMagnitude > 0.04f ||
                gamepad.leftTrigger.isPressed ||
                gamepad.rightTrigger.isPressed ||
                gamepad.buttonSouth.isPressed ||
                gamepad.buttonEast.isPressed ||
                gamepad.buttonWest.isPressed ||
                gamepad.buttonNorth.isPressed))
            {
                SetInputDevice(BattleInputDevice.Gamepad);
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if ((keyboard != null && keyboard.anyKey.isPressed) ||
                (mouse != null &&
                (mouse.leftButton.isPressed ||
                mouse.rightButton.isPressed ||
                mouse.delta.ReadValue().sqrMagnitude > 0.01f)))
            {
                SetInputDevice(BattleInputDevice.KeyboardMouse);
            }
#endif
        }

        public Color HealthColor(float fill)
        {
            if (_settings.HighContrast)
            {
                return fill > 0.35f
                    ? new Color(0.2f, 1f, 0.32f)
                    : new Color(1f, 0.18f, 0.08f);
            }
            return fill > 0.35f
                ? new Color(0.28f, 0.76f, 0.3f)
                : new Color(0.9f, 0.18f, 0.1f);
        }

        public void Dispose()
        {
            _settings.PresentationChanged -= OnSettingsChanged;
        }

        private void OnSettingsChanged()
        {
            Apply();
            RefreshPrompts();
        }

        private void Apply()
        {
            float scale = _settings.HudScale;
            for (int i = 0; i < _contrastPanels.Length; i++)
            {
                Color color = _panelColors[i];
                if (_settings.HighContrast)
                {
                    color = new Color(0.006f, 0.008f, 0.007f, 1f);
                }
                _contrastPanels[i].color = color;
            }
            for (int i = 0; i < _textOutlines.Count; i++)
            {
                _textOutlines[i].enabled = _settings.HighContrast;
            }
            for (int i = 0; i < _scaledText.Count; i++)
            {
                int size = Mathf.RoundToInt(_baseFontSizes[i] * scale);
                _scaledText[i].fontSize = size;
                if (_scaledText[i].resizeTextForBestFit)
                {
                    _scaledText[i].resizeTextMaxSize = size;
                }
            }
        }

        private void RefreshPrompts()
        {
            bool gamepad = _device == BattleInputDevice.Gamepad;
            _fire.text = gamepad
                ? "FIRE  RT"
                : "FIRE  " + KeyLabel(
                    _settings.GetBinding(GameInputAction.Fire));
            _scope.text = gamepad
                ? "SCOPE  LT"
                : "SCOPE  " + KeyLabel(
                    _settings.GetBinding(GameInputAction.Sniper));
            _brake.text = gamepad
                ? "BRAKE  A"
                : "BRAKE  " + KeyLabel(
                    _settings.GetBinding(GameInputAction.Brake));
            string[] labels = gamepad
                ? new[] { "X", "Y", "B" }
                : new[]
                {
                    KeyLabel(_settings.GetBinding(GameInputAction.Repair)),
                    KeyLabel(_settings.GetBinding(GameInputAction.FirstAid)),
                    KeyLabel(_settings.GetBinding(GameInputAction.Extinguisher))
                };
            for (int i = 0; i < _consumableLabels.Length; i++)
            {
                _consumableLabels[i].text = labels[i];
            }
        }

        private void AddTextOutlines(Transform root)
        {
            Text[] labels = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].GetComponentInParent<GameSettingsPanel>() != null)
                {
                    continue;
                }
                Outline outline = labels[i].gameObject
                    .GetComponent<Outline>();
                if (outline == null)
                {
                    outline = labels[i].gameObject
                        .AddComponent<Outline>();
                }
                outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
                outline.effectDistance = new Vector2(1f, -1f);
                _textOutlines.Add(outline);
                _scaledText.Add(labels[i]);
                _baseFontSizes.Add(labels[i].fontSize);
            }
        }

        private static Text CreatePrompt(
            string name,
            Transform parent,
            Font font,
            Vector2 min,
            Vector2 max)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.offsetMin = min;
            rect.offsetMax = max;
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = 12;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.9f, 0.93f, 0.9f);
            return text;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Color color)
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

        private static string KeyLabel(KeyCode key)
        {
            string value = key.ToString();
            if (value.StartsWith("Alpha", StringComparison.Ordinal))
            {
                value = value.Substring(5);
            }
            if (value == "LeftControl") return "L CTRL";
            if (value == "LeftShift") return "L SHIFT";
            if (value == "Space") return "SPACE";
            return value.ToUpperInvariant();
        }
    }
}
