using System;

namespace ClaudeOfTanks.Network
{
    public interface INetworkBattleClientRuntime : IDisposable
    {
        NetworkWorldSnapshot LatestSnapshot { get; }
        SnapshotBuffer Buffer { get; }
        bool IsConnected { get; }
        bool SendInput(NetworkInputCommand command);
        int Pump();
    }
}
