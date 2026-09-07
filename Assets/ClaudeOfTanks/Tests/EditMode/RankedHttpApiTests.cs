using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Server;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class RankedHttpApiTests
    {
        [Test]
        public void RealHttpServiceCreatesIdentityAndRankedMatchTicket()
        {
            int identitySequence = 0;
            int identityToken = 0;
            int queueSequence = 0;
            int queueToken = 0;
            int matchToken = 0;
            using (RankedRatingStore ratings = new RankedRatingStore(
                identityFactory: () =>
                    "r_http_player_" + (++identitySequence).ToString("D4"),
                tokenFactory: () =>
                    "identity_token_value_" + (++identityToken).ToString("D8")))
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry(
                tokenFactory: () =>
                    "match_token_value_" + (++matchToken).ToString("D8")))
            using (RankedMatchmaker matchmaker = new RankedMatchmaker(
                ratings,
                registry,
                Host,
                new[] { "verdant" },
                id => id == "m1a1" || id == "t90m",
                () => "queue_id_" + (++queueSequence).ToString("D4"),
                () => "queue_token_value_" + (++queueToken).ToString("D8")))
            using (RankedHttpApi api = new RankedHttpApi(
                ratings,
                matchmaker,
                registry))
            {
                int port = FreeTcpPort();
                using (DedicatedMatchWebSocketService service =
                    new DedicatedMatchWebSocketService(
                        registry,
                        "http://127.0.0.1:" + port + "/",
                        () => 1000,
                        new[] { "http://localhost:5173" },
                        api))
                {
                    service.Start();
                    IdentityDto alpha = Identity(port, "Alpha");
                    IdentityDto bravo = Identity(port, "Bravo");
                    QueueDto alphaQueue = Join(port, alpha, "m1a1");
                    QueueDto bravoQueue = Join(port, bravo, "t90m");

                    Assert.That(alpha.playerId, Does.StartWith("r_http_player_"));
                    Assert.That(alpha.token, Has.Length.GreaterThanOrEqualTo(24));
                    Assert.That(alphaQueue.status, Is.EqualTo("queued"));
                    Assert.That(bravoQueue.status, Is.EqualTo("matched"));

                    HttpResponse poll = Send(
                        port,
                        "GET",
                        "/ranked/queue/" + alphaQueue.ticketId,
                        null,
                        alphaQueue.ticketToken);
                    Assert.That(poll.Status, Is.EqualTo(200));
                    QueueDto matched = JsonUtility.FromJson<QueueDto>(poll.Body);
                    Assert.That(matched.status, Is.EqualTo("matched"));
                    Assert.That(matched.match.roster, Has.Length.EqualTo(2));
                    Assert.That(matched.match.token, Has.Length.GreaterThanOrEqualTo(24));
                    Assert.That(matched.match.mapId, Is.EqualTo("verdant"));
                    Assert.That(matched.match.round, Is.EqualTo(1));
                    Assert.That(matched.match.seed, Is.Not.Zero);
                    Assert.That(matched.match.mode, Is.EqualTo("standard"));
                    Assert.That(matched.match.teamSize, Is.EqualTo(1));
                    Assert.That(
                        matched.match.roster[0].entityId,
                        Is.Not.Empty);
                    Assert.That(
                        matched.match.roster[0].equipment,
                        Is.Not.Null);

                    HttpResponse profile = Send(
                        port,
                        "GET",
                        "/ranked/profile/" + alpha.playerId);
                    Assert.That(profile.Status, Is.EqualTo(200));
                    Assert.That(
                        JsonUtility.FromJson<ProfileDto>(profile.Body).playerId,
                        Is.EqualTo(alpha.playerId));

                    IdentityDto waiting = Identity(port, "Waiting");
                    QueueDto waitingQueue = Join(port, waiting, "m1a1", 7);
                    HttpResponse cancelled = Send(
                        port,
                        "DELETE",
                        "/ranked/queue/" + waitingQueue.ticketId,
                        null,
                        waitingQueue.ticketToken);
                    Assert.That(cancelled.Status, Is.EqualTo(200));
                    Assert.That(cancelled.Body, Does.Contain("\"cancelled\":true"));

                    HttpResponse board = Send(
                        port,
                        "GET",
                        "/ranked/leaderboard?limit=1");
                    Assert.That(board.Status, Is.EqualTo(200));
                    Assert.That(
                        JsonUtility.FromJson<LeaderboardDto>(board.Body).players,
                        Has.Length.EqualTo(1));

                    HttpResponse health = Send(port, "GET", "/healthz");
                    HealthDto healthBody = JsonUtility.FromJson<HealthDto>(health.Body);
                    Assert.That(health.Status, Is.EqualTo(200));
                    Assert.That(healthBody.ok, Is.True);
                    Assert.That(healthBody.matches, Is.EqualTo(1));
                    Assert.That(healthBody.queuedPlayers, Is.Zero);
                    Assert.That(healthBody.ratedMatches, Is.EqualTo(1));

                    HttpResponse preflight = Send(
                        port,
                        "OPTIONS",
                        "/ranked/queue",
                        null,
                        null,
                        "http://localhost:5173");
                    Assert.That(preflight.Status, Is.EqualTo(204));
                    Assert.That(
                        preflight.Headers,
                        Does.Contain("Access-Control-Allow-Methods"));

                    HttpResponse forbidden = Send(
                        port,
                        "GET",
                        "/healthz",
                        null,
                        null,
                        "https://forbidden.example");
                    Assert.That(forbidden.Status, Is.EqualTo(403));
                }
            }
        }

        [Test]
        public void HttpServiceReturnsBoundedAuthenticationErrors()
        {
            using (RankedRatingStore ratings = new RankedRatingStore())
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry())
            using (RankedMatchmaker matchmaker = new RankedMatchmaker(
                ratings,
                registry,
                Host,
                new[] { "verdant" },
                id => id == "m1a1"))
            using (RankedHttpApi api = new RankedHttpApi(
                ratings,
                matchmaker,
                registry))
            {
                DedicatedHttpResponse response = api.Handle(new DedicatedHttpRequest
                {
                    Method = "POST",
                    Target = "/ranked/queue",
                    Body = Encoding.UTF8.GetBytes(
                        "{\"playerId\":\"missing\",\"specId\":\"m1a1\",\"teamSize\":1}")
                });
                Assert.That(response.Status, Is.EqualTo(401));
                Assert.That(
                    Encoding.UTF8.GetString(response.Body),
                    Does.Contain("ranked_auth_failed"));

                for (int i = 0; i < 240; i++)
                {
                    Assert.That(api.Handle(new DedicatedHttpRequest
                    {
                        Method = "GET",
                        Target = "/healthz",
                        RemoteAddress = "rate-test"
                    }).Status, Is.EqualTo(200));
                }
                Assert.That(api.Handle(new DedicatedHttpRequest
                {
                    Method = "GET",
                    Target = "/healthz",
                    RemoteAddress = "rate-test"
                }).Status, Is.EqualTo(429));
            }
        }

        private static IdentityDto Identity(int port, string name)
        {
            HttpResponse response = Send(
                port,
                "POST",
                "/ranked/identity",
                "{\"name\":\"" + name + "\"}",
                null,
                "http://localhost:5173");
            Assert.That(response.Status, Is.EqualTo(201));
            Assert.That(
                response.Headers,
                Does.Contain("Access-Control-Allow-Origin: http://localhost:5173"));
            return JsonUtility.FromJson<IdentityDto>(response.Body);
        }

        private static QueueDto Join(
            int port,
            IdentityDto identity,
            string specId,
            int teamSize = 1)
        {
            HttpResponse response = Send(
                port,
                "POST",
                "/ranked/queue",
                "{\"playerId\":\"" + identity.playerId +
                "\",\"specId\":\"" + specId + "\",\"teamSize\":" +
                teamSize + "}",
                identity.token);
            Assert.That(response.Status, Is.EqualTo(202));
            return JsonUtility.FromJson<QueueDto>(response.Body);
        }

        private static HttpResponse Send(
            int port,
            string method,
            string path,
            string body = null,
            string bearer = null,
            string origin = null)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(IPAddress.Loopback, port);
                client.NoDelay = true;
                byte[] payload = body == null
                    ? Array.Empty<byte>()
                    : Encoding.UTF8.GetBytes(body);
                StringBuilder request = new StringBuilder()
                    .Append(method).Append(' ').Append(path).Append(" HTTP/1.1\r\n")
                    .Append("Host: 127.0.0.1\r\n")
                    .Append("Connection: close\r\n");
                if (body != null)
                    request.Append("Content-Type: application/json\r\n");
                if (!string.IsNullOrEmpty(bearer))
                    request.Append("Authorization: Bearer ").Append(bearer).Append("\r\n");
                if (!string.IsNullOrEmpty(origin))
                    request.Append("Origin: ").Append(origin).Append("\r\n");
                request.Append("Content-Length: ").Append(payload.Length).Append("\r\n\r\n");
                NetworkStream stream = client.GetStream();
                byte[] head = Encoding.ASCII.GetBytes(request.ToString());
                stream.Write(head, 0, head.Length);
                if (payload.Length > 0) stream.Write(payload, 0, payload.Length);
                stream.Flush();
                client.Client.Shutdown(SocketShutdown.Send);
                using (MemoryStream response = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        response.Write(buffer, 0, read);
                    string text = Encoding.UTF8.GetString(response.ToArray());
                    int split = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                    Assert.That(split, Is.GreaterThanOrEqualTo(0));
                    string headers = text.Substring(0, split);
                    string[] firstLine = headers.Split(new[] { "\r\n" }, StringSplitOptions.None)[0]
                        .Split(' ');
                    return new HttpResponse
                    {
                        Status = int.Parse(firstLine[1]),
                        Headers = headers,
                        Body = text.Substring(split + 4)
                    };
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

        private static AuthoritativeMatchHost Host(RoomMatchPlan plan)
        {
            BattleState state = new BattleState(new FlatHeightField(), plan.Seed);
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

        [Serializable] private sealed class IdentityDto
        {
            public string playerId;
            public string token;
        }
        [Serializable] private sealed class QueueDto
        {
            public string ticketId;
            public string ticketToken;
            public string status;
            public MatchDto match;
        }
        [Serializable] private sealed class MatchDto
        {
            public string token;
            public int round;
            public uint seed;
            public string mapId;
            public string mode;
            public int teamSize;
            public RosterDto[] roster;
        }
        [Serializable] private sealed class RosterDto
        {
            public string id;
            public string entityId;
            public string[] equipment;
        }
        [Serializable] private sealed class LeaderboardDto
        {
            public ProfileDto[] players;
        }
        [Serializable] private sealed class ProfileDto { public string playerId; }
        [Serializable] private sealed class HealthDto
        {
            public bool ok;
            public int matches;
            public int queuedPlayers;
            public int ratedMatches;
        }
        private sealed class HttpResponse
        {
            public int Status;
            public string Headers;
            public string Body;
        }
    }
}
