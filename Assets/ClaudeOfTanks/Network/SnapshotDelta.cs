using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public sealed class NetworkSnapshotFrame
    {
        public long BaseTick;
        public NetworkWorldSnapshot Payload;
        public string[] RemovedEntityIds;

        public bool IsKeyframe => BaseTick < 0;
    }

    public enum SnapshotFrameAdmission
    {
        Accepted,
        Invalid,
        Stale,
        MissingBase
    }

    public static class SnapshotDelta
    {
        public static NetworkSnapshotFrame Create(
            NetworkWorldSnapshot current,
            NetworkWorldSnapshot baseline = null)
        {
            RequireSnapshot(current, nameof(current));
            RequireCompleteStructureState(current, nameof(current));
            if (baseline == null)
            {
                return new NetworkSnapshotFrame
                {
                    BaseTick = -1,
                    Payload = CopyWithState(
                        current,
                        current.Entities,
                        current.DestroyedStaticObstacleIndices),
                    RemovedEntityIds = Array.Empty<string>()
                };
            }

            RequireSnapshot(baseline, nameof(baseline));
            RequireCompleteStructureState(baseline, nameof(baseline));
            if (baseline.Tick >= current.Tick ||
                baseline.ViewerEntityId != current.ViewerEntityId)
            {
                throw new ArgumentException("Delta baseline does not match the current stream.");
            }

            List<NetworkEntitySnapshot> changed = new List<NetworkEntitySnapshot>();
            for (int i = 0; i < current.Entities.Length; i++)
            {
                NetworkEntitySnapshot entity = current.Entities[i];
                NetworkEntitySnapshot previous = Find(baseline.Entities, entity.EntityId);
                if (previous == null || !Equivalent(previous, entity)) changed.Add(entity);
            }

            List<string> removed = new List<string>();
            for (int i = 0; i < baseline.Entities.Length; i++)
            {
                string entityId = baseline.Entities[i].EntityId;
                if (Find(current.Entities, entityId) == null) removed.Add(entityId);
            }
            ushort[] destroyedStaticObstacles = AddedDestroyedStaticObstacles(
                current,
                baseline);

            return new NetworkSnapshotFrame
            {
                BaseTick = baseline.Tick,
                Payload = CopyWithState(
                    current,
                    changed.ToArray(),
                    destroyedStaticObstacles),
                RemovedEntityIds = removed.ToArray()
            };
        }

        public static NetworkWorldSnapshot Apply(
            NetworkSnapshotFrame frame,
            NetworkWorldSnapshot baseline = null)
        {
            if (frame == null || frame.Payload == null || frame.RemovedEntityIds == null)
                throw new ArgumentException("Snapshot frame is incomplete.", nameof(frame));
            RequireSnapshot(frame.Payload, nameof(frame));
            if (frame.IsKeyframe)
            {
                if (frame.RemovedEntityIds.Length != 0 ||
                    frame.Payload.StaticObstacleRevision !=
                        frame.Payload.DestroyedStaticObstacleIndices.Length)
                {
                    throw new ArgumentException(
                        "Keyframe state is inconsistent.",
                        nameof(frame));
                }
                return CopyWithState(
                    frame.Payload,
                    frame.Payload.Entities,
                    frame.Payload.DestroyedStaticObstacleIndices);
            }
            RequireSnapshot(baseline, nameof(baseline));
            RequireCompleteStructureState(baseline, nameof(baseline));
            if (baseline.Tick != frame.BaseTick ||
                baseline.Tick >= frame.Payload.Tick ||
                baseline.ViewerEntityId != frame.Payload.ViewerEntityId)
            {
                throw new ArgumentException("Snapshot delta baseline is unavailable.", nameof(baseline));
            }

            HashSet<string> removals =
                new HashSet<string>(frame.RemovedEntityIds, StringComparer.Ordinal);
            if (removals.Count != frame.RemovedEntityIds.Length)
                throw new ArgumentException("Snapshot removals contain duplicates.", nameof(frame));
            foreach (string entityId in removals)
            {
                if (Find(baseline.Entities, entityId) == null)
                    throw new ArgumentException("Snapshot removes an unknown entity.", nameof(frame));
            }
            Dictionary<string, NetworkEntitySnapshot> upserts =
                new Dictionary<string, NetworkEntitySnapshot>(StringComparer.Ordinal);
            for (int i = 0; i < frame.Payload.Entities.Length; i++)
            {
                NetworkEntitySnapshot entity = frame.Payload.Entities[i];
                if (removals.Contains(entity.EntityId) || upserts.ContainsKey(entity.EntityId))
                    throw new ArgumentException("Snapshot delta entity operations conflict.", nameof(frame));
                upserts.Add(entity.EntityId, entity);
            }

            List<NetworkEntitySnapshot> entities = new List<NetworkEntitySnapshot>();
            for (int i = 0; i < baseline.Entities.Length; i++)
            {
                NetworkEntitySnapshot previous = baseline.Entities[i];
                if (removals.Contains(previous.EntityId)) continue;
                NetworkEntitySnapshot replacement;
                if (upserts.TryGetValue(previous.EntityId, out replacement))
                {
                    entities.Add(replacement);
                    upserts.Remove(previous.EntityId);
                }
                else
                {
                    entities.Add(previous);
                }
            }
            for (int i = 0; i < frame.Payload.Entities.Length; i++)
            {
                NetworkEntitySnapshot entity = frame.Payload.Entities[i];
                if (upserts.ContainsKey(entity.EntityId)) entities.Add(entity);
            }
            ushort[] destroyedStaticObstacles = MergeDestroyedStaticObstacles(
                baseline,
                frame.Payload);
            return CopyWithState(
                frame.Payload,
                entities.ToArray(),
                destroyedStaticObstacles);
        }

        private static NetworkWorldSnapshot CopyWithState(
            NetworkWorldSnapshot source,
            NetworkEntitySnapshot[] entities,
            ushort[] destroyedStaticObstacleIndices)
        {
            return new NetworkWorldSnapshot
            {
                Tick = source.Tick,
                ServerTimeMs = source.ServerTimeMs,
                ViewerEntityId = source.ViewerEntityId,
                AcknowledgedInputSequence = source.AcknowledgedInputSequence,
                GameMode = source.GameMode,
                Winner = source.Winner,
                Draw = source.Draw,
                ViewerSpotted = source.ViewerSpotted,
                MatchMode = Copy(source.MatchMode),
                StaticObstacleRevision = source.StaticObstacleRevision,
                DestroyedStaticObstacleIndices =
                    (ushort[])destroyedStaticObstacleIndices.Clone(),
                Entities = (NetworkEntitySnapshot[])entities.Clone(),
                Shells = (NetworkShellSnapshot[])source.Shells.Clone(),
                Events = (BattleEvent[])source.Events.Clone()
            };
        }

        private static NetworkMatchModeSnapshot Copy(
            NetworkMatchModeSnapshot source)
        {
            return new NetworkMatchModeSnapshot
            {
                AlphaScore = source.AlphaScore,
                BravoScore = source.BravoScore,
                Zones = (Float3[])source.Zones.Clone(),
                ZoneControl = (float[])source.ZoneControl.Clone(),
                ZoneOwners = (Team?[])source.ZoneOwners.Clone(),
                AlphaFlag = source.AlphaFlag,
                BravoFlag = source.BravoFlag,
                AlphaFlagCarrier = source.AlphaFlagCarrier,
                BravoFlagCarrier = source.BravoFlagCarrier,
                BallPosition = source.BallPosition,
                BallVelocity = source.BallVelocity,
                HordeWave = source.HordeWave
            };
        }

        private static ushort[] AddedDestroyedStaticObstacles(
            NetworkWorldSnapshot current,
            NetworkWorldSnapshot baseline)
        {
            if (current.StaticObstacleRevision < baseline.StaticObstacleRevision)
                throw new ArgumentException("Static obstacle revision cannot move backward.");
            List<ushort> added = new List<ushort>();
            int baselineIndex = 0;
            for (int currentIndex = 0;
                currentIndex < current.DestroyedStaticObstacleIndices.Length;
                currentIndex++)
            {
                ushort value = current.DestroyedStaticObstacleIndices[currentIndex];
                while (baselineIndex < baseline.DestroyedStaticObstacleIndices.Length &&
                    baseline.DestroyedStaticObstacleIndices[baselineIndex] < value)
                {
                    throw new ArgumentException(
                        "Destroyed static obstacle state cannot be restored.");
                }
                if (baselineIndex < baseline.DestroyedStaticObstacleIndices.Length &&
                    baseline.DestroyedStaticObstacleIndices[baselineIndex] == value)
                {
                    baselineIndex++;
                }
                else
                {
                    added.Add(value);
                }
            }
            if (baselineIndex != baseline.DestroyedStaticObstacleIndices.Length ||
                current.StaticObstacleRevision - baseline.StaticObstacleRevision !=
                    (uint)added.Count)
            {
                throw new ArgumentException("Static obstacle revision is inconsistent.");
            }
            return added.ToArray();
        }

        private static ushort[] MergeDestroyedStaticObstacles(
            NetworkWorldSnapshot baseline,
            NetworkWorldSnapshot delta)
        {
            if (delta.StaticObstacleRevision < baseline.StaticObstacleRevision ||
                delta.StaticObstacleRevision - baseline.StaticObstacleRevision !=
                    (uint)delta.DestroyedStaticObstacleIndices.Length)
            {
                throw new ArgumentException("Static obstacle delta revision is invalid.");
            }
            ushort[] merged = new ushort[
                baseline.DestroyedStaticObstacleIndices.Length +
                delta.DestroyedStaticObstacleIndices.Length];
            int baselineIndex = 0;
            int deltaIndex = 0;
            int outputIndex = 0;
            while (baselineIndex < baseline.DestroyedStaticObstacleIndices.Length ||
                deltaIndex < delta.DestroyedStaticObstacleIndices.Length)
            {
                if (deltaIndex >= delta.DestroyedStaticObstacleIndices.Length ||
                    (baselineIndex < baseline.DestroyedStaticObstacleIndices.Length &&
                     baseline.DestroyedStaticObstacleIndices[baselineIndex] <
                        delta.DestroyedStaticObstacleIndices[deltaIndex]))
                {
                    merged[outputIndex++] =
                        baseline.DestroyedStaticObstacleIndices[baselineIndex++];
                    continue;
                }
                if (baselineIndex < baseline.DestroyedStaticObstacleIndices.Length &&
                    baseline.DestroyedStaticObstacleIndices[baselineIndex] ==
                        delta.DestroyedStaticObstacleIndices[deltaIndex])
                {
                    throw new ArgumentException(
                        "Static obstacle delta contains an existing index.");
                }
                merged[outputIndex++] =
                    delta.DestroyedStaticObstacleIndices[deltaIndex++];
            }
            return merged;
        }

        private static NetworkEntitySnapshot Find(
            NetworkEntitySnapshot[] entities,
            string entityId)
        {
            for (int i = 0; i < entities.Length; i++)
                if (entities[i].EntityId == entityId) return entities[i];
            return null;
        }

        private static bool Equivalent(
            NetworkEntitySnapshot a,
            NetworkEntitySnapshot b)
        {
            return a.EntityId == b.EntityId &&
                a.VehicleSpecId == b.VehicleSpecId &&
                a.Team == b.Team &&
                a.Position.Equals(b.Position) &&
                a.Yaw == b.Yaw &&
                a.TurretYaw == b.TurretYaw &&
                a.SpeedMps == b.SpeedMps &&
                a.Health == b.Health &&
                a.MaxHealth == b.MaxHealth &&
                a.ReloadRemainingS == b.ReloadRemainingS &&
                a.Destroyed == b.Destroyed &&
                a.Burning == b.Burning &&
                a.ModuleYellowMask == b.ModuleYellowMask &&
                a.ModuleRedMask == b.ModuleRedMask &&
                a.CrewAliveMask == b.CrewAliveMask &&
                a.ShellSlot == b.ShellSlot &&
                a.Kills == b.Kills;
        }

        private static void RequireSnapshot(NetworkWorldSnapshot snapshot, string argument)
        {
            if (snapshot == null ||
                snapshot.Tick < 0 ||
                snapshot.Entities == null ||
                snapshot.Shells == null ||
                snapshot.Events == null ||
                snapshot.MatchMode == null ||
                snapshot.MatchMode.Zones == null ||
                snapshot.MatchMode.Zones.Length != 3 ||
                snapshot.MatchMode.ZoneControl == null ||
                snapshot.MatchMode.ZoneControl.Length != 3 ||
                snapshot.MatchMode.ZoneOwners == null ||
                snapshot.MatchMode.ZoneOwners.Length != 3 ||
                snapshot.DestroyedStaticObstacleIndices == null)
            {
                throw new ArgumentException("Snapshot is incomplete.", argument);
            }
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            ushort previousDestroyedIndex = 0;
            for (int i = 0; i < snapshot.DestroyedStaticObstacleIndices.Length; i++)
            {
                ushort index = snapshot.DestroyedStaticObstacleIndices[i];
                if (index >= BattleState.MaximumStaticObstacles ||
                    (i > 0 && index <= previousDestroyedIndex))
                {
                    throw new ArgumentException(
                        "Snapshot static obstacle indices are invalid.",
                        argument);
                }
                previousDestroyedIndex = index;
            }
            for (int i = 0; i < snapshot.Entities.Length; i++)
            {
                NetworkEntitySnapshot entity = snapshot.Entities[i];
                if (entity == null ||
                    string.IsNullOrEmpty(entity.EntityId) ||
                    !ids.Add(entity.EntityId) ||
                    (entity.ModuleYellowMask &
                     entity.ModuleRedMask) != 0u ||
                    ((entity.ModuleYellowMask |
                      entity.ModuleRedMask) &
                     ~NetworkDamageState.AllModuleMask) != 0u ||
                    (entity.CrewAliveMask &
                     ~NetworkDamageState.AllCrewAliveMask) != 0)
                {
                    throw new ArgumentException("Snapshot entity identities are invalid.", argument);
                }
            }
        }

        private static void RequireCompleteStructureState(
            NetworkWorldSnapshot snapshot,
            string argument)
        {
            if (snapshot.StaticObstacleRevision !=
                snapshot.DestroyedStaticObstacleIndices.Length)
            {
                throw new ArgumentException(
                    "Snapshot static obstacle revision is incomplete.",
                    argument);
            }
        }
    }

    public sealed class SnapshotFrameReceiver
    {
        public const int DefaultHistoryCapacity = 32;
        private readonly Dictionary<long, NetworkWorldSnapshot> _history =
            new Dictionary<long, NetworkWorldSnapshot>();
        private readonly Queue<long> _order = new Queue<long>();
        private readonly int _capacity;

        public SnapshotFrameReceiver(int capacity = DefaultHistoryCapacity)
        {
            if (capacity < 2) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
        }

        public NetworkWorldSnapshot Latest { get; private set; }
        public bool NeedsKeyframe { get; private set; } = true;

        public SnapshotFrameAdmission Receive(
            NetworkSnapshotFrame frame,
            out NetworkWorldSnapshot snapshot)
        {
            snapshot = null;
            if (frame == null || frame.Payload == null ||
                frame.Payload.Tick <= (Latest != null ? Latest.Tick : -1))
            {
                return frame != null && frame.Payload != null
                    ? SnapshotFrameAdmission.Stale
                    : SnapshotFrameAdmission.Invalid;
            }

            NetworkWorldSnapshot baseline = null;
            if (!frame.IsKeyframe && !_history.TryGetValue(frame.BaseTick, out baseline))
            {
                NeedsKeyframe = true;
                return SnapshotFrameAdmission.MissingBase;
            }
            try
            {
                snapshot = SnapshotDelta.Apply(frame, baseline);
            }
            catch (ArgumentException)
            {
                NeedsKeyframe = true;
                return SnapshotFrameAdmission.Invalid;
            }

            Latest = snapshot;
            NeedsKeyframe = false;
            _history[snapshot.Tick] = snapshot;
            _order.Enqueue(snapshot.Tick);
            while (_order.Count > _capacity)
            {
                long tick = _order.Dequeue();
                _history.Remove(tick);
            }
            return SnapshotFrameAdmission.Accepted;
        }

        public void Clear()
        {
            _history.Clear();
            _order.Clear();
            Latest = null;
            NeedsKeyframe = true;
        }
    }
}
