using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public enum RoomPhase
    {
        Waiting,
        Starting,
        Playing,
        Finished
    }

    public enum RoomTeam
    {
        Alpha,
        Bravo,
        Spectator
    }

    public sealed class RoomPolicyException : Exception
    {
        public RoomPolicyException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; }
    }

    public sealed class RoomPlayerSnapshot
    {
        public string PlayerId;
        public string EntityId;
        public string DisplayName;
        public RoomTeam Team;
        public string VehicleSpecId;
        public string[] Equipment;
        public string CamoId;
        public bool Ready;
        public bool Connected;
        public bool IsHost;
        public int? Rating;
    }

    public sealed class RoomStateSnapshot
    {
        public string RoomCode;
        public RoomPhase Phase;
        public string HostPlayerId;
        public int MaximumPlayers;
        public int MaximumSpectators;
        public bool AllowTeamSwitch;
        public bool Locked;
        public string MapId;
        public GameModeId GameMode;
        public int TeamSize;
        public long Revision;
        public int Round;
        public uint? MatchSeed;
        public string LastResult;
        public string LastResultReason;
        public RoomPlayerSnapshot[] Players;
    }

    public sealed class RoomJoinReceipt
    {
        public RoomStateSnapshot State;
        public RoomPlayerSnapshot Player;
        public string ResumeToken;
    }

    public sealed class RoomMatchSeat
    {
        public string PlayerId;
        public string EntityId;
        public string DisplayName;
        public Team Team;
        public string VehicleSpecId;
        public string[] Equipment;
        public string CamoId;
        public int? Rating;
    }

    public sealed class RoomMatchPlan
    {
        public int Round;
        public uint Seed;
        public string MapId;
        public GameModeId GameMode;
        public int TeamSize;
        public RoomMatchSeat[] Seats;
        public string[] SpectatorPlayerIds;
    }

    public sealed class AuthoritativeRoom : IDisposable
    {
        public const int MaximumTeamSize = 7;
        public const int MaximumPlayers = 14;
        public const int MaximumSpectators = 8;
        public const long DefaultReconnectGraceMs = 30000;
        private readonly Dictionary<string, PlayerRecord> _players =
            new Dictionary<string, PlayerRecord>(StringComparer.Ordinal);
        private readonly RoomResumeTokens _tokens = new RoomResumeTokens();
        private readonly Func<string, bool> _vehicleAllowed;
        private readonly Func<string, bool> _mapAllowed;
        private readonly Func<string, bool> _camoAllowed;
        private int _nextEntityOrdinal = 1;
        private bool _disposed;

        public AuthoritativeRoom(
            string roomCode,
            string hostPlayerId,
            string hostName,
            string hostVehicleSpecId = null,
            int teamSize = 1,
            int maximumPlayers = MaximumPlayers,
            int maximumSpectators = MaximumSpectators,
            GameModeId gameMode = GameModeId.Standard,
            string mapId = "random",
            bool allowTeamSwitch = true,
            Func<string, bool> vehicleAllowed = null,
            Func<string, bool> mapAllowed = null,
            Func<string, bool> camoAllowed = null)
        {
            RoomCode = CleanRoomCode(roomCode);
            if (teamSize < 1 || teamSize > MaximumTeamSize)
                throw Policy("invalid_team_size", "Team size must be between one and seven.");
            if (maximumPlayers < 2 || maximumPlayers > MaximumPlayers)
                throw Policy("invalid_capacity", "Player capacity must be between two and fourteen.");
            if (maximumSpectators < 0 || maximumSpectators > MaximumSpectators)
                throw Policy("invalid_capacity", "Spectator capacity is invalid.");
            _vehicleAllowed = vehicleAllowed ?? (_ => true);
            _mapAllowed = mapAllowed ?? (_ => true);
            _camoAllowed = camoAllowed ?? (_ => true);
            TeamSize = teamSize;
            PlayerCapacity = maximumPlayers;
            SpectatorCapacity = maximumSpectators;
            GameMode = gameMode;
            if (!Enum.IsDefined(typeof(GameModeId), GameMode))
                throw Policy("invalid_game_mode", "Game mode is invalid.");
            MapId = CleanContentId(mapId, "invalid_map");
            AllowTeamSwitch = allowTeamSwitch;
            Phase = RoomPhase.Waiting;

            PlayerRecord host = CreatePlayer(
                hostPlayerId,
                hostName,
                RoomTeam.Alpha,
                hostVehicleSpecId,
                true,
                null);
            _players.Add(host.PlayerId, host);
            HostPlayerId = host.PlayerId;
            HostResumeToken = _tokens.Issue(host.PlayerId);
        }

        public string RoomCode { get; }
        public string HostPlayerId { get; private set; }
        public string HostResumeToken { get; private set; }
        public RoomPhase Phase { get; private set; }
        public int PlayerCapacity { get; }
        public int SpectatorCapacity { get; }
        public int TeamSize { get; private set; }
        public bool AllowTeamSwitch { get; }
        public bool Locked { get; private set; }
        public string MapId { get; private set; }
        public GameModeId GameMode { get; private set; }
        public long Revision { get; private set; }
        public int Round { get; private set; }
        public uint? MatchSeed { get; private set; }
        public string LastResult { get; private set; }
        public string LastResultReason { get; private set; }

        public RoomJoinReceipt Join(
            string playerId,
            string displayName,
            RoomTeam? requestedTeam = null,
            string vehicleSpecId = null,
            int? rating = null)
        {
            RequireWaiting();
            if (Locked) throw Policy("lobby_locked", "Room is locked.");
            string id = CleanPlayerId(playerId);
            if (_players.ContainsKey(id)) throw Policy("duplicate_player", "Player already joined.");
            RoomTeam team = requestedTeam ?? AutoTeam();
            if (GameMode == GameModeId.EndlessHorde && team == RoomTeam.Bravo)
                team = RoomTeam.Alpha;
            team = ResolveJoinTeam(team);
            if (GameMode == GameModeId.EndlessHorde)
                TeamSize = Math.Max(TeamSize, ActivePlayerCount() + 1);
            PlayerRecord player = CreatePlayer(
                id,
                UniqueName(displayName, null),
                team,
                vehicleSpecId,
                false,
                rating);
            _players.Add(id, player);
            string token = _tokens.Issue(id);
            Touch();
            return Receipt(player, token);
        }

        public RoomJoinReceipt Reconnect(
            string playerId,
            string resumeToken,
            long nowMs)
        {
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            PlayerRecord player = RequirePlayer(playerId);
            if (player.Connected)
                throw Policy("already_connected", "Player is already connected.");
            if (nowMs > player.ReservedUntilMs ||
                !_tokens.Consume(player.PlayerId, resumeToken, nowMs))
            {
                throw Policy("invalid_resume", "Resume token is invalid or expired.");
            }
            player.Connected = true;
            player.ReservedUntilMs = long.MaxValue;
            string nextToken = _tokens.Issue(player.PlayerId);
            if (player.PlayerId == HostPlayerId) HostResumeToken = nextToken;
            Touch();
            return Receipt(player, nextToken);
        }

        public void Disconnect(
            string playerId,
            long nowMs,
            long reconnectGraceMs = DefaultReconnectGraceMs)
        {
            if (reconnectGraceMs < 0)
                throw new ArgumentOutOfRangeException(nameof(reconnectGraceMs));
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            PlayerRecord player = RequirePlayer(playerId);
            if (!player.Connected) return;
            player.Connected = false;
            player.Ready = false;
            player.ReservedUntilMs = checked(nowMs + reconnectGraceMs);
            _tokens.Reserve(player.PlayerId, player.ReservedUntilMs);
            Touch();
        }

        public void ExpireReservations(long nowMs)
        {
            if (nowMs < 0) throw new ArgumentOutOfRangeException(nameof(nowMs));
            List<string> expired = new List<string>();
            foreach (PlayerRecord player in _players.Values)
            {
                if (!player.Connected && nowMs > player.ReservedUntilMs)
                    expired.Add(player.PlayerId);
            }
            for (int i = 0; i < expired.Count; i++) Remove(expired[i]);
        }

        public void Leave(string playerId)
        {
            Remove(CleanPlayerId(playerId));
        }

        public void SetName(string playerId, string displayName)
        {
            RequireWaiting();
            PlayerRecord player = RequireConnectedPlayer(playerId);
            player.DisplayName = UniqueName(displayName, player.PlayerId);
            Touch();
        }

        public void SelectVehicle(string playerId, string vehicleSpecId)
        {
            RequireWaiting();
            PlayerRecord player = RequireEditablePlayer(playerId);
            string id = CleanContentId(vehicleSpecId, "invalid_vehicle");
            if (!_vehicleAllowed(id))
                throw Policy("vehicle_not_allowed", "Vehicle is unavailable in this room.");
            player.VehicleSpecId = id;
            Touch();
        }

        public void SelectEquipment(string playerId, params string[] equipment)
        {
            RequireWaiting();
            PlayerRecord player = RequireEditablePlayer(playerId);
            player.Equipment = CleanEquipment(equipment);
            Touch();
        }

        public void SelectCamo(string playerId, string camoId)
        {
            RequireWaiting();
            PlayerRecord player = RequireEditablePlayer(playerId);
            string id = CleanContentId(camoId, "invalid_camo");
            if (!_camoAllowed(id))
                throw Policy("camo_not_allowed", "Camouflage is unavailable in this room.");
            player.CamoId = id;
            Touch();
        }

        public void SetReady(string playerId, bool ready)
        {
            RequireWaiting();
            PlayerRecord player = RequireConnectedPlayer(playerId);
            if (player.Team != RoomTeam.Spectator &&
                string.IsNullOrEmpty(player.VehicleSpecId))
            {
                throw Policy("vehicle_required", "Select a vehicle before readying.");
            }
            player.Ready = ready;
            Touch();
        }

        public void SetTeam(string playerId, RoomTeam team)
        {
            RequireWaiting();
            PlayerRecord player = RequireEditablePlayer(playerId);
            if (!Enum.IsDefined(typeof(RoomTeam), team))
                throw Policy("invalid_team", "Team is invalid.");
            if (!AllowTeamSwitch && player.PlayerId != HostPlayerId)
                throw Policy("team_switch_disabled", "Team switching is disabled.");
            if (GameMode == GameModeId.EndlessHorde && team == RoomTeam.Bravo)
                throw Policy("cooperative_team", "Horde players deploy on Alpha.");
            if (player.Team == RoomTeam.Spectator &&
                team != RoomTeam.Spectator &&
                ActivePlayerCount() >= PlayerCapacity)
            {
                throw Policy("lobby_full", "Player slots are full.");
            }
            ValidateTeamCapacity(team, player.PlayerId);
            player.Team = team;
            Touch();
        }

        public void SetGameMode(string playerId, GameModeId gameMode)
        {
            RequireWaiting();
            RequireHost(playerId);
            if (!Enum.IsDefined(typeof(GameModeId), gameMode))
                throw Policy("invalid_game_mode", "Game mode is invalid.");
            if (gameMode == GameModeId.EndlessHorde)
            {
                if (ActivePlayerCount() > MaximumTeamSize)
                    throw Policy("horde_capacity", "Horde supports seven players.");
                TeamSize = Math.Max(TeamSize, ActivePlayerCount());
                foreach (PlayerRecord player in _players.Values)
                    if (player.Team != RoomTeam.Spectator) player.Team = RoomTeam.Alpha;
            }
            GameMode = gameMode;
            ClearReady();
            Touch();
        }

        public void SetTeamSize(string playerId, int teamSize)
        {
            RequireWaiting();
            RequireHost(playerId);
            if (teamSize < 1 || teamSize > MaximumTeamSize)
                throw Policy("invalid_team_size", "Team size must be between one and seven.");
            if (CountTeam(RoomTeam.Alpha, null) > teamSize ||
                CountTeam(RoomTeam.Bravo, null) > teamSize)
            {
                throw Policy("team_size_too_small", "Move players before reducing team size.");
            }
            TeamSize = teamSize;
            ClearReady();
            Touch();
        }

        public void SetMap(string playerId, string mapId)
        {
            RequireWaiting();
            RequireHost(playerId);
            string id = CleanContentId(mapId, "invalid_map");
            if (!_mapAllowed(id)) throw Policy("map_not_allowed", "Map is unavailable.");
            MapId = id;
            ClearReady();
            Touch();
        }

        public void SetLocked(string playerId, bool locked)
        {
            RequireWaiting();
            RequireHost(playerId);
            Locked = locked;
            Touch();
        }

        public RoomMatchPlan Start(string playerId, uint matchSeed)
        {
            RequireWaiting();
            RequireHost(playerId);
            List<RoomMatchSeat> seats = new List<RoomMatchSeat>();
            List<string> spectators = new List<string>();
            foreach (PlayerRecord player in _players.Values)
            {
                if (player.Team == RoomTeam.Spectator)
                {
                    if (player.Connected) spectators.Add(player.PlayerId);
                    continue;
                }
                if (!player.Connected || !player.Ready ||
                    string.IsNullOrEmpty(player.VehicleSpecId))
                {
                    throw Policy("players_not_ready", "Every active player must be connected and ready.");
                }
                seats.Add(new RoomMatchSeat
                {
                    PlayerId = player.PlayerId,
                    EntityId = player.EntityId,
                    DisplayName = player.DisplayName,
                    Team = player.Team == RoomTeam.Alpha ? Team.Alpha : Team.Bravo,
                    VehicleSpecId = player.VehicleSpecId,
                    Equipment = (string[])player.Equipment.Clone(),
                    CamoId = player.CamoId,
                    Rating = player.Rating
                });
            }
            if (seats.Count == 0) throw Policy("players_not_ready", "No active players are ready.");
            Round++;
            MatchSeed = matchSeed;
            Phase = RoomPhase.Starting;
            Locked = true;
            LastResult = null;
            LastResultReason = null;
            Touch();
            return new RoomMatchPlan
            {
                Round = Round,
                Seed = matchSeed,
                MapId = MapId,
                GameMode = GameMode,
                TeamSize = TeamSize,
                Seats = seats.ToArray(),
                SpectatorPlayerIds = spectators.ToArray()
            };
        }

        public void MarkPlaying()
        {
            if (Phase != RoomPhase.Starting) return;
            Phase = RoomPhase.Playing;
            Touch();
        }

        public void Finish(string result, string reason = null)
        {
            if (Phase != RoomPhase.Playing && Phase != RoomPhase.Starting) return;
            Phase = RoomPhase.Waiting;
            Locked = false;
            MatchSeed = null;
            LastResult = result;
            LastResultReason = reason;
            ClearReady();
            Touch();
        }

        public RoomStateSnapshot Snapshot()
        {
            List<RoomPlayerSnapshot> players = new List<RoomPlayerSnapshot>(_players.Count);
            foreach (PlayerRecord player in _players.Values) players.Add(Copy(player));
            players.Sort((a, b) => string.CompareOrdinal(a.PlayerId, b.PlayerId));
            return new RoomStateSnapshot
            {
                RoomCode = RoomCode,
                Phase = Phase,
                HostPlayerId = HostPlayerId,
                MaximumPlayers = PlayerCapacity,
                MaximumSpectators = SpectatorCapacity,
                AllowTeamSwitch = AllowTeamSwitch,
                Locked = Locked,
                MapId = MapId,
                GameMode = GameMode,
                TeamSize = TeamSize,
                Revision = Revision,
                Round = Round,
                MatchSeed = MatchSeed,
                LastResult = LastResult,
                LastResultReason = LastResultReason,
                Players = players.ToArray()
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _tokens.Dispose();
            _players.Clear();
        }

        private PlayerRecord CreatePlayer(
            string playerId,
            string displayName,
            RoomTeam team,
            string vehicleSpecId,
            bool isHost,
            int? rating)
        {
            string id = CleanPlayerId(playerId);
            ValidateTeamCapacity(team, null);
            string vehicle = string.IsNullOrEmpty(vehicleSpecId)
                ? null
                : CleanContentId(vehicleSpecId, "invalid_vehicle");
            if (vehicle != null && !_vehicleAllowed(vehicle))
                throw Policy("vehicle_not_allowed", "Vehicle is unavailable in this room.");
            return new PlayerRecord
            {
                PlayerId = id,
                EntityId = RoomCode.ToLowerInvariant() + "-entity-" + _nextEntityOrdinal++,
                DisplayName = UniqueName(displayName, null),
                Team = team,
                VehicleSpecId = vehicle,
                Equipment = Array.Empty<string>(),
                CamoId = "factory",
                Connected = true,
                IsHost = isHost,
                Rating = rating,
                ReservedUntilMs = long.MaxValue
            };
        }

        private RoomTeam ResolveJoinTeam(RoomTeam team)
        {
            if (!Enum.IsDefined(typeof(RoomTeam), team))
                throw Policy("invalid_team", "Team is invalid.");
            if (team == RoomTeam.Spectator)
            {
                if (CountTeam(team, null) >= SpectatorCapacity)
                    throw Policy("spectators_full", "Spectator slots are full.");
                return team;
            }
            int activeLimit = GameMode == GameModeId.EndlessHorde
                ? Math.Min(PlayerCapacity, MaximumTeamSize)
                : Math.Min(PlayerCapacity, TeamSize * 2);
            if (ActivePlayerCount() >= activeLimit)
            {
                throw Policy("lobby_full", "Player slots are full.");
            }
            int teamLimit = GameMode == GameModeId.EndlessHorde
                ? MaximumTeamSize
                : TeamSize;
            if (CountTeam(team, null) >= teamLimit)
            {
                RoomTeam alternate = team == RoomTeam.Alpha ? RoomTeam.Bravo : RoomTeam.Alpha;
                if (GameMode == GameModeId.EndlessHorde ||
                    CountTeam(alternate, null) >= TeamSize)
                {
                    throw Policy("team_full", "Team slots are full.");
                }
                return alternate;
            }
            return team;
        }

        private void ValidateTeamCapacity(RoomTeam team, string excludingPlayerId)
        {
            if (team == RoomTeam.Spectator)
            {
                if (CountTeam(team, excludingPlayerId) >= SpectatorCapacity)
                    throw Policy("spectators_full", "Spectator slots are full.");
            }
            else if (CountTeam(team, excludingPlayerId) >=
                (GameMode == GameModeId.EndlessHorde ? MaximumTeamSize : TeamSize))
            {
                throw Policy("team_full", "Team slots are full.");
            }
        }

        private void Remove(string playerId)
        {
            PlayerRecord removed;
            if (!_players.TryGetValue(playerId, out removed)) return;
            _players.Remove(playerId);
            _tokens.Remove(playerId);
            if (HostPlayerId == playerId && _players.Count > 0)
            {
                PlayerRecord next = null;
                foreach (PlayerRecord candidate in _players.Values)
                {
                    if (next == null ||
                        string.CompareOrdinal(candidate.PlayerId, next.PlayerId) < 0)
                    {
                        next = candidate;
                    }
                }
                next.IsHost = true;
                HostPlayerId = next.PlayerId;
            }
            Touch();
        }

        private RoomJoinReceipt Receipt(PlayerRecord player, string token)
        {
            return new RoomJoinReceipt
            {
                State = Snapshot(),
                Player = Copy(player),
                ResumeToken = token
            };
        }

        private PlayerRecord RequirePlayer(string playerId)
        {
            string id = CleanPlayerId(playerId);
            PlayerRecord player;
            if (!_players.TryGetValue(id, out player))
                throw Policy("unknown_player", "Player is not in this room.");
            return player;
        }

        private PlayerRecord RequireEditablePlayer(string playerId)
        {
            PlayerRecord player = RequireConnectedPlayer(playerId);
            if (player.Ready)
                throw Policy("selection_locked", "Unready before changing selection.");
            return player;
        }

        private void RequireWaiting()
        {
            if (Phase != RoomPhase.Waiting)
                throw Policy("lobby_locked", "Room can only be edited while waiting.");
        }

        private void RequireHost(string playerId)
        {
            PlayerRecord player = RequireConnectedPlayer(playerId);
            if (player.PlayerId != HostPlayerId)
                throw Policy("host_only", "Only the host may change this setting.");
        }

        private PlayerRecord RequireConnectedPlayer(string playerId)
        {
            PlayerRecord player = RequirePlayer(playerId);
            if (!player.Connected)
                throw Policy("player_disconnected", "Player is disconnected.");
            return player;
        }

        private RoomTeam AutoTeam()
        {
            if (GameMode == GameModeId.EndlessHorde) return RoomTeam.Alpha;
            return CountTeam(RoomTeam.Alpha, null) <= CountTeam(RoomTeam.Bravo, null)
                ? RoomTeam.Alpha
                : RoomTeam.Bravo;
        }

        private int CountTeam(RoomTeam team, string excludingPlayerId)
        {
            int count = 0;
            foreach (PlayerRecord player in _players.Values)
                if (player.PlayerId != excludingPlayerId && player.Team == team) count++;
            return count;
        }

        private int ActivePlayerCount()
        {
            return _players.Count - CountTeam(RoomTeam.Spectator, null);
        }

        private void ClearReady()
        {
            foreach (PlayerRecord player in _players.Values) player.Ready = false;
        }

        private string UniqueName(string displayName, string excludingPlayerId)
        {
            string requested = CleanName(displayName);
            string candidate = requested;
            int suffix = 2;
            while (NameExists(candidate, excludingPlayerId))
            {
                string marker = " (" + suffix++ + ")";
                int length = Math.Min(requested.Length, 24 - marker.Length);
                candidate = requested.Substring(0, length) + marker;
            }
            return candidate;
        }

        private bool NameExists(string name, string excludingPlayerId)
        {
            foreach (PlayerRecord player in _players.Values)
            {
                if (player.PlayerId != excludingPlayerId &&
                    string.Equals(player.DisplayName, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void Touch()
        {
            Revision++;
        }

        private static RoomPlayerSnapshot Copy(PlayerRecord player)
        {
            return new RoomPlayerSnapshot
            {
                PlayerId = player.PlayerId,
                EntityId = player.EntityId,
                DisplayName = player.DisplayName,
                Team = player.Team,
                VehicleSpecId = player.VehicleSpecId,
                Equipment = (string[])player.Equipment.Clone(),
                CamoId = player.CamoId,
                Ready = player.Ready,
                Connected = player.Connected,
                IsHost = player.IsHost,
                Rating = player.Rating
            };
        }

        private static string[] CleanEquipment(string[] equipment)
        {
            if (equipment == null || equipment.Length == 0) return Array.Empty<string>();
            List<string> result = new List<string>(3);
            for (int i = 0; i < equipment.Length && result.Count < 3; i++)
            {
                string id;
                try { id = CleanContentId(equipment[i], "invalid_equipment"); }
                catch (RoomPolicyException) { continue; }
                if (!result.Contains(id)) result.Add(id);
            }
            return result.ToArray();
        }

        private static string CleanRoomCode(string value)
        {
            string code = (value ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length != 6) throw Policy("invalid_room_code", "Room code must be six characters.");
            for (int i = 0; i < code.Length; i++)
                if (!IsAsciiLetterOrDigit(code[i]))
                    throw Policy("invalid_room_code", "Room code must be alphanumeric.");
            return code;
        }

        private static string CleanPlayerId(string value)
        {
            string id = (value ?? string.Empty).Trim();
            if (id.Length < 1 || id.Length > 48)
                throw Policy("invalid_id", "Player id must be 1-48 characters.");
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (!IsAsciiLetterOrDigit(c) && c != '_' && c != '-')
                    throw Policy("invalid_id", "Player id contains unsafe characters.");
            }
            return id;
        }

        private static string CleanName(string value)
        {
            string name = (value ?? string.Empty).Trim();
            if (name.Length == 0) throw Policy("invalid_name", "Display name is required.");
            return name.Length <= 24 ? name : name.Substring(0, 24);
        }

        private static string CleanContentId(string value, string code)
        {
            string id = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (id.Length < 1 || id.Length > 64)
                throw Policy(code, "Content id is invalid.");
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (!IsAsciiLetterOrDigit(c) && c != '_' && c != '-')
                    throw Policy(code, "Content id contains unsafe characters.");
            }
            return id;
        }

        private static bool IsAsciiLetterOrDigit(char value)
        {
            return value >= 'a' && value <= 'z' ||
                value >= 'A' && value <= 'Z' ||
                value >= '0' && value <= '9';
        }

        private static RoomPolicyException Policy(string code, string message)
        {
            return new RoomPolicyException(code, message);
        }

        private sealed class PlayerRecord
        {
            public string PlayerId;
            public string EntityId;
            public string DisplayName;
            public RoomTeam Team;
            public string VehicleSpecId;
            public string[] Equipment;
            public string CamoId;
            public bool Ready;
            public bool Connected;
            public bool IsHost;
            public int? Rating;
            public long ReservedUntilMs;
        }
    }
}
