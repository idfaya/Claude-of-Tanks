using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ClaudeOfTanks.Network
{
    public sealed class DedicatedMatchTicket
    {
        public string MatchId;
        public string PlayerId;
        public string TicketToken;
        public long ExpiresAtMs;
    }

    public sealed class DedicatedMatchAdmission
    {
        public string MatchId;
        public string PlayerId;
        public string EntityId;
        public string SessionToken;
        public int ConnectionGeneration;
    }

    public sealed class DedicatedMatchRecord
    {
        internal readonly Dictionary<string, DedicatedPlayerRecord> Players =
            new Dictionary<string, DedicatedPlayerRecord>(StringComparer.Ordinal);

        public string MatchId { get; internal set; }
        public string MapId { get; internal set; }
        public int Round { get; internal set; }
        public uint Seed { get; internal set; }
        public AuthoritativeMatchHost Host { get; internal set; }
        public long CreatedAtMs { get; internal set; }
        public long? FinishedAtMs { get; internal set; }
        public RatedResult? Result { get; internal set; }
        public int PlayerCount => Players.Count;

        public int ConnectedPlayerCount
        {
            get
            {
                int count = 0;
                foreach (DedicatedPlayerRecord player in Players.Values)
                    if (player.Connected) count++;
                return count;
            }
        }
    }

    internal sealed class DedicatedPlayerRecord
    {
        public string PlayerId;
        public string EntityId;
        public byte[] TicketHash;
        public long TicketExpiresAtMs;
        public bool TicketConsumed;
        public byte[] SessionHash;
        public bool Connected;
        public int ConnectionGeneration;
    }

    public sealed class DedicatedMatchRegistry : IDisposable
    {
        public const long DefaultTicketLifetimeMs = 120000;
        public const long DefaultFinishedRetentionMs = 30000;
        public const int MaximumMatches = 1024;
        private readonly Dictionary<string, DedicatedMatchRecord> _matches =
            new Dictionary<string, DedicatedMatchRecord>(StringComparer.Ordinal);
        private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        private readonly Func<string> _matchIdFactory;
        private readonly Func<string> _tokenFactory;
        private bool _disposed;

        public DedicatedMatchRegistry(
            Func<string> matchIdFactory = null,
            Func<string> tokenFactory = null)
        {
            _matchIdFactory = matchIdFactory ?? (() => Base64Url(RandomBytes(12)));
            _tokenFactory = tokenFactory ?? (() => Base64Url(RandomBytes(24)));
        }

        public int MatchCount => _matches.Count;

        public DedicatedMatchTicket[] CreateMatch(
            RoomMatchPlan plan,
            AuthoritativeMatchHost host,
            long nowMs,
            string matchId = null,
            long ticketLifetimeMs = DefaultTicketLifetimeMs)
        {
            ThrowIfDisposed();
            if (plan == null || plan.Seats == null || plan.Seats.Length < 2 ||
                plan.Seats.Length > AuthoritativeRoom.MaximumPlayers)
                throw new ArgumentException("Dedicated match requires 2-14 seats.", nameof(plan));
            if (host == null) throw new ArgumentNullException(nameof(host));
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            if (ticketLifetimeMs < 1) throw new ArgumentOutOfRangeException(nameof(ticketLifetimeMs));
            if (_matches.Count >= MaximumMatches)
                throw new InvalidOperationException("Dedicated match capacity is exhausted.");
            string id = matchId ?? AllocateMatchId();
            ValidateSafeId(id, 6, 64, "match id");
            if (_matches.ContainsKey(id))
                throw new InvalidOperationException("Dedicated match id already exists.");

            DedicatedMatchRecord record = new DedicatedMatchRecord
            {
                MatchId = id,
                MapId = plan.MapId,
                Round = plan.Round,
                Seed = plan.Seed,
                Host = host,
                CreatedAtMs = nowMs
            };
            DedicatedMatchTicket[] tickets = new DedicatedMatchTicket[plan.Seats.Length];
            long expiresAt = checked(nowMs + ticketLifetimeMs);
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                RoomMatchSeat seat = plan.Seats[i];
                if (seat == null) throw new ArgumentException("Match seat is missing.", nameof(plan));
                ValidateSafeId(seat.PlayerId, 1, 48, "player id");
                ValidateSafeId(seat.EntityId, 1, 64, "entity id");
                if (record.Players.ContainsKey(seat.PlayerId))
                    throw new ArgumentException("Match player ids must be unique.", nameof(plan));
                foreach (DedicatedPlayerRecord existing in record.Players.Values)
                    if (existing.EntityId == seat.EntityId)
                        throw new ArgumentException("Match entity ids must be unique.", nameof(plan));
                string token = StrongToken();
                record.Players.Add(seat.PlayerId, new DedicatedPlayerRecord
                {
                    PlayerId = seat.PlayerId,
                    EntityId = seat.EntityId,
                    TicketHash = Hash(token),
                    TicketExpiresAtMs = expiresAt
                });
                tickets[i] = new DedicatedMatchTicket
                {
                    MatchId = id,
                    PlayerId = seat.PlayerId,
                    TicketToken = token,
                    ExpiresAtMs = expiresAt
                };
            }
            _matches.Add(id, record);
            return tickets;
        }

        public DedicatedMatchAdmission Admit(
            string matchId,
            string playerId,
            string ticketToken,
            long nowMs)
        {
            ThrowIfDisposed();
            DedicatedPlayerRecord player = RequirePlayer(matchId, playerId);
            if (player.TicketConsumed ||
                nowMs < 0 ||
                nowMs > player.TicketExpiresAtMs ||
                !Matches(player.TicketHash, ticketToken))
            {
                throw new UnauthorizedAccessException("Match ticket is invalid, expired, or consumed.");
            }
            player.TicketConsumed = true;
            player.TicketHash = null;
            return Connect(player, matchId);
        }

        public DedicatedMatchAdmission Reconnect(
            string matchId,
            string playerId,
            string sessionToken)
        {
            ThrowIfDisposed();
            DedicatedPlayerRecord player = RequirePlayer(matchId, playerId);
            if (player.SessionHash == null || !Matches(player.SessionHash, sessionToken))
                throw new UnauthorizedAccessException("Match session is invalid.");
            return Connect(player, matchId);
        }

        public bool Disconnect(string matchId, string playerId, int connectionGeneration)
        {
            ThrowIfDisposed();
            DedicatedPlayerRecord player = RequirePlayer(matchId, playerId);
            if (player.ConnectionGeneration != connectionGeneration) return false;
            player.Connected = false;
            return true;
        }

        public int Advance(string matchId, int requestedTicks)
        {
            ThrowIfDisposed();
            DedicatedMatchRecord match = RequireMatch(matchId);
            return match.FinishedAtMs.HasValue ? 0 : match.Host.AdvanceTicks(requestedTicks);
        }

        public void Finish(string matchId, RatedResult result, long nowMs)
        {
            ThrowIfDisposed();
            if (!Enum.IsDefined(typeof(RatedResult), result))
                throw new ArgumentOutOfRangeException(nameof(result));
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            DedicatedMatchRecord match = RequireMatch(matchId);
            if (match.FinishedAtMs.HasValue) return;
            match.Result = result;
            match.FinishedAtMs = nowMs;
        }

        public DedicatedMatchRecord Get(string matchId)
        {
            ThrowIfDisposed();
            DedicatedMatchRecord match;
            return _matches.TryGetValue(matchId ?? string.Empty, out match) ? match : null;
        }

        public int Sweep(long nowMs, long retentionMs = DefaultFinishedRetentionMs)
        {
            ThrowIfDisposed();
            if (nowMs < 0 || retentionMs < 0) throw new ArgumentOutOfRangeException();
            List<string> expired = new List<string>();
            foreach (DedicatedMatchRecord match in _matches.Values)
                if (match.FinishedAtMs.HasValue &&
                    nowMs - match.FinishedAtMs.Value > retentionMs)
                    expired.Add(match.MatchId);
            for (int i = 0; i < expired.Count; i++) _matches.Remove(expired[i]);
            return expired.Count;
        }

        public bool Remove(string matchId)
        {
            ThrowIfDisposed();
            return _matches.Remove(matchId ?? string.Empty);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _matches.Clear();
            _random.Dispose();
        }

        private DedicatedMatchAdmission Connect(
            DedicatedPlayerRecord player,
            string matchId)
        {
            string session = StrongToken();
            player.SessionHash = Hash(session);
            player.Connected = true;
            player.ConnectionGeneration++;
            return new DedicatedMatchAdmission
            {
                MatchId = matchId,
                PlayerId = player.PlayerId,
                EntityId = player.EntityId,
                SessionToken = session,
                ConnectionGeneration = player.ConnectionGeneration
            };
        }

        private DedicatedPlayerRecord RequirePlayer(string matchId, string playerId)
        {
            DedicatedMatchRecord match = RequireMatch(matchId);
            DedicatedPlayerRecord player;
            if (!match.Players.TryGetValue(playerId ?? string.Empty, out player))
                throw new KeyNotFoundException("Player is not ticketed for this match.");
            return player;
        }

        private DedicatedMatchRecord RequireMatch(string matchId)
        {
            DedicatedMatchRecord match;
            if (!_matches.TryGetValue(matchId ?? string.Empty, out match))
                throw new KeyNotFoundException("Dedicated match does not exist.");
            return match;
        }

        private string AllocateMatchId()
        {
            for (int i = 0; i < 64; i++)
            {
                string id = _matchIdFactory();
                ValidateSafeId(id, 6, 64, "match id");
                if (!_matches.ContainsKey(id)) return id;
            }
            throw new InvalidOperationException("Could not allocate a dedicated match id.");
        }

        private string StrongToken()
        {
            string token = _tokenFactory();
            if (string.IsNullOrEmpty(token) || token.Length < 24)
                throw new InvalidOperationException("Token factory returned a weak token.");
            return token;
        }

        private byte[] RandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            _random.GetBytes(bytes);
            return bytes;
        }

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static bool Matches(byte[] expected, string token)
        {
            if (expected == null) return false;
            byte[] actual = Hash(token);
            int difference = expected.Length ^ actual.Length;
            int count = Math.Min(expected.Length, actual.Length);
            for (int i = 0; i < count; i++) difference |= expected[i] ^ actual[i];
            return difference == 0;
        }

        private static void ValidateSafeId(
            string value,
            int minimumLength,
            int maximumLength,
            string label)
        {
            if (string.IsNullOrEmpty(value) ||
                value.Length < minimumLength ||
                value.Length > maximumLength)
                throw new ArgumentException(label + " is invalid.");
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!(c >= 'a' && c <= 'z') &&
                    !(c >= 'A' && c <= 'Z') &&
                    !(c >= '0' && c <= '9') && c != '_' && c != '-')
                    throw new ArgumentException(label + " is invalid.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DedicatedMatchRegistry));
        }
    }
}
