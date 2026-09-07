using System;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    public enum LobbyWireMessageKind : byte
    {
        Hello = 1,
        Command = 2,
        State = 3,
        Error = 4,
        MatchStart = 5,
        Leave = 6,
        Ping = 7,
        Pong = 8,
        MatchReady = 9
    }

    public enum LobbyCommandKind : byte
    {
        SetName = 1,
        SelectVehicle = 2,
        SelectEquipment = 3,
        SelectCamo = 4,
        SetReady = 5,
        SetTeam = 6,
        SetGameMode = 7,
        SetTeamSize = 8,
        SetMap = 9,
        SetLocked = 10,
        Start = 11
    }

    public sealed class LobbyCommand
    {
        public LobbyCommandKind Kind;
        public string Text;
        public string[] Equipment;
        public bool BoolValue;
        public int IntValue;
        public RoomTeam Team;
        public GameModeId GameMode;
        public uint MatchSeed;
    }

    public sealed class LobbyWireMessage
    {
        public LobbyWireMessageKind Kind;
        public uint Sequence;
        public LobbyCommand Command;
        public RoomStateSnapshot State;
        public RoomMatchPlan MatchPlan;
        public string ErrorCode;
        public string ErrorMessage;
        public uint PingNonce;
    }

    public static class LobbyCommandApplier
    {
        public static RoomMatchPlan Apply(
            AuthoritativeRoom room,
            string playerId,
            LobbyCommand command)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (command == null) throw Policy("invalid_command");
            switch (command.Kind)
            {
                case LobbyCommandKind.SetName:
                    room.SetName(playerId, command.Text);
                    break;
                case LobbyCommandKind.SelectVehicle:
                    room.SelectVehicle(playerId, command.Text);
                    break;
                case LobbyCommandKind.SelectEquipment:
                    room.SelectEquipment(
                        playerId,
                        command.Equipment ?? Array.Empty<string>());
                    break;
                case LobbyCommandKind.SelectCamo:
                    room.SelectCamo(playerId, command.Text);
                    break;
                case LobbyCommandKind.SetReady:
                    room.SetReady(playerId, command.BoolValue);
                    break;
                case LobbyCommandKind.SetTeam:
                    room.SetTeam(playerId, command.Team);
                    break;
                case LobbyCommandKind.SetGameMode:
                    room.SetGameMode(playerId, command.GameMode);
                    break;
                case LobbyCommandKind.SetTeamSize:
                    room.SetTeamSize(playerId, command.IntValue);
                    break;
                case LobbyCommandKind.SetMap:
                    room.SetMap(playerId, command.Text);
                    break;
                case LobbyCommandKind.SetLocked:
                    room.SetLocked(playerId, command.BoolValue);
                    break;
                case LobbyCommandKind.Start:
                    return room.Start(playerId, command.MatchSeed);
                default:
                    throw Policy("invalid_command");
            }
            return null;
        }

        private static RoomPolicyException Policy(string code)
        {
            return new RoomPolicyException(code, "Lobby command is invalid.");
        }
    }
}
