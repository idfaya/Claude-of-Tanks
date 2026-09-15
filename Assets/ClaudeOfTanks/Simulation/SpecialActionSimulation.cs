using System;

namespace ClaudeOfTanks.Simulation
{
    public enum SpecialActionKind
    {
        None,
        GuidedMissile,
        HydropneumaticAim,
        MagazineReload
    }

    public static class SpecialActionSimulation
    {
        public static SpecialActionKind KindFor(
            TankSpec spec)
        {
            if (spec == null)
                return SpecialActionKind.None;
            if (spec.HydropneumaticAim != null)
            {
                return SpecialActionKind
                    .HydropneumaticAim;
            }
            bool guided = false;
            bool conventional = false;
            ShellSpec[] shells =
                spec.Shells ??
                Array.Empty<ShellSpec>();
            for (int i = 0;
                i < shells.Length;
                i++)
            {
                guided |= shells[i].Guided;
                conventional |= !shells[i].Guided;
            }
            if (guided && conventional)
            {
                return SpecialActionKind
                    .GuidedMissile;
            }
            return spec.MagazineSize > 1
                ? SpecialActionKind
                    .MagazineReload
                : SpecialActionKind.None;
        }

        public static bool Activate(
            TankState tank)
        {
            if (tank == null)
                throw new ArgumentNullException(
                    nameof(tank));
            if (tank.Destroyed)
                return false;
            switch (KindFor(tank.Spec))
            {
                case SpecialActionKind
                    .HydropneumaticAim:
                    tank.HydropneumaticAimActive =
                        !tank.HydropneumaticAimActive;
                    return true;
                case SpecialActionKind
                    .GuidedMissile:
                    return ToggleGuidedMissile(
                        tank);
                case SpecialActionKind
                    .MagazineReload:
                    return DamageSimulation
                        .StartMagazineReload(
                            tank.Combat,
                            tank.DamageSpec);
                default:
                    return false;
            }
        }

        public static string ShortLabel(
            TankState tank)
        {
            switch (KindFor(tank?.Spec))
            {
                case SpecialActionKind
                    .GuidedMissile:
                    return "ATGM";
                case SpecialActionKind
                    .HydropneumaticAim:
                    return tank
                            .HydropneumaticAimActive
                        ? "E ON"
                        : "E";
                case SpecialActionKind
                    .MagazineReload:
                    return "RELOAD";
                default:
                    return string.Empty;
            }
        }

        private static bool ToggleGuidedMissile(
            TankState tank)
        {
            ShellSpec[] shells =
                tank.Spec.Shells;
            int current =
                tank.Combat.ShellSlot;
            int slot;
            if (current >= 0 &&
                current < shells.Length &&
                shells[current].Guided)
            {
                slot =
                    ConventionalSlot(
                        tank,
                        tank
                            .PreviousConventionalShellSlot);
            }
            else
            {
                if (current >= 0 &&
                    current < shells.Length)
                {
                    tank
                        .PreviousConventionalShellSlot =
                        current;
                }
                slot = GuidedSlot(tank);
            }
            return slot >= 0 &&
                DamageSimulation.SelectShell(
                    tank.Combat,
                    slot,
                    tank.DamageSpec);
        }

        private static int GuidedSlot(
            TankState tank)
        {
            for (int i = 0;
                i < tank.Spec.Shells.Length;
                i++)
            {
                if (tank.Spec.Shells[i].Guided &&
                    DamageSimulation
                        .HasAmmunition(
                            tank.Combat,
                            i))
                {
                    return i;
                }
            }
            return -1;
        }

        private static int ConventionalSlot(
            TankState tank,
            int preferred)
        {
            if (preferred >= 0 &&
                preferred <
                    tank.Spec.Shells.Length &&
                !tank.Spec.Shells[preferred]
                    .Guided &&
                DamageSimulation.HasAmmunition(
                    tank.Combat,
                    preferred))
            {
                return preferred;
            }
            for (int i = 0;
                i < tank.Spec.Shells.Length;
                i++)
            {
                if (!tank.Spec.Shells[i]
                        .Guided &&
                    DamageSimulation
                        .HasAmmunition(
                            tank.Combat,
                            i))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
