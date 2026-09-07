using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class RankedMatchmakerTests
    {
        [Test]
        public void QueueAuthenticatesBalancesCreatesTicketsAndSettles()
        {
            int identitySequence = 0;
            int identityToken = 0;
            int queueSequence = 0;
            int queueToken = 0;
            int matchToken = 0;
            using (RankedRatingStore ratings = new RankedRatingStore(
                identityFactory: () => "r_match_player_" + (++identitySequence).ToString("D3"),
                tokenFactory: () => "identity_token_value_" + (++identityToken).ToString("D8")))
            using (DedicatedMatchRegistry registry = new DedicatedMatchRegistry(
                tokenFactory: () => "match_token_value_" + (++matchToken).ToString("D8")))
            using (RankedMatchmaker matchmaker = new RankedMatchmaker(
                ratings,
                registry,
                Host,
                new[] { "verdant", "cinder" },
                id => id == "m1a2" || id == "t90m",
                () => "queue_id_" + (++queueSequence).ToString("D3"),
                () => "queue_token_value_" + (++queueToken).ToString("D8")))
            {
                RatingIdentity[] identities = new RatingIdentity[4];
                RankedQueueJoin[] joins = new RankedQueueJoin[4];
                for (int i = 0; i < identities.Length; i++)
                {
                    identities[i] = ratings.CreateIdentity("Commander");
                    joins[i] = matchmaker.Join(
                        identities[i].Profile.PlayerId,
                        identities[i].BearerToken,
                        i % 2 == 0 ? "m1a2" : "t90m",
                        new[] { "rammer", "optics", "rammer", "extra" },
                        i % 2 == 0 ? "summer" : "winter",
                        2,
                        1000);
                }

                Assert.That(joins[0].Status, Is.EqualTo(RankedQueueStatus.Queued));
                Assert.That(joins[3].Status, Is.EqualTo(RankedQueueStatus.Matched));
                Assert.That(matchmaker.QueuedPlayerCount, Is.Zero);
                Assert.That(matchmaker.Poll(joins[0].QueueId, "wrong"), Is.Null);
                RankedQueueView view = matchmaker.Poll(
                    joins[0].QueueId, joins[0].QueueToken);
                Assert.That(view.Status, Is.EqualTo(RankedQueueStatus.Matched));
                Assert.That(view.Assignment.Roster, Has.Length.EqualTo(4));
                Assert.That(CountTeam(view.Assignment.Roster, Team.Alpha), Is.EqualTo(2));
                Assert.That(CountTeam(view.Assignment.Roster, Team.Bravo), Is.EqualTo(2));
                Assert.That(view.Assignment.Roster[0].Equipment, Has.Length.EqualTo(3));
                Assert.That(
                    UniqueNameCount(view.Assignment.Roster),
                    Is.EqualTo(view.Assignment.Roster.Length));

                DedicatedMatchAdmission admission = registry.Admit(
                    view.Assignment.MatchTicket.MatchId,
                    view.Assignment.MatchTicket.PlayerId,
                    view.Assignment.MatchTicket.TicketToken,
                    1001);
                Assert.That(admission.EntityId, Is.Not.Empty);

                RatingUpdate[] updates = matchmaker.Finish(
                    view.Assignment.MatchTicket.MatchId,
                    RatedResult.Alpha,
                    2000);
                Assert.That(updates, Has.Length.EqualTo(4));
                Assert.That(matchmaker.Poll(
                    joins[0].QueueId, joins[0].QueueToken).Status,
                    Is.EqualTo(RankedQueueStatus.Finished));
                Assert.That(matchmaker.Finish(
                    view.Assignment.MatchTicket.MatchId,
                    RatedResult.Bravo,
                    2001), Is.Null);
                matchmaker.Pump(2000 + RankedMatchmaker.ResultLifetimeMs + 1);
                Assert.That(matchmaker.Poll(
                    joins[0].QueueId, joins[0].QueueToken), Is.Null);
            }
        }

        [Test]
        public void SearchBandExpandsButRemainsBounded()
        {
            Assert.That(RankedMatchmaker.SearchBand(0), Is.EqualTo(150));
            Assert.That(RankedMatchmaker.SearchBand(60000), Is.EqualTo(200));
            Assert.That(RankedMatchmaker.SearchBand(600000), Is.EqualTo(600));
            Assert.That(RankedMatchmaker.SearchBand(long.MaxValue), Is.EqualTo(600));
        }

        private static int CountTeam(RoomMatchSeat[] roster, Team team)
        {
            int count = 0;
            for (int i = 0; i < roster.Length; i++)
                if (roster[i].Team == team) count++;
            return count;
        }

        private static int UniqueNameCount(RoomMatchSeat[] roster)
        {
            System.Collections.Generic.HashSet<string> names =
                new System.Collections.Generic.HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < roster.Length; i++) names.Add(roster[i].DisplayName);
            return names.Count;
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
    }
}
