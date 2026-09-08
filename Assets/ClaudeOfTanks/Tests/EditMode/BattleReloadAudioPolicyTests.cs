using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattleReloadAudioPolicyTests
    {
        [Test]
        public void RapidReloadDoesNotScheduleOverlappingMechanicalCues()
        {
            BattleReloadCuePlan plan = BattleReloadAudioPolicy.Resolve(
                0.3f,
                DamageReloadKind.Shell,
                30f);

            Assert.That(plan.Profile, Is.EqualTo(BattleReloadProfile.Rapid));
            Assert.That(plan.Ready, Is.False);
            Assert.That(plan.Cues, Is.Empty);
        }

        [Test]
        public void ConventionalReloadMatchesCanonicalMechanicalSequence()
        {
            BattleReloadCuePlan plan = BattleReloadAudioPolicy.Resolve(
                7.5f,
                DamageReloadKind.Shell,
                125f);

            Assert.That(plan.Profile, Is.EqualTo(BattleReloadProfile.Shell));
            Assert.That(
                plan.Cues.Select(cue => cue.Type),
                Is.EqualTo(new[]
                {
                    BattleReloadCueType.BreechOpen,
                    BattleReloadCueType.Extract,
                    BattleReloadCueType.ShellLift,
                    BattleReloadCueType.ShellLift,
                    BattleReloadCueType.Ram,
                    BattleReloadCueType.BreechClose
                }));
            for (int i = 1; i < plan.Cues.Length; i++)
                Assert.That(plan.Cues[i].At, Is.GreaterThan(plan.Cues[i - 1].At));
            Assert.That(plan.Ready, Is.True);
        }

        [Test]
        public void MagazineAndIntraClipProfilesMatchCanonicalTiming()
        {
            BattleReloadCuePlan magazine = BattleReloadAudioPolicy.Resolve(
                18f,
                DamageReloadKind.Magazine,
                120f);
            BattleReloadCuePlan intraClip = BattleReloadAudioPolicy.Resolve(
                2.5f,
                DamageReloadKind.IntraClip,
                120f);

            Assert.That(
                magazine.Cues.Count(cue =>
                    cue.Type == BattleReloadCueType.Index),
                Is.EqualTo(3));
            Assert.That(magazine.Cues[magazine.Cues.Length - 1].At, Is.LessThan(1f));
            Assert.That(
                intraClip.Cues.Select(cue => cue.Type),
                Is.EqualTo(new[]
                {
                    BattleReloadCueType.Motor,
                    BattleReloadCueType.Index,
                    BattleReloadCueType.BreechClose
                }));
        }

        [Test]
        public void NetworkFallbackInfersAutoloaderCycleFromRemainingTime()
        {
            TankSpec spec = TankSpec.Medium();
            spec.MagazineSize = 4;
            spec.MagazineReloadS = 18f;
            spec.IntraClipS = 2.5f;

            Assert.That(
                BattleReloadAudioPolicy.InferKind(spec, 2.2f),
                Is.EqualTo(DamageReloadKind.IntraClip));
            Assert.That(
                BattleReloadAudioPolicy.InferKind(spec, 17.4f),
                Is.EqualTo(DamageReloadKind.Magazine));
            Assert.That(
                BattleReloadAudioPolicy.InferTotal(
                    spec,
                    DamageReloadKind.Magazine),
                Is.EqualTo(18f));
        }
    }
}
