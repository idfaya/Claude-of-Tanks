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
        public int Kills;
    }

    public sealed class NetworkShellSnapshot
    {
        public int Id;
        public string ShooterEntityId;
        public Float3 Position;
        public Float3 Velocity;
    }

    public sealed class NetworkMatchModeSnapshot
    {
        public float AlphaScore;
        public float BravoScore;
        public Float3[] Zones = new Float3[3];
        public float[] ZoneControl = new float[3];
        public Team?[] ZoneOwners = new Team?[3];
        public Float3 AlphaFlag;
        public Float3 BravoFlag;
        public string AlphaFlagCarrier;
        public string BravoFlagCarrier;
        public Float3 BallPosition;
        public Float3 BallVelocity;
        public int HordeWave = 1;
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
        public NetworkMatchModeSnapshot MatchMode =
            new NetworkMatchModeSnapshot();
        public uint StaticObstacleRevision;
        public ushort[] DestroyedStaticObstacleIndices = System.Array.Empty<ushort>();
        public NetworkEntitySnapshot[] Entities;
        public NetworkShellSnapshot[] Shells;
        public BattleEvent[] Events;
    }
}
