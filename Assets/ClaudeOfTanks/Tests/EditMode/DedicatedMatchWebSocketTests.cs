using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class DedicatedMatchWebSocketTests
    {
        [Test]
        public void DedicatedHandshakeCodecRoundTripsAndRejectsTrailingData()
        {
            DedicatedSocketAuthRequest request = new DedicatedSocketAuthRequest
            {
                Kind = DedicatedSocketAuthKind.Reconnect,
                MatchId = "match_socket",
                PlayerId = "alpha-player",
                Token = "dedicated_token_value_00000001"
            };
            byte[] requestPacket = DedicatedSocketProtocol.EncodeRequest(request);
            DedicatedSocketAuthRequest decodedRequest =
                DedicatedSocketProtocol.DecodeRequest(requestPacket);
            Assert.That(decodedRequest.Kind, Is.EqualTo(request.Kind));
            Assert.That(decodedRequest.MatchId, Is.EqualTo(request.MatchId));
            Assert.That(decodedRequest.PlayerId, Is.EqualTo(request.PlayerId));
            Assert.That(decodedRequest.Token, Is.EqualTo(request.Token));

            DedicatedSocketAuthResponse response = new DedicatedSocketAuthResponse
            {
                MatchId = request.MatchId,
                PlayerId = request.PlayerId,
                EntityId = "alpha-entity",
                SessionToken = "dedicated_session_value_000001",
                ConnectionGeneration = 3
            };
            byte[] responsePacket = DedicatedSocketProtocol.EncodeResponse(response);
            DedicatedSocketAuthResponse decodedResponse =
                DedicatedSocketProtocol.DecodeResponse(responsePacket);
            Assert.That(decodedResponse.EntityId, Is.EqualTo(response.EntityId));
            Assert.That(decodedResponse.SessionToken, Is.EqualTo(response.SessionToken));
            Assert.That(decodedResponse.ConnectionGeneration, Is.EqualTo(3));

            Array.Resize(ref responsePacket, responsePacket.Length + 1);
            Assert.Throws<FormatException>(() =>
                DedicatedSocketProtocol.DecodeResponse(responsePacket));
        }

        [Test]
        public void DedicatedServiceAuthenticatesAndRunsAuthoritativeMatch()
        {
            int tokenSequence = 0;
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry(
                () => "match_socket",
                () => "dedicated_token_value_" + (++tokenSequence).ToString("D8")))
            {
                RoomMatchPlan plan = Plan();
                BattleState battleState;
                AuthoritativeMatchHost host = Host(plan, out battleState);
                DedicatedMatchTicket[] tickets =
                    registry.CreateMatch(plan, host, 1000, "match_socket");
                int port = FreeTcpPort();
                string prefix = "http://127.0.0.1:" + port + "/";
                using (DedicatedMatchWebSocketService service =
                    new DedicatedMatchWebSocketService(registry, prefix, () => 1000))
                {
                    service.Start();
                    string alphaSessionToken;
                    DedicatedSocketConnection alphaConnection =
                        WebSocketNetworkEndpoint.ConnectDedicatedAsync(
                            new Uri("ws://127.0.0.1:" + port + "/match"),
                            new DedicatedSocketAuthRequest
                            {
                                Kind = DedicatedSocketAuthKind.Ticket,
                                MatchId = tickets[0].MatchId,
                                PlayerId = tickets[0].PlayerId,
                                Token = tickets[0].TicketToken
                            }).GetAwaiter().GetResult();
                    DedicatedSocketConnection bravoConnection =
                        WebSocketNetworkEndpoint.ConnectDedicatedAsync(
                            new Uri("ws://127.0.0.1:" + port + "/match"),
                            new DedicatedSocketAuthRequest
                            {
                                Kind = DedicatedSocketAuthKind.Ticket,
                                MatchId = tickets[1].MatchId,
                                PlayerId = tickets[1].PlayerId,
                                Token = tickets[1].TicketToken
                            }).GetAwaiter().GetResult();
                    using (alphaConnection)
                    using (bravoConnection)
                    using (NetworkClientPump alphaClient = new NetworkClientPump(
                        alphaConnection.Admission.PlayerId,
                        alphaConnection.Admission.EntityId,
                        alphaConnection.Transport))
                    using (NetworkClientPump bravoClient = new NetworkClientPump(
                        bravoConnection.Admission.PlayerId,
                        bravoConnection.Admission.EntityId,
                        bravoConnection.Transport))
                    {
                        Assert.That(alphaConnection.Admission.ConnectionGeneration, Is.EqualTo(1));
                        Assert.That(alphaConnection.Admission.SessionToken, Is.Not.Empty);
                        alphaSessionToken = alphaConnection.Admission.SessionToken;
                        Assert.That(bravoConnection.Admission.ConnectionGeneration, Is.EqualTo(1));
                        NetworkInputCommand input = new NetworkInputCommand
                        {
                            Sequence = 1u,
                            ClientTick = 0,
                            SnapshotAckTick = -1,
                            Throttle = 1f,
                            AimDistanceM = 100f
                        };
                        Assert.That(alphaClient.SendInput(input), Is.True);

                        bool received = false;
                        int ticksRequested = 0;
                        for (int i = 0; i < 400 && !received; i++)
                        {
                            service.Pump(1);
                            ticksRequested++;
                            alphaClient.Pump();
                            bravoClient.Pump();
                            received = alphaClient.LatestSnapshot != null &&
                                bravoClient.LatestSnapshot != null &&
                                host.Tick >= 3 &&
                                host.CreateSnapshot(tickets[0].PlayerId)
                                    .Entities[0].Position.Z > -100f;
                            if (!received) Thread.Sleep(2);
                        }

                        Assert.That(received, Is.True);
                        Assert.That(host.Tick, Is.EqualTo(ticksRequested));
                        Assert.That(alphaClient.LatestSnapshot.ViewerEntityId, Is.EqualTo("alpha-entity"));
                        Assert.That(bravoClient.LatestSnapshot.ViewerEntityId, Is.EqualTo("bravo-entity"));
                        Assert.That(service.ActiveConnectionCount, Is.EqualTo(2));
                        Assert.That(
                            battleState.DamageStaticObstacle(0, 10000f),
                            Is.True);
                    }

                    bool disconnected = false;
                    for (int i = 0; i < 200 && !disconnected; i++)
                    {
                        service.Pump(0);
                        disconnected = registry.Get("match_socket").ConnectedPlayerCount == 0;
                        if (!disconnected) Thread.Sleep(2);
                    }
                    Assert.That(disconnected, Is.True);
                    long tickBeforeReconnect = host.Tick;
                    service.Pump(3);
                    Assert.That(host.Tick, Is.EqualTo(tickBeforeReconnect + 3),
                        "a started dedicated match must not pause during reconnect");

                    DedicatedSocketConnection resumed =
                        WebSocketNetworkEndpoint.ConnectDedicatedAsync(
                            new Uri("ws://127.0.0.1:" + port + "/match"),
                            new DedicatedSocketAuthRequest
                            {
                                Kind = DedicatedSocketAuthKind.Reconnect,
                                MatchId = tickets[0].MatchId,
                                PlayerId = tickets[0].PlayerId,
                                Token = alphaSessionToken
                            }).GetAwaiter().GetResult();
                    using (DedicatedNetworkClientRuntime resumedClient =
                        new DedicatedNetworkClientRuntime(
                            new Uri(
                                "ws://127.0.0.1:" +
                                port +
                                "/match"),
                            resumed))
                    {
                        Assert.That(resumed.Admission.ConnectionGeneration, Is.EqualTo(2));
                        Assert.That(resumed.Admission.SessionToken, Is.Not.EqualTo(alphaSessionToken));
                        bool recoveredStructureState = false;
                        for (int i = 0; i < 200 && !recoveredStructureState; i++)
                        {
                            service.Pump(1);
                            resumedClient.Pump();
                            recoveredStructureState =
                                resumedClient.LatestSnapshot != null &&
                                resumedClient.LatestSnapshot.StaticObstacleRevision == 1u &&
                                resumedClient.LatestSnapshot.DestroyedStaticObstacleIndices.Length == 1 &&
                                resumedClient.LatestSnapshot.DestroyedStaticObstacleIndices[0] == 0;
                            if (!recoveredStructureState) Thread.Sleep(2);
                        }
                        Assert.That(registry.Get("match_socket").ConnectedPlayerCount, Is.EqualTo(1));
                        Assert.That(recoveredStructureState, Is.True);

                        resumed.Transport.Close("test_reconnect");
                        bool automaticallyReconnected = false;
                        for (int i = 0; i < 500 && !automaticallyReconnected; i++)
                        {
                            service.Pump(1);
                            resumedClient.Pump();
                            automaticallyReconnected =
                                resumedClient.IsConnected &&
                                resumedClient.ConnectionGeneration == 3 &&
                                resumedClient.LatestSnapshot != null &&
                                resumedClient.LatestSnapshot.Tick > tickBeforeReconnect;
                            if (!automaticallyReconnected) Thread.Sleep(2);
                        }
                        Assert.That(automaticallyReconnected, Is.True);
                        Assert.That(resumedClient.LastError, Is.Null);
                    }
                }
            }
        }

        private static int FreeTcpPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static RoomMatchPlan Plan()
        {
            return new RoomMatchPlan
            {
                Round = 1,
                Seed = 77u,
                MapId = "verdant",
                GameMode = GameModeId.Standard,
                Seats = new[]
                {
                    new RoomMatchSeat
                    {
                        PlayerId = "alpha-player",
                        EntityId = "alpha-entity",
                        Team = Team.Alpha,
                        VehicleSpecId = "medium",
                        Equipment = Array.Empty<string>(),
                        CamoId = "factory"
                    },
                    new RoomMatchSeat
                    {
                        PlayerId = "bravo-player",
                        EntityId = "bravo-entity",
                        Team = Team.Bravo,
                        VehicleSpecId = "medium",
                        Equipment = Array.Empty<string>(),
                        CamoId = "factory"
                    }
                },
                SpectatorPlayerIds = Array.Empty<string>()
            };
        }

        private static AuthoritativeMatchHost Host(
            RoomMatchPlan plan,
            out BattleState state)
        {
            state = new BattleState(
                new FlatHeightField(),
                plan.Seed,
                500f,
                new[]
                {
                    new StaticObstacle(
                        "reconnect-cover",
                        new Float3(30f, 0f, 0f),
                        2f,
                        2f,
                        3f,
                        0f,
                        StaticObstacleFlags.All,
                        true)
                });
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                RoomMatchSeat seat = plan.Seats[i];
                state.Tanks.Add(new TankState(
                    seat.EntityId,
                    seat.Team,
                    TankSpec.Medium(),
                    new Float3(i * 8f, 0f, seat.Team == Team.Alpha ? -100f : 100f),
                    seat.Team == Team.Alpha ? 0f : MathUtil.Pi));
            }
            AuthoritativeMatchHost host =
                new AuthoritativeMatchHost(new BattleSimulation(state));
            for (int i = 0; i < plan.Seats.Length; i++)
                host.RegisterPlayer(plan.Seats[i].PlayerId, plan.Seats[i].EntityId);
            return host;
        }
    }
}
