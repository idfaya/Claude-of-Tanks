using System;
using ClaudeOfTanks.Network;
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
                    "--cot-origins=https://game.example,http://127.0.0.1:5173"
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
                DedicatedServerOptions.HasServerFlag(new[] { "--cot-server" }),
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
    }
}
