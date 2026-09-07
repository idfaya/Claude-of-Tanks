using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClaudeOfTanks.Simulation
{
    public static class ReplayWireCodec
    {
        public const int MaximumEncodedBytes = 64 * 1024 * 1024;
        public const int MaximumFrames = 60 * 60 * 30;
        public const int MaximumTanks = 32;
        public const int MaximumLandforms = 128;
        private const uint Magic = 0x52544f43u;
        private const ushort Version = 1;

        public static byte[] Encode(ReplayRecording recording)
        {
            if (recording == null) throw new ArgumentNullException(nameof(recording));
            ValidateCount(recording.Tanks.Count, 1, MaximumTanks, "tank");
            ValidateCount(recording.Frames.Count, 0, MaximumFrames, "frame");

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(recording.Seed);
                WriteFinite(writer, recording.WorldHalfExtentM, "world extent");
                writer.Write((byte)recording.GameMode);
                WriteHeightField(writer, recording.HeightField);
                writer.Write((byte)recording.Tanks.Count);

                Dictionary<string, ushort> tankIndices =
                    new Dictionary<string, ushort>(StringComparer.Ordinal);
                for (int i = 0; i < recording.Tanks.Count; i++)
                {
                    ReplayTankSeed tank = recording.Tanks[i];
                    if (!tankIndices.TryAdd(tank.Id, (ushort)i))
                        throw new InvalidDataException("Replay tank ids must be unique.");
                    WriteString(writer, tank.Id, 64);
                    writer.Write((byte)tank.Team);
                    WriteTankSpec(writer, tank.Spec);
                    WriteFloat3(writer, tank.Position);
                    WriteFinite(writer, tank.Yaw, "tank yaw");
                }

                writer.Write(recording.Frames.Count);
                for (int frameIndex = 0; frameIndex < recording.Frames.Count; frameIndex++)
                {
                    ReplayFrame frame = recording.Frames[frameIndex];
                    WriteFinite(writer, frame.DeltaTime, "frame delta");
                    if (frame.DeltaTime <= 0f || frame.DeltaTime > 0.25f)
                        throw new InvalidDataException("Replay frame delta is outside its bound.");
                    ValidateCount(frame.Inputs.Count, 0, recording.Tanks.Count, "input");
                    writer.Write((byte)frame.Inputs.Count);
                    for (int tankIndex = 0; tankIndex < recording.Tanks.Count; tankIndex++)
                    {
                        ReplayTankSeed tank = recording.Tanks[tankIndex];
                        TankInput input;
                        if (!frame.Inputs.TryGetValue(tank.Id, out input)) continue;
                        writer.Write((byte)tankIndices[tank.Id]);
                        WriteInput(writer, input);
                    }
                }
                writer.Flush();
                if (stream.Length > MaximumEncodedBytes)
                    throw new InvalidDataException("Replay exceeds its encoded size limit.");
                return stream.ToArray();
            }
        }

        public static ReplayRecording Decode(byte[] packet)
        {
            if (packet == null || packet.Length == 0 || packet.Length > MaximumEncodedBytes)
                throw new FormatException("Replay size is invalid.");
            try
            {
                using (MemoryStream stream = new MemoryStream(packet, false))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new FormatException("Replay magic is invalid.");
                    if (reader.ReadUInt16() != Version)
                        throw new FormatException("Replay version is unsupported.");
                    uint seed = reader.ReadUInt32();
                    float worldExtent = ReadFinite(reader, "world extent");
                    if (worldExtent <= 0f || worldExtent > 100000f)
                        throw new FormatException("Replay world extent is invalid.");
                    GameModeId gameMode = ReadEnum<GameModeId>(reader.ReadByte(), "game mode");
                    IHeightField heightField = ReadHeightField(reader);
                    int tankCount = reader.ReadByte();
                    ValidateCount(tankCount, 1, MaximumTanks, "tank");

                    BattleState state = new BattleState(heightField, seed, worldExtent);
                    HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
                    string[] tankIds = new string[tankCount];
                    for (int i = 0; i < tankCount; i++)
                    {
                        string id = ReadString(reader, 64);
                        if (!ids.Add(id)) throw new FormatException("Replay tank ids are duplicated.");
                        Team team = ReadEnum<Team>(reader.ReadByte(), "team");
                        TankSpec spec = ReadTankSpec(reader);
                        Float3 position = ReadFloat3(reader);
                        float yaw = ReadFinite(reader, "tank yaw");
                        state.Tanks.Add(new TankState(id, team, spec, position, yaw));
                        tankIds[i] = id;
                    }

                    ReplayRecording recording = new ReplayRecording(state, gameMode);
                    int frameCount = reader.ReadInt32();
                    ValidateCount(frameCount, 0, MaximumFrames, "frame");
                    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                    {
                        float deltaTime = ReadFinite(reader, "frame delta");
                        if (deltaTime <= 0f || deltaTime > 0.25f)
                            throw new FormatException("Replay frame delta is outside its bound.");
                        int inputCount = reader.ReadByte();
                        ValidateCount(inputCount, 0, tankCount, "input");
                        ReplayFrame frame = new ReplayFrame { DeltaTime = deltaTime };
                        for (int inputIndex = 0; inputIndex < inputCount; inputIndex++)
                        {
                            int tankIndex = reader.ReadByte();
                            if (tankIndex < 0 || tankIndex >= tankIds.Length)
                                throw new FormatException("Replay input tank index is invalid.");
                            if (frame.Inputs.ContainsKey(tankIds[tankIndex]))
                                throw new FormatException("Replay frame has duplicate tank inputs.");
                            frame.Inputs.Add(tankIds[tankIndex], ReadInput(reader));
                        }
                        recording.Frames.Add(frame);
                    }
                    if (stream.Position != stream.Length)
                        throw new FormatException("Replay has trailing data.");
                    return recording;
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new FormatException("Replay is truncated.", exception);
            }
            catch (IOException exception)
            {
                throw new FormatException("Replay cannot be read.", exception);
            }
        }

        private static void WriteHeightField(BinaryWriter writer, IHeightField heightField)
        {
            if (heightField is FlatHeightField)
            {
                writer.Write((byte)0);
                return;
            }
            LandformHeightField landformField = heightField as LandformHeightField;
            if (landformField == null)
                throw new NotSupportedException("Replay height field type is unsupported.");
            writer.Write((byte)1);
            TerrainLandform[] landforms = landformField.CopyLandforms();
            ValidateCount(landforms.Length, 0, MaximumLandforms, "landform");
            writer.Write((byte)landforms.Length);
            for (int i = 0; i < landforms.Length; i++)
            {
                TerrainLandform form = landforms[i];
                WriteString(writer, form.Kind ?? "hill", 32);
                WriteFinite(writer, form.X, "landform x");
                WriteFinite(writer, form.Z, "landform z");
                WriteFinite(writer, form.Height, "landform height");
                WriteFinite(writer, form.Length, "landform length");
                WriteFinite(writer, form.Width, "landform width");
                WriteFinite(writer, form.RadiusX, "landform radius x");
                WriteFinite(writer, form.RadiusZ, "landform radius z");
                WriteFinite(writer, form.YawRad, "landform yaw");
            }
        }

        private static IHeightField ReadHeightField(BinaryReader reader)
        {
            byte kind = reader.ReadByte();
            if (kind == 0) return new FlatHeightField();
            if (kind != 1) throw new FormatException("Replay height field type is invalid.");
            int count = reader.ReadByte();
            ValidateCount(count, 0, MaximumLandforms, "landform");
            TerrainLandform[] landforms = new TerrainLandform[count];
            for (int i = 0; i < count; i++)
            {
                landforms[i] = new TerrainLandform
                {
                    Kind = ReadString(reader, 32),
                    X = ReadFinite(reader, "landform x"),
                    Z = ReadFinite(reader, "landform z"),
                    Height = ReadFinite(reader, "landform height"),
                    Length = ReadFinite(reader, "landform length"),
                    Width = ReadFinite(reader, "landform width"),
                    RadiusX = ReadFinite(reader, "landform radius x"),
                    RadiusZ = ReadFinite(reader, "landform radius z"),
                    YawRad = ReadFinite(reader, "landform yaw")
                };
            }
            return new LandformHeightField(landforms);
        }

        private static void WriteTankSpec(BinaryWriter writer, TankSpec spec)
        {
            if (spec == null || spec.Shell == null)
                throw new InvalidDataException("Replay tank spec is missing.");
            WriteString(writer, spec.Id, 64);
            WriteString(writer, spec.DisplayName, 128);
            WriteFinite(writer, spec.MaxHealth, "max health");
            WriteFinite(writer, spec.EnginePowerHp, "engine power");
            WriteFinite(writer, spec.WeightTons, "weight");
            WriteFinite(writer, spec.TopSpeedKmh, "top speed");
            WriteFinite(writer, spec.ReverseSpeedKmh, "reverse speed");
            WriteFinite(writer, spec.HullTraverseDegS, "hull traverse");
            WriteFinite(writer, spec.TurretTraverseDegS, "turret traverse");
            WriteFinite(writer, spec.TerrainResistance, "terrain resistance");
            WriteFinite(writer, spec.TrackTraction, "track traction");
            WriteFinite(writer, spec.ArmorFrontMm, "front armor");
            WriteFinite(writer, spec.ArmorSideMm, "side armor");
            WriteFinite(writer, spec.ArmorRearMm, "rear armor");
            WriteFinite(writer, spec.CollisionRadiusM, "collision radius");
            ShellSpec shell = spec.Shell;
            WriteString(writer, shell.Name, 64);
            WriteString(writer, shell.Type, 32);
            WriteFinite(writer, shell.CaliberMm, "caliber");
            WriteFinite(writer, shell.VelocityMps, "velocity");
            WriteFinite(writer, shell.Damage, "damage");
            WriteFinite(writer, shell.Pen100Mm, "penetration 100");
            WriteFinite(writer, shell.Pen1000Mm, "penetration 1000");
            WriteFinite(writer, shell.Pen2000Mm, "penetration 2000");
            WriteFinite(writer, shell.ReloadS, "reload");
            writer.Write(shell.Guided);
            WriteFinite(writer, shell.GravityScale, "gravity");
            WriteFinite(writer, shell.GuidanceTurnRateRadS, "guidance rate");
        }

        private static TankSpec ReadTankSpec(BinaryReader reader)
        {
            TankSpec spec = new TankSpec
            {
                Id = ReadString(reader, 64),
                DisplayName = ReadString(reader, 128),
                MaxHealth = ReadFinite(reader, "max health"),
                EnginePowerHp = ReadFinite(reader, "engine power"),
                WeightTons = ReadFinite(reader, "weight"),
                TopSpeedKmh = ReadFinite(reader, "top speed"),
                ReverseSpeedKmh = ReadFinite(reader, "reverse speed"),
                HullTraverseDegS = ReadFinite(reader, "hull traverse"),
                TurretTraverseDegS = ReadFinite(reader, "turret traverse"),
                TerrainResistance = ReadFinite(reader, "terrain resistance"),
                TrackTraction = ReadFinite(reader, "track traction"),
                ArmorFrontMm = ReadFinite(reader, "front armor"),
                ArmorSideMm = ReadFinite(reader, "side armor"),
                ArmorRearMm = ReadFinite(reader, "rear armor"),
                CollisionRadiusM = ReadFinite(reader, "collision radius")
            };
            spec.Shell = new ShellSpec
            {
                Name = ReadString(reader, 64),
                Type = ReadString(reader, 32),
                CaliberMm = ReadFinite(reader, "caliber"),
                VelocityMps = ReadFinite(reader, "velocity"),
                Damage = ReadFinite(reader, "damage"),
                Pen100Mm = ReadFinite(reader, "penetration 100"),
                Pen1000Mm = ReadFinite(reader, "penetration 1000"),
                Pen2000Mm = ReadFinite(reader, "penetration 2000"),
                ReloadS = ReadFinite(reader, "reload"),
                Guided = reader.ReadBoolean(),
                GravityScale = ReadFinite(reader, "gravity"),
                GuidanceTurnRateRadS = ReadFinite(reader, "guidance rate")
            };
            if (spec.MaxHealth <= 0f || spec.WeightTons <= 0f ||
                spec.CollisionRadiusM <= 0f || spec.Shell.ReloadS <= 0f)
                throw new FormatException("Replay tank spec has invalid bounds.");
            return spec;
        }

        private static void WriteInput(BinaryWriter writer, TankInput input)
        {
            WriteFinite(writer, input.Throttle, "throttle");
            WriteFinite(writer, input.Steer, "steer");
            byte flags = 0;
            if (input.Brake) flags |= 1;
            if (input.Fire) flags |= 2;
            if (input.UseRepairKit) flags |= 4;
            if (input.UseFirstAidKit) flags |= 8;
            if (input.UseFireExtinguisher) flags |= 16;
            writer.Write(flags);
            WriteFloat3(writer, input.AimPoint);
        }

        private static TankInput ReadInput(BinaryReader reader)
        {
            float throttle = ReadFinite(reader, "throttle");
            float steer = ReadFinite(reader, "steer");
            if (throttle < -1f || throttle > 1f || steer < -1f || steer > 1f)
                throw new FormatException("Replay input axis is invalid.");
            byte flags = reader.ReadByte();
            if ((flags & ~31) != 0) throw new FormatException("Replay input flags are invalid.");
            return new TankInput
            {
                Throttle = throttle,
                Steer = steer,
                Brake = (flags & 1) != 0,
                Fire = (flags & 2) != 0,
                UseRepairKit = (flags & 4) != 0,
                UseFirstAidKit = (flags & 8) != 0,
                UseFireExtinguisher = (flags & 16) != 0,
                AimPoint = ReadFloat3(reader)
            };
        }

        private static void WriteFloat3(BinaryWriter writer, Float3 value)
        {
            WriteFinite(writer, value.X, "vector x");
            WriteFinite(writer, value.Y, "vector y");
            WriteFinite(writer, value.Z, "vector z");
        }

        private static Float3 ReadFloat3(BinaryReader reader)
        {
            return new Float3(
                ReadFinite(reader, "vector x"),
                ReadFinite(reader, "vector y"),
                ReadFinite(reader, "vector z"));
        }

        private static void WriteFinite(BinaryWriter writer, float value, string label)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new InvalidDataException("Replay " + label + " is not finite.");
            writer.Write(value);
        }

        private static float ReadFinite(BinaryReader reader, string label)
        {
            float value = reader.ReadSingle();
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new FormatException("Replay " + label + " is not finite.");
            return value;
        }

        private static void WriteString(BinaryWriter writer, string value, int maximumBytes)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (bytes.Length == 0 || bytes.Length > maximumBytes)
                throw new InvalidDataException("Replay string length is invalid.");
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader, int maximumBytes)
        {
            int length = reader.ReadUInt16();
            if (length == 0 || length > maximumBytes)
                throw new FormatException("Replay string length is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            string value = Encoding.UTF8.GetString(bytes);
            if (Encoding.UTF8.GetByteCount(value) != length)
                throw new FormatException("Replay string encoding is invalid.");
            return value;
        }

        private static T ReadEnum<T>(byte raw, string label) where T : struct
        {
            T value = (T)Enum.ToObject(typeof(T), raw);
            if (!Enum.IsDefined(typeof(T), value))
                throw new FormatException("Replay " + label + " is invalid.");
            return value;
        }

        private static void ValidateCount(int value, int minimum, int maximum, string label)
        {
            if (value < minimum || value > maximum)
                throw new FormatException("Replay " + label + " count is invalid.");
        }
    }
}
