using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal static class RankedPanelUi
    {
        public static Text Caption(
            string name,
            Transform parent,
            Font font,
            string value)
        {
            Text text = Label(
                name,
                parent,
                font,
                12,
                TextAnchor.MiddleLeft);
            text.text = value;
            text.color = new Color(0.62f, 0.69f, 0.66f);
            return text;
        }

        public static InputField Input(
            string name,
            Transform parent,
            Font font,
            string value)
        {
            GameObject root = DefaultControls.CreateInputField(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            InputField input = root.GetComponent<InputField>();
            input.textComponent.font = font;
            input.text = value;
            return input;
        }

        public static Dropdown Dropdown(
            string name,
            Transform parent,
            Font font)
        {
            GameObject root = DefaultControls.CreateDropdown(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Dropdown dropdown = root.GetComponent<Dropdown>();
            dropdown.ClearOptions();
            foreach (Text text in
                root.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
            }
            return dropdown;
        }

        public static Button Button(
            string name,
            Transform parent,
            Font font,
            string value)
        {
            GameObject root = DefaultControls.CreateButton(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Text text = root.GetComponentInChildren<Text>();
            text.font = font;
            text.text = value;
            return root.GetComponent<Button>();
        }

        public static Text Label(
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
            text.color = Color.white;
            text.alignment = alignment;
            return text;
        }

        public static Image Image(
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

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(
            RectTransform rect,
            float left,
            float right,
            float top,
            float bottom)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(left, top);
            rect.offsetMax = new Vector2(right, bottom);
        }
    }
}
