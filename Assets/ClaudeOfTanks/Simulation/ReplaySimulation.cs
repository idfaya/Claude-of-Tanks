using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public sealed class ReplayRecording
    {
        internal readonly IHeightField HeightField;
        internal readonly uint Seed;
        internal readonly float WorldHalfExtentM;
        internal readonly GameModeId GameMode;
        internal readonly List<ReplayTankSeed> Tanks = new List<ReplayTankSeed>();
        internal readonly List<ReplayFrame> Frames = new List<ReplayFrame>();

        internal ReplayRecording(BattleState state, GameModeId gameMode)
        {
            HeightField = state.HeightField;
            Seed = state.InitialSeed;
            WorldHalfExtentM = state.WorldHalfExtentM;
            GameMode = gameMode;
            for (int i = 0; i < state.Tanks.Count; i++)
            {
                TankState tank = state.Tanks[i];
                Tanks.Add(new ReplayTankSeed
                {
                    Id = tank.Id,
                    Team = tank.Team,
                    Spec = tank.Spec,
                    Position = tank.Position,
                    Yaw = tank.Yaw
                });
            }
        }

        public int FrameCount => Frames.Count;
        public int TankCount => Tanks.Count;
        public float DurationS
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < Frames.Count; i++) total += Frames[i].DeltaTime;
                return total;
            }
        }

        public string GetTankId(int index) { return Tanks[index].Id; }
        public string GetTankSpecId(int index) { return Tanks[index].Spec.Id; }
        public Team GetTankTeam(int index) { return Tanks[index].Team; }
    }

    internal sealed class ReplayTankSeed
    {
        public string Id;
        public Team Team;
        public TankSpec Spec;
        public Float3 Position;
        public float Yaw;
    }

    internal sealed class ReplayFrame
    {
        public float DeltaTime;
        public readonly Dictionary<string, TankInput> Inputs =
            new Dictionary<string, TankInput>(StringComparer.Ordinal);
    }

    public sealed class BattleReplayRecorder
    {
        public readonly ReplayRecording Recording;

        public BattleReplayRecorder(BattleState initialState, GameModeId gameMode)
        {
            Recording = new ReplayRecording(initialState, gameMode);
        }

        public void Record(IReadOnlyDictionary<string, TankInput> inputs, float dt)
        {
            ReplayFrame frame = new ReplayFrame { DeltaTime = dt };
            if (inputs != null)
            {
                foreach (KeyValuePair<string, TankInput> input in inputs)
                    frame.Inputs[input.Key] = input.Value;
            }
            Recording.Frames.Add(frame);
        }
    }

    public static class BattleReplayPlayer
    {
        public static BattleSimulation Play(ReplayRecording recording)
        {
            BattleReplaySession session = new BattleReplaySession(recording);
            session.Seek(recording.FrameCount);
            return session.Simulation;
        }
    }

    public sealed class BattleReplaySession
    {
        private readonly ReplayRecording _recording;

        public BattleReplaySession(ReplayRecording recording)
        {
            _recording = recording ?? throw new ArgumentNullException(nameof(recording));
            Reset();
        }

        public BattleSimulation Simulation { get; private set; }
        public int CurrentFrame { get; private set; }
        public int FrameCount => _recording.FrameCount;
        public bool Complete => CurrentFrame >= FrameCount;
        public float DurationS => _recording.DurationS;
        public float CurrentTimeS => Simulation.State.TimeS;

        public bool Step()
        {
            if (Complete) return false;
            ReplayFrame frame = _recording.Frames[CurrentFrame++];
            Simulation.Step(frame.Inputs, frame.DeltaTime);
            return true;
        }

        public void Seek(int frame)
        {
            if (frame < 0 || frame > FrameCount)
                throw new ArgumentOutOfRangeException(nameof(frame));
            if (frame < CurrentFrame) Reset();
            while (CurrentFrame < frame) Step();
        }

        public void SeekTime(float timeS)
        {
            if (float.IsNaN(timeS) || float.IsInfinity(timeS))
                throw new ArgumentOutOfRangeException(nameof(timeS));
            float target = Math.Max(0f, Math.Min(timeS, DurationS));
            int frame = 0;
            float elapsed = 0f;
            while (frame < _recording.Frames.Count &&
                elapsed + _recording.Frames[frame].DeltaTime <= target)
            {
                elapsed += _recording.Frames[frame].DeltaTime;
                frame++;
            }
            Seek(frame);
        }

        public void Reset()
        {
            BattleState state = new BattleState(
                _recording.HeightField,
                _recording.Seed,
                _recording.WorldHalfExtentM);
            for (int i = 0; i < _recording.Tanks.Count; i++)
            {
                ReplayTankSeed seed = _recording.Tanks[i];
                state.Tanks.Add(new TankState(
                    seed.Id, seed.Team, seed.Spec, seed.Position, seed.Yaw));
            }
            Simulation = new BattleSimulation(state, _recording.GameMode);
            CurrentFrame = 0;
        }
    }
}
