using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleCrewVoiceDirector
    {
        private readonly BattleCrewVoice _voice;
        private readonly Dictionary<string, DamageModuleCondition> _modules =
            new Dictionary<string, DamageModuleCondition>(
                StringComparer.Ordinal);
        private readonly Dictionary<string, bool> _crew =
            new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly HashSet<string> _seenEnemies =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly SpottingSimulation _spotting =
            new SpottingSimulation();
        private string _tankId;
        private bool _burning;
        private bool _lowHpCalled;
        private bool _reloadActive;
        private bool _moving;
        private float _reloadTotal;
        private string _presentedResult;
        private bool _awarenessInitialized;
        private bool _viewerSpotted;
        private bool _sixthSensePending;
        private float _spottedAt;

        public BattleCrewVoiceDirector(BattleCrewVoice voice)
        {
            _voice = voice ??
                throw new ArgumentNullException(nameof(voice));
        }

        public void BeginBattle(float now)
        {
            ResetState();
            _voice.Request(
                CrewVoiceId.BattleStart,
                now,
                delayS: 0.48f);
        }

        public void Handle(
            BattleEvent battleEvent,
            string listenerOwnerId,
            TankState occupied,
            float now)
        {
            if (string.IsNullOrEmpty(listenerOwnerId)) return;
            if (battleEvent.Type == BattleEventType.ShellFired &&
                battleEvent.SourceId == listenerOwnerId)
            {
                _voice.Request(CrewVoiceId.Firing, now);
                return;
            }
            if (battleEvent.Type == BattleEventType.ShellHit)
            {
                if (battleEvent.TargetId == listenerOwnerId)
                {
                    CrewVoiceId? damage = CaptureEdges(occupied);
                    _voice.Request(
                        damage ?? (battleEvent.Penetrated
                            ? CrewVoiceId.WereHit
                            : CrewVoiceId.BouncedUs),
                        now,
                        delayS: 0.12f);
                }
                else if (battleEvent.SourceId == listenerOwnerId)
                {
                    _voice.Request(
                        battleEvent.Penetrated && battleEvent.Value > 0f
                            ? CrewVoiceId.Penetration
                            : CrewVoiceId.Ricochet,
                        now,
                        delayS: battleEvent.Penetrated ? 0.18f : 0.16f);
                }
                return;
            }
            if (battleEvent.Type == BattleEventType.TankDestroyed)
            {
                if (battleEvent.TargetId == listenerOwnerId)
                {
                    _voice.Silence(now);
                }
                else if (battleEvent.SourceId == listenerOwnerId)
                {
                    _voice.Request(
                        CrewVoiceId.TargetDestroyed,
                        now,
                        delayS: 0.22f);
                }
                return;
            }
            if (battleEvent.Type == BattleEventType.ConsumableUsed &&
                battleEvent.SourceId == listenerOwnerId)
            {
                int slot = (int)battleEvent.Value;
                CrewVoiceId id = slot == (int)ConsumableSlot.FirstAidKit
                    ? CrewVoiceId.CrewRecovered
                    : slot == (int)ConsumableSlot.FireExtinguisher
                        ? CrewVoiceId.FireOut
                        : CrewVoiceId.Repairs;
                _voice.Request(id, now, delayS: 0.12f);
            }
        }

        public void Sync(TankState occupied, float now)
        {
            if (occupied == null || occupied.Destroyed)
            {
                if (occupied != null && occupied.Id == _tankId)
                    _voice.Silence(now);
                return;
            }
            if (_tankId != occupied.Id)
            {
                CaptureInitial(occupied);
                return;
            }
            CrewVoiceId? edge = CaptureEdges(occupied);
            if (edge.HasValue)
                _voice.Request(edge.Value, now, delayS: 0.1f);
            SyncReload(occupied, now);
            bool moving = Math.Abs(occupied.SpeedMps) > 0.8f;
            if (moving && !_moving)
                _voice.Request(CrewVoiceId.OnTheMove, now);
            _moving = moving;
        }

        public void PresentResult(string result, float now)
        {
            string normalized = (result ?? "draw").ToLowerInvariant();
            if (_presentedResult == normalized) return;
            _presentedResult = normalized;
            _voice.CancelPending(
                CrewVoiceGroup.ShotResult,
                stopObsoleteActive: true,
                now: now);
            CrewVoiceId id = normalized.Contains("victory")
                ? CrewVoiceId.Victory
                : normalized.Contains("defeat")
                    ? CrewVoiceId.Defeat
                    : CrewVoiceId.Draw;
            _voice.Request(id, now, delayS: 0.2f);
        }

        public void SyncAwareness(
            IList<TankState> tanks,
            string listenerOwnerId,
            Func<Float3, Float3, bool> isOccluded,
            bool? viewerSpotted,
            float now)
        {
            if (tanks == null || string.IsNullOrEmpty(listenerOwnerId))
                return;
            TankState occupied = null;
            for (int i = 0; i < tanks.Count; i++)
            {
                if (tanks[i] != null && tanks[i].Id == listenerOwnerId)
                {
                    occupied = tanks[i];
                    break;
                }
            }
            if (occupied == null || occupied.Destroyed) return;

            bool newlyVisible = false;
            bool locallySpotted = false;
            for (int i = 0; i < tanks.Count; i++)
            {
                TankState enemy = tanks[i];
                if (enemy == null || enemy.Destroyed ||
                    enemy.Team == occupied.Team)
                {
                    continue;
                }
                bool visible = isOccluded == null ||
                    _spotting.CanSpot(occupied, enemy, isOccluded);
                if (visible && _seenEnemies.Add(enemy.Id) &&
                    _awarenessInitialized)
                {
                    newlyVisible = true;
                }
                if (!viewerSpotted.HasValue &&
                    _spotting.CanSpot(enemy, occupied, isOccluded))
                {
                    locallySpotted = true;
                }
            }
            if (!_awarenessInitialized)
                _awarenessInitialized = true;
            else if (newlyVisible)
                _voice.Request(
                    CrewVoiceId.EnemySpotted,
                    now,
                    delayS: 0.1f);

            bool spotted = viewerSpotted ?? locallySpotted;
            if (spotted && !_viewerSpotted)
            {
                _spottedAt = now;
                _sixthSensePending = true;
            }
            else if (!spotted)
            {
                _sixthSensePending = false;
            }
            if (spotted && _sixthSensePending &&
                now >= _spottedAt + 3f)
            {
                _voice.Request(CrewVoiceId.SixthSense, now);
                _sixthSensePending = false;
            }
            _viewerSpotted = spotted;
        }

        public void ResetAll()
        {
            ResetState();
            _voice.ResetAll();
        }

        private CrewVoiceId? CaptureEdges(TankState tank)
        {
            if (tank == null) return null;
            if (_tankId != tank.Id)
            {
                CaptureInitial(tank);
                return null;
            }

            CrewVoiceId? best = null;
            DamageCombatState combat = tank.Combat;
            foreach (KeyValuePair<string, DamageModuleState> entry
                in combat.Modules)
            {
                DamageModuleCondition previous;
                if (!_modules.TryGetValue(entry.Key, out previous))
                {
                    _modules[entry.Key] = entry.Value.Condition;
                    continue;
                }
                DamageModuleCondition current = entry.Value.Condition;
                if (current > previous &&
                    (current == DamageModuleCondition.Red ||
                     !IsTrack(entry.Key)))
                {
                    best = Higher(best, DamageCall(entry.Key));
                }
                else if (current == DamageModuleCondition.Ok &&
                    previous != DamageModuleCondition.Ok)
                {
                    best = Higher(best, RepairCall(entry.Key));
                }
                _modules[entry.Key] = current;
            }
            foreach (KeyValuePair<string, bool> entry in combat.Crew)
            {
                bool previous;
                if (!_crew.TryGetValue(entry.Key, out previous))
                {
                    _crew[entry.Key] = entry.Value;
                    continue;
                }
                if (previous && !entry.Value)
                    best = Higher(best, CrewDownCall(entry.Key));
                else if (!previous && entry.Value)
                    best = Higher(best, CrewVoiceId.CrewRecovered);
                _crew[entry.Key] = entry.Value;
            }
            if (!_burning && combat.Fire.Burning)
                best = Higher(best, CrewVoiceId.Fire);
            else if (_burning && !combat.Fire.Burning)
                best = Higher(best, CrewVoiceId.FireOut);
            _burning = combat.Fire.Burning;

            float healthRatio = combat.MaxHealth > 0f
                ? combat.Health / combat.MaxHealth
                : 0f;
            if (!_lowHpCalled && healthRatio > 0f && healthRatio <= 0.25f)
            {
                _lowHpCalled = true;
                best = Higher(best, CrewVoiceId.LowHp);
            }
            return best;
        }

        private void CaptureInitial(TankState tank)
        {
            ResetState();
            _tankId = tank.Id;
            foreach (KeyValuePair<string, DamageModuleState> entry
                in tank.Combat.Modules)
            {
                _modules[entry.Key] = entry.Value.Condition;
            }
            foreach (KeyValuePair<string, bool> entry in tank.Combat.Crew)
                _crew[entry.Key] = entry.Value;
            _burning = tank.Combat.Fire.Burning;
            _lowHpCalled = tank.Combat.MaxHealth > 0f &&
                tank.Combat.Health / tank.Combat.MaxHealth <= 0.25f;
            float remaining = ReloadRemaining(tank);
            _reloadActive = remaining > 0f;
            _reloadTotal = ReloadTotal(tank);
            _moving = Math.Abs(tank.SpeedMps) > 0.8f;
        }

        private void SyncReload(TankState tank, float now)
        {
            float remaining = ReloadRemaining(tank);
            bool active = remaining > 0f;
            float total = ReloadTotal(tank);
            if (!_reloadActive && active && total >= 2.2f)
            {
                _voice.Request(
                    CrewVoiceId.Reloading,
                    now,
                    delayS: 0.1f);
            }
            else if (_reloadActive && !active && _reloadTotal >= 1.25f)
            {
                _voice.Request(
                    CrewVoiceId.Reloaded,
                    now,
                    delayS: 0.08f);
            }
            _reloadActive = active;
            if (active) _reloadTotal = total;
        }

        private void ResetState()
        {
            _modules.Clear();
            _crew.Clear();
            _tankId = null;
            _burning = false;
            _lowHpCalled = false;
            _reloadActive = false;
            _reloadTotal = 0f;
            _moving = false;
            _presentedResult = null;
            _seenEnemies.Clear();
            _awarenessInitialized = false;
            _viewerSpotted = false;
            _sixthSensePending = false;
            _spottedAt = 0f;
        }

        private static float ReloadRemaining(TankState tank)
        {
            DamageReloadState reload = tank.Combat.Reload;
            return reload != null && reload.RemainingS > 0f
                ? reload.RemainingS
                : tank.ReloadRemainingS;
        }

        private static float ReloadTotal(TankState tank)
        {
            DamageReloadState reload = tank.Combat.Reload;
            if (reload != null && reload.TotalS > 0f)
                return reload.TotalS;
            DamageReloadKind kind =
                BattleReloadAudioPolicy.InferKind(
                    tank.Spec,
                    tank.ReloadRemainingS);
            return BattleReloadAudioPolicy.InferTotal(tank.Spec, kind);
        }

        private static CrewVoiceId? Higher(
            CrewVoiceId? current,
            CrewVoiceId? candidate)
        {
            if (!candidate.HasValue) return current;
            if (!current.HasValue) return candidate;
            return BattleCrewVoiceCatalog.Get(candidate.Value).Priority >
                BattleCrewVoiceCatalog.Get(current.Value).Priority
                    ? candidate
                    : current;
        }

        private static CrewVoiceId? DamageCall(string id)
        {
            if (id == "ammoRack") return CrewVoiceId.AmmoRack;
            if (id == "fuelTank") return CrewVoiceId.FuelTank;
            if (id == "engine" || id == "transmission")
                return CrewVoiceId.EngineDamaged;
            if (IsTrack(id)) return CrewVoiceId.TrackGone;
            if (id == "optics") return CrewVoiceId.OpticsDamaged;
            if (id == "radio") return CrewVoiceId.RadioDamaged;
            if (id == "gun" || id == "gunMount" ||
                id == "turretRing" || id == "autoloader" ||
                id == "feedSystem" || id == "missileRack")
            {
                return CrewVoiceId.GunDamaged;
            }
            return null;
        }

        private static CrewVoiceId? RepairCall(string id)
        {
            if (id == "engine" || id == "transmission")
                return CrewVoiceId.EngineRepaired;
            if (IsTrack(id)) return CrewVoiceId.TrackRepaired;
            if (id == "gun" || id == "gunMount" ||
                id == "turretRing" || id == "autoloader" ||
                id == "feedSystem" || id == "missileRack")
            {
                return CrewVoiceId.GunRepaired;
            }
            return CrewVoiceId.Repairs;
        }

        private static CrewVoiceId? CrewDownCall(string id)
        {
            if (id == "commander") return CrewVoiceId.CommanderDown;
            if (id == "gunner") return CrewVoiceId.GunnerDown;
            if (id == "driver") return CrewVoiceId.DriverDown;
            if (id == "loader") return CrewVoiceId.LoaderDown;
            return null;
        }

        private static bool IsTrack(string id)
        {
            return id == "trackL" || id == "trackR";
        }
    }
}
