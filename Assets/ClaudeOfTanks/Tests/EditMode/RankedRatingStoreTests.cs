using System;
using System.IO;
using ClaudeOfTanks.Network;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class RankedRatingStoreTests
    {
        [Test]
        public void IdentityAuthenticationSettlementAndLeaderboardHideSecrets()
        {
            int identitySequence = 0;
            int token = 0;
            using (RankedRatingStore store = new RankedRatingStore(
                identityFactory: () => "r_player_identity_" + ++identitySequence,
                tokenFactory: () => "token_value_long_enough_" + ++token))
            {
                RatingIdentity alpha = store.CreateIdentity("Alpha");
                RatingIdentity bravo = store.CreateIdentity("Bravo");

                Assert.That(store.Authenticate(alpha.Profile.PlayerId, alpha.BearerToken), Is.True);
                Assert.That(store.Authenticate(alpha.Profile.PlayerId, "wrong"), Is.False);
                Assert.That(store.GetProfile(alpha.Profile.PlayerId), Has.No.Property("BearerToken"));

                RatingUpdate[] updates = store.Settle(
                    "match_001",
                    RatedResult.Alpha,
                    new[]
                    {
                        new RatedPlayer { PlayerId = alpha.Profile.PlayerId, Team = RoomTeam.Alpha },
                        new RatedPlayer { PlayerId = bravo.Profile.PlayerId, Team = RoomTeam.Bravo }
                    });

                Assert.That(updates, Has.Length.EqualTo(2));
                Assert.That(store.GetProfile(alpha.Profile.PlayerId).Rating, Is.GreaterThan(1000));
                Assert.That(store.GetProfile(bravo.Profile.PlayerId).Rating, Is.LessThan(1000));
                Assert.That(store.Settle(
                    "match_001",
                    RatedResult.Bravo,
                    new[]
                    {
                        new RatedPlayer { PlayerId = alpha.Profile.PlayerId, Team = RoomTeam.Alpha },
                        new RatedPlayer { PlayerId = bravo.Profile.PlayerId, Team = RoomTeam.Bravo }
                    }), Is.Null);
                Assert.That(store.Leaderboard(10)[0].Profile.PlayerId, Is.EqualTo(alpha.Profile.PlayerId));
            }
        }

        [Test]
        public void AtomicFilePersistsHashedIdentityAndIdempotency()
        {
            string directory = Path.Combine(Path.GetTempPath(), "cot-rating-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "ratings.bin");
            string playerId;
            string bearer;
            int identitySequence = 0;
            int token = 0;
            try
            {
                using (RankedRatingStore store = new RankedRatingStore(
                    path,
                    () => "r_persistent_player_" + ++identitySequence,
                    () => "persistent_bearer_token_value_" + ++token))
                {
                    RatingIdentity identity = store.CreateIdentity("Persistent");
                    playerId = identity.Profile.PlayerId;
                    bearer = identity.BearerToken;
                    RatingIdentity opponent = store.CreateIdentity("Opponent");
                    store.Settle(
                        "persisted_match",
                        RatedResult.Draw,
                        new[]
                        {
                            new RatedPlayer { PlayerId = playerId, Team = RoomTeam.Alpha },
                            new RatedPlayer
                            {
                                PlayerId = opponent.Profile.PlayerId,
                                Team = RoomTeam.Bravo
                            }
                        });
                }

                byte[] file = File.ReadAllBytes(path);
                string text = System.Text.Encoding.UTF8.GetString(file);
                Assert.That(text, Does.Not.Contain(bearer));

                using (RankedRatingStore reloaded = new RankedRatingStore(path))
                {
                    Assert.That(reloaded.Authenticate(playerId, bearer), Is.True);
                    Assert.That(reloaded.GetProfile(playerId).Matches, Is.EqualTo(1));
                    Assert.That(reloaded.SettledMatchCount, Is.EqualTo(1));
                }
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void CorruptPersistentStoreIsRejected()
        {
            string path = Path.Combine(Path.GetTempPath(), "cot-rating-corrupt-" +
                Guid.NewGuid().ToString("N") + ".bin");
            try
            {
                File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
                Assert.Throws<InvalidDataException>(() => new RankedRatingStore(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
