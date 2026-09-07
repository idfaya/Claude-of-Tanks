using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;

namespace ClaudeOfTanks.Server
{
    public sealed class DedicatedServerMatchFactory
    {
        private readonly ContentCatalog _catalog;
        private readonly HashSet<string> _productionVehicles;
        private readonly string[] _mapRotation;

        public DedicatedServerMatchFactory(ContentCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _productionVehicles = new HashSet<string>(
                catalog.ProductionVehicleIds,
                StringComparer.Ordinal);
            MapDefinition[] maps = catalog.Maps;
            _mapRotation = new string[maps.Length];
            for (int i = 0; i < maps.Length; i++) _mapRotation[i] = maps[i].id;
            if (_mapRotation.Length == 0)
                throw new InvalidOperationException("Ranked map rotation is empty.");
        }

        public string[] MapRotation => (string[])_mapRotation.Clone();

        public bool IsVehicleAllowed(string id)
        {
            return !string.IsNullOrEmpty(id) && _productionVehicles.Contains(id);
        }

        public AuthoritativeMatchHost Create(RoomMatchPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            AuthoritativeMatchHost host = new AuthoritativeMatchHost(
                RoomMatchBattleFactory.Create(_catalog, plan));
            for (int i = 0; i < plan.Seats.Length; i++)
                host.RegisterPlayer(plan.Seats[i].PlayerId, plan.Seats[i].EntityId);
            string[] spectators =
                plan.SpectatorPlayerIds ?? Array.Empty<string>();
            for (int i = 0; i < spectators.Length; i++)
                host.RegisterSpectator(spectators[i]);
            return host;
        }
    }
}
