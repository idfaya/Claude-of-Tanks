using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    [Serializable]
    public sealed class CustomCamouflageProfile
    {
        public string scheme = "organic";
        public string baseColor = "#48523d";
        public string weatherColor = "#59634a";
        public string patchColorA = "#2d3528";
        public string patchColorB = "#777050";
        public float scale = 0.34f;

        public CamouflageRecipe ToRecipe()
        {
            return new CamouflageRecipe
            {
                scheme = scheme,
                baseColor = baseColor,
                weatherColor = weatherColor,
                patchColors = new[]
                {
                    patchColorA,
                    patchColorB
                },
                camoScale = Mathf.Clamp(
                    scale,
                    0.1f,
                    1.5f)
            };
        }
    }

    public static class CustomCamouflageStore
    {
        private const string Key =
            "cot.custom-camouflage.v1";
        private static CustomCamouflageProfile
            _cached;

        public static CustomCamouflageProfile Load()
        {
            if (_cached != null)
                return Copy(_cached);
            string json = PlayerPrefs.GetString(
                Key,
                string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _cached =
                        JsonUtility
                            .FromJson<
                                CustomCamouflageProfile>(
                                json);
                }
                catch (ArgumentException)
                {
                    _cached = null;
                }
            }
            _cached = Sanitize(
                _cached ??
                new CustomCamouflageProfile());
            return Copy(_cached);
        }

        public static void Save(
            CustomCamouflageProfile profile)
        {
            _cached = Sanitize(
                profile ??
                throw new ArgumentNullException(
                    nameof(profile)));
            PlayerPrefs.SetString(
                Key,
                JsonUtility.ToJson(_cached));
            PlayerPrefs.Save();
        }

        private static CustomCamouflageProfile
            Sanitize(
                CustomCamouflageProfile value)
        {
            return new CustomCamouflageProfile
            {
                scheme = ValidScheme(
                    value.scheme),
                baseColor = ValidColor(
                    value.baseColor,
                    "#48523d"),
                weatherColor = ValidColor(
                    value.weatherColor,
                    "#59634a"),
                patchColorA = ValidColor(
                    value.patchColorA,
                    "#2d3528"),
                patchColorB = ValidColor(
                    value.patchColorB,
                    "#777050"),
                scale = Mathf.Clamp(
                    value.scale,
                    0.1f,
                    1.5f)
            };
        }

        private static string ValidScheme(
            string value)
        {
            switch (value)
            {
                case "solid":
                case "organic":
                case "digital":
                case "stripes":
                case "geometric":
                case "dots":
                    return value;
                default:
                    return "organic";
            }
        }

        private static string ValidColor(
            string value,
            string fallback)
        {
            Color ignored;
            return !string.IsNullOrEmpty(value) &&
                ColorUtility.TryParseHtmlString(
                    value,
                    out ignored)
                    ? value
                    : fallback;
        }

        private static CustomCamouflageProfile
            Copy(
                CustomCamouflageProfile source)
        {
            return new CustomCamouflageProfile
            {
                scheme = source.scheme,
                baseColor = source.baseColor,
                weatherColor =
                    source.weatherColor,
                patchColorA =
                    source.patchColorA,
                patchColorB =
                    source.patchColorB,
                scale = source.scale
            };
        }
    }
}
