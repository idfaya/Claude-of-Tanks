using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public enum RankedQueueStatus
    {
        Queued,
        Matched,
        Finished,
        Cancelled,
        Expired
    }

    public sealed class RankedQueueJoin
    {
        public string QueueId;
        public string QueueToken;
        public RankedQueueStatus Status;
    }

    public sealed class RankedMatchAssignment
    {
        public DedicatedMatchTicket MatchTicket;
        public RoomMatchPlan Plan;
    }

    public sealed class RankedQueueView
    {
        public string QueueId;
        public RankedQueueStatus Status;
        public long QueuedAtMs;
        public int TeamSize;
        public int Rating;
        public RankedMatchAssignment Assignment;
        public RatedResult? Result;
        public PublicRatingProfile Profile;
    }

    public sealed class RankedMatchmaker : IDisposable
    {
        public const int MaximumActivePlayers = 2048;
        public const int MaximumEntries = 4096;
        public const long QueueLifetimeMs = 600000;
        public const long MatchLifetimeMs = 1500000;
        public const long ResultLifetimeMs = 120000;
        private static readonly int[] TeamSizes = { 1, 2, 3, 5, 7 };
        private readonly Dictionary<string, QueueRecord> _entries =
            new Dictionary<string, QueueRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _activeByPlayer =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, MatchRecord> _matches =
            new Dictionary<string, MatchRecord>(StringComparer.Ordinal);
        private readonly RankedRatingStore _ratings;
        private readonly DedicatedMatchRegistry _registry;
        private readonly Func<RoomMatchPlan, AuthoritativeMatchHost> _hostFactory;
        private readonly Func<string, bool> _vehicleAllowed;
        private readonly Func<string, string, bool> _equipmentAllowed;
        private readonly string[] _mapRotation;
        private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        private readonly Func<string> _queueIdFactory;
        private readonly Func<string> _queueTokenFactory;
        private readonly Func<string> _matchIdFactory;
        private int _matchSequence;
        private bool _disposed;

        public RankedMatchmaker(
            RankedRatingStore ratings,
            DedicatedMatchRegistry registry,
            Func<RoomMatchPlan, AuthoritativeMatchHost> hostFactory,
            string[] mapRotation,
            Func<string, bool> vehicleAllowed,
            Func<string> queueIdFactory = null,
            Func<string> queueTokenFactory = null,
            Func<string, string, bool> equipmentAllowed = null,
            Func<string> matchIdFactory = null)
        {
            _ratings = ratings ?? throw new ArgumentNullException(nameof(ratings));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _hostFactory = hostFactory ?? throw new ArgumentNullException(nameof(hostFactory));
            _vehicleAllowed = vehicleAllowed ?? throw new ArgumentNullException(nameof(vehicleAllowed));
            _equipmentAllowed =
                equipmentAllowed ?? ((vehicleId, equipmentId) => true);
            if (mapRotation == null || mapRotation.Length == 0)
                throw new ArgumentException("Map rotation is required.", nameof(mapRotation));
            _mapRotation = (string[])mapRotation.Clone();
            for (int i = 0; i < _mapRotation.Length; i++)
                _mapRotation[i] = CleanContentId(_mapRotation[i], "map id");
            _queueIdFactory = queueIdFactory ?? (() => "q_" + Base64Url(RandomBytes(12)));
            _queueTokenFactory = queueTokenFactory ?? (() => Base64Url(RandomBytes(24)));
            _matchIdFactory =
                matchIdFactory ??
                (() => "ranked_" + Base64Url(RandomBytes(12)));
        }

        public int QueuedPlayerCount
        {
            get
            {
                int count = 0;
                foreach (QueueRecord entry in _entries.Values)
                    if (entry.Status == RankedQueueStatus.Queued) count++;
                return count;
            }
        }
        public int RatedMatchCount => _matches.Count;

        public RankedQueueJoin Join(
            string playerId,
            string identityToken,
            string vehicleSpecId,
            string[] equipment,
            string camoId,
            int teamSize,
            long nowMs)
        {
            ThrowIfDisposed();
            if (!_ratings.Authenticate(playerId, identityToken))
                throw new UnauthorizedAccessException("Ranked identity authentication failed.");
            if (_activeByPlayer.ContainsKey(playerId))
                throw new InvalidOperationException("Player is already queued or matched.");
            if (_activeByPlayer.Count >= MaximumActivePlayers || _entries.Count >= MaximumEntries)
                throw new InvalidOperationException("Ranked queue is at capacity.");
            if (Array.IndexOf(TeamSizes, teamSize) < 0)
                throw new ArgumentOutOfRangeException(nameof(teamSize));
            string vehicleId = CleanContentId(vehicleSpecId, "vehicle id");
            if (!_vehicleAllowed(vehicleId))
                throw new ArgumentException("Vehicle is unavailable in ranked play.", nameof(vehicleSpecId));
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            PublicRatingProfile profile = _ratings.GetProfile(playerId);
            string queueId = AllocateQueueId();
            string queueToken = StrongQueueToken();
            QueueRecord entry = new QueueRecord
            {
                QueueId = queueId,
                TokenHash = Hash(queueToken),
                PlayerId = playerId,
                DisplayName = profile.DisplayName,
                Rating = profile.Rating,
                VehicleSpecId = vehicleId,
                Equipment = SanitizeEquipment(vehicleId, equipment),
                CamoId = CleanContentId(
                    string.IsNullOrEmpty(camoId) ? "factory" : camoId,
                    "camo id"),
                TeamSize = teamSize,
                QueuedAtMs = nowMs,
                Status = RankedQueueStatus.Queued
            };
            _entries.Add(queueId, entry);
            _activeByPlayer.Add(playerId, queueId);
            Pump(nowMs);
            return new RankedQueueJoin
            {
                QueueId = queueId,
                QueueToken = queueToken,
                Status = entry.Status
            };
        }

        public RankedQueueView Poll(string queueId, string queueToken)
        {
            ThrowIfDisposed();
            QueueRecord entry;
            if (!_entries.TryGetValue(queueId ?? string.Empty, out entry) ||
                !Matches(entry.TokenHash, queueToken))
                return null;
            return View(entry);
        }

        public bool Cancel(string queueId, string queueToken, long nowMs)
        {
            ThrowIfDisposed();
            QueueRecord entry;
            if (!_entries.TryGetValue(queueId ?? string.Empty, out entry) ||
                !Matches(entry.TokenHash, queueToken) ||
                entry.Status != RankedQueueStatus.Queued)
                return false;
            entry.Status = RankedQueueStatus.Cancelled;
            entry.CompletedAtMs = nowMs;
            _activeByPlayer.Remove(entry.PlayerId);
            return true;
        }

        public void Pump(long nowMs)
        {
            ThrowIfDisposed();
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            List<string> remove = new List<string>();
            foreach (QueueRecord entry in _entries.Values)
            {
                if (entry.Status == RankedQueueStatus.Queued &&
                    nowMs - entry.QueuedAtMs > QueueLifetimeMs)
                {
                    entry.Status = RankedQueueStatus.Expired;
                    entry.CompletedAtMs = nowMs;
                    _activeByPlayer.Remove(entry.PlayerId);
                }
                else if (entry.CompletedAtMs.HasValue &&
                    nowMs - entry.CompletedAtMs.Value > ResultLifetimeMs)
                    remove.Add(entry.QueueId);
            }
            for (int i = 0; i < remove.Count; i++) _entries.Remove(remove[i]);
            List<string> finishedMatches = new List<string>();
            foreach (KeyValuePair<string, MatchRecord> pair in _matches)
            {
                MatchRecord match = pair.Value;
                if (!match.Settled && nowMs - match.CreatedAtMs > MatchLifetimeMs)
                {
                    for (int i = 0; i < match.Entries.Count; i++)
                    {
                        QueueRecord entry = match.Entries[i];
                        if (entry.Status != RankedQueueStatus.Matched) continue;
                        entry.Status = RankedQueueStatus.Expired;
                        entry.CompletedAtMs = nowMs;
                        _activeByPlayer.Remove(entry.PlayerId);
                    }
                    match.Settled = true;
                    _registry.Remove(pair.Key);
                }
                bool hasTicket = false;
                for (int i = 0; i < match.Entries.Count; i++)
                    if (_entries.ContainsKey(match.Entries[i].QueueId)) hasTicket = true;
                if (match.Settled && !hasTicket) finishedMatches.Add(pair.Key);
            }
            for (int i = 0; i < finishedMatches.Count; i++) _matches.Remove(finishedMatches[i]);
            List<KeyValuePair<string, RatedResult>> results =
                new List<KeyValuePair<string, RatedResult>>();
            foreach (KeyValuePair<string, MatchRecord> pair in _matches)
            {
                if (pair.Value.Settled) continue;
                DedicatedMatchRecord record = _registry.Get(pair.Key);
                if (record == null) continue;
                if (record.Host.Draw)
                    results.Add(new KeyValuePair<string, RatedResult>(
                        pair.Key,
                        RatedResult.Draw));
                else if (record.Host.Winner.HasValue)
                    results.Add(new KeyValuePair<string, RatedResult>(
                        pair.Key,
                        record.Host.Winner.Value == Team.Alpha
                            ? RatedResult.Alpha
                            : RatedResult.Bravo));
            }
            for (int i = 0; i < results.Count; i++)
                Finish(results[i].Key, results[i].Value, nowMs);
            for (int i = 0; i < TeamSizes.Length; i++) MatchSize(TeamSizes[i], nowMs);
        }

        public RatingUpdate[] Finish(string matchId, RatedResult result, long nowMs)
        {
            ThrowIfDisposed();
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            MatchRecord match;
            if (!_matches.TryGetValue(matchId ?? string.Empty, out match))
                throw new KeyNotFoundException("Ranked match does not exist.");
            if (match.Settled) return null;
            RatingUpdate[] updates = _ratings.Settle(matchId, result, match.RatedPlayers);
            Dictionary<string, PublicRatingProfile> byPlayer =
                new Dictionary<string, PublicRatingProfile>(StringComparer.Ordinal);
            if (updates != null)
                for (int i = 0; i < updates.Length; i++)
                    byPlayer[updates[i].Profile.PlayerId] = updates[i].Profile;
            for (int i = 0; i < match.Entries.Count; i++)
            {
                QueueRecord entry = match.Entries[i];
                entry.Status = RankedQueueStatus.Finished;
                entry.Result = result;
                entry.Profile = byPlayer.ContainsKey(entry.PlayerId)
                    ? byPlayer[entry.PlayerId]
                    : _ratings.GetProfile(entry.PlayerId);
                entry.CompletedAtMs = nowMs;
                _activeByPlayer.Remove(entry.PlayerId);
            }
            match.Settled = true;
            _registry.Finish(matchId, result, nowMs);
            return updates;
        }

        public static int SearchBand(long waitedMs)
        {
            if (waitedMs < 0) waitedMs = 0;
            long minutes = waitedMs / 60000;
            if (minutes >= 9) return 600;
            return 150 + (int)minutes * 50;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _random.Dispose();
            _entries.Clear();
            _activeByPlayer.Clear();
            _matches.Clear();
        }

        private void MatchSize(int teamSize, long nowMs)
        {
            List<QueueRecord> queued = new List<QueueRecord>();
            foreach (QueueRecord entry in _entries.Values)
                if (entry.Status == RankedQueueStatus.Queued && entry.TeamSize == teamSize)
                    queued.Add(entry);
            queued.Sort((a, b) =>
            {
                int time = a.QueuedAtMs.CompareTo(b.QueuedAtMs);
                return time != 0 ? time : string.CompareOrdinal(a.PlayerId, b.PlayerId);
            });
            int required = teamSize * 2;
            while (queued.Count >= required)
            {
                QueueRecord oldest = queued[0];
                int band = SearchBand(nowMs - oldest.QueuedAtMs);
                List<QueueRecord> candidates = queued.FindAll(
                    entry => Math.Abs(entry.Rating - oldest.Rating) <= band);
                if (candidates.Count < required) return;
                List<QueueRecord> group = candidates.GetRange(0, required);
                CreateMatch(group, teamSize, nowMs);
                for (int i = 0; i < group.Count; i++) queued.Remove(group[i]);
            }
        }

        private void CreateMatch(List<QueueRecord> group, int teamSize, long nowMs)
        {
            group.Sort((a, b) =>
            {
                int rating = b.Rating.CompareTo(a.Rating);
                return rating != 0 ? rating : string.CompareOrdinal(a.PlayerId, b.PlayerId);
            });
            List<QueueRecord> alpha = new List<QueueRecord>();
            List<QueueRecord> bravo = new List<QueueRecord>();
            int alphaRating = 0;
            int bravoRating = 0;
            for (int i = 0; i < group.Count; i++)
            {
                QueueRecord entry = group[i];
                bool useAlpha = alpha.Count < teamSize &&
                    (bravo.Count >= teamSize || alphaRating <= bravoRating);
                (useAlpha ? alpha : bravo).Add(entry);
                if (useAlpha) alphaRating += entry.Rating;
                else bravoRating += entry.Rating;
            }

            int sequence = ++_matchSequence;
            string matchId = AllocateMatchId();
            RoomMatchSeat[] seats = new RoomMatchSeat[group.Count];
            List<RatedPlayer> rated = new List<RatedPlayer>(group.Count);
            List<string> names = new List<string>(group.Count);
            int cursor = 0;
            AddSeats(alpha, RoomTeam.Alpha, matchId, seats, rated, names, ref cursor);
            AddSeats(bravo, RoomTeam.Bravo, matchId, seats, rated, names, ref cursor);
            RoomMatchPlan plan = new RoomMatchPlan
            {
                Round = 1,
                Seed = unchecked(0x6d2b79f5u ^ (uint)(sequence * -1640531527)),
                MapId = _mapRotation[(sequence - 1) % _mapRotation.Length],
                GameMode = GameModeId.Standard,
                TeamSize = teamSize,
                Seats = seats,
                SpectatorPlayerIds = Array.Empty<string>()
            };
            DedicatedMatchTicket[] tickets = _registry.CreateMatch(
                plan, _hostFactory(plan), nowMs, matchId);
            Dictionary<string, DedicatedMatchTicket> byPlayer =
                new Dictionary<string, DedicatedMatchTicket>(StringComparer.Ordinal);
            for (int i = 0; i < tickets.Length; i++) byPlayer.Add(tickets[i].PlayerId, tickets[i]);
            for (int i = 0; i < group.Count; i++)
            {
                QueueRecord entry = group[i];
                entry.Status = RankedQueueStatus.Matched;
                entry.Assignment = new RankedMatchAssignment
                {
                    MatchTicket = byPlayer[entry.PlayerId],
                    Plan = ClonePlan(plan)
                };
            }
            _matches.Add(matchId, new MatchRecord
            {
                Entries = group,
                RatedPlayers = rated.ToArray(),
                CreatedAtMs = nowMs
            });
        }

        private static void AddSeats(
            List<QueueRecord> source,
            RoomTeam team,
            string matchId,
            RoomMatchSeat[] destination,
            List<RatedPlayer> rated,
            List<string> names,
            ref int cursor)
        {
            for (int i = 0; i < source.Count; i++)
            {
                QueueRecord entry = source[i];
                string displayName = UniqueName(entry.DisplayName, names);
                names.Add(displayName);
                destination[cursor++] = new RoomMatchSeat
                {
                    PlayerId = entry.PlayerId,
                    EntityId = matchId + "-entity-" + cursor,
                    DisplayName = displayName,
                    Team = team == RoomTeam.Alpha ? Team.Alpha : Team.Bravo,
                    VehicleSpecId = entry.VehicleSpecId,
                    Equipment = (string[])entry.Equipment.Clone(),
                    CamoId = entry.CamoId,
                    Rating = entry.Rating
                };
                rated.Add(new RatedPlayer { PlayerId = entry.PlayerId, Team = team });
            }
        }

        private static RoomMatchPlan ClonePlan(RoomMatchPlan source)
        {
            RoomMatchSeat[] seats = new RoomMatchSeat[source.Seats.Length];
            for (int i = 0; i < seats.Length; i++)
            {
                RoomMatchSeat seat = source.Seats[i];
                seats[i] = new RoomMatchSeat
                {
                    PlayerId = seat.PlayerId,
                    EntityId = seat.EntityId,
                    DisplayName = seat.DisplayName,
                    Team = seat.Team,
                    VehicleSpecId = seat.VehicleSpecId,
                    Equipment = seat.Equipment == null
                        ? Array.Empty<string>()
                        : (string[])seat.Equipment.Clone(),
                    CamoId = seat.CamoId,
                    Rating = seat.Rating
                };
            }
            return new RoomMatchPlan
            {
                Round = source.Round,
                Seed = source.Seed,
                MapId = source.MapId,
                GameMode = source.GameMode,
                TeamSize = source.TeamSize,
                Seats = seats,
                SpectatorPlayerIds = source.SpectatorPlayerIds == null
                    ? Array.Empty<string>()
                    : (string[])source.SpectatorPlayerIds.Clone()
            };
        }

        private RankedQueueView View(QueueRecord entry)
        {
            return new RankedQueueView
            {
                QueueId = entry.QueueId,
                Status = entry.Status,
                QueuedAtMs = entry.QueuedAtMs,
                TeamSize = entry.TeamSize,
                Rating = entry.Rating,
                Assignment = entry.Assignment,
                Result = entry.Result,
                Profile = entry.Profile
            };
        }

        private string AllocateQueueId()
        {
            for (int i = 0; i < 64; i++)
            {
                string id = _queueIdFactory();
                if (IsSafeId(id, 8, 64) &&
                    !_entries.ContainsKey(id))
                    return id;
            }
            throw new InvalidOperationException("Could not allocate a queue id.");
        }

        private string AllocateMatchId()
        {
            for (int attempt = 0; attempt < 64; attempt++)
            {
                string id = _matchIdFactory();
                if (!IsSafeId(id, 8, 64)) continue;
                if (!_matches.ContainsKey(id) &&
                    _registry.Get(id) == null)
                {
                    return id;
                }
            }
            throw new InvalidOperationException(
                "Could not allocate a unique ranked match id.");
        }

        private string StrongQueueToken()
        {
            string token = _queueTokenFactory();
            if (string.IsNullOrEmpty(token) || token.Length < 24)
                throw new InvalidOperationException("Queue token is weak.");
            return token;
        }

        private string[] SanitizeEquipment(
            string vehicleSpecId,
            string[] equipment)
        {
            if (equipment == null) return Array.Empty<string>();
            List<string> clean =
                new List<string>(LoadoutSimulation.EquipmentSlots);
            for (int i = 0;
                i < equipment.Length &&
                clean.Count < LoadoutSimulation.EquipmentSlots;
                i++)
            {
                string id;
                try { id = CleanContentId(equipment[i], "equipment id"); }
                catch (ArgumentException) { continue; }
                if (!LoadoutSimulation.IsEquipment(id) ||
                    clean.Contains(id) ||
                    !_equipmentAllowed(vehicleSpecId, id))
                {
                    continue;
                }
                clean.Add(id);
            }
            return clean.ToArray();
        }

        private static string UniqueName(string requested, List<string> used)
        {
            string root = string.IsNullOrWhiteSpace(requested) ? "Commander" : requested.Trim();
            if (root.Length > 24) root = root.Substring(0, 24);
            string candidate = root;
            int suffix = 2;
            while (used.Exists(name =>
                string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                string marker = " (" + suffix++ + ")";
                candidate = root.Substring(0, Math.Min(root.Length, 24 - marker.Length)) + marker;
            }
            return candidate;
        }

        private static string CleanContentId(string value, string label)
        {
            string id = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (id.Length < 1 || id.Length > 64)
                throw new ArgumentException(label + " is invalid.");
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (!(c >= 'a' && c <= 'z') &&
                    !(c >= '0' && c <= '9') && c != '_' && c != '-')
                    throw new ArgumentException(label + " is invalid.");
            }
            return id;
        }

        private static bool IsSafeId(string value, int minimum, int maximum)
        {
            if (string.IsNullOrEmpty(value) ||
                value.Length < minimum || value.Length > maximum) return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!(c >= 'a' && c <= 'z') &&
                    !(c >= 'A' && c <= 'Z') &&
                    !(c >= '0' && c <= '9') && c != '_' && c != '-')
                    return false;
            }
            return true;
        }

        private byte[] RandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            _random.GetBytes(bytes);
            return bytes;
        }

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static bool Matches(byte[] expected, string token)
        {
            byte[] actual = Hash(token);
            int difference = expected.Length ^ actual.Length;
            for (int i = 0; i < Math.Min(expected.Length, actual.Length); i++)
                difference |= expected[i] ^ actual[i];
            return difference == 0;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RankedMatchmaker));
        }

        private sealed class QueueRecord
        {
            public string QueueId;
            public byte[] TokenHash;
            public string PlayerId;
            public string DisplayName;
            public int Rating;
            public string VehicleSpecId;
            public string[] Equipment;
            public string CamoId;
            public int TeamSize;
            public long QueuedAtMs;
            public RankedQueueStatus Status;
            public RankedMatchAssignment Assignment;
            public RatedResult? Result;
            public PublicRatingProfile Profile;
            public long? CompletedAtMs;
        }

        private sealed class MatchRecord
        {
            public List<QueueRecord> Entries;
            public RatedPlayer[] RatedPlayers;
            public long CreatedAtMs;
            public bool Settled;
        }
    }
}
