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
        public void ProximitySpottingIgnoresViewAndOcclusion()
        {
            SpottingSimulation spotting = new SpottingSimulation(445f, 30f);
            TankState spotter = Tank("spotter", Team.Alpha, Float3.Zero, 0f);
            TankState target = Tank("target", Team.Bravo, new Float3(0f, 0f, -49f), 0f);

            Assert.That(spotting.CanSpot(spotter, target, (origin, end) => true), Is.True);
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
