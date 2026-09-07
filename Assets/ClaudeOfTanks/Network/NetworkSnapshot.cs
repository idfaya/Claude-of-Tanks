using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public sealed class NetworkEntitySnapshot
    {
        public string EntityId;
        public string VehicleSpecId;
        public Team Team;
        public Float3 Position;
        public float Yaw;
        public float TurretYaw;
        public float SpeedMps;
        public float Health;
        public float MaxHealth;
        public float ReloadRemainingS;
        public bool Destroyed;
        public bool Burning;
        public int ShellSlot;
    }

    public sealed class NetworkShellSnapshot
    {
        public int Id;
        public string ShooterEntityId;
        public Float3 Position;
        public Float3 Velocity;
    }

    public sealed class NetworkWorldSnapshot
    {
        public long Tick;
        public double ServerTimeMs;
        public string ViewerEntityId;
        public uint? AcknowledgedInputSequence;
        public GameModeId GameMode;
        public Team? Winner;
        public bool Draw;
        public NetworkEntitySnapshot[] Entities;
        public NetworkShellSnapshot[] Shells;
        public BattleEvent[] Events;
    }
}
