using System;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public enum BattleReloadProfile
    {
        Rapid,
        Magazine,
        IntraClip,
        Shell
    }

    public enum BattleReloadCueType
    {
        Motor,
        Index,
        BreechOpen,
        Extract,
        ShellLift,
        Ram,
        BreechClose
    }

    public struct BattleReloadCue
    {
        public readonly float At;
        public readonly BattleReloadCueType Type;

        public BattleReloadCue(float at, BattleReloadCueType type)
        {
            At = at;
            Type = type;
        }
    }

    public sealed class BattleReloadCuePlan
    {
        public readonly BattleReloadProfile Profile;
        public readonly bool Ready;
        public readonly BattleReloadCue[] Cues;

        public BattleReloadCuePlan(
            BattleReloadProfile profile,
            bool ready,
            BattleReloadCue[] cues)
        {
            Profile = profile;
            Ready = ready;
            Cues = cues ?? Array.Empty<BattleReloadCue>();
        }
    }

    public static class BattleReloadAudioPolicy
    {
        public static BattleReloadCuePlan Resolve(
            float totalS,
            DamageReloadKind kind,
            float caliberMm)
        {
            float total = Math.Max(0.05f, totalS);
            float caliber = Math.Max(12f, caliberMm);
            if (total < 0.55f)
            {
                return new BattleReloadCuePlan(
                    BattleReloadProfile.Rapid,
                    false,
                    Array.Empty<BattleReloadCue>());
            }
            if (kind == DamageReloadKind.Magazine)
            {
                return new BattleReloadCuePlan(
                    BattleReloadProfile.Magazine,
                    true,
                    new[]
                    {
                        Cue(0.02f, BattleReloadCueType.Motor),
                        Cue(0.22f, BattleReloadCueType.Index),
                        Cue(0.48f, BattleReloadCueType.Index),
                        Cue(0.74f, BattleReloadCueType.Index),
                        Cue(0.92f, BattleReloadCueType.BreechClose)
                    });
            }
            if (kind == DamageReloadKind.IntraClip)
            {
                return new BattleReloadCuePlan(
                    BattleReloadProfile.IntraClip,
                    true,
                    new[]
                    {
                        Cue(0.10f, BattleReloadCueType.Motor),
                        Cue(0.48f, BattleReloadCueType.Index),
                        Cue(0.86f, BattleReloadCueType.BreechClose)
                    });
            }

            BattleReloadCue[] cues = caliber >= 105f && total >= 4f
                ? new[]
                {
                    Cue(0.015f, BattleReloadCueType.BreechOpen),
                    Cue(Math.Min(0.20f, 0.72f / total), BattleReloadCueType.Extract),
                    Cue(0.40f, BattleReloadCueType.ShellLift),
                    Cue(0.64f, BattleReloadCueType.ShellLift),
                    Cue(Math.Max(0.72f, 1f - 0.70f / total), BattleReloadCueType.Ram),
                    Cue(Math.Max(0.86f, 1f - 0.22f / total), BattleReloadCueType.BreechClose)
                }
                : new[]
                {
                    Cue(0.015f, BattleReloadCueType.BreechOpen),
                    Cue(Math.Min(0.20f, 0.72f / total), BattleReloadCueType.Extract),
                    Cue(0.40f, BattleReloadCueType.ShellLift),
                    Cue(Math.Max(0.72f, 1f - 0.70f / total), BattleReloadCueType.Ram),
                    Cue(Math.Max(0.86f, 1f - 0.22f / total), BattleReloadCueType.BreechClose)
                };
            return new BattleReloadCuePlan(
                BattleReloadProfile.Shell,
                true,
                cues);
        }

        public static DamageReloadKind InferKind(
            TankSpec spec,
            float remainingS)
        {
            if (spec == null || spec.MagazineSize <= 1)
                return DamageReloadKind.Shell;
            float intraClip = InferTotal(
                spec,
                DamageReloadKind.IntraClip);
            return remainingS > intraClip + 0.05f
                ? DamageReloadKind.Magazine
                : DamageReloadKind.IntraClip;
        }

        public static float InferTotal(
            TankSpec spec,
            DamageReloadKind kind)
        {
            if (spec == null || spec.Shell == null) return 0.05f;
            if (kind == DamageReloadKind.Magazine &&
                spec.MagazineReloadS > 0f)
            {
                return Math.Max(0.05f, spec.MagazineReloadS);
            }
            if (kind == DamageReloadKind.IntraClip &&
                spec.IntraClipS > 0f)
            {
                return Math.Max(0.05f, spec.IntraClipS);
            }
            return Math.Max(0.05f, spec.Shell.ReloadS);
        }

        private static BattleReloadCue Cue(
            float at,
            BattleReloadCueType type)
        {
            return new BattleReloadCue(at, type);
        }
    }
}
