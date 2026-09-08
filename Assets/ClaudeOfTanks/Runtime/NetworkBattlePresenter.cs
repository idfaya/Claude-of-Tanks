using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class NetworkBattlePresenter : IDisposable
    {
        public const double InterpolationDelayMs = 100.0;
        private readonly ContentCatalog _catalog;
        private readonly BattleState _state;
        private readonly MatchModeState _matchMode;
        private readonly MapRuntime _map;
        private readonly string _mapId;
        private readonly BattleEffects _effects;
        private readonly BattleAudio _audio;
        private readonly Dictionary<string, TankState> _tanks =
            new Dictionary<string, TankState>(StringComparer.Ordinal);
        private readonly Dictionary<string, TankView> _views =
            new Dictionary<string, TankView>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _camouflageIds =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<int, GameObject> _shells =
            new Dictionary<int, GameObject>();
        private readonly List<TankState> _visibleTanks =
            new List<TankState>();
        private readonly List<int> _staleShells = new List<int>();
        private readonly HashSet<string> _visibleIds =
            new HashSet<string>(StringComparer.Ordinal);
        private long _eventTick = -1;
        private uint _staticRevision;

        public NetworkBattlePresenter(
            ContentCatalog catalog,
            RoomMatchPlan plan,
            Transform parent)
        {
            _catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            MapDefinition map = catalog.GetMap(plan.MapId);
            _mapId = plan.MapId;
            IHeightField heightField =
                MapSimulationAdapter.BuildHeightField(map);
            _state = new BattleState(
                heightField,
                plan.Seed,
                MapVegetationPlacementBuilder.WorldHalfExtentM,
                MapSimulationAdapter.BuildStaticObstacles(
                    map,
                    heightField));
            _matchMode = new MatchModeState(plan.GameMode);
            RoomMatchSeat[] seats =
                plan.Seats ?? Array.Empty<RoomMatchSeat>();
            for (int i = 0; i < seats.Length; i++)
            {
                RoomMatchSeat seat = seats[i];
                if (seat != null &&
                    !string.IsNullOrEmpty(seat.EntityId))
                {
                    _camouflageIds[seat.EntityId] =
                        string.IsNullOrEmpty(seat.CamoId)
                            ? "factory"
                            : seat.CamoId;
                }
            }
            _map = MapRuntime.Create(map);
            _map.Root.SetParent(parent, false);
            _effects = BattleEffects.Create();
            _effects.transform.SetParent(parent, false);
            _audio = BattleAudio.Create();
            _audio.transform.SetParent(parent, false);
        }

        public BattleState State => _state;
        public MatchModeState MatchMode => _matchMode;
        public IList<TankState> VisibleTanks => _visibleTanks;
        public BattleEffects Effects => _effects;
        public BattleAudio Audio => _audio;
        public string Status { get; private set; } = "CONNECTED";
        public float StatusUntil { get; private set; }
        public int ShotsFired { get; private set; }
        public int Hits { get; private set; }
        public int Penetrations { get; private set; }
        public float DamageDealt { get; private set; }
        public float DamageReceived { get; private set; }

        public TankState Apply(
            NetworkWorldSnapshot snapshot,
            SnapshotBuffer buffer,
            LocalTankPredictor predictor,
            string localEntityId)
        {
            if (snapshot == null || buffer == null) return null;
            _state.TimeS = (float)(snapshot.ServerTimeMs / 1000.0);
            ApplyMatchMode(snapshot);
            ApplyStaticState(snapshot);
            _visibleIds.Clear();
            _visibleTanks.Clear();
            double sampleTime =
                snapshot.ServerTimeMs - InterpolationDelayMs;
            for (int i = 0; i < snapshot.Entities.Length; i++)
            {
                NetworkEntitySnapshot authority = snapshot.Entities[i];
                TankState tank = EnsureTank(authority);
                if (predictor != null &&
                    authority.EntityId == localEntityId)
                {
                    ApplyPredicted(tank, predictor);
                }
                else
                {
                    SampledEntityState sample;
                    if (!buffer.TrySampleEntity(
                        authority.EntityId,
                        sampleTime,
                        out sample))
                    {
                        sample = From(authority);
                    }
                    ApplySample(tank, sample);
                }
                _views[tank.Id].Root.gameObject.SetActive(true);
                _views[tank.Id].Sync(tank);
                _visibleIds.Add(tank.Id);
                _visibleTanks.Add(tank);
            }
            foreach (KeyValuePair<string, TankView> pair in _views)
            {
                if (!_visibleIds.Contains(pair.Key) &&
                    pair.Value.Root.gameObject.activeSelf)
                {
                    pair.Value.Root.gameObject.SetActive(false);
                }
            }
            SyncShells(snapshot.Shells);
            if (snapshot.Tick > _eventTick)
            {
                PlayEvents(snapshot.Events, localEntityId);
                _eventTick = snapshot.Tick;
            }
            TankState local;
            if (!string.IsNullOrEmpty(localEntityId) &&
                _tanks.TryGetValue(localEntityId, out local))
            {
                return local;
            }
            return _visibleTanks.Count > 0 ? _visibleTanks[0] : null;
        }

        public void UpdateVisibility(Vector3 cameraPosition)
        {
            _map.UpdateVegetationVisibility(cameraPosition);
        }

        public void UpdateAudio(
            Vector3 listenerPosition,
            string listenerOwnerId,
            bool scoped)
        {
            _audio.SyncEngines(
                _visibleTanks,
                listenerOwnerId,
                listenerPosition,
                scoped);
        }

        public void SetTankVisible(string entityId, bool visible)
        {
            TankView view;
            if (_views.TryGetValue(entityId, out view))
                view.Root.gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            foreach (TankView view in _views.Values) view.Destroy();
            foreach (GameObject shell in _shells.Values) DestroyShell(shell);
            _views.Clear();
            _tanks.Clear();
            _camouflageIds.Clear();
            _shells.Clear();
            _map.Dispose();
            Release(_effects.gameObject);
            Release(_audio.gameObject);
        }

        private TankState EnsureTank(NetworkEntitySnapshot entity)
        {
            TankState tank;
            if (_tanks.TryGetValue(entity.EntityId, out tank)) return tank;
            VehicleDefinition definition =
                _catalog.GetVehicle(entity.VehicleSpecId);
            TankSpec spec = definition.ToTankSpec();
            spec.MaxHealth = entity.MaxHealth;
            tank = new TankState(
                entity.EntityId,
                entity.Team,
                spec,
                entity.Position,
                entity.Yaw);
            _state.Tanks.Add(tank);
            _tanks.Add(tank.Id, tank);
            string camouflageId;
            if (!_camouflageIds.TryGetValue(
                    tank.Id,
                    out camouflageId))
            {
                camouflageId = "auto";
            }
            _views.Add(
                tank.Id,
                TankView.Create(
                    tank,
                    definition,
                    camouflageId,
                    _mapId,
                    _catalog));
            return tank;
        }

        private static void ApplySample(
            TankState tank,
            SampledEntityState sample)
        {
            tank.Position = sample.Position;
            tank.Yaw = sample.Yaw;
            tank.TurretYaw = sample.TurretYaw;
            tank.SpeedMps = sample.SpeedMps;
            tank.Health = sample.Health;
            tank.ReloadRemainingS = sample.ReloadRemainingS;
            tank.Destroyed = sample.Destroyed;
            tank.Kills = sample.Kills;
            tank.Combat.Health = sample.Health;
            tank.Combat.Destroyed = sample.Destroyed;
            tank.Combat.Fire.Burning = sample.Burning;
            if (sample.ShellSlot >= 0 &&
                sample.ShellSlot < tank.Combat.Ammo.Length)
            {
                tank.Combat.ShellSlot = sample.ShellSlot;
            }
        }

        private static void ApplyPredicted(
            TankState target,
            LocalTankPredictor predictor)
        {
            TankState source = predictor.State;
            target.Position = predictor.PresentedPosition;
            target.Yaw = predictor.PresentedYaw;
            target.TurretYaw = source.TurretYaw;
            target.SpeedMps = source.SpeedMps;
            target.Health = source.Health;
            target.ReloadRemainingS = source.ReloadRemainingS;
            target.Destroyed = source.Destroyed;
            target.Kills = source.Kills;
            target.Combat.Health = source.Combat.Health;
            target.Combat.Destroyed = source.Combat.Destroyed;
            target.Combat.Fire.Burning = source.Combat.Fire.Burning;
            target.Combat.ShellSlot = source.Combat.ShellSlot;
        }

        private void ApplyMatchMode(NetworkWorldSnapshot snapshot)
        {
            NetworkMatchModeSnapshot source = snapshot.MatchMode;
            _matchMode.AlphaScore = source.AlphaScore;
            _matchMode.BravoScore = source.BravoScore;
            _matchMode.Winner = snapshot.Winner;
            _matchMode.Draw = snapshot.Draw;
            Array.Copy(source.Zones, _matchMode.Zones, 3);
            Array.Copy(source.ZoneControl, _matchMode.ZoneControl, 3);
            Array.Copy(source.ZoneOwners, _matchMode.ZoneOwners, 3);
            _matchMode.AlphaFlag = source.AlphaFlag;
            _matchMode.BravoFlag = source.BravoFlag;
            _matchMode.AlphaFlagCarrier = source.AlphaFlagCarrier;
            _matchMode.BravoFlagCarrier = source.BravoFlagCarrier;
            _matchMode.BallPosition = source.BallPosition;
            _matchMode.BallVelocity = source.BallVelocity;
            _matchMode.HordeWave = source.HordeWave;
        }

        private void ApplyStaticState(NetworkWorldSnapshot snapshot)
        {
            if (snapshot.StaticObstacleRevision == _staticRevision) return;
            ushort[] destroyed = snapshot.DestroyedStaticObstacleIndices;
            for (int i = 0; i < destroyed.Length; i++)
            {
                int index = destroyed[i];
                if (index < _state.StaticObstacles.Length &&
                    !_state.IsStaticObstacleDestroyed(index))
                {
                    _state.DamageStaticObstacle(index, float.MaxValue);
                }
            }
            _staticRevision = snapshot.StaticObstacleRevision;
            _map.SyncDestroyedStructures(_state);
        }

        private void SyncShells(NetworkShellSnapshot[] snapshots)
        {
            _staleShells.Clear();
            foreach (int id in _shells.Keys) _staleShells.Add(id);
            for (int i = 0; i < snapshots.Length; i++)
            {
                NetworkShellSnapshot snapshot = snapshots[i];
                GameObject shell;
                if (!_shells.TryGetValue(snapshot.Id, out shell))
                {
                    shell = GameObject.CreatePrimitive(
                        PrimitiveType.Sphere);
                    shell.name = "NetworkShell-" + snapshot.Id;
                    shell.transform.localScale = Vector3.one * 0.18f;
                    Collider collider = shell.GetComponent<Collider>();
                    if (collider != null) Release(collider);
                    shell.GetComponent<Renderer>().sharedMaterial =
                        new Material(Shader.Find("Standard"))
                        {
                            color = new Color(1f, 0.72f, 0.14f)
                        };
                    _shells.Add(snapshot.Id, shell);
                }
                shell.transform.position = snapshot.Position.ToUnity();
                _staleShells.Remove(snapshot.Id);
            }
            for (int i = 0; i < _staleShells.Count; i++)
            {
                int id = _staleShells[i];
                DestroyShell(_shells[id]);
                _shells.Remove(id);
            }
        }

        private void PlayEvents(
            BattleEvent[] events,
            string localEntityId)
        {
            for (int i = 0; i < events.Length; i++)
            {
                BattleEvent battleEvent = events[i];
                _audio.Play(battleEvent);
                TankView target;
                _views.TryGetValue(battleEvent.TargetId ?? string.Empty, out target);
                _effects.Play(
                    battleEvent,
                    target != null ? target.Root : null);
                if (battleEvent.Type == BattleEventType.ShellFired &&
                    battleEvent.SourceId == localEntityId)
                {
                    ShotsFired++;
                }
                else if (battleEvent.Type == BattleEventType.ShellHit)
                {
                    if (battleEvent.SourceId == localEntityId)
                    {
                        Hits++;
                        if (battleEvent.Penetrated) Penetrations++;
                        DamageDealt += battleEvent.Value;
                    }
                    if (battleEvent.TargetId == localEntityId)
                        DamageReceived += battleEvent.Value;
                    Status = battleEvent.Penetrated
                        ? Mathf.RoundToInt(battleEvent.Value).ToString()
                        : "RICOCHET";
                    StatusUntil = Time.unscaledTime + 0.8f;
                }
                else if (battleEvent.Type ==
                    BattleEventType.TankDestroyed)
                {
                    Status = "DESTROYED";
                    StatusUntil = Time.unscaledTime + 1.4f;
                }
            }
        }

        private static SampledEntityState From(
            NetworkEntitySnapshot source)
        {
            return new SampledEntityState
            {
                EntityId = source.EntityId,
                VehicleSpecId = source.VehicleSpecId,
                Team = source.Team,
                Position = source.Position,
                Yaw = source.Yaw,
                TurretYaw = source.TurretYaw,
                SpeedMps = source.SpeedMps,
                Health = source.Health,
                MaxHealth = source.MaxHealth,
                ReloadRemainingS = source.ReloadRemainingS,
                Destroyed = source.Destroyed,
                Burning = source.Burning,
                ShellSlot = source.ShellSlot,
                Kills = source.Kills
            };
        }

        private static void DestroyShell(GameObject shell)
        {
            Renderer renderer =
                shell != null ? shell.GetComponent<Renderer>() : null;
            if (renderer != null) Release(renderer.sharedMaterial);
            Release(shell);
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
