using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public enum DamageModuleCondition
    {
        Ok,
        Yellow,
        Red
    }

    public enum DamageReloadKind
    {
        Ready,
        Shell,
        IntraClip,
        Magazine
    }

    public enum MagazineReloadDenialReason
    {
        None,
        NoMagazine,
        MagazineReloading,
        MagazineFull
    }

    public sealed class DamageShellSpec
    {
        public string Type = "AP";
        public int? Count;
        public float? ReloadS;
        public bool Guided;
    }

    public sealed class DamageAutoloaderSpec
    {
        public int MagazineSize = 1;
        public float? FullReloadS;
        public float? IntraClipS;
    }

    public sealed class DamageGunSpec
    {
        public float ReloadS = 5.5f;
        public readonly List<DamageShellSpec> Shells = new List<DamageShellSpec>();
        public DamageAutoloaderSpec Autoloader;
    }

    public sealed class DamageTankSpec
    {
        public float MaxHealth = 1000f;
        public bool IsPostwar;
        public readonly List<string> ModuleIds = new List<string>();
        public readonly List<string> CrewIds = new List<string>();
        public DamageGunSpec Gun = new DamageGunSpec();
    }

    public sealed class DamageModuleState
    {
        public float Health;
        public float MaxHealth;
        public DamageModuleCondition Condition;
        public float RepairElapsedS;
    }

    public sealed class FireState
    {
        public bool Burning;
        public float TickTimerS;
        public int TicksLeft;
    }

    public sealed class DamageReloadState
    {
        public float RemainingS;
        public float TotalS;
        public DamageReloadKind Kind;
    }

    public sealed class MagazineState
    {
        public int Rounds;
        public int Capacity;
    }

    public sealed class DamageEquipmentModifiers
    {
        public float RepairRate = 1f;
        public float Reload = 1f;
        public float Extinguish = 1f;
        public float FireTicks = 1f;
        public float EngineFire = 1f;
    }

    public sealed class DamageCombatState
    {
        public float Health;
        public float MaxHealth;
        public bool Destroyed;
        public readonly Dictionary<string, DamageModuleState> Modules =
            new Dictionary<string, DamageModuleState>(StringComparer.Ordinal);
        public readonly Dictionary<string, bool> Crew =
            new Dictionary<string, bool>(StringComparer.Ordinal);
        public readonly FireState Fire = new FireState();
        public readonly DamageEquipmentModifiers Equipment = new DamageEquipmentModifiers();
        public DamageReloadState Reload;
        public DamageReloadState GunReload;
        public DamageReloadState[] ReloadChannels = Array.Empty<DamageReloadState>();
        public MagazineState Magazine;
        public int ShellSlot;
        public int[] Ammo = Array.Empty<int>();
        public int[] AmmoCapacity = Array.Empty<int>();
    }

    public struct ModuleDamageResult
    {
        public DamageModuleCondition Condition;
        public bool FireStarted;
        public bool AmmoRacked;
        public bool Destroyed;
    }

    public struct FireTickResult
    {
        public float Damage;
        public int Ticks;
        public bool Extinguished;
        public bool Destroyed;
    }

    public static class DamageSimulation
    {
        public const float RepairDurationS = 10f;
        public const float FireTickIntervalS = 0.5f;

        private const float FireTickHealthFraction = 0.005f;
        private const float FireTickModuleDamage = 10f;
        private const float FireExtinguishChance = 0.12f;
        private const float EngineFireChance = 0.15f;
        private const float AmmoRackReloadMultiplier = 1.5f;

        private static readonly string[] DefaultModules =
        {
            "gun", "turretRing", "engine", "transmission", "fuelTank",
            "ammoRack", "radio", "optics", "trackL", "trackR"
        };

        private static readonly string[] DefaultCrew =
        {
            "commander", "gunner", "driver", "loader"
        };

        private static readonly Dictionary<string, float> ModuleHealth =
            new Dictionary<string, float>(StringComparer.Ordinal)
            {
                ["gun"] = 150f,
                ["turretRing"] = 120f,
                ["gunMount"] = 120f,
                ["autoloader"] = 125f,
                ["feedSystem"] = 110f,
                ["missileRack"] = 120f,
                ["engine"] = 160f,
                ["transmission"] = 140f,
                ["fuelTank"] = 120f,
                ["ammoRack"] = 150f,
                ["radio"] = 90f,
                ["optics"] = 80f,
                ["trackL"] = 100f,
                ["trackR"] = 100f
            };

        private static readonly Dictionary<string, int> DefaultAmmunition =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["AP"] = 24,
                ["APCR"] = 20,
                ["APFSDS"] = 24,
                ["HEAT"] = 16,
                ["HE"] = 12
            };

        public static DamageCombatState CreateCombatState(DamageTankSpec spec)
        {
            if (spec == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }

            if (spec.Gun == null)
            {
                throw new ArgumentException("Damage tank spec requires a gun.", nameof(spec));
            }

            DamageCombatState state = new DamageCombatState
            {
                Health = Math.Max(0f, spec.MaxHealth),
                MaxHealth = Math.Max(0f, spec.MaxHealth)
            };

            float moduleScale = spec.IsPostwar ? 2.5f : 1f;
            IList<string> moduleIds = spec.ModuleIds.Count > 0
                ? (IList<string>)spec.ModuleIds
                : DefaultModules;
            for (int i = 0; i < moduleIds.Count; i++)
            {
                string id = moduleIds[i];
                if (string.IsNullOrEmpty(id) || state.Modules.ContainsKey(id))
                {
                    continue;
                }

                float baseHealth;
                if (!ModuleHealth.TryGetValue(id, out baseHealth))
                {
                    throw new ArgumentException("Unknown damage module: " + id, nameof(spec));
                }

                float health = baseHealth * moduleScale;
                state.Modules.Add(id, new DamageModuleState
                {
                    Health = health,
                    MaxHealth = health,
                    Condition = DamageModuleCondition.Ok
                });
            }

            IList<string> crewIds = spec.CrewIds.Count > 0
                ? (IList<string>)spec.CrewIds
                : DefaultCrew;
            for (int i = 0; i < crewIds.Count; i++)
            {
                string id = crewIds[i];
                if (!string.IsNullOrEmpty(id))
                {
                    state.Crew[id] = true;
                }
            }

            int shellCount = spec.Gun.Shells.Count;
            state.Ammo = new int[shellCount];
            state.AmmoCapacity = new int[shellCount];
            for (int i = 0; i < shellCount; i++)
            {
                int capacity = AmmunitionCapacity(spec.Gun.Shells[i]);
                state.Ammo[i] = capacity;
                state.AmmoCapacity[i] = capacity;
            }

            state.GunReload = NewReload(spec.Gun.ReloadS);
            state.ReloadChannels = new DamageReloadState[shellCount];
            for (int i = 0; i < shellCount; i++)
            {
                DamageShellSpec shell = spec.Gun.Shells[i];
                state.ReloadChannels[i] = shell.Guided
                    ? NewReload(shell.ReloadS ?? spec.Gun.ReloadS)
                    : state.GunReload;
            }

            state.Reload = shellCount > 0 ? state.ReloadChannels[0] : state.GunReload;
            if (spec.Gun.Autoloader != null)
            {
                int capacity = Math.Max(1, spec.Gun.Autoloader.MagazineSize);
                state.Magazine = new MagazineState { Rounds = capacity, Capacity = capacity };
            }

            return state;
        }

        public static ModuleDamageResult DamageModule(
            DamageCombatState state,
            string moduleId,
            float damage,
            Func<float> random01 = null)
        {
            RequireState(state);
            DamageModuleState module;
            if (!state.Modules.TryGetValue(moduleId, out module))
            {
                throw new ArgumentException("Unknown or unauthored damage module: " + moduleId, nameof(moduleId));
            }

            DamageModuleCondition previous = module.Condition;
            module.Health = Math.Max(0f, module.Health - Math.Max(0f, damage));
            RefreshModuleCondition(module, previous);

            bool ammoRacked = moduleId == "ammoRack" &&
                module.Condition == DamageModuleCondition.Red;
            bool ignite = false;
            if (moduleId == "engine" && random01 != null)
            {
                ignite = random01() < EngineFireChance * PositiveMultiplier(state.Equipment.EngineFire);
            }
            else if (moduleId == "fuelTank")
            {
                ignite = module.Condition == DamageModuleCondition.Red;
            }

            bool fireStarted = ignite && !state.Fire.Burning;
            if (ignite)
            {
                IgniteFire(state);
            }

            FinalizeState(state, ammoRacked);
            return new ModuleDamageResult
            {
                Condition = module.Condition,
                FireStarted = fireStarted,
                AmmoRacked = ammoRacked,
                Destroyed = state.Destroyed
            };
        }

        public static void DamageHealth(DamageCombatState state, float damage)
        {
            RequireState(state);
            state.Health = Math.Max(0f, state.Health - Math.Max(0f, damage));
            FinalizeState(state, false);
        }

        public static bool KnockOutCrew(DamageCombatState state, string crewId)
        {
            RequireState(state);
            bool alive;
            if (!state.Crew.TryGetValue(crewId, out alive) || !alive)
            {
                return false;
            }

            state.Crew[crewId] = false;
            FinalizeState(state, false);
            return true;
        }

        public static void IgniteFire(DamageCombatState state)
        {
            RequireState(state);
            if (state.Destroyed)
            {
                return;
            }

            state.Fire.Burning = true;
            state.Fire.TickTimerS = 0f;
            state.Fire.TicksLeft = Math.Max(
                2,
                (int)Math.Round(10f * PositiveMultiplier(state.Equipment.FireTicks)));
        }

        public static FireTickResult AdvanceFire(
            DamageCombatState state,
            float deltaTimeS,
            Func<float> random01)
        {
            RequireState(state);
            if (random01 == null)
            {
                throw new ArgumentNullException(nameof(random01));
            }

            FireTickResult result = new FireTickResult { Destroyed = state.Destroyed };
            if (!state.Fire.Burning || state.Destroyed || deltaTimeS <= 0f)
            {
                return result;
            }

            state.Fire.TickTimerS += deltaTimeS;
            while (state.Fire.Burning &&
                   !state.Destroyed &&
                   state.Fire.TickTimerS + 0.000001f >= FireTickIntervalS)
            {
                state.Fire.TickTimerS -= FireTickIntervalS;
                result.Damage += ApplyFireTick(state, random01, ref result.Extinguished);
                result.Ticks++;
            }

            result.Destroyed = state.Destroyed;
            if (!state.Fire.Burning)
            {
                state.Fire.TickTimerS = 0f;
            }

            return result;
        }

        public static List<string> TickModuleRepairs(DamageCombatState state, float deltaTimeS)
        {
            List<string> repaired = new List<string>();
            AdvanceModuleRepairs(state, deltaTimeS, repaired);
            return repaired;
        }

        public static void AdvanceModuleRepairs(DamageCombatState state, float deltaTimeS)
        {
            AdvanceModuleRepairs(state, deltaTimeS, null);
        }

        private static void AdvanceModuleRepairs(
            DamageCombatState state,
            float deltaTimeS,
            List<string> repaired)
        {
            if (state == null || state.Destroyed || deltaTimeS <= 0f)
            {
                return;
            }

            float elapsed = deltaTimeS * PositiveMultiplier(state.Equipment.RepairRate);
            foreach (KeyValuePair<string, DamageModuleState> entry in state.Modules)
            {
                DamageModuleState module = entry.Value;
                if (module.Condition != DamageModuleCondition.Red)
                {
                    continue;
                }

                module.RepairElapsedS += elapsed;
                if (module.RepairElapsedS + 0.000001f < RepairDurationS)
                {
                    continue;
                }

                module.Health = module.MaxHealth * 0.5f;
                RefreshModuleCondition(module, DamageModuleCondition.Red);
                repaired?.Add(entry.Key);
            }
        }

        public static List<string> RepairAllModules(DamageCombatState state)
        {
            List<string> repaired = new List<string>();
            if (state == null)
            {
                return repaired;
            }

            foreach (KeyValuePair<string, DamageModuleState> entry in state.Modules)
            {
                DamageModuleState module = entry.Value;
                if (module.Condition == DamageModuleCondition.Ok)
                {
                    continue;
                }

                module.Health = module.MaxHealth;
                RefreshModuleCondition(module, module.Condition);
                repaired.Add(entry.Key);
            }

            return repaired;
        }

        public static bool HasAmmunition(DamageCombatState state, int slot)
        {
            return state != null &&
                slot >= 0 &&
                slot < state.Ammo.Length &&
                state.Ammo[slot] > 0;
        }

        public static bool ConsumeAmmunition(DamageCombatState state, int slot)
        {
            if (!HasAmmunition(state, slot))
            {
                return false;
            }

            state.Ammo[slot]--;
            return true;
        }

        public static int SelectFirstAvailableShell(DamageCombatState state, DamageTankSpec spec)
        {
            RequireState(state);
            for (int i = 0; i < state.Ammo.Length; i++)
            {
                if (state.Ammo[i] > 0 && SelectShell(state, i, spec))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool SelectShell(DamageCombatState state, int slot, DamageTankSpec spec)
        {
            RequireState(state);
            RequireSpec(spec);
            if (slot < 0 || slot >= spec.Gun.Shells.Count || !HasAmmunition(state, slot))
            {
                return false;
            }

            if (slot == state.ShellSlot)
            {
                return true;
            }

            state.ShellSlot = slot;
            state.Reload = state.ReloadChannels[slot];
            StartReload(state, spec);
            return true;
        }

        public static void StartReload(DamageCombatState state, DamageTankSpec spec)
        {
            RequireState(state);
            RequireSpec(spec);
            DamageShellSpec loaded = LoadedShell(state, spec);
            if (state.Magazine != null && spec.Gun.Autoloader != null &&
                (loaded == null || !loaded.Guided))
            {
                BeginMagazineReload(state, spec);
                return;
            }

            BeginShellReload(state, spec, loaded);
        }

        public static void StartPostShotReload(DamageCombatState state, DamageTankSpec spec)
        {
            RequireState(state);
            RequireSpec(spec);
            DamageShellSpec loaded = LoadedShell(state, spec);
            if (loaded != null && loaded.Guided)
            {
                BeginShellReload(state, spec, loaded);
                return;
            }

            if (state.Magazine == null || spec.Gun.Autoloader == null)
            {
                StartReload(state, spec);
                return;
            }

            state.Magazine.Rounds = Math.Max(0, state.Magazine.Rounds - 1);
            if (state.Magazine.Rounds == 0)
            {
                BeginMagazineReload(state, spec);
                return;
            }

            float total = Math.Max(0.05f, spec.Gun.Autoloader.IntraClipS ?? spec.Gun.ReloadS);
            state.Reload.TotalS = total;
            state.Reload.RemainingS = total;
            state.Reload.Kind = DamageReloadKind.IntraClip;
        }

        public static MagazineReloadDenialReason GetMagazineReloadDenialReason(
            DamageCombatState state)
        {
            if (state == null || state.Magazine == null)
            {
                return MagazineReloadDenialReason.NoMagazine;
            }

            DamageReloadState channel = state.GunReload ?? state.Reload;
            if (channel.Kind == DamageReloadKind.Magazine && channel.RemainingS > 0f)
            {
                return MagazineReloadDenialReason.MagazineReloading;
            }

            if (state.Magazine.Rounds >= state.Magazine.Capacity)
            {
                return MagazineReloadDenialReason.MagazineFull;
            }

            return MagazineReloadDenialReason.None;
        }

        public static bool StartMagazineReload(DamageCombatState state, DamageTankSpec spec)
        {
            if (GetMagazineReloadDenialReason(state) != MagazineReloadDenialReason.None)
            {
                return false;
            }

            RequireSpec(spec);
            return BeginMagazineReload(state, spec);
        }

        public static bool TickReload(DamageCombatState state, float deltaTimeS)
        {
            if (state == null || deltaTimeS <= 0f)
            {
                return false;
            }

            DamageReloadState active = state.Reload;
            bool activeReady = false;
            if (state.ReloadChannels.Length > 0)
            {
                for (int i = 0; i < state.ReloadChannels.Length; i++)
                {
                    DamageReloadState channel = state.ReloadChannels[i];
                    bool duplicate = false;
                    for (int prior = 0; prior < i; prior++)
                    {
                        if (ReferenceEquals(state.ReloadChannels[prior], channel))
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (duplicate || channel.RemainingS <= 0f)
                    {
                        continue;
                    }

                    bool becameReady = AdvanceReloadChannel(state, channel, deltaTimeS);
                    if (becameReady && ReferenceEquals(channel, active))
                    {
                        activeReady = true;
                    }
                }

                return activeReady;
            }

            return AdvanceReloadChannel(state, active, deltaTimeS);
        }

        private static float ApplyFireTick(
            DamageCombatState state,
            Func<float> random01,
            ref bool extinguished)
        {
            float damage = state.MaxHealth * FireTickHealthFraction;
            state.Health = Math.Max(0f, state.Health - damage);

            bool ammoRacked = false;
            string[] affectedModules = { "engine", "fuelTank", "ammoRack" };
            for (int i = 0; i < affectedModules.Length; i++)
            {
                DamageModuleState module;
                if (!state.Modules.TryGetValue(affectedModules[i], out module) ||
                    module.Health <= 0f)
                {
                    continue;
                }

                DamageModuleCondition previous = module.Condition;
                module.Health = Math.Max(0f, module.Health - FireTickModuleDamage);
                RefreshModuleCondition(module, previous);
                ammoRacked |= affectedModules[i] == "ammoRack" &&
                    module.Condition == DamageModuleCondition.Red;
            }

            state.Fire.TicksLeft--;
            float extinguishChance = Math.Min(
                1f,
                FireExtinguishChance * PositiveMultiplier(state.Equipment.Extinguish));
            if (random01() < extinguishChance || state.Fire.TicksLeft <= 0)
            {
                state.Fire.Burning = false;
                extinguished = true;
            }

            FinalizeState(state, ammoRacked);
            if (state.Destroyed)
            {
                extinguished = true;
            }

            return damage;
        }

        private static bool BeginMagazineReload(DamageCombatState state, DamageTankSpec spec)
        {
            if (state.Magazine == null || spec.Gun.Autoloader == null)
            {
                return false;
            }

            state.Magazine.Rounds = 0;
            DamageReloadState channel = state.GunReload ?? state.Reload;
            DamageShellSpec cannonRound = null;
            for (int i = 0; i < spec.Gun.Shells.Count; i++)
            {
                if (!spec.Gun.Shells[i].Guided)
                {
                    cannonRound = spec.Gun.Shells[i];
                    break;
                }
            }

            float baseS = Math.Max(
                0.05f,
                spec.Gun.Autoloader.FullReloadS ?? spec.Gun.ReloadS);
            float total = baseS * ReloadMultiplier(state, cannonRound);
            channel.TotalS = total;
            channel.RemainingS = total;
            channel.Kind = DamageReloadKind.Magazine;
            return true;
        }

        private static void BeginShellReload(
            DamageCombatState state,
            DamageTankSpec spec,
            DamageShellSpec loaded)
        {
            float baseS = loaded != null && loaded.ReloadS.HasValue
                ? loaded.ReloadS.Value
                : spec.Gun.ReloadS;
            float total = baseS * ReloadMultiplier(state, loaded);
            state.Reload.TotalS = total;
            state.Reload.RemainingS = total;
            state.Reload.Kind = DamageReloadKind.Shell;
        }

        private static float ReloadMultiplier(DamageCombatState state, DamageShellSpec loaded)
        {
            float multiplier = 1f;
            bool loaderAlive;
            if (state.Crew.TryGetValue("loader", out loaderAlive) && !loaderAlive)
            {
                multiplier *= 1.5f;
            }

            DamageModuleState module;
            if (state.Modules.TryGetValue("ammoRack", out module) &&
                module.Condition != DamageModuleCondition.Ok)
            {
                multiplier *= AmmoRackReloadMultiplier;
            }

            if (state.Modules.TryGetValue("autoloader", out module) ||
                state.Modules.TryGetValue("feedSystem", out module))
            {
                if (module.Condition == DamageModuleCondition.Yellow)
                {
                    multiplier *= 1.35f;
                }
                else if (module.Condition == DamageModuleCondition.Red)
                {
                    multiplier *= 2f;
                }
            }

            if (loaded != null && loaded.Guided &&
                state.Modules.TryGetValue("missileRack", out module))
            {
                if (module.Condition == DamageModuleCondition.Yellow)
                {
                    multiplier *= 1.4f;
                }
                else if (module.Condition == DamageModuleCondition.Red)
                {
                    multiplier *= 1.8f;
                }
            }

            return multiplier * PositiveMultiplier(state.Equipment.Reload);
        }

        private static bool AdvanceReloadChannel(
            DamageCombatState state,
            DamageReloadState channel,
            float deltaTimeS)
        {
            if (channel == null || channel.RemainingS <= 0f)
            {
                return false;
            }

            channel.RemainingS = Math.Max(0f, channel.RemainingS - deltaTimeS);
            if (channel.RemainingS > 0.000001f)
            {
                return false;
            }

            channel.RemainingS = 0f;
            if (channel.Kind == DamageReloadKind.Magazine && state.Magazine != null)
            {
                state.Magazine.Rounds = state.Magazine.Capacity;
            }

            channel.Kind = DamageReloadKind.Ready;
            return true;
        }

        private static DamageReloadState NewReload(float totalS)
        {
            return new DamageReloadState
            {
                TotalS = totalS,
                Kind = DamageReloadKind.Ready
            };
        }

        private static int AmmunitionCapacity(DamageShellSpec shell)
        {
            if (shell == null)
            {
                return 0;
            }

            if (shell.Count.HasValue)
            {
                return Math.Max(0, shell.Count.Value);
            }

            int capacity;
            return DefaultAmmunition.TryGetValue(shell.Type ?? string.Empty, out capacity)
                ? capacity
                : 20;
        }

        private static DamageShellSpec LoadedShell(DamageCombatState state, DamageTankSpec spec)
        {
            return state.ShellSlot >= 0 && state.ShellSlot < spec.Gun.Shells.Count
                ? spec.Gun.Shells[state.ShellSlot]
                : null;
        }

        private static void RefreshModuleCondition(
            DamageModuleState module,
            DamageModuleCondition previous)
        {
            module.Condition = module.Health <= 0f
                ? DamageModuleCondition.Red
                : module.Health <= module.MaxHealth * 0.5f
                    ? DamageModuleCondition.Yellow
                    : DamageModuleCondition.Ok;
            if (module.Condition == DamageModuleCondition.Red &&
                previous != DamageModuleCondition.Red)
            {
                module.RepairElapsedS = 0f;
            }
            else if (module.Condition != DamageModuleCondition.Red)
            {
                module.RepairElapsedS = 0f;
            }
        }

        private static void FinalizeState(DamageCombatState state, bool ammoRacked)
        {
            if (ammoRacked)
            {
                state.Health = 0f;
            }

            bool allCrewDead = state.Crew.Count > 0;
            foreach (bool alive in state.Crew.Values)
            {
                if (alive)
                {
                    allCrewDead = false;
                    break;
                }
            }

            if (state.Health <= 0f || allCrewDead)
            {
                state.Health = 0f;
                state.Destroyed = true;
                state.Fire.Burning = false;
            }
        }

        private static float PositiveMultiplier(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value)
                ? value
                : 1f;
        }

        private static void RequireState(DamageCombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
        }

        private static void RequireSpec(DamageTankSpec spec)
        {
            if (spec == null || spec.Gun == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }
        }
    }
}
