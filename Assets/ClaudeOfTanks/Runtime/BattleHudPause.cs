using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ClaudeOfTanks.Runtime
{
    internal sealed class BattleHudPause : IDisposable
    {
        private readonly Transform _hud;
        private readonly GameSettingsPanel _panel;
        private readonly Action _clearInput;

        public BattleHudPause(
            Transform hud,
            GameSettingsPanel panel,
            Action clearInput)
        {
            _hud = hud ??
                throw new ArgumentNullException(nameof(hud));
            _panel = panel ??
                throw new ArgumentNullException(nameof(panel));
            _clearInput = clearInput ??
                throw new ArgumentNullException(nameof(clearInput));
            _panel.SetPauseContext(true);
            _panel.VisibilityChanged += OnVisibilityChanged;
        }

        public bool IsPaused => _panel.IsVisible;

        public void SetPaused(bool paused, bool blocked)
        {
            if (paused && blocked) return;
            if (paused) _panel.Open();
            else _panel.Close();
        }

        public void Tick(bool blocked)
        {
            bool pressed = Input.GetKeyDown(KeyCode.Escape);
#if ENABLE_INPUT_SYSTEM
            pressed |=
                (Keyboard.current != null &&
                    Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null &&
                    Gamepad.current.startButton.wasPressedThisFrame);
#endif
            if (pressed) SetPaused(!IsPaused, blocked);
        }

        public void Dispose()
        {
            _panel.VisibilityChanged -= OnVisibilityChanged;
            if (IsPaused) ApplyPause(false);
        }

        private void OnVisibilityChanged(bool visible)
        {
            _clearInput();
            ApplyPause(visible);
        }

        private void ApplyPause(bool paused)
        {
            BattleController solo =
                _hud.GetComponentInParent<BattleController>();
            if (solo != null) solo.enabled = !paused;
            Transform owner = _hud.parent;
            BattleAudio audio = owner != null
                ? owner.GetComponentInChildren<BattleAudio>()
                : null;
            audio?.SetPauseDucking(paused);
        }
    }
}
