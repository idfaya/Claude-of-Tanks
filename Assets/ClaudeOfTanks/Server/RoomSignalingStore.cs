using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ClaudeOfTanks.Server
{
    internal sealed class RoomSignalingStore
    {
        private readonly object _gate = new object();
        private readonly Dictionary<string, Room> _rooms =
            new Dictionary<string, Room>(StringComparer.Ordinal);

        public RoomSignalingResult Create(
            RoomSignalingConnection connection,
            RoomSignalingWirePayload payload)
        {
            RequirePlayer(payload);
            string code = CreateRoomCode();
            Room room = new Room
            {
                Code = code,
                HostId = payload.player.id,
                HostName = payload.player.name,
                Mode = string.IsNullOrEmpty(payload.mode)
                    ? "private"
                    : payload.mode,
                MaximumPlayers = Math.Max(2, Math.Min(14, payload.maxPlayers))
            };
            string resumeToken;
            Peer peer = PeerFrom(connection, payload, out resumeToken);
            lock (_gate)
            {
                while (_rooms.ContainsKey(code))
                    code = CreateRoomCode();
                room.Code = code;
                room.Peers.Add(peer.Id, peer);
                _rooms.Add(code, room);
                connection.Peer = peer;
                connection.Room = room;
                return new RoomSignalingResult
                {
                    Response = RoomPayload(room, peer, false, resumeToken)
                };
            }
        }

        public RoomSignalingResult Join(
            RoomSignalingConnection connection,
            RoomSignalingWirePayload payload)
        {
            RequirePlayer(payload);
            string code = NormalizeRoomCode(payload.roomCode);
            lock (_gate)
            {
                if (!_rooms.TryGetValue(code, out Room room))
                    throw Error("room_not_found", "Room does not exist.");
                bool resumed = room.Peers.TryGetValue(
                    payload.player.id,
                    out Peer peer);
                if (!resumed && room.Peers.Count >= room.MaximumPlayers)
                    throw Error("room_full", "Room is full.");
                if (resumed)
                {
                    RequireResumeToken(peer, payload.resumeToken);
                    RoomSignalingConnection previous = peer.Connection;
                    if (previous != null && previous != connection)
                    {
                        previous.Room = null;
                        previous.Peer = null;
                    }
                    peer.Name = payload.player.name;
                    peer.SessionId = CleanSessionId(payload.sessionId);
                    peer.Connection = connection;
                }
                else
                {
                    peer = PeerFrom(connection, payload, out _);
                    room.Peers.Add(peer.Id, peer);
                }
                string resumeToken = RotateResumeToken(peer);
                connection.Peer = peer;
                connection.Room = room;
                RoomSignalingResult result = new RoomSignalingResult
                {
                    Response = RoomPayload(room, peer, true, resumeToken)
                };
                foreach (Peer other in room.Peers.Values)
                {
                    if (other == peer) continue;
                    result.Deliveries.Add(new RoomSignalingDelivery(
                        other,
                        Event(
                            "peer_joined",
                            new RoomSignalingWirePayload
                            {
                                roomCode = room.Code,
                                peerId = peer.Id,
                                sessionId = peer.SessionId,
                                player = Player(peer),
                                hostId = room.HostId
                            })));
                }
                DrainMailbox(peer, result.Deliveries);
                return result;
            }
        }

        public RoomSignalingResult Relay(
            RoomSignalingConnection connection,
            RoomSignalingWirePayload payload)
        {
            lock (_gate)
            {
                Peer sender = RequireMembership(connection, payload.roomCode);
                if (!connection.Room.Peers.TryGetValue(
                    payload.toPeerId ?? string.Empty,
                    out Peer target))
                {
                    throw Error("peer_not_found", "Target peer does not exist.");
                }
                if (!string.IsNullOrEmpty(payload.toSessionId) &&
                    payload.toSessionId != target.SessionId)
                {
                    throw Error("stale_peer_session", "Target session is stale.");
                }
                RoomSignalingEnvelope message = Event(
                    "room_signal",
                    new RoomSignalingWirePayload
                    {
                        roomCode = connection.Room.Code,
                        toPeerId = target.Id,
                        toSessionId = target.SessionId,
                        fromPeerId = sender.Id,
                        fromSessionId = sender.SessionId,
                        signal = payload.signal
                    });
                RoomSignalingResult result = new RoomSignalingResult();
                if (target.Connection == null)
                    Enqueue(target, message);
                else
                    result.Deliveries.Add(
                        new RoomSignalingDelivery(target, message));
                return result;
            }
        }

        public RoomSignalingResult Poll(
            RoomSignalingConnection connection,
            string roomCode)
        {
            lock (_gate)
            {
                Peer peer = RequireMembership(connection, roomCode);
                RoomSignalingResult result = new RoomSignalingResult();
                DrainMailbox(peer, result.Deliveries);
                return result;
            }
        }

        public RoomSignalingResult Leave(
            RoomSignalingConnection connection,
            string reason)
        {
            lock (_gate)
            {
                Room room = connection.Room;
                Peer peer = connection.Peer;
                if (room == null || peer == null)
                    return new RoomSignalingResult();
                connection.Room = null;
                connection.Peer = null;
                if (!room.Peers.TryGetValue(peer.Id, out Peer current) ||
                    current.Connection != connection)
                {
                    return new RoomSignalingResult();
                }
                room.Peers.Remove(peer.Id);
                RoomSignalingResult result = new RoomSignalingResult();
                string eventType = peer.Id == room.HostId
                    ? "room_closed"
                    : "peer_left";
                foreach (Peer other in room.Peers.Values)
                {
                    result.Deliveries.Add(new RoomSignalingDelivery(
                        other,
                        Event(
                            eventType,
                            new RoomSignalingWirePayload
                            {
                                roomCode = room.Code,
                                peerId = peer.Id,
                                sessionId = peer.SessionId,
                                reason = reason
                            })));
                }
                if (peer.Id == room.HostId)
                    _rooms.Remove(room.Code);
                return result;
            }
        }

        public void Detach(RoomSignalingConnection connection)
        {
            lock (_gate)
            {
                Peer peer = connection.Peer;
                if (peer != null && peer.Connection == connection)
                    peer.Connection = null;
                connection.Room = null;
                connection.Peer = null;
            }
        }

        private static RoomSignalingWirePayload RoomPayload(
            Room room,
            Peer self,
            bool excludeSelf,
            string resumeToken)
        {
            List<RoomSignalingWirePeer> peers =
                new List<RoomSignalingWirePeer>();
            foreach (Peer peer in room.Peers.Values)
            {
                if (excludeSelf && peer == self) continue;
                peers.Add(new RoomSignalingWirePeer
                {
                    peerId = peer.Id,
                    sessionId = peer.SessionId,
                    isHost = peer.Id == room.HostId,
                    player = Player(peer)
                });
            }
            return new RoomSignalingWirePayload
            {
                roomCode = room.Code,
                peerId = self.Id,
                sessionId = self.SessionId,
                hostId = room.HostId,
                hostName = room.HostName,
                mode = room.Mode,
                maxPlayers = room.MaximumPlayers,
                resumeToken = resumeToken,
                peers = peers.ToArray()
            };
        }

        private static Peer PeerFrom(
            RoomSignalingConnection connection,
            RoomSignalingWirePayload payload,
            out string resumeToken)
        {
            Peer peer = new Peer
            {
                Id = payload.player.id,
                Name = payload.player.name,
                SessionId = CleanSessionId(payload.sessionId),
                Connection = connection
            };
            resumeToken = RotateResumeToken(peer);
            return peer;
        }

        private static string RotateResumeToken(Peer peer)
        {
            byte[] bytes = new byte[32];
            using (RandomNumberGenerator random =
                RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            string token = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            peer.ResumeTokenHash = Hash(token);
            return token;
        }

        private static void RequireResumeToken(
            Peer peer,
            string resumeToken)
        {
            byte[] actual = Hash(resumeToken ?? string.Empty);
            byte[] expected = peer.ResumeTokenHash;
            int difference = expected == null
                ? 1
                : expected.Length ^ actual.Length;
            int count = expected == null
                ? 0
                : Math.Min(expected.Length, actual.Length);
            for (int i = 0; i < count; i++)
                difference |= expected[i] ^ actual[i];
            if (difference != 0)
                throw Error(
                    "invalid_resume",
                    "Room resume token is invalid.");
        }

        private static byte[] Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return sha.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(value));
        }

        private static RoomSignalingWirePlayer Player(Peer peer)
        {
            return new RoomSignalingWirePlayer
            {
                id = peer.Id,
                name = peer.Name
            };
        }

        private static void RequirePlayer(RoomSignalingWirePayload payload)
        {
            if (payload?.player == null ||
                !SafeIdentifier(payload.player.id, 1, 48) ||
                string.IsNullOrWhiteSpace(payload.player.name))
            {
                throw Error("invalid_player", "Player identity is invalid.");
            }
            payload.player.name = payload.player.name.Trim();
            if (payload.player.name.Length > 24)
                payload.player.name = payload.player.name.Substring(0, 24);
        }

        private static string CleanSessionId(string value)
        {
            string session = (value ?? string.Empty).Trim();
            if (!SafeIdentifier(session, 8, 64))
                throw Error("invalid_session", "Session id is invalid.");
            return session;
        }

        private static string NormalizeRoomCode(string value)
        {
            string code = (value ?? string.Empty).Trim().ToUpperInvariant();
            if (!SafeIdentifier(code, 6, 6))
                throw Error("invalid_room_code", "Room code is invalid.");
            return code;
        }

        private static bool SafeIdentifier(
            string value,
            int minimum,
            int maximum)
        {
            if (value == null ||
                value.Length < minimum ||
                value.Length > maximum)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                    return false;
            }
            return true;
        }

        private static string CreateRoomCode()
        {
            const string alphabet =
                "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            byte[] bytes = Guid.NewGuid().ToByteArray();
            char[] code = new char[6];
            for (int i = 0; i < code.Length; i++)
                code[i] = alphabet[bytes[i] % alphabet.Length];
            return new string(code);
        }

        private static void Enqueue(
            Peer peer,
            RoomSignalingEnvelope message)
        {
            if (peer.Mailbox.Count >= 64)
                peer.Mailbox.Dequeue();
            peer.Mailbox.Enqueue(message);
        }

        private static void DrainMailbox(
            Peer peer,
            List<RoomSignalingDelivery> deliveries)
        {
            while (peer.Mailbox.Count > 0)
                deliveries.Add(
                    new RoomSignalingDelivery(
                        peer,
                        peer.Mailbox.Dequeue()));
        }

        private static Peer RequireMembership(
            RoomSignalingConnection connection,
            string roomCode)
        {
            if (connection.Room == null ||
                connection.Peer == null ||
                connection.Peer.Connection != connection ||
                connection.Room.Code != NormalizeRoomCode(roomCode))
            {
                throw Error("not_in_room", "Connection is not in this room.");
            }
            return connection.Peer;
        }

        private static RoomSignalingEnvelope Event(
            string type,
            RoomSignalingWirePayload payload)
        {
            return new RoomSignalingEnvelope
            {
                type = type,
                payload = payload
            };
        }

        private static RoomSignalingStoreException Error(
            string code,
            string message)
        {
            return new RoomSignalingStoreException(code, message);
        }

        internal sealed class Room
        {
            public string Code;
            public string HostId;
            public string HostName;
            public string Mode;
            public int MaximumPlayers;
            public readonly Dictionary<string, Peer> Peers =
                new Dictionary<string, Peer>(StringComparer.Ordinal);
        }

        internal sealed class Peer
        {
            public string Id;
            public string Name;
            public string SessionId;
            public byte[] ResumeTokenHash;
            public RoomSignalingConnection Connection;
            public readonly Queue<RoomSignalingEnvelope> Mailbox =
                new Queue<RoomSignalingEnvelope>();
        }
    }

    internal sealed class RoomSignalingResult
    {
        public RoomSignalingWirePayload Response;
        public readonly List<RoomSignalingDelivery> Deliveries =
            new List<RoomSignalingDelivery>();
    }

    internal readonly struct RoomSignalingDelivery
    {
        public RoomSignalingDelivery(
            RoomSignalingStore.Peer peer,
            RoomSignalingEnvelope message)
        {
            Peer = peer;
            Message = message;
        }

        public RoomSignalingStore.Peer Peer { get; }
        public RoomSignalingEnvelope Message { get; }
    }

    internal sealed class RoomSignalingStoreException : Exception
    {
        public RoomSignalingStoreException(string code, string message)
            : base(message)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
