using System;

namespace ClaudeOfTanks.Runtime
{
    public enum CrewVoiceId
    {
        BattleStart,
        Victory,
        Defeat,
        Draw,
        EnemySpotted,
        SixthSense,
        Firing,
        Penetration,
        Ricochet,
        EnemyCrit,
        EnemyAmmoRack,
        TargetDestroyed,
        WereHit,
        BouncedUs,
        LowHp,
        AmmoRack,
        FuelTank,
        Fire,
        FireOut,
        EngineDamaged,
        TrackGone,
        GunDamaged,
        OpticsDamaged,
        RadioDamaged,
        CommanderDown,
        GunnerDown,
        DriverDown,
        LoaderDown,
        Reloading,
        Reloaded,
        OnTheMove,
        Repairs,
        CrewRecovered,
        TrackRepaired,
        GunRepaired,
        EngineRepaired
    }

    public enum CrewVoiceGroup
    {
        Flow,
        Result,
        Awareness,
        GunCycle,
        ShotResult,
        Incoming,
        Damage,
        Recovery
    }

    public sealed class BattleCrewVoiceLine
    {
        public readonly CrewVoiceId Id;
        public readonly string[] Files;
        public readonly int Priority;
        public readonly float CooldownS;
        public readonly CrewVoiceGroup Group;
        public readonly float GroupCooldownS;
        public readonly float StaleS;

        public BattleCrewVoiceLine(
            CrewVoiceId id,
            string files,
            int priority,
            float cooldownS,
            CrewVoiceGroup group,
            float groupCooldownS = 0f,
            float staleS = 1.2f)
        {
            Id = id;
            Files = files.Split(',');
            Priority = priority;
            CooldownS = cooldownS;
            Group = group;
            GroupCooldownS = groupCooldownS;
            StaleS = staleS;
        }
    }

    public static class BattleCrewVoiceCatalog
    {
        private static readonly BattleCrewVoiceLine[] Lines =
        {
            L(CrewVoiceId.BattleStart, "battle_start,battle_start_b,battle_start_c", 2, 8f, CrewVoiceGroup.Flow, stale: 1.2f),
            L(CrewVoiceId.Victory, "victory,victory_b", 4, 10f, CrewVoiceGroup.Result, stale: 8f),
            L(CrewVoiceId.Defeat, "defeat,defeat_b", 4, 10f, CrewVoiceGroup.Result, stale: 8f),
            L(CrewVoiceId.Draw, "draw", 4, 10f, CrewVoiceGroup.Result, stale: 8f),
            L(CrewVoiceId.EnemySpotted, "enemy_spotted,enemy_spotted_b,enemy_spotted_c", 1, 10f, CrewVoiceGroup.Awareness, 5f, 0.9f),
            L(CrewVoiceId.SixthSense, "sixth_sense,sixth_sense_b,sixth_sense_c", 3, 14f, CrewVoiceGroup.Awareness, stale: 1f),
            L(CrewVoiceId.Firing, "firing,firing_b,firing_c", 0, 12f, CrewVoiceGroup.GunCycle, stale: 0.35f),
            L(CrewVoiceId.Penetration, "penetration,penetration_b,penetration_c", 1, 6f, CrewVoiceGroup.ShotResult, 3.5f, 0.8f),
            L(CrewVoiceId.Ricochet, "ricochet,ricochet_b", 1, 5f, CrewVoiceGroup.ShotResult, 3.5f, 0.8f),
            L(CrewVoiceId.EnemyCrit, "enemy_crit,enemy_crit_b,enemy_crit_c", 1, 8f, CrewVoiceGroup.ShotResult, 3.5f, 0.8f),
            L(CrewVoiceId.EnemyAmmoRack, "enemy_ammo_rack", 2, 12f, CrewVoiceGroup.ShotResult, stale: 1f),
            L(CrewVoiceId.TargetDestroyed, "target_destroyed,target_destroyed_b,target_destroyed_c,target_destroyed_d", 3, 3.5f, CrewVoiceGroup.ShotResult, stale: 2f),
            L(CrewVoiceId.WereHit, "were_hit,were_hit_b,were_hit_c", 2, 6f, CrewVoiceGroup.Incoming, stale: 0.8f),
            L(CrewVoiceId.BouncedUs, "bounced_us,bounced_us_b,bounced_us_c", 2, 6f, CrewVoiceGroup.Incoming, stale: 0.8f),
            L(CrewVoiceId.LowHp, "low_hp,low_hp_b", 3, 25f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.AmmoRack, "ammo_rack,ammo_rack_b", 4, 8f, CrewVoiceGroup.Damage, stale: 1.3f),
            L(CrewVoiceId.FuelTank, "fuel_tank,fuel_tank_b", 3, 10f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.Fire, "fire,fire_b", 4, 10f, CrewVoiceGroup.Damage, stale: 1.3f),
            L(CrewVoiceId.FireOut, "fire_out,fire_out_b", 2, 10f, CrewVoiceGroup.Recovery, stale: 1.2f),
            L(CrewVoiceId.EngineDamaged, "engine_damaged,engine_damaged_b", 3, 8f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.TrackGone, "track_gone,track_gone_b", 3, 6f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.GunDamaged, "gun_damaged,gun_damaged_b", 3, 8f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.OpticsDamaged, "optics_damaged,optics_damaged_b", 3, 10f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.RadioDamaged, "radio_damaged,radio_damaged_b", 3, 10f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.CommanderDown, "commander_down,commander_down_b", 3, 12f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.GunnerDown, "gunner_down,gunner_down_b", 3, 12f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.DriverDown, "driver_down,driver_down_b", 3, 12f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.LoaderDown, "loader_down,loader_down_b", 3, 12f, CrewVoiceGroup.Damage, stale: 1.1f),
            L(CrewVoiceId.Reloading, "reloading,reloading_b", 0, 9f, CrewVoiceGroup.GunCycle, stale: 0.45f),
            L(CrewVoiceId.Reloaded, "reloaded,reloaded_b,reloaded_c", 0, 3f, CrewVoiceGroup.GunCycle, stale: 0.45f),
            L(CrewVoiceId.OnTheMove, "on_the_move,on_the_move_b", 1, 15f, CrewVoiceGroup.Flow, stale: 1f),
            L(CrewVoiceId.Repairs, "repairs,repairs_b,repairs_c", 1, 8f, CrewVoiceGroup.Recovery, stale: 1.2f),
            L(CrewVoiceId.CrewRecovered, "crew_recovered,crew_recovered_b", 1, 8f, CrewVoiceGroup.Recovery, stale: 1.2f),
            L(CrewVoiceId.TrackRepaired, "track_repaired,track_repaired_b", 1, 8f, CrewVoiceGroup.Recovery, stale: 1.2f),
            L(CrewVoiceId.GunRepaired, "gun_repaired,gun_repaired_b", 1, 8f, CrewVoiceGroup.Recovery, stale: 1.2f),
            L(CrewVoiceId.EngineRepaired, "engine_repaired,engine_repaired_b", 1, 8f, CrewVoiceGroup.Recovery, stale: 1.2f)
        };

        public static int LineCount => Lines.Length;
        public static int VariantCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Lines.Length; i++)
                    count += Lines[i].Files.Length;
                return count;
            }
        }

        public static BattleCrewVoiceLine Get(CrewVoiceId id)
        {
            int index = (int)id;
            if (index < 0 || index >= Lines.Length ||
                Lines[index].Id != id)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            return Lines[index];
        }

        private static BattleCrewVoiceLine L(
            CrewVoiceId id,
            string files,
            int priority,
            float cooldown,
            CrewVoiceGroup group,
            float groupCooldown = 0f,
            float stale = 1.2f)
        {
            return new BattleCrewVoiceLine(
                id,
                files,
                priority,
                cooldown,
                group,
                groupCooldown,
                stale);
        }
    }
}
