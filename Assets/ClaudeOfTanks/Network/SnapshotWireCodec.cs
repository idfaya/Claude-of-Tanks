using System;
using System.IO;
using System.Text;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public static class SnapshotWireCodec
    {
        public const int MaximumPacketBytes = 65535;
        public const int MaximumEntities = 64;
        public const int MaximumShells = 256;
        public const int MaximumEvents = 256;
        private const uint Magic = 0x4e544f43u;

        public static byte[] Encode(NetworkWorldSnapshot snapshot)
        {
            ValidateSnapshot(snapshot);
            using (MemoryStream stream = new MemoryStream(4096))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write((ushort)NetworkProtocol.Version);
                writer.Write(snapshot.Tick);
                writer.Write(snapshot.ServerTimeMs);
                WriteString(writer, snapshot.ViewerEntityId);
                writer.Write(snapshot.AcknowledgedInputSequence.HasValue);
                if (snapshot.AcknowledgedInputSequence.HasValue)
                    writer.Write(snapshot.AcknowledgedInputSequence.Value);
                writer.Write((byte)snapshot.GameMode);
                writer.Write(snapshot.Winner.HasValue ? (sbyte)snapshot.Winner.Value : (sbyte)-1);
                writer.Write(snapshot.Draw);

                writer.Write((ushort)snapshot.Entities.Length);
                for (int i = 0; i < snapshot.Entities.Length; i++)
                    WriteEntity(writer, snapshot.Entities[i]);
                writer.Write((ushort)snapshot.Shells.Length);
                for (int i = 0; i < snapshot.Shells.Length; i++)
                    WriteShell(writer, snapshot.Shells[i]);
                writer.Write((ushort)snapshot.Events.Length);
                for (int i = 0; i < snapshot.Events.Length; i++)
                    WriteEvent(writer, snapshot.Events[i]);
                writer.Flush();
                if (stream.Length > MaximumPacketBytes)
                    throw new InvalidOperationException("Snapshot exceeds packet size limit.");
                return stream.ToArray();
            }
        }

        public static NetworkWorldSnapshot Decode(byte[] packet)
        {
            if (packet == null || packet.Length == 0 || packet.Length > MaximumPacketBytes)
                throw new FormatException("Snapshot packet size is invalid.");
            try
            {
                using (MemoryStream stream = new MemoryStream(packet, false))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new FormatException("Snapshot magic is invalid.");
                    if (reader.ReadUInt16() != NetworkProtocol.Version)
                        throw new FormatException("Snapshot protocol version is unsupported.");

                    NetworkWorldSnapshot snapshot = new NetworkWorldSnapshot
                    {
                        Tick = reader.ReadInt64(),
                        ServerTimeMs = reader.ReadDouble(),
                        ViewerEntityId = ReadString(reader),
                        AcknowledgedInputSequence = reader.ReadBoolean()
                            ? reader.ReadUInt32()
                            : (uint?)null,
                        GameMode = (GameModeId)reader.ReadByte()
                    };
                    sbyte winner = reader.ReadSByte();
                    snapshot.Winner = winner < 0 ? (Team?)null : (Team)winner;
                    snapshot.Draw = reader.ReadBoolean();

                    int entityCount = ReadCount(reader, MaximumEntities, "entity");
                    snapshot.Entities = new NetworkEntitySnapshot[entityCount];
                    for (int i = 0; i < entityCount; i++) snapshot.Entities[i] = ReadEntity(reader);
                    int shellCount = ReadCount(reader, MaximumShells, "shell");
                    snapshot.Shells = new NetworkShellSnapshot[shellCount];
                    for (int i = 0; i < shellCount; i++) snapshot.Shells[i] = ReadShell(reader);
                    int eventCount = ReadCount(reader, MaximumEvents, "event");
                    snapshot.Events = new BattleEvent[eventCount];
                    for (int i = 0; i < eventCount; i++) snapshot.Events[i] = ReadEvent(reader);

                    if (stream.Position != stream.Length)
                        throw new FormatException("Snapshot contains trailing bytes.");
                    ValidateSnapshot(snapshot);
                    return snapshot;
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new FormatException("Snapshot packet is truncated.", exception);
            }
            catch (IOException exception)
            {
                throw new FormatException("Snapshot packet cannot be read.", exception);
            }
        }

        private static void WriteEntity(BinaryWriter writer, NetworkEntitySnapshot entity)
        {
            WriteString(writer, entity.EntityId);
            WriteString(writer, entity.VehicleSpecId);
            writer.Write((byte)entity.Team);
            WriteFloat3(writer, entity.Position);
            writer.Write(entity.Yaw);
            writer.Write(entity.TurretYaw);
            writer.Write(entity.SpeedMps);
            writer.Write(entity.Health);
            writer.Write(entity.MaxHealth);
            writer.Write(entity.ReloadRemainingS);
            writer.Write(entity.Destroyed);
            writer.Write(entity.Burning);
            writer.Write((byte)entity.ShellSlot);
        }

        private static NetworkEntitySnapshot ReadEntity(BinaryReader reader)
        {
            return new NetworkEntitySnapshot
            {
                EntityId = ReadString(reader),
                VehicleSpecId = ReadString(reader),
                Team = (Team)reader.ReadByte(),
                Position = ReadFloat3(reader),
                Yaw = reader.ReadSingle(),
                TurretYaw = reader.ReadSingle(),
                SpeedMps = reader.ReadSingle(),
                Health = reader.ReadSingle(),
                MaxHealth = reader.ReadSingle(),
                ReloadRemainingS = reader.ReadSingle(),
                Destroyed = reader.ReadBoolean(),
                Burning = reader.ReadBoolean(),
                ShellSlot = reader.ReadByte()
            };
        }

        private static void WriteShell(BinaryWriter writer, NetworkShellSnapshot shell)
        {
            writer.Write(shell.Id);
            WriteString(writer, shell.ShooterEntityId);
            WriteFloat3(writer, shell.Position);
            WriteFloat3(writer, shell.Velocity);
        }

        private static NetworkShellSnapshot ReadShell(BinaryReader reader)
        {
            return new NetworkShellSnapshot
            {
                Id = reader.ReadInt32(),
                ShooterEntityId = ReadString(reader),
                Position = ReadFloat3(reader),
                Velocity = ReadFloat3(reader)
            };
        }

        private static void WriteEvent(BinaryWriter writer, BattleEvent battleEvent)
        {
            writer.Write((byte)battleEvent.Type);
            WriteString(writer, battleEvent.SourceId);
            WriteString(writer, battleEvent.TargetId);
            WriteFloat3(writer, battleEvent.Position);
            WriteFloat3(writer, battleEvent.Direction);
            WriteFloat3(writer, battleEvent.Normal);
            WriteString(writer, battleEvent.ShellType);
            writer.Write(battleEvent.CaliberMm);
            writer.Write(battleEvent.Value);
            writer.Write(battleEvent.Penetrated);
        }

        private static BattleEvent ReadEvent(BinaryReader reader)
        {
            return new BattleEvent
            {
                Type = (BattleEventType)reader.ReadByte(),
                SourceId = ReadString(reader),
                TargetId = ReadString(reader),
                Position = ReadFloat3(reader),
                Direction = ReadFloat3(reader),
                Normal = ReadFloat3(reader),
                ShellType = ReadString(reader),
                CaliberMm = reader.ReadSingle(),
                Value = reader.ReadSingle(),
                Penetrated = reader.ReadBoolean()
            };
        }

        private static void WriteFloat3(BinaryWriter writer, Float3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        private static Float3 ReadFloat3(BinaryReader reader)
        {
            return new Float3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write((byte)0);
                return;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > byte.MaxValue)
                throw new InvalidOperationException("Snapshot string exceeds 255 UTF-8 bytes.");
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadByte();
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return length == 0 ? null : Encoding.UTF8.GetString(bytes);
        }

        private static int ReadCount(BinaryReader reader, int maximum, string label)
        {
            int count = reader.ReadUInt16();
            if (count > maximum)
                throw new FormatException("Snapshot " + label + " count exceeds limit.");
            return count;
        }

        private static void ValidateSnapshot(NetworkWorldSnapshot snapshot)
        {
            if (snapshot == null ||
                snapshot.Tick < 0 ||
                !IsFinite(snapshot.ServerTimeMs) ||
                snapshot.Entities == null ||
                snapshot.Entities.Length > MaximumEntities ||
                snapshot.Shells == null ||
                snapshot.Shells.Length > MaximumShells ||
                snapshot.Events == null ||
                snapshot.Events.Length > MaximumEvents ||
                !Enum.IsDefined(typeof(GameModeId), snapshot.GameMode) ||
                (snapshot.Winner.HasValue &&
                 !Enum.IsDefined(typeof(Team), snapshot.Winner.Value)))
            {
                throw new FormatException("Snapshot header is invalid.");
            }

            for (int i = 0; i < snapshot.Entities.Length; i++)
            {
                NetworkEntitySnapshot entity = snapshot.Entities[i];
                if (entity == null ||
                    string.IsNullOrEmpty(entity.EntityId) ||
                    string.IsNullOrEmpty(entity.VehicleSpecId) ||
                    !Enum.IsDefined(typeof(Team), entity.Team) ||
                    !IsFinite(entity.Position) ||
                    !IsFinite(entity.Yaw) ||
                    !IsFinite(entity.TurretYaw) ||
                    !IsFinite(entity.SpeedMps) ||
                    !IsFinite(entity.Health) ||
                    !IsFinite(entity.MaxHealth) ||
                    !IsFinite(entity.ReloadRemainingS) ||
                    entity.ShellSlot < 0 ||
                    entity.ShellSlot > 15)
                {
                    throw new FormatException("Snapshot entity is invalid.");
                }
            }
            for (int i = 0; i < snapshot.Shells.Length; i++)
            {
                NetworkShellSnapshot shell = snapshot.Shells[i];
                if (shell == null ||
                    string.IsNullOrEmpty(shell.ShooterEntityId) ||
                    !IsFinite(shell.Position) ||
                    !IsFinite(shell.Velocity))
                {
                    throw new FormatException("Snapshot shell is invalid.");
                }
            }
            for (int i = 0; i < snapshot.Events.Length; i++)
            {
                BattleEvent battleEvent = snapshot.Events[i];
                if (!Enum.IsDefined(typeof(BattleEventType), battleEvent.Type) ||
                    !IsFinite(battleEvent.Position) ||
                    !IsFinite(battleEvent.Direction) ||
                    !IsFinite(battleEvent.Normal) ||
                    !IsFinite(battleEvent.CaliberMm) ||
                    !IsFinite(battleEvent.Value))
                {
                    throw new FormatException("Snapshot event is invalid.");
                }
            }
        }

        private static bool IsFinite(Float3 value)
        {
            return IsFinite(value.X) && IsFinite(value.Y) && IsFinite(value.Z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
