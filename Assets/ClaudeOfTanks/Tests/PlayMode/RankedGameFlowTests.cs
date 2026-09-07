using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Server;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    public sealed class RankedGameFlowTests
    {
        [UnityTest]
        public IEnumerator RankedQueueHandsTwoClientsToDedicatedBattle()
        {
            GameObject alphaRoot = null;
            GameObject bravoRoot = null;
            int identitySequence = 0;
            int identityToken = 0;
            int queueSequence = 0;
            int queueToken = 0;
            int matchToken = 0;
            try
            {
                using (RankedRatingStore ratings =
                new RankedRatingStore(
                    identityFactory: () =>
                        "r_unity_player_" +
                        (++identitySequence).ToString("D4"),
                    tokenFactory: () =>
                        "identity_token_" +
                        (++identityToken).ToString("D12")))
            using (DedicatedMatchRegistry registry =
                new DedicatedMatchRegistry(
                    tokenFactory: () =>
                        "match_token_" +
                        (++matchToken).ToString("D16")))
            {
                ContentCatalog catalog = ContentCatalog.Load();
                DedicatedServerMatchFactory factory =
                    new DedicatedServerMatchFactory(catalog);
                using (RankedMatchmaker matchmaker =
                    new RankedMatchmaker(
                        ratings,
                        registry,
                        factory.Create,
                        new[] { "verdant" },
                        factory.IsVehicleAllowed,
                        () => "queue_unity_" +
                            (++queueSequence).ToString("D4"),
                        () => "queue_token_" +
                            (++queueToken).ToString("D16")))
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
                            () => DateTimeOffset.UtcNow
                                .ToUnixTimeMilliseconds(),
                            httpHandler: api))
                    {
                        service.Start();
                        alphaRoot = new GameObject(
                            "RankedAlphaGameFlow");
                        bravoRoot = new GameObject(
                            "RankedBravoGameFlow");
                        GameFlowController alpha =
                            alphaRoot.AddComponent<GameFlowController>();
                        GameFlowController bravo =
                            bravoRoot.AddComponent<GameFlowController>();
                        alpha.Ranked.ConfigureIdentityStore(
                            new MemoryIdentityStore());
                        bravo.Ranked.ConfigureIdentityStore(
                            new MemoryIdentityStore());
                        alpha.Select(
                            5,
                            0,
                            GameModeId.CaptureTheFlag);
                        bravo.Select(
                            7,
                            1,
                            GameModeId.EndlessHorde);
                        string alphaVehicle =
                            alpha.SelectedVehicleId;
                        string bravoVehicle =
                            bravo.SelectedVehicleId;
                        string endpoint =
                            "http://127.0.0.1:" + port + "/";
                        alpha.RankedPanel.SetConnectionFields(
                            endpoint,
                            "Ranked Alpha",
                            1);
                        bravo.RankedPanel.SetConnectionFields(
                            endpoint,
                            "Ranked Bravo",
                            1);

                        Assert.That(
                            alpha.RankedPanel.FindMatch(),
                            Is.True);
                        Assert.That(
                            bravo.RankedPanel.FindMatch(),
                            Is.True);
                        yield return WaitUntil(
                            () =>
                                alpha.ActiveNetworkBattle != null &&
                                bravo.ActiveNetworkBattle != null,
                            service,
                            TimeSpan.FromSeconds(12),
                            () => "alpha=" + alpha.Ranked.State +
                                ":" + alpha.Ranked.LastException +
                                ", bravo=" + bravo.Ranked.State +
                                ":" + bravo.Ranked.LastException +
                                ", service=" +
                                service.LastConnectionError);
                        yield return WaitUntil(
                            () =>
                                alpha.ActiveNetworkBattle
                                    .LatestSnapshot != null &&
                                bravo.ActiveNetworkBattle
                                    .LatestSnapshot != null &&
                                alpha.ActiveNetworkBattle
                                    .LatestSnapshot.Tick >= 3,
                            service,
                            TimeSpan.FromSeconds(10));

                        Assert.That(
                            alpha.ActiveNetworkBattle.IsHost,
                            Is.False);
                        Assert.That(
                            bravo.ActiveNetworkBattle.IsHost,
                            Is.False);
                        Assert.That(
                            alpha.ActiveNetworkBattle.MapId,
                            Is.EqualTo("verdant"));
                        Assert.That(
                            alpha.ActiveNetworkBattle.GameMode,
                            Is.EqualTo(GameModeId.Standard));
                        Assert.That(
                            alpha.ActiveNetworkBattle
                                .VisibleTankCount,
                            Is.GreaterThanOrEqualTo(1));
                        Assert.That(
                            SnapshotHasVehicle(
                                alpha.ActiveNetworkBattle
                                    .LatestSnapshot,
                                alphaVehicle),
                            Is.True);
                        Assert.That(
                            SnapshotHasVehicle(
                                bravo.ActiveNetworkBattle
                                    .LatestSnapshot,
                                bravoVehicle),
                            Is.True);

                        alpha.ReturnToGarage();
                        bravo.ReturnToGarage();
                        yield return null;
                        Assert.That(alpha.IsGarageVisible, Is.True);
                        Assert.That(bravo.IsGarageVisible, Is.True);
                    }
                }
            }
            }
            finally
            {
                if (alphaRoot != null)
                    UnityEngine.Object.DestroyImmediate(alphaRoot);
                if (bravoRoot != null)
                    UnityEngine.Object.DestroyImmediate(bravoRoot);
            }
        }

        private static IEnumerator WaitUntil(
            Func<bool> condition,
            DedicatedMatchWebSocketService service,
            TimeSpan timeout,
            Func<string> details = null)
        {
            float deadline =
                Time.realtimeSinceStartup + (float)timeout.TotalSeconds;
            while (!condition())
            {
                service.Pump(1);
                if (Time.realtimeSinceStartup > deadline)
                    Assert.Fail(
                        "Timed out waiting for ranked game flow. " +
                        (details?.Invoke() ?? string.Empty));
                yield return null;
            }
        }

        private static bool SnapshotHasVehicle(
            NetworkWorldSnapshot snapshot,
            string vehicleId)
        {
            for (int i = 0; i < snapshot.Entities.Length; i++)
            {
                if (snapshot.Entities[i].VehicleSpecId == vehicleId)
                    return true;
            }
            return false;
        }

        private static int FreeTcpPort()
        {
            TcpListener listener =
                new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port =
                ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private sealed class MemoryIdentityStore :
            IRankedIdentityStore
        {
            private readonly Dictionary<string, RankedIdentity> _values =
                new Dictionary<string, RankedIdentity>();

            public RankedIdentity Load(string scope)
            {
                RankedIdentity value;
                return _values.TryGetValue(scope, out value)
                    ? value
                    : null;
            }

            public void Save(
                string scope,
                RankedIdentity identity)
            {
                _values[scope] = identity;
            }

            public void Clear(string scope)
            {
                _values.Remove(scope);
            }
        }
    }
}
