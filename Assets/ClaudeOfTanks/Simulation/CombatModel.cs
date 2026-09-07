using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public enum Team
    {
        Alpha,
        Bravo
    }

    public sealed class ShellSpec
    {
        public string Name = "AP";
        public string Type = "AP";
        public float CaliberMm = 120f;
        public float VelocityMps = 900f;
        public float Damage = 240f;
        public float Pen100Mm = 180f;
        public float Pen1000Mm = 145f;
        public float Pen2000Mm;
        public float ReloadS = 5.5f;
        public bool Guided;
        public float GravityScale = 1f;
        public float GuidanceTurnRateRadS = 2.4f;
    }

    public sealed class TankSpec
    {
        public string Id = "medium";
        public string DisplayName = "COT Medium";
        public float MaxHealth = 1000f;
        public float EnginePowerHp = 720f;
        public float WeightTons = 42f;
        public float TopSpeedKmh = 52f;
        public float ReverseSpeedKmh = 18f;
        public float HullTraverseDegS = 38f;
        public float TurretTraverseDegS = 32f;
        public float TerrainResistance = 1f;
        public float TrackTraction = 1f;
        public float ArmorFrontMm = 150f;
        public float ArmorSideMm = 80f;
        public float ArmorRearMm = 45f;
        public float CollisionRadiusM = 2.2f;
        public ShellSpec Shell = new ShellSpec();

        public static TankSpec Medium()
        {
            return new TankSpec();
        }

        public static TankSpec Heavy()
        {
            return new TankSpec
            {
                Id = "heavy",
                DisplayName = "COT Heavy",
                MaxHealth = 1450f,
                EnginePowerHp = 800f,
                WeightTons = 62f,
                TopSpeedKmh = 38f,
                ReverseSpeedKmh = 14f,
                HullTraverseDegS = 28f,
                TurretTraverseDegS = 24f,
                ArmorFrontMm = 220f,
                ArmorSideMm = 120f,
                ArmorRearMm = 65f,
                CollisionRadiusM = 2.5f,
                Shell = new ShellSpec
                {
                    Name = "AP Heavy",
                    VelocityMps = 820f,
                    Damage = 390f,
                    Pen100Mm = 225f,
                    Pen1000Mm = 185f,
                    ReloadS = 7.8f
                }
            };
        }
    }

    public struct TankInput
    {
        public float Throttle;
        public float Steer;
        public bool Brake;
        public bool Fire;
        public bool UseRepairKit;
        public bool UseFirstAidKit;
        public bool UseFireExtinguisher;
        public Float3 AimPoint;
    }

    public sealed class TankState
    {
        public readonly string Id;
        public readonly Team Team;
        public readonly TankSpec Spec;
        public Float3 Position;
        public float Yaw;
        public float SpeedMps;
        public float TurretYaw;
        public float Health;
        public float ReloadRemainingS;
        public bool Destroyed;
        public int Kills;
        public float TraverseMultiplier = 1f;
        public float TurretMultiplier = 1f;
        public readonly float[] ConsumableReadyAt = new float[3];
        public readonly DamageCombatState Combat;
        public readonly DamageTankSpec DamageSpec;

        public TankState(string id, Team team, TankSpec spec, Float3 position, float yaw)
        {
            Id = id;
            Team = team;
            Spec = spec;
            Position = position;
            Yaw = yaw;
            Health = spec.MaxHealth;
            DamageSpec = new DamageTankSpec { MaxHealth = spec.MaxHealth };
            DamageSpec.Gun.ReloadS = spec.Shell.ReloadS;
            DamageSpec.Gun.Shells.Add(new DamageShellSpec { Type = spec.Shell.Type });
            Combat = DamageSimulation.CreateCombatState(DamageSpec);
        }
    }

    public sealed class ShellState
    {
        public int Id;
        public string ShooterId;
        public Team ShooterTeam;
        public ShellSpec Spec;
        public Float3 Position;
        public Float3 PreviousPosition;
        public Float3 Velocity;
        public float AgeS;
        public float DistanceM;
        public bool Dead;
    }

    public enum BattleEventType
    {
        ShellFired,
        ShellHit,
        TankDestroyed,
        ConsumableUsed,
        StructureHit,
        StructureDestroyed
    }

    public struct BattleEvent
    {
        public BattleEventType Type;
        public string SourceId;
        public string TargetId;
        public Float3 Position;
        public Float3 Direction;
        public Float3 Normal;
        public string ShellType;
        public float CaliberMm;
        public float Value;
        public bool Penetrated;
    }

    public interface IHeightField
    {
        float HeightAt(float x, float z);
    }

    public interface ITerrainSurface : IHeightField
    {
        Float3 NormalAt(float x, float z);
        float ResistanceAt(float x, float z);
    }

    public sealed class FlatHeightField : ITerrainSurface
    {
        public float HeightAt(float x, float z)
        {
            return 0f;
        }

        public Float3 NormalAt(float x, float z)
        {
            return new Float3(0f, 1f, 0f);
        }

        public float ResistanceAt(float x, float z)
        {
            return 1f;
        }
    }

    public sealed class BattleState
    {
        public const float FixedDeltaTime = 1f / 60f;
        public const int MaximumStaticObstacles = 4096;

        public readonly List<TankState> Tanks = new List<TankState>();
        public readonly List<ShellState> Shells = new List<ShellState>();
        public readonly List<BattleEvent> Events = new List<BattleEvent>();
        public readonly StaticObstacle[] StaticObstacles;
        private readonly float[] _staticObstacleHealth;
        private readonly bool[] _staticObstacleDestroyed;
        public readonly IHeightField HeightField;
        public readonly DeterministicRandom Random;
        public readonly uint InitialSeed;
        public readonly float WorldHalfExtentM;
        public float TimeS;
        public int NextShellId = 1;
        public uint StaticObstacleRevision { get; private set; }

        public BattleState(
            IHeightField heightField,
            uint seed,
            float worldHalfExtentM = 500f,
            IReadOnlyList<StaticObstacle> staticObstacles = null)
        {
            HeightField = heightField ?? throw new ArgumentNullException(nameof(heightField));
            InitialSeed = seed;
            Random = new DeterministicRandom(seed);
            WorldHalfExtentM = MathF.Max(50f, worldHalfExtentM);
            int obstacleCount = staticObstacles == null ? 0 : staticObstacles.Count;
            if (obstacleCount > MaximumStaticObstacles)
                throw new ArgumentOutOfRangeException(nameof(staticObstacles));
            StaticObstacles = new StaticObstacle[obstacleCount];
            _staticObstacleHealth = new float[obstacleCount];
            _staticObstacleDestroyed = new bool[obstacleCount];
            HashSet<string> obstacleIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < obstacleCount; i++)
            {
                if (!staticObstacles[i].IsValid ||
                    !obstacleIds.Add(staticObstacles[i].Id))
                {
                    throw new ArgumentException(
                        "Static obstacles must be valid and have unique ids.",
                        nameof(staticObstacles));
                }
                StaticObstacles[i] = staticObstacles[i];
                _staticObstacleHealth[i] = StaticObstacleDurability(staticObstacles[i]);
            }
        }

        public bool IsStaticObstacleDestroyed(int index)
        {
            if (index < 0 || index >= StaticObstacles.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _staticObstacleDestroyed[index];
        }

        public float StaticObstacleHealth(int index)
        {
            if (index < 0 || index >= StaticObstacles.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _staticObstacleHealth[index];
        }

        public bool DamageStaticObstacle(int index, float damage)
        {
            if (index < 0 || index >= StaticObstacles.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (float.IsNaN(damage) || float.IsInfinity(damage) || damage < 0f)
                throw new ArgumentOutOfRangeException(nameof(damage));
            if (!StaticObstacles[index].Destructible ||
                _staticObstacleDestroyed[index] ||
                damage <= 0f)
            {
                return false;
            }

            _staticObstacleHealth[index] = MathF.Max(
                0f,
                _staticObstacleHealth[index] - damage);
            if (_staticObstacleHealth[index] > 0f) return false;
            _staticObstacleDestroyed[index] = true;
            StaticObstacleRevision++;
            return true;
        }

        public bool TryFindFirstStaticObstacleHit(
            StaticObstacleFlags requiredFlag,
            Float3 start,
            Float3 end,
            out int obstacleIndex,
            out StaticObstacle obstacle,
            out float fraction,
            out Float3 normal)
        {
            obstacleIndex = -1;
            obstacle = default;
            fraction = float.MaxValue;
            normal = Float3.Zero;
            for (int i = 0; i < StaticObstacles.Length; i++)
            {
                if (_staticObstacleDestroyed[i]) continue;
                StaticObstacle candidate = StaticObstacles[i];
                if (!candidate.HasFlag(requiredFlag)) continue;
                float candidateFraction;
                Float3 candidateNormal;
                if (CollisionSimulation.SegmentIntersectsObstacle(
                        start,
                        end,
                        candidate,
                        out candidateFraction,
                        out candidateNormal) &&
                    candidateFraction < fraction)
                {
                    obstacleIndex = i;
                    obstacle = candidate;
                    fraction = candidateFraction;
                    normal = candidateNormal;
                }
            }
            return obstacleIndex >= 0;
        }

        public bool IsVisionOccluded(Float3 start, Float3 end)
        {
            int obstacleIndex;
            StaticObstacle obstacle;
            float fraction;
            Float3 normal;
            return TryFindFirstStaticObstacleHit(
                StaticObstacleFlags.Vision,
                start,
                end,
                out obstacleIndex,
                out obstacle,
                out fraction,
                out normal);
        }

        private static float StaticObstacleDurability(StaticObstacle obstacle)
        {
            if (!obstacle.Destructible) return float.PositiveInfinity;
            float volume = obstacle.HalfWidthM * 2f *
                obstacle.HalfLengthM * 2f *
                obstacle.HeightM;
            return MathUtil.Clamp(volume * 0.8f, 80f, 900f);
        }
    }
}
