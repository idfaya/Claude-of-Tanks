using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class NetworkBattleController
    {
        private NetworkInputCommand ReadPausedInput()
        {
            return NetworkBattlePauseInput.Create(
                _predictor.State,
                ++_sequence,
                _actionSequence,
                _commandTick);
        }
    }

    public static class NetworkBattlePauseInput
    {
        public static NetworkInputCommand Create(
            TankState state,
            uint sequence,
            uint actionSequence,
            long clientTick)
        {
            if (state == null)
                throw new System.ArgumentNullException(nameof(state));
            return new NetworkInputCommand
            {
                Sequence = sequence,
                ActionSequence = actionSequence,
                ClientTick = clientTick,
                SnapshotAckTick = -1,
                Throttle = 0f,
                Steer = 0f,
                Brake = true,
                AimYawRad = state.Yaw + state.TurretYaw,
                AimPitchRad = 0f,
                AimDistanceM = 100f,
                Actions = NetworkActionBits.None
            };
        }
    }
}
