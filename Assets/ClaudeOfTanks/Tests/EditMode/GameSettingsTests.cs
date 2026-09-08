using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Tests
{
    public sealed class GameSettingsTests
    {
        [Test]
        public void SettingsPersistClampAndApply()
        {
            MemoryStore store = new MemoryStore();
            RecordingTarget target = new RecordingTarget { QualityLevelCount = 4 };
            GameSettings settings = new GameSettings(store, target);

            Assert.That(settings.MasterVolume, Is.EqualTo(0.85f).Within(0.001f));
            Assert.That(settings.EngineVolume, Is.EqualTo(1f).Within(0.001f));
            Assert.That(settings.CombatVolume, Is.EqualTo(1f).Within(0.001f));
            Assert.That(settings.AmbienceVolume, Is.EqualTo(1f).Within(0.001f));
            Assert.That(settings.UiVolume, Is.EqualTo(1f).Within(0.001f));
            Assert.That(settings.VoiceVolume, Is.EqualTo(1f).Within(0.001f));
            Assert.That(settings.QualityLevel, Is.EqualTo(3));
            Assert.That(settings.Fullscreen, Is.True);
            Assert.That(settings.ReducedMotion, Is.False);
            Assert.That(settings.HighContrast, Is.False);
            Assert.That(settings.HudScale, Is.EqualTo(1f).Within(0.001f));

            settings.SetMasterVolume(2f);
            settings.SetEngineVolume(-1f);
            settings.SetCombatVolume(0.2f);
            settings.SetAmbienceVolume(0.3f);
            settings.SetUiVolume(0.4f);
            settings.SetVoiceVolume(2f);
            settings.SetQualityLevel(99);
            settings.SetFullscreen(false);
            settings.SetReducedMotion(true);
            settings.SetHighContrast(true);
            settings.SetHudScale(3f);
            Assert.That(settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(settings.EngineVolume, Is.Zero);
            Assert.That(settings.CombatVolume, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(settings.AmbienceVolume, Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(settings.UiVolume, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(settings.VoiceVolume, Is.EqualTo(1f));
            Assert.That(settings.QualityLevel, Is.EqualTo(3));
            Assert.That(settings.Fullscreen, Is.False);
            Assert.That(settings.ReducedMotion, Is.True);
            Assert.That(settings.HighContrast, Is.True);
            Assert.That(settings.HudScale, Is.EqualTo(1.25f).Within(0.001f));
            Assert.That(target.Volume, Is.EqualTo(1f));
            Assert.That(target.Quality, Is.EqualTo(3));
            Assert.That(target.Fullscreen, Is.False);
            Assert.That(store.SaveCount, Is.GreaterThanOrEqualTo(6));

            GameSettings restored = new GameSettings(store, target);
            Assert.That(restored.MasterVolume, Is.EqualTo(1f));
            Assert.That(restored.EngineVolume, Is.Zero);
            Assert.That(restored.CombatVolume, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(restored.AmbienceVolume, Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(restored.UiVolume, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(restored.VoiceVolume, Is.EqualTo(1f));
            Assert.That(restored.QualityLevel, Is.EqualTo(3));
            Assert.That(restored.Fullscreen, Is.False);
            Assert.That(restored.ReducedMotion, Is.True);
            Assert.That(restored.HighContrast, Is.True);
            Assert.That(restored.HudScale, Is.EqualTo(1.25f).Within(0.001f));

            restored.ResetDefaults();
            Assert.That(restored.MasterVolume, Is.EqualTo(0.85f).Within(0.001f));
            Assert.That(restored.EngineVolume, Is.EqualTo(1f));
            Assert.That(restored.CombatVolume, Is.EqualTo(1f));
            Assert.That(restored.AmbienceVolume, Is.EqualTo(1f));
            Assert.That(restored.UiVolume, Is.EqualTo(1f));
            Assert.That(restored.VoiceVolume, Is.EqualTo(1f));
        }

        [Test]
        public void RebindingSwapsConflictsAndPersists()
        {
            MemoryStore store = new MemoryStore();
            RecordingTarget target = new RecordingTarget { QualityLevelCount = 3 };
            GameSettings settings = new GameSettings(store, target);

            settings.SetBinding(GameInputAction.Forward, KeyCode.S);
            Assert.That(settings.GetBinding(GameInputAction.Forward), Is.EqualTo(KeyCode.S));
            Assert.That(settings.GetBinding(GameInputAction.Reverse), Is.EqualTo(KeyCode.W));
            Assert.Throws<System.ArgumentException>(() =>
                settings.SetBinding(GameInputAction.Fire, KeyCode.Escape));

            GameSettings restored = new GameSettings(store, target);
            Assert.That(restored.GetBinding(GameInputAction.Forward), Is.EqualTo(KeyCode.S));
            Assert.That(restored.GetBinding(GameInputAction.Reverse), Is.EqualTo(KeyCode.W));

            store.SetInt("cot.settings.bind.Forward", (int)KeyCode.Q);
            store.SetInt("cot.settings.bind.Reverse", (int)KeyCode.Q);
            GameSettings recovered = new GameSettings(store, target);
            Assert.That(recovered.GetBinding(GameInputAction.Forward), Is.EqualTo(KeyCode.W));
            Assert.That(recovered.GetBinding(GameInputAction.Reverse), Is.EqualTo(KeyCode.S));
        }

        [Test]
        public void SharedSettingsPanelCapturesBindingsAndControlsVisibility()
        {
            GameObject parent = new GameObject("SettingsTest", typeof(RectTransform));
            GameSettings settings = new GameSettings(
                new MemoryStore(),
                new RecordingTarget { QualityLevelCount = 3 });
            try
            {
                GameSettingsPanel panel = GameSettingsPanel.Create(parent.transform, settings);
                Assert.That(panel.IsVisible, Is.False);
                panel.Open();
                Assert.That(panel.IsVisible, Is.True);
                Assert.That(panel.transform.Find("Shade/Surface/Volume"), Is.Not.Null);
                Slider engineVolume = panel.transform
                    .Find("Shade/Surface/AudioMix/EngineVolume")
                    .GetComponent<Slider>();
                Slider combatVolume = panel.transform
                    .Find("Shade/Surface/AudioMix/CombatVolume")
                    .GetComponent<Slider>();
                Assert.That(panel.transform.Find(
                    "Shade/Surface/AudioMix/AmbienceVolume"), Is.Not.Null);
                Assert.That(panel.transform.Find(
                    "Shade/Surface/AudioMix/UiVolume"), Is.Not.Null);
                Assert.That(panel.transform.Find(
                    "Shade/Surface/AudioMix/VoiceVolume"), Is.Not.Null);
                Assert.That(panel.transform.Find("Shade/Surface/Quality"), Is.Not.Null);
                Assert.That(panel.transform.Find("Shade/Surface/Fullscreen"), Is.Not.Null);
                Toggle reducedMotion = panel.transform
                    .Find("Shade/Surface/ReducedMotion")
                    .GetComponent<Toggle>();
                Toggle highContrast = panel.transform
                    .Find("Shade/Surface/HighContrast")
                    .GetComponent<Toggle>();
                Slider hudScale = panel.transform
                    .Find("Shade/Surface/HudScale")
                    .GetComponent<Slider>();
                reducedMotion.isOn = true;
                highContrast.isOn = true;
                hudScale.value = 1.2f;
                engineVolume.value = 0.35f;
                combatVolume.value = 0.45f;
                Assert.That(settings.ReducedMotion, Is.True);
                Assert.That(settings.HighContrast, Is.True);
                Assert.That(settings.HudScale, Is.EqualTo(1.2f).Within(0.001f));
                Assert.That(settings.EngineVolume, Is.EqualTo(0.35f).Within(0.001f));
                Assert.That(settings.CombatVolume, Is.EqualTo(0.45f).Within(0.001f));

                panel.BeginRebind(GameInputAction.Forward);
                Assert.That(panel.WaitingForBinding, Is.EqualTo(GameInputAction.Forward));
                Button binding = panel.transform.Find("Shade/Surface/ForwardBinding")
                    .GetComponent<Button>();
                Assert.That(binding.GetComponentInChildren<Text>().text, Is.EqualTo("PRESS KEY"));
                panel.CaptureBinding(KeyCode.UpArrow);
                Assert.That(settings.GetBinding(GameInputAction.Forward), Is.EqualTo(KeyCode.UpArrow));
                Assert.That(binding.GetComponentInChildren<Text>().text, Is.EqualTo("UPARROW"));
                Assert.That(panel.WaitingForBinding, Is.Null);
                panel.Close();
                Assert.That(panel.IsVisible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        private sealed class MemoryStore : ISettingsStore
        {
            private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
            private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();
            public int SaveCount { get; private set; }
            public int GetInt(string key, int fallback)
            {
                int value;
                return _ints.TryGetValue(key, out value) ? value : fallback;
            }
            public float GetFloat(string key, float fallback)
            {
                float value;
                return _floats.TryGetValue(key, out value) ? value : fallback;
            }
            public void SetInt(string key, int value) { _ints[key] = value; }
            public void SetFloat(string key, float value) { _floats[key] = value; }
            public void Save() { SaveCount++; }
        }

        private sealed class RecordingTarget : ISettingsTarget
        {
            public int QualityLevelCount { get; set; }
            public float Volume { get; private set; }
            public int Quality { get; private set; }
            public bool Fullscreen { get; private set; }
            public void ApplyVolume(float value) { Volume = value; }
            public void ApplyQuality(int value) { Quality = value; }
            public void ApplyFullscreen(bool value) { Fullscreen = value; }
        }
    }
}
