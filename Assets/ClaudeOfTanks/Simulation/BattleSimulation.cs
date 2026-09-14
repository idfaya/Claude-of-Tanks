using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public sealed class BattleSimulation
    {
        private readonly BattleState _state;
        private readonly Func<float> _nextRandom;
        private readonly MatchModeSimulation _matchMode;
        private readonly BattleArmorResolver _armor;

        public BattleSimulation(BattleState state, GameModeId gameMode = GameModeId.Standard)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _nextRandom = _state.Random.NextFloat;
            _matchMode = new MatchModeSimulation(_state, gameMode);
            _armor = new BattleArmorResolver(_state);
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
                ResolveStaticObstacleContacts(tank, previous);
                ResolveTankContacts(tank, previous);
                TryUseConsumables(tank, input);
                SelectShell(tank, input.ShellSlot);
                if (tank.Combat.Fire.Burning)
                {
                    DamageSimulation.AdvanceFire(
                        tank.Combat, dt, _nextRandom);
                    tank.Health = tank.Combat.Health;
                    tank.Destroyed = tank.Combat.Destroyed;
                }
                if (input.Fire)
                {
                    TryFire(
                        tank,
                        input.AimPoint);
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

        private static void SelectShell(
            TankState tank,
            int requestedSlot)
        {
            int slot = Math.Max(
                0,
                Math.Min(
                    tank.Spec.Shells.Length - 1,
                    requestedSlot));
            if (slot != tank.Combat.ShellSlot)
            {
                DamageSimulation.SelectShell(
                    tank.Combat,
                    slot,
                    tank.DamageSpec);
            }
        }

        private void TryFire(
            TankState tank,
            Float3 aimPoint)
        {
            DamageModuleState gun;
            if (tank.Destroyed ||
                (tank.Combat.Modules.TryGetValue("gun", out gun) &&
                 gun.Condition == DamageModuleCondition.Red) ||
                tank.Combat.Reload.Kind != DamageReloadKind.Ready ||
                !DamageSimulation.ConsumeAmmunition(
                    tank.Combat,
                    tank.Combat.ShellSlot))
            {
                return;
            }
            ShellSpec shellSpec =
                tank.Spec.Shells[tank.Combat.ShellSlot];

            float gunYaw = tank.Yaw + tank.TurretYaw;
            Float3 muzzle = tank.Position + new Float3(0f, 1.65f, 0f) + Float3.Forward(gunYaw) * 3.6f;
            Float3 direction = (aimPoint - muzzle).Normalized;
            if (direction.SqrMagnitude < 0.5f)
            {
                direction = Float3.Forward(gunYaw);
            }

            direction = BallisticsSimulation.ApplyDispersion(
                direction,
                TankMovement.DispersionSigmaRad(tank),
                _state.Random);

            ShellState shell = new ShellState
            {
                Id = _state.NextShellId++,
                ShooterId = tank.Id,
                ShooterTeam = tank.Team,
                Spec = shellSpec,
                Position = muzzle,
                PreviousPosition = muzzle,
                Velocity = direction * shellSpec.VelocityMps
            };
            _state.Shells.Add(shell);
            TankMovement.ApplyPostShotBloom(tank);
            DamageSimulation.StartPostShotReload(tank.Combat, tank.DamageSpec);
            tank.ReloadRemainingS = tank.Combat.Reload.RemainingS;
            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ShellFired,
                SourceId = tank.Id,
                Position = muzzle,
                Direction = direction,
                ShellType = shellSpec.Type,
                CaliberMm = shellSpec.CaliberMm
            });
        }

        private void StepShells(float dt)
        {
            for (int i = _state.Shells.Count - 1; i >= 0; i--)
            {
                ShellState shell = _state.Shells[i];
                BallisticsSimulation.Step(shell, dt);

                float targetFraction;
                TankState target = FindShellTarget(
                    shell,
                    out targetFraction);
                int obstacleIndex;
                StaticObstacle obstacle;
                float obstacleFraction;
                Float3 obstacleNormal;
                bool obstacleHit = _state.TryFindFirstStaticObstacleHit(
                    StaticObstacleFlags.Shells,
                    shell.PreviousPosition,
                    shell.Position,
                    out obstacleIndex,
                    out obstacle,
                    out obstacleFraction,
                    out obstacleNormal);
                if (obstacleHit && (target == null || obstacleFraction <= targetFraction))
                {
                    shell.Position = PointOnSegment(
                        shell.PreviousPosition, shell.Position, obstacleFraction);
                    if (obstacle.Crushable)
                    {
                        CrushObstacle(
                            obstacleIndex,
                            shell.ShooterId,
                            shell.Position,
                            shell.Velocity.Normalized,
                            shell.Spec.CaliberMm);
                        shell.Dead = true;
                        _state.Shells.RemoveAt(i);
                        continue;
                    }
                    _state.Events.Add(new BattleEvent
                    {
                        Type = BattleEventType.StructureHit,
                        SourceId = shell.ShooterId,
                        TargetId = obstacle.Id,
                        Position = shell.Position,
                        Direction = shell.Velocity.Normalized,
                        Normal = obstacleNormal,
                        ShellType = shell.Spec.Type,
                        CaliberMm = shell.Spec.CaliberMm,
                        Value = shell.Spec.Damage
                    });
                    if (_state.DamageStaticObstacle(obstacleIndex, shell.Spec.Damage))
                    {
                        _state.Events.Add(new BattleEvent
                        {
                            Type = BattleEventType.StructureDestroyed,
                            SourceId = shell.ShooterId,
                            TargetId = obstacle.Id,
                            Position = shell.Position,
                            Direction = shell.Velocity.Normalized,
                            Normal = obstacleNormal,
                            ShellType = shell.Spec.Type,
                            CaliberMm = shell.Spec.CaliberMm
                        });
                    }
                    shell.Dead = true;
                }
                else if (target != null)
                {
                    shell.Position = PointOnSegment(
                        shell.PreviousPosition, shell.Position, targetFraction);
                    _armor.ResolveHit(shell, target);
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

        private TankState FindShellTarget(ShellState shell, out float bestT)
        {
            TankState best = null;
            bestT = float.MaxValue;
            for (int i = 0; i < _state.Tanks.Count; i++)
            {
                TankState tank = _state.Tanks[i];
                if (tank.Destroyed || tank.Team == shell.ShooterTeam || tank.Id == shell.ShooterId)
                {
                    continue;
                }

                float t;
                if (!_armor.TryFirstHit(
                        shell.PreviousPosition,
                        shell.Position,
                        tank,
                        out t))
                {
                    continue;
                }
                if (t < bestT)
                {
                    bestT = t;
                    best = tank;
                }
            }

            return best;
        }

        public static float PenetrationAtDistance(ShellSpec spec, float distanceM)
        {
            return BallisticsSimulation.PenetrationAtDistance(spec, distanceM);
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

        private void ResolveStaticObstacleContacts(TankState tank, Float3 previous)
        {
            if (tank.Destroyed) return;
            int crushed = 0;
            while (crushed < 16)
            {
                int obstacleIndex;
                if (!_state.TryFindStaticObstacleOverlap(
                        StaticObstacleFlags.Movement,
                        tank.Position,
                        tank.Spec.CollisionRadiusM,
                        out obstacleIndex))
                {
                    return;
                }
                StaticObstacle obstacle = _state.StaticObstacles[obstacleIndex];
                if (obstacle.Crushable)
                {
                    float directionSign = tank.SpeedMps < 0f ? -1f : 1f;
                    Float3 direction =
                        Float3.Forward(tank.Yaw) * directionSign;
                    CrushObstacle(
                        obstacleIndex,
                        tank.Id,
                        obstacle.Center,
                        direction,
                        0f);
                    tank.SpeedMps *= obstacle.CrushSpeedRetention;
                    crushed++;
                    continue;
                }
                tank.Position = previous;
                tank.SpeedMps = 0f;
                return;
            }
        }

        private void CrushObstacle(
            int obstacleIndex,
            string sourceId,
            Float3 position,
            Float3 direction,
            float caliberMm)
        {
            StaticObstacle obstacle = _state.StaticObstacles[obstacleIndex];
            if (!_state.DamageStaticObstacle(obstacleIndex, float.MaxValue)) return;
            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.PropCrushed,
                SourceId = sourceId,
                TargetId = obstacle.Id,
                Position = position,
                Direction = direction,
                Normal = new Float3(0f, 1f, 0f),
                CaliberMm = caliberMm
            });
        }

        private static Float3 PointOnSegment(Float3 start, Float3 end, float fraction)
        {
            return start + (end - start) * fraction;
        }

    }
}
