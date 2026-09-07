using System;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public sealed class RankedMatchHandoff : IDisposable
    {
        private DedicatedSocketConnection _connection;

        public RankedMatchHandoff(
            Uri socketEndpoint,
            RankedMatchAssignmentData assignment,
            DedicatedSocketConnection connection)
        {
            SocketEndpoint = socketEndpoint ??
                throw new ArgumentNullException(nameof(socketEndpoint));
            Assignment = assignment ??
                throw new ArgumentNullException(nameof(assignment));
            _connection = connection ??
                throw new ArgumentNullException(nameof(connection));
            if (connection.Admission.MatchId != assignment.matchId ||
                connection.Admission.PlayerId != assignment.playerId)
            {
                throw new ArgumentException(
                    "Ranked assignment does not match the dedicated admission.");
            }
            Plan = CreatePlan(assignment);
            PlayerId = connection.Admission.PlayerId;
            EntityId = connection.Admission.EntityId;
        }

        public Uri SocketEndpoint { get; }
        public RankedMatchAssignmentData Assignment { get; }
        public RoomMatchPlan Plan { get; }
        public string PlayerId { get; }
        public string EntityId { get; }

        public DedicatedNetworkClientRuntime CreateMatchRuntime(
            LocalTankPredictor predictor)
        {
            if (_connection == null)
                throw new InvalidOperationException(
                    "Ranked handoff was already consumed.");
            DedicatedSocketConnection connection = _connection;
            _connection = null;
            return new DedicatedNetworkClientRuntime(
                SocketEndpoint,
                connection,
                predictor);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }

        private static RoomMatchPlan CreatePlan(
            RankedMatchAssignmentData assignment)
        {
            if (string.IsNullOrEmpty(assignment.matchId) ||
                string.IsNullOrEmpty(assignment.playerId) ||
                string.IsNullOrEmpty(assignment.token) ||
                string.IsNullOrEmpty(assignment.mapId) ||
                assignment.roster == null ||
                assignment.roster.Length < 2 ||
                assignment.roster.Length >
                    AuthoritativeRoom.MaximumPlayers)
            {
                throw new FormatException(
                    "Ranked match assignment is incomplete.");
            }
            GameModeId mode;
            if (!Enum.TryParse(
                    assignment.mode,
                    true,
                    out mode) ||
                !Enum.IsDefined(typeof(GameModeId), mode))
            {
                throw new FormatException(
                    "Ranked match mode is invalid.");
            }
            RoomMatchSeat[] seats =
                new RoomMatchSeat[assignment.roster.Length];
            for (int i = 0; i < seats.Length; i++)
            {
                RankedRosterSeatData source = assignment.roster[i];
                if (source == null ||
                    string.IsNullOrEmpty(source.id) ||
                    string.IsNullOrEmpty(source.entityId) ||
                    string.IsNullOrEmpty(source.specId))
                {
                    throw new FormatException(
                        "Ranked roster seat is incomplete.");
                }
                Team team;
                if (string.Equals(
                        source.team,
                        "alpha",
                        StringComparison.OrdinalIgnoreCase))
                {
                    team = Team.Alpha;
                }
                else if (string.Equals(
                    source.team,
                    "bravo",
                    StringComparison.OrdinalIgnoreCase))
                {
                    team = Team.Bravo;
                }
                else
                {
                    throw new FormatException(
                        "Ranked roster team is invalid.");
                }
                seats[i] = new RoomMatchSeat
                {
                    PlayerId = source.id,
                    EntityId = source.entityId,
                    DisplayName = source.name,
                    Team = team,
                    VehicleSpecId = source.specId,
                    Equipment = source.equipment ??
                        Array.Empty<string>(),
                    CamoId = string.IsNullOrEmpty(source.camo)
                        ? "factory"
                        : source.camo,
                    Rating = source.rating
                };
            }
            int teamSize = assignment.teamSize > 0
                ? assignment.teamSize
                : seats.Length / 2;
            if (teamSize < 1 || teamSize * 2 != seats.Length)
                throw new FormatException(
                    "Ranked team size does not match the roster.");
            return new RoomMatchPlan
            {
                Round = assignment.round > 0
                    ? assignment.round
                    : 1,
                Seed = assignment.seed,
                MapId = assignment.mapId,
                GameMode = mode,
                TeamSize = teamSize,
                Seats = seats,
                SpectatorPlayerIds = Array.Empty<string>()
            };
        }
    }
}
