using System;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    [Flags]
    public enum NetworkActionBits : byte
    {
        None = 0,
        Fire = 1,
        RepairKit = 2,
        FirstAidKit = 4,
        FireExtinguisher = 8
    }

    public struct NetworkInputCommand
    {
        public string PlayerId;
        public uint Sequence;
        public uint ActionSequence;
        public long ClientTick;
        public long SnapshotAckTick;
        public float Throttle;
        public float Steer;
        public bool Brake;
        public float AimYawRad;
        public float AimPitchRad;
        public float AimDistanceM;
        public NetworkActionBits Actions;
    }

    public enum InputAdmission
    {
        Accepted,
        UnknownPlayer,
        Invalid,
        Stale,
        TooFarAhead,
        TooOld
    }

    public static class NetworkProtocol
    {
        public const int Version = 1;
        public const int TickRate = 60;
        public const int SnapshotRate = 20;
        public const int MaximumFutureTicks = 120;
        public const int MaximumPastTicks = 600;
        public const float MinimumAimDistanceM = 1f;
        public const float MaximumAimDistanceM = 2000f;
        public const float MaximumAimPitchRad = 1.45f;

        public static bool IsValid(NetworkInputCommand command)
        {
            return !string.IsNullOrEmpty(command.PlayerId) &&
                command.PlayerId.Length <= 64 &&
                command.ClientTick >= 0 &&
                IsFinite(command.Throttle) &&
                IsFinite(command.Steer) &&
                IsFinite(command.AimYawRad) &&
                IsFinite(command.AimPitchRad) &&
                IsFinite(command.AimDistanceM) &&
                command.Throttle >= -1f &&
                command.Throttle <= 1f &&
                command.Steer >= -1f &&
                command.Steer <= 1f &&
                command.AimPitchRad >= -MaximumAimPitchRad &&
                command.AimPitchRad <= MaximumAimPitchRad &&
                command.SnapshotAckTick >= -1 &&
                (command.Actions & ~(NetworkActionBits.Fire |
                    NetworkActionBits.RepairKit |
                    NetworkActionBits.FirstAidKit |
                    NetworkActionBits.FireExtinguisher)) == 0 &&
                command.AimDistanceM >= MinimumAimDistanceM &&
                command.AimDistanceM <= MaximumAimDistanceM;
        }

        public static bool IsSequenceNewer(uint candidate, uint baseline)
        {
            return candidate != baseline && unchecked((int)(candidate - baseline)) > 0;
        }

        public static TankInput ToTankInput(NetworkInputCommand command, TankState tank)
        {
            float horizontal = MathF.Cos(command.AimPitchRad);
            Float3 direction = new Float3(
                MathF.Sin(command.AimYawRad) * horizontal,
                MathF.Sin(command.AimPitchRad),
                MathF.Cos(command.AimYawRad) * horizontal);
            return new TankInput
            {
                Throttle = command.Throttle,
                Steer = command.Steer,
                Brake = command.Brake,
                Fire = (command.Actions & NetworkActionBits.Fire) != 0,
                UseRepairKit = (command.Actions & NetworkActionBits.RepairKit) != 0,
                UseFirstAidKit = (command.Actions & NetworkActionBits.FirstAidKit) != 0,
                UseFireExtinguisher =
                    (command.Actions & NetworkActionBits.FireExtinguisher) != 0,
                AimPoint = tank.Position + new Float3(0f, 1.65f, 0f) +
                    direction * command.AimDistanceM
            };
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
