using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeOfTanks.Network
{
    internal sealed class DedicatedWebSocketSession
    {
        public DedicatedWebSocketSession(
            DedicatedMatchAdmission admission,
            WebSocketNetworkEndpoint transport,
            AuthoritativeHostPump pump)
        {
            Admission = admission;
            Transport = transport;
            Pump = pump;
        }

        public DedicatedMatchAdmission Admission { get; }
        public WebSocketNetworkEndpoint Transport { get; }
        public AuthoritativeHostPump Pump { get; }
        public bool Closed { get; set; }

        public void Dispose(DedicatedMatchRegistry registry)
        {
            try
            {
                registry.Disconnect(
                    Admission.MatchId,
                    Admission.PlayerId,
                    Admission.ConnectionGeneration);
            }
            catch (KeyNotFoundException)
            {
            }
            Pump.Dispose();
            Transport.Dispose();
        }
    }

    internal sealed class DedicatedServiceMatch
    {
        public DedicatedServiceMatch(DedicatedMatchRecord record)
        {
            Record = record;
        }

        public DedicatedMatchRecord Record { get; }
        public bool Started { get; set; }
    }

    internal sealed class AcceptedWebSocketConnection : IWebSocketConnection
    {
        private readonly WebSocket _socket;
        private readonly TcpClient _client;

        public AcceptedWebSocketConnection(WebSocket socket, TcpClient client)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public WebSocketState State => _socket.State;

        public Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Accepted WebSockets are already connected.");
        }

        public Task SendAsync(
            ArraySegment<byte> payload,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            return _socket.SendAsync(payload, messageType, endOfMessage, cancellationToken);
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> payload,
            CancellationToken cancellationToken)
        {
            return _socket.ReceiveAsync(payload, cancellationToken);
        }

        public Task CloseAsync(
            WebSocketCloseStatus status,
            string reason,
            CancellationToken cancellationToken)
        {
            return _socket.CloseAsync(status, reason, cancellationToken);
        }

        public void Dispose()
        {
            _socket.Dispose();
            _client.Dispose();
        }
    }
}
