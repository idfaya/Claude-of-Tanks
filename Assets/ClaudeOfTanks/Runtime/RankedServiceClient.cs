using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    [Serializable]
    public sealed class RankedIdentity
    {
        public string playerId;
        public string token;
        public string name;
        public int rating;
        public string rank;
        public int matches;
    }

    [Serializable]
    public sealed class RankedProfile
    {
        public string playerId;
        public string name;
        public int rating;
        public string rank;
        public int matches;
        public int wins;
        public int losses;
        public int draws;
        public int bestRating;
        public int place;
    }

    [Serializable]
    public sealed class RankedLeaderboard
    {
        public RankedProfile[] players;
    }

    [Serializable]
    public sealed class RankedRosterSeatData
    {
        public string id;
        public string entityId;
        public string name;
        public string specId;
        public string[] equipment;
        public string camo;
        public string team;
        public int rating;
    }

    [Serializable]
    public sealed class RankedMatchAssignmentData
    {
        public string matchId;
        public string playerId;
        public string token;
        public int round;
        public uint seed;
        public string mapId;
        public string mode;
        public int teamSize;
        public RankedRosterSeatData[] roster;
    }

    [Serializable]
    public sealed class RankedQueueState
    {
        public string ticketId;
        public string ticketToken;
        public string status;
        public long queuedAtMs;
        public int teamSize;
        public int rating;
        public RankedMatchAssignmentData match;
        public string result;
        public RankedProfile profile;
    }

    public sealed class RankedServiceException : Exception
    {
        public RankedServiceException(
            int statusCode,
            string code,
            string message)
            : base(string.IsNullOrEmpty(message) ? code : message)
        {
            StatusCode = statusCode;
            Code = code;
        }

        public int StatusCode { get; }
        public string Code { get; }
    }

    public interface IRankedIdentityStore
    {
        RankedIdentity Load(string scope);
        void Save(string scope, RankedIdentity identity);
        void Clear(string scope);
    }

    public sealed partial class RankedServiceClient : IDisposable
    {
        [Serializable] private sealed class IdentityRequest
        {
            public string name;
        }

        [Serializable] private sealed class QueueRequest
        {
            public string playerId;
            public string specId;
            public string[] equipment;
            public string camo;
            public int teamSize;
        }

        [Serializable] private sealed class ErrorResponse
        {
            public string error;
            public string message;
        }

        private readonly HttpClient _http;
        private readonly bool _ownsHttp;
        private readonly IRankedIdentityStore _identityStore;
        private readonly Uri _baseUri;
        private readonly string _identityScope;
        private RankedIdentity _identity;
        private bool _disposed;

        public RankedServiceClient(
            string serviceUrl,
            IRankedIdentityStore identityStore = null,
            HttpClient httpClient = null)
        {
            _baseUri = NormalizeServiceUri(serviceUrl);
            _identityScope = _baseUri.AbsoluteUri.TrimEnd('/');
            _identityStore = identityStore ??
                new PlayerPrefsRankedIdentityStore();
            _identity = _identityStore.Load(_identityScope);
            _http = httpClient;
            _ownsHttp = false;
            if (_http != null)
                _http.Timeout = TimeSpan.FromSeconds(10);
        }

        public Uri ServiceUri => _baseUri;
        public Uri MatchSocketUri => MatchSocketUriFor(_baseUri);
        public RankedIdentity Identity => _identity;

        public async Task<RankedIdentity> EnsureIdentityAsync(
            string displayName,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (_identity != null) return _identity;
            IdentityRequest body = new IdentityRequest
            {
                name = NormalizeName(displayName)
            };
            RankedIdentity identity = await SendAsync<RankedIdentity>(
                HttpMethod.Post,
                "ranked/identity",
                body,
                null,
                cancellationToken);
            if (identity == null ||
                string.IsNullOrEmpty(identity.playerId) ||
                string.IsNullOrEmpty(identity.token))
            {
                throw new FormatException(
                    "Ranked identity response is incomplete.");
            }
            _identity = identity;
            _identityStore.Save(_identityScope, identity);
            return identity;
        }

        public async Task<RankedProfile> ProfileAsync(
            string playerId,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return await SendAsync<RankedProfile>(
                HttpMethod.Get,
                "ranked/profile/" +
                    Uri.EscapeDataString(playerId ?? string.Empty),
                null,
                null,
                cancellationToken);
        }

        public async Task<RankedLeaderboard> LeaderboardAsync(
            int limit,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return await SendAsync<RankedLeaderboard>(
                HttpMethod.Get,
                "ranked/leaderboard?limit=" +
                    Math.Max(1, Math.Min(100, limit)),
                null,
                null,
                cancellationToken);
        }

        public async Task<RankedQueueState> JoinAsync(
            string displayName,
            string vehicleSpecId,
            string[] equipment,
            string camoId,
            int teamSize,
            CancellationToken cancellationToken)
        {
            RankedIdentity identity = await EnsureIdentityAsync(
                displayName,
                cancellationToken);
            try
            {
                return await JoinCoreAsync(
                    identity,
                    vehicleSpecId,
                    equipment,
                    camoId,
                    teamSize,
                    cancellationToken);
            }
            catch (RankedServiceException error)
                when (error.StatusCode == 401)
            {
                ClearIdentity();
                identity = await EnsureIdentityAsync(
                    displayName,
                    cancellationToken);
                return await JoinCoreAsync(
                    identity,
                    vehicleSpecId,
                    equipment,
                    camoId,
                    teamSize,
                    cancellationToken);
            }
        }

        public Task<RankedQueueState> PollAsync(
            string ticketId,
            string ticketToken,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return SendAsync<RankedQueueState>(
                HttpMethod.Get,
                "ranked/queue/" + Uri.EscapeDataString(ticketId),
                null,
                ticketToken,
                cancellationToken);
        }

        public async Task CancelAsync(
            string ticketId,
            string ticketToken,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            await SendAsync<CancelResponse>(
                HttpMethod.Delete,
                "ranked/queue/" + Uri.EscapeDataString(ticketId),
                null,
                ticketToken,
                cancellationToken);
        }

        public void ClearIdentity()
        {
            _identity = null;
            _identityStore.Clear(_identityScope);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_ownsHttp) _http.Dispose();
        }

        public static Uri NormalizeServiceUri(string value)
        {
            Uri parsed;
            if (!Uri.TryCreate(value, UriKind.Absolute, out parsed) ||
                (parsed.Scheme != "http" &&
                 parsed.Scheme != "https") ||
                !string.IsNullOrEmpty(parsed.Query) ||
                !string.IsNullOrEmpty(parsed.Fragment))
            {
                throw new ArgumentException(
                    "Ranked service URL must use HTTP or HTTPS.",
                    nameof(value));
            }
            UriBuilder builder = new UriBuilder(parsed)
            {
                Path = parsed.AbsolutePath.TrimEnd('/') + "/",
                Query = string.Empty,
                Fragment = string.Empty
            };
            return builder.Uri;
        }

        public static Uri MatchSocketUriFor(Uri serviceUri)
        {
            UriBuilder builder = new UriBuilder(serviceUri)
            {
                Scheme = serviceUri.Scheme == "https" ? "wss" : "ws",
                Path = serviceUri.AbsolutePath.TrimEnd('/') + "/match",
                Query = string.Empty,
                Fragment = string.Empty
            };
            return builder.Uri;
        }

        private Task<RankedQueueState> JoinCoreAsync(
            RankedIdentity identity,
            string vehicleSpecId,
            string[] equipment,
            string camoId,
            int teamSize,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(vehicleSpecId))
                throw new ArgumentException(
                    "Ranked vehicle is required.",
                    nameof(vehicleSpecId));
            QueueRequest body = new QueueRequest
            {
                playerId = identity.playerId,
                specId = vehicleSpecId,
                equipment = equipment ?? Array.Empty<string>(),
                camo = string.IsNullOrEmpty(camoId)
                    ? "factory"
                    : camoId,
                teamSize = teamSize
            };
            return SendAsync<RankedQueueState>(
                HttpMethod.Post,
                "ranked/queue",
                body,
                identity.token,
                cancellationToken);
        }

        private async Task<T> SendAsync<T>(
            HttpMethod method,
            string path,
            object body,
            string bearer,
            CancellationToken cancellationToken)
            where T : class
        {
            if (_http == null)
            {
                return await SendUnityAsync<T>(
                    method,
                    path,
                    body,
                    bearer,
                    cancellationToken);
            }
            using (HttpRequestMessage request = new HttpRequestMessage(
                method,
                new Uri(_baseUri, path)))
            {
                if (body != null)
                {
                    request.Content = new StringContent(
                        JsonUtility.ToJson(body),
                        Encoding.UTF8,
                        "application/json");
                }
                if (!string.IsNullOrEmpty(bearer))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Authorization",
                        "Bearer " + bearer);
                }
                using (HttpResponseMessage response =
                    await _http.SendAsync(
                        request,
                        HttpCompletionOption.ResponseContentRead,
                        cancellationToken))
                {
                    string json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        ErrorResponse error = Parse<ErrorResponse>(json);
                        throw new RankedServiceException(
                            (int)response.StatusCode,
                            error?.error ?? "ranked_service_error",
                            error?.message);
                    }
                    T value = Parse<T>(json);
                    if (value == null)
                        throw new FormatException(
                            "Ranked service returned an invalid response.");
                    return value;
                }
            }
        }

        private static T Parse<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string NormalizeName(string value)
        {
            string clean = string.IsNullOrWhiteSpace(value)
                ? "Commander"
                : value.Trim();
            while (clean.Contains("  "))
                clean = clean.Replace("  ", " ");
            return clean.Length <= 24
                ? clean
                : clean.Substring(0, 24);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(RankedServiceClient));
        }

        [Serializable] private sealed class CancelResponse
        {
            public bool cancelled;
        }
    }
}
