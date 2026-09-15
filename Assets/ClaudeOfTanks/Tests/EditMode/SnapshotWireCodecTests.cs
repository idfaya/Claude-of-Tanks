using System;
using System.IO;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class SnapshotWireCodecTests
    {
        [Test]
        public void FullSnapshotRoundTripsWithoutIdentityLoss()
        {
            NetworkWorldSnapshot source = new NetworkWorldSnapshot
            {
                Tick = 123,
                ServerTimeMs = 2050.0,
                ViewerEntityId = "peer-entity",
                AcknowledgedInputSequence = 19u,
                GameMode = GameModeId.ZoneControl,
                Winner = Team.Alpha,
                ViewerSpotted = true,
                MatchMode = new NetworkMatchModeSnapshot
                {
                    AlphaScore = 64f,
                    BravoScore = 27f,
                    Zones = new[]
                    {
                        new Float3(-20f, 0f, 0f),
                        Float3.Zero,
                        new Float3(20f, 0f, 0f)
                    },
                    ZoneControl = new[] { -0.5f, 0.25f, 1f },
                    ZoneOwners = new Team?[]
                    {
                        Team.Bravo,
                        null,
                        Team.Alpha
                    },
                    AlphaFlag = new Float3(0f, 0f, -180f),
                    BravoFlag = new Float3(0f, 0f, 180f),
                    AlphaFlagCarrier = "entity-a",
                    BallPosition = new Float3(4f, 2f, 8f),
                    BallVelocity = new Float3(1f, 0f, 2f),
                    HordeWave = 4,
                    HordeAlive = 5,
                    HordeTotal = 8,
                    HordeNextWaveInS = 3.5f,
                    Pickups = new[]
                    {
                        new NetworkModePickupSnapshot
                        {
                            Id = "pickup-1",
                            Kind = "ammo",
                            Position = new Float3(9f, 0f, 11f),
                            SpawnedWave = 4
                        }
                    }
                },
                StaticObstacleRevision = 2u,
                DestroyedStaticObstacleIndices = new ushort[] { 3, 8 },
                Entities = new[]
                {
                    new NetworkEntitySnapshot
                    {
                        EntityId = "entity-a",
                        VehicleSpecId = "m1a2",
                        Team = Team.Alpha,
                        Position = new Float3(1.25f, 2f, -3.5f),
                        Yaw = 0.7f,
                        TurretYaw = -0.2f,
                        GunPitchRad = 0.12f,
                        TerrainPitchRad = 0.08f,
                        HullRollRad = -0.04f,
                        VerticalSpeedMps = -2.5f,
                        Grounded = false,
                        Overturned = true,
                        SpeedMps = 8f,
                        Health = 1750f,
                        MaxHealth = 2600f,
                        ReloadRemainingS = 2.4f,
                        Burning = true,
                        ModuleYellowMask = 0x00000008u,
                        ModuleRedMask = 0x00000101u,
                        CrewAliveMask = 0x0b,
                        ShellSlot = 1,
                        Kills = 3
                    }
                },
                Shells = new[]
                {
                    new NetworkShellSnapshot
                    {
                        Id = 7,
                        ShooterEntityId = "entity-a",
                        Position = new Float3(2f, 3f, 4f),
                        Velocity = new Float3(0f, -4f, 1300f)
                    }
                },
                Events = new[]
                {
                    new BattleEvent
                    {
                        Type = BattleEventType.ShellHit,
                        SourceId = "entity-a",
                        TargetId = "entity-b",
                        Position = new Float3(4f, 2f, 8f),
                        Direction = new Float3(0f, 0f, 1f),
                        Normal = new Float3(0f, 0f, -1f),
                        ShellType = "APFSDS",
                        CaliberMm = 120f,
                        Value = 440f,
                        Penetrated = true
                    }
                }
            };

            byte[] packet = SnapshotWireCodec.Encode(source);
            NetworkWorldSnapshot decoded = SnapshotWireCodec.Decode(packet);

            Assert.That(packet.Length, Is.LessThan(SnapshotWireCodec.MaximumPacketBytes));
            Assert.That(decoded.Tick, Is.EqualTo(source.Tick));
            Assert.That(decoded.ViewerEntityId, Is.EqualTo(source.ViewerEntityId));
            Assert.That(decoded.AcknowledgedInputSequence, Is.EqualTo(19u));
            Assert.That(decoded.GameMode, Is.EqualTo(GameModeId.ZoneControl));
            Assert.That(decoded.ViewerSpotted, Is.True);
            Assert.That(decoded.MatchMode.AlphaScore, Is.EqualTo(64f));
            Assert.That(decoded.MatchMode.ZoneControl, Is.EqualTo(
                new[] { -0.5f, 0.25f, 1f }));
            Assert.That(decoded.MatchMode.ZoneOwners[2], Is.EqualTo(Team.Alpha));
            Assert.That(decoded.MatchMode.AlphaFlagCarrier, Is.EqualTo("entity-a"));
            Assert.That(decoded.MatchMode.HordeWave, Is.EqualTo(4));
            Assert.That(decoded.MatchMode.HordeAlive, Is.EqualTo(5));
            Assert.That(decoded.MatchMode.HordeTotal, Is.EqualTo(8));
            Assert.That(decoded.MatchMode.HordeNextWaveInS, Is.EqualTo(3.5f));
            Assert.That(decoded.MatchMode.Pickups.Length, Is.EqualTo(1));
            Assert.That(decoded.MatchMode.Pickups[0].Kind, Is.EqualTo("ammo"));
            Assert.That(decoded.StaticObstacleRevision, Is.EqualTo(2u));
            Assert.That(decoded.DestroyedStaticObstacleIndices, Is.EqualTo(new ushort[] { 3, 8 }));
            Assert.That(decoded.Entities[0].EntityId, Is.EqualTo("entity-a"));
            Assert.That(decoded.Entities[0].VehicleSpecId, Is.EqualTo("m1a2"));
            Assert.That(decoded.Entities[0].Position, Is.EqualTo(source.Entities[0].Position));
            Assert.That(
                decoded.Entities[0].GunPitchRad,
                Is.EqualTo(0.12f));
            Assert.That(
                decoded.Entities[0].TerrainPitchRad,
                Is.EqualTo(0.08f));
            Assert.That(
                decoded.Entities[0].HullRollRad,
                Is.EqualTo(-0.04f));
            Assert.That(
                decoded.Entities[0].VerticalSpeedMps,
                Is.EqualTo(-2.5f));
            Assert.That(decoded.Entities[0].Grounded, Is.False);
            Assert.That(decoded.Entities[0].Overturned, Is.True);
            Assert.That(decoded.Entities[0].ModuleYellowMask, Is.EqualTo(0x00000008u));
            Assert.That(decoded.Entities[0].ModuleRedMask, Is.EqualTo(0x00000101u));
            Assert.That(decoded.Entities[0].CrewAliveMask, Is.EqualTo(0x0bu));
            Assert.That(decoded.Entities[0].Kills, Is.EqualTo(3));
            Assert.That(decoded.Shells[0].Velocity, Is.EqualTo(source.Shells[0].Velocity));
            Assert.That(decoded.Events[0].ShellType, Is.EqualTo("APFSDS"));
            Assert.That(decoded.Events[0].Penetrated, Is.True);
        }

        [Test]
        public void CodecRejectsTruncatedTrailingAndNonFinitePayloads()
        {
            NetworkWorldSnapshot source = EmptySnapshot();
            byte[] packet = SnapshotWireCodec.Encode(source);
            byte[] truncated = new byte[packet.Length - 1];
            Array.Copy(packet, truncated, truncated.Length);
            byte[] trailing = new byte[packet.Length + 1];
            Array.Copy(packet, trailing, packet.Length);

            Assert.Throws<FormatException>(() => SnapshotWireCodec.Decode(truncated));
            Assert.Throws<FormatException>(() => SnapshotWireCodec.Decode(trailing));

            source.ServerTimeMs = double.NaN;
            Assert.Throws<FormatException>(() => SnapshotWireCodec.Encode(source));
        }

        [Test]
        public void CodecReadsVersionOneWithIntactStructures()
        {
            byte[] packet;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(0x4e544f43u);
                writer.Write((ushort)1);
                writer.Write((long)7);
                writer.Write(116.0);
                writer.Write((byte)0);
                writer.Write(false);
                writer.Write((byte)GameModeId.Standard);
                writer.Write((sbyte)-1);
                writer.Write(false);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Flush();
                packet = stream.ToArray();
            }

            NetworkWorldSnapshot decoded = SnapshotWireCodec.Decode(packet);

            Assert.That(decoded.Tick, Is.EqualTo(7));
            Assert.That(decoded.StaticObstacleRevision, Is.Zero);
            Assert.That(decoded.DestroyedStaticObstacleIndices, Is.Empty);
        }

        [Test]
        public void CodecReadsVersionTwoWithDefaultMatchModeState()
        {
            byte[] packet;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(0x4e544f43u);
                writer.Write((ushort)2);
                writer.Write((long)9);
                writer.Write(150.0);
                writer.Write((byte)0);
                writer.Write(false);
                writer.Write((byte)GameModeId.EndlessHorde);
                writer.Write((sbyte)-1);
                writer.Write(false);
                writer.Write((uint)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Flush();
                packet = stream.ToArray();
            }

            NetworkWorldSnapshot decoded =
                SnapshotWireCodec.Decode(packet);

            Assert.That(decoded.Tick, Is.EqualTo(9));
            Assert.That(decoded.MatchMode, Is.Not.Null);
            Assert.That(decoded.MatchMode.HordeWave, Is.EqualTo(1));
        }

        [Test]
        public void CodecReadsVersionThreeEntityWithHealthyDamageDefaults()
        {
            byte[] packet;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(0x4e544f43u);
                writer.Write((ushort)3);
                writer.Write((long)12);
                writer.Write(200.0);
                WriteLegacyString(writer, "viewer");
                writer.Write(false);
                writer.Write((byte)GameModeId.Standard);
                writer.Write((sbyte)-1);
                writer.Write(false);
                WriteLegacyMatchMode(writer);
                writer.Write((uint)0);
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                WriteLegacyString(writer, "entity");
                WriteLegacyString(writer, "medium");
                writer.Write((byte)Team.Alpha);
                for (int i = 0; i < 3; i++) writer.Write(0f);
                writer.Write(0f);
                writer.Write(0f);
                writer.Write(0f);
                writer.Write(1000f);
                writer.Write(1000f);
                writer.Write(0f);
                writer.Write(false);
                writer.Write(false);
                writer.Write((byte)0);
                writer.Write(2);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Flush();
                packet = stream.ToArray();
            }

            NetworkWorldSnapshot decoded =
                SnapshotWireCodec.Decode(packet);

            Assert.That(decoded.ViewerSpotted, Is.False);
            Assert.That(decoded.Entities[0].ModuleYellowMask, Is.Zero);
            Assert.That(decoded.Entities[0].ModuleRedMask, Is.Zero);
            Assert.That(
                decoded.Entities[0].CrewAliveMask,
                Is.EqualTo(NetworkDamageState.AllCrewAliveMask));
        }

        [Test]
        public void CodecRejectsDuplicateDestroyedObstacleIndices()
        {
            NetworkWorldSnapshot source = EmptySnapshot();
            source.StaticObstacleRevision = 2u;
            source.DestroyedStaticObstacleIndices = new ushort[] { 4, 4 };

            Assert.Throws<FormatException>(() => SnapshotWireCodec.Encode(source));
        }

        private static NetworkWorldSnapshot EmptySnapshot()
        {
            return new NetworkWorldSnapshot
            {
                Tick = 0,
                ServerTimeMs = 0.0,
                GameMode = GameModeId.Standard,
                Entities = Array.Empty<NetworkEntitySnapshot>(),
                Shells = Array.Empty<NetworkShellSnapshot>(),
                Events = Array.Empty<BattleEvent>()
            };
        }

        private static void WriteLegacyMatchMode(BinaryWriter writer)
        {
            writer.Write(0f);
            writer.Write(0f);
            for (int i = 0; i < 3; i++)
            {
                for (int axis = 0; axis < 3; axis++) writer.Write(0f);
                writer.Write(0f);
                writer.Write((sbyte)-1);
            }
            for (int vector = 0; vector < 2; vector++)
                for (int axis = 0; axis < 3; axis++) writer.Write(0f);
            WriteLegacyString(writer, null);
            WriteLegacyString(writer, null);
            for (int vector = 0; vector < 2; vector++)
                for (int axis = 0; axis < 3; axis++) writer.Write(0f);
            writer.Write(1);
        }

        private static void WriteLegacyString(
            BinaryWriter writer,
            string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write((byte)0);
                return;
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }
    }
}
