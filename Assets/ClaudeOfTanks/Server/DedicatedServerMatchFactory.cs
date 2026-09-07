using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Server
{
    public sealed class DedicatedServerMatchFactory
    {
        private readonly ContentCatalog _catalog;
        private readonly HashSet<string> _productionVehicles;
        private readonly string[] _mapRotation;

        public DedicatedServerMatchFactory(ContentCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _productionVehicles = new HashSet<string>(
                catalog.ProductionVehicleIds,
                StringComparer.Ordinal);
            MapDefinition[] maps = catalog.Maps;
            _mapRotation = new string[maps.Length];
            for (int i = 0; i < maps.Length; i++) _mapRotation[i] = maps[i].id;
            if (_mapRotation.Length == 0)
                throw new InvalidOperationException("Ranked map rotation is empty.");
        }

        public string[] MapRotation => (string[])_mapRotation.Clone();

        public bool IsVehicleAllowed(string id)
        {
            return !string.IsNullOrEmpty(id) && _productionVehicles.Contains(id);
        }

        public AuthoritativeMatchHost Create(RoomMatchPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            MapDefinition map = _catalog.GetMap(plan.MapId);
            IHeightField heightField = MapSimulationAdapter.BuildHeightField(map);
            BattleState state = new BattleState(
                heightField,
                plan.Seed,
                MapVegetationPlacementBuilder.WorldHalfExtentM,
                MapSimulationAdapter.BuildStaticObstacles(map, heightField));
            int alphaIndex = 0;
            int bravoIndex = 0;
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                RoomMatchSeat seat = plan.Seats[i];
                int teamIndex = seat.Team == Team.Alpha
                    ? alphaIndex++
                    : bravoIndex++;
                Float3 position = Spawn(map, heightField, seat.Team, teamIndex);
                TankState tank = new TankState(
                    seat.EntityId,
                    seat.Team,
                    _catalog.GetVehicle(seat.VehicleSpecId).ToTankSpec(),
                    position,
                    seat.Team == Team.Alpha ? 0f : MathUtil.Pi);
                LoadoutSimulation.ApplyEquipment(tank, seat.Equipment);
                state.Tanks.Add(tank);
            }
            AuthoritativeMatchHost host = new AuthoritativeMatchHost(
                new BattleSimulation(state, plan.GameMode));
            for (int i = 0; i < plan.Seats.Length; i++)
                host.RegisterPlayer(plan.Seats[i].PlayerId, plan.Seats[i].EntityId);
            string[] spectators =
                plan.SpectatorPlayerIds ?? Array.Empty<string>();
            for (int i = 0; i < spectators.Length; i++)
                host.RegisterSpectator(spectators[i]);
            return host;
        }

        private static Float3 Spawn(
            MapDefinition map,
            IHeightField heightField,
            Team team,
            int teamIndex)
        {
            MapPoint anchor;
            if (team == Team.Alpha)
            {
                anchor = map.spawns.player;
            }
            else
            {
                MapPoint[] enemies = map.spawns.enemies ?? Array.Empty<MapPoint>();
                anchor = enemies.Length > 0
                    ? enemies[teamIndex % enemies.Length]
                    : new MapPoint
                    {
                        x = -map.spawns.player.x,
                        z = -map.spawns.player.z
                    };
            }
            int row = teamIndex / 3;
            int column = teamIndex % 3;
            float lateral = (column - 1) * 7f;
            float depth = row * 8f * (team == Team.Alpha ? -1f : 1f);
            float x = MathUtil.Clamp(
                anchor.x + lateral,
                -MapVegetationPlacementBuilder.WorldHalfExtentM + 5f,
                MapVegetationPlacementBuilder.WorldHalfExtentM - 5f);
            float z = MathUtil.Clamp(
                anchor.z + depth,
                -MapVegetationPlacementBuilder.WorldHalfExtentM + 5f,
                MapVegetationPlacementBuilder.WorldHalfExtentM - 5f);
            return new Float3(x, heightField.HeightAt(x, z), z);
        }
    }
}
