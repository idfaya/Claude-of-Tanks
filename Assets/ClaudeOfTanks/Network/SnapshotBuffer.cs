using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public struct SampledEntityState
    {
        public string EntityId;
        public string VehicleSpecId;
        public Team Team;
        public Float3 Position;
        public float Yaw;
        public float TurretYaw;
        public bool HydropneumaticAimActive;
        public float HullPitchRad;
        public float SpeedMps;
        public float Health;
        public float MaxHealth;
        public float ReloadRemainingS;
        public bool Destroyed;
        public bool Burning;
        public uint ModuleYellowMask;
        public uint ModuleRedMask;
        public byte CrewAliveMask;
        public int ShellSlot;
        public int Kills;
        public bool Extrapolated;
    }

    public sealed class SnapshotBuffer
    {
        public const int DefaultCapacity = 32;
        public const double MaximumExtrapolationMs = 250.0;
        private readonly List<NetworkWorldSnapshot> _snapshots;
        private readonly int _capacity;

        public SnapshotBuffer(int capacity = DefaultCapacity)
        {
            if (capacity < 2) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _snapshots = new List<NetworkWorldSnapshot>(capacity);
        }

        public int Count => _snapshots.Count;
        public long LatestTick => _snapshots.Count == 0 ? -1 : _snapshots[_snapshots.Count - 1].Tick;

        public bool Push(NetworkWorldSnapshot snapshot)
        {
            if (snapshot == null ||
                snapshot.Entities == null ||
                snapshot.Shells == null ||
                snapshot.Events == null ||
                snapshot.DestroyedStaticObstacleIndices == null ||
                snapshot.Tick <= LatestTick)
            {
                return false;
            }

            _snapshots.Add(snapshot);
            if (_snapshots.Count > _capacity) _snapshots.RemoveAt(0);
            return true;
        }

        public bool TrySampleEntity(
            string entityId,
            double targetServerTimeMs,
            out SampledEntityState sample)
        {
            sample = default;
            if (_snapshots.Count == 0 || string.IsNullOrEmpty(entityId))
            {
                return false;
            }

            NetworkWorldSnapshot latest = _snapshots[_snapshots.Count - 1];
            NetworkEntitySnapshot latestEntity = Find(latest, entityId);
            if (latestEntity == null)
            {
                return false;
            }

            if (targetServerTimeMs >= latest.ServerTimeMs)
            {
                sample = Copy(latestEntity);
                double elapsedMs = Math.Min(
                    targetServerTimeMs - latest.ServerTimeMs,
                    MaximumExtrapolationMs);
                if (elapsedMs > 0.0 && !latestEntity.Destroyed)
                {
                    float dt = (float)(elapsedMs / 1000.0);
                    sample.Position += Float3.Forward(sample.Yaw) * sample.SpeedMps * dt;
                    sample.Extrapolated = true;
                }
                return true;
            }

            NetworkWorldSnapshot newer = null;
            NetworkWorldSnapshot older = null;
            for (int i = _snapshots.Count - 1; i >= 0; i--)
            {
                NetworkWorldSnapshot candidate = _snapshots[i];
                if (candidate.ServerTimeMs >= targetServerTimeMs)
                {
                    newer = candidate;
                    continue;
                }
                older = candidate;
                break;
            }

            if (newer == null) newer = latest;
            NetworkEntitySnapshot newerEntity = Find(newer, entityId);
            if (newerEntity == null)
            {
                return false;
            }
            if (older == null)
            {
                sample = Copy(newerEntity);
                return true;
            }

            NetworkEntitySnapshot olderEntity = Find(older, entityId);
            if (olderEntity == null)
            {
                sample = Copy(newerEntity);
                return true;
            }

            double span = newer.ServerTimeMs - older.ServerTimeMs;
            float t = span <= 0.0
                ? 1f
                : MathUtil.Clamp01((float)((targetServerTimeMs - older.ServerTimeMs) / span));
            sample = Interpolate(olderEntity, newerEntity, t);
            return true;
        }

        public void Clear()
        {
            _snapshots.Clear();
        }

        private static NetworkEntitySnapshot Find(
            NetworkWorldSnapshot snapshot,
            string entityId)
        {
            for (int i = 0; i < snapshot.Entities.Length; i++)
                if (snapshot.Entities[i].EntityId == entityId) return snapshot.Entities[i];
            return null;
        }

        private static SampledEntityState Copy(NetworkEntitySnapshot source)
        {
            return new SampledEntityState
            {
                EntityId = source.EntityId,
                VehicleSpecId = source.VehicleSpecId,
                Team = source.Team,
                Position = source.Position,
                Yaw = source.Yaw,
                TurretYaw = source.TurretYaw,
                HydropneumaticAimActive =
                    source.HydropneumaticAimActive,
                HullPitchRad = source.HullPitchRad,
                SpeedMps = source.SpeedMps,
                Health = source.Health,
                MaxHealth = source.MaxHealth,
                ReloadRemainingS = source.ReloadRemainingS,
                Destroyed = source.Destroyed,
                Burning = source.Burning,
                ModuleYellowMask = source.ModuleYellowMask,
                ModuleRedMask = source.ModuleRedMask,
                CrewAliveMask = source.CrewAliveMask,
                ShellSlot = source.ShellSlot,
                Kills = source.Kills
            };
        }

        private static SampledEntityState Interpolate(
            NetworkEntitySnapshot older,
            NetworkEntitySnapshot newer,
            float t)
        {
            SampledEntityState result = Copy(newer);
            result.Position = Float3.Lerp(older.Position, newer.Position, t);
            result.Yaw = older.Yaw + MathUtil.DeltaAngle(older.Yaw, newer.Yaw) * t;
            result.TurretYaw = older.TurretYaw +
                MathUtil.DeltaAngle(older.TurretYaw, newer.TurretYaw) * t;
            result.HullPitchRad = Lerp(
                older.HullPitchRad,
                newer.HullPitchRad,
                t);
            result.SpeedMps = Lerp(older.SpeedMps, newer.SpeedMps, t);
            result.Health = Lerp(older.Health, newer.Health, t);
            result.ReloadRemainingS = Lerp(
                older.ReloadRemainingS,
                newer.ReloadRemainingS,
                t);
            return result;
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
