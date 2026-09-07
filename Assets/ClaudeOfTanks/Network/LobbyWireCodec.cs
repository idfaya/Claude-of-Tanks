using System;
using System.IO;
using System.Text;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public static class LobbyWireCodec
    {
        public const int MaximumPacketBytes = 32 * 1024;
        public const int MaximumEquipment = 3;
        private const uint Magic = 0x4c544f43u;
        private const ushort Version = 2;
        private const ushort MinimumSupportedVersion = 1;
        private const ushort NullString = ushort.MaxValue;
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        public static byte[] EncodeHello(uint sequence)
        {
            return Encode(LobbyWireMessageKind.Hello, sequence, null);
        }

        public static byte[] EncodeLeave(uint sequence)
        {
            return Encode(LobbyWireMessageKind.Leave, sequence, null);
        }

        public static byte[] EncodeMatchReady(uint sequence)
        {
            return Encode(LobbyWireMessageKind.MatchReady, sequence, null);
        }

        public static byte[] EncodePing(uint sequence, uint nonce)
        {
            return Encode(
                LobbyWireMessageKind.Ping,
                sequence,
                writer => writer.Write(nonce));
        }

        public static byte[] EncodePong(uint sequence, uint nonce)
        {
            return Encode(
                LobbyWireMessageKind.Pong,
                sequence,
                writer => writer.Write(nonce));
        }

        public static byte[] EncodeCommand(
            uint sequence,
            LobbyCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            return Encode(
                LobbyWireMessageKind.Command,
                sequence,
                writer => WriteCommand(writer, command));
        }

        public static byte[] EncodeState(
            uint sequence,
            RoomStateSnapshot state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return Encode(
                LobbyWireMessageKind.State,
                sequence,
                writer => LobbyWireDataCodec.WriteState(writer, state));
        }

        public static byte[] EncodeError(
            uint sequence,
            string code,
            string message)
        {
            return Encode(
                LobbyWireMessageKind.Error,
                sequence,
                writer =>
                {
                    WriteString(writer, code, 64, false);
                    WriteString(writer, message, 512, false);
                });
        }

        public static byte[] EncodeMatchStart(
            uint sequence,
            RoomMatchPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            return Encode(
                LobbyWireMessageKind.MatchStart,
                sequence,
                writer => LobbyWireDataCodec.WriteMatchPlan(writer, plan));
        }

        public static LobbyWireMessage Decode(byte[] packet)
        {
            if (packet == null || packet.Length == 0 ||
                packet.Length > MaximumPacketBytes)
            {
                throw new FormatException("Lobby packet size is invalid.");
            }
            try
            {
                using (MemoryStream stream = new MemoryStream(packet, false))
                using (BinaryReader reader =
                    new BinaryReader(stream, StrictUtf8, true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new FormatException("Lobby packet magic is invalid.");
                    ushort version = reader.ReadUInt16();
                    if (version < MinimumSupportedVersion ||
                        version > Version)
                        throw new FormatException(
                            "Lobby protocol version is unsupported.");
                    LobbyWireMessageKind kind =
                        (LobbyWireMessageKind)reader.ReadByte();
                    if (!Enum.IsDefined(typeof(LobbyWireMessageKind), kind))
                        throw new FormatException(
                            "Lobby message kind is invalid.");
                    LobbyWireMessage message = new LobbyWireMessage
                    {
                        Kind = kind,
                        Sequence = reader.ReadUInt32()
                    };
                    ReadPayload(reader, message, version);
                    if (stream.Position != stream.Length)
                        throw new FormatException(
                            "Lobby packet contains trailing bytes.");
                    return message;
                }
            }
            catch (EndOfStreamException error)
            {
                throw new FormatException("Lobby packet is truncated.", error);
            }
            catch (IOException error)
            {
                throw new FormatException("Lobby packet cannot be read.", error);
            }
            catch (DecoderFallbackException error)
            {
                throw new FormatException("Lobby packet text is invalid.", error);
            }
        }

        private static byte[] Encode(
            LobbyWireMessageKind kind,
            uint sequence,
            Action<BinaryWriter> writePayload)
        {
            using (MemoryStream stream = new MemoryStream(512))
            using (BinaryWriter writer =
                new BinaryWriter(stream, StrictUtf8, true))
            {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write((byte)kind);
                writer.Write(sequence);
                writePayload?.Invoke(writer);
                writer.Flush();
                if (stream.Length > MaximumPacketBytes)
                    throw new FormatException("Lobby packet is too large.");
                return stream.ToArray();
            }
        }

        private static void ReadPayload(
            BinaryReader reader,
            LobbyWireMessage message,
            ushort version)
        {
            switch (message.Kind)
            {
                case LobbyWireMessageKind.Hello:
                case LobbyWireMessageKind.Leave:
                case LobbyWireMessageKind.MatchReady:
                    return;
                case LobbyWireMessageKind.Command:
                    message.Command = ReadCommand(reader);
                    return;
                case LobbyWireMessageKind.State:
                    message.State = LobbyWireDataCodec.ReadState(reader);
                    return;
                case LobbyWireMessageKind.Error:
                    message.ErrorCode = ReadString(reader, 64, false);
                    message.ErrorMessage = ReadString(reader, 512, false);
                    return;
                case LobbyWireMessageKind.MatchStart:
                    message.MatchPlan =
                        LobbyWireDataCodec.ReadMatchPlan(
                            reader,
                            version >= 2);
                    return;
                case LobbyWireMessageKind.Ping:
                case LobbyWireMessageKind.Pong:
                    message.PingNonce = reader.ReadUInt32();
                    return;
                default:
                    throw new FormatException("Lobby message kind is invalid.");
            }
        }

        private static void WriteCommand(
            BinaryWriter writer,
            LobbyCommand command)
        {
            if (!Enum.IsDefined(typeof(LobbyCommandKind), command.Kind))
                throw new FormatException("Lobby command kind is invalid.");
            writer.Write((byte)command.Kind);
            switch (command.Kind)
            {
                case LobbyCommandKind.SetName:
                    WriteString(writer, command.Text, 96, false);
                    break;
                case LobbyCommandKind.SelectVehicle:
                case LobbyCommandKind.SelectCamo:
                case LobbyCommandKind.SetMap:
                    WriteString(writer, command.Text, 64, false);
                    break;
                case LobbyCommandKind.SelectEquipment:
                    WriteStrings(writer, command.Equipment);
                    break;
                case LobbyCommandKind.SetReady:
                case LobbyCommandKind.SetLocked:
                    writer.Write(command.BoolValue);
                    break;
                case LobbyCommandKind.SetTeam:
                    if (!Enum.IsDefined(typeof(RoomTeam), command.Team))
                        throw new FormatException("Lobby team is invalid.");
                    writer.Write((byte)command.Team);
                    break;
                case LobbyCommandKind.SetGameMode:
                    if (!Enum.IsDefined(typeof(GameModeId), command.GameMode))
                        throw new FormatException("Lobby game mode is invalid.");
                    writer.Write((int)command.GameMode);
                    break;
                case LobbyCommandKind.SetTeamSize:
                    writer.Write(command.IntValue);
                    break;
                case LobbyCommandKind.Start:
                    writer.Write(command.MatchSeed);
                    break;
                default:
                    throw new FormatException("Lobby command kind is invalid.");
            }
        }

        private static LobbyCommand ReadCommand(BinaryReader reader)
        {
            LobbyCommand command = new LobbyCommand
            {
                Kind = (LobbyCommandKind)reader.ReadByte()
            };
            if (!Enum.IsDefined(typeof(LobbyCommandKind), command.Kind))
                throw new FormatException("Lobby command kind is invalid.");
            switch (command.Kind)
            {
                case LobbyCommandKind.SetName:
                    command.Text = ReadString(reader, 96, false);
                    break;
                case LobbyCommandKind.SelectVehicle:
                case LobbyCommandKind.SelectCamo:
                case LobbyCommandKind.SetMap:
                    command.Text = ReadString(reader, 64, false);
                    break;
                case LobbyCommandKind.SelectEquipment:
                    command.Equipment = ReadStrings(reader);
                    break;
                case LobbyCommandKind.SetReady:
                case LobbyCommandKind.SetLocked:
                    command.BoolValue = ReadBoolean(reader);
                    break;
                case LobbyCommandKind.SetTeam:
                    command.Team = (RoomTeam)reader.ReadByte();
                    if (!Enum.IsDefined(typeof(RoomTeam), command.Team))
                        throw new FormatException("Lobby team is invalid.");
                    break;
                case LobbyCommandKind.SetGameMode:
                    command.GameMode = (GameModeId)reader.ReadInt32();
                    if (!Enum.IsDefined(typeof(GameModeId), command.GameMode))
                        throw new FormatException("Lobby game mode is invalid.");
                    break;
                case LobbyCommandKind.SetTeamSize:
                    command.IntValue = reader.ReadInt32();
                    break;
                case LobbyCommandKind.Start:
                    command.MatchSeed = reader.ReadUInt32();
                    break;
            }
            return command;
        }

        private static void WriteStrings(
            BinaryWriter writer,
            string[] values)
        {
            string[] items = values ?? Array.Empty<string>();
            if (items.Length > MaximumEquipment)
                throw new FormatException("Equipment count is invalid.");
            writer.Write((byte)items.Length);
            for (int i = 0; i < items.Length; i++)
                WriteString(writer, items[i], 64, false);
        }

        private static string[] ReadStrings(BinaryReader reader)
        {
            int count = reader.ReadByte();
            if (count > MaximumEquipment)
                throw new FormatException("Equipment count is invalid.");
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadString(reader, 64, false);
            return result;
        }

        private static void WriteString(
            BinaryWriter writer,
            string value,
            int maximumBytes,
            bool nullable)
        {
            if (value == null)
            {
                if (!nullable)
                    throw new FormatException("Lobby text is required.");
                writer.Write(NullString);
                return;
            }
            byte[] bytes = StrictUtf8.GetBytes(value);
            if (bytes.Length > maximumBytes || (!nullable && bytes.Length == 0))
                throw new FormatException("Lobby text length is invalid.");
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(
            BinaryReader reader,
            int maximumBytes,
            bool nullable)
        {
            int length = reader.ReadUInt16();
            if (length == NullString)
            {
                if (!nullable)
                    throw new FormatException("Lobby text is required.");
                return null;
            }
            if (length > maximumBytes || (!nullable && length == 0))
                throw new FormatException("Lobby text length is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return StrictUtf8.GetString(bytes);
        }

        internal static bool ReadBoolean(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            if (value > 1)
                throw new FormatException("Lobby boolean is invalid.");
            return value == 1;
        }
    }
}
