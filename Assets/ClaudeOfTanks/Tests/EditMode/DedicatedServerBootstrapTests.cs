using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Server;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class DedicatedServerBootstrapTests
    {
        [Test]
        public void OptionsUseBoundedDefaultsAndCommandLineOverrides()
        {
            DedicatedServerOptions defaults = DedicatedServerOptions.Parse(
                Array.Empty<string>(),
                name => null);
            Assert.That(defaults.ListenPrefix, Is.EqualTo("http://127.0.0.1:18791/"));
            Assert.That(defaults.AllowedOrigins, Is.Empty);

            DedicatedServerOptions configured = DedicatedServerOptions.Parse(
                new[]
                {
                    "--cot-server",
                    "--cot-bind=0.0.0.0",
                    "--cot-port",
                    "19001",
                    "--cot-origins=https://game.example,http://127.0.0.1:5173",
                    "--cot-rating-file=Temp/test-ratings.bin"
                },
                name => null);
            Assert.That(configured.ListenPrefix, Is.EqualTo("http://0.0.0.0:19001/"));
            Assert.That(
                configured.AllowedOrigins,
                Is.EqualTo(new[]
                {
                    "https://game.example",
                    "http://127.0.0.1:5173"
                }));
            Assert.That(
                configured.RatingFile,
                Is.EqualTo(System.IO.Path.GetFullPath("Temp/test-ratings.bin")));
            Assert.That(
                DedicatedServerOptions.HasServerFlag(new[] { "--cot-server" }),
                Is.True);
        }

        [Test]
        public void MatchFactoryUsesCompleteGeneratedContent()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            DedicatedServerMatchFactory factory =
                new DedicatedServerMatchFactory(catalog);
            Assert.That(factory.MapRotation, Has.Length.EqualTo(20));
            Assert.That(factory.IsVehicleAllowed("m1a1"), Is.True);
            Assert.That(factory.IsVehicleAllowed("missing"), Is.False);

            RoomMatchPlan plan = new RoomMatchPlan
            {
                Round = 1,
                Seed = 17u,
                MapId = factory.MapRotation[0],
                GameMode = GameModeId.Standard,
                Seats = new[]
                {
                    Seat("alpha", "alpha-entity", Team.Alpha, "m1a1"),
                    Seat("bravo", "bravo-entity", Team.Bravo, "t90m")
                },
                SpectatorPlayerIds = Array.Empty<string>()
            };
            AuthoritativeMatchHost host = factory.Create(plan);
            NetworkWorldSnapshot alpha = host.CreateSnapshot("alpha");
            NetworkWorldSnapshot bravo = host.CreateSnapshot("bravo");
            Assert.That(alpha.ViewerEntityId, Is.EqualTo("alpha-entity"));
            Assert.That(bravo.ViewerEntityId, Is.EqualTo("bravo-entity"));
            Assert.That(
                Array.Exists(
                    alpha.Entities,
                    entity => entity.EntityId == "alpha-entity"),
                Is.True);
            Assert.That(
                Array.Exists(
                    bravo.Entities,
                    entity => entity.EntityId == "bravo-entity"),
                Is.True);
        }

        [Test]
        public void OptionsRejectInvalidPortsAndOrigins()
        {
            Assert.Throws<ArgumentException>(() =>
                DedicatedServerOptions.Parse(
                    new[] { "--cot-port=0" },
                    name => null));
            Assert.Throws<ArgumentException>(() =>
                DedicatedServerOptions.Parse(
                    new[] { "--cot-origins=file:///tmp/server" },
                    name => null));
            Assert.Throws<ArgumentException>(() =>
                DedicatedServerOptions.Parse(
                    new[] { "--cot-bind=public.example" },
                    name => null));
        }

        [Test]
        public void TickSchedulerUsesSixtyHertzAndBoundsCatchUp()
        {
            DedicatedServerTickScheduler scheduler = new DedicatedServerTickScheduler();
            int ticks = 0;
            for (int i = 0; i < 60; i++)
                ticks += scheduler.Consume(BattleState.FixedDeltaTime);
            Assert.That(ticks, Is.EqualTo(NetworkProtocol.TickRate));
            Assert.That(
                scheduler.Consume(1.0),
                Is.EqualTo(AuthoritativeMatchHost.MaximumCatchUpTicks));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                scheduler.Consume(double.NaN));
        }

        private static RoomMatchSeat Seat(
            string playerId,
            string entityId,
            Team team,
            string specId)
        {
            return new RoomMatchSeat
            {
                PlayerId = playerId,
                EntityId = entityId,
                DisplayName = playerId,
                Team = team,
                VehicleSpecId = specId,
                Equipment = Array.Empty<string>(),
                CamoId = "factory"
            };
        }
    }
}
