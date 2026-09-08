using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Tests
{
    public sealed class UiAudioFeedbackTests
    {
        [Test]
        public void BindsActionButtonsOnceAndTracksLiveUiVolume()
        {
            GameObject root = new GameObject(
                "UiRoot",
                typeof(RectTransform),
                typeof(Canvas));
            GameObject actionRoot = new GameObject(
                "Action",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            actionRoot.transform.SetParent(root.transform, false);
            Button action = actionRoot.GetComponent<Button>();
            GameObject inertRoot = new GameObject(
                "Rail",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            inertRoot.transform.SetParent(root.transform, false);
            Button inert = inertRoot.GetComponent<Button>();
            inert.transition = Selectable.Transition.None;
            GameSettings settings = new GameSettings(
                new MemorySettingsStore(),
                new SettingsTarget());

            try
            {
                UiAudioFeedback feedback =
                    UiAudioFeedback.BindTree(root.transform, settings);
                UiAudioFeedback.BindTree(root.transform, settings);

                Assert.That(
                    action.GetComponent<UiAudioButtonFeedback>(),
                    Is.Not.Null);
                Assert.That(
                    inert.GetComponent<UiAudioButtonFeedback>(),
                    Is.Null);

                action.onClick.Invoke();
                Assert.That(feedback.PlayCount, Is.EqualTo(1));
                Assert.That(feedback.Source.clip.name, Is.EqualTo("UiClick"));
                float volume = feedback.Source.volume;

                settings.SetUiVolume(0.25f);

                Assert.That(
                    feedback.Source.volume,
                    Is.EqualTo(volume * 0.25f).Within(0.001f));
                action.onClick.Invoke();
                Assert.That(feedback.PlayCount, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private sealed class MemorySettingsStore : ISettingsStore
        {
            private readonly Dictionary<string, int> _ints =
                new Dictionary<string, int>();
            private readonly Dictionary<string, float> _floats =
                new Dictionary<string, float>();

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
            public void Save() { }
        }

        private sealed class SettingsTarget : ISettingsTarget
        {
            public int QualityLevelCount => 3;
            public void ApplyVolume(float value) { }
            public void ApplyQuality(int value) { }
            public void ApplyFullscreen(bool value) { }
        }
    }
}
