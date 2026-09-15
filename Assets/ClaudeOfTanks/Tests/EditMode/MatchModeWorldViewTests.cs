using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class MatchModeWorldViewTests
    {
        [Test]
        public void WorldViewShowsOnlyActiveObjectiveForMode()
        {
            MatchModeWorldView view =
                MatchModeWorldView.Create(null);
            try
            {
                MatchModeState flags =
                    new MatchModeState(
                        GameModeId.CaptureTheFlag);
                flags.AlphaFlag =
                    new Float3(-10f, 0f, 3f);
                flags.BravoFlag =
                    new Float3(10f, 0f, -3f);
                view.Sync(flags);
                Assert.That(
                    view.Root.Find("AlphaFlag")
                        .gameObject.activeSelf,
                    Is.True);
                Assert.That(
                    view.Root.Find("BravoFlag")
                        .gameObject.activeSelf,
                    Is.True);
                Assert.That(
                    view.Root.Find("TurboBall")
                        .gameObject.activeSelf,
                    Is.False);

                MatchModeState ball =
                    new MatchModeState(
                        GameModeId.TurboBall);
                ball.BallPosition =
                    new Float3(7f, 2.2f, -4f);
                view.Sync(ball);
                Assert.That(
                    view.Root.Find("AlphaFlag")
                        .gameObject.activeSelf,
                    Is.False);
                Assert.That(
                    view.Root.Find("TurboBall")
                        .gameObject.activeSelf,
                    Is.True);
                Assert.That(
                    view.Root.Find("TurboBall")
                        .position,
                    Is.EqualTo(
                        new Vector3(7f, 2.2f, -4f)));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void WorldViewColorsZonesAndPrunesPickups()
        {
            MatchModeWorldView view =
                MatchModeWorldView.Create(null);
            try
            {
                MatchModeState zones =
                    new MatchModeState(
                        GameModeId.ZoneControl);
                zones.Zones[0] = new Float3(-30f, 0f, 0f);
                zones.Zones[1] = Float3.Zero;
                zones.Zones[2] = new Float3(30f, 0f, 0f);
                zones.ZoneControl[2] = 1f;
                zones.ZoneOwners[2] = Team.Alpha;
                zones.Pickups.Add(
                    new ModePickup
                    {
                        Id = "h1",
                        Kind = "heal",
                        Position = new Float3(1f, 0f, 2f),
                        Active = true,
                        SpawnedWave = 2
                    });
                view.Sync(zones);
                Assert.That(
                    view.Root.Find("Zone-2")
                        .gameObject.activeSelf,
                    Is.True);
                Assert.That(
                    view.Root.Find("Pickup-h1"),
                    Is.Not.Null);

                zones.Pickups.Clear();
                view.Sync(zones);
                Assert.That(
                    view.Root.Find("Pickup-h1"),
                    Is.Null);

                view.Sync(
                    new MatchModeState(
                        GameModeId.Standard));
                Assert.That(
                    view.Root.Find("Zone-2")
                        .gameObject.activeSelf,
                    Is.False);
            }
            finally
            {
                view.Dispose();
            }
        }
    }
}
