using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class PrivateRoomConnectionDefaults
    {
        private const string PlayerIdKey = "cot.privateRoom.playerId";

        public static string SignalingEndpoint()
        {
            string environment = Environment.GetEnvironmentVariable(
                "COT_SIGNAL_URL");
            if (!string.IsNullOrWhiteSpace(environment))
                return environment.Trim();
            return "ws://127.0.0.1:8080/signal";
        }

        public static string OriginFor(Uri endpoint)
        {
            UriBuilder origin = new UriBuilder(endpoint)
            {
                Scheme = endpoint.Scheme == "wss" ? "https" : "http",
                Path = string.Empty,
                Query = string.Empty,
                Fragment = string.Empty
            };
            return origin.Uri.GetLeftPart(UriPartial.Authority);
        }

        public static string LoadOrCreatePlayerId()
        {
            string value = PlayerPrefs.GetString(PlayerIdKey, string.Empty);
            if (!string.IsNullOrEmpty(value)) return value;
            value = "unity-" + Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PlayerIdKey, value);
            PlayerPrefs.Save();
            return value;
        }
    }
}
