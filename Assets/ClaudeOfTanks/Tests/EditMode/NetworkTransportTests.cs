using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class NetworkTransportTests
    {
        [Test]
        public void LoopbackPreservesControlAndCoalescesReplaceableState()
        {
            LoopbackNetworkTransportPair pair = LoopbackNetworkTransportPair.Create(2);
            List<byte> control = new List<byte>();
            byte state = 0;
            pair.Host.ControlReceived += packet => control.Add(packet[0]);
            pair.Client.StateReceived += packet => state = packet[0];

            Assert.That(pair.Client.SendControl(new byte[] { 1 }), Is.True);
            Assert.That(pair.Client.SendControl(new byte[] { 2 }), Is.True);
            Assert.That(pair.Client.SendControl(new byte[] { 3 }), Is.False);
            Assert.That(pair.Host.Pump(), Is.EqualTo(2));
            Assert.That(control, Is.EqualTo(new byte[] { 1, 2 }));

            pair.Host.SendState(new byte[] { 4 });
            pair.Host.SendState(new byte[] { 5 });
            Assert.That(pair.Client.Pump(), Is.EqualTo(1));
            Assert.That(state, Is.EqualTo(5));
            Assert.That(pair.Host.Stats.StateCoalesced, Is.EqualTo(1));
            Assert.That(pair.Host.Stats.PeakControlQueue, Is.EqualTo(2));
        }

        [Test]
        public void InputCodecRoundTripsAndRejectsTruncation()
        {
            NetworkInputCommand command = Command(9u, 12);
            command.ActionSequence = 4u;
            command.SnapshotAckTick = 9;
            command.Actions = NetworkActionBits.Fire | NetworkActionBits.RepairKit;

            byte[] packet = InputWireCodec.Encode(command);
            NetworkInputCommand decoded = InputWireCodec.Decode(packet);

            Assert.That(decoded.PlayerId, Is.EqualTo("peer"));
            Assert.That(decoded.Sequence, Is.EqualTo(9u));
            Assert.That(decoded.SnapshotAckTick, Is.EqualTo(9));
            Assert.That(decoded.Actions, Is.EqualTo(command.Actions));
            Assert.Throws<System.FormatException>(() =>
                InputWireCodec.Decode(new byte[] { packet[0], packet[1] }));
        }

        [Test]
        public void HostAndClientPumpsRecoverKeyframeThenUseAcknowledgedDelta()
        {
            BattleState state = new BattleState(new FlatHeightField(), 501u);
            TankState player = new TankState(
                "entity", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            state.Tanks.Add(player);
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("peer", player.Id);
            LoopbackNetworkTransportPair pair = LoopbackNetworkTransportPair.Create();
            LocalTankPredictor predictor = new LocalTankPredictor(
                "peer", player.Id, Team.Alpha, player.Spec, new FlatHeightField());

            using (AuthoritativeHostPump hostPump =
                new AuthoritativeHostPump(host, "peer", pair.Host))
            using (NetworkClientPump clientPump =
                new NetworkClientPump("peer", player.Id, pair.Client, predictor))
            {
                clientPump.SendInput(Command(0u, 0));
                hostPump.Update(3);
                Assert.That(pair.Client.DropPendingState(), Is.True);
                Assert.That(clientPump.LatestSnapshot, Is.Null);

                clientPump.SendInput(Command(1u, 3));
                hostPump.Update(3);
                clientPump.Pump();

                Assert.That(clientPump.LatestSnapshot.Tick, Is.EqualTo(6));
                Assert.That(hostPump.KeyframesSent, Is.EqualTo(2));
                Assert.That(clientPump.NeedsKeyframe, Is.False);

                clientPump.SendInput(Command(2u, 6));
                hostPump.Update(3);
                clientPump.Pump();

                Assert.That(clientPump.LatestSnapshot.Tick, Is.EqualTo(9));
                Assert.That(hostPump.DeltasSent, Is.EqualTo(1));
                Assert.That(clientPump.SnapshotsAccepted, Is.EqualTo(2));
                Assert.That(host.GetSnapshotAcknowledgement("peer"), Is.EqualTo(6));
                Assert.That(predictor.PendingInputCount, Is.LessThanOrEqualTo(1));
                Assert.That(player.Position.Z, Is.GreaterThan(0f));
            }
        }

        private static NetworkInputCommand Command(uint sequence, long clientTick)
        {
            return new NetworkInputCommand
            {
                PlayerId = "peer",
                Sequence = sequence,
                ClientTick = clientTick,
                SnapshotAckTick = -1,
                Throttle = 1f,
                AimYawRad = 0f,
                AimPitchRad = 0f,
                AimDistanceM = 100f
            };
        }
    }
}
