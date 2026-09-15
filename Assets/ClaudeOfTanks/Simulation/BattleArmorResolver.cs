using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    internal sealed class BattleArmorResolver
    {
        private readonly BattleState _state;
        private readonly Func<float> _nextRandom;
        private readonly ArmorPlateTrace[] _plateHits =
            new ArmorPlateTrace[384];
        private readonly ArmorVolumeTrace[] _moduleHits =
            new ArmorVolumeTrace[32];
        private readonly ArmorVolumeTrace[] _crewHits =
            new ArmorVolumeTrace[16];

        public BattleArmorResolver(BattleState state)
        {
            _state = state ??
                throw new ArgumentNullException(nameof(state));
            _nextRandom = state.Random.NextFloat;
        }

        public bool TryFirstHit(
            Float3 start,
            Float3 end,
            TankState tank,
            out float fraction)
        {
            int count = TankArmorTrace.TracePlates(
                start,
                end,
                tank,
                _plateHits);
            if (count > 0)
            {
                fraction = _plateHits[0].Fraction;
                return true;
            }
            if (tank.Spec.Armor != null)
            {
                fraction = 0f;
                return false;
            }
            return SegmentSphere(
                start,
                end,
                tank.Position +
                    new Float3(0f, 1.25f, 0f),
                tank.Spec.CollisionRadiusM,
                out fraction);
        }

        public void ResolveHit(
            ShellState shell,
            TankState target)
        {
            Float3 travel = shell.Velocity.Normalized;
            Float3 targetForward =
                Float3.Forward(target.Yaw);
            ArmorShellType shellType;
            if (!Enum.TryParse(
                    shell.Spec.Type,
                    true,
                    out shellType))
            {
                shellType = ArmorShellType.AP;
            }
            Float3 traceEnd = shell.Position +
                travel * MathF.Max(
                    1f,
                    target.Spec.Armor?.BoundingRadiusM * 2f ??
                    target.Spec.CollisionRadiusM * 2f);
            int plateCount = TankArmorTrace.TracePlates(
                shell.PreviousPosition,
                traceEnd,
                target,
                _plateHits);
            ArmorHitResult armorResult = plateCount > 0
                ? ResolveArmorStack(
                    shell,
                    target,
                    shellType,
                    targetForward,
                    plateCount)
                : ResolveDirectionalFallback(
                    shell,
                    target,
                    shellType,
                    targetForward);
            Float3 outwardNormal = plateCount > 0
                ? _plateHits[0].Normal
                : (shell.Position -
                    (target.Position +
                     new Float3(0f, 1.25f, 0f)))
                    .Normalized;
            float damage = ResolveDamage(
                shell,
                target,
                shellType,
                traceEnd,
                plateCount,
                armorResult);
            target.Health = target.Combat.Health;
            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ShellHit,
                SourceId = shell.ShooterId,
                TargetId = target.Id,
                Position = shell.Position,
                Direction = travel,
                Normal = outwardNormal,
                ShellType = shell.Spec.Type,
                CaliberMm = shell.Spec.CaliberMm,
                Value = damage,
                Penetrated = armorResult.Penetrated
            });
            if (!target.Combat.Destroyed) return;
            target.Destroyed = true;
            TankState shooter =
                FindTank(shell.ShooterId);
            if (shooter != null) shooter.Kills++;
            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.TankDestroyed,
                SourceId = shell.ShooterId,
                TargetId = target.Id,
                Position = target.Position,
                Direction = travel,
                Normal = outwardNormal,
                ShellType = shell.Spec.Type,
                CaliberMm = shell.Spec.CaliberMm
            });
        }

        private float ResolveDamage(
            ShellState shell,
            TankState target,
            ArmorShellType shellType,
            Float3 traceEnd,
            int plateCount,
            ArmorHitResult armorResult)
        {
            if (armorResult.Penetrated)
            {
                float damage = shell.Spec.Damage *
                    _state.Random.Range(0.75f, 1.25f);
                DamageSimulation.DamageHealth(
                    target.Combat,
                    damage);
                ResolveInternalDamage(
                    shell,
                    target,
                    traceEnd,
                    plateCount > 0
                        ? _plateHits[0].Fraction
                        : 0f);
                return damage;
            }
            if (shellType != ArmorShellType.HE &&
                shellType != ArmorShellType.HESH)
            {
                return 0f;
            }
            return DamageSimulation.ApplyHeSplash(
                target.Combat,
                shell.Spec.Damage,
                armorResult.EffectiveThicknessMm,
                _nextRandom).Damage;
        }

        private ArmorHitResult ResolveArmorStack(
            ShellState shell,
            TankState target,
            ArmorShellType shellType,
            Float3 targetForward,
            int plateCount)
        {
            float penetration =
                ArmorSimulation.RollPenetrationMm(
                    shell.Spec,
                    shell.DistanceM,
                    _state.Random);
            float cumulative = 0f;
            ArmorHitResult result = default;
            for (int i = 0; i < plateCount; i++)
            {
                ArmorPlateTrace hit = _plateHits[i];
                ArmorPlateSpec plate =
                    new ArmorPlateSpec(
                        hit.Plate.PhysicalMm,
                        hit.Plate.KeMm,
                        hit.Plate.CeMm);
                float angle =
                    ArmorSimulation.ImpactAngleDegrees(
                        shell.Velocity,
                        hit.Normal);
                bool ricochet =
                    ArmorSimulation.WouldRicochet(
                        shellType,
                        shell.Spec.CaliberMm,
                        plate.PhysicalMm,
                        angle,
                        shell.Spec
                            .EffectiveOvermatchCaliberMm);
                if (ricochet)
                {
                    return new ArmorHitResult
                    {
                        Direction =
                            ArmorSimulation.DirectionFromHit(
                                shell.Velocity,
                                targetForward),
                        ImpactAngleDeg = angle,
                        EffectiveAngleDeg = angle,
                        EffectiveThicknessMm = cumulative,
                        PenetrationRollMm = penetration,
                        Ricocheted = true,
                        Penetrated = false
                    };
                }
                ResolveLinkedModuleDamage(
                    shell,
                    target,
                    hit.Plate.ModuleLink);
                if (string.Equals(
                        hit.Plate.Kind,
                        "era",
                        StringComparison.OrdinalIgnoreCase))
                {
                    target.Combat.EraSpent.Add(
                        hit.Plate.Name ??
                        string.Empty);
                    penetration =
                        ArmorSimulation
                            .ApplyEraPenetration(
                                shellType,
                                penetration,
                                hit.Plate
                                    .EraKeReduction,
                                hit.Plate
                                    .EraCeFlatMm,
                                shell.Spec.Tandem);
                    result = new ArmorHitResult
                    {
                        Direction =
                            ArmorSimulation.DirectionFromHit(
                                shell.Velocity,
                                targetForward),
                        ImpactAngleDeg = angle,
                        EffectiveAngleDeg = angle,
                        EffectiveThicknessMm = cumulative,
                        PenetrationRollMm = penetration,
                        Ricocheted = false,
                        Penetrated = false
                    };
                    if (penetration <= 0f)
                        return result;
                    continue;
                }
                float effectiveAngle = angle;
                float effective =
                    ArmorSimulation.EffectiveThicknessMm(
                        shellType,
                        shell.Spec.CaliberMm,
                        plate,
                        angle,
                        out effectiveAngle);
                cumulative += effective;
                result = new ArmorHitResult
                {
                    Direction =
                        ArmorSimulation.DirectionFromHit(
                            shell.Velocity,
                            targetForward),
                    ImpactAngleDeg = angle,
                    EffectiveAngleDeg = effectiveAngle,
                    EffectiveThicknessMm = cumulative,
                    PenetrationRollMm = penetration,
                    Ricocheted = false,
                    Penetrated =
                        penetration >= cumulative
                };
                if (penetration < cumulative ||
                    string.Equals(
                        hit.Plate.Kind,
                        "main",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
            }
            result.Penetrated = false;
            return result;
        }

        private ArmorHitResult ResolveDirectionalFallback(
            ShellState shell,
            TankState target,
            ArmorShellType shellType,
            Float3 targetForward)
        {
            ArmorDirection direction =
                ArmorSimulation.DirectionFromHit(
                    shell.Velocity,
                    targetForward);
            float armor =
                ArmorSimulation.SelectDirectionalArmorMm(
                    direction,
                    target.Spec.ArmorFrontMm,
                    target.Spec.ArmorSideMm,
                    target.Spec.ArmorRearMm);
            Float3 normal =
                (shell.Position -
                 (target.Position +
                  new Float3(0f, 1.25f, 0f)))
                .Normalized;
            return ArmorSimulation.ResolveHit(
                shell.Spec,
                shellType,
                shell.Spec.CaliberMm,
                new ArmorPlateSpec(armor),
                shell.Velocity,
                normal,
                targetForward,
                shell.DistanceM,
                _state.Random);
        }

        private void ResolveInternalDamage(
            ShellState shell,
            TankState target,
            Float3 traceEnd,
            float armorFraction)
        {
            int moduleCount =
                TankArmorTrace.TraceVolumes(
                    shell.PreviousPosition,
                    traceEnd,
                    target,
                    false,
                    _moduleHits);
            for (int i = 0; i < moduleCount; i++)
            {
                ArmorVolumeTrace hit = _moduleHits[i];
                float chance =
                    DamageSimulation.ModuleHitChance(
                        hit.Volume.Id);
                if (hit.ExitFraction + 0.0001f <
                        armorFraction ||
                    _state.Random.NextFloat() >= chance ||
                    !target.Combat.Modules.ContainsKey(
                        hit.Volume.Id))
                {
                    continue;
                }
                DamageSimulation.DamageModule(
                    target.Combat,
                    hit.Volume.Id,
                    MathF.Max(
                        shell.Spec.ModuleDamage,
                        shell.Spec.CaliberMm) *
                        _state.Random.Range(
                            0.75f,
                            1.25f),
                    _nextRandom);
            }
            int crewCount = TankArmorTrace.TraceVolumes(
                shell.PreviousPosition,
                traceEnd,
                target,
                true,
                _crewHits);
            for (int i = 0; i < crewCount; i++)
            {
                ArmorVolumeTrace hit = _crewHits[i];
                if (hit.ExitFraction + 0.0001f <
                        armorFraction ||
                    _state.Random.NextFloat() >= 0.33f)
                {
                    continue;
                }
                DamageSimulation.KnockOutCrew(
                    target.Combat,
                    hit.Volume.Id);
            }
        }

        private TankState FindTank(string id)
        {
            List<TankState> tanks = _state.Tanks;
            for (int i = 0; i < tanks.Count; i++)
                if (tanks[i].Id == id) return tanks[i];
            return null;
        }

        private void ResolveLinkedModuleDamage(
            ShellState shell,
            TankState target,
            string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId) ||
                !target.Combat.Modules.ContainsKey(moduleId) ||
                _state.Random.NextFloat() >=
                    DamageSimulation.ModuleHitChance(
                        moduleId))
            {
                return;
            }
            DamageSimulation.DamageModule(
                target.Combat,
                moduleId,
                MathF.Max(
                    shell.Spec.ModuleDamage,
                    shell.Spec.CaliberMm) *
                    _state.Random.Range(0.75f, 1.25f),
                _nextRandom);
        }

        private static bool SegmentSphere(
            Float3 start,
            Float3 end,
            Float3 center,
            float radius,
            out float fraction)
        {
            Float3 segment = end - start;
            Float3 offset = start - center;
            float a = Float3.Dot(segment, segment);
            float b = 2f *
                Float3.Dot(offset, segment);
            float c = Float3.Dot(offset, offset) -
                radius * radius;
            float discriminant =
                b * b - 4f * a * c;
            if (a <= 0.000001f ||
                discriminant < 0f)
            {
                fraction = 0f;
                return false;
            }
            float root = MathF.Sqrt(discriminant);
            float first = (-b - root) / (2f * a);
            float second = (-b + root) / (2f * a);
            fraction = first >= 0f && first <= 1f
                ? first
                : second;
            return fraction >= 0f &&
                fraction <= 1f;
        }
    }
}
