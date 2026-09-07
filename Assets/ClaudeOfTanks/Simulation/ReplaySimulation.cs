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
            if (recording == null) throw new ArgumentNullException(nameof(recording));
            BattleState state = new BattleState(
                recording.HeightField, recording.Seed, recording.WorldHalfExtentM);
            for (int i = 0; i < recording.Tanks.Count; i++)
            {
                ReplayTankSeed seed = recording.Tanks[i];
                state.Tanks.Add(new TankState(
                    seed.Id, seed.Team, seed.Spec, seed.Position, seed.Yaw));
            }
            BattleSimulation simulation = new BattleSimulation(state, recording.GameMode);
            for (int i = 0; i < recording.Frames.Count; i++)
            {
                ReplayFrame frame = recording.Frames[i];
                simulation.Step(frame.Inputs, frame.DeltaTime);
            }
            return simulation;
        }
    }
}
