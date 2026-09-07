using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static ClaudeOfTanks.Runtime.RankedPanelUi;

namespace ClaudeOfTanks.Runtime
{
    public sealed class GarageLoadoutPanel : MonoBehaviour
    {
        private readonly List<ToggleBinding> _equipment =
            new List<ToggleBinding>();
        private ContentCatalog _catalog;
        private GarageLoadoutController _controller;
        private GameObject _surface;
        private Dropdown _camouflage;
        private Text _selection;
        private bool _refreshing;
        private bool _subscribed;

        public bool IsVisible =>
            _surface != null && _surface.activeSelf;
        public int EquipmentToggleCount => _equipment.Count;
        public string SelectionText =>
            _selection != null ? _selection.text : string.Empty;

        public static GarageLoadoutPanel Create(
            Transform parent,
            ContentCatalog catalog,
            GarageLoadoutController controller)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            GameObject root = new GameObject(
                "Loadout",
                typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>());
            GarageLoadoutPanel panel =
                root.AddComponent<GarageLoadoutPanel>();
            panel._catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));
            panel._controller = controller ??
                throw new ArgumentNullException(nameof(controller));
            panel.Build();
            panel.Subscribe();
            panel.Close();
            return panel;
        }

        public void Open()
        {
            _surface.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public void Close()
        {
            if (_surface != null) _surface.SetActive(false);
        }

        public bool SetEquipment(string equipmentId, bool equipped)
        {
            return _controller.SetEquipment(equipmentId, equipped);
        }

        public bool SetCamouflage(string camouflageId)
        {
            return _controller.SetCamouflage(camouflageId);
        }

        private void Build()
        {
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            Image shade = Image(
                "Shade",
                transform,
                new Color(0.01f, 0.015f, 0.018f, 0.68f));
            Stretch(shade.rectTransform);
            Button shadeButton =
                shade.gameObject.AddComponent<Button>();
            shadeButton.transition = Selectable.Transition.None;
            shadeButton.onClick.AddListener(Close);
            _surface = shade.gameObject;

            Image rail = Image(
                "Rail",
                shade.transform,
                new Color(0.045f, 0.055f, 0.052f, 0.98f));
            rail.gameObject.AddComponent<Button>().transition =
                Selectable.Transition.None;
            RectTransform railRect = rail.rectTransform;
            railRect.anchorMin = new Vector2(1f, 0f);
            railRect.anchorMax = new Vector2(1f, 1f);
            railRect.pivot = new Vector2(1f, 0.5f);
            railRect.sizeDelta = new Vector2(500f, 0f);
            railRect.anchoredPosition = Vector2.zero;

            Text title = Label(
                "Title",
                rail.transform,
                font,
                24,
                TextAnchor.MiddleLeft);
            title.text = "LOADOUT";
            Place(title.rectTransform, 28f, 340f, -58f, -18f);
            Button close = Button(
                "Close",
                rail.transform,
                font,
                "CLOSE");
            Place(
                close.GetComponent<RectTransform>(),
                374f,
                472f,
                -58f,
                -18f);
            close.onClick.AddListener(Close);

            Text equipmentLabel = Caption(
                "EquipmentLabel",
                rail.transform,
                font,
                "EQUIPMENT  /  SELECT UP TO 3");
            Place(
                equipmentLabel.rectTransform,
                28f,
                472f,
                -100f,
                -76f);

            EquipmentDefinition[] definitions =
                _catalog.Equipment;
            for (int i = 0; i < definitions.Length; i++)
            {
                EquipmentDefinition definition = definitions[i];
                Toggle toggle = CreateToggle(
                    definition.id,
                    rail.transform,
                    font,
                    definition.shortName);
                int column = i % 2;
                int row = i / 2;
                float left = 28f + column * 224f;
                float top = -112f - row * 42f;
                Place(
                    toggle.GetComponent<RectTransform>(),
                    left,
                    left + 208f,
                    top - 36f,
                    top);
                string equipmentId = definition.id;
                toggle.onValueChanged.AddListener(
                    value => OnEquipmentChanged(
                        equipmentId,
                        value));
                _equipment.Add(new ToggleBinding
                {
                    Id = equipmentId,
                    Toggle = toggle
                });
            }

            Text camouflageLabel = Caption(
                "CamouflageLabel",
                rail.transform,
                font,
                "CAMOUFLAGE");
            Place(
                camouflageLabel.rectTransform,
                28f,
                472f,
                -432f,
                -408f);
            _camouflage = Dropdown(
                "Camouflage",
                rail.transform,
                font);
            List<string> camouflageNames = new List<string>();
            for (int i = 0; i < _catalog.Camouflage.Length; i++)
                camouflageNames.Add(_catalog.Camouflage[i].name);
            _camouflage.AddOptions(camouflageNames);
            Place(
                _camouflage.GetComponent<RectTransform>(),
                28f,
                472f,
                -474f,
                -434f);
            _camouflage.onValueChanged.AddListener(
                OnCamouflageChanged);

            _selection = Label(
                "Selection",
                rail.transform,
                font,
                13,
                TextAnchor.UpperLeft);
            _selection.fontStyle = FontStyle.Normal;
            _selection.horizontalOverflow =
                HorizontalWrapMode.Wrap;
            _selection.verticalOverflow =
                VerticalWrapMode.Truncate;
            Place(
                _selection.rectTransform,
                28f,
                472f,
                -610f,
                -492f);

            Button reset = Button(
                "Reset",
                rail.transform,
                font,
                "RESET");
            Place(
                reset.GetComponent<RectTransform>(),
                28f,
                472f,
                -684f,
                -640f);
            reset.onClick.AddListener(_controller.Reset);
            Refresh();
        }

        private void OnEquipmentChanged(
            string equipmentId,
            bool value)
        {
            if (_refreshing) return;
            if (!_controller.SetEquipment(equipmentId, value))
                Refresh();
        }

        private void OnCamouflageChanged(int index)
        {
            if (_refreshing ||
                index < 0 ||
                index >= _catalog.Camouflage.Length)
            {
                return;
            }
            if (!_controller.SetCamouflage(
                    _catalog.Camouflage[index].id))
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_controller == null || _selection == null) return;
            _refreshing = true;
            for (int i = 0; i < _equipment.Count; i++)
            {
                ToggleBinding binding = _equipment[i];
                binding.Toggle.interactable =
                    _controller.IsEligible(binding.Id);
                binding.Toggle.SetIsOnWithoutNotify(
                    _controller.IsEquipped(binding.Id));
            }
            int camouflageIndex = Array.FindIndex(
                _catalog.Camouflage,
                item => item.id == _controller.CamouflageId);
            _camouflage.SetValueWithoutNotify(
                Mathf.Max(0, camouflageIndex));
            _selection.text = SelectionTextFor(
                _controller.Equipment,
                _controller.CamouflageId);
            _refreshing = false;
        }

        private void Subscribe()
        {
            if (_subscribed || _controller == null) return;
            _controller.Changed += Refresh;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || _controller == null) return;
            _controller.Changed -= Refresh;
            _subscribed = false;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private string SelectionTextFor(
            string[] equipment,
            string camouflageId)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ACTIVE  ");
            if (equipment.Length == 0)
            {
                text.Append("NO EQUIPMENT");
            }
            else
            {
                for (int i = 0; i < equipment.Length; i++)
                {
                    if (i > 0) text.Append("  /  ");
                    text.Append(
                        _catalog.GetEquipment(
                            equipment[i]).shortName.ToUpperInvariant());
                }
            }
            text.AppendLine();
            int index = Array.FindIndex(
                _catalog.Camouflage,
                item => item.id == camouflageId);
            text.Append("PAINT  ");
            text.Append(index >= 0
                ? _catalog.Camouflage[index].name.ToUpperInvariant()
                : "FACTORY");
            return text.ToString();
        }

        private static Toggle CreateToggle(
            string name,
            Transform parent,
            Font font,
            string label)
        {
            GameObject root = DefaultControls.CreateToggle(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Text text = root.GetComponentInChildren<Text>();
            text.font = font;
            text.fontSize = 13;
            text.color = new Color(0.82f, 0.86f, 0.84f);
            text.text = label;
            return root.GetComponent<Toggle>();
        }

        private sealed class ToggleBinding
        {
            public string Id;
            public Toggle Toggle;
        }
    }
}
