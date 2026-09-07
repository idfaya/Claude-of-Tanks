using System;
using System.Text;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class PlayerPrefsRankedIdentityStore :
        IRankedIdentityStore
    {
        private const string Prefix = "cot.ranked.identity.v1.";

        public RankedIdentity Load(string scope)
        {
            string json = PlayerPrefs.GetString(
                Key(scope),
                string.Empty);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                RankedIdentity identity =
                    JsonUtility.FromJson<RankedIdentity>(json);
                return Valid(identity) ? identity : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public void Save(
            string scope,
            RankedIdentity identity)
        {
            if (!Valid(identity))
                throw new ArgumentException(
                    "Ranked identity is incomplete.",
                    nameof(identity));
            PlayerPrefs.SetString(
                Key(scope),
                JsonUtility.ToJson(identity));
            PlayerPrefs.Save();
        }

        public void Clear(string scope)
        {
            PlayerPrefs.DeleteKey(Key(scope));
            PlayerPrefs.Save();
        }

        private static string Key(string scope)
        {
            return Prefix + Convert.ToBase64String(
                Encoding.UTF8.GetBytes(scope ?? string.Empty))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static bool Valid(RankedIdentity identity)
        {
            return identity != null &&
                !string.IsNullOrEmpty(identity.playerId) &&
                !string.IsNullOrEmpty(identity.token);
        }
    }
}
