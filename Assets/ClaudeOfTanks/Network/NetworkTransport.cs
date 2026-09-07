using System;

namespace ClaudeOfTanks.Network
{
    public interface INetworkTransportEndpoint : IDisposable
    {
        bool IsOpen { get; }
        event Action<byte[]> ControlReceived;
        event Action<byte[]> StateReceived;
        event Action<string> Closed;
        bool SendControl(byte[] packet);
        bool SendState(byte[] packet);
        int Pump(int maximumControlMessages = int.MaxValue);
        void Close(string reason = "closed");
    }
}
