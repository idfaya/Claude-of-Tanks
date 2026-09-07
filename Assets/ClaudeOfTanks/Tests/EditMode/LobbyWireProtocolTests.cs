using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class LobbyWireProtocolTests
    {
        [Test]
        public void StateAndMatchPlanRoundTripWithoutIdentityLoss()
        {
            using (AuthoritativeRoom room = Room())
            {
                RoomJoinReceipt guest = room.Join(
                    "guest",
                    "Guest",
                    RoomTeam.Bravo,
                    "t90m");
                room.SelectEquipment("guest", "repair", "optics");
                room.SelectCamo("guest", "winter");
                room.SelectVehicle("host", "m1a2");
                room.SetReady("host", true);
                room.SetReady("guest", true);

                LobbyWireMessage state = LobbyWireCodec.Decode(
                    LobbyWireCodec.EncodeState(17u, room.Snapshot()));
                Assert.That(state.Kind, Is.EqualTo(
                    LobbyWireMessageKind.State));
                Assert.That(state.Sequence, Is.EqualTo(17u));
                Assert.That(state.State.Players, Has.Length.EqualTo(2));
                Assert.That(
                    Player(state.State, "guest").EntityId,
                    Is.EqualTo(guest.Player.EntityId));
                Assert.That(
                    Player(state.State, "guest").Equipment,
                    Is.EqualTo(new[] { "repair", "optics" }));

                RoomMatchPlan source = room.Start("host", 7701u);
                LobbyWireMessage start = LobbyWireCodec.Decode(
                    LobbyWireCodec.EncodeMatchStart(18u, source));
                Assert.That(start.Kind, Is.EqualTo(
                    LobbyWireMessageKind.MatchStart));
                Assert.That(start.MatchPlan.Seed, Is.EqualTo(7701u));
                Assert.That(start.MatchPlan.Seats, Has.Length.EqualTo(2));
                Assert.That(
                    start.MatchPlan.Seats[1].EntityId,
                    Is.EqualTo(source.Seats[1].EntityId));
            }
        }

        [Test]
        public void CodecRejectsTrailingBytesInvalidUtf8AndOversize()
        {
            byte[] hello = LobbyWireCodec.EncodeHello(1u);
            Array.Resize(ref hello, hello.Length + 1);
            Assert.Throws<FormatException>(() =>
                LobbyWireCodec.Decode(hello));

            byte[] error = LobbyWireCodec.EncodeError(
                2u,
                "bad",
                "message");
            error[13] = 0xff;
            Assert.Throws<FormatException>(() =>
                LobbyWireCodec.Decode(error));

            Assert.Throws<FormatException>(() =>
                LobbyWireCodec.Decode(
                    new byte[LobbyWireCodec.MaximumPacketBytes + 1]));

            byte[] ready = LobbyWireCodec.EncodeCommand(
                3u,
                new LobbyCommand
                {
                    Kind = LobbyCommandKind.SetReady,
                    BoolValue = true
                });
            ready[12] = 2;
            Assert.Throws<FormatException>(() =>
                LobbyWireCodec.Decode(ready));
        }

        [Test]
        public void BoundPeerAppliesCommandsRejectsHostOnlyAndAcksStart()
        {
            using (AuthoritativeRoom room = Room())
            {
                room.Join("guest", "Guest", RoomTeam.Bravo);
                LoopbackNetworkTransportPair pair =
                    LoopbackNetworkTransportPair.Create();
                using (LobbyHostPeerPump host =
                    new LobbyHostPeerPump("guest", pair.Host))
                using (LobbyClientPump client =
                    new LobbyClientPump(pair.Client))
                {
                    bool ready = false;
                    host.StateRequested += peer =>
                        peer.SendState(room.Snapshot());
                    host.CommandReceived += (peer, command) =>
                    {
                        try
                        {
                            LobbyCommandApplier.Apply(
                                room,
                                peer.PlayerId,
                                command);
                            peer.SendState(room.Snapshot());
                        }
                        catch (RoomPolicyException policy)
                        {
                            peer.SendError(policy.Code, policy.Message);
                        }
                    };
                    host.MatchReady += _ => ready = true;

                    host.Pump();
                    client.Pump();
                    Assert.That(client.State, Is.Not.Null);

                    Assert.That(client.Submit(new LobbyCommand
                    {
                        Kind = LobbyCommandKind.SelectVehicle,
                        Text = "t90m"
                    }), Is.True);
                    host.Pump();
                    client.Pump();
                    Assert.That(
                        Player(client.State, "guest").VehicleSpecId,
                        Is.EqualTo("t90m"));

                    Assert.That(client.Submit(new LobbyCommand
                    {
                        Kind = LobbyCommandKind.SetMap,
                        Text = "cinder"
                    }), Is.True);
                    host.Pump();
                    client.Pump();
                    Assert.That(client.LastErrorCode, Is.EqualTo("host_only"));

                    room.SelectVehicle("host", "m1a2");
                    room.SetReady("host", true);
                    room.SetReady("guest", true);
                    RoomMatchPlan plan = room.Start("host", 991u);
                    Assert.That(host.SendMatchStart(plan), Is.True);
                    client.Pump();
                    host.Pump();
                    Assert.That(ready, Is.True);
                    Assert.That(client.MatchPlan.Seed, Is.EqualTo(991u));
                }
            }
        }

        private static AuthoritativeRoom Room()
        {
            return new AuthoritativeRoom(
                "ABC123",
                "host",
                "Host",
                null,
                teamSize: 2,
                vehicleAllowed: id => id == "m1a2" || id == "t90m",
                mapAllowed: id => id == "random" || id == "cinder",
                camoAllowed: id => id == "factory" || id == "winter");
        }

        private static RoomPlayerSnapshot Player(
            RoomStateSnapshot state,
            string playerId)
        {
            for (int i = 0; i < state.Players.Length; i++)
            {
                if (state.Players[i].PlayerId == playerId)
                    return state.Players[i];
            }
            return null;
        }
    }
}
