using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class BattleHudHydropneumaticControl
    {
        private readonly RectTransform _button;
        private readonly Text _label;
        private bool _queued;

        public BattleHudHydropneumaticControl(
            Button button)
        {
            _button =
                button.GetComponent<RectTransform>();
            _label =
                button.GetComponentInChildren<Text>();
            button.onClick.AddListener(
                () => _queued = true);
            button.gameObject.SetActive(false);
        }

        public void SetState(TankState tank)
        {
            bool available =
                tank.Spec.HydropneumaticAim != null;
            _button.gameObject.SetActive(available);
            if (available)
            {
                _label.text =
                    tank.HydropneumaticAimActive
                        ? "E ON"
                        : "E";
            }
        }

        public void SetLayout(bool portrait)
        {
            Vector2 position = portrait
                ? new Vector2(588f, 306f)
                : new Vector2(430f, 76f);
            _button.anchorMin = Vector2.zero;
            _button.anchorMax = Vector2.zero;
            _button.offsetMin = position;
            _button.offsetMax =
                position + new Vector2(64f, 40f);
        }

        public bool Consume()
        {
            bool value = _queued;
            _queued = false;
            return value;
        }

        public void Clear()
        {
            _queued = false;
        }
    }
}
