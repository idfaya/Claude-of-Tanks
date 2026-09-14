using System;
using System.Text;
using UnityEngine;

namespace ClaudeOfTanks.WebRTC
{
    public enum RoomSignalingState
    {
        Idle,
        Connecting,
        Open,
        Reconnecting,
        Closed
    }

    [Serializable]
    public sealed class SignalingPlayer
    {
        public string id;
        public string name;
    }

    [Serializable]
    public sealed class SignalingPeer
    {
        public string peerId;
        public SignalingPlayer player;
        public string sessionId;
        public bool isHost;
    }

    [Serializable]
    public sealed class SignalingPayload
    {
        public string roomCode;
        public string peerId;
        public string hostId;
        public string hostName;
        public string mode;
        public int maxPlayers;
        public SignalingPeer[] peers;
        public SignalingPlayer player;
        public string sessionId;
        public string resumeToken;
        public string toPeerId;
        public string toSessionId;
        public string fromPeerId;
        public string fromSessionId;
        public string reason;
        public string state;
        public int attempt;
        public int delayMs;
        public string code;
        public string message;
        public WebRtcSignal signal;
    }

    [Serializable]
    public sealed class SignalingEnvelope
    {
        public string type;
        public string requestId;
        public SignalingPayload payload;
    }

    public sealed class SignalingRoomInfo
    {
        internal SignalingRoomInfo(
            string roomCode,
            string peerId,
            string hostId,
            string hostName,
            string mode,
            int maxPlayers,
            SignalingPeer[] peers,
            string resumeToken)
        {
            RoomCode = roomCode;
            PeerId = peerId;
            HostId = hostId;
            HostName = hostName ?? string.Empty;
            Mode = mode ?? string.Empty;
            MaxPlayers = maxPlayers;
            Peers = peers ?? Array.Empty<SignalingPeer>();
            ResumeToken = resumeToken ?? string.Empty;
        }

        public string RoomCode { get; }
        public string PeerId { get; }
        public string HostId { get; }
        public string HostName { get; }
        public string Mode { get; }
        public int MaxPlayers { get; }
        public SignalingPeer[] Peers { get; }
        internal string ResumeToken { get; }
    }

    public sealed class RoomSignalingException : Exception
    {
        public RoomSignalingException(string code, string message)
            : base(message)
        {
            Code = string.IsNullOrEmpty(code) ? "signaling_error" : code;
        }

        public string Code { get; }
    }

    public static class RoomSignalingProtocol
    {
        public const int MaximumMessageBytes = 128 * 1024;
        public const int MaximumQueuedEvents = 64;
        public const int MaximumQueuedSignals = 256;
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        public static SignalingPlayer CleanPlayer(string id, string name)
        {
            string cleanId = (id ?? string.Empty).Trim();
            string cleanName = CollapseSpaces(name).Trim();
            if (cleanName.Length > 24)
                cleanName = cleanName.Substring(0, 24);
            if (!IsSafeIdentifier(cleanId, 1, 48) ||
                string.IsNullOrEmpty(cleanName))
            {
                throw new ArgumentException(
                    "Signaling player requires a safe id and name.");
            }
            return new SignalingPlayer { id = cleanId, name = cleanName };
        }

        public static string CleanSessionId(string value)
        {
            string id = (value ?? string.Empty).Trim();
            if (!IsSafeIdentifier(id, 8, 64))
                throw new ArgumentException("Invalid signaling session id.");
            return id;
        }

        public static string CreateSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string NormalizeRoomCode(string value)
        {
            string source = (value ?? string.Empty).ToUpperInvariant();
            StringBuilder code = new StringBuilder(6);
            for (int i = 0; i < source.Length && code.Length < 6; i++)
            {
                char c = source[i];
                if ((c < 'A' || c > 'Z') && (c < '0' || c > '9'))
                    continue;
                if (c == '0' || c == 'O') c = 'Q';
                else if (c == '1' || c == 'I') c = 'L';
                code.Append(c);
            }
            if (code.Length != 6)
                throw new ArgumentException("Room code must be six alphanumeric characters.");
            return code.ToString();
        }

        public static string Serialize(
            string type,
            string requestId,
            SignalingPayload payload)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Signaling message type is required.", nameof(type));
            string json = JsonUtility.ToJson(new SignalingEnvelope
            {
                type = type,
                requestId = requestId,
                payload = payload ?? new SignalingPayload()
            });
            if (Encoding.UTF8.GetByteCount(json) > MaximumMessageBytes)
                throw new ArgumentException("Signaling message is too large.", nameof(payload));
            return json;
        }

        public static SignalingEnvelope Deserialize(byte[] utf8, int length)
        {
            if (utf8 == null) throw new ArgumentNullException(nameof(utf8));
            if (length < 1 || length > MaximumMessageBytes || length > utf8.Length)
                throw new FormatException("Signaling message size is invalid.");
            string json;
            try
            {
                json = StrictUtf8.GetString(utf8, 0, length);
            }
            catch (DecoderFallbackException error)
            {
                throw new FormatException(
                    "Signaling message is not valid UTF-8.",
                    error);
            }
            SignalingEnvelope envelope;
            try
            {
                envelope = JsonUtility.FromJson<SignalingEnvelope>(json);
            }
            catch (ArgumentException error)
            {
                throw new FormatException("Signaling message is invalid JSON.", error);
            }
            if (envelope == null || string.IsNullOrEmpty(envelope.type))
                throw new FormatException("Signaling message type is missing.");
            envelope.payload = envelope.payload ?? new SignalingPayload();
            return envelope;
        }

        public static SignalingRoomInfo ReadRoomInfo(
            SignalingPayload payload,
            string fallbackRoomCode = null,
            bool requireHost = false)
        {
            if (payload == null)
                throw new RoomSignalingException(
                    "invalid_room_response",
                    "Room response must be an object.");
            string roomCode = NormalizeRoomCode(
                string.IsNullOrEmpty(payload.roomCode)
                    ? fallbackRoomCode
                    : payload.roomCode);
            string peerId = (payload.peerId ?? string.Empty).Trim();
            string hostId = (payload.hostId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(hostId) && !requireHost)
                hostId = peerId;
            if (!IsSafeIdentifier(peerId, 1, 48) ||
                !IsSafeIdentifier(hostId, 1, 48) ||
                !IsSafeIdentifier(
                    payload.resumeToken,
                    32,
                    128))
            {
                throw new RoomSignalingException(
                    "invalid_room_response",
                    "Room response is missing canonical identity.");
            }
            SignalingPeer[] peers = payload.peers ?? Array.Empty<SignalingPeer>();
            if (peers.Length > 14)
                throw new RoomSignalingException(
                    "invalid_room_response",
                    "Room response exceeds capacity.");
            return new SignalingRoomInfo(
                roomCode,
                peerId,
                hostId,
                payload.hostName,
                payload.mode,
                payload.maxPlayers,
                peers,
                payload.resumeToken);
        }

        public static void ValidateSignal(WebRtcSignal signal)
        {
            if (signal == null)
                throw new ArgumentNullException(nameof(signal));
            if (signal.kind == "restart") return;
            if (signal.kind == "description")
            {
                if (signal.description == null ||
                    (signal.description.type != "offer" &&
                     signal.description.type != "answer") ||
                    string.IsNullOrEmpty(signal.description.sdp) ||
                    signal.description.sdp.Length >
                    WebRtcPeerSession.MaximumSdpCharacters)
                {
                    throw new FormatException("WebRTC description signal is invalid.");
                }
                return;
            }
            if (signal.kind == "ice")
            {
                if (signal.candidate == null ||
                    string.IsNullOrEmpty(signal.candidate.candidate) ||
                    signal.candidate.candidate.Length >
                    WebRtcPeerSession.MaximumCandidateCharacters)
                {
                    throw new FormatException("WebRTC candidate signal is invalid.");
                }
                return;
            }
            throw new FormatException("WebRTC signal kind is invalid.");
        }

        private static bool IsSafeIdentifier(string value, int minimum, int maximum)
        {
            if (value == null || value.Length < minimum || value.Length > maximum)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((c < 'a' || c > 'z') &&
                    (c < 'A' || c > 'Z') &&
                    (c < '0' || c > '9') &&
                    c != '_' &&
                    c != '-')
                {
                    return false;
                }
            }
            return true;
        }

        private static string CollapseSpaces(string value)
        {
            string source = value ?? string.Empty;
            StringBuilder result = new StringBuilder(source.Length);
            bool previousSpace = false;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                bool space = char.IsWhiteSpace(c);
                if (!space || !previousSpace)
                    result.Append(space ? ' ' : c);
                previousSpace = space;
            }
            return result.ToString();
        }
    }
}
