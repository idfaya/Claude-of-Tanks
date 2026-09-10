using System;
#if COT_STANDALONE_SERVER
using System.Text.Json;
#else
using UnityEngine;
#endif

namespace ClaudeOfTanks.Server
{
    internal static class ServerJson
    {
#if COT_STANDALONE_SERVER
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                IncludeFields = true,
                MaxDepth = 64
            };
#endif

        public static string Serialize(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
#if COT_STANDALONE_SERVER
            return JsonSerializer.Serialize(value, value.GetType(), Options);
#else
            return JsonUtility.ToJson(value);
#endif
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new FormatException("JSON payload is empty.");
#if COT_STANDALONE_SERVER
            return JsonSerializer.Deserialize<T>(json, Options);
#else
            return JsonUtility.FromJson<T>(json);
#endif
        }
    }
}
