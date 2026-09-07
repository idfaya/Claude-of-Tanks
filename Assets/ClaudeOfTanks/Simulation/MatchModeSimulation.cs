using System;

namespace ClaudeOfTanks.Simulation
{
    public enum GameModeId
    {
        Standard,
        CaptureTheFlag,
        ZoneControl,
        TurboBall,
        EndlessHorde
    }

    public sealed class MatchModeState
    {
        public readonly GameModeId Id;
        public float AlphaScore;
        public float BravoScore;
        public Team? Winner;
        public bool Draw;
        public readonly Float3[] Zones = new Float3[3];
        public readonly float[] ZoneControl = new float[3];
        public readonly Team?[] ZoneOwners = new Team?[3];
        public Float3 AlphaFlag;
        public Float3 BravoFlag;
        public string AlphaFlagCarrier;
        public string BravoFlagCarrier;
        public Float3 BallPosition;
        public Float3 BallVelocity;
        public int HordeWave = 1;

        public MatchModeState(GameModeId id)
        {
            Id = id;
        }
    }

    public sealed class MatchModeSimulation
    {
        public const float RespawnSeconds = 6f;
        private const float FlagRadius = 8f;
        private const float ZoneRadius = 30f;
        private const float BallRadius = 4f;
        private readonly BattleState _battle;
        private readonly MatchModeState _state;
        private Float3 _alphaBase;
        private Float3 _bravoBase;
        private bool _initialized;

        public MatchModeSimulation(BattleState battle, GameModeId mode)
        {
            _battle = battle ?? throw new ArgumentNullException(nameof(battle));
            _state = new MatchModeState(mode);
        }

        public MatchModeState State => _state;

        public void Step(float dt)
        {
            if (!_initialized) Initialize();
            if (_state.Winner.HasValue || _state.Draw || dt <= 0f) return;
            switch (_state.Id)
            {
                case GameModeId.Standard: StepStandard(); break;
                case GameModeId.CaptureTheFlag: StepFlags(); break;
                case GameModeId.ZoneControl: StepZones(dt); break;
                case GameModeId.TurboBall: StepBall(dt); break;
                case GameModeId.EndlessHorde: StepHorde(); break;
            }
        }

        private void Initialize()
        {
            _alphaBase = TeamCenter(Team.Alpha, new Float3(0f, 0f, -180f));
            _bravoBase = TeamCenter(Team.Bravo, new Float3(0f, 0f, 180f));
            _state.AlphaFlag = _alphaBase;
            _state.BravoFlag = _bravoBase;
            Float3 middle = (_alphaBase + _bravoBase) * 0.5f;
            Float3 axis = (_bravoBase - _alphaBase).Normalized;
            Float3 side = new Float3(axis.Z, 0f, -axis.X);
            _state.Zones[0] = middle - side * 105f;
            _state.Zones[1] = middle;
            _state.Zones[2] = middle + side * 105f;
            _state.BallPosition = middle + new Float3(0f, 2.2f, 0f);
            _initialized = true;
        }

        private void StepStandard()
        {
            bool alpha = HasAlive(Team.Alpha);
            bool bravo = HasAlive(Team.Bravo);
            if (!alpha && !bravo) _state.Draw = true;
            else if (!alpha) _state.Winner = Team.Bravo;
            else if (!bravo) _state.Winner = Team.Alpha;
        }

        private void StepFlags()
        {
            UpdateFlag(Team.Alpha, ref _state.AlphaFlagCarrier, ref _state.AlphaFlag);
            UpdateFlag(Team.Bravo, ref _state.BravoFlagCarrier, ref _state.BravoFlag);
            if (_state.AlphaScore >= 3f) _state.Winner = Team.Alpha;
            if (_state.BravoScore >= 3f) _state.Winner = Team.Bravo;
        }

        private void UpdateFlag(Team owner, ref string carrierId, ref Float3 position)
        {
            Float3 home = owner == Team.Alpha ? _alphaBase : _bravoBase;
            if (carrierId == null)
            {
                TankState thief = ClosestEnemy(owner, position, FlagRadius);
                if (thief != null) carrierId = thief.Id;
                else position = home;
                return;
            }
            TankState carrier = FindTank(carrierId);
            if (carrier == null || carrier.Destroyed)
            {
                carrierId = null;
                return;
            }
            position = carrier.Position;
            Float3 carrierHome = carrier.Team == Team.Alpha ? _alphaBase : _bravoBase;
            if (HorizontalDistanceSq(position, carrierHome) <= 12f * 12f)
            {
                if (carrier.Team == Team.Alpha) _state.AlphaScore++;
                else _state.BravoScore++;
                carrierId = null;
                position = home;
            }
        }

        private void StepZones(float dt)
        {
            for (int zone = 0; zone < _state.Zones.Length; zone++)
            {
                int alpha = CountNear(Team.Alpha, _state.Zones[zone], ZoneRadius);
                int bravo = CountNear(Team.Bravo, _state.Zones[zone], ZoneRadius);
                if (alpha > 0 && bravo == 0) _state.ZoneControl[zone] += dt / 8f;
                else if (bravo > 0 && alpha == 0) _state.ZoneControl[zone] -= dt / 8f;
                _state.ZoneControl[zone] = MathUtil.Clamp(_state.ZoneControl[zone], -1f, 1f);
                if (_state.ZoneControl[zone] >= 1f) _state.ZoneOwners[zone] = Team.Alpha;
                else if (_state.ZoneControl[zone] <= -1f) _state.ZoneOwners[zone] = Team.Bravo;
                if (_state.ZoneOwners[zone] == Team.Alpha) _state.AlphaScore += 2f * dt;
                if (_state.ZoneOwners[zone] == Team.Bravo) _state.BravoScore += 2f * dt;
            }
            if (_state.AlphaScore >= 1000f) _state.Winner = Team.Alpha;
            if (_state.BravoScore >= 1000f) _state.Winner = Team.Bravo;
        }

        private void StepBall(float dt)
        {
            TankState touch = ClosestTank(_state.BallPosition, BallRadius);
            if (touch != null)
            {
                Float3 forward = Float3.Forward(touch.Yaw);
                _state.BallVelocity += forward * (MathF.Abs(touch.SpeedMps) + 8f);
            }
            _state.BallVelocity *= MathF.Pow(0.992f, dt * 60f);
            _state.BallPosition += _state.BallVelocity * dt;
            if (HorizontalDistanceSq(_state.BallPosition, _alphaBase) <= 18f * 18f)
                ScoreBall(Team.Bravo);
            else if (HorizontalDistanceSq(_state.BallPosition, _bravoBase) <= 18f * 18f)
                ScoreBall(Team.Alpha);
        }

        private void ScoreBall(Team team)
        {
            if (team == Team.Alpha) _state.AlphaScore++;
            else _state.BravoScore++;
            _state.BallPosition = (_alphaBase + _bravoBase) * 0.5f + new Float3(0f, 2.2f, 0f);
            _state.BallVelocity = Float3.Zero;
            if (_state.AlphaScore >= 5f) _state.Winner = Team.Alpha;
            if (_state.BravoScore >= 5f) _state.Winner = Team.Bravo;
        }

        private void StepHorde()
        {
            if (!HasAlive(Team.Alpha))
            {
                _state.Winner = Team.Bravo;
                return;
            }
            if (HasAlive(Team.Bravo)) return;
            _state.HordeWave++;
            for (int i = 0; i < _battle.Tanks.Count; i++)
            {
                TankState tank = _battle.Tanks[i];
                if (tank.Team != Team.Bravo) continue;
                tank.Combat.Destroyed = false;
                tank.Combat.Health = tank.Combat.MaxHealth;
                tank.Health = tank.Combat.Health;
                tank.Destroyed = false;
            }
        }

        private Float3 TeamCenter(Team team, Float3 fallback)
        {
            Float3 sum = Float3.Zero;
            int count = 0;
            for (int i = 0; i < _battle.Tanks.Count; i++)
                if (_battle.Tanks[i].Team == team) { sum += _battle.Tanks[i].Position; count++; }
            return count > 0 ? sum / count : fallback;
        }

        private bool HasAlive(Team team)
        {
            for (int i = 0; i < _battle.Tanks.Count; i++)
                if (_battle.Tanks[i].Team == team && !_battle.Tanks[i].Destroyed) return true;
            return false;
        }

        private int CountNear(Team team, Float3 point, float radius)
        {
            int count = 0;
            for (int i = 0; i < _battle.Tanks.Count; i++)
                if (_battle.Tanks[i].Team == team && !_battle.Tanks[i].Destroyed &&
                    HorizontalDistanceSq(_battle.Tanks[i].Position, point) <= radius * radius) count++;
            return count;
        }

        private TankState ClosestEnemy(Team owner, Float3 point, float radius)
        {
            TankState best = null;
            float bestDistance = radius * radius;
            for (int i = 0; i < _battle.Tanks.Count; i++)
            {
                TankState tank = _battle.Tanks[i];
                float distance = HorizontalDistanceSq(tank.Position, point);
                if (!tank.Destroyed && tank.Team != owner && distance <= bestDistance)
                { best = tank; bestDistance = distance; }
            }
            return best;
        }

        private TankState ClosestTank(Float3 point, float radius)
        {
            TankState best = null;
            float bestDistance = radius * radius;
            for (int i = 0; i < _battle.Tanks.Count; i++)
            {
                TankState tank = _battle.Tanks[i];
                float distance = HorizontalDistanceSq(tank.Position, point);
                if (!tank.Destroyed && distance <= bestDistance) { best = tank; bestDistance = distance; }
            }
            return best;
        }

        private TankState FindTank(string id)
        {
            for (int i = 0; i < _battle.Tanks.Count; i++)
                if (_battle.Tanks[i].Id == id) return _battle.Tanks[i];
            return null;
        }

        private static float HorizontalDistanceSq(Float3 a, Float3 b)
        {
            float x = a.X - b.X;
            float z = a.Z - b.Z;
            return x * x + z * z;
        }
    }
}
