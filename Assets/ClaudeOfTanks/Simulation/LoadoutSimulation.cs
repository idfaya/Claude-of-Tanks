using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public enum ConsumableSlot
    {
        RepairKit,
        FirstAidKit,
        FireExtinguisher
    }

    public static class LoadoutSimulation
    {
        public const int EquipmentSlots = 3;
        public const float ConsumableCooldownS = 25f;

        public static string[] SanitizeEquipment(
            IEnumerable<string> ids, bool modern, bool autoloader)
        {
            List<string> result = new List<string>(EquipmentSlots);
            if (ids == null) return result.ToArray();
            foreach (string id in ids)
            {
                if (!IsEquipmentAllowed(id, modern, autoloader) ||
                    result.Contains(id))
                {
                    continue;
                }
                result.Add(id);
                if (result.Count == EquipmentSlots) break;
            }
            return result.ToArray();
        }

        public static void ApplyEquipment(TankState tank, IEnumerable<string> ids)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            string[] loadout = SanitizeEquipment(
                ids,
                tank.Spec.IsModern,
                tank.Combat.Magazine != null);
            tank.Equipment = (string[])loadout.Clone();
            for (int i = 0; i < loadout.Length; i++)
            {
                switch (loadout[i])
                {
                    case "rammer":
                        ScaleReload(tank, 0.9f);
                        break;
                    case "vstab":
                        tank.Combat.Equipment.Bloom *= 0.8f;
                        break;
                    case "gld":
                        tank.Combat.Equipment.AimTime *= 0.9f;
                        break;
                    case "vents":
                        ScaleReload(tank, 0.975f);
                        tank.Combat.Equipment.AimTime *= 0.975f;
                        tank.Combat.Equipment.ViewRange *= 1.025f;
                        tank.Combat.Equipment.Camouflage += 0.02f;
                        break;
                    case "optics":
                        tank.Combat.Equipment.ViewRange *= 1.1f;
                        break;
                    case "binoculars":
                        tank.Combat.Equipment.StationaryViewRange *= 1.25f;
                        break;
                    case "camo_net":
                        tank.Combat.Equipment.StationaryCamouflage += 0.12f;
                        break;
                    case "rotation":
                        tank.TraverseMultiplier *= 1.1f;
                        tank.TurretMultiplier *= 1.1f;
                        break;
                    case "toolbox": tank.Combat.Equipment.RepairRate *= 1.25f; break;
                    case "spall_liner":
                        tank.Combat.Equipment.HeSplash *= 0.75f;
                        tank.Combat.Equipment.CrewHe *= 0.5f;
                        break;
                    case "auto_ext":
                        tank.Combat.Equipment.FireTicks *= 0.5f;
                        tank.Combat.Equipment.Extinguish *= 2f;
                        break;
                    case "fuel_safety":
                        tank.Combat.Equipment.EngineFire *= 0.5f;
                        ScaleModule(tank, "fuelTank", 1.5f);
                        break;
                    case "wet_rack": ScaleModule(tank, "ammoRack", 1.5f); break;
                    case "susp":
                        ScaleModule(tank, "trackL", 1.5f);
                        ScaleModule(tank, "trackR", 1.5f);
                        break;
                }
            }
        }

        public static bool IsEquipmentAllowed(
            string id,
            bool modern,
            bool autoloader)
        {
            if (!IsEquipment(id)) return false;
            if ((id == "vstab" || id == "auto_ext") && !modern) return false;
            return id != "rammer" || !autoloader;
        }

        public static bool UseConsumable(
            TankState tank, ConsumableSlot slot, float timeS)
        {
            if (tank == null || tank.Destroyed) return false;
            int index = (int)slot;
            if (timeS < tank.ConsumableReadyAt[index]) return false;
            bool used = false;
            switch (slot)
            {
                case ConsumableSlot.RepairKit:
                    used = DamageSimulation.RepairAllModules(tank.Combat).Count > 0;
                    break;
                case ConsumableSlot.FirstAidKit:
                    foreach (string crew in new List<string>(tank.Combat.Crew.Keys))
                    {
                        if (!tank.Combat.Crew[crew]) { tank.Combat.Crew[crew] = true; used = true; }
                    }
                    break;
                case ConsumableSlot.FireExtinguisher:
                    used = tank.Combat.Fire.Burning;
                    tank.Combat.Fire.Burning = false;
                    tank.Combat.Fire.TicksLeft = 0;
                    tank.Combat.Fire.TickTimerS = 0f;
                    break;
            }
            if (used) tank.ConsumableReadyAt[index] = timeS + ConsumableCooldownS;
            return used;
        }

        private static void ScaleModule(TankState tank, string id, float multiplier)
        {
            DamageModuleState module;
            if (!tank.Combat.Modules.TryGetValue(id, out module)) return;
            module.MaxHealth *= multiplier;
            module.Health *= multiplier;
        }

        private static void ScaleReload(
            TankState tank,
            float multiplier)
        {
            DamageGunSpec gun = tank.DamageSpec.Gun;
            gun.ReloadS *= multiplier;
            for (int i = 0; i < gun.Shells.Count; i++)
            {
                if (gun.Shells[i].ReloadS.HasValue)
                {
                    gun.Shells[i].ReloadS =
                        gun.Shells[i].ReloadS.Value * multiplier;
                }
            }
            if (gun.Autoloader != null &&
                gun.Autoloader.FullReloadS.HasValue)
            {
                gun.Autoloader.FullReloadS =
                    gun.Autoloader.FullReloadS.Value * multiplier;
            }

            HashSet<DamageReloadState> scaled =
                new HashSet<DamageReloadState>();
            ScaleReloadState(tank.Combat.GunReload, multiplier, scaled);
            ScaleReloadState(tank.Combat.Reload, multiplier, scaled);
            for (int i = 0;
                i < tank.Combat.ReloadChannels.Length;
                i++)
            {
                ScaleReloadState(
                    tank.Combat.ReloadChannels[i],
                    multiplier,
                    scaled);
            }
        }

        private static void ScaleReloadState(
            DamageReloadState state,
            float multiplier,
            HashSet<DamageReloadState> scaled)
        {
            if (state == null || !scaled.Add(state)) return;
            state.TotalS *= multiplier;
            state.RemainingS *= multiplier;
        }

        public static bool IsEquipment(string id)
        {
            switch (id)
            {
                case "rammer": case "vstab": case "gld": case "vents":
                case "optics": case "binoculars": case "camo_net": case "rotation":
                case "susp": case "toolbox": case "spall_liner": case "wet_rack":
                case "fuel_safety": case "auto_ext": return true;
                default: return false;
            }
        }
    }
}
