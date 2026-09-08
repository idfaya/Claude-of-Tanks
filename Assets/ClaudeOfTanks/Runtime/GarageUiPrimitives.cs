using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal static class GarageUiPrimitives
    {
        public static Button ActionButton(
            string name,
            string label,
            Font font,
            RectTransform topRail,
            RectTransform selectionRail,
            RectTransform commandBand,
            Color surface,
            Color text,
            Color accent)
        {
            RectTransform parent;
            Vector2 anchor;
            Vector2 position;
            Vector2 size;
            bool primary = name == "Deploy";

            switch (name)
            {
                case "Deploy":
                    parent = commandBand;
                    anchor = new Vector2(0.5f, 0.5f);
                    position = new Vector2(0f, 1f);
                    size = new Vector2(286f, 58f);
                    break;
                case "Settings":
                    parent = topRail;
                    anchor = new Vector2(1f, 0.5f);
                    position = new Vector2(-174f, 0f);
                    size = new Vector2(112f, 36f);
                    break;
                case "Replays":
                    parent = topRail;
                    anchor = new Vector2(1f, 0.5f);
                    position = new Vector2(-62f, 0f);
                    size = new Vector2(112f, 36f);
                    break;
                case "PrivateRoom":
                    parent = selectionRail;
                    anchor = new Vector2(0.5f, 0f);
                    position = new Vector2(0f, 150f);
                    size = new Vector2(292f, 42f);
                    break;
                case "Ranked":
                    parent = selectionRail;
                    anchor = new Vector2(0.5f, 0f);
                    position = new Vector2(0f, 100f);
                    size = new Vector2(292f, 42f);
                    break;
                default:
                    parent = selectionRail;
                    anchor = new Vector2(0.5f, 0f);
                    position = new Vector2(0f, 50f);
                    size = new Vector2(292f, 42f);
                    break;
            }

            GameObject root = DefaultControls.CreateButton(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            RectTransform rect =
                root.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image image = root.GetComponent<Image>();
            image.color = Color.white;
            Button button = root.GetComponent<Button>();
            button.colors = primary
                ? Colors(
                    accent,
                    new Color(1f, 0.75f, 0.27f),
                    new Color(0.70f, 0.43f, 0.10f))
                : Colors(
                    surface,
                    new Color(0.10f, 0.13f, 0.13f),
                    new Color(0.03f, 0.04f, 0.04f));

            Text buttonText =
                root.GetComponentInChildren<Text>();
            buttonText.font = font;
            buttonText.fontSize = primary ? 17 : 11;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.color = primary
                ? new Color(0.08f, 0.07f, 0.035f)
                : text;
            buttonText.text = label;
            return button;
        }

        public static Dropdown Selector(
            string name,
            string caption,
            float top,
            RectTransform parent,
            Font font,
            Color surface,
            Color muted,
            Color text)
        {
            Text label = Label(
                name + "Caption",
                parent,
                font,
                9,
                TextAnchor.MiddleLeft,
                muted);
            label.text = caption;
            Place(
                label.rectTransform,
                new Vector2(24f, top - 23f),
                new Vector2(316f, top),
                new Vector2(0f, 1f));

            GameObject root = DefaultControls.CreateDropdown(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Place(
                root.GetComponent<RectTransform>(),
                new Vector2(24f, top - 70f),
                new Vector2(316f, top - 28f),
                new Vector2(0f, 1f));
            root.GetComponent<Image>().color = surface;
            Dropdown dropdown = root.GetComponent<Dropdown>();
            dropdown.colors = Colors(
                surface,
                new Color(0.09f, 0.12f, 0.12f),
                new Color(0.03f, 0.04f, 0.04f));
            root.GetComponent<Image>().color = Color.white;
            Transform arrow = root.transform.Find("Arrow");
            if (arrow != null) arrow.gameObject.SetActive(false);
            Text chevron = Label(
                "SelectorChevron",
                root.transform,
                font,
                10,
                TextAnchor.MiddleCenter,
                muted);
            chevron.text = "V";
            Place(
                chevron.rectTransform,
                new Vector2(-30f, -30f),
                new Vector2(-8f, -8f),
                new Vector2(1f, 1f));
            Text[] labels =
                root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].font = font;
                labels[i].fontSize = 12;
                labels[i].color = text;
            }
            return dropdown;
        }

        public static RectTransform Panel(
            string name,
            Transform parent,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            Image image = Image(name, parent, color);
            RectTransform rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static Text Label(
            string name,
            Transform parent,
            Font font,
            int size,
            TextAnchor alignment,
            Color color)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            root.transform.SetParent(parent, false);
            Text value = root.GetComponent<Text>();
            value.font = font;
            value.fontSize = size;
            value.fontStyle = FontStyle.Bold;
            value.alignment = alignment;
            value.color = color;
            value.horizontalOverflow =
                HorizontalWrapMode.Wrap;
            value.verticalOverflow =
                VerticalWrapMode.Truncate;
            return value;
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

        public static ColorBlock Colors(
            Color normal,
            Color highlighted,
            Color pressed)
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = normal;
            colors.highlightedColor = highlighted;
            colors.pressedColor = pressed;
            colors.selectedColor = highlighted;
            colors.disabledColor =
                new Color(normal.r, normal.g, normal.b, 0.42f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        public static void Place(
            RectTransform rect,
            Vector2 min,
            Vector2 max,
            Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }
    }
}
