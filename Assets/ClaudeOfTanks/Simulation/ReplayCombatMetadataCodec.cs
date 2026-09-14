using System;
using System.IO;
using System.Text;

namespace ClaudeOfTanks.Simulation
{
    internal static class ReplayCombatMetadataCodec
    {
        private const uint Magic = 0x434d4f43u;

        public static void Write(
            BinaryWriter writer,
            ReplayRecording recording)
        {
            writer.Write(Magic);
            writer.Write((byte)recording.Tanks.Count);
            for (int i = 0;
                i < recording.Tanks.Count;
                i++)
            {
                TankSpec spec =
                    recording.Tanks[i].Spec;
                ShellSpec[] shells =
                    spec.Shells != null &&
                    spec.Shells.Length > 0
                        ? spec.Shells
                        : new[] { spec.Shell };
                RequireCount(
                    shells.Length,
                    1,
                    16,
                    "shell");
                writer.Write((byte)shells.Length);
                for (int shell = 0;
                    shell < shells.Length;
                    shell++)
                {
                    WriteShell(writer, shells[shell]);
                }
                WriteArmor(writer, spec.Armor);
            }
        }

        public static void Read(
            BinaryReader reader,
            ReplayRecording recording)
        {
            if (reader.ReadUInt32() != Magic)
                throw new FormatException(
                    "Replay combat metadata magic is invalid.");
            int tankCount = reader.ReadByte();
            if (tankCount != recording.Tanks.Count)
                throw new FormatException(
                    "Replay combat metadata tank count is invalid.");
            for (int i = 0; i < tankCount; i++)
            {
                TankSpec spec =
                    recording.Tanks[i].Spec;
                int shellCount = reader.ReadByte();
                RequireCount(
                    shellCount,
                    1,
                    16,
                    "shell");
                spec.Shells =
                    new ShellSpec[shellCount];
                for (int shell = 0;
                    shell < shellCount;
                    shell++)
                {
                    spec.Shells[shell] =
                        ReadShell(reader);
                }
                spec.Shell = spec.Shells[0];
                spec.Armor = ReadArmor(reader);
            }
        }

        private static void WriteShell(
            BinaryWriter writer,
            ShellSpec shell)
        {
            if (shell == null)
                throw new InvalidDataException(
                    "Replay shell spec is missing.");
            WriteString(writer, shell.Name, 64);
            WriteString(writer, shell.Type, 32);
            WriteFinite(writer, shell.CaliberMm);
            WriteFinite(writer, shell.VelocityMps);
            WriteFinite(writer, shell.Damage);
            WriteFinite(writer, shell.Pen100Mm);
            WriteFinite(writer, shell.Pen1000Mm);
            WriteFinite(writer, shell.Pen2000Mm);
            WriteFinite(writer, shell.ReloadS);
            writer.Write(shell.Count);
            writer.Write(shell.Guided);
            WriteFinite(writer, shell.GravityScale);
            WriteFinite(
                writer,
                shell.GuidanceTurnRateRadS);
        }

        private static ShellSpec ReadShell(
            BinaryReader reader)
        {
            return new ShellSpec
            {
                Name = ReadString(reader, 64),
                Type = ReadString(reader, 32),
                CaliberMm = ReadFinite(reader),
                VelocityMps = ReadFinite(reader),
                Damage = ReadFinite(reader),
                Pen100Mm = ReadFinite(reader),
                Pen1000Mm = ReadFinite(reader),
                Pen2000Mm = ReadFinite(reader),
                ReloadS = ReadFinite(reader),
                Count = reader.ReadInt32(),
                Guided = reader.ReadBoolean(),
                GravityScale = ReadFinite(reader),
                GuidanceTurnRateRadS =
                    ReadFinite(reader)
            };
        }

        private static void WriteArmor(
            BinaryWriter writer,
            TankArmorModel armor)
        {
            writer.Write(armor != null);
            if (armor == null) return;
            WriteFloat3(writer, armor.TurretPivot);
            WriteFinite(writer, armor.BoundingRadiusM);
            WritePlates(writer, armor.HullPlates);
            WritePlates(writer, armor.TurretPlates);
            WriteVolumes(writer, armor.Modules);
            WriteVolumes(writer, armor.Crew);
        }

        private static TankArmorModel ReadArmor(
            BinaryReader reader)
        {
            if (!reader.ReadBoolean()) return null;
            TankArmorModel armor =
                new TankArmorModel
                {
                    TurretPivot = ReadFloat3(reader),
                    BoundingRadiusM =
                        ReadFinite(reader),
                    HullPlates = ReadPlates(reader),
                    TurretPlates = ReadPlates(reader),
                    Modules = ReadVolumes(reader),
                    Crew = ReadVolumes(reader)
                };
            if (armor.BoundingRadiusM < 0f ||
                armor.BoundingRadiusM > 20f)
            {
                throw new FormatException(
                    "Replay armor radius is invalid.");
            }
            return armor;
        }

        private static void WritePlates(
            BinaryWriter writer,
            ArmorPlateModel[] plates)
        {
            plates =
                plates ?? Array.Empty<ArmorPlateModel>();
            RequireCount(
                plates.Length,
                0,
                384,
                "armor plate");
            writer.Write((ushort)plates.Length);
            for (int i = 0; i < plates.Length; i++)
            {
                ArmorPlateModel plate = plates[i] ??
                    throw new InvalidDataException(
                        "Replay armor plate is missing.");
                WriteString(
                    writer,
                    plate.Name ?? "plate",
                    128);
                WriteString(
                    writer,
                    plate.Kind ?? "main",
                    32);
                WriteFinite(writer, plate.PhysicalMm);
                WriteFinite(writer, plate.KeMm);
                WriteFinite(writer, plate.CeMm);
                Float3[] vertices =
                    plate.Vertices ??
                    Array.Empty<Float3>();
                RequireCount(
                    vertices.Length,
                    3,
                    32,
                    "armor vertex");
                writer.Write((byte)vertices.Length);
                for (int vertex = 0;
                    vertex < vertices.Length;
                    vertex++)
                {
                    WriteFloat3(
                        writer,
                        vertices[vertex]);
                }
            }
        }

        private static ArmorPlateModel[] ReadPlates(
            BinaryReader reader)
        {
            int count = reader.ReadUInt16();
            RequireCount(
                count,
                0,
                384,
                "armor plate");
            ArmorPlateModel[] result =
                new ArmorPlateModel[count];
            for (int i = 0; i < count; i++)
            {
                ArmorPlateModel plate =
                    new ArmorPlateModel
                    {
                        Name =
                            ReadString(reader, 128),
                        Kind =
                            ReadString(reader, 32),
                        PhysicalMm =
                            ReadFinite(reader),
                        KeMm = ReadFinite(reader),
                        CeMm = ReadFinite(reader)
                    };
                int vertices = reader.ReadByte();
                RequireCount(
                    vertices,
                    3,
                    32,
                    "armor vertex");
                plate.Vertices =
                    new Float3[vertices];
                for (int vertex = 0;
                    vertex < vertices;
                    vertex++)
                {
                    plate.Vertices[vertex] =
                        ReadFloat3(reader);
                }
                result[i] = plate;
            }
            return result;
        }

        private static void WriteVolumes(
            BinaryWriter writer,
            ArmorVolumeModel[] volumes)
        {
            volumes = volumes ??
                Array.Empty<ArmorVolumeModel>();
            RequireCount(
                volumes.Length,
                0,
                64,
                "armor volume");
            writer.Write((byte)volumes.Length);
            for (int i = 0; i < volumes.Length; i++)
            {
                ArmorVolumeModel volume =
                    volumes[i] ??
                    throw new InvalidDataException(
                        "Replay armor volume is missing.");
                WriteString(writer, volume.Id, 64);
                writer.Write(volume.TurretLocal);
                writer.Write(volume.External);
                WriteFloat3(writer, volume.Minimum);
                WriteFloat3(writer, volume.Maximum);
                ArmorVolumeShapeModel[] shapes =
                    volume.Shapes ??
                    Array.Empty<ArmorVolumeShapeModel>();
                RequireCount(
                    shapes.Length,
                    0,
                    64,
                    "armor volume shape");
                writer.Write((byte)shapes.Length);
                for (int shape = 0;
                    shape < shapes.Length;
                    shape++)
                {
                    WriteShape(writer, shapes[shape]);
                }
            }
        }

        private static ArmorVolumeModel[] ReadVolumes(
            BinaryReader reader)
        {
            int count = reader.ReadByte();
            RequireCount(
                count,
                0,
                64,
                "armor volume");
            ArmorVolumeModel[] result =
                new ArmorVolumeModel[count];
            for (int i = 0; i < count; i++)
            {
                ArmorVolumeModel volume =
                    new ArmorVolumeModel
                    {
                        Id = ReadString(reader, 64),
                        TurretLocal =
                            reader.ReadBoolean(),
                        External =
                            reader.ReadBoolean(),
                        Minimum =
                            ReadFloat3(reader),
                        Maximum =
                            ReadFloat3(reader),
                        Shapes =
                            ReadShapes(reader)
                    };
                if (volume.Maximum.X <
                        volume.Minimum.X ||
                    volume.Maximum.Y <
                        volume.Minimum.Y ||
                    volume.Maximum.Z <
                        volume.Minimum.Z)
                {
                    throw new FormatException(
                        "Replay armor volume is invalid.");
                }
                result[i] = volume;
            }
            return result;
        }

        private static void WriteShape(
            BinaryWriter writer,
            ArmorVolumeShapeModel shape)
        {
            if (shape == null)
                throw new InvalidDataException(
                    "Replay armor volume shape is missing.");
            WriteString(writer, shape.Kind, 32);
            WriteFloat3(writer, shape.Center);
            WriteFloat3(writer, shape.Radii);
            WriteFloat3(writer, shape.A);
            WriteFloat3(writer, shape.B);
            WriteFinite(writer, shape.Radius);
            WriteFinite(writer, shape.RadiusA);
            WriteFinite(writer, shape.RadiusB);
            writer.Write((byte)shape.Axis);
            WriteFinite(writer, shape.HalfLength);
        }

        private static ArmorVolumeShapeModel[] ReadShapes(
            BinaryReader reader)
        {
            int count = reader.ReadByte();
            RequireCount(
                count,
                0,
                64,
                "armor volume shape");
            ArmorVolumeShapeModel[] result =
                new ArmorVolumeShapeModel[count];
            for (int i = 0; i < count; i++)
            {
                result[i] =
                    new ArmorVolumeShapeModel
                    {
                        Kind = ReadString(reader, 32),
                        Center = ReadFloat3(reader),
                        Radii = ReadFloat3(reader),
                        A = ReadFloat3(reader),
                        B = ReadFloat3(reader),
                        Radius = ReadFinite(reader),
                        RadiusA = ReadFinite(reader),
                        RadiusB = ReadFinite(reader),
                        Axis = reader.ReadByte(),
                        HalfLength =
                            ReadFinite(reader)
                    };
                if (result[i].Axis > 2)
                    throw new FormatException(
                        "Replay armor shape axis is invalid.");
            }
            return result;
        }

        private static void WriteString(
            BinaryWriter writer,
            string value,
            int maximum)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(
                    value ?? string.Empty);
            if (bytes.Length < 1 ||
                bytes.Length > maximum)
            {
                throw new InvalidDataException(
                    "Replay combat string is invalid.");
            }
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(
            BinaryReader reader,
            int maximum)
        {
            int length = reader.ReadByte();
            if (length < 1 || length > maximum)
                throw new FormatException(
                    "Replay combat string is invalid.");
            byte[] bytes =
                reader.ReadBytes(length);
            if (bytes.Length != length)
                throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }

        private static void WriteFloat3(
            BinaryWriter writer,
            Float3 value)
        {
            WriteFinite(writer, value.X);
            WriteFinite(writer, value.Y);
            WriteFinite(writer, value.Z);
        }

        private static Float3 ReadFloat3(
            BinaryReader reader)
        {
            return new Float3(
                ReadFinite(reader),
                ReadFinite(reader),
                ReadFinite(reader));
        }

        private static void WriteFinite(
            BinaryWriter writer,
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                throw new InvalidDataException(
                    "Replay combat number is invalid.");
            }
            writer.Write(value);
        }

        private static float ReadFinite(
            BinaryReader reader)
        {
            float value = reader.ReadSingle();
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                throw new FormatException(
                    "Replay combat number is invalid.");
            }
            return value;
        }

        private static void RequireCount(
            int value,
            int minimum,
            int maximum,
            string label)
        {
            if (value < minimum || value > maximum)
                throw new FormatException(
                    "Replay " + label +
                    " count is invalid.");
        }
    }
}
