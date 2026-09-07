using System;
using System.IO;
using System.Text;

namespace ClaudeOfTanks.Network
{
    public enum DedicatedSocketAuthKind : byte
    {
        Ticket = 1,
        Reconnect = 2
    }

    public sealed class DedicatedSocketAuthRequest
    {
        public DedicatedSocketAuthKind Kind;
        public string MatchId;
        public string PlayerId;
        public string Token;
    }

    public sealed class DedicatedSocketAuthResponse
    {
        public string MatchId;
        public string PlayerId;
        public string EntityId;
        public string SessionToken;
        public int ConnectionGeneration;
    }

    public static class DedicatedSocketProtocol
    {
        public const int MaximumHandshakeBytes = 1024;
        private const uint RequestMagic = 0x41544f43u;
        private const uint ResponseMagic = 0x52544f43u;
        private const ushort Version = 1;

        public static byte[] EncodeRequest(DedicatedSocketAuthRequest request)
        {
            ValidateRequest(request);
            using (MemoryStream stream = new MemoryStream(256))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(RequestMagic);
                writer.Write(Version);
                writer.Write((byte)request.Kind);
                WriteString(writer, request.MatchId, 64);
                WriteString(writer, request.PlayerId, 48);
                WriteString(writer, request.Token, 256);
                writer.Flush();
                return Complete(stream);
            }
        }

        public static DedicatedSocketAuthRequest DecodeRequest(byte[] packet)
        {
            return Read(packet, reader =>
            {
                if (reader.ReadUInt32() != RequestMagic)
                    throw new FormatException("Dedicated auth request magic is invalid.");
                if (reader.ReadUInt16() != Version)
                    throw new FormatException("Dedicated auth version is unsupported.");
                DedicatedSocketAuthRequest request = new DedicatedSocketAuthRequest
                {
                    Kind = (DedicatedSocketAuthKind)reader.ReadByte(),
                    MatchId = ReadString(reader, 64),
                    PlayerId = ReadString(reader, 48),
                    Token = ReadString(reader, 256)
                };
                ValidateRequest(request);
                return request;
            });
        }

        public static byte[] EncodeResponse(DedicatedSocketAuthResponse response)
        {
            ValidateResponse(response);
            using (MemoryStream stream = new MemoryStream(256))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(ResponseMagic);
                writer.Write(Version);
                WriteString(writer, response.MatchId, 64);
                WriteString(writer, response.PlayerId, 48);
                WriteString(writer, response.EntityId, 64);
                WriteString(writer, response.SessionToken, 256);
                writer.Write(response.ConnectionGeneration);
                writer.Flush();
                return Complete(stream);
            }
        }

        public static DedicatedSocketAuthResponse DecodeResponse(byte[] packet)
        {
            return Read(packet, reader =>
            {
                if (reader.ReadUInt32() != ResponseMagic)
                    throw new FormatException("Dedicated auth response magic is invalid.");
                if (reader.ReadUInt16() != Version)
                    throw new FormatException("Dedicated auth version is unsupported.");
                DedicatedSocketAuthResponse response = new DedicatedSocketAuthResponse
                {
                    MatchId = ReadString(reader, 64),
                    PlayerId = ReadString(reader, 48),
                    EntityId = ReadString(reader, 64),
                    SessionToken = ReadString(reader, 256),
                    ConnectionGeneration = reader.ReadInt32()
                };
                ValidateResponse(response);
                return response;
            });
        }

        private static T Read<T>(byte[] packet, Func<BinaryReader, T> parse)
        {
            if (packet == null || packet.Length == 0 || packet.Length > MaximumHandshakeBytes)
                throw new FormatException("Dedicated handshake size is invalid.");
            try
            {
                using (MemoryStream stream = new MemoryStream(packet, false))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    T value = parse(reader);
                    if (stream.Position != stream.Length)
                        throw new FormatException("Dedicated handshake has trailing data.");
                    return value;
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new FormatException("Dedicated handshake is truncated.", exception);
            }
            catch (IOException exception)
            {
                throw new FormatException("Dedicated handshake cannot be read.", exception);
            }
        }

        private static byte[] Complete(MemoryStream stream)
        {
            if (stream.Length > MaximumHandshakeBytes)
                throw new FormatException("Dedicated handshake exceeds its size limit.");
            return stream.ToArray();
        }

        private static void ValidateRequest(DedicatedSocketAuthRequest request)
        {
            if (request == null ||
                (request.Kind != DedicatedSocketAuthKind.Ticket &&
                 request.Kind != DedicatedSocketAuthKind.Reconnect))
            {
                throw new FormatException("Dedicated auth request is invalid.");
            }
            ValidateString(request.MatchId, 64, "match id");
            ValidateString(request.PlayerId, 48, "player id");
            ValidateString(request.Token, 256, "token");
        }

        private static void ValidateResponse(DedicatedSocketAuthResponse response)
        {
            if (response == null || response.ConnectionGeneration < 1)
                throw new FormatException("Dedicated auth response is invalid.");
            ValidateString(response.MatchId, 64, "match id");
            ValidateString(response.PlayerId, 48, "player id");
            ValidateString(response.EntityId, 64, "entity id");
            ValidateString(response.SessionToken, 256, "session token");
        }

        private static void WriteString(BinaryWriter writer, string value, int maximumBytes)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (bytes.Length == 0 || bytes.Length > maximumBytes)
                throw new FormatException("Dedicated handshake string is invalid.");
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader, int maximumBytes)
        {
            int length = reader.ReadUInt16();
            if (length == 0 || length > maximumBytes)
                throw new FormatException("Dedicated handshake string is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }

        private static void ValidateString(string value, int maximumBytes, string label)
        {
            int bytes = Encoding.UTF8.GetByteCount(value ?? string.Empty);
            if (bytes == 0 || bytes > maximumBytes)
                throw new FormatException("Dedicated " + label + " is invalid.");
        }
    }
}
