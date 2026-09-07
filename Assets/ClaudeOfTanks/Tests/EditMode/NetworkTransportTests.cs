using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
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

        [Test]
        public void WebSocketLaneCodecRejectsInvalidFrames()
        {
            byte[] frame = WebSocketLaneCodec.Encode(
                NetworkTransportLane.Control,
                new byte[] { 7, 8, 9 });
            NetworkTransportLane lane;
            Assert.That(WebSocketLaneCodec.Decode(frame, out lane), Is.EqualTo(new byte[] { 7, 8, 9 }));
            Assert.That(lane, Is.EqualTo(NetworkTransportLane.Control));

            frame[3] = 99;
            Assert.Throws<FormatException>(() => WebSocketLaneCodec.Decode(frame, out lane));
            Assert.Throws<FormatException>(() =>
                WebSocketLaneCodec.Encode((NetworkTransportLane)99, new byte[] { 1 }));
        }

        [Test]
        public void WebSocketTransportPreservesControlAndCoalescesState()
        {
            FakeWebSocketPair sockets = FakeWebSocketPair.Create();
            using (WebSocketNetworkEndpoint client =
                WebSocketNetworkEndpoint.Attach(sockets.Left))
            using (WebSocketNetworkEndpoint host =
                WebSocketNetworkEndpoint.Attach(sockets.Right))
            {
                List<byte> controls = new List<byte>();
                byte state = 0;
                host.ControlReceived += packet => controls.Add(packet[0]);
                client.StateReceived += packet => state = packet[0];

                Assert.That(client.SendControl(new byte[] { 1 }), Is.True);
                Assert.That(client.SendControl(new byte[] { 2 }), Is.True);
                Assert.That(host.SendState(new byte[] { 4 }), Is.True);
                Assert.That(host.SendState(new byte[] { 5 }), Is.True);

                Assert.That(SpinWait.SpinUntil(
                    () => client.Stats.ControlSent >= 2 &&
                        host.Stats.StateCoalesced + client.Stats.StateCoalesced >= 1,
                    2000), Is.True);
                Assert.That(SpinWait.SpinUntil(
                    () =>
                    {
                        host.Pump();
                        client.Pump();
                        return controls.Count == 2 && state == 5;
                    },
                    2000), Is.True);
                Assert.That(controls, Is.EqualTo(new byte[] { 1, 2 }));
                Assert.That(state, Is.EqualTo(5));
                Assert.That(
                    host.Stats.StateCoalesced + client.Stats.StateCoalesced,
                    Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void WebSocketTransportRejectsMalformedBinaryFrame()
        {
            FakeWebSocketPair sockets = FakeWebSocketPair.Create();
            using (sockets.Left)
            using (WebSocketNetworkEndpoint endpoint =
                WebSocketNetworkEndpoint.Attach(sockets.Right))
            {
                string reason = null;
                endpoint.Closed += value => reason = value;
                sockets.Left.SendAsync(
                    new ArraySegment<byte>(new byte[] { 1, 2, 3 }),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None).GetAwaiter().GetResult();

                Assert.That(SpinWait.SpinUntil(() => !endpoint.IsOpen, 2000), Is.True);
                Assert.That(reason, Is.Null);
                endpoint.Pump();
                Assert.That(reason, Is.EqualTo("invalid_frame"));
                Assert.That(endpoint.Stats.InvalidFrames, Is.EqualTo(1));
            }
        }

        [Test]
        public void HostAndClientPumpsOperateOverWebSocketTransport()
        {
            BattleState state = new BattleState(new FlatHeightField(), 502u);
            TankState player = new TankState(
                "socket-entity", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            state.Tanks.Add(player);
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            host.RegisterPlayer("socket-peer", player.Id);
            FakeWebSocketPair sockets = FakeWebSocketPair.Create();

            using (WebSocketNetworkEndpoint clientSocket =
                WebSocketNetworkEndpoint.Attach(sockets.Left))
            using (WebSocketNetworkEndpoint hostSocket =
                WebSocketNetworkEndpoint.Attach(sockets.Right))
            using (AuthoritativeHostPump hostPump =
                new AuthoritativeHostPump(host, "socket-peer", hostSocket))
            using (NetworkClientPump clientPump =
                new NetworkClientPump("socket-peer", player.Id, clientSocket))
            {
                NetworkInputCommand command = Command(1u, 0);
                command.PlayerId = "socket-peer";
                Assert.That(clientPump.SendInput(command), Is.True);

                bool completed = false;
                for (int i = 0; i < 400 && !completed; i++)
                {
                    hostPump.Update(1);
                    clientPump.Pump();
                    completed = clientPump.LatestSnapshot != null &&
                        player.Position.Z > 0f;
                    if (!completed) Thread.Sleep(2);
                }

                Assert.That(completed, Is.True);
                Assert.That(clientPump.LatestSnapshot.Tick, Is.GreaterThanOrEqualTo(3));
                Assert.That(hostPump.KeyframesSent, Is.GreaterThanOrEqualTo(1));
                Assert.That(clientPump.SnapshotsAccepted, Is.GreaterThanOrEqualTo(1));
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

        private sealed class FakeWebSocketPair
        {
            private FakeWebSocketPair(
                FakeWebSocketConnection left,
                FakeWebSocketConnection right)
            {
                Left = left;
                Right = right;
            }

            public FakeWebSocketConnection Left { get; }
            public FakeWebSocketConnection Right { get; }

            public static FakeWebSocketPair Create()
            {
                FakeWebSocketConnection left = new FakeWebSocketConnection();
                FakeWebSocketConnection right = new FakeWebSocketConnection();
                left.Peer = right;
                right.Peer = left;
                return new FakeWebSocketPair(left, right);
            }
        }

        private sealed class FakeWebSocketConnection : IWebSocketConnection
        {
            private readonly object _gate = new object();
            private readonly Queue<FakeMessage> _messages = new Queue<FakeMessage>();
            private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
            private WebSocketState _state = WebSocketState.Open;

            public FakeWebSocketConnection Peer { private get; set; }
            public WebSocketState State
            {
                get
                {
                    lock (_gate) return _state;
                }
            }

            public Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task SendAsync(
                ArraySegment<byte> payload,
                WebSocketMessageType messageType,
                bool endOfMessage,
                CancellationToken cancellationToken)
            {
                if (State != WebSocketState.Open)
                    throw new InvalidOperationException("Fake socket is closed.");
                byte[] copy = new byte[payload.Count];
                Buffer.BlockCopy(payload.Array, payload.Offset, copy, 0, payload.Count);
                Peer.Enqueue(new FakeMessage(copy, messageType));
                return Task.CompletedTask;
            }

            public async Task<WebSocketReceiveResult> ReceiveAsync(
                ArraySegment<byte> payload,
                CancellationToken cancellationToken)
            {
                await _signal.WaitAsync(cancellationToken);
                FakeMessage message;
                lock (_gate) message = _messages.Dequeue();
                if (message.Type == WebSocketMessageType.Close)
                {
                    return new WebSocketReceiveResult(
                        0,
                        WebSocketMessageType.Close,
                        true,
                        WebSocketCloseStatus.NormalClosure,
                        "closed");
                }
                if (payload.Count < message.Data.Length)
                    throw new InvalidOperationException("Fake receive buffer is too small.");
                Buffer.BlockCopy(message.Data, 0, payload.Array, payload.Offset, message.Data.Length);
                return new WebSocketReceiveResult(message.Data.Length, message.Type, true);
            }

            public Task CloseAsync(
                WebSocketCloseStatus status,
                string reason,
                CancellationToken cancellationToken)
            {
                CloseLocal();
                Peer.Enqueue(new FakeMessage(Array.Empty<byte>(), WebSocketMessageType.Close));
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                CloseLocal();
                _signal.Dispose();
            }

            private void Enqueue(FakeMessage message)
            {
                lock (_gate)
                {
                    if (_state != WebSocketState.Open &&
                        message.Type != WebSocketMessageType.Close)
                    {
                        return;
                    }
                    _messages.Enqueue(message);
                }
                _signal.Release();
            }

            private void CloseLocal()
            {
                lock (_gate)
                {
                    if (_state == WebSocketState.Closed) return;
                    _state = WebSocketState.Closed;
                }
            }
        }

        private sealed class FakeMessage
        {
            public FakeMessage(byte[] data, WebSocketMessageType type)
            {
                Data = data;
                Type = type;
            }

            public byte[] Data { get; }
            public WebSocketMessageType Type { get; }
        }
    }
}
