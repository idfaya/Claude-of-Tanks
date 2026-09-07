using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class SnapshotDeltaTests
    {
        [Test]
        public void DeltaRoundTripAppliesChangesAddsAndVisibilityRemovals()
        {
            NetworkWorldSnapshot baseline = Snapshot(
                3,
                Entity("alpha", 0f, 1000f),
                Entity("hidden", 10f, 900f));
            NetworkWorldSnapshot current = Snapshot(
                6,
                Entity("alpha", 2f, 800f),
                Entity("newly-spotted", 30f, 700f));

            NetworkSnapshotFrame frame = SnapshotDelta.Create(current, baseline);

            Assert.That(frame.IsKeyframe, Is.False);
            Assert.That(frame.BaseTick, Is.EqualTo(3));
            Assert.That(frame.Payload.Entities, Has.Length.EqualTo(2));
            Assert.That(frame.RemovedEntityIds, Is.EqualTo(new[] { "hidden" }));

            byte[] packet = SnapshotFrameWireCodec.Encode(frame);
            NetworkSnapshotFrame decodedFrame = SnapshotFrameWireCodec.Decode(packet);
            NetworkWorldSnapshot reconstructed = SnapshotDelta.Apply(decodedFrame, baseline);

            Assert.That(reconstructed.Tick, Is.EqualTo(6));
            Assert.That(reconstructed.Entities, Has.Length.EqualTo(2));
            Assert.That(Find(reconstructed, "hidden"), Is.Null);
            Assert.That(Find(reconstructed, "alpha").Position.X, Is.EqualTo(2f));
            Assert.That(Find(reconstructed, "alpha").Health, Is.EqualTo(800f));
            Assert.That(Find(reconstructed, "newly-spotted"), Is.Not.Null);
        }

        [Test]
        public void ReceiverRejectsMissingBaseAndRecoversFromKeyframe()
        {
            NetworkWorldSnapshot baseline = Snapshot(30, Entity("alpha", 0f, 1000f));
            NetworkWorldSnapshot current = Snapshot(33, Entity("alpha", 1f, 950f));
            NetworkSnapshotFrame delta = SnapshotDelta.Create(current, baseline);
            SnapshotFrameReceiver receiver = new SnapshotFrameReceiver();

            NetworkWorldSnapshot output;
            Assert.That(
                receiver.Receive(delta, out output),
                Is.EqualTo(SnapshotFrameAdmission.MissingBase));
            Assert.That(receiver.NeedsKeyframe, Is.True);
            Assert.That(output, Is.Null);

            NetworkSnapshotFrame keyframe = SnapshotDelta.Create(baseline);
            Assert.That(
                receiver.Receive(keyframe, out output),
                Is.EqualTo(SnapshotFrameAdmission.Accepted));
            Assert.That(receiver.NeedsKeyframe, Is.False);
            Assert.That(output.Tick, Is.EqualTo(30));

            Assert.That(
                receiver.Receive(delta, out output),
                Is.EqualTo(SnapshotFrameAdmission.Accepted));
            Assert.That(output.Tick, Is.EqualTo(33));
            Assert.That(Find(output, "alpha").Health, Is.EqualTo(950f));
            Assert.That(
                receiver.Receive(keyframe, out output),
                Is.EqualTo(SnapshotFrameAdmission.Stale));
        }

        [Test]
        public void UnchangedEntityIsOmittedFromDelta()
        {
            NetworkEntitySnapshot entity = Entity("alpha", 0f, 1000f);
            NetworkWorldSnapshot baseline = Snapshot(9, entity);
            NetworkWorldSnapshot current = Snapshot(12, entity);

            NetworkSnapshotFrame frame = SnapshotDelta.Create(current, baseline);

            Assert.That(frame.Payload.Entities, Is.Empty);
            Assert.That(frame.RemovedEntityIds, Is.Empty);
            NetworkWorldSnapshot reconstructed = SnapshotDelta.Apply(frame, baseline);
            Assert.That(Find(reconstructed, "alpha"), Is.Not.Null);
        }

        [Test]
        public void DeltaRejectsUnknownRemoval()
        {
            NetworkWorldSnapshot baseline = Snapshot(9, Entity("alpha", 0f, 1000f));
            NetworkWorldSnapshot current = Snapshot(12, Entity("alpha", 1f, 1000f));
            NetworkSnapshotFrame frame = SnapshotDelta.Create(current, baseline);
            frame.RemovedEntityIds = new[] { "never-visible" };

            Assert.Throws<ArgumentException>(() => SnapshotDelta.Apply(frame, baseline));
        }

        private static NetworkEntitySnapshot Find(
            NetworkWorldSnapshot snapshot,
            string entityId)
        {
            for (int i = 0; i < snapshot.Entities.Length; i++)
                if (snapshot.Entities[i].EntityId == entityId) return snapshot.Entities[i];
            return null;
        }

        private static NetworkWorldSnapshot Snapshot(
            long tick,
            params NetworkEntitySnapshot[] entities)
        {
            return new NetworkWorldSnapshot
            {
                Tick = tick,
                ServerTimeMs = tick * (1000.0 / NetworkProtocol.TickRate),
                ViewerEntityId = "alpha",
                GameMode = GameModeId.Standard,
                Entities = entities,
                Shells = Array.Empty<NetworkShellSnapshot>(),
                Events = Array.Empty<BattleEvent>()
            };
        }

        private static NetworkEntitySnapshot Entity(string id, float x, float health)
        {
            return new NetworkEntitySnapshot
            {
                EntityId = id,
                VehicleSpecId = "medium",
                Team = id == "alpha" ? Team.Alpha : Team.Bravo,
                Position = new Float3(x, 0f, 0f),
                Health = health,
                MaxHealth = 1000f
            };
        }
    }
}
