using System;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleHud : MonoBehaviour
    {
        private Text _vehicle;
        private Text _stats;
        private Text _objective;
        private Text _status;
        private Image _healthFill;
        private GameObject _touchRoot;
        private Vector2 _touchDrive;
        private bool _fireHeld;
        private readonly bool[] _consumables = new bool[3];
        private Action _restart;
        private Action _garage;

        public Vector2 TouchDrive => _touchDrive;
        public bool FireHeld => _fireHeld;

        public static BattleHud Create(Action restart, Action garage = null)
        {
            GameObject root = new GameObject("BattleHUD");
            BattleHud hud = root.AddComponent<BattleHud>();
            hud._restart = restart;
            hud._garage = garage;
            hud.Build();
            return hud;
        }

        public bool ConsumeConsumable(int slot)
        {
            bool value = _consumables[slot];
            _consumables[slot] = false;
            return value;
        }

        public void SetState(
            TankState player, MatchModeState mode, string status, bool battleOver)
        {
            if (player == null) return;
            _vehicle.text = player.Spec.DisplayName;
            _stats.text = string.Format(
                "HP {0:0}/{1:0}    {2}    RELOAD {3:0.0}s",
                player.Health, player.Spec.MaxHealth, player.Spec.Shell.Type,
                player.ReloadRemainingS);
            _healthFill.fillAmount = Mathf.Clamp01(player.Health / player.Spec.MaxHealth);
            _healthFill.color = _healthFill.fillAmount > 0.35f
                ? new Color(0.28f, 0.76f, 0.3f) : new Color(0.9f, 0.18f, 0.1f);
            _objective.text = mode.Id == GameModeId.Standard ? "STANDARD"
                : mode.Id == GameModeId.EndlessHorde ? "WAVE " + mode.HordeWave
                : string.Format("{0:0}  {1}  {2:0}", mode.AlphaScore, ModeLabel(mode.Id), mode.BravoScore);
            _status.text = status;
            Transform restart = transform.Find("Restart");
            if (restart != null) restart.gameObject.SetActive(battleOver);
        }

        public void SetTouchVisible(bool visible)
        {
            _touchRoot.SetActive(visible);
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject events = new GameObject("EventSystem");
                events.AddComponent<EventSystem>();
                events.AddComponent<StandaloneInputModule>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Image panel = Image("VehiclePanel", transform, new Color(0.035f, 0.045f, 0.04f, 0.88f));
            Rect(panel.rectTransform, new Vector2(20f, 20f), new Vector2(390f, 92f), new Vector2(0f, 0f));
            _vehicle = Label("Vehicle", panel.transform, font, 18, TextAnchor.UpperLeft);
            Rect(_vehicle.rectTransform, new Vector2(16f, -36f), new Vector2(-16f, -8f),
                new Vector2(0f, 1f), new Vector2(1f, 1f));
            _stats = Label("Stats", panel.transform, font, 14, TextAnchor.MiddleLeft);
            Rect(_stats.rectTransform, new Vector2(16f, 24f), new Vector2(-16f, 48f),
                Vector2.zero, new Vector2(1f, 0f));
            Image healthBack = Image("Health", panel.transform, new Color(0.08f, 0.09f, 0.08f, 1f));
            Rect(healthBack.rectTransform, new Vector2(16f, 10f), new Vector2(-16f, 17f),
                Vector2.zero, new Vector2(1f, 0f));
            _healthFill = Image("Fill", healthBack.transform, Color.green);
            _healthFill.type = UnityEngine.UI.Image.Type.Filled;
            _healthFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            Rect(_healthFill.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

            _objective = Label("Objective", transform, font, 18, TextAnchor.MiddleCenter);
            Rect(_objective.rectTransform, new Vector2(-260f, -42f), new Vector2(260f, -8f), new Vector2(0.5f, 1f));
            _status = Label("Status", transform, font, 28, TextAnchor.MiddleCenter);
            Rect(_status.rectTransform, new Vector2(-260f, -88f), new Vector2(260f, -44f), new Vector2(0.5f, 1f));
            Text reticle = Label("Reticle", transform, font, 28, TextAnchor.MiddleCenter);
            reticle.text = "+";
            Rect(reticle.rectTransform, new Vector2(-20f, -20f), new Vector2(20f, 20f), new Vector2(0.5f, 0.5f));

            CreateButton("Repair", "4", new Vector2(430f, 20f), () => _consumables[0] = true);
            CreateButton("FirstAid", "5", new Vector2(486f, 20f), () => _consumables[1] = true);
            CreateButton("Extinguish", "6", new Vector2(542f, 20f), () => _consumables[2] = true);
            CreateButton("Restart", "RESTART", new Vector2(-70f, -50f), _restart, new Vector2(140f, 42f), new Vector2(0.5f, 0.5f));
            transform.Find("Restart").gameObject.SetActive(false);
            CreateButton("Garage", "GARAGE", new Vector2(20f, -48f), _garage,
                new Vector2(90f, 34f), new Vector2(0f, 1f));
            BuildTouchControls(font);
        }

        private void BuildTouchControls(Font font)
        {
            _touchRoot = new GameObject("TouchControls");
            _touchRoot.transform.SetParent(transform, false);
            HoldButton(_touchRoot.transform, "Forward", "^", new Vector2(112f, 132f), value => SetDrive(1, value));
            HoldButton(_touchRoot.transform, "Reverse", "v", new Vector2(112f, 36f), value => SetDrive(2, value));
            HoldButton(_touchRoot.transform, "Left", "<", new Vector2(32f, 52f), value => SetDrive(3, value));
            HoldButton(_touchRoot.transform, "Right", ">", new Vector2(192f, 52f), value => SetDrive(4, value));
            HoldButton(_touchRoot.transform, "Fire", "FIRE", new Vector2(-120f, 65f),
                value => _fireHeld = value, new Vector2(104f, 104f), new Vector2(1f, 0f));
            _touchRoot.SetActive(Application.isMobilePlatform || Input.touchSupported);
        }

        private void SetDrive(int direction, bool held)
        {
            float value = held ? 1f : 0f;
            if (direction == 1) _touchDrive.y = value;
            else if (direction == 2) _touchDrive.y = -value;
            else if (direction == 3) _touchDrive.x = -value;
            else _touchDrive.x = value;
        }

        private void HoldButton(
            Transform parent, string name, string text, Vector2 position, Action<bool> action,
            Vector2? size = null, Vector2? anchor = null)
        {
            Button button = CreateButton(name, text, position, null, size ?? new Vector2(72f, 72f), anchor ?? Vector2.zero, parent);
            HoldControl hold = button.gameObject.AddComponent<HoldControl>();
            hold.SetAction(action);
        }

        private Button CreateButton(
            string name, string text, Vector2 position, Action action,
            Vector2? size = null, Vector2? anchor = null, Transform parent = null)
        {
            Image image = Image(name, parent ?? transform, new Color(0.08f, 0.1f, 0.09f, 0.78f));
            Rect(image.rectTransform, position, position + (size ?? new Vector2(48f, 48f)), anchor ?? Vector2.zero);
            Button button = image.gameObject.AddComponent<Button>();
            if (action != null) button.onClick.AddListener(() => action());
            Text label = Label("Label", image.transform,
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), 14, TextAnchor.MiddleCenter);
            label.text = text;
            Rect(label.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);
            return button;
        }

        private static Image Image(string name, Transform parent, Color color)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            Image image = child.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Label(string name, Transform parent, Font font, int size, TextAnchor alignment)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            child.transform.SetParent(parent, false);
            Text text = child.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static void Rect(
            RectTransform rect, Vector2 min, Vector2 max, Vector2 anchorMin, Vector2? anchorMax = null)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax ?? anchorMin;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private static string ModeLabel(GameModeId id)
        {
            if (id == GameModeId.CaptureTheFlag) return "CTF";
            if (id == GameModeId.ZoneControl) return "ZONES";
            if (id == GameModeId.TurboBall) return "TURBO";
            return id.ToString().ToUpperInvariant();
        }
    }

    public sealed class HoldControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Action<bool> _action;
        public void SetAction(Action<bool> action) { _action = action; }
        public void OnPointerDown(PointerEventData eventData) { _action?.Invoke(true); }
        public void OnPointerUp(PointerEventData eventData) { _action?.Invoke(false); }
        private void OnDisable() { _action?.Invoke(false); }
    }
}
