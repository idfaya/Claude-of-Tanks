using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal static class SceneStudioUiFactory
    {
        public static Slider Slider(
            Transform parent,
            Font font,
            string label,
            float top,
            float minimum,
            float maximum)
        {
            Text caption = Label(
                label + "Label",
                parent,
                font,
                label,
                10);
            Place(
                caption.rectTransform,
                20f,
                72f,
                top - 32f,
                top);
            GameObject root =
                DefaultControls.CreateSlider(
                    new DefaultControls.Resources());
            root.name = label;
            root.transform.SetParent(parent, false);
            Slider slider =
                root.GetComponent<Slider>();
            slider.minValue = minimum;
            slider.maxValue = maximum;
            Place(
                root.GetComponent<
                    RectTransform>(),
                78f,
                370f,
                top - 32f,
                top);
            return slider;
        }

        public static void PlaceControl(
            Transform control,
            float top)
        {
            Place(
                control.GetComponent<
                    RectTransform>(),
                20f,
                370f,
                top - 36f,
                top);
        }

        public static Image Image(
            string name,
            Transform parent,
            Color color)
        {
            GameObject root =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Image));
            root.transform.SetParent(parent, false);
            Image image =
                root.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(
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
            root.transform.SetParent(parent, false);
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        public static Dropdown Dropdown(
            string name,
            Transform parent,
            Font font)
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
            return dropdown;
        }

        public static Button Button(
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
            Text text =
                root.GetComponentInChildren<Text>();
            text.font = font;
            text.text = label;
            return root.GetComponent<Button>();
        }

        public static Toggle Toggle(
            string name,
            Transform parent,
            Font font,
            string label)
        {
            GameObject root =
                DefaultControls.CreateToggle(
                    new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Text text =
                root.GetComponentInChildren<Text>();
            text.font = font;
            text.text = label;
            return root.GetComponent<Toggle>();
        }

        public static void Place(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float top,
            Vector2? anchor = null)
        {
            Vector2 point =
                anchor ??
                new Vector2(0f, 1f);
            rect.anchorMin = point;
            rect.anchorMax = point;
            rect.offsetMin =
                new Vector2(left, bottom);
            rect.offsetMax =
                new Vector2(right, top);
        }
    }
}
