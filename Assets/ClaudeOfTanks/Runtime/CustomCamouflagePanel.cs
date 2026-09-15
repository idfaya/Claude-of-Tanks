using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class CustomCamouflagePanel :
        MonoBehaviour
    {
        private static readonly string[] Schemes =
        {
            "solid",
            "organic",
            "digital",
            "stripes",
            "geometric",
            "dots"
        };

        private GameObject _surface;
        private Dropdown _scheme;
        private InputField _base;
        private InputField _weather;
        private InputField _patchA;
        private InputField _patchB;
        private Slider _scale;
        private Text _scaleValue;
        private Action _saved;

        public static CustomCamouflagePanel Create(
            Transform parent)
        {
            GameObject root = new GameObject(
                "CustomCamouflageStudio",
                typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect =
                root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            CustomCamouflagePanel panel =
                root.AddComponent<
                    CustomCamouflagePanel>();
            panel.Build();
            panel.Close();
            return panel;
        }

        public void Open(Action saved)
        {
            _saved = saved;
            CustomCamouflageProfile profile =
                CustomCamouflageStore.Load();
            _scheme.SetValueWithoutNotify(
                Math.Max(
                    0,
                    Array.IndexOf(
                        Schemes,
                        profile.scheme)));
            _base.SetTextWithoutNotify(
                profile.baseColor);
            _weather.SetTextWithoutNotify(
                profile.weatherColor);
            _patchA.SetTextWithoutNotify(
                profile.patchColorA);
            _patchB.SetTextWithoutNotify(
                profile.patchColorB);
            _scale.SetValueWithoutNotify(
                profile.scale);
            RefreshScale(profile.scale);
            _surface.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Close()
        {
            if (_surface != null)
                _surface.SetActive(false);
        }

        private void Build()
        {
            Font font =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");
            Image shade = Image(
                "Shade",
                transform,
                new Color(
                    0.01f,
                    0.015f,
                    0.018f,
                    0.78f));
            Stretch(shade.rectTransform);
            _surface = shade.gameObject;
            Image panel = Image(
                "Panel",
                shade.transform,
                new Color(
                    0.045f,
                    0.055f,
                    0.052f,
                    0.99f));
            RectTransform panelRect =
                panel.rectTransform;
            panelRect.anchorMin =
                new Vector2(0.5f, 0.5f);
            panelRect.anchorMax =
                new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta =
                new Vector2(460f, 480f);
            panelRect.anchoredPosition =
                Vector2.zero;

            Text title = Label(
                "Title",
                panel.transform,
                font,
                "CUSTOM CAMOUFLAGE",
                22);
            Place(title.rectTransform, 24f, 436f, -52f, -16f);
            _scheme = Dropdown(
                "Scheme",
                panel.transform,
                font,
                Schemes);
            Place(
                _scheme.GetComponent<
                    RectTransform>(),
                24f,
                436f,
                -108f,
                -70f);
            _base = ColorField(
                panel.transform,
                font,
                "BASE",
                -132f);
            _weather = ColorField(
                panel.transform,
                font,
                "WEATHER",
                -190f);
            _patchA = ColorField(
                panel.transform,
                font,
                "PATCH A",
                -248f);
            _patchB = ColorField(
                panel.transform,
                font,
                "PATCH B",
                -306f);

            _scale = DefaultControls.CreateSlider(
                new DefaultControls.Resources())
                .GetComponent<Slider>();
            _scale.name = "Scale";
            _scale.transform.SetParent(
                panel.transform,
                false);
            _scale.minValue = 0.1f;
            _scale.maxValue = 1.5f;
            Place(
                _scale.GetComponent<
                    RectTransform>(),
                24f,
                352f,
                -372f,
                -342f);
            _scaleValue = Label(
                "ScaleValue",
                panel.transform,
                font,
                string.Empty,
                13);
            _scaleValue.alignment =
                TextAnchor.MiddleRight;
            Place(
                _scaleValue.rectTransform,
                360f,
                436f,
                -372f,
                -342f);
            _scale.onValueChanged.AddListener(
                RefreshScale);

            Button apply = Button(
                "Apply",
                panel.transform,
                font,
                "APPLY");
            Place(
                apply.GetComponent<RectTransform>(),
                238f,
                436f,
                -444f,
                -400f);
            apply.onClick.AddListener(Apply);
            Button cancel = Button(
                "Cancel",
                panel.transform,
                font,
                "CANCEL");
            Place(
                cancel.GetComponent<RectTransform>(),
                24f,
                222f,
                -444f,
                -400f);
            cancel.onClick.AddListener(Close);
        }

        private void Apply()
        {
            CustomCamouflageStore.Save(
                new CustomCamouflageProfile
                {
                    scheme =
                        Schemes[_scheme.value],
                    baseColor = _base.text,
                    weatherColor =
                        _weather.text,
                    patchColorA =
                        _patchA.text,
                    patchColorB =
                        _patchB.text,
                    scale = _scale.value
                });
            _saved?.Invoke();
            Close();
        }

        private void RefreshScale(float value)
        {
            _scaleValue.text =
                value.ToString("0.00");
        }

        private static InputField ColorField(
            Transform parent,
            Font font,
            string label,
            float top)
        {
            Text caption = Label(
                label + "Label",
                parent,
                font,
                label,
                11);
            Place(
                caption.rectTransform,
                24f,
                126f,
                top - 36f,
                top);
            GameObject fieldObject =
                DefaultControls.CreateInputField(
                    new DefaultControls.Resources());
            fieldObject.name = label;
            fieldObject.transform.SetParent(
                parent,
                false);
            InputField field =
                fieldObject.GetComponent<
                    InputField>();
            field.textComponent.font = font;
            field.textComponent.color =
                Color.white;
            Place(
                fieldObject.GetComponent<
                    RectTransform>(),
                138f,
                436f,
                top - 36f,
                top);
            return field;
        }

        private static Image Image(
            string name,
            Transform parent,
            Color color)
        {
            GameObject root =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Image));
            root.transform.SetParent(
                parent,
                false);
            Image image =
                root.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Label(
            string name,
            Transform parent,
            Font font,
            string value,
            int size)
        {
            GameObject root =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Text));
            root.transform.SetParent(
                parent,
                false);
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.text = value;
            text.alignment =
                TextAnchor.MiddleLeft;
            return text;
        }

        private static Dropdown Dropdown(
            string name,
            Transform parent,
            Font font,
            IEnumerable<string> options)
        {
            GameObject root =
                DefaultControls.CreateDropdown(
                    new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Dropdown dropdown =
                root.GetComponent<Dropdown>();
            dropdown.captionText.font = font;
            dropdown.itemText.font = font;
            dropdown.AddOptions(
                new List<string>(options));
            return dropdown;
        }

        private static Button Button(
            string name,
            Transform parent,
            Font font,
            string label)
        {
            GameObject root =
                DefaultControls.CreateButton(
                    new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            root.GetComponentInChildren<Text>()
                .font = font;
            root.GetComponentInChildren<Text>()
                .text = label;
            return root.GetComponent<Button>();
        }

        private static void Stretch(
            RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float top)
        {
            rect.anchorMin =
                new Vector2(0f, 1f);
            rect.anchorMax =
                new Vector2(0f, 1f);
            rect.offsetMin =
                new Vector2(left, bottom);
            rect.offsetMax =
                new Vector2(right, top);
        }
    }
}
