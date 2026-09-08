using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace ClaudeOfTanks.Runtime
{
    public enum GameInputAction
    {
        Forward,
        Reverse,
        Left,
        Right,
        Brake,
        Fire,
        Sniper,
        Repair,
        FirstAid,
        Extinguisher
    }

    public interface ISettingsStore
    {
        int GetInt(string key, int fallback);
        float GetFloat(string key, float fallback);
        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        void Save();
    }

    public interface ISettingsTarget
    {
        int QualityLevelCount { get; }
        void ApplyVolume(float value);
        void ApplyQuality(int value);
        void ApplyFullscreen(bool value);
    }

    public sealed class GameSettings
    {
        private const string Prefix = "cot.settings.";
        public const float MinimumHudScale = 0.8f;
        public const float MaximumHudScale = 1.25f;
        private static GameSettings _current;
        private readonly ISettingsStore _store;
        private readonly ISettingsTarget _target;
        private readonly Dictionary<GameInputAction, KeyCode> _bindings =
            new Dictionary<GameInputAction, KeyCode>();

        public GameSettings(ISettingsStore store, ISettingsTarget target)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            Load();
        }

        public static GameSettings Current =>
            _current ?? (_current = new GameSettings(
                new PlayerPrefsSettingsStore(),
                new UnitySettingsTarget()));

        public static readonly KeyCode[] AllowedBindings =
        {
            KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
            KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F,
            KeyCode.Space, KeyCode.LeftShift, KeyCode.LeftControl,
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
            KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
        };

        public float MasterVolume { get; private set; }
        public float EngineVolume { get; private set; }
        public float CombatVolume { get; private set; }
        public float AmbienceVolume { get; private set; }
        public float UiVolume { get; private set; }
        public float VoiceVolume { get; private set; }
        public int QualityLevel { get; private set; }
        public bool Fullscreen { get; private set; }
        public bool ReducedMotion { get; private set; }
        public bool HighContrast { get; private set; }
        public float HudScale { get; private set; }
        public event Action PresentationChanged;
        public event Action AudioChanged;

        public KeyCode GetBinding(GameInputAction action)
        {
            KeyCode key;
            if (!_bindings.TryGetValue(action, out key))
                throw new ArgumentOutOfRangeException(nameof(action));
            return key;
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            _store.SetFloat(Prefix + "volume", MasterVolume);
            _store.Save();
            _target.ApplyVolume(MasterVolume);
            AudioChanged?.Invoke();
        }

        public void SetEngineVolume(float value)
        {
            EngineVolume = SaveVolume("volume.engine", value);
            AudioChanged?.Invoke();
        }

        public void SetCombatVolume(float value)
        {
            CombatVolume = SaveVolume("volume.combat", value);
            AudioChanged?.Invoke();
        }

        public void SetAmbienceVolume(float value)
        {
            AmbienceVolume = SaveVolume("volume.ambience", value);
            AudioChanged?.Invoke();
        }

        public void SetUiVolume(float value)
        {
            UiVolume = SaveVolume("volume.ui", value);
            AudioChanged?.Invoke();
        }

        public void SetVoiceVolume(float value)
        {
            VoiceVolume = SaveVolume("volume.voice", value);
            AudioChanged?.Invoke();
        }

        public void SetQualityLevel(int value)
        {
            int maximum = Math.Max(0, _target.QualityLevelCount - 1);
            QualityLevel = Mathf.Clamp(value, 0, maximum);
            _store.SetInt(Prefix + "quality", QualityLevel);
            _store.Save();
            _target.ApplyQuality(QualityLevel);
            PresentationChanged?.Invoke();
        }

        public void SetFullscreen(bool value)
        {
            Fullscreen = value;
            _store.SetInt(Prefix + "fullscreen", value ? 1 : 0);
            _store.Save();
            _target.ApplyFullscreen(value);
        }

        public void SetReducedMotion(bool value)
        {
            ReducedMotion = value;
            _store.SetInt(Prefix + "reducedMotion", value ? 1 : 0);
            _store.Save();
            PresentationChanged?.Invoke();
        }

        public void SetHighContrast(bool value)
        {
            HighContrast = value;
            _store.SetInt(Prefix + "highContrast", value ? 1 : 0);
            _store.Save();
            PresentationChanged?.Invoke();
        }

        public void SetHudScale(float value)
        {
            HudScale = Mathf.Clamp(
                value,
                MinimumHudScale,
                MaximumHudScale);
            _store.SetFloat(Prefix + "hudScale", HudScale);
            _store.Save();
            PresentationChanged?.Invoke();
        }

        public void SetBinding(GameInputAction action, KeyCode key)
        {
            if (!Enum.IsDefined(typeof(GameInputAction), action))
                throw new ArgumentOutOfRangeException(nameof(action));
            if (Array.IndexOf(AllowedBindings, key) < 0)
                throw new ArgumentException("Key is unavailable for rebinding.", nameof(key));
            KeyCode previous = GetBinding(action);
            GameInputAction? collision = null;
            foreach (KeyValuePair<GameInputAction, KeyCode> binding in _bindings)
                if (binding.Key != action && binding.Value == key) collision = binding.Key;
            _bindings[action] = key;
            if (collision.HasValue) _bindings[collision.Value] = previous;
            SaveBindings();
            PresentationChanged?.Invoke();
        }

        public void ResetDefaults()
        {
            InstallDefaults();
            SetMasterVolume(0.85f);
            SetEngineVolume(1f);
            SetCombatVolume(1f);
            SetAmbienceVolume(1f);
            SetUiVolume(1f);
            SetVoiceVolume(1f);
            SetQualityLevel(Math.Max(0, _target.QualityLevelCount - 1));
            SetFullscreen(true);
            SetReducedMotion(false);
            SetHighContrast(false);
            SetHudScale(1f);
            SaveBindings();
        }

        public void Apply()
        {
            _target.ApplyVolume(MasterVolume);
            _target.ApplyQuality(QualityLevel);
            _target.ApplyFullscreen(Fullscreen);
        }

        public bool IsPressed(GameInputAction action)
        {
            return IsKeyPressed(GetBinding(action), false);
        }

        public bool WasPressedThisFrame(GameInputAction action)
        {
            return IsKeyPressed(GetBinding(action), true);
        }

        public static bool WasKeyPressedThisFrame(KeyCode key)
        {
            return IsKeyPressed(key, true);
        }

        private void Load()
        {
            InstallDefaults();
            MasterVolume = Mathf.Clamp01(_store.GetFloat(Prefix + "volume", 0.85f));
            EngineVolume = LoadVolume("volume.engine");
            CombatVolume = LoadVolume("volume.combat");
            AmbienceVolume = LoadVolume("volume.ambience");
            UiVolume = LoadVolume("volume.ui");
            VoiceVolume = LoadVolume("volume.voice");
            int maximum = Math.Max(0, _target.QualityLevelCount - 1);
            QualityLevel = Mathf.Clamp(
                _store.GetInt(Prefix + "quality", maximum),
                0,
                maximum);
            Fullscreen = _store.GetInt(Prefix + "fullscreen", 1) != 0;
            ReducedMotion =
                _store.GetInt(Prefix + "reducedMotion", 0) != 0;
            HighContrast =
                _store.GetInt(Prefix + "highContrast", 0) != 0;
            HudScale = Mathf.Clamp(
                _store.GetFloat(Prefix + "hudScale", 1f),
                MinimumHudScale,
                MaximumHudScale);
            Array actions = Enum.GetValues(typeof(GameInputAction));
            Dictionary<GameInputAction, KeyCode> loaded =
                new Dictionary<GameInputAction, KeyCode>();
            HashSet<KeyCode> unique = new HashSet<KeyCode>();
            bool validBindings = true;
            for (int i = 0; i < actions.Length; i++)
            {
                GameInputAction action = (GameInputAction)actions.GetValue(i);
                KeyCode fallback = _bindings[action];
                KeyCode stored = (KeyCode)_store.GetInt(BindingKey(action), (int)fallback);
                if (Array.IndexOf(AllowedBindings, stored) < 0 || !unique.Add(stored))
                    validBindings = false;
                loaded[action] = stored;
            }
            if (validBindings)
                foreach (KeyValuePair<GameInputAction, KeyCode> binding in loaded)
                    _bindings[binding.Key] = binding.Value;
            Apply();
        }

        private float LoadVolume(string key)
        {
            return Mathf.Clamp01(_store.GetFloat(Prefix + key, 1f));
        }

        private float SaveVolume(string key, float value)
        {
            float clamped = Mathf.Clamp01(value);
            _store.SetFloat(Prefix + key, clamped);
            _store.Save();
            return clamped;
        }

        private void InstallDefaults()
        {
            _bindings[GameInputAction.Forward] = KeyCode.W;
            _bindings[GameInputAction.Reverse] = KeyCode.S;
            _bindings[GameInputAction.Left] = KeyCode.A;
            _bindings[GameInputAction.Right] = KeyCode.D;
            _bindings[GameInputAction.Brake] = KeyCode.LeftControl;
            _bindings[GameInputAction.Fire] = KeyCode.Space;
            _bindings[GameInputAction.Sniper] = KeyCode.LeftShift;
            _bindings[GameInputAction.Repair] = KeyCode.Alpha4;
            _bindings[GameInputAction.FirstAid] = KeyCode.Alpha5;
            _bindings[GameInputAction.Extinguisher] = KeyCode.Alpha6;
        }

        private void SaveBindings()
        {
            foreach (KeyValuePair<GameInputAction, KeyCode> binding in _bindings)
                _store.SetInt(BindingKey(binding.Key), (int)binding.Value);
            _store.Save();
        }

        private static string BindingKey(GameInputAction action)
        {
            return Prefix + "bind." + action;
        }

        private static bool IsKeyPressed(KeyCode key, bool thisFrame)
        {
            bool pressed = thisFrame ? Input.GetKeyDown(key) : Input.GetKey(key);
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            Key inputKey;
            if (keyboard != null && TryInputSystemKey(key, out inputKey))
            {
                KeyControl control = keyboard[inputKey];
                pressed |= thisFrame ? control.wasPressedThisFrame : control.isPressed;
            }
#endif
            return pressed;
        }

#if ENABLE_INPUT_SYSTEM
        private static bool TryInputSystemKey(KeyCode key, out Key result)
        {
            switch (key)
            {
                case KeyCode.W: result = Key.W; return true;
                case KeyCode.A: result = Key.A; return true;
                case KeyCode.S: result = Key.S; return true;
                case KeyCode.D: result = Key.D; return true;
                case KeyCode.Q: result = Key.Q; return true;
                case KeyCode.E: result = Key.E; return true;
                case KeyCode.R: result = Key.R; return true;
                case KeyCode.F: result = Key.F; return true;
                case KeyCode.Space: result = Key.Space; return true;
                case KeyCode.LeftShift: result = Key.LeftShift; return true;
                case KeyCode.LeftControl: result = Key.LeftCtrl; return true;
                case KeyCode.UpArrow: result = Key.UpArrow; return true;
                case KeyCode.DownArrow: result = Key.DownArrow; return true;
                case KeyCode.LeftArrow: result = Key.LeftArrow; return true;
                case KeyCode.RightArrow: result = Key.RightArrow; return true;
                case KeyCode.Alpha1: result = Key.Digit1; return true;
                case KeyCode.Alpha2: result = Key.Digit2; return true;
                case KeyCode.Alpha3: result = Key.Digit3; return true;
                case KeyCode.Alpha4: result = Key.Digit4; return true;
                case KeyCode.Alpha5: result = Key.Digit5; return true;
                case KeyCode.Alpha6: result = Key.Digit6; return true;
                default: result = Key.None; return false;
            }
        }
#endif
    }

    internal sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public int GetInt(string key, int fallback) { return PlayerPrefs.GetInt(key, fallback); }
        public float GetFloat(string key, float fallback) { return PlayerPrefs.GetFloat(key, fallback); }
        public void SetInt(string key, int value) { PlayerPrefs.SetInt(key, value); }
        public void SetFloat(string key, float value) { PlayerPrefs.SetFloat(key, value); }
        public void Save() { PlayerPrefs.Save(); }
    }

    internal sealed class UnitySettingsTarget : ISettingsTarget
    {
        public int QualityLevelCount => Math.Max(1, QualitySettings.names.Length);
        public void ApplyVolume(float value) { AudioListener.volume = value; }
        public void ApplyQuality(int value) { QualitySettings.SetQualityLevel(value, true); }
        public void ApplyFullscreen(bool value)
        {
            if (!Application.isEditor) Screen.fullScreen = value;
        }
    }
}
