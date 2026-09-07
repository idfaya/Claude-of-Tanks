using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ClaudeOfTanks.Network
{
    internal sealed class RoomResumeTokens : IDisposable
    {
        private readonly Dictionary<string, TokenRecord> _records =
            new Dictionary<string, TokenRecord>(StringComparer.Ordinal);
        private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

        public string Issue(string playerId)
        {
            byte[] bytes = new byte[32];
            _random.GetBytes(bytes);
            string token = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            _records[playerId] = new TokenRecord
            {
                Hash = Hash(token),
                ExpiresAtMs = long.MaxValue
            };
            return token;
        }

        public void Reserve(string playerId, long expiresAtMs)
        {
            TokenRecord record;
            if (_records.TryGetValue(playerId, out record))
                record.ExpiresAtMs = expiresAtMs;
        }

        public bool Consume(string playerId, string token, long nowMs)
        {
            TokenRecord record;
            if (string.IsNullOrEmpty(token) ||
                !_records.TryGetValue(playerId, out record) ||
                nowMs > record.ExpiresAtMs)
            {
                return false;
            }
            return FixedTimeEquals(record.Hash, Hash(token));
        }

        public void Remove(string playerId)
        {
            _records.Remove(playerId);
        }

        public void Dispose()
        {
            _records.Clear();
            _random.Dispose();
        }

        private static byte[] Hash(string token)
        {
            using (SHA256 sha = SHA256.Create())
                return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            int difference = 0;
            for (int i = 0; i < left.Length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }

        private sealed class TokenRecord
        {
            public byte[] Hash;
            public long ExpiresAtMs;
        }
    }
}
