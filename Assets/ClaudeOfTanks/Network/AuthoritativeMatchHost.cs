using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public sealed class AuthoritativeMatchHost
    {
        public const int MaximumCatchUpTicks = 8;
        private readonly BattleSimulation _simulation;
        private readonly SpottingSimulation _spotting;
        private readonly Func<Float3, Float3, bool> _isOccluded;
        private readonly Dictionary<string, PeerState> _peers =
            new Dictionary<string, PeerState>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _ownersByEntity =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, TankInput> _inputs =
            new Dictionary<string, TankInput>(StringComparer.Ordinal);
        private readonly BotController _bots;

        public AuthoritativeMatchHost(
            BattleSimulation simulation,
            SpottingSimulation spotting = null)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _spotting = spotting ?? new SpottingSimulation();
            _isOccluded = _simulation.State.IsVisionOccluded;
            _bots = new BotController(_spotting, _isOccluded);
        }

        public long Tick { get; private set; }
        public bool ShouldPublishSnapshot =>
            Tick > 0 && Tick % (NetworkProtocol.TickRate / NetworkProtocol.SnapshotRate) == 0;

        public void RegisterPlayer(string playerId, string entityId)
        {
            if (string.IsNullOrEmpty(playerId) || playerId.Length > 64)
                throw new ArgumentException("Player id is invalid.", nameof(playerId));
            if (string.IsNullOrEmpty(entityId) || FindTank(entityId) == null)
                throw new ArgumentException("Entity id is invalid.", nameof(entityId));
            if (_peers.ContainsKey(playerId))
                throw new InvalidOperationException("Player is already registered.");
            if (_ownersByEntity.ContainsKey(entityId))
                throw new InvalidOperationException("Entity already has a player owner.");

            _peers.Add(playerId, new PeerState { EntityId = entityId });
            _ownersByEntity.Add(entityId, playerId);
        }

        public void RegisterSpectator(string playerId)
        {
            if (string.IsNullOrEmpty(playerId) || playerId.Length > 64)
                throw new ArgumentException("Player id is invalid.", nameof(playerId));
            if (_peers.ContainsKey(playerId))
                throw new InvalidOperationException("Player is already registered.");
            _peers.Add(playerId, new PeerState());
        }

        public InputAdmission SubmitInput(NetworkInputCommand command)
        {
            if (!NetworkProtocol.IsValid(command)) return InputAdmission.Invalid;
            PeerState peer;
            if (!_peers.TryGetValue(command.PlayerId, out peer) ||
                string.IsNullOrEmpty(peer.EntityId))
            {
                return InputAdmission.UnknownPlayer;
            }
            if (command.ClientTick > Tick + NetworkProtocol.MaximumFutureTicks)
                return InputAdmission.TooFarAhead;
            if (command.ClientTick < Tick - NetworkProtocol.MaximumPastTicks)
                return InputAdmission.TooOld;
            if (command.SnapshotAckTick > Tick)
                return InputAdmission.TooFarAhead;
            if (command.SnapshotAckTick > peer.SnapshotAckTick)
                peer.SnapshotAckTick = command.SnapshotAckTick;
            if (peer.HasInputSequence &&
                !NetworkProtocol.IsSequenceNewer(command.Sequence, peer.InputSequence))
            {
                return InputAdmission.Stale;
            }

            peer.Command = command;
            peer.InputSequence = command.Sequence;
            peer.HasInputSequence = true;
            if (command.Actions != NetworkActionBits.None &&
                (!peer.HasActionSequence ||
                 NetworkProtocol.IsSequenceNewer(command.ActionSequence, peer.ActionSequence)))
            {
                peer.PendingActions |= command.Actions;
                peer.ActionSequence = command.ActionSequence;
                peer.HasActionSequence = true;
            }
            return InputAdmission.Accepted;
        }

        public long GetSnapshotAcknowledgement(string playerId)
        {
            PeerState peer;
            if (!_peers.TryGetValue(playerId, out peer))
                throw new ArgumentException("Unknown player.", nameof(playerId));
            return peer.SnapshotAckTick;
        }

        public int AdvanceTicks(int requestedTicks)
        {
            int count = Math.Max(0, Math.Min(requestedTicks, MaximumCatchUpTicks));
            for (int step = 0; step < count; step++)
            {
                BuildInputs();
                _simulation.Step(_inputs, BattleState.FixedDeltaTime);
                Tick++;
            }
            return count;
        }

        public NetworkWorldSnapshot CreateSnapshot(string playerId)
        {
            PeerState peer;
            if (!_peers.TryGetValue(playerId, out peer))
                throw new ArgumentException("Unknown player.", nameof(playerId));
            TankState viewer = string.IsNullOrEmpty(peer.EntityId)
                ? null
                : FindTank(peer.EntityId);
            HashSet<string> visible = BuildVisibleSet(viewer);
            List<NetworkEntitySnapshot> entities = new List<NetworkEntitySnapshot>(visible.Count);
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
            {
                TankState tank = tanks[i];
                if (!visible.Contains(tank.Id)) continue;
                entities.Add(Capture(tank));
            }

            List<NetworkShellSnapshot> shells = new List<NetworkShellSnapshot>();
            for (int i = 0; i < _simulation.State.Shells.Count; i++)
            {
                ShellState shell = _simulation.State.Shells[i];
                if (!CanSeeShell(viewer, visible, shell)) continue;
                shells.Add(new NetworkShellSnapshot
                {
                    Id = shell.Id,
                    ShooterEntityId = shell.ShooterId,
                    Position = shell.Position,
                    Velocity = shell.Velocity
                });
            }

            List<BattleEvent> events = new List<BattleEvent>();
            for (int i = 0; i < _simulation.State.Events.Count; i++)
            {
                BattleEvent battleEvent = _simulation.State.Events[i];
                if (IsEventVisible(visible, battleEvent)) events.Add(battleEvent);
            }

            List<ushort> destroyedStaticObstacles = new List<ushort>();
            for (int i = 0; i < _simulation.State.StaticObstacles.Length; i++)
            {
                if (_simulation.State.IsStaticObstacleDestroyed(i))
                    destroyedStaticObstacles.Add((ushort)i);
            }

            return new NetworkWorldSnapshot
            {
                Tick = Tick,
                ServerTimeMs = Tick * (1000.0 / NetworkProtocol.TickRate),
                ViewerEntityId = peer.EntityId,
                AcknowledgedInputSequence =
                    peer.HasInputSequence ? peer.InputSequence : (uint?)null,
                GameMode = _simulation.MatchMode.Id,
                Winner = _simulation.MatchMode.Winner,
                Draw = _simulation.MatchMode.Draw,
                StaticObstacleRevision = _simulation.State.StaticObstacleRevision,
                DestroyedStaticObstacleIndices = destroyedStaticObstacles.ToArray(),
                Entities = entities.ToArray(),
                Shells = shells.ToArray(),
                Events = events.ToArray()
            };
        }

        private void BuildInputs()
        {
            _inputs.Clear();
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
            {
                TankState tank = tanks[i];
                if (tank.Destroyed) continue;
                string playerId;
                if (_ownersByEntity.TryGetValue(tank.Id, out playerId))
                {
                    PeerState peer = _peers[playerId];
                    TankInput input = peer.HasInputSequence
                        ? NetworkProtocol.ToTankInput(peer.Command, tank)
                        : NeutralInput(tank);
                    ApplyPendingActions(ref input, peer.PendingActions);
                    peer.PendingActions = NetworkActionBits.None;
                    _inputs[tank.Id] = input;
                }
                else
                {
                    _inputs[tank.Id] = _bots.Decide(tank, tanks);
                }
            }
        }

        private HashSet<string> BuildVisibleSet(TankState viewer)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
            {
                TankState tank = tanks[i];
                if (viewer == null ||
                    tank.Id == viewer.Id ||
                    tank.Team == viewer.Team ||
                    _spotting.CanSpot(viewer, tank, _isOccluded))
                {
                    result.Add(tank.Id);
                }
            }
            return result;
        }

        private static bool CanSeeShell(
            TankState viewer,
            HashSet<string> visibleEntities,
            ShellState shell)
        {
            if (viewer == null || visibleEntities.Contains(shell.ShooterId)) return true;
            return (shell.Position - viewer.Position).SqrMagnitude <=
                SpottingSimulation.ProximitySpotRangeM *
                SpottingSimulation.ProximitySpotRangeM;
        }

        private static bool IsEventVisible(
            HashSet<string> visibleEntities,
            BattleEvent battleEvent)
        {
            return (!string.IsNullOrEmpty(battleEvent.SourceId) &&
                    visibleEntities.Contains(battleEvent.SourceId)) ||
                (!string.IsNullOrEmpty(battleEvent.TargetId) &&
                 visibleEntities.Contains(battleEvent.TargetId));
        }

        private static NetworkEntitySnapshot Capture(TankState tank)
        {
            return new NetworkEntitySnapshot
            {
                EntityId = tank.Id,
                VehicleSpecId = tank.Spec.Id,
                Team = tank.Team,
                Position = tank.Position,
                Yaw = tank.Yaw,
                TurretYaw = tank.TurretYaw,
                SpeedMps = tank.SpeedMps,
                Health = tank.Health,
                MaxHealth = tank.Spec.MaxHealth,
                ReloadRemainingS = tank.ReloadRemainingS,
                Destroyed = tank.Destroyed,
                Burning = tank.Combat.Fire.Burning,
                ShellSlot = tank.Combat.ShellSlot
            };
        }

        private TankState FindTank(string entityId)
        {
            List<TankState> tanks = _simulation.State.Tanks;
            for (int i = 0; i < tanks.Count; i++)
                if (tanks[i].Id == entityId) return tanks[i];
            return null;
        }

        private static TankInput NeutralInput(TankState tank)
        {
            return new TankInput
            {
                AimPoint = tank.Position +
                    Float3.Forward(tank.Yaw + tank.TurretYaw) * 100f
            };
        }

        private static void ApplyPendingActions(
            ref TankInput input,
            NetworkActionBits actions)
        {
            input.Fire = (actions & NetworkActionBits.Fire) != 0;
            input.UseRepairKit = (actions & NetworkActionBits.RepairKit) != 0;
            input.UseFirstAidKit = (actions & NetworkActionBits.FirstAidKit) != 0;
            input.UseFireExtinguisher =
                (actions & NetworkActionBits.FireExtinguisher) != 0;
        }

        private sealed class PeerState
        {
            public string EntityId;
            public NetworkInputCommand Command;
            public uint InputSequence;
            public bool HasInputSequence;
            public uint ActionSequence;
            public bool HasActionSequence;
            public NetworkActionBits PendingActions;
            public long SnapshotAckTick = -1;
        }
    }
}
