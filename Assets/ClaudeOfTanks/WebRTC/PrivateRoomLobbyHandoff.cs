using System;
using ClaudeOfTanks.Network;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomHostMatchHandoff
    {
        private readonly PrivateRoomHostRtcSession _rtc;
        private bool _consumed;

        internal PrivateRoomHostMatchHandoff(
            PrivateRoomHostRtcSession rtc,
            AuthoritativeRoom room,
            RoomMatchPlan plan)
        {
            _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
            Room = room ?? throw new ArgumentNullException(nameof(room));
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        public AuthoritativeRoom Room { get; }
        public RoomMatchPlan Plan { get; }

        public PrivateRoomAuthoritativeHostRuntime CreateMatchRuntime(
            AuthoritativeMatchHost authority,
            LocalTankPredictor hostPredictor = null)
        {
            if (_consumed)
                throw new InvalidOperationException(
                    "Host match handoff was already consumed.");
            if (authority == null)
                throw new ArgumentNullException(nameof(authority));
            RegisterPlan(authority, Plan);
            string hostEntityId = FindEntityId(
                Plan,
                Room.HostPlayerId);
            PrivateRoomAuthoritativeHostRuntime runtime =
                new PrivateRoomAuthoritativeHostRuntime(
                _rtc,
                authority,
                Room.HostPlayerId,
                hostEntityId,
                hostPredictor);
            Room.MarkPlaying();
            _consumed = true;
            return runtime;
        }

        private static void RegisterPlan(
            AuthoritativeMatchHost authority,
            RoomMatchPlan plan)
        {
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                RoomMatchSeat seat = plan.Seats[i];
                authority.RegisterPlayer(seat.PlayerId, seat.EntityId);
            }
            for (int i = 0; i < plan.SpectatorPlayerIds.Length; i++)
                authority.RegisterSpectator(plan.SpectatorPlayerIds[i]);
        }

        private static string FindEntityId(
            RoomMatchPlan plan,
            string playerId)
        {
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                if (plan.Seats[i].PlayerId == playerId)
                    return plan.Seats[i].EntityId;
            }
            for (int i = 0; i < plan.SpectatorPlayerIds.Length; i++)
            {
                if (plan.SpectatorPlayerIds[i] == playerId)
                    return "spectator-" + playerId;
            }
            throw new InvalidOperationException(
                "Host is missing from the match plan.");
        }
    }

    public sealed class PrivateRoomClientMatchHandoff
    {
        private readonly PrivateRoomClientRtcSession _rtc;
        private readonly string _playerId;
        private bool _consumed;

        internal PrivateRoomClientMatchHandoff(
            PrivateRoomClientRtcSession rtc,
            string playerId,
            RoomMatchPlan plan)
        {
            _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
            _playerId = !string.IsNullOrEmpty(playerId)
                ? playerId
                : throw new ArgumentException(
                    "Client player id is required.",
                    nameof(playerId));
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        public RoomMatchPlan Plan { get; }
        public bool IsSpectator => FindSeat(Plan, _playerId) == null;

        public PrivateRoomNetworkClientRuntime CreateMatchRuntime(
            LocalTankPredictor predictor = null)
        {
            if (_consumed)
                throw new InvalidOperationException(
                    "Client match handoff was already consumed.");
            RoomMatchSeat seat = FindSeat(Plan, _playerId);
            string entityId = seat != null
                ? seat.EntityId
                : "spectator-" + _playerId;
            PrivateRoomNetworkClientRuntime runtime =
                new PrivateRoomNetworkClientRuntime(
                _rtc,
                _playerId,
                entityId,
                predictor);
            _consumed = true;
            return runtime;
        }

        private static RoomMatchSeat FindSeat(
            RoomMatchPlan plan,
            string playerId)
        {
            for (int i = 0; i < plan.Seats.Length; i++)
            {
                if (plan.Seats[i].PlayerId == playerId)
                    return plan.Seats[i];
            }
            return null;
        }
    }
}
