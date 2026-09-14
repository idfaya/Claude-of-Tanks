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
        public const int MaximumStaticObstacles = BattleState.MaximumStaticObstacles;
        private const uint Magic = 0x52544f43u;
        private const uint LoadoutMetadataMagic = 0x4c544f43u;
        private const uint SimulationMetadataMagic = 0x534d4f43u;
        private const ushort Version = 7;
        private const ushort HydropneumaticVersion = 6;
        private const ushort SimulationMetadataVersion = 5;
        private const ushort LoadoutMetadataVersion = 4;
        private const ushort CrushableObstacleVersion = 3;
        private const ushort StaticObstacleVersion = 2;
        private const ushort PreviousVersion = 1;

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
                WriteStaticObstacles(writer, recording.StaticObstacles);
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
                    foreach (string entityId in frame.Inputs.Keys)
                    {
                        if (!tankIndices.ContainsKey(entityId))
                            throw new InvalidDataException(
                                "Replay input references an unknown tank.");
                    }
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
                WriteLoadoutMetadata(writer, recording);
                WriteSimulationMetadata(writer, recording);
                ReplayCombatMetadataCodec.Write(
                    writer,
                    recording);
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
                    ushort version = reader.ReadUInt16();
                    if (version != Version &&
                        version != LoadoutMetadataVersion &&
                        version != CrushableObstacleVersion &&
                        version != StaticObstacleVersion &&
                        version != PreviousVersion &&
                        version != SimulationMetadataVersion &&
                        version != HydropneumaticVersion)
                        throw new FormatException("Replay version is unsupported.");
                    uint seed = reader.ReadUInt32();
                    float worldExtent = ReadFinite(reader, "world extent");
                    if (worldExtent <= 0f || worldExtent > 100000f)
                        throw new FormatException("Replay world extent is invalid.");
                    GameModeId gameMode = ReadEnum<GameModeId>(reader.ReadByte(), "game mode");
                    IHeightField heightField = ReadHeightField(reader);
                    StaticObstacle[] staticObstacles = version >= 2
                        ? ReadStaticObstacles(reader, version)
                        : Array.Empty<StaticObstacle>();
                    int tankCount = reader.ReadByte();
                    ValidateCount(tankCount, 1, MaximumTanks, "tank");

                    BattleState state = new BattleState(
                        heightField,
                        seed,
                        worldExtent,
                        staticObstacles);
                    HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
                    string[] tankIds = new string[tankCount];
                    for (int i = 0; i < tankCount; i++)
                    {
                        string id = ReadString(reader, 64);
                        if (!ids.Add(id)) throw new FormatException("Replay tank ids are duplicated.");
                        Team team = ReadEnum<Team>(reader.ReadByte(), "team");
                        TankSpec spec = ReadTankSpec(reader, version);
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
                            frame.Inputs.Add(
                                tankIds[tankIndex],
                                ReadInput(reader, version));
                        }
                        recording.Frames.Add(frame);
                    }
                    if (version >= LoadoutMetadataVersion)
                        ReadLoadoutMetadata(reader, recording);
                    if (version >= SimulationMetadataVersion)
                        ReadSimulationMetadata(
                            reader,
                            recording,
                            version);
                    if (version >= Version)
                        ReplayCombatMetadataCodec.Read(
                            reader,
                            recording);
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

        private static void WriteStaticObstacles(
            BinaryWriter writer,
            IReadOnlyList<StaticObstacle> obstacles)
        {
            int count = obstacles == null ? 0 : obstacles.Count;
            ValidateCount(count, 0, MaximumStaticObstacles, "static obstacle");
            writer.Write((ushort)count);
            for (int i = 0; i < count; i++)
            {
                StaticObstacle obstacle = obstacles[i];
                WriteString(writer, obstacle.Id, 96);
                WriteFloat3(writer, obstacle.Center);
                WriteFinite(writer, obstacle.HalfWidthM, "obstacle half width");
                WriteFinite(writer, obstacle.HalfLengthM, "obstacle half length");
                WriteFinite(writer, obstacle.HeightM, "obstacle height");
                WriteFinite(writer, obstacle.YawRad, "obstacle yaw");
                writer.Write((byte)obstacle.Flags);
                writer.Write(obstacle.Destructible);
                writer.Write(obstacle.Crushable);
                WriteFinite(
                    writer,
                    obstacle.CrushSpeedRetention,
                    "obstacle crush speed retention");
            }
        }

        private static StaticObstacle[] ReadStaticObstacles(
            BinaryReader reader,
            ushort version)
        {
            int count = reader.ReadUInt16();
            ValidateCount(count, 0, MaximumStaticObstacles, "static obstacle");
            StaticObstacle[] obstacles = new StaticObstacle[count];
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                string id = ReadString(reader, 96);
                if (!ids.Add(id))
                    throw new FormatException("Replay static obstacle ids are duplicated.");
                Float3 center = ReadFloat3(reader);
                float halfWidth = ReadFinite(reader, "obstacle half width");
                float halfLength = ReadFinite(reader, "obstacle half length");
                float height = ReadFinite(reader, "obstacle height");
                float yaw = ReadFinite(reader, "obstacle yaw");
                StaticObstacleFlags flags = (StaticObstacleFlags)reader.ReadByte();
                bool destructible = reader.ReadBoolean();
                bool crushable =
                    version >= CrushableObstacleVersion &&
                    reader.ReadBoolean();
                float crushSpeedRetention =
                    version >= CrushableObstacleVersion
                    ? ReadFinite(reader, "obstacle crush speed retention")
                    : 0.94f;
                if (halfWidth <= 0f || halfLength <= 0f || height <= 0f ||
                    crushSpeedRetention < 0f || crushSpeedRetention > 1f ||
                    flags == StaticObstacleFlags.None ||
                    (flags & ~StaticObstacleFlags.All) != 0)
                {
                    throw new FormatException("Replay static obstacle bounds are invalid.");
                }
                obstacles[i] = new StaticObstacle(
                    id,
                    center,
                    halfWidth,
                    halfLength,
                    height,
                    yaw,
                    flags,
                    destructible,
                    crushable,
                    crushSpeedRetention);
            }
            return obstacles;
        }

        private static void WriteLoadoutMetadata(
            BinaryWriter writer,
            ReplayRecording recording)
        {
            writer.Write(LoadoutMetadataMagic);
            writer.Write((byte)recording.Tanks.Count);
            for (int i = 0; i < recording.Tanks.Count; i++)
            {
                ReplayTankSeed tank = recording.Tanks[i];
                string[] equipment =
                    tank.Equipment ?? Array.Empty<string>();
                ValidateCount(
                    equipment.Length,
                    0,
                    LoadoutSimulation.EquipmentSlots,
                    "equipment");
                writer.Write((byte)equipment.Length);
                for (int item = 0;
                    item < equipment.Length;
                    item++)
                {
                    WriteString(writer, equipment[item], 64);
                }
                WriteString(
                    writer,
                    string.IsNullOrEmpty(tank.CamouflageId)
                        ? "factory"
                        : tank.CamouflageId,
                    64);
            }
        }

        private static void ReadLoadoutMetadata(
            BinaryReader reader,
            ReplayRecording recording)
        {
            if (reader.ReadUInt32() != LoadoutMetadataMagic)
                throw new FormatException(
                    "Replay loadout metadata magic is invalid.");
            int tankCount = reader.ReadByte();
            if (tankCount != recording.Tanks.Count)
                throw new FormatException(
                    "Replay loadout metadata tank count is invalid.");
            for (int i = 0; i < tankCount; i++)
            {
                int equipmentCount = reader.ReadByte();
                ValidateCount(
                    equipmentCount,
                    0,
                    LoadoutSimulation.EquipmentSlots,
                    "equipment");
                string[] equipment =
                    new string[equipmentCount];
                HashSet<string> equipmentIds =
                    new HashSet<string>(StringComparer.Ordinal);
                for (int item = 0;
                    item < equipmentCount;
                    item++)
                {
                    string id = ReadString(reader, 64);
                    if (!equipmentIds.Add(id))
                        throw new FormatException(
                            "Replay equipment ids are duplicated.");
                    equipment[item] = id;
                }
                recording.Tanks[i].Equipment =
                    LoadoutSimulation.SanitizeEquipment(
                        equipment,
                        true,
                        false);
                if (recording.Tanks[i].Equipment.Length !=
                    equipment.Length)
                {
                    throw new FormatException(
                        "Replay equipment id is invalid.");
                }
                recording.Tanks[i].CamouflageId =
                    ReadString(reader, 64);
            }
        }

        private static void WriteSimulationMetadata(
            BinaryWriter writer,
            ReplayRecording recording)
        {
            writer.Write(SimulationMetadataMagic);
            writer.Write((byte)recording.Tanks.Count);
            for (int i = 0; i < recording.Tanks.Count; i++)
            {
                TankSpec spec = recording.Tanks[i].Spec;
                WriteString(
                    writer,
                    string.IsNullOrEmpty(spec.Role)
                        ? "medium"
                        : spec.Role,
                    32);
                writer.Write(spec.IsModern);
                WriteFinite(writer, spec.AimTimeS, "aim time");
                WriteFinite(
                    writer,
                    spec.BaseAccuracyMAt100,
                    "base accuracy");
                WriteFinite(writer, spec.AimBloomMove, "move bloom");
                WriteFinite(
                    writer,
                    spec.AimBloomHullRotation,
                    "hull bloom");
                WriteFinite(
                    writer,
                    spec.AimBloomTurretRotation,
                    "turret bloom");
                WriteFinite(
                    writer,
                    spec.AimBloomAfterShot,
                    "after-shot bloom");
                WriteFinite(writer, spec.ViewRangeM, "view range");
                WriteFinite(
                    writer,
                    spec.CamouflageStill,
                    "stationary camouflage");
                WriteFinite(
                    writer,
                    spec.CamouflageMoving,
                    "moving camouflage");
                writer.Write(spec.MagazineSize);
                WriteFinite(
                    writer,
                    spec.MagazineReloadS,
                    "magazine reload");
                WriteFinite(writer, spec.IntraClipS, "intra-clip");
                HydropneumaticAimSpec hydropneumatic =
                    spec.HydropneumaticAim;
                writer.Write(hydropneumatic != null);
                if (hydropneumatic != null)
                {
                    WriteFinite(
                        writer,
                        hydropneumatic.NoseDownRad,
                        "hydropneumatic nose down");
                    WriteFinite(
                        writer,
                        hydropneumatic.NoseUpRad,
                        "hydropneumatic nose up");
                    WriteFinite(
                        writer,
                        hydropneumatic.SpeedRadS,
                        "hydropneumatic speed");
                    WriteFinite(
                        writer,
                        hydropneumatic.CompressionM,
                        "hydropneumatic compression");
                    WriteFinite(
                        writer,
                        hydropneumatic.DroopM,
                        "hydropneumatic droop");
                }
                writer.Write(spec.FixedHydraulicGun);
            }
        }

        private static void ReadSimulationMetadata(
            BinaryReader reader,
            ReplayRecording recording,
            ushort version)
        {
            if (reader.ReadUInt32() != SimulationMetadataMagic)
                throw new FormatException(
                    "Replay simulation metadata magic is invalid.");
            int tankCount = reader.ReadByte();
            if (tankCount != recording.Tanks.Count)
                throw new FormatException(
                    "Replay simulation metadata tank count is invalid.");
            for (int i = 0; i < tankCount; i++)
            {
                TankSpec spec = recording.Tanks[i].Spec;
                spec.Role = ReadString(reader, 32);
                spec.IsModern = reader.ReadBoolean();
                spec.AimTimeS = ReadFinite(reader, "aim time");
                spec.BaseAccuracyMAt100 =
                    ReadFinite(reader, "base accuracy");
                spec.AimBloomMove =
                    ReadFinite(reader, "move bloom");
                spec.AimBloomHullRotation =
                    ReadFinite(reader, "hull bloom");
                spec.AimBloomTurretRotation =
                    ReadFinite(reader, "turret bloom");
                spec.AimBloomAfterShot =
                    ReadFinite(reader, "after-shot bloom");
                spec.ViewRangeM =
                    ReadFinite(reader, "view range");
                spec.CamouflageStill =
                    ReadFinite(reader, "stationary camouflage");
                spec.CamouflageMoving =
                    ReadFinite(reader, "moving camouflage");
                spec.MagazineSize = reader.ReadInt32();
                spec.MagazineReloadS =
                    ReadFinite(reader, "magazine reload");
                spec.IntraClipS =
                    ReadFinite(reader, "intra-clip");
                if (version >= HydropneumaticVersion &&
                    reader.ReadBoolean())
                {
                    spec.HydropneumaticAim =
                        new HydropneumaticAimSpec
                        {
                            NoseDownRad = ReadFinite(
                                reader,
                                "hydropneumatic nose down"),
                            NoseUpRad = ReadFinite(
                                reader,
                                "hydropneumatic nose up"),
                            SpeedRadS = ReadFinite(
                                reader,
                                "hydropneumatic speed"),
                            CompressionM = ReadFinite(
                                reader,
                                "hydropneumatic compression"),
                            DroopM = ReadFinite(
                                reader,
                                "hydropneumatic droop")
                        };
                }
                if (version >= HydropneumaticVersion)
                {
                    spec.FixedHydraulicGun =
                        reader.ReadBoolean();
                }
                if (spec.AimTimeS <= 0f ||
                    spec.BaseAccuracyMAt100 <= 0f ||
                    spec.AimBloomMove < 0f ||
                    spec.AimBloomHullRotation < 0f ||
                    spec.AimBloomTurretRotation < 0f ||
                    spec.AimBloomAfterShot < 1f ||
                    spec.ViewRangeM <
                        SpottingSimulation.ProximitySpotRangeM ||
                    spec.CamouflageStill < 0f ||
                    spec.CamouflageStill > 1f ||
                    spec.CamouflageMoving < 0f ||
                    spec.CamouflageMoving > 1f ||
                    spec.MagazineSize < 1 ||
                    spec.MagazineSize > 64 ||
                    spec.MagazineReloadS < 0f ||
                    spec.IntraClipS < 0f ||
                    (spec.HydropneumaticAim != null &&
                     !spec.HydropneumaticAim.IsValid) ||
                    (spec.FixedHydraulicGun &&
                     spec.HydropneumaticAim == null))
                {
                    throw new FormatException(
                        "Replay simulation metadata is invalid.");
                }
                string[] equipment = recording.Tanks[i].Equipment;
                string[] legal = LoadoutSimulation.SanitizeEquipment(
                    equipment,
                    spec.IsModern,
                    spec.MagazineSize > 1);
                if (legal.Length != equipment.Length)
                {
                    throw new FormatException(
                        "Replay equipment is illegal for its vehicle.");
                }
            }
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
            WriteShell(writer, spec.Shell, false);
        }

        private static void WriteShell(
            BinaryWriter writer,
            ShellSpec shell,
            bool includeCount)
        {
            if (shell == null)
                throw new InvalidDataException(
                    "Replay shell spec is missing.");
            WriteString(writer, shell.Name, 64);
            WriteString(writer, shell.Type, 32);
            WriteFinite(writer, shell.CaliberMm, "caliber");
            WriteFinite(writer, shell.VelocityMps, "velocity");
            WriteFinite(writer, shell.Damage, "damage");
            WriteFinite(writer, shell.Pen100Mm, "penetration 100");
            WriteFinite(writer, shell.Pen1000Mm, "penetration 1000");
            WriteFinite(writer, shell.Pen2000Mm, "penetration 2000");
            WriteFinite(writer, shell.ReloadS, "reload");
            if (includeCount) writer.Write(shell.Count);
            writer.Write(shell.Guided);
            WriteFinite(writer, shell.GravityScale, "gravity");
            WriteFinite(writer, shell.GuidanceTurnRateRadS, "guidance rate");
        }

        private static TankSpec ReadTankSpec(
            BinaryReader reader,
            ushort version)
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
            spec.Shell = ReadShell(reader, false);
            spec.Shells =
                new[] { spec.Shell };
            if (spec.MaxHealth <= 0f || spec.WeightTons <= 0f ||
                spec.CollisionRadiusM <= 0f || spec.Shell.ReloadS <= 0f)
                throw new FormatException("Replay tank spec has invalid bounds.");
            if (version < SimulationMetadataVersion)
            {
                // v1-v4 predate era/autoloader metadata. Preserve their
                // original permissive equipment behavior during playback.
                spec.IsModern = true;
                spec.AimBloomMove = 0f;
                spec.AimBloomHullRotation = 0f;
                spec.AimBloomTurretRotation = 0f;
                spec.AimBloomAfterShot = 1f;
                spec.ViewRangeM =
                    SpottingSimulation.DefaultViewRangeM;
                spec.CamouflageStill = 0f;
                spec.CamouflageMoving = 0f;
            }
            return spec;
        }

        private static ShellSpec ReadShell(
            BinaryReader reader,
            bool includeCount)
        {
            return new ShellSpec
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
                Count = includeCount
                    ? reader.ReadInt32()
                    : 0,
                Guided = reader.ReadBoolean(),
                GravityScale = ReadFinite(reader, "gravity"),
                GuidanceTurnRateRadS = ReadFinite(reader, "guidance rate")
            };
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
            if (input.ToggleHydropneumaticAim) flags |= 32;
            writer.Write(flags);
            writer.Write((byte)Math.Max(
                0,
                Math.Min(15, input.ShellSlot)));
            WriteFloat3(writer, input.AimPoint);
        }

        private static TankInput ReadInput(
            BinaryReader reader,
            ushort version)
        {
            float throttle = ReadFinite(reader, "throttle");
            float steer = ReadFinite(reader, "steer");
            if (throttle < -1f || throttle > 1f || steer < -1f || steer > 1f)
                throw new FormatException("Replay input axis is invalid.");
            byte flags = reader.ReadByte();
            byte knownFlags =
                version >= HydropneumaticVersion
                    ? (byte)63
                    : (byte)31;
            if ((flags & ~knownFlags) != 0)
                throw new FormatException(
                    "Replay input flags are invalid.");
            return new TankInput
            {
                Throttle = throttle,
                Steer = steer,
                Brake = (flags & 1) != 0,
                Fire = (flags & 2) != 0,
                UseRepairKit = (flags & 4) != 0,
                UseFirstAidKit = (flags & 8) != 0,
                UseFireExtinguisher = (flags & 16) != 0,
                ToggleHydropneumaticAim =
                    version >= HydropneumaticVersion &&
                    (flags & 32) != 0,
                ShellSlot = version >= Version
                    ? reader.ReadByte()
                    : 0,
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
