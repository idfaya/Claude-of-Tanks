using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed partial class MatchModeSimulation
    {
        private const float BallGravityMps2 =
            9.81f;
        private const float BallLinearDrag =
            0.992f;
        private const float BallTouchRadiusM =
            6.5f;
        private const float BallTouchCooldownS =
            0.12f;
        private const float HordeIntermissionS =
            6f;
        private const float PickupRadiusM = 7f;
        private const int HordeInitialActive = 3;
        private float _lastBallTouchS =
            -1000000000f;
        private float _nextHordeWaveAtS = -1f;
        private bool _hordeStarted;
        private int _pickupSequence;

        public bool TryHitBall(
            ShellState shell)
        {
            if (_state.Id !=
                    GameModeId.TurboBall ||
                shell == null ||
                shell.Dead ||
                SegmentDistanceSquared(
                    _state.BallPosition,
                    shell.PreviousPosition,
                    shell.Position) >
                (BallRadius + 0.35f) *
                (BallRadius + 0.35f))
            {
                return false;
            }
            Float3 direction =
                shell.Velocity.Normalized;
            _state.BallVelocity =
                new Float3(
                    direction.X * 34f,
                    MathF.Max(
                        5f,
                        direction.Y * 20f +
                        7f),
                    direction.Z * 34f);
            shell.Dead = true;
            return true;
        }

        public Float3? BotTarget(
            TankState tank)
        {
            if (tank == null ||
                tank.Destroyed ||
                !tank.ModeActive)
            {
                return null;
            }
            switch (_state.Id)
            {
                case GameModeId
                    .CaptureTheFlag:
                    if (_state
                            .AlphaFlagCarrier ==
                            tank.Id ||
                        _state
                            .BravoFlagCarrier ==
                            tank.Id)
                    {
                        return tank.Team ==
                            Team.Alpha
                                ? _alphaBase
                                : _bravoBase;
                    }
                    return tank.Team ==
                        Team.Alpha
                            ? _state.BravoFlag
                            : _state.AlphaFlag;
                case GameModeId.ZoneControl:
                    return NearestUnownedZone(
                        tank);
                case GameModeId.TurboBall:
                    return _state.BallPosition;
                case GameModeId.EndlessHorde:
                    return NearestEnemyPosition(
                        tank);
                default:
                    return null;
            }
        }

        private void StepBall(float dt)
        {
            TankState touch =
                ClosestTank(
                    _state.BallPosition,
                    BallTouchRadiusM);
            if (touch != null &&
                _battle.TimeS -
                    _lastBallTouchS >=
                BallTouchCooldownS)
            {
                Float3 forward =
                    Float3.Forward(touch.Yaw);
                Float3 drive =
                    forward *
                    touch.SpeedMps;
                Float3 offset =
                    _state.BallPosition -
                    touch.Position;
                float distance =
                    MathF.Sqrt(
                        offset.X * offset.X +
                        offset.Z * offset.Z);
                Float3 normal = distance >
                    0.01f
                        ? new Float3(
                            offset.X /
                                distance,
                            0f,
                            offset.Z /
                                distance)
                        : forward;
                float closing =
                    MathF.Max(
                        0f,
                        drive.X * normal.X +
                        drive.Z * normal.Z);
                _state.BallVelocity =
                    new Float3(
                        _state.BallVelocity.X *
                            0.42f +
                        drive.X * 0.82f +
                        normal.X *
                            (4f +
                             closing * 0.35f),
                        MathF.Max(
                            _state.BallVelocity.Y,
                            2.5f +
                            closing * 0.12f),
                        _state.BallVelocity.Z *
                            0.42f +
                        drive.Z * 0.82f +
                        normal.Z *
                            (4f +
                             closing * 0.35f));
                _lastBallTouchS =
                    _battle.TimeS;
            }
            _state.BallVelocity.Y -=
                BallGravityMps2 * dt;
            _state.BallPosition +=
                _state.BallVelocity * dt;
            _state.BallVelocity.X *=
                BallLinearDrag;
            _state.BallVelocity.Z *=
                BallLinearDrag;
            float floor =
                _battle.HeightField.HeightAt(
                    _state.BallPosition.X,
                    _state.BallPosition.Z) +
                BallRadius;
            if (_state.BallPosition.Y < floor)
            {
                _state.BallPosition.Y = floor;
                _state.BallVelocity.Y =
                    _state.BallVelocity.Y < -1f
                        ? _state
                            .BallVelocity.Y *
                            -0.58f
                        : 0f;
            }
            float margin =
                MathF.Max(
                    10f,
                    _battle.WorldHalfExtentM -
                    12f);
            BounceAtBoundary(
                ref _state.BallPosition.X,
                ref _state.BallVelocity.X,
                margin);
            BounceAtBoundary(
                ref _state.BallPosition.Z,
                ref _state.BallVelocity.Z,
                margin);
            if (HorizontalDistanceSq(
                    _state.BallPosition,
                    _alphaBase) <=
                18f * 18f)
            {
                ScoreBall(Team.Bravo);
            }
            else if (HorizontalDistanceSq(
                         _state.BallPosition,
                         _bravoBase) <=
                     18f * 18f)
            {
                ScoreBall(Team.Alpha);
            }
        }

        private void ScoreBall(Team team)
        {
            if (team == Team.Alpha)
                _state.AlphaScore++;
            else
                _state.BravoScore++;
            ResetBall();
            for (int i = 0;
                i < _battle.Tanks.Count;
                i++)
            {
                Respawn(_battle.Tanks[i]);
            }
            if (_state.AlphaScore >= 5f)
                _state.Winner = Team.Alpha;
            if (_state.BravoScore >= 5f)
                _state.Winner = Team.Bravo;
        }

        private void ResetBall()
        {
            Float3 middle =
                (_alphaBase + _bravoBase) *
                0.5f;
            _state.BallPosition =
                new Float3(
                    middle.X,
                    _battle.HeightField
                        .HeightAt(
                            middle.X,
                            middle.Z) +
                    BallRadius,
                    middle.Z);
            _state.BallVelocity = Float3.Zero;
        }

        private Float3? NearestUnownedZone(
            TankState tank)
        {
            float best =
                float.MaxValue;
            Float3? result = null;
            for (int i = 0;
                i < _state.Zones.Length;
                i++)
            {
                if (_state.ZoneOwners[i] ==
                    tank.Team)
                {
                    continue;
                }
                float distance =
                    HorizontalDistanceSq(
                        tank.Position,
                        _state.Zones[i]);
                if (distance < best)
                {
                    best = distance;
                    result = _state.Zones[i];
                }
            }
            return result;
        }

        private Float3? NearestEnemyPosition(
            TankState tank)
        {
            float best =
                float.MaxValue;
            Float3? result = null;
            for (int i = 0;
                i < _battle.Tanks.Count;
                i++)
            {
                TankState target =
                    _battle.Tanks[i];
                if (target.Team ==
                        tank.Team ||
                    target.Destroyed ||
                    !target.ModeActive)
                {
                    continue;
                }
                float distance =
                    HorizontalDistanceSq(
                        tank.Position,
                        target.Position);
                if (distance < best)
                {
                    best = distance;
                    result = target.Position;
                }
            }
            return result;
        }

        private static void BounceAtBoundary(
            ref float position,
            ref float velocity,
            float margin)
        {
            if (MathF.Abs(position) <= margin)
                return;
            position =
                MathF.Sign(position) * margin;
            velocity *= -0.65f;
        }

        private static float SegmentDistanceSquared(
            Float3 point,
            Float3 start,
            Float3 end)
        {
            Float3 segment = end - start;
            float lengthSquared =
                segment.SqrMagnitude;
            float t = lengthSquared >
                0.000001f
                    ? Float3.Dot(
                        point - start,
                        segment) /
                      lengthSquared
                    : 0f;
            t = MathUtil.Clamp01(t);
            Float3 nearest =
                start + segment * t;
            return (point - nearest)
                .SqrMagnitude;
        }
    }
}
