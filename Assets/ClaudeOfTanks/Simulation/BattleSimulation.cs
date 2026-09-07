using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public sealed class BattleSimulation
    {
        private const float GravityMps2 = 9.81f;
        private const float ShellLifetimeS = 6f;
        private const float WorldHalfExtentM = 120f;
        private readonly BattleState _state;

        public BattleSimulation(BattleState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public BattleState State => _state;

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
                ResolveWorldBounds(tank);
                ResolveTankContacts(tank, previous);
                if (input.Fire)
                {
                    TryFire(tank, input.AimPoint);
                }
            }

            StepShells(dt);
            _state.TimeS += dt;
        }

        private void TryFire(TankState tank, Float3 aimPoint)
        {
            if (tank.Destroyed || tank.ReloadRemainingS > 0f)
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

            float spread = 0.0018f;
            direction.X += _state.Random.Range(-spread, spread);
            direction.Y += _state.Random.Range(-spread, spread);
            direction.Z += _state.Random.Range(-spread, spread);
            direction = direction.Normalized;

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
            tank.ReloadRemainingS = tank.Spec.Shell.ReloadS;
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
                shell.PreviousPosition = shell.Position;
                shell.Position += shell.Velocity * dt;
                shell.Position.Y -= 0.5f * GravityMps2 * dt * dt;
                shell.Velocity.Y -= GravityMps2 * dt;
                shell.DistanceM += Float3.Distance(shell.PreviousPosition, shell.Position);
                shell.AgeS += dt;

                TankState target = FindShellTarget(shell);
                if (target != null)
                {
                    ResolveHit(shell, target);
                    shell.Dead = true;
                }
                else if (shell.Position.Y <= _state.HeightField.HeightAt(shell.Position.X, shell.Position.Z) ||
                         shell.AgeS > ShellLifetimeS)
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
            float facing = Float3.Dot(travel, targetForward);
            float armor = MathF.Abs(facing) > 0.62f
                ? (facing < 0f ? target.Spec.ArmorFrontMm : target.Spec.ArmorRearMm)
                : target.Spec.ArmorSideMm;
            float penetration = PenetrationAtDistance(shell.Spec, shell.DistanceM) *
                                _state.Random.Range(0.9f, 1.1f);
            bool penetrated = penetration >= armor;
            float damage = penetrated ? shell.Spec.Damage * _state.Random.Range(0.9f, 1.1f) : 0f;
            target.Health = MathF.Max(0f, target.Health - damage);

            _state.Events.Add(new BattleEvent
            {
                Type = BattleEventType.ShellHit,
                SourceId = shell.ShooterId,
                TargetId = target.Id,
                Position = shell.Position,
                Value = damage,
                Penetrated = penetrated
            });

            if (target.Health <= 0f)
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
            float t = MathUtil.Clamp01((distanceM - 100f) / 900f);
            return spec.Pen100Mm + (spec.Pen1000Mm - spec.Pen100Mm) * t;
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
            float limit = WorldHalfExtentM - tank.Spec.CollisionRadiusM;
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
