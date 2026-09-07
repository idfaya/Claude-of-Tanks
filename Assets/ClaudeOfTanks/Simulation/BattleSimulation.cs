using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public sealed class BattleSimulation
    {
        private readonly BattleState _state;
        private readonly Func<float> _nextRandom;
        private readonly MatchModeSimulation _matchMode;

        public BattleSimulation(BattleState state, GameModeId gameMode = GameModeId.Standard)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _nextRandom = _state.Random.NextFloat;
            _matchMode = new MatchModeSimulation(_state, gameMode);
        }

        public BattleState State => _state;
        public MatchModeState MatchMode => _matchMode.State;

        public void Step(IReadOnlyDictionary<string, TankInput> inputs, float dt)
        {
            _state.Events.Clear();

            for (int i = 0; i < _state.Tanks.Count; i++)
            {
                TankState tank = _state.Tanks[i];
                TankInput input;
                if (inputs == null || !inputs.TryGetValue(tank.Id, out input))
                {
                    input = default;
                    input.AimPoint = tank.Position + Float3.Forward(tank.Yaw + tank.TurretYaw) * 100f;
                }

                Float3 previous = tank.Position;
                TankMovement.Step(tank, input, _state.HeightField, dt);
                DamageSimulation.TickReload(tank.Combat, dt);
                tank.ReloadRemainingS = tank.Combat.Reload.RemainingS;
                DamageSimulation.AdvanceModuleRepairs(tank.Combat, dt);
                ResolveWorldBounds(tank);
                ResolveTankContacts(tank, previous);
                TryUseConsumables(tank, input);
                if (tank.Combat.Fire.Burning)
                {
                    DamageSimulation.AdvanceFire(
                        tank.Combat, dt, _nextRandom);
                    tank.Health = tank.Combat.Health;
                    tank.Destroyed = tank.Combat.Destroyed;
                }
                if (input.Fire)
                {
                    TryFire(tank, input.AimPoint);
                }
            }

            StepShells(dt);
            _state.TimeS += dt;
            _matchMode.Step(dt);
        }

        private void TryUseConsumables(TankState tank, TankInput input)
        {
            ConsumableSlot? slot = input.UseRepairKit ? ConsumableSlot.RepairKit
                : input.UseFirstAidKit ? ConsumableSlot.FirstAidKit
                : input.UseFireExtinguisher ? ConsumableSlot.FireExtinguisher
                : (ConsumableSlot?)null;
            if (!slot.HasValue || !LoadoutSimulation.UseConsumable(tank, slot.Value, _state.TimeS))
                return;
            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ConsumableUsed,
                SourceId = tank.Id,
                Value = (int)slot.Value
            });
        }

        private void TryFire(TankState tank, Float3 aimPoint)
        {
            if (tank.Destroyed ||
                tank.Combat.Reload.Kind != DamageReloadKind.Ready ||
                !DamageSimulation.ConsumeAmmunition(tank.Combat, tank.Combat.ShellSlot))
            {
                return;
            }

            float gunYaw = tank.Yaw + tank.TurretYaw;
            Float3 muzzle = tank.Position + new Float3(0f, 1.65f, 0f) + Float3.Forward(gunYaw) * 3.6f;
            Float3 direction = (aimPoint - muzzle).Normalized;
            if (direction.SqrMagnitude < 0.5f)
            {
                direction = Float3.Forward(gunYaw);
            }

            direction = BallisticsSimulation.ApplyDispersion(direction, 0.0018f, _state.Random);

            ShellState shell = new ShellState
            {
                Id = _state.NextShellId++,
                ShooterId = tank.Id,
                ShooterTeam = tank.Team,
                Spec = tank.Spec.Shell,
                Position = muzzle,
                PreviousPosition = muzzle,
                Velocity = direction * tank.Spec.Shell.VelocityMps
            };
            _state.Shells.Add(shell);
            DamageSimulation.StartPostShotReload(tank.Combat, tank.DamageSpec);
            tank.ReloadRemainingS = tank.Combat.Reload.RemainingS;
            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ShellFired,
                SourceId = tank.Id,
                Position = muzzle
            });
        }

        private void StepShells(float dt)
        {
            for (int i = _state.Shells.Count - 1; i >= 0; i--)
            {
                ShellState shell = _state.Shells[i];
                BallisticsSimulation.Step(shell, dt);

                TankState target = FindShellTarget(shell);
                if (target != null)
                {
                    ResolveHit(shell, target);
                    shell.Dead = true;
                }
                else if (shell.Position.Y <= _state.HeightField.HeightAt(shell.Position.X, shell.Position.Z) ||
                         shell.Dead)
                {
                    shell.Dead = true;
                }

                if (shell.Dead)
                {
                    _state.Shells.RemoveAt(i);
                }
            }
        }

        private TankState FindShellTarget(ShellState shell)
        {
            TankState best = null;
            float bestT = float.MaxValue;
            for (int i = 0; i < _state.Tanks.Count; i++)
            {
                TankState tank = _state.Tanks[i];
                if (tank.Destroyed || tank.Team == shell.ShooterTeam || tank.Id == shell.ShooterId)
                {
                    continue;
                }

                float t;
                Float3 center = tank.Position + new Float3(0f, 1.25f, 0f);
                if (SegmentSphere(shell.PreviousPosition, shell.Position, center, tank.Spec.CollisionRadiusM, out t) &&
                    t < bestT)
                {
                    bestT = t;
                    best = tank;
                }
            }

            return best;
        }

        private void ResolveHit(ShellState shell, TankState target)
        {
            Float3 travel = shell.Velocity.Normalized;
            Float3 targetForward = Float3.Forward(target.Yaw);
            ArmorDirection direction = ArmorSimulation.DirectionFromHit(travel, targetForward);
            float armor = ArmorSimulation.SelectDirectionalArmorMm(
                direction, target.Spec.ArmorFrontMm, target.Spec.ArmorSideMm, target.Spec.ArmorRearMm);
            Float3 center = target.Position + new Float3(0f, 1.25f, 0f);
            Float3 outwardNormal = (shell.Position - center).Normalized;
            ArmorShellType shellType;
            if (!Enum.TryParse(shell.Spec.Type, true, out shellType)) shellType = ArmorShellType.AP;
            ArmorHitResult armorResult = ArmorSimulation.ResolveHit(
                shell.Spec,
                shellType,
                shell.Spec.CaliberMm,
                new ArmorPlateSpec(armor),
                travel,
                outwardNormal,
                targetForward,
                shell.DistanceM,
                _state.Random);
            bool penetrated = armorResult.Penetrated;
            float damage = penetrated ? shell.Spec.Damage * _state.Random.Range(0.9f, 1.1f) : 0f;
            DamageSimulation.DamageHealth(target.Combat, damage);
            if (penetrated)
            {
                float moduleRoll = _state.Random.NextFloat();
                string moduleId = moduleRoll < 0.2f ? "ammoRack"
                    : moduleRoll < 0.4f ? "engine"
                    : moduleRoll < 0.6f ? "gun"
                    : moduleRoll < 0.8f ? "trackL"
                    : "trackR";
                DamageSimulation.DamageModule(
                    target.Combat,
                    moduleId,
                    shell.Spec.Damage * 0.35f,
                    _nextRandom);
            }
            target.Health = target.Combat.Health;

            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ShellHit,
                SourceId = shell.ShooterId,
                TargetId = target.Id,
                Position = shell.Position,
                Value = damage,
                Penetrated = penetrated
            });

            if (target.Combat.Destroyed)
            {
                target.Destroyed = true;
                TankState shooter = FindTank(shell.ShooterId);
                if (shooter != null)
                {
                    shooter.Kills++;
                }

                _state.Events.Add(new BattleEvent
                {
                    Type = BattleEventType.TankDestroyed,
                    SourceId = shell.ShooterId,
                    TargetId = target.Id,
                    Position = target.Position
                });
            }
        }

        public static float PenetrationAtDistance(ShellSpec spec, float distanceM)
        {
            return BallisticsSimulation.PenetrationAtDistance(spec, distanceM);
        }

        private TankState FindTank(string id)
        {
            for (int i = 0; i < _state.Tanks.Count; i++)
            {
                if (_state.Tanks[i].Id == id)
                {
                    return _state.Tanks[i];
                }
            }

            return null;
        }

        private void ResolveWorldBounds(TankState tank)
        {
            Float3 position = tank.Position;
            float limit = _state.WorldHalfExtentM - tank.Spec.CollisionRadiusM;
            position.X = MathUtil.Clamp(position.X, -limit, limit);
            position.Z = MathUtil.Clamp(position.Z, -limit, limit);
            tank.Position = position;
        }

        private void ResolveTankContacts(TankState tank, Float3 previous)
        {
            for (int i = 0; i < _state.Tanks.Count; i++)
            {
                TankState other = _state.Tanks[i];
                if (ReferenceEquals(tank, other) || other.Destroyed)
                {
                    continue;
                }

                float minDistance = tank.Spec.CollisionRadiusM + other.Spec.CollisionRadiusM;
                Float3 delta = tank.Position - other.Position;
                delta.Y = 0f;
                if (delta.SqrMagnitude < minDistance * minDistance)
                {
                    tank.Position = previous;
                    tank.SpeedMps = 0f;
                    return;
                }
            }
        }

        private static bool SegmentSphere(Float3 start, Float3 end, Float3 center, float radius, out float t)
        {
            Float3 segment = end - start;
            Float3 offset = start - center;
            float a = Float3.Dot(segment, segment);
            float b = 2f * Float3.Dot(offset, segment);
            float c = Float3.Dot(offset, offset) - radius * radius;
            float discriminant = b * b - 4f * a * c;
            if (a <= 0.000001f || discriminant < 0f)
            {
                t = 0f;
                return false;
            }

            float root = MathF.Sqrt(discriminant);
            float first = (-b - root) / (2f * a);
            float second = (-b + root) / (2f * a);
            t = first >= 0f && first <= 1f ? first : second;
            return t >= 0f && t <= 1f;
        }
    }
}
