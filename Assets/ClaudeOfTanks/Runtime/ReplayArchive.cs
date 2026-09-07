using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class ReplayArchiveEntry
    {
        public string Id { get; internal set; }
        public long CreatedUnixMs { get; internal set; }
        public string MapId { get; internal set; }
        public string PlayerEntityId { get; internal set; }
        public string PlayerVehicleId { get; internal set; }
        public GameModeId GameMode { get; internal set; }
        public int FrameCount { get; internal set; }
        public float DurationS { get; internal set; }
        public long FileBytes { get; internal set; }
    }

    public sealed class ArchivedReplay
    {
        public ReplayArchiveEntry Entry { get; internal set; }
        public ReplayRecording Recording { get; internal set; }
    }

    public sealed class ReplayArchive
    {
        public const int MaximumEntries = 12;
        public const int MaximumArchiveBytes = 48 * 1024 * 1024;
        private const uint Magic = 0x41544f43u;
        private const ushort Version = 1;
        private const string Extension = ".cotreplay";
        private static ReplayArchive _current;
        private readonly string _directory;
        private readonly Func<long> _clock;
        private readonly Func<string> _idFactory;

        public ReplayArchive(
            string directory,
            Func<long> clock = null,
            Func<string> idFactory = null)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Replay directory is required.", nameof(directory));
            _directory = Path.GetFullPath(directory);
            _clock = clock ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            _idFactory = idFactory ?? (() => Guid.NewGuid().ToString("N"));
        }

        public static ReplayArchive Current =>
            _current ?? (_current = new ReplayArchive(
                Path.Combine(Application.persistentDataPath, "replays")));

        public ReplayArchiveEntry Save(
            ReplayRecording recording,
            string mapId,
            string playerEntityId)
        {
            if (recording == null) throw new ArgumentNullException(nameof(recording));
            ValidateToken(mapId, 64, "map id");
            ValidateToken(playerEntityId, 64, "player entity id");
            string id = _idFactory();
            ValidateFileId(id);

            string playerVehicleId = null;
            for (int i = 0; i < recording.TankCount; i++)
                if (recording.GetTankId(i) == playerEntityId)
                    playerVehicleId = recording.GetTankSpecId(i);
            ValidateToken(playerVehicleId, 64, "player vehicle id");

            byte[] replayBytes = ReplayWireCodec.Encode(recording);
            byte[] compressed = Compress(replayBytes);
            if (compressed.Length > MaximumArchiveBytes)
                throw new InvalidDataException("Compressed replay exceeds its archive limit.");
            byte[] checksum;
            using (SHA256 sha = SHA256.Create()) checksum = sha.ComputeHash(compressed);

            ReplayArchiveEntry entry = new ReplayArchiveEntry
            {
                Id = id,
                CreatedUnixMs = _clock(),
                MapId = mapId,
                PlayerEntityId = playerEntityId,
                PlayerVehicleId = playerVehicleId,
                GameMode = ReadMode(recording),
                FrameCount = recording.FrameCount,
                DurationS = recording.DurationS
            };
            Directory.CreateDirectory(_directory);
            string path = PathFor(id);
            string temporary = path + ".tmp";
            try
            {
                using (FileStream file = new FileStream(
                    temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter writer = new BinaryWriter(file, Encoding.UTF8, true))
                {
                    writer.Write(Magic);
                    writer.Write(Version);
                    WriteString(writer, entry.Id, 64);
                    writer.Write(entry.CreatedUnixMs);
                    WriteString(writer, entry.MapId, 64);
                    WriteString(writer, entry.PlayerEntityId, 64);
                    WriteString(writer, entry.PlayerVehicleId, 64);
                    writer.Write((byte)entry.GameMode);
                    writer.Write(entry.FrameCount);
                    writer.Write(entry.DurationS);
                    writer.Write(replayBytes.Length);
                    writer.Write(compressed.Length);
                    writer.Write(checksum);
                    writer.Write(compressed);
                    writer.Flush();
                    file.Flush(true);
                }
                if (File.Exists(path))
                    throw new IOException("Replay id already exists.");
                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
            entry.FileBytes = new FileInfo(path).Length;
            Prune();
            return entry;
        }

        public ReplayArchiveEntry[] List()
        {
            if (!Directory.Exists(_directory)) return Array.Empty<ReplayArchiveEntry>();
            string[] files = Directory.GetFiles(_directory, "*" + Extension);
            List<ReplayArchiveEntry> entries = new List<ReplayArchiveEntry>();
            for (int i = 0; i < files.Length; i++)
            {
                try { entries.Add(Read(files[i], false).Entry); }
                catch (Exception error) when (
                    error is IOException ||
                    error is UnauthorizedAccessException ||
                    error is FormatException ||
                    error is InvalidDataException)
                {
                    // A corrupt replay must not make the browser unusable.
                }
            }
            entries.Sort((a, b) =>
            {
                int time = b.CreatedUnixMs.CompareTo(a.CreatedUnixMs);
                return time != 0 ? time : string.CompareOrdinal(b.Id, a.Id);
            });
            return entries.ToArray();
        }

        public ArchivedReplay Load(string id)
        {
            ValidateFileId(id);
            string path = PathFor(id);
            if (!File.Exists(path)) throw new FileNotFoundException("Replay does not exist.", path);
            return Read(path, true);
        }

        public bool Delete(string id)
        {
            ValidateFileId(id);
            string path = PathFor(id);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        private ArchivedReplay Read(string path, bool decode)
        {
            FileInfo info = new FileInfo(path);
            if (info.Length <= 0 || info.Length > MaximumArchiveBytes + 4096)
                throw new FormatException("Replay archive file size is invalid.");
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(file, Encoding.UTF8, true))
            {
                if (reader.ReadUInt32() != Magic)
                    throw new FormatException("Replay archive magic is invalid.");
                if (reader.ReadUInt16() != Version)
                    throw new FormatException("Replay archive version is unsupported.");
                ReplayArchiveEntry entry = new ReplayArchiveEntry
                {
                    Id = ReadString(reader, 64),
                    CreatedUnixMs = reader.ReadInt64(),
                    MapId = ReadString(reader, 64),
                    PlayerEntityId = ReadString(reader, 64),
                    PlayerVehicleId = ReadString(reader, 64),
                    GameMode = ReadEnum<GameModeId>(reader.ReadByte(), "game mode"),
                    FrameCount = reader.ReadInt32(),
                    DurationS = reader.ReadSingle(),
                    FileBytes = info.Length
                };
                ValidateFileId(entry.Id);
                if (Path.GetFileNameWithoutExtension(path) != entry.Id)
                    throw new FormatException("Replay archive id does not match its filename.");
                if (entry.CreatedUnixMs < 0 ||
                    entry.FrameCount < 0 ||
                    entry.FrameCount > ReplayWireCodec.MaximumFrames ||
                    !Finite(entry.DurationS) ||
                    entry.DurationS < 0f ||
                    entry.DurationS > 60f * 30f)
                    throw new FormatException("Replay archive metadata is invalid.");
                ValidateToken(entry.MapId, 64, "map id");
                ValidateToken(entry.PlayerEntityId, 64, "player entity id");
                ValidateToken(entry.PlayerVehicleId, 64, "player vehicle id");
                int uncompressedLength = reader.ReadInt32();
                int compressedLength = reader.ReadInt32();
                if (uncompressedLength <= 0 ||
                    uncompressedLength > ReplayWireCodec.MaximumEncodedBytes ||
                    compressedLength <= 0 ||
                    compressedLength > MaximumArchiveBytes)
                    throw new FormatException("Replay archive payload length is invalid.");
                byte[] expectedChecksum = reader.ReadBytes(32);
                if (expectedChecksum.Length != 32) throw new EndOfStreamException();
                byte[] compressed = reader.ReadBytes(compressedLength);
                if (compressed.Length != compressedLength) throw new EndOfStreamException();
                if (file.Position != file.Length)
                    throw new FormatException("Replay archive has trailing data.");
                byte[] actualChecksum;
                using (SHA256 sha = SHA256.Create()) actualChecksum = sha.ComputeHash(compressed);
                if (!FixedTimeEquals(expectedChecksum, actualChecksum))
                    throw new FormatException("Replay archive checksum is invalid.");

                ReplayRecording recording = null;
                if (decode)
                {
                    byte[] replayBytes = Decompress(compressed, uncompressedLength);
                    recording = ReplayWireCodec.Decode(replayBytes);
                    if (recording.FrameCount != entry.FrameCount ||
                        Math.Abs(recording.DurationS - entry.DurationS) > 0.01f ||
                        ReadMode(recording) != entry.GameMode)
                        throw new FormatException("Replay archive metadata does not match payload.");
                }
                return new ArchivedReplay { Entry = entry, Recording = recording };
            }
        }

        private void Prune()
        {
            ReplayArchiveEntry[] entries = List();
            for (int i = MaximumEntries; i < entries.Length; i++)
                Delete(entries[i].Id);
        }

        private string PathFor(string id)
        {
            return Path.Combine(_directory, id + Extension);
        }

        private static byte[] Compress(byte[] source)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream deflate =
                    new DeflateStream(
                        output,
                        System.IO.Compression.CompressionLevel.Optimal,
                        true))
                    deflate.Write(source, 0, source.Length);
                return output.ToArray();
            }
        }

        private static byte[] Decompress(byte[] source, int expectedLength)
        {
            using (MemoryStream input = new MemoryStream(source, false))
            using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream(expectedLength))
            {
                byte[] buffer = new byte[8192];
                int total = 0;
                while (true)
                {
                    int count = deflate.Read(buffer, 0, buffer.Length);
                    if (count == 0) break;
                    total += count;
                    if (total > expectedLength)
                        throw new FormatException("Replay decompression exceeds its declared length.");
                    output.Write(buffer, 0, count);
                }
                if (total != expectedLength)
                    throw new FormatException("Replay decompressed length is invalid.");
                return output.ToArray();
            }
        }

        private static GameModeId ReadMode(ReplayRecording recording)
        {
            BattleReplaySession session = new BattleReplaySession(recording);
            return session.Simulation.MatchMode.Id;
        }

        private static void WriteString(BinaryWriter writer, string value, int maximumBytes)
        {
            ValidateToken(value, maximumBytes, "string");
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader, int maximumBytes)
        {
            int length = reader.ReadUInt16();
            if (length <= 0 || length > maximumBytes)
                throw new FormatException("Replay archive string length is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            string value = Encoding.UTF8.GetString(bytes);
            if (Encoding.UTF8.GetByteCount(value) != length)
                throw new FormatException("Replay archive string encoding is invalid.");
            return value;
        }

        private static void ValidateFileId(string id)
        {
            ValidateToken(id, 64, "id");
            for (int i = 0; i < id.Length; i++)
            {
                char value = id[i];
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_')
                    throw new ArgumentException("Replay id contains an invalid character.", nameof(id));
            }
        }

        private static void ValidateToken(string value, int maximumBytes, string label)
        {
            int length = Encoding.UTF8.GetByteCount(value ?? string.Empty);
            if (length <= 0 || length > maximumBytes)
                throw new FormatException("Replay archive " + label + " is invalid.");
        }

        private static T ReadEnum<T>(byte raw, string label) where T : struct
        {
            T value = (T)Enum.ToObject(typeof(T), raw);
            if (!Enum.IsDefined(typeof(T), value))
                throw new FormatException("Replay archive " + label + " is invalid.");
            return value;
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int difference = 0;
            for (int i = 0; i < a.Length; i++) difference |= a[i] ^ b[i];
            return difference == 0;
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
