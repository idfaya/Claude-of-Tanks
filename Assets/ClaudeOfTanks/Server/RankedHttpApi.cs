using System;
using System.Collections.Generic;
using System.Text;
using ClaudeOfTanks.Network;
using UnityEngine;

namespace ClaudeOfTanks.Server
{
    public sealed class RankedHttpApi : IDedicatedHttpHandler, IDisposable
    {
        private const string JsonContentType = "application/json; charset=utf-8";
        private const int MaximumTrackedClients = 4096;
        private const int MaximumRequestsPerMinute = 240;
        private readonly RankedRatingStore _ratings;
        private readonly RankedMatchmaker _matchmaker;
        private readonly DedicatedMatchRegistry _registry;
        private readonly Func<long> _clock;
        private readonly Dictionary<string, RateWindow> _rateWindows =
            new Dictionary<string, RateWindow>(StringComparer.Ordinal);
        private bool _disposed;

        public RankedHttpApi(
            RankedRatingStore ratings,
            RankedMatchmaker matchmaker,
            DedicatedMatchRegistry registry,
            Func<long> clock = null)
        {
            _ratings = ratings ?? throw new ArgumentNullException(nameof(ratings));
            _matchmaker = matchmaker ?? throw new ArgumentNullException(nameof(matchmaker));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _clock = clock ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public DedicatedHttpResponse Handle(DedicatedHttpRequest request)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RankedHttpApi));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!AdmitRequest(request.RemoteAddress))
                return Error(429, "rate_limited", null, request.Origin);
            try
            {
                Uri uri = new Uri("http://localhost" + request.Target);
                string path = uri.AbsolutePath;
                if (request.Method == "OPTIONS" &&
                    path.StartsWith("/ranked/", StringComparison.Ordinal))
                {
                    return Response(204, "No Content", null, request.Origin);
                }
                if (request.Method == "GET" && path == "/healthz")
                {
                    return Json(200, new HealthResponse
                    {
                        ok = true,
                        service = "cot-match",
                        matches = _registry.MatchCount,
                        connectedPlayers = _registry.ConnectedPlayerCount,
                        queuedPlayers = _matchmaker.QueuedPlayerCount,
                        ratedMatches = _matchmaker.RatedMatchCount
                    }, request.Origin);
                }
                if (request.Method == "POST" && path == "/ranked/identity")
                {
                    IdentityBody body = ReadJson<IdentityBody>(request.Body);
                    RatingIdentity identity = _ratings.CreateIdentity(body.name);
                    return Json(
                        201,
                        IdentityResponse.From(identity),
                        request.Origin);
                }
                if (request.Method == "GET" && path == "/ranked/leaderboard")
                {
                    int limit = QueryInt(uri.Query, "limit", 50);
                    LeaderboardEntry[] entries = _ratings.Leaderboard(limit);
                    ProfileResponse[] players = new ProfileResponse[entries.Length];
                    for (int i = 0; i < entries.Length; i++)
                        players[i] = ProfileResponse.From(
                            entries[i].Profile,
                            entries[i].Place);
                    return Json(
                        200,
                        new LeaderboardResponse { players = players },
                        request.Origin);
                }
                const string profilePrefix = "/ranked/profile/";
                if (request.Method == "GET" &&
                    path.StartsWith(profilePrefix, StringComparison.Ordinal))
                {
                    string playerId = DecodePath(path.Substring(profilePrefix.Length));
                    PublicRatingProfile profile = _ratings.GetProfile(playerId);
                    return profile != null
                        ? Json(200, ProfileResponse.From(profile), request.Origin)
                        : Error(404, "profile_not_found", null, request.Origin);
                }
                if (request.Method == "POST" && path == "/ranked/queue")
                {
                    QueueBody body = ReadJson<QueueBody>(request.Body);
                    RankedQueueJoin join = _matchmaker.Join(
                        body.playerId,
                        Bearer(request.Authorization),
                        body.specId,
                        body.equipment,
                        string.IsNullOrEmpty(body.camo) ? "factory" : body.camo,
                        body.teamSize == 0 ? 1 : body.teamSize,
                        _clock());
                    RankedQueueView view = _matchmaker.Poll(
                        join.QueueId,
                        join.QueueToken);
                    return Json(
                        202,
                        QueueResponse.From(view, join.QueueToken),
                        request.Origin);
                }
                const string queuePrefix = "/ranked/queue/";
                if (path.StartsWith(queuePrefix, StringComparison.Ordinal))
                {
                    string queueId = DecodePath(path.Substring(queuePrefix.Length));
                    string token = Bearer(request.Authorization);
                    if (request.Method == "GET")
                    {
                        RankedQueueView view = _matchmaker.Poll(queueId, token);
                        return view != null
                            ? Json(200, QueueResponse.From(view), request.Origin)
                            : Error(404, "ticket_not_found", null, request.Origin);
                    }
                    if (request.Method == "DELETE")
                    {
                        bool cancelled = _matchmaker.Cancel(
                            queueId,
                            token,
                            _clock());
                        return cancelled
                            ? Json(
                                200,
                                new CancelResponse { cancelled = true },
                                request.Origin)
                            : Error(
                                409,
                                "ticket_not_cancellable",
                                null,
                                request.Origin);
                    }
                    return Error(405, "method_not_allowed", null, request.Origin);
                }
                return Error(404, "not_found", null, request.Origin);
            }
            catch (UnauthorizedAccessException exception)
            {
                return Error(401, "ranked_auth_failed", exception.Message, request.Origin);
            }
            catch (ArgumentException exception)
            {
                return Error(400, "invalid_request", exception.Message, request.Origin);
            }
            catch (InvalidOperationException exception)
            {
                return Error(409, "request_conflict", exception.Message, request.Origin);
            }
            catch (FormatException exception)
            {
                return Error(400, "invalid_request", exception.Message, request.Origin);
            }
        }

        public void Pump(long nowMs)
        {
            if (_disposed) return;
            _matchmaker.Pump(nowMs);
            List<string> expired = null;
            foreach (KeyValuePair<string, RateWindow> pair in _rateWindows)
            {
                if (nowMs - pair.Value.StartedAtMs <= 120000) continue;
                if (expired == null) expired = new List<string>();
                expired.Add(pair.Key);
            }
            if (expired != null)
                for (int i = 0; i < expired.Count; i++) _rateWindows.Remove(expired[i]);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _matchmaker.Dispose();
            _ratings.Dispose();
            _rateWindows.Clear();
        }

        private bool AdmitRequest(string remoteAddress)
        {
            string key = string.IsNullOrEmpty(remoteAddress) ? "unknown" : remoteAddress;
            long nowMs = _clock();
            RateWindow window;
            if (!_rateWindows.TryGetValue(key, out window))
            {
                if (_rateWindows.Count >= MaximumTrackedClients) return false;
                window = new RateWindow { StartedAtMs = nowMs };
                _rateWindows.Add(key, window);
            }
            if (nowMs - window.StartedAtMs >= 60000)
            {
                window.StartedAtMs = nowMs;
                window.Count = 0;
            }
            if (window.Count >= MaximumRequestsPerMinute) return false;
            window.Count++;
            return true;
        }

        private static T ReadJson<T>(byte[] body) where T : class, new()
        {
            if (body == null || body.Length == 0) return new T();
            string json = new UTF8Encoding(false, true).GetString(body);
            if (!json.TrimStart().StartsWith("{", StringComparison.Ordinal))
                throw new FormatException("Request body must be a JSON object.");
            T value = JsonUtility.FromJson<T>(json);
            if (value == null) throw new FormatException("Request body must be a JSON object.");
            return value;
        }

        private static string Bearer(string authorization)
        {
            if (string.IsNullOrEmpty(authorization) ||
                !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
            return authorization.Substring(7).Trim();
        }

        private static string DecodePath(string value)
        {
            string decoded = Uri.UnescapeDataString(value ?? string.Empty);
            if (decoded.Length == 0 || decoded.IndexOf('/') >= 0)
                throw new ArgumentException("Resource id is invalid.");
            return decoded;
        }

        private static int QueryInt(string query, string key, int fallback)
        {
            string prefix = key + "=";
            string[] parts = (query ?? string.Empty).TrimStart('?').Split('&');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].StartsWith(prefix, StringComparison.Ordinal)) continue;
                int parsed;
                return int.TryParse(parts[i].Substring(prefix.Length), out parsed)
                    ? parsed
                    : fallback;
            }
            return fallback;
        }

        private static DedicatedHttpResponse Json(
            int status,
            object body,
            string origin)
        {
            return Response(
                status,
                Reason(status),
                Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)),
                origin);
        }

        private static string Reason(int status)
        {
            switch (status)
            {
                case 200: return "OK";
                case 201: return "Created";
                case 202: return "Accepted";
                case 400: return "Bad Request";
                case 401: return "Unauthorized";
                case 404: return "Not Found";
                case 405: return "Method Not Allowed";
                case 409: return "Conflict";
                case 429: return "Too Many Requests";
                default: return "Error";
            }
        }

        private static DedicatedHttpResponse Error(
            int status,
            string code,
            string message,
            string origin)
        {
            return Json(
                status,
                new ErrorResponse { error = code, message = message },
                origin);
        }

        private static DedicatedHttpResponse Response(
            int status,
            string reason,
            byte[] body,
            string origin)
        {
            Dictionary<string, string> headers = null;
            if (!string.IsNullOrEmpty(origin))
            {
                headers = new Dictionary<string, string>
                {
                    ["Access-Control-Allow-Origin"] = origin,
                    ["Access-Control-Allow-Headers"] = "authorization, content-type",
                    ["Access-Control-Allow-Methods"] = "GET, POST, DELETE, OPTIONS",
                    ["Vary"] = "Origin"
                };
            }
            return new DedicatedHttpResponse
            {
                Status = status,
                Reason = reason,
                ContentType = body == null ? null : JsonContentType,
                Body = body ?? Array.Empty<byte>(),
                Headers = headers
            };
        }

        [Serializable] private sealed class IdentityBody { public string name; }
        [Serializable] private sealed class QueueBody
        {
            public string playerId;
            public string specId;
            public string[] equipment;
            public string camo;
            public int teamSize;
        }
        [Serializable] private sealed class HealthResponse
        {
            public bool ok;
            public string service;
            public int matches;
            public int connectedPlayers;
            public int queuedPlayers;
            public int ratedMatches;
        }
        [Serializable] private sealed class ErrorResponse
        {
            public string error;
            public string message;
        }
        [Serializable] private sealed class CancelResponse { public bool cancelled; }
        [Serializable] private sealed class LeaderboardResponse
        {
            public ProfileResponse[] players;
        }
        [Serializable] private sealed class IdentityResponse : ProfileResponse
        {
            public string token;

            public static IdentityResponse From(RatingIdentity identity)
            {
                IdentityResponse result = new IdentityResponse
                {
                    token = identity.BearerToken
                };
                result.Copy(identity.Profile);
                return result;
            }
        }
        [Serializable] private class ProfileResponse
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

            public static ProfileResponse From(
                PublicRatingProfile profile,
                int place = 0)
            {
                ProfileResponse result = new ProfileResponse { place = place };
                result.Copy(profile);
                return result;
            }

            protected void Copy(PublicRatingProfile profile)
            {
                playerId = profile.PlayerId;
                name = profile.DisplayName;
                rating = profile.Rating;
                rank = profile.Rank.ToString();
                matches = profile.Matches;
                wins = profile.Wins;
                losses = profile.Losses;
                draws = profile.Draws;
                bestRating = profile.BestRating;
            }
        }
        [Serializable] private sealed class QueueResponse
        {
            public string ticketId;
            public string ticketToken;
            public string status;
            public long queuedAtMs;
            public int teamSize;
            public int rating;
            public MatchResponse match;
            public string result;
            public ProfileResponse profile;

            public static QueueResponse From(
                RankedQueueView view,
                string ticketToken = null)
            {
                return new QueueResponse
                {
                    ticketId = view.QueueId,
                    ticketToken = ticketToken,
                    status = view.Status.ToString().ToLowerInvariant(),
                    queuedAtMs = view.QueuedAtMs,
                    teamSize = view.TeamSize,
                    rating = view.Rating,
                    match = MatchResponse.From(view.Assignment),
                    result = view.Result.HasValue
                        ? view.Result.Value.ToString().ToLowerInvariant()
                        : null,
                    profile = view.Profile != null
                        ? ProfileResponse.From(view.Profile)
                        : null
                };
            }
        }
        [Serializable] private sealed class MatchResponse
        {
            public string matchId;
            public string playerId;
            public string token;
            public string mapId;
            public RosterResponse[] roster;

            public static MatchResponse From(RankedMatchAssignment assignment)
            {
                if (assignment == null) return null;
                RoomMatchSeat[] seats = assignment.Roster ?? Array.Empty<RoomMatchSeat>();
                RosterResponse[] roster = new RosterResponse[seats.Length];
                for (int i = 0; i < seats.Length; i++)
                    roster[i] = RosterResponse.From(seats[i]);
                return new MatchResponse
                {
                    matchId = assignment.MatchTicket.MatchId,
                    playerId = assignment.MatchTicket.PlayerId,
                    token = assignment.MatchTicket.TicketToken,
                    mapId = assignment.MapId,
                    roster = roster
                };
            }
        }
        [Serializable] private sealed class RosterResponse
        {
            public string id;
            public string name;
            public string specId;
            public string camo;
            public string team;
            public int rating;

            public static RosterResponse From(RoomMatchSeat seat)
            {
                return new RosterResponse
                {
                    id = seat.PlayerId,
                    name = seat.DisplayName,
                    specId = seat.VehicleSpecId,
                    camo = seat.CamoId,
                    team = seat.Team == ClaudeOfTanks.Simulation.Team.Alpha
                        ? "alpha"
                        : "bravo",
                    rating = seat.Rating ?? RankedRatingStore.StartingRating
                };
            }
        }
        private sealed class RateWindow
        {
            public long StartedAtMs;
            public int Count;
        }
    }
}
