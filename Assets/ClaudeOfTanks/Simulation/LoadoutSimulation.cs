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
                if (!IsEquipment(id) || result.Contains(id)) continue;
                if ((id == "vstab" || id == "auto_ext") && !modern) continue;
                if (id == "rammer" && autoloader) continue;
                result.Add(id);
                if (result.Count == EquipmentSlots) break;
            }
            return result.ToArray();
        }

        public static void ApplyEquipment(TankState tank, IEnumerable<string> ids)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            string[] loadout = SanitizeEquipment(ids, true, tank.Combat.Magazine != null);
            for (int i = 0; i < loadout.Length; i++)
            {
                switch (loadout[i])
                {
                    case "rammer":
                        tank.DamageSpec.Gun.ReloadS *= 0.9f;
                        tank.Combat.Reload.TotalS *= 0.9f;
                        tank.Combat.GunReload.TotalS *= 0.9f;
                        break;
                    case "rotation":
                        tank.TraverseMultiplier *= 1.1f;
                        tank.TurretMultiplier *= 1.1f;
                        break;
                    case "toolbox": tank.Combat.Equipment.RepairRate *= 1.25f; break;
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

        private static bool IsEquipment(string id)
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
