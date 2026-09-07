using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public static class RoomMatchBattleFactory
    {
        public static BattleSimulation Create(
            ContentCatalog catalog,
            RoomMatchPlan plan)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (plan.Seats == null || plan.Seats.Length == 0)
                throw new ArgumentException(
                    "Match plan requires at least one seat.",
                    nameof(plan));

            MapDefinition map = catalog.GetMap(plan.MapId);
            IHeightField heightField =
                MapSimulationAdapter.BuildHeightField(map);
            BattleState state = new BattleState(
                heightField,
                plan.Seed,
                MapVegetationPlacementBuilder.WorldHalfExtentM,
                MapSimulationAdapter.BuildStaticObstacles(
                    map,
                    heightField));
            int alphaIndex = 0;
            int bravoIndex = 0;
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                RoomMatchSeat seat = plan.Seats[i] ??
                    throw new ArgumentException(
                        "Match plan contains an empty seat.",
                        nameof(plan));
                int teamIndex = seat.Team == Team.Alpha
                    ? alphaIndex++
                    : bravoIndex++;
                Float3 position = Spawn(
                    map,
                    heightField,
                    seat.Team,
                    teamIndex);
                AddTank(
                    state,
                    catalog,
                    seat.EntityId,
                    seat.Team,
                    seat.VehicleSpecId,
                    position,
                    seat.Team == Team.Alpha ? 0f : MathUtil.Pi,
                    seat.Equipment ?? Array.Empty<string>());
            }
            int teamSize = plan.TeamSize > 0
                ? Math.Min(
                    AuthoritativeRoom.MaximumTeamSize,
                    plan.TeamSize)
                : Math.Max(1, Math.Max(alphaIndex, bravoIndex));
            if (plan.GameMode != GameModeId.EndlessHorde)
                FillBots(
                    state,
                    catalog,
                    map,
                    heightField,
                    plan.Seed,
                    Team.Alpha,
                    alphaIndex,
                    teamSize);
            FillBots(
                state,
                catalog,
                map,
                heightField,
                plan.Seed,
                Team.Bravo,
                bravoIndex,
                teamSize);
            return new BattleSimulation(state, plan.GameMode);
        }

        private static void FillBots(
            BattleState state,
            ContentCatalog catalog,
            MapDefinition map,
            IHeightField heightField,
            uint seed,
            Team team,
            int count,
            int target)
        {
            string[] vehicles = catalog.ProductionVehicleIds;
            for (int index = count; index < target; index++)
            {
                int vehicleIndex = (int)(
                    (seed + (uint)team * 31u + (uint)index * 17u) %
                    (uint)vehicles.Length);
                AddTank(
                    state,
                    catalog,
                    "bot-" + team.ToString().ToLowerInvariant() +
                        "-" + (index + 1),
                    team,
                    vehicles[vehicleIndex],
                    Spawn(map, heightField, team, index),
                    team == Team.Alpha ? 0f : MathUtil.Pi,
                    Array.Empty<string>());
            }
        }

        private static void AddTank(
            BattleState state,
            ContentCatalog catalog,
            string entityId,
            Team team,
            string vehicleId,
            Float3 position,
            float yaw,
            string[] equipment)
        {
            TankState tank = new TankState(
                entityId,
                team,
                catalog.GetVehicle(vehicleId).ToTankSpec(),
                position,
                yaw);
            LoadoutSimulation.ApplyEquipment(tank, equipment);
            state.Tanks.Add(tank);
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
                MapPoint[] enemies =
                    map.spawns.enemies ?? Array.Empty<MapPoint>();
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
            float depth =
                row * 8f * (team == Team.Alpha ? -1f : 1f);
            float limit =
                MapVegetationPlacementBuilder.WorldHalfExtentM - 5f;
            float x = MathUtil.Clamp(anchor.x + lateral, -limit, limit);
            float z = MathUtil.Clamp(anchor.z + depth, -limit, limit);
            return new Float3(
                x,
                heightField.HeightAt(x, z),
                z);
        }
    }
}
