using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed class ReplayBrowserPanel : MonoBehaviour
    {
        private ReplayArchive _archive;
        private Action<string> _play;
        private GameObject _panel;
        private RectTransform _surface;
        private Dropdown _entries;
        private Text _details;
        private Button _playButton;
        private Button _deleteButton;
        private ReplayArchiveEntry[] _items = Array.Empty<ReplayArchiveEntry>();
        private int _layoutWidth;
        private int _layoutHeight;

        public bool IsVisible => _panel != null && _panel.activeSelf;
        public int EntryCount => _items.Length;
        public string SelectedReplayId =>
            _items.Length == 0 ? null : _items[Mathf.Clamp(_entries.value, 0, _items.Length - 1)].Id;

        public static ReplayBrowserPanel Create(
            Transform parent,
            ReplayArchive archive,
            Action<string> play)
        {
            GameObject root = new GameObject("ReplayBrowser", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            Stretch(rect);
            ReplayBrowserPanel browser = root.AddComponent<ReplayBrowserPanel>();
            browser._archive = archive ?? throw new ArgumentNullException(nameof(archive));
            browser._play = play ?? throw new ArgumentNullException(nameof(play));
            browser.Build();
            browser.Close();
            return browser;
        }

        public void Open()
        {
            Refresh();
            _panel.SetActive(true);
            transform.SetAsLastSibling();
            ApplyViewportLayout();
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Refresh()
        {
            _items = _archive.List();
            _entries.ClearOptions();
            List<string> options = new List<string>();
            for (int i = 0; i < _items.Length; i++)
            {
                ReplayArchiveEntry entry = _items[i];
                DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds(entry.CreatedUnixMs)
                    .ToLocalTime();
                options.Add(string.Format(
                    "{0:yyyy-MM-dd HH:mm}   {1}   {2}",
                    date,
                    entry.MapId.ToUpperInvariant(),
                    ModeLabel(entry.GameMode)));
            }
            if (options.Count == 0) options.Add("NO SAVED REPLAYS");
            _entries.AddOptions(options);
            _entries.SetValueWithoutNotify(0);
            _playButton.interactable = _items.Length > 0;
            _deleteButton.interactable = _items.Length > 0;
            UpdateDetails(0);
        }

        public void Select(int index)
        {
            if (_items.Length == 0) return;
            _entries.SetValueWithoutNotify(Mathf.Clamp(index, 0, _items.Length - 1));
            UpdateDetails(_entries.value);
        }

        public void PlaySelected()
        {
            string id = SelectedReplayId;
            if (id != null) _play(id);
        }

        public void DeleteSelected()
        {
            string id = SelectedReplayId;
            if (id == null) return;
            _archive.Delete(id);
            Refresh();
        }

        private void Update()
        {
            if (IsVisible && (_layoutWidth != Screen.width || _layoutHeight != Screen.height))
                ApplyViewportLayout();
        }

        private void Build()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Image shade = Image("Shade", transform, new Color(0.01f, 0.015f, 0.018f, 0.84f));
            Stretch(shade.rectTransform);
            _panel = shade.gameObject;

            Image surface = Image("Surface", shade.transform, new Color(0.055f, 0.065f, 0.062f, 0.98f));
            _surface = surface.rectTransform;
            _surface.anchorMin = new Vector2(0.5f, 0.5f);
            _surface.anchorMax = new Vector2(0.5f, 0.5f);
            _surface.sizeDelta = new Vector2(720f, 430f);
            _surface.anchoredPosition = Vector2.zero;

            Text title = Label("Title", surface.transform, font, 26, TextAnchor.MiddleLeft);
            title.text = "REPLAY ARCHIVE";
            PlaceHorizontal(title.rectTransform, 28f, -28f, -62f, -18f);

            _entries = Dropdown("Entries", surface.transform, font);
            PlaceHorizontal(_entries.GetComponent<RectTransform>(), 28f, -28f, -126f, -82f);
            _entries.onValueChanged.AddListener(UpdateDetails);

            _details = Label("Details", surface.transform, font, 16, TextAnchor.UpperLeft);
            _details.color = new Color(0.78f, 0.84f, 0.81f);
            PlaceHorizontal(_details.rectTransform, 28f, -28f, -290f, -154f);

            _playButton = Button("Play", surface.transform, font, "PLAY");
            Place(_playButton.GetComponent<RectTransform>(),
                new Vector2(28f, 24f), new Vector2(188f, 72f), Vector2.zero);
            _playButton.onClick.AddListener(PlaySelected);
            _deleteButton = Button("Delete", surface.transform, font, "DELETE");
            Place(_deleteButton.GetComponent<RectTransform>(),
                new Vector2(204f, 24f), new Vector2(364f, 72f), Vector2.zero);
            _deleteButton.onClick.AddListener(DeleteSelected);
            Button close = Button("Close", surface.transform, font, "CLOSE");
            Place(close.GetComponent<RectTransform>(),
                new Vector2(-188f, 24f), new Vector2(-28f, 72f), new Vector2(1f, 0f));
            close.onClick.AddListener(Close);
        }

        private void UpdateDetails(int index)
        {
            if (_items.Length == 0)
            {
                _details.text = "Complete a battle to create a deterministic replay.";
                return;
            }
            ReplayArchiveEntry entry = _items[Mathf.Clamp(index, 0, _items.Length - 1)];
            int seconds = Mathf.CeilToInt(entry.DurationS);
            _details.text = string.Format(
                "MAP  {0}\nMODE  {1}\nVEHICLE  {2}\nDURATION  {3:00}:{4:00}\nFRAMES  {5:N0}     FILE  {6:0.0} KB",
                entry.MapId.ToUpperInvariant(),
                ModeLabel(entry.GameMode),
                entry.PlayerVehicleId.ToUpperInvariant(),
                seconds / 60,
                seconds % 60,
                entry.FrameCount,
                entry.FileBytes / 1024f);
        }

        private void ApplyViewportLayout()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            float canvasScale = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
            float widthScale = Mathf.Min(720f, Screen.width - 32f) / (720f * canvasScale);
            float heightScale = Mathf.Min(430f, Screen.height - 32f) / (430f * canvasScale);
            _surface.localScale = Vector3.one * Mathf.Clamp(
                Mathf.Min(widthScale, heightScale),
                0.65f,
                1.8f);
            _layoutWidth = Screen.width;
            _layoutHeight = Screen.height;
        }

        private static string ModeLabel(ClaudeOfTanks.Simulation.GameModeId mode)
        {
            switch (mode)
            {
                case ClaudeOfTanks.Simulation.GameModeId.CaptureTheFlag: return "CAPTURE THE FLAG";
                case ClaudeOfTanks.Simulation.GameModeId.ZoneControl: return "ZONE CONTROL";
                case ClaudeOfTanks.Simulation.GameModeId.TurboBall: return "TURBO BALL";
                case ClaudeOfTanks.Simulation.GameModeId.EndlessHorde: return "ENDLESS HORDE";
                default: return "STANDARD";
            }
        }

        private static Dropdown Dropdown(string name, Transform parent, Font font)
        {
            GameObject root = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.14f);
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.color = Color.white;
            }
            return root.GetComponent<Dropdown>();
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
            text.color = Color.white;
            text.text = label;
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
            Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
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
