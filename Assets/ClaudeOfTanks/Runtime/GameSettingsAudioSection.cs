using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class GameSettingsAudioSection
    {
        private readonly List<SliderBinding> _bindings =
            new List<SliderBinding>();

        public GameSettingsAudioSection(
            Transform parent,
            Font font,
            GameSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            GameObject root = new GameObject(
                "AudioMix",
                typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(28f, -208f);
            rect.offsetMax = new Vector2(-28f, -118f);

            Add(
                root.transform,
                font,
                "EngineVolume",
                "ENGINE",
                0,
                0,
                settings.SetEngineVolume,
                () => settings.EngineVolume);
            Add(
                root.transform,
                font,
                "CombatVolume",
                "COMBAT",
                1,
                0,
                settings.SetCombatVolume,
                () => settings.CombatVolume);
            Add(
                root.transform,
                font,
                "AmbienceVolume",
                "AMBIENCE",
                0,
                1,
                settings.SetAmbienceVolume,
                () => settings.AmbienceVolume);
            Add(
                root.transform,
                font,
                "UiVolume",
                "UI",
                1,
                1,
                settings.SetUiVolume,
                () => settings.UiVolume);
            Add(
                root.transform,
                font,
                "VoiceVolume",
                "VOICE",
                0,
                2,
                settings.SetVoiceVolume,
                () => settings.VoiceVolume);
        }

        public void Refresh()
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                SliderBinding binding = _bindings[i];
                binding.Slider.SetValueWithoutNotify(binding.Read());
            }
        }

        private void Add(
            Transform parent,
            Font font,
            string name,
            string label,
            int column,
            int row,
            UnityEngine.Events.UnityAction<float> write,
            Func<float> read)
        {
            float x = column * 322f;
            float y = -row * 30f;
            Text title = Label(name + "Label", parent, font);
            title.text = label;
            Place(
                title.rectTransform,
                new Vector2(x, y - 24f),
                new Vector2(x + 78f, y));
            Slider slider = Slider(name, parent);
            Place(
                slider.GetComponent<RectTransform>(),
                new Vector2(x + 82f, y - 21f),
                new Vector2(x + 294f, y - 3f));
            slider.onValueChanged.AddListener(write);
            _bindings.Add(new SliderBinding
            {
                Slider = slider,
                Read = read
            });
        }

        private static Slider Slider(string name, Transform parent)
        {
            GameObject root = DefaultControls.CreateSlider(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Transform background = root.transform.Find("Background");
            if (background != null)
            {
                background.GetComponent<Image>().color =
                    new Color(0.13f, 0.16f, 0.15f);
            }
            Transform fill = root.transform.Find("Fill Area/Fill");
            if (fill != null)
            {
                fill.GetComponent<Image>().color =
                    new Color(0.3f, 0.75f, 0.38f);
            }
            Slider slider = root.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        private static Text Label(
            string name,
            Transform parent,
            Font font)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            root.transform.SetParent(parent, false);
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = 11;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = new Color(0.68f, 0.75f, 0.72f);
            return text;
        }

        private static void Place(
            RectTransform rect,
            Vector2 min,
            Vector2 max)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private sealed class SliderBinding
        {
            public Slider Slider;
            public Func<float> Read;
        }
    }
}
