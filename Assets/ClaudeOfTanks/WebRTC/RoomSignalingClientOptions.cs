using System;
using ClaudeOfTanks.Network;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class RoomSignalingClientOptions
    {
        public int ConnectTimeoutMs = 5000;
        public int RequestTimeoutMs = 30000;
        public int EventPollIntervalMs = 500;
        public int EventPollTimeoutMs = 10000;
        public int[] ReconnectDelaysMs =
            { 250, 500, 1000, 2000, 4000, 8000, 15000, 30000 };
        public string SessionId;
        public string Origin;
        public Func<IWebSocketConnection> ConnectionFactory;
    }
}
