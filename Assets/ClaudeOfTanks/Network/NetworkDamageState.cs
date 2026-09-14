using System;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public static class NetworkDamageState
    {
        public const ushort AllCrewAliveMask = 0x01ff;
        public const uint AllModuleMask = (1u << 14) - 1u;

        private static readonly string[] ModuleIds =
        {
            "gun",
            "turretRing",
            "gunMount",
            "autoloader",
            "feedSystem",
            "missileRack",
            "engine",
            "transmission",
            "fuelTank",
            "ammoRack",
            "radio",
            "optics",
            "trackL",
            "trackR"
        };

        private static readonly string[] CrewIds =
        {
            "commander",
            "gunner",
            "driver",
            "loader",
            "assistantDriver",
            "assistantLoader",
            "radioOperator",
            "weaponOperatorLeft",
            "weaponOperatorRight"
        };

        public static void Capture(
            DamageCombatState state,
            out uint yellowMask,
            out uint redMask,
            out ushort crewAliveMask)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            yellowMask = 0u;
            redMask = 0u;
            for (int i = 0; i < ModuleIds.Length; i++)
            {
                DamageModuleState module;
                if (!state.Modules.TryGetValue(ModuleIds[i], out module))
                    continue;
                uint bit = 1u << i;
                if (module.Condition == DamageModuleCondition.Yellow)
                    yellowMask |= bit;
                else if (module.Condition == DamageModuleCondition.Red)
                    redMask |= bit;
            }

            crewAliveMask = 0;
            for (int i = 0; i < CrewIds.Length; i++)
            {
                bool alive;
                if (!state.Crew.TryGetValue(CrewIds[i], out alive) || alive)
                    crewAliveMask |= (ushort)(1 << i);
            }
        }

        public static void Apply(
            DamageCombatState state,
            NetworkEntitySnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));
            Apply(
                state,
                snapshot.ModuleYellowMask,
                snapshot.ModuleRedMask,
                snapshot.CrewAliveMask);
        }

        public static void Apply(
            DamageCombatState state,
            uint yellowMask,
            uint redMask,
            ushort crewAliveMask)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            for (int i = 0; i < ModuleIds.Length; i++)
            {
                DamageModuleState module;
                if (!state.Modules.TryGetValue(ModuleIds[i], out module))
                    continue;
                uint bit = 1u << i;
                DamageModuleCondition condition =
                    (redMask & bit) != 0u
                        ? DamageModuleCondition.Red
                        : (yellowMask & bit) != 0u
                            ? DamageModuleCondition.Yellow
                            : DamageModuleCondition.Ok;
                module.Condition = condition;
                module.Health = condition == DamageModuleCondition.Red
                    ? 0f
                    : condition == DamageModuleCondition.Yellow
                        ? module.MaxHealth * 0.5f
                        : module.MaxHealth;
            }
            for (int i = 0; i < CrewIds.Length; i++)
            {
                if (state.Crew.ContainsKey(CrewIds[i]))
                    state.Crew[CrewIds[i]] =
                        (crewAliveMask & (1 << i)) != 0;
            }
        }

        public static void CaptureInto(
            DamageCombatState state,
            NetworkEntitySnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));
            Capture(
                state,
                out snapshot.ModuleYellowMask,
                out snapshot.ModuleRedMask,
                out snapshot.CrewAliveMask);
        }

        public static void Copy(
            DamageCombatState source,
            DamageCombatState target)
        {
            uint yellowMask;
            uint redMask;
            ushort crewAliveMask;
            Capture(
                source,
                out yellowMask,
                out redMask,
                out crewAliveMask);
            Apply(target, yellowMask, redMask, crewAliveMask);
        }
    }
}
