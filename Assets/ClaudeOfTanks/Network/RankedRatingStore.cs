using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClaudeOfTanks.Network
{
    public enum RatingRank
    {
        Recruit,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master
    }

    public enum RatedResult
    {
        Alpha,
        Bravo,
        Draw
    }

    public sealed class PublicRatingProfile
    {
        public string PlayerId;
        public string DisplayName;
        public int Rating;
        public int BestRating;
        public int Matches;
        public int Wins;
        public int Losses;
        public int Draws;
        public RatingRank Rank;
    }

    public sealed class RatingIdentity
    {
        public PublicRatingProfile Profile;
        public string BearerToken;
    }

    public sealed class RatedPlayer
    {
        public string PlayerId;
        public RoomTeam Team;
    }

    public sealed class RatingUpdate
    {
        public PublicRatingProfile Profile;
        public int Before;
        public int Delta;
    }

    public sealed class LeaderboardEntry
    {
        public int Place;
        public PublicRatingProfile Profile;
    }

    public sealed class RankedRatingStore : IDisposable
    {
        public const int StartingRating = 1000;
        public const int MinimumRating = 100;
        public const int MaximumRating = 3000;
        public const int MaximumProfiles = 100000;
        public const int MaximumSettledMatches = 10000;
        private const uint FileMagic = 0x52544f43u;
        private const ushort FileVersion = 1;
        private readonly Dictionary<string, ProfileRecord> _profiles =
            new Dictionary<string, ProfileRecord>(StringComparer.Ordinal);
        private readonly HashSet<string> _settledMatches =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _settledOrder = new Queue<string>();
        private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        private readonly string _filePath;
        private readonly Func<string> _identityFactory;
        private readonly Func<string> _tokenFactory;
        private bool _disposed;

        public RankedRatingStore(
            string filePath = null,
            Func<string> identityFactory = null,
            Func<string> tokenFactory = null)
        {
            _filePath = string.IsNullOrWhiteSpace(filePath) ? null : Path.GetFullPath(filePath);
            _identityFactory = identityFactory ?? CreatePlayerId;
            _tokenFactory = tokenFactory ?? CreateToken;
            Load();
        }

        public int ProfileCount => _profiles.Count;
        public int SettledMatchCount => _settledMatches.Count;

        public RatingIdentity CreateIdentity(string displayName)
        {
            ThrowIfDisposed();
            if (_profiles.Count >= MaximumProfiles)
                throw new InvalidOperationException("Rating profile capacity is exhausted.");
            string playerId;
            int attempts = 0;
            do
            {
                playerId = _identityFactory();
                if (++attempts > 64)
                    throw new InvalidOperationException("Could not allocate a unique player id.");
            }
            while (_profiles.ContainsKey(playerId));
            ValidatePlayerId(playerId);
            string token = _tokenFactory();
            if (string.IsNullOrEmpty(token) || token.Length < 24)
                throw new InvalidOperationException("Token factory returned a weak token.");

            ProfileRecord profile = new ProfileRecord
            {
                PlayerId = playerId,
                DisplayName = CleanName(displayName),
                TokenHash = Hash(token),
                Rating = StartingRating,
                BestRating = StartingRating
            };
            _profiles.Add(playerId, profile);
            Save();
            return new RatingIdentity
            {
                Profile = Public(profile),
                BearerToken = token
            };
        }

        public bool Authenticate(string playerId, string bearerToken)
        {
            ThrowIfDisposed();
            ProfileRecord profile;
            return _profiles.TryGetValue(playerId ?? string.Empty, out profile) &&
                FixedTimeEquals(profile.TokenHash, Hash(bearerToken ?? string.Empty));
        }

        public PublicRatingProfile GetProfile(string playerId)
        {
            ThrowIfDisposed();
            ProfileRecord profile;
            return _profiles.TryGetValue(playerId ?? string.Empty, out profile)
                ? Public(profile)
                : null;
        }

        public PublicRatingProfile Rename(
            string playerId,
            string bearerToken,
            string displayName)
        {
            ThrowIfDisposed();
            if (!Authenticate(playerId, bearerToken)) return null;
            ProfileRecord profile = _profiles[playerId];
            profile.DisplayName = CleanName(displayName);
            Save();
            return Public(profile);
        }

        public LeaderboardEntry[] Leaderboard(int limit = 50)
        {
            ThrowIfDisposed();
            int count = Math.Max(1, Math.Min(100, limit));
            List<ProfileRecord> profiles = new List<ProfileRecord>(_profiles.Values);
            profiles.Sort((a, b) =>
            {
                int rating = b.Rating.CompareTo(a.Rating);
                if (rating != 0) return rating;
                int matches = b.Matches.CompareTo(a.Matches);
                return matches != 0
                    ? matches
                    : string.CompareOrdinal(a.PlayerId, b.PlayerId);
            });
            count = Math.Min(count, profiles.Count);
            LeaderboardEntry[] result = new LeaderboardEntry[count];
            for (int i = 0; i < count; i++)
                result[i] = new LeaderboardEntry { Place = i + 1, Profile = Public(profiles[i]) };
            return result;
        }

        public RatingUpdate[] Settle(
            string matchId,
            RatedResult result,
            IReadOnlyList<RatedPlayer> players)
        {
            ThrowIfDisposed();
            ValidateMatchId(matchId);
            if (_settledMatches.Contains(matchId)) return null;
            if (!Enum.IsDefined(typeof(RatedResult), result))
                throw new ArgumentOutOfRangeException(nameof(result));
            if (players == null || players.Count < 2 || players.Count > 14)
                throw new ArgumentException("Rated settlement requires 2-14 players.", nameof(players));

            List<ProfileRecord> alpha = new List<ProfileRecord>();
            List<ProfileRecord> bravo = new List<ProfileRecord>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < players.Count; i++)
            {
                RatedPlayer player = players[i];
                ProfileRecord profile;
                if (player == null ||
                    !seen.Add(player.PlayerId) ||
                    !_profiles.TryGetValue(player.PlayerId ?? string.Empty, out profile) ||
                    (player.Team != RoomTeam.Alpha && player.Team != RoomTeam.Bravo))
                {
                    throw new ArgumentException("Rated settlement contains an invalid player.", nameof(players));
                }
                (player.Team == RoomTeam.Alpha ? alpha : bravo).Add(profile);
            }
            if (alpha.Count == 0 || bravo.Count == 0)
                throw new ArgumentException("Rated settlement requires both teams.", nameof(players));

            double alphaExpected = Expected(Average(alpha), Average(bravo));
            double alphaScore = result == RatedResult.Draw ? 0.5 :
                result == RatedResult.Alpha ? 1.0 : 0.0;
            List<RatingUpdate> updates = new List<RatingUpdate>(players.Count);
            ApplyTeam(alpha, alphaScore, alphaExpected, result, RoomTeam.Alpha, updates);
            ApplyTeam(bravo, 1.0 - alphaScore, 1.0 - alphaExpected, result, RoomTeam.Bravo, updates);
            RememberSettlement(matchId);
            Save();
            return updates.ToArray();
        }

        public static RatingRank RankFor(int rating)
        {
            if (rating >= 1800) return RatingRank.Master;
            if (rating >= 1600) return RatingRank.Diamond;
            if (rating >= 1400) return RatingRank.Platinum;
            if (rating >= 1200) return RatingRank.Gold;
            if (rating >= 1000) return RatingRank.Silver;
            if (rating >= 800) return RatingRank.Bronze;
            return RatingRank.Recruit;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _random.Dispose();
            _profiles.Clear();
            _settledMatches.Clear();
            _settledOrder.Clear();
        }

        private void ApplyTeam(
            List<ProfileRecord> team,
            double score,
            double expected,
            RatedResult result,
            RoomTeam teamId,
            List<RatingUpdate> updates)
        {
            for (int i = 0; i < team.Count; i++)
            {
                ProfileRecord profile = team[i];
                int before = profile.Rating;
                int k = profile.Matches < 10 ? 48 : 32;
                int delta = (int)Math.Round(k * (score - expected));
                profile.Rating = Math.Max(
                    MinimumRating,
                    Math.Min(MaximumRating, before + delta));
                profile.BestRating = Math.Max(profile.BestRating, profile.Rating);
                profile.Matches++;
                if (result == RatedResult.Draw) profile.Draws++;
                else if ((result == RatedResult.Alpha && teamId == RoomTeam.Alpha) ||
                         (result == RatedResult.Bravo && teamId == RoomTeam.Bravo))
                    profile.Wins++;
                else profile.Losses++;
                updates.Add(new RatingUpdate
                {
                    Before = before,
                    Delta = profile.Rating - before,
                    Profile = Public(profile)
                });
            }
        }

        private void RememberSettlement(string matchId)
        {
            _settledMatches.Add(matchId);
            _settledOrder.Enqueue(matchId);
            while (_settledOrder.Count > MaximumSettledMatches)
            {
                string expired = _settledOrder.Dequeue();
                _settledMatches.Remove(expired);
            }
        }

        private void Load()
        {
            if (_filePath == null || !File.Exists(_filePath)) return;
            try
            {
                using (FileStream stream = File.OpenRead(_filePath))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    if (reader.ReadUInt32() != FileMagic || reader.ReadUInt16() != FileVersion)
                        throw new InvalidDataException("Rating store header is invalid.");
                    int profileCount = reader.ReadInt32();
                    if (profileCount < 0 || profileCount > MaximumProfiles)
                        throw new InvalidDataException("Rating profile count is invalid.");
                    for (int i = 0; i < profileCount; i++) ReadProfile(reader);
                    int settlementCount = reader.ReadInt32();
                    if (settlementCount < 0 || settlementCount > MaximumSettledMatches)
                        throw new InvalidDataException("Settlement count is invalid.");
                    for (int i = 0; i < settlementCount; i++)
                    {
                        string id = ReadString(reader, 64);
                        ValidateMatchId(id);
                        if (!_settledMatches.Add(id))
                            throw new InvalidDataException("Settlement ids must be unique.");
                        _settledOrder.Enqueue(id);
                    }
                    if (stream.Position != stream.Length)
                        throw new InvalidDataException("Rating store contains trailing bytes.");
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new InvalidDataException("Rating store is truncated.", exception);
            }
        }

        private void ReadProfile(BinaryReader reader)
        {
            string playerId = ReadString(reader, 64);
            ValidatePlayerId(playerId);
            string name = ReadString(reader, 24);
            int hashLength = reader.ReadByte();
            byte[] hash = reader.ReadBytes(hashLength);
            if (hashLength != 32 || hash.Length != hashLength)
                throw new InvalidDataException("Rating token hash is invalid.");
            ProfileRecord profile = new ProfileRecord
            {
                PlayerId = playerId,
                DisplayName = CleanName(name),
                TokenHash = hash,
                Rating = reader.ReadInt32(),
                BestRating = reader.ReadInt32(),
                Matches = reader.ReadInt32(),
                Wins = reader.ReadInt32(),
                Losses = reader.ReadInt32(),
                Draws = reader.ReadInt32()
            };
            if (profile.Rating < MinimumRating || profile.Rating > MaximumRating ||
                profile.BestRating < profile.Rating || profile.BestRating > MaximumRating ||
                profile.Matches < 0 || profile.Wins < 0 || profile.Losses < 0 ||
                profile.Draws < 0 ||
                profile.Wins + profile.Losses + profile.Draws != profile.Matches ||
                _profiles.ContainsKey(playerId))
            {
                throw new InvalidDataException("Rating profile data is invalid.");
            }
            _profiles.Add(playerId, profile);
        }

        private void Save()
        {
            if (_filePath == null) return;
            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string temporary = _filePath + ".tmp";
            using (FileStream stream = File.Create(temporary))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                writer.Write(FileMagic);
                writer.Write(FileVersion);
                writer.Write(_profiles.Count);
                List<string> ids = new List<string>(_profiles.Keys);
                ids.Sort(StringComparer.Ordinal);
                for (int i = 0; i < ids.Count; i++) WriteProfile(writer, _profiles[ids[i]]);
                writer.Write(_settledOrder.Count);
                foreach (string matchId in _settledOrder) WriteString(writer, matchId);
                writer.Flush();
                stream.Flush(true);
            }
            if (File.Exists(_filePath)) File.Replace(temporary, _filePath, null);
            else File.Move(temporary, _filePath);
        }

        private static void WriteProfile(BinaryWriter writer, ProfileRecord profile)
        {
            WriteString(writer, profile.PlayerId);
            WriteString(writer, profile.DisplayName);
            writer.Write((byte)profile.TokenHash.Length);
            writer.Write(profile.TokenHash);
            writer.Write(profile.Rating);
            writer.Write(profile.BestRating);
            writer.Write(profile.Matches);
            writer.Write(profile.Wins);
            writer.Write(profile.Losses);
            writer.Write(profile.Draws);
        }

        private static PublicRatingProfile Public(ProfileRecord profile)
        {
            return new PublicRatingProfile
            {
                PlayerId = profile.PlayerId,
                DisplayName = profile.DisplayName,
                Rating = profile.Rating,
                BestRating = profile.BestRating,
                Matches = profile.Matches,
                Wins = profile.Wins,
                Losses = profile.Losses,
                Draws = profile.Draws,
                Rank = RankFor(profile.Rating)
            };
        }

        private static double Average(List<ProfileRecord> profiles)
        {
            double sum = 0;
            for (int i = 0; i < profiles.Count; i++) sum += profiles[i].Rating;
            return sum / profiles.Count;
        }

        private static double Expected(double own, double opponent)
        {
            return 1.0 / (1.0 + Math.Pow(10.0, (opponent - own) / 400.0));
        }

        private string CreatePlayerId()
        {
            return "r_" + Base64Url(RandomBytes(12));
        }

        private string CreateToken()
        {
            return Base64Url(RandomBytes(24));
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
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length) return false;
            int difference = 0;
            for (int i = 0; i < left.Length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }

        private static string CleanName(string value)
        {
            string name = (value ?? string.Empty).Trim();
            if (name.Length == 0) throw new ArgumentException("Display name is required.");
            return name.Length <= 24 ? name : name.Substring(0, 24);
        }

        private static void ValidatePlayerId(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("r_", StringComparison.Ordinal) ||
                value.Length < 14 || value.Length > 50)
                throw new InvalidDataException("Rating player id is invalid.");
            for (int i = 2; i < value.Length; i++)
            {
                char c = value[i];
                if (!(c >= 'a' && c <= 'z') &&
                    !(c >= 'A' && c <= 'Z') &&
                    !(c >= '0' && c <= '9') && c != '_' && c != '-')
                    throw new InvalidDataException("Rating player id is invalid.");
            }
        }

        private static void ValidateMatchId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 64)
                throw new ArgumentException("Match id is invalid.");
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!(c >= 'a' && c <= 'z') &&
                    !(c >= 'A' && c <= 'Z') &&
                    !(c >= '0' && c <= '9') && c != '_' && c != '-')
                    throw new ArgumentException("Match id is invalid.");
            }
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length == 0 || bytes.Length > byte.MaxValue)
                throw new InvalidDataException("Persistent string is invalid.");
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader, int maximumCharacters)
        {
            int length = reader.ReadByte();
            if (length == 0) throw new InvalidDataException("Persistent string is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            string value = Encoding.UTF8.GetString(bytes);
            if (value.Length > maximumCharacters)
                throw new InvalidDataException("Persistent string exceeds its limit.");
            return value;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RankedRatingStore));
        }

        private sealed class ProfileRecord
        {
            public string PlayerId;
            public string DisplayName;
            public byte[] TokenHash;
            public int Rating;
            public int BestRating;
            public int Matches;
            public int Wins;
            public int Losses;
            public int Draws;
        }
    }
}
