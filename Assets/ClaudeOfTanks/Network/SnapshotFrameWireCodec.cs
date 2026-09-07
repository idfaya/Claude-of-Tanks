using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClaudeOfTanks.Network
{
    public static class SnapshotFrameWireCodec
    {
        public const int MaximumPacketBytes = SnapshotWireCodec.MaximumPacketBytes;
        public const int MaximumRemovedEntities = SnapshotWireCodec.MaximumEntities;
        private const uint Magic = 0x44544f43u;

        public static byte[] Encode(NetworkSnapshotFrame frame)
        {
            Validate(frame);
            byte[] payload = SnapshotWireCodec.Encode(frame.Payload);
            using (MemoryStream stream = new MemoryStream(payload.Length + 128))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write((ushort)NetworkProtocol.Version);
                writer.Write(frame.BaseTick);
                writer.Write((ushort)frame.RemovedEntityIds.Length);
                for (int i = 0; i < frame.RemovedEntityIds.Length; i++)
                    WriteString(writer, frame.RemovedEntityIds[i]);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                if (stream.Length > MaximumPacketBytes)
                    throw new InvalidOperationException("Snapshot frame exceeds packet size limit.");
                return stream.ToArray();
            }
        }

        public static NetworkSnapshotFrame Decode(byte[] packet)
        {
            if (packet == null || packet.Length == 0 || packet.Length > MaximumPacketBytes)
                throw new FormatException("Snapshot frame size is invalid.");
            try
            {
                using (MemoryStream stream = new MemoryStream(packet, false))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new FormatException("Snapshot frame magic is invalid.");
                    if (reader.ReadUInt16() != NetworkProtocol.Version)
                        throw new FormatException("Snapshot frame version is unsupported.");
                    NetworkSnapshotFrame frame = new NetworkSnapshotFrame
                    {
                        BaseTick = reader.ReadInt64()
                    };
                    int removedCount = reader.ReadUInt16();
                    if (removedCount > MaximumRemovedEntities)
                        throw new FormatException("Snapshot removal count exceeds limit.");
                    frame.RemovedEntityIds = new string[removedCount];
                    for (int i = 0; i < removedCount; i++)
                        frame.RemovedEntityIds[i] = ReadString(reader);
                    int payloadLength = reader.ReadInt32();
                    if (payloadLength <= 0 || payloadLength > MaximumPacketBytes ||
                        payloadLength != stream.Length - stream.Position)
                    {
                        throw new FormatException("Snapshot frame payload length is invalid.");
                    }
                    byte[] payload = reader.ReadBytes(payloadLength);
                    if (payload.Length != payloadLength) throw new EndOfStreamException();
                    frame.Payload = SnapshotWireCodec.Decode(payload);
                    Validate(frame);
                    return frame;
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new FormatException("Snapshot frame is truncated.", exception);
            }
            catch (IOException exception)
            {
                throw new FormatException("Snapshot frame cannot be read.", exception);
            }
        }

        private static void Validate(NetworkSnapshotFrame frame)
        {
            if (frame == null ||
                frame.Payload == null ||
                frame.RemovedEntityIds == null ||
                frame.BaseTick < -1 ||
                frame.BaseTick >= frame.Payload.Tick ||
                frame.RemovedEntityIds.Length > MaximumRemovedEntities ||
                (frame.IsKeyframe && frame.RemovedEntityIds.Length != 0))
            {
                throw new FormatException("Snapshot frame metadata is invalid.");
            }

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < frame.RemovedEntityIds.Length; i++)
            {
                string id = frame.RemovedEntityIds[i];
                if (string.IsNullOrEmpty(id) || !ids.Add(id))
                    throw new FormatException("Snapshot frame removals are invalid.");
            }
            HashSet<string> payloadIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < frame.Payload.Entities.Length; i++)
            {
                string id = frame.Payload.Entities[i].EntityId;
                if (!payloadIds.Add(id) || ids.Contains(id))
                    throw new FormatException("Snapshot frame operations conflict.");
            }
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length == 0 || bytes.Length > byte.MaxValue)
                throw new FormatException("Snapshot frame entity id is invalid.");
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadByte();
            if (length == 0) throw new FormatException("Snapshot frame entity id is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
