using System;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class NetworkBattleController
    {
        public void ConfigureRanked(
            RankedMatchHandoff handoff,
            Action returned)
        {
            if (_initialized) throw new InvalidOperationException(
                "Network battle is already configured.");
            if (handoff == null) throw new ArgumentNullException(
                nameof(handoff));
            _pendingHandoff = handoff;
            _catalog = ContentCatalog.Load();
            _plan = handoff.Plan;
            _localPlayerId = handoff.PlayerId;
            _localEntityId = handoff.EntityId;
            IsSpectator = false;
            _predictor = CreatePredictor(
                FindSeat(_plan, _localPlayerId));
            _client = handoff.CreateMatchRuntime(_predictor);
            _pendingHandoff = null;
            _rankedReturned = returned ??
                throw new ArgumentNullException(nameof(returned));
            InitializePresentation();
        }
    }
}
