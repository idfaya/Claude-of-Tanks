using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class BattleHud
    {
        private void BuildResultScreen(Font font)
        {
            Image shade = Image(
                "BattleResult",
                transform,
                new Color(0.015f, 0.02f, 0.025f, 0.94f));
            _resultRoot = shade.gameObject;
            Rect(
                shade.rectTransform,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one);

            _resultTitle = Label(
                "Verdict",
                shade.transform,
                font,
                48,
                TextAnchor.MiddleCenter);
            Rect(
                _resultTitle.rectTransform,
                new Vector2(-360f, 76f),
                new Vector2(360f, 144f),
                new Vector2(0.5f, 0.5f));
            _resultStats = Label(
                "Summary",
                shade.transform,
                font,
                18,
                TextAnchor.MiddleCenter);
            Rect(
                _resultStats.rectTransform,
                new Vector2(-430f, -30f),
                new Vector2(430f, 65f),
                new Vector2(0.5f, 0.5f));
            _killcamButton = CreateButton(
                "Killcam",
                "KILLCAM",
                new Vector2(-390f, -112f),
                () => _killcam?.Invoke(),
                new Vector2(180f, 48f),
                new Vector2(0.5f, 0.5f),
                shade.transform);
            _fullReplayButton = CreateButton(
                "FullReplay",
                "FULL REPLAY",
                new Vector2(-190f, -112f),
                () => _fullReplay?.Invoke(),
                new Vector2(180f, 48f),
                new Vector2(0.5f, 0.5f),
                shade.transform);
            CreateButton(
                "BattleAgain",
                "BATTLE AGAIN",
                new Vector2(10f, -112f),
                _restart,
                new Vector2(180f, 48f),
                new Vector2(0.5f, 0.5f),
                shade.transform);
            CreateButton(
                "ReturnToGarage",
                "GARAGE",
                new Vector2(210f, -112f),
                _garage,
                new Vector2(180f, 48f),
                new Vector2(0.5f, 0.5f),
                shade.transform);
            _resultRoot.SetActive(false);
        }

        private void BuildReplayOverlay(Font font)
        {
            _replayRoot = new GameObject(
                "ReplayOverlay",
                typeof(RectTransform));
            _replayRoot.transform.SetParent(transform, false);
            Rect(
                _replayRoot.GetComponent<RectTransform>(),
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one);
            Image band = Image(
                "Band",
                _replayRoot.transform,
                new Color(0.02f, 0.03f, 0.028f, 0.88f));
            Rect(
                band.rectTransform,
                new Vector2(0f, -54f),
                Vector2.zero,
                new Vector2(0f, 1f),
                Vector2.one);
            _replayStatus = Label(
                "Status",
                band.transform,
                font,
                16,
                TextAnchor.MiddleLeft);
            Rect(
                _replayStatus.rectTransform,
                new Vector2(20f, 8f),
                new Vector2(-170f, -8f),
                Vector2.zero,
                Vector2.one);
            CreateButton(
                "ExitReplay",
                "EXIT REPLAY",
                new Vector2(-150f, -46f),
                () => _exitReplay?.Invoke(),
                new Vector2(130f, 38f),
                new Vector2(1f, 1f),
                band.transform);
            _replayRoot.SetActive(false);
        }

        private void BuildScopeOverlay(Font font)
        {
            _scopeRoot = new GameObject(
                "SniperScope",
                typeof(RectTransform));
            _scopeRoot.transform.SetParent(transform, false);
            Rect(
                _scopeRoot.GetComponent<RectTransform>(),
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one);

            Image left = Image(
                "LeftShade",
                _scopeRoot.transform,
                new Color(0f, 0f, 0f, 0.32f));
            Rect(
                left.rectTransform,
                Vector2.zero,
                new Vector2(48f, 0f),
                Vector2.zero,
                new Vector2(0f, 1f));
            Image right = Image(
                "RightShade",
                _scopeRoot.transform,
                new Color(0f, 0f, 0f, 0.32f));
            Rect(
                right.rectTransform,
                new Vector2(-48f, 0f),
                Vector2.zero,
                new Vector2(1f, 0f),
                Vector2.one);
            Image top = Image(
                "TopShade",
                _scopeRoot.transform,
                new Color(0f, 0f, 0f, 0.24f));
            Rect(
                top.rectTransform,
                new Vector2(0f, -36f),
                Vector2.zero,
                new Vector2(0f, 1f),
                Vector2.one);
            Image bottom = Image(
                "BottomShade",
                _scopeRoot.transform,
                new Color(0f, 0f, 0f, 0.24f));
            Rect(
                bottom.rectTransform,
                Vector2.zero,
                new Vector2(0f, 36f),
                Vector2.zero,
                new Vector2(1f, 0f));
            Image horizontal = Image(
                "Horizontal",
                _scopeRoot.transform,
                new Color(1f, 1f, 1f, 0.38f));
            Rect(
                horizontal.rectTransform,
                new Vector2(40f, -1f),
                new Vector2(-40f, 1f),
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f));
            Image vertical = Image(
                "Vertical",
                _scopeRoot.transform,
                new Color(1f, 1f, 1f, 0.38f));
            Rect(
                vertical.rectTransform,
                new Vector2(-1f, 40f),
                new Vector2(1f, -40f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 1f));
            foreach (Image image in
                _scopeRoot.GetComponentsInChildren<Image>())
            {
                image.raycastTarget = false;
            }

            _scopeZoom = Label(
                "Zoom",
                _scopeRoot.transform,
                font,
                16,
                TextAnchor.MiddleCenter);
            _scopeZoom.raycastTarget = false;
            Rect(
                _scopeZoom.rectTransform,
                new Vector2(-60f, -126f),
                new Vector2(60f, -96f),
                new Vector2(0.5f, 1f));
            _scopeRoot.SetActive(false);
        }

        private void BuildTouchControls(Font font)
        {
            _touchRoot = new GameObject(
                "TouchControls",
                typeof(RectTransform));
            _touchRoot.transform.SetParent(transform, false);
            Rect(
                _touchRoot.GetComponent<RectTransform>(),
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one);

            Image aimSurface = Image(
                "AimSurface",
                _touchRoot.transform,
                Color.clear);
            Rect(
                aimSurface.rectTransform,
                Vector2.zero,
                Vector2.zero,
                new Vector2(0.35f, 0.18f),
                new Vector2(1f, 0.88f));
            TouchAimControl aim =
                aimSurface.gameObject.AddComponent<TouchAimControl>();
            aim.SetAction((active, position) =>
            {
                _touchAimActive = active;
                _touchAimPosition = position;
            });

            GameObject drivePad = new GameObject(
                "DrivePad",
                typeof(RectTransform));
            drivePad.transform.SetParent(_touchRoot.transform, false);
            Rect(
                drivePad.GetComponent<RectTransform>(),
                new Vector2(24f, 112f),
                new Vector2(242f, 330f),
                Vector2.zero);
            HoldButton(
                drivePad.transform,
                "Forward",
                "^",
                new Vector2(75f, 146f),
                value => SetDrive(0, value),
                new Vector2(68f, 68f));
            HoldButton(
                drivePad.transform,
                "Reverse",
                "v",
                new Vector2(75f, 4f),
                value => SetDrive(1, value),
                new Vector2(68f, 68f));
            HoldButton(
                drivePad.transform,
                "Left",
                "<",
                new Vector2(4f, 75f),
                value => SetDrive(2, value),
                new Vector2(68f, 68f));
            HoldButton(
                drivePad.transform,
                "Right",
                ">",
                new Vector2(146f, 75f),
                value => SetDrive(3, value),
                new Vector2(68f, 68f));

            HoldButton(
                _touchRoot.transform,
                "Fire",
                "FIRE",
                new Vector2(-144f, 32f),
                value => _fireHeld = value,
                new Vector2(112f, 112f),
                new Vector2(1f, 0f));
            HoldButton(
                _touchRoot.transform,
                "Brake",
                "BRAKE",
                new Vector2(-244f, 40f),
                value => _brakeHeld = value,
                new Vector2(84f, 84f),
                new Vector2(1f, 0f));
            CreateButton(
                "Sniper",
                "SCOPE",
                new Vector2(-144f, 160f),
                () => _sniperToggleQueued = true,
                new Vector2(112f, 52f),
                new Vector2(1f, 0f),
                _touchRoot.transform);
            for (int slot = 0; slot < 3; slot++)
            {
                int selected = slot;
                CreateButton(
                    "Shell" + (slot + 1),
                    (slot + 1).ToString(),
                    new Vector2(
                        -316f + slot * 58f,
                        160f),
                    () => _touchShellSlot = selected,
                    new Vector2(52f, 52f),
                    new Vector2(1f, 0f),
                    _touchRoot.transform);
            }
            _touchRoot.SetActive(
                Application.isMobilePlatform || Input.touchSupported);
        }

        private void SetDrive(int direction, bool held)
        {
            _driveHeld[direction] = held;
            _touchDrive.x =
                (_driveHeld[3] ? 1f : 0f) -
                (_driveHeld[2] ? 1f : 0f);
            _touchDrive.y =
                (_driveHeld[0] ? 1f : 0f) -
                (_driveHeld[1] ? 1f : 0f);
        }

        private void HoldButton(
            Transform parent,
            string name,
            string text,
            Vector2 position,
            Action<bool> action,
            Vector2? size = null,
            Vector2? anchor = null)
        {
            Button button = CreateButton(
                name,
                text,
                position,
                null,
                size ?? new Vector2(72f, 72f),
                anchor ?? Vector2.zero,
                parent);
            HoldControl hold =
                button.gameObject.AddComponent<HoldControl>();
            hold.SetAction(action);
        }

        private Button CreateButton(
            string name,
            string text,
            Vector2 position,
            Action action,
            Vector2? size = null,
            Vector2? anchor = null,
            Transform parent = null)
        {
            Image image = Image(
                name,
                parent ?? transform,
                new Color(0.08f, 0.1f, 0.09f, 0.78f));
            Rect(
                image.rectTransform,
                position,
                position + (size ?? new Vector2(48f, 48f)),
                anchor ?? Vector2.zero);
            Button button = image.gameObject.AddComponent<Button>();
            if (action != null)
                button.onClick.AddListener(() => action());
            Text label = Label(
                "Label",
                image.transform,
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf"),
                14,
                TextAnchor.MiddleCenter);
            label.text = text;
            Rect(
                label.rectTransform,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one);
            return button;
        }

        private static Image Image(
            string name,
            Transform parent,
            Color color)
        {
            GameObject child = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            child.transform.SetParent(parent, false);
            Image image = child.GetComponent<Image>();
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
            GameObject child = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            child.transform.SetParent(parent, false);
            Text text = child.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static void Rect(
            RectTransform rect,
            Vector2 min,
            Vector2 max,
            Vector2 anchorMin,
            Vector2? anchorMax = null)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax ?? anchorMin;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }
    }

    public sealed class HoldControl :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler
    {
        private Action<bool> _action;

        public void SetAction(Action<bool> action) { _action = action; }
        public void OnPointerDown(PointerEventData eventData)
        {
            _action?.Invoke(true);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            _action?.Invoke(false);
        }
        private void OnDisable() { _action?.Invoke(false); }
    }

    public sealed class TouchAimControl :
        MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private Action<bool, Vector2> _action;

        public void SetAction(Action<bool, Vector2> action)
        {
            _action = action;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            _action?.Invoke(true, eventData.position);
        }
        public void OnDrag(PointerEventData eventData)
        {
            _action?.Invoke(true, eventData.position);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            _action?.Invoke(false, eventData.position);
        }
        private void OnDisable()
        {
            _action?.Invoke(false, Vector2.zero);
        }
    }
}
