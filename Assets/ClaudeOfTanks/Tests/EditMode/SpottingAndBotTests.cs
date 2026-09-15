using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class SpottingAndBotTests
    {
        [Test]
        public void SpottingRejectsTargetsOutsideViewRange()
        {
            SpottingSimulation spotting = new SpottingSimulation();
            TankState spotter = Tank("spotter", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank(
                "target",
                Team.Bravo,
                new Float3(0f, 0f, SpottingSimulation.DefaultViewRangeM + 1f),
                MathUtil.Pi);

            Assert.That(spotting.CanSpot(spotter, target), Is.False);
        }

        [Test]
        public void SpottingUsesHorizontalFieldOfView()
        {
            SpottingSimulation spotting = new SpottingSimulation(445f, 90f);
            TankState spotter = Tank("spotter", Team.Alpha, Float3.Zero, 0f);
            TankState ahead = Tank("ahead", Team.Bravo, new Float3(0f, 0f, 100f), MathUtil.Pi);
            TankState behind = Tank("behind", Team.Bravo, new Float3(0f, 0f, -100f), 0f);

            Assert.That(spotting.CanSpot(spotter, ahead), Is.True);
            Assert.That(spotting.CanSpot(spotter, behind), Is.False);
        }

        [Test]
        public void SpottingRequiresClearLineOfSightOutsideProximityRange()
        {
            SpottingSimulation spotting = new SpottingSimulation();
            TankState spotter = Tank("spotter", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank("target", Team.Bravo, new Float3(0f, 0f, 100f), MathUtil.Pi);

            Assert.That(spotting.CanSpot(spotter, target, (origin, end) => true), Is.False);
            Assert.That(spotting.CanSpot(spotter, target, (origin, end) => false), Is.True);
        }

        [Test]
        public void BattleStateStructuresOccludeSpotting()
        {
            BattleState state = new BattleState(
                new FlatHeightField(),
                17u,
                500f,
                new[]
                {
                    new StaticObstacle(
                        "building",
                        new Float3(0f, 0f, 80f),
                        10f,
                        10f,
                        12f,
                        0.2f,
                        StaticObstacleFlags.All)
                });
            TankState spotter = Tank("spotter", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank(
                "target", Team.Bravo, new Float3(0f, 0f, 160f), MathUtil.Pi);
            SpottingSimulation spotting = new SpottingSimulation();

            Assert.That(
                spotting.CanSpot(spotter, target, state.IsVisionOccluded),
                Is.False);
        }

        [Test]
        public void ProximitySpottingIgnoresViewAndOcclusion()
        {
            SpottingSimulation spotting = new SpottingSimulation(445f, 30f);
            TankState spotter = Tank("spotter", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank("target", Team.Bravo, new Float3(0f, 0f, -49f), 0f);

            Assert.That(spotting.CanSpot(spotter, target, (origin, end) => true), Is.True);
        }

        [Test]
        public void OpticsAndStationaryBinocularsExtendEffectiveViewRange()
        {
            SpottingSimulation spotting = new SpottingSimulation();
            TankState target = Tank(
                "target",
                Team.Bravo,
                new Float3(0f, 0f, 315f),
                MathUtil.Pi);
            TankState baseline = Tank(
                "baseline",
                Team.Alpha,
                Float3.Zero,
                0f);
            TankState optics = Tank(
                "optics",
                Team.Alpha,
                Float3.Zero,
                0f);
            TankState binoculars = Tank(
                "binoculars",
                Team.Alpha,
                Float3.Zero,
                0f);
            LoadoutSimulation.ApplyEquipment(optics, new[] { "optics" });
            LoadoutSimulation.ApplyEquipment(
                binoculars,
                new[] { "binoculars" });

            Assert.That(spotting.CanSpot(baseline, target), Is.False);
            Assert.That(spotting.CanSpot(optics, target), Is.True);
            Assert.That(spotting.CanSpot(binoculars, target), Is.True);

            binoculars.SpeedMps = 1f;
            Assert.That(spotting.CanSpot(binoculars, target), Is.False);
        }

        [Test]
        public void VentsAndStationaryCamouflageNetReduceDetectionRange()
        {
            SpottingSimulation spotting = new SpottingSimulation();
            TankState spotter = Tank(
                "spotter",
                Team.Alpha,
                Float3.Zero,
                0f);
            TankState baseline = Tank(
                "baseline",
                Team.Bravo,
                new Float3(0f, 0f, 292f),
                MathUtil.Pi);
            TankState concealed = Tank(
                "concealed",
                Team.Bravo,
                baseline.Position,
                MathUtil.Pi);
            LoadoutSimulation.ApplyEquipment(
                concealed,
                new[] { "vents", "camo_net" });

            Assert.That(spotting.CanSpot(spotter, baseline), Is.True);
            Assert.That(spotting.CanSpot(spotter, concealed), Is.False);

            concealed.SpeedMps = 1f;
            Assert.That(spotting.CanSpot(spotter, concealed), Is.True);
        }

        [Test]
        public void FoliageConcealsUntilNearbyFiringMakesItTransparent()
        {
            BattleState state = new BattleState(
                new FlatHeightField(),
                18u,
                500f,
                new[]
                {
                    Tree("tree-a", 265f),
                    Tree("tree-b", 270f),
                    Tree("tree-c", 275f)
                });
            TankState spotter = Tank(
                "spotter",
                Team.Alpha,
                Float3.Zero,
                0f);
            TankState target = Tank(
                "target",
                Team.Bravo,
                new Float3(0f, 0f, 280f),
                MathUtil.Pi);
            SpottingSimulation spotting =
                new SpottingSimulation();
            float bush =
                state.ConcealmentBonusBetween(
                    spotter.Position,
                    target.Position,
                    false);

            Assert.That(bush, Is.GreaterThan(0.2f));
            Assert.That(
                spotting.CanSpot(
                    spotter,
                    target,
                    state.IsVisionOccluded,
                    10f,
                    bush),
                Is.False);

            target.LastFiredAtS = 10f;
            target.FireCamouflageLoss =
                SpottingSimulation
                    .FireCamouflageLossFor(120f);
            float firingBush =
                state.ConcealmentBonusBetween(
                    spotter.Position,
                    target.Position,
                    true);
            Assert.That(firingBush, Is.Zero);
            Assert.That(
                spotting.CanSpot(
                    spotter,
                    target,
                    state.IsVisionOccluded,
                    10f,
                    firingBush),
                Is.True);
        }

        [Test]
        public void BotSelectsNearestVisibleEnemyWithStableTieBreak()
        {
            TankState bot = Tank("bot", Team.Alpha, Float3.Zero, 0f);
            TankState zulu = Tank("zulu", Team.Bravo, new Float3(0f, 0f, 90f), MathUtil.Pi);
            TankState alpha = Tank("alpha", Team.Bravo, new Float3(0f, 0f, 90f), MathUtil.Pi);
            TankState hidden = Tank("hidden", Team.Bravo, new Float3(0f, 0f, 60f), MathUtil.Pi);
            BotController controller = new BotController(
                new SpottingSimulation(),
                (origin, end) => end.Z < 80f);

            TankState selected = controller.SelectTarget(
                bot,
                new List<TankState> { zulu, hidden, alpha });

            Assert.That(selected, Is.SameAs(alpha));
        }

        [Test]
        public void BotChasesAndTurnsTowardEnemy()
        {
            TankState bot = Tank("bot", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank("target", Team.Bravo, new Float3(80f, 0f, 80f), MathUtil.Pi);
            BotController controller = new BotController(new SpottingSimulation());

            TankInput input = controller.Decide(bot, new List<TankState> { bot, target });

            Assert.That(input.Throttle, Is.GreaterThan(0f));
            Assert.That(input.Steer, Is.GreaterThan(0f));
            Assert.That(input.Fire, Is.False);
            Assert.That(input.AimPoint, Is.EqualTo(target.Position + new Float3(0f, 1.25f, 0f)));
        }

        [Test]
        public void BotFiresOnlyWhenAlignedReloadedAndUnobstructed()
        {
            TankState bot = Tank("bot", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank("target", Team.Bravo, new Float3(0f, 0f, 90f), MathUtil.Pi);
            BotController clearController = new BotController(new SpottingSimulation());

            TankInput ready = clearController.Decide(bot, new List<TankState> { bot, target });
            Assert.That(ready.Fire, Is.True);

            bot.ReloadRemainingS = 0.25f;
            TankInput reloading = clearController.Decide(bot, new List<TankState> { bot, target });
            Assert.That(reloading.Fire, Is.False);

            bot.ReloadRemainingS = 0f;
            bot.TurretYaw = 0.2f;
            TankInput misaligned = clearController.Decide(bot, new List<TankState> { bot, target });
            Assert.That(misaligned.Fire, Is.False);

            BotController blockedController = new BotController(
                new SpottingSimulation(),
                (origin, end) => true);
            Assert.That(
                blockedController.Decide(bot, new List<TankState> { bot, target }).Fire,
                Is.False);
        }

        [Test]
        public void BotDecisionIsRepeatableForIdenticalState()
        {
            TankState bot = Tank("bot", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank("target", Team.Bravo, new Float3(15f, 0f, 75f), MathUtil.Pi);
            List<TankState> tanks = new List<TankState> { bot, target };
            BotController controller = new BotController(new SpottingSimulation());

            TankInput first = controller.Decide(bot, tanks);
            TankInput second = controller.Decide(bot, tanks);

            Assert.That(second.Throttle, Is.EqualTo(first.Throttle));
            Assert.That(second.Steer, Is.EqualTo(first.Steer));
            Assert.That(second.Brake, Is.EqualTo(first.Brake));
            Assert.That(second.Fire, Is.EqualTo(first.Fire));
            Assert.That(second.AimPoint, Is.EqualTo(first.AimPoint));
        }

        [Test]
        public void BotRoutesAroundBlockingObstacleTowardObjective()
        {
            BattleState state = new BattleState(
                new FlatHeightField(),
                19u,
                500f,
                new[]
                {
                    new StaticObstacle(
                        "block",
                        new Float3(0f, 0f, 30f),
                        5f,
                        5f,
                        5f,
                        0f,
                        StaticObstacleFlags.All)
                });
            TankState bot = Tank(
                "bot",
                Team.Alpha,
                Float3.Zero,
                0f);
            BotController controller =
                new BotController(
                    new SpottingSimulation(),
                    state.IsVisionOccluded,
                    state,
                    ignored =>
                        new Float3(
                            0f,
                            0f,
                            100f));

            TankInput input =
                controller.Decide(
                    bot,
                    new List<TankState>
                    {
                        bot
                    });

            Assert.That(input.Throttle, Is.GreaterThan(0f));
            Assert.That(
                System.Math.Abs(input.Steer),
                Is.GreaterThan(0.1f));
        }

        private static StaticObstacle Tree(
            string id,
            float z)
        {
            return new StaticObstacle(
                id,
                new Float3(0f, 0f, z),
                0.25f,
                0.25f,
                7f,
                0f,
                StaticObstacleFlags.Movement |
                StaticObstacleFlags.Shells |
                StaticObstacleFlags
                    .Concealment,
                true,
                true,
                1f);
        }

        private static TankState Tank(
            string id,
            Team team,
            Float3 position,
            float yaw)
        {
            return new TankState(id, team, TankSpec.Medium(), position, yaw);
        }
    }
}
