using System;
using System.IO;
using System.Text;

namespace ClaudeOfTanks.Network
{
    public static class InputWireCodec
    {
        public const int MaximumPacketBytes = 512;
        private const uint Magic = 0x49544f43u;

        public static byte[] Encode(NetworkInputCommand command)
        {
            if (!NetworkProtocol.IsValid(command))
                throw new FormatException("Input command is invalid.");
            using (MemoryStream stream = new MemoryStream(96))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write((ushort)NetworkProtocol.Version);
                WriteString(writer, command.PlayerId);
                writer.Write(command.Sequence);
                writer.Write(command.ActionSequence);
                writer.Write(command.ClientTick);
                writer.Write(command.SnapshotAckTick);
                writer.Write(command.Throttle);
                writer.Write(command.Steer);
                writer.Write(command.Brake);
                writer.Write(command.AimYawRad);
                writer.Write(command.AimPitchRad);
                writer.Write(command.AimDistanceM);
                writer.Write((byte)command.Actions);
                writer.Flush();
                return stream.ToArray();
            }
        }

        public static NetworkInputCommand Decode(byte[] packet)
        {
            if (packet == null || packet.Length == 0 || packet.Length > MaximumPacketBytes)
                throw new FormatException("Input packet size is invalid.");
            try
            {
                using (MemoryStream stream = new MemoryStream(packet, false))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new FormatException("Input packet magic is invalid.");
                    if (reader.ReadUInt16() != NetworkProtocol.Version)
                        throw new FormatException("Input protocol version is unsupported.");
                    NetworkInputCommand command = new NetworkInputCommand
                    {
                        PlayerId = ReadString(reader),
                        Sequence = reader.ReadUInt32(),
                        ActionSequence = reader.ReadUInt32(),
                        ClientTick = reader.ReadInt64(),
                        SnapshotAckTick = reader.ReadInt64(),
                        Throttle = reader.ReadSingle(),
                        Steer = reader.ReadSingle(),
                        Brake = reader.ReadBoolean(),
                        AimYawRad = reader.ReadSingle(),
                        AimPitchRad = reader.ReadSingle(),
                        AimDistanceM = reader.ReadSingle(),
                        Actions = (NetworkActionBits)reader.ReadByte()
                    };
                    if (stream.Position != stream.Length ||
                        !NetworkProtocol.IsValid(command) ||
                        (command.Actions & ~(NetworkActionBits.Fire |
                            NetworkActionBits.RepairKit |
                            NetworkActionBits.FirstAidKit |
                            NetworkActionBits.FireExtinguisher |
                            NetworkActionBits.HydropneumaticAim)) != 0)
                    {
                        throw new FormatException("Input packet payload is invalid.");
                    }
                    return command;
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new FormatException("Input packet is truncated.", exception);
            }
            catch (IOException exception)
            {
                throw new FormatException("Input packet cannot be read.", exception);
            }
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length == 0 || bytes.Length > 64)
                throw new FormatException("Input player id is invalid.");
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadByte();
            if (length == 0 || length > 64)
                throw new FormatException("Input player id is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
