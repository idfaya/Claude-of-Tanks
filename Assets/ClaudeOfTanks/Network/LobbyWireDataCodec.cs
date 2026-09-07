using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    internal static class LobbyWireDataCodec
    {
        private const ushort NullString = ushort.MaxValue;
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        public static void WriteState(
            BinaryWriter writer,
            RoomStateSnapshot state)
        {
            WriteString(writer, state.RoomCode, 16, false);
            writer.Write((byte)state.Phase);
            WriteString(writer, state.HostPlayerId, 64, false);
            writer.Write(state.MaximumPlayers);
            writer.Write(state.MaximumSpectators);
            writer.Write(state.AllowTeamSwitch);
            writer.Write(state.Locked);
            WriteString(writer, state.MapId, 64, false);
            writer.Write((int)state.GameMode);
            writer.Write(state.TeamSize);
            writer.Write(state.Revision);
            writer.Write(state.Round);
            WriteNullableUInt(writer, state.MatchSeed);
            WriteString(writer, state.LastResult, 128, true);
            WriteString(writer, state.LastResultReason, 256, true);
            RoomPlayerSnapshot[] players =
                state.Players ?? Array.Empty<RoomPlayerSnapshot>();
            if (players.Length > AuthoritativeRoom.MaximumPlayers +
                AuthoritativeRoom.MaximumSpectators)
            {
                throw new FormatException("Lobby player count is invalid.");
            }
            writer.Write((byte)players.Length);
            for (int i = 0; i < players.Length; i++)
                WritePlayer(writer, players[i]);
        }

        public static RoomStateSnapshot ReadState(BinaryReader reader)
        {
            RoomStateSnapshot state = new RoomStateSnapshot
            {
                RoomCode = ReadString(reader, 16, false),
                Phase = (RoomPhase)reader.ReadByte(),
                HostPlayerId = ReadString(reader, 64, false),
                MaximumPlayers = reader.ReadInt32(),
                MaximumSpectators = reader.ReadInt32(),
                AllowTeamSwitch = LobbyWireCodec.ReadBoolean(reader),
                Locked = LobbyWireCodec.ReadBoolean(reader),
                MapId = ReadString(reader, 64, false),
                GameMode = (GameModeId)reader.ReadInt32(),
                TeamSize = reader.ReadInt32(),
                Revision = reader.ReadInt64(),
                Round = reader.ReadInt32(),
                MatchSeed = ReadNullableUInt(reader),
                LastResult = ReadString(reader, 128, true),
                LastResultReason = ReadString(reader, 256, true)
            };
            if (!Enum.IsDefined(typeof(RoomPhase), state.Phase) ||
                !Enum.IsDefined(typeof(GameModeId), state.GameMode) ||
                state.MaximumPlayers < 2 ||
                state.MaximumPlayers > AuthoritativeRoom.MaximumPlayers ||
                state.MaximumSpectators < 0 ||
                state.MaximumSpectators > AuthoritativeRoom.MaximumSpectators ||
                state.TeamSize < 1 ||
                state.TeamSize > AuthoritativeRoom.MaximumTeamSize ||
                state.Revision < 0 ||
                state.Round < 0)
            {
                throw new FormatException("Lobby state metadata is invalid.");
            }
            int count = reader.ReadByte();
            if (count > AuthoritativeRoom.MaximumPlayers +
                AuthoritativeRoom.MaximumSpectators)
            {
                throw new FormatException("Lobby player count is invalid.");
            }
            state.Players = new RoomPlayerSnapshot[count];
            HashSet<string> playerIds =
                new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> entityIds =
                new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                RoomPlayerSnapshot player = ReadPlayer(reader);
                if (!playerIds.Add(player.PlayerId) ||
                    !entityIds.Add(player.EntityId))
                {
                    throw new FormatException(
                        "Lobby player identity is duplicated.");
                }
                state.Players[i] = player;
            }
            return state;
        }

        public static void WriteMatchPlan(
            BinaryWriter writer,
            RoomMatchPlan plan)
        {
            if (plan.Round < 1)
                throw new FormatException("Match round is invalid.");
            writer.Write(plan.Round);
            writer.Write(plan.Seed);
            WriteString(writer, plan.MapId, 64, false);
            writer.Write((int)plan.GameMode);
            RoomMatchSeat[] seats = plan.Seats ?? Array.Empty<RoomMatchSeat>();
            if (seats.Length < 1 ||
                seats.Length > AuthoritativeRoom.MaximumPlayers)
            {
                throw new FormatException("Match seat count is invalid.");
            }
            int teamSize = plan.TeamSize > 0
                ? plan.TeamSize
                : InferTeamSize(seats);
            if (teamSize < 1 ||
                teamSize > AuthoritativeRoom.MaximumTeamSize)
            {
                throw new FormatException("Match team size is invalid.");
            }
            writer.Write((byte)teamSize);
            writer.Write((byte)seats.Length);
            for (int i = 0; i < seats.Length; i++)
                WriteSeat(writer, seats[i]);
            string[] spectators =
                plan.SpectatorPlayerIds ?? Array.Empty<string>();
            if (spectators.Length > AuthoritativeRoom.MaximumSpectators)
                throw new FormatException("Spectator count is invalid.");
            writer.Write((byte)spectators.Length);
            for (int i = 0; i < spectators.Length; i++)
                WriteString(writer, spectators[i], 64, false);
        }

        public static RoomMatchPlan ReadMatchPlan(
            BinaryReader reader,
            bool includesTeamSize)
        {
            RoomMatchPlan plan = new RoomMatchPlan
            {
                Round = reader.ReadInt32(),
                Seed = reader.ReadUInt32(),
                MapId = ReadString(reader, 64, false),
                GameMode = (GameModeId)reader.ReadInt32(),
                TeamSize = includesTeamSize
                    ? reader.ReadByte()
                    : 0
            };
            if (plan.Round < 1 ||
                !Enum.IsDefined(typeof(GameModeId), plan.GameMode) ||
                (includesTeamSize &&
                 (plan.TeamSize < 1 ||
                  plan.TeamSize > AuthoritativeRoom.MaximumTeamSize)))
            {
                throw new FormatException("Match plan metadata is invalid.");
            }
            int seatCount = reader.ReadByte();
            if (seatCount < 1 ||
                seatCount > AuthoritativeRoom.MaximumPlayers)
            {
                throw new FormatException("Match seat count is invalid.");
            }
            plan.Seats = new RoomMatchSeat[seatCount];
            HashSet<string> playerIds =
                new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> entityIds =
                new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < seatCount; i++)
            {
                RoomMatchSeat seat = ReadSeat(reader);
                if (!playerIds.Add(seat.PlayerId) ||
                    !entityIds.Add(seat.EntityId))
                {
                    throw new FormatException(
                        "Match seat identity is duplicated.");
                }
                plan.Seats[i] = seat;
            }
            if (!includesTeamSize)
                plan.TeamSize = InferTeamSize(plan.Seats);
            int spectatorCount = reader.ReadByte();
            if (spectatorCount > AuthoritativeRoom.MaximumSpectators)
                throw new FormatException("Spectator count is invalid.");
            plan.SpectatorPlayerIds = new string[spectatorCount];
            for (int i = 0; i < spectatorCount; i++)
            {
                plan.SpectatorPlayerIds[i] =
                    ReadString(reader, 64, false);
                if (!playerIds.Add(plan.SpectatorPlayerIds[i]))
                    throw new FormatException(
                        "Match participant identity is duplicated.");
            }
            return plan;
        }

        private static int InferTeamSize(RoomMatchSeat[] seats)
        {
            int alpha = 0;
            int bravo = 0;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i].Team == Team.Alpha) alpha++;
                else bravo++;
            }
            return Math.Max(1, Math.Max(alpha, bravo));
        }

        private static void WritePlayer(
            BinaryWriter writer,
            RoomPlayerSnapshot player)
        {
            if (player == null)
                throw new FormatException("Lobby player is missing.");
            WriteString(writer, player.PlayerId, 64, false);
            WriteString(writer, player.EntityId, 96, false);
            WriteString(writer, player.DisplayName, 96, false);
            if (!Enum.IsDefined(typeof(RoomTeam), player.Team))
                throw new FormatException("Lobby player team is invalid.");
            writer.Write((byte)player.Team);
            WriteString(writer, player.VehicleSpecId, 64, true);
            WriteStrings(writer, player.Equipment);
            WriteString(writer, player.CamoId, 64, false);
            writer.Write(player.Ready);
            writer.Write(player.Connected);
            writer.Write(player.IsHost);
            writer.Write(player.Rating.HasValue);
            if (player.Rating.HasValue) writer.Write(player.Rating.Value);
        }

        private static RoomPlayerSnapshot ReadPlayer(BinaryReader reader)
        {
            RoomPlayerSnapshot player = new RoomPlayerSnapshot
            {
                PlayerId = ReadString(reader, 64, false),
                EntityId = ReadString(reader, 96, false),
                DisplayName = ReadString(reader, 96, false),
                Team = (RoomTeam)reader.ReadByte(),
                VehicleSpecId = ReadString(reader, 64, true),
                Equipment = ReadStrings(reader),
                CamoId = ReadString(reader, 64, false),
                Ready = LobbyWireCodec.ReadBoolean(reader),
                Connected = LobbyWireCodec.ReadBoolean(reader),
                IsHost = LobbyWireCodec.ReadBoolean(reader)
            };
            if (!Enum.IsDefined(typeof(RoomTeam), player.Team))
                throw new FormatException("Lobby player team is invalid.");
            if (LobbyWireCodec.ReadBoolean(reader))
                player.Rating = reader.ReadInt32();
            return player;
        }

        private static void WriteSeat(
            BinaryWriter writer,
            RoomMatchSeat seat)
        {
            if (seat == null)
                throw new FormatException("Match seat is missing.");
            WriteString(writer, seat.PlayerId, 64, false);
            WriteString(writer, seat.EntityId, 96, false);
            WriteString(writer, seat.DisplayName, 96, false);
            if (!Enum.IsDefined(typeof(Team), seat.Team))
                throw new FormatException("Match seat team is invalid.");
            writer.Write((byte)seat.Team);
            WriteString(writer, seat.VehicleSpecId, 64, false);
            WriteStrings(writer, seat.Equipment);
            WriteString(writer, seat.CamoId, 64, false);
            writer.Write(seat.Rating.HasValue);
            if (seat.Rating.HasValue) writer.Write(seat.Rating.Value);
        }

        private static RoomMatchSeat ReadSeat(BinaryReader reader)
        {
            RoomMatchSeat seat = new RoomMatchSeat
            {
                PlayerId = ReadString(reader, 64, false),
                EntityId = ReadString(reader, 96, false),
                DisplayName = ReadString(reader, 96, false),
                Team = (Team)reader.ReadByte(),
                VehicleSpecId = ReadString(reader, 64, false),
                Equipment = ReadStrings(reader),
                CamoId = ReadString(reader, 64, false)
            };
            if (!Enum.IsDefined(typeof(Team), seat.Team))
                throw new FormatException("Match seat team is invalid.");
            if (LobbyWireCodec.ReadBoolean(reader))
                seat.Rating = reader.ReadInt32();
            return seat;
        }

        private static void WriteStrings(
            BinaryWriter writer,
            string[] values)
        {
            string[] items = values ?? Array.Empty<string>();
            if (items.Length > LobbyWireCodec.MaximumEquipment)
                throw new FormatException("Equipment count is invalid.");
            writer.Write((byte)items.Length);
            for (int i = 0; i < items.Length; i++)
                WriteString(writer, items[i], 64, false);
        }

        private static string[] ReadStrings(BinaryReader reader)
        {
            int count = reader.ReadByte();
            if (count > LobbyWireCodec.MaximumEquipment)
                throw new FormatException("Equipment count is invalid.");
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadString(reader, 64, false);
            return result;
        }

        private static void WriteNullableUInt(
            BinaryWriter writer,
            uint? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue) writer.Write(value.Value);
        }

        private static uint? ReadNullableUInt(BinaryReader reader)
        {
            return LobbyWireCodec.ReadBoolean(reader)
                ? reader.ReadUInt32()
                : (uint?)null;
        }

        private static void WriteString(
            BinaryWriter writer,
            string value,
            int maximumBytes,
            bool nullable)
        {
            if (value == null)
            {
                if (!nullable)
                    throw new FormatException("Lobby text is required.");
                writer.Write(NullString);
                return;
            }
            byte[] bytes = StrictUtf8.GetBytes(value);
            if (bytes.Length > maximumBytes || (!nullable && bytes.Length == 0))
                throw new FormatException("Lobby text length is invalid.");
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(
            BinaryReader reader,
            int maximumBytes,
            bool nullable)
        {
            int length = reader.ReadUInt16();
            if (length == NullString)
            {
                if (!nullable)
                    throw new FormatException("Lobby text is required.");
                return null;
            }
            if (length > maximumBytes || (!nullable && length == 0))
                throw new FormatException("Lobby text length is invalid.");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return StrictUtf8.GetString(bytes);
        }
    }
}
