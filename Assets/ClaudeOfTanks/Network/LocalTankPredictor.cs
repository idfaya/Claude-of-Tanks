using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public sealed class LocalTankPredictor
    {
        public const int MaximumPendingInputs = 120;
        public const float HardSnapDistanceM = 7f;
        private const float PositionCorrectionTimeS = 0.11f;
        private const float YawCorrectionTimeS = 0.075f;
        private readonly List<NetworkInputCommand> _pending =
            new List<NetworkInputCommand>(MaximumPendingInputs);
        private readonly IHeightField _heightField;
        private readonly TankState _state;
        private readonly string _playerId;
        private Float3 _positionCorrection;
        private float _yawCorrection;
        private bool _seeded;
        private uint _latestSequence;
        private bool _hasSequence;

        public LocalTankPredictor(
            string playerId,
            string entityId,
            Team team,
            TankSpec spec,
            IHeightField heightField,
            IEnumerable<string> equipment = null)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException("Player id is required.", nameof(playerId));
            if (string.IsNullOrEmpty(entityId))
                throw new ArgumentException("Entity id is required.", nameof(entityId));
            _playerId = playerId;
            _heightField = heightField ?? throw new ArgumentNullException(nameof(heightField));
            _state = new TankState(entityId, team, spec ??
                throw new ArgumentNullException(nameof(spec)), Float3.Zero, 0f);
            LoadoutSimulation.ApplyEquipment(_state, equipment);
        }

        public TankState State => _state;
        public int PendingInputCount => _pending.Count;
        public Float3 PresentedPosition => _state.Position + _positionCorrection;
        public float PresentedYaw => _state.Yaw + _yawCorrection;

        public bool Predict(NetworkInputCommand command)
        {
            if (!NetworkProtocol.IsValid(command) ||
                command.PlayerId != _playerId ||
                (_hasSequence &&
                 !NetworkProtocol.IsSequenceNewer(command.Sequence, _latestSequence)))
            {
                return false;
            }

            _hasSequence = true;
            _latestSequence = command.Sequence;
            if (_pending.Count == MaximumPendingInputs) _pending.RemoveAt(0);
            _pending.Add(command);
            TankMovement.Step(
                _state,
                NetworkProtocol.ToTankInput(command, _state),
                _heightField,
                BattleState.FixedDeltaTime);
            return true;
        }

        public void Reconcile(
            NetworkEntitySnapshot authority,
            uint acknowledgedInputSequence)
        {
            if (authority == null || authority.EntityId != _state.Id)
                throw new ArgumentException("Authority entity does not match predictor.", nameof(authority));

            Float3 previousPresented = PresentedPosition;
            float previousPresentedYaw = PresentedYaw;
            ApplyAuthority(authority);
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                uint sequence = _pending[i].Sequence;
                if (sequence == acknowledgedInputSequence ||
                    NetworkProtocol.IsSequenceNewer(acknowledgedInputSequence, sequence))
                {
                    _pending.RemoveAt(i);
                }
            }
            for (int i = 0; i < _pending.Count; i++)
            {
                TankMovement.Step(
                    _state,
                    NetworkProtocol.ToTankInput(_pending[i], _state),
                    _heightField,
                    BattleState.FixedDeltaTime);
            }

            if (!_seeded)
            {
                _positionCorrection = Float3.Zero;
                _yawCorrection = 0f;
                _seeded = true;
                return;
            }

            Float3 error = previousPresented - _state.Position;
            if (error.Magnitude > HardSnapDistanceM)
            {
                _positionCorrection = Float3.Zero;
                _yawCorrection = 0f;
                return;
            }
            _positionCorrection = error;
            _yawCorrection = MathUtil.DeltaAngle(_state.Yaw, previousPresentedYaw);
        }

        public void AdvancePresentation(float deltaTimeS)
        {
            if (deltaTimeS <= 0f) return;
            float positionDecay = MathF.Exp(-deltaTimeS / PositionCorrectionTimeS);
            float yawDecay = MathF.Exp(-deltaTimeS / YawCorrectionTimeS);
            _positionCorrection *= positionDecay;
            _yawCorrection *= yawDecay;
        }

        public void Clear()
        {
            _pending.Clear();
            _positionCorrection = Float3.Zero;
            _yawCorrection = 0f;
            _seeded = false;
            _hasSequence = false;
        }

        private void ApplyAuthority(NetworkEntitySnapshot authority)
        {
            _state.Position = authority.Position;
            _state.Yaw = authority.Yaw;
            _state.TurretYaw = authority.TurretYaw;
            _state.GunPitchRad =
                authority.GunPitchRad;
            _state.HydropneumaticAimActive =
                authority.HydropneumaticAimActive;
            _state.HullPitchRad =
                authority.HullPitchRad;
            _state.TerrainPitchRad =
                authority.TerrainPitchRad;
            _state.HullRollRad =
                authority.HullRollRad;
            _state.VerticalSpeedMps =
                authority.VerticalSpeedMps;
            _state.Grounded = authority.Grounded;
            _state.Overturned =
                authority.Overturned;
            _state.SpeedMps = authority.SpeedMps;
            _state.Health = authority.Health;
            _state.ReloadRemainingS = authority.ReloadRemainingS;
            _state.Destroyed = authority.Destroyed;
            _state.Kills = authority.Kills;
            _state.Combat.Health = authority.Health;
            _state.Combat.Destroyed = authority.Destroyed;
            _state.Combat.Fire.Burning = authority.Burning;
            NetworkDamageState.Apply(_state.Combat, authority);
            if (authority.ShellSlot >= 0 &&
                authority.ShellSlot < _state.Combat.Ammo.Length)
            {
                _state.Combat.ShellSlot = authority.ShellSlot;
            }
        }
    }
}
