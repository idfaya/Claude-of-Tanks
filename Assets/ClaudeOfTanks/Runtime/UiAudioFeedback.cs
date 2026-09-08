using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed class UiAudioFeedback : MonoBehaviour
    {
        private const float ClickVolume = 0.22f;
        private GameSettings _settings;
        private AudioClip _click;

        public AudioSource Source { get; private set; }
        public int PlayCount { get; private set; }

        public static UiAudioFeedback BindTree(
            Transform root,
            GameSettings settings = null)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            UiAudioFeedback feedback =
                root.GetComponentInParent<UiAudioFeedback>();
            if (feedback == null)
            {
                feedback =
                    root.gameObject.AddComponent<UiAudioFeedback>();
                feedback.Initialize(settings ?? GameSettings.Current);
            }

            Button[] buttons =
                root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button.name == "Rail" &&
                    button.transition == Selectable.Transition.None)
                {
                    continue;
                }
                UiAudioButtonFeedback.Bind(button, feedback);
            }
            return feedback;
        }

        public void PlayClick()
        {
            PlayCount++;
            Source.Stop();
            Source.volume = ClickVolume * _settings.UiVolume;
            Source.pitch = PlayCount % 2 == 0 ? 1.04f : 1f;
            if (Application.isPlaying) Source.Play();
        }

        private void Initialize(GameSettings settings)
        {
            _settings = settings;
            Source = gameObject.AddComponent<AudioSource>();
            Source.playOnAwake = false;
            Source.loop = false;
            Source.spatialBlend = 0f;
            Source.dopplerLevel = 0f;
            _click = BattleAudioClips.Tone(
                "UiClick",
                720f,
                0.055f,
                0.34f,
                0.04f);
            Source.clip = _click;
            _settings.AudioChanged += ApplyMix;
            ApplyMix();
        }

        private void ApplyMix()
        {
            if (Source != null)
                Source.volume = ClickVolume * _settings.UiVolume;
        }

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.AudioChanged -= ApplyMix;
            if (_click == null) return;
            if (Application.isPlaying) Destroy(_click);
            else DestroyImmediate(_click);
        }
    }

    public sealed class UiAudioButtonFeedback : MonoBehaviour
    {
        private UiAudioFeedback _feedback;

        public static void Bind(
            Button button,
            UiAudioFeedback feedback)
        {
            if (button == null)
                throw new ArgumentNullException(nameof(button));
            if (feedback == null)
                throw new ArgumentNullException(nameof(feedback));
            UiAudioButtonFeedback binding =
                button.GetComponent<UiAudioButtonFeedback>();
            if (binding != null) return;
            binding =
                button.gameObject.AddComponent<UiAudioButtonFeedback>();
            binding._feedback = feedback;
            button.onClick.AddListener(binding.Play);
        }

        public void Play()
        {
            _feedback?.PlayClick();
        }
    }
}
