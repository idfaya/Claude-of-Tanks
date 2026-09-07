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
                        SpeedMps = 8f,
                        Health = 1750f,
                        MaxHealth = 2600f,
                        ReloadRemainingS = 2.4f,
                        Burning = true,
                        ShellSlot = 1
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
            Assert.That(decoded.StaticObstacleRevision, Is.EqualTo(2u));
            Assert.That(decoded.DestroyedStaticObstacleIndices, Is.EqualTo(new ushort[] { 3, 8 }));
            Assert.That(decoded.Entities[0].EntityId, Is.EqualTo("entity-a"));
            Assert.That(decoded.Entities[0].VehicleSpecId, Is.EqualTo("m1a2"));
            Assert.That(decoded.Entities[0].Position, Is.EqualTo(source.Entities[0].Position));
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
    }
}
