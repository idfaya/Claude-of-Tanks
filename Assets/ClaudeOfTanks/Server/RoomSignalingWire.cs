using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ClaudeOfTanks.Server
{
    internal static class RoomSignalingProtocolLimits
    {
        public const int MaximumMessageBytes = 128 * 1024;
    }

    [Serializable]
    internal sealed class RoomSignalingEnvelope
    {
        public string type;
        public string requestId;
        public RoomSignalingWirePayload payload;
    }

    [Serializable]
    internal sealed class RoomSignalingWirePayload
    {
        public string roomCode;
        public string peerId;
        public string hostId;
        public string hostName;
        public string mode;
        public int maxPlayers;
        public RoomSignalingWirePeer[] peers;
        public RoomSignalingWirePlayer player;
        public string sessionId;
        public string toPeerId;
        public string toSessionId;
        public string fromPeerId;
        public string fromSessionId;
        public string reason;
        public string code;
        public string message;
        public RoomSignalingWireSignal signal;
    }

    [Serializable]
    internal sealed class RoomSignalingWirePeer
    {
        public string peerId;
        public RoomSignalingWirePlayer player;
        public string sessionId;
        public bool isHost;
    }

    [Serializable]
    internal sealed class RoomSignalingWirePlayer
    {
        public string id;
        public string name;
    }

    [Serializable]
    internal sealed class RoomSignalingWireSignal
    {
        public string kind;
        public RoomSignalingWireDescription description;
        public RoomSignalingWireCandidate candidate;
    }

    [Serializable]
    internal sealed class RoomSignalingWireDescription
    {
        public string type;
        public string sdp;
    }

    [Serializable]
    internal sealed class RoomSignalingWireCandidate
    {
        public string candidate;
        public string sdpMid;
        public int sdpMLineIndex;
    }

    internal sealed class RoomSignalingConnection : IDisposable
    {
        private readonly SemaphoreSlim _sendGate =
            new SemaphoreSlim(1, 1);

        public RoomSignalingConnection(
            WebSocket socket,
            TcpClient client)
        {
            Socket = socket;
            Client = client;
        }

        public WebSocket Socket { get; }
        public TcpClient Client { get; }
        public RoomSignalingStore.Room Room;
        public RoomSignalingStore.Peer Peer;

        public async Task SendAsync(
            RoomSignalingEnvelope envelope,
            CancellationToken cancellationToken)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(
                JsonUtility.ToJson(envelope));
            await _sendGate.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            try
            {
                if (Socket.State != WebSocketState.Open) return;
                await Socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _sendGate.Release();
            }
        }

        public void Dispose()
        {
            try { Socket.Dispose(); }
            catch { }
            try { Client.Dispose(); }
            catch { }
            _sendGate.Dispose();
        }
    }
}
