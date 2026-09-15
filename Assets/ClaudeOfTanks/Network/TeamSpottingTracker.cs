using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Network
{
    internal sealed class TeamSpottingTracker
    {
        private readonly BattleState _battle;
        private readonly SpottingSimulation _spotting;
        private readonly Func<Float3, Float3, bool>
            _isOccluded;
        private readonly Dictionary<string, SpotIntel>
            _alpha =
                new Dictionary<string, SpotIntel>(
                    StringComparer.Ordinal);
        private readonly Dictionary<string, SpotIntel>
            _bravo =
                new Dictionary<string, SpotIntel>(
                    StringComparer.Ordinal);
        private readonly Dictionary<string, float>
            _nextCheckAt =
                new Dictionary<string, float>(
                    StringComparer.Ordinal);

        public TeamSpottingTracker(
            BattleState battle,
            SpottingSimulation spotting,
            Func<Float3, Float3, bool>
                isOccluded)
        {
            _battle = battle ??
                throw new ArgumentNullException(
                    nameof(battle));
            _spotting = spotting ??
                throw new ArgumentNullException(
                    nameof(spotting));
            _isOccluded = isOccluded;
        }

        public void Update()
        {
            ForceFiredChecks();
            List<TankState> tanks =
                _battle.Tanks;
            for (int targetIndex = 0;
                targetIndex < tanks.Count;
                targetIndex++)
            {
                TankState target =
                    tanks[targetIndex];
                if (target.Destroyed)
                    continue;
                float due;
                if (_nextCheckAt.TryGetValue(
                        target.Id,
                        out due) &&
                    _battle.TimeS + 0.000001f <
                        due)
                {
                    continue;
                }
                CheckTarget(target, tanks);
            }
        }

        public bool IsVisibleFor(
            TankState viewer,
            TankState target)
        {
            if (viewer == null ||
                target == null)
            {
                return true;
            }
            if (viewer.Team == target.Team ||
                viewer.Id == target.Id)
            {
                return true;
            }
            SpotIntel intel;
            if (!IntelFor(viewer.Team)
                    .TryGetValue(
                        target.Id,
                        out intel) ||
                intel.ExpiresAtS +
                    0.000001f <
                    _battle.TimeS)
            {
                return false;
            }
            if (intel.DirectSpotters
                    .Contains(viewer.Id))
            {
                return true;
            }
            float dx =
                viewer.Position.X -
                intel.SpotterPosition.X;
            float dz =
                viewer.Position.Z -
                intel.SpotterPosition.Z;
            return dx * dx + dz * dz <=
                intel.ShareRangeM *
                intel.ShareRangeM;
        }

        public bool IsSixthSenseVisible(
            TankState viewer)
        {
            if (viewer == null ||
                viewer.Destroyed)
            {
                return false;
            }
            Team enemy =
                viewer.Team == Team.Alpha
                    ? Team.Bravo
                    : Team.Alpha;
            SpotIntel intel;
            if (!IntelFor(enemy)
                    .TryGetValue(
                        viewer.Id,
                        out intel))
            {
                return false;
            }
            float shownAt =
                intel.SpottedAtS +
                SpottingSimulation
                    .SixthSenseDelaySeconds;
            return _battle.TimeS >= shownAt &&
                _battle.TimeS <=
                    shownAt +
                    SpottingSimulation
                        .SixthSenseShowSeconds;
        }

        private void ForceFiredChecks()
        {
            List<BattleEvent> events =
                _battle.Events;
            for (int i = 0;
                i < events.Count;
                i++)
            {
                if (events[i].Type ==
                    BattleEventType.ShellFired)
                {
                    _nextCheckAt[
                        events[i].SourceId] =
                        _battle.TimeS;
                }
            }
        }

        private void CheckTarget(
            TankState target,
            List<TankState> tanks)
        {
            float nearest =
                float.MaxValue;
            SpotIntel alphaPass = null;
            SpotIntel bravoPass = null;
            for (int i = 0;
                i < tanks.Count;
                i++)
            {
                TankState spotter = tanks[i];
                if (spotter == target ||
                    spotter.Destroyed ||
                    spotter.Team ==
                        target.Team)
                {
                    continue;
                }
                float dx =
                    spotter.Position.X -
                    target.Position.X;
                float dz =
                    spotter.Position.Z -
                    target.Position.Z;
                float distance =
                    MathF.Sqrt(
                        dx * dx +
                        dz * dz);
                if (distance < nearest)
                    nearest = distance;
                bool fired =
                    SpottingSimulation
                        .FireBloomAt(
                            target,
                            _battle.TimeS) > 0f;
                float bush =
                    _battle
                        .ConcealmentBonusBetween(
                            spotter.Position,
                            target.Position,
                            fired);
                if (!_spotting.CanSpot(
                        spotter,
                        target,
                        _isOccluded,
                        _battle.TimeS,
                        bush))
                {
                    continue;
                }
                SpotIntel pass =
                    spotter.Team == Team.Alpha
                        ? alphaPass
                        : bravoPass;
                if (pass == null)
                {
                    pass = new SpotIntel();
                    if (spotter.Team ==
                        Team.Alpha)
                    {
                        alphaPass = pass;
                    }
                    else
                    {
                        bravoPass = pass;
                    }
                }
                pass.DirectSpotters.Add(
                    spotter.Id);
                float range =
                    SpottingSimulation
                        .SignalRangeFor(spotter);
                if (range >
                    pass.ShareRangeM)
                {
                    pass.ShareRangeM = range;
                    pass.SpotterPosition =
                        spotter.Position;
                }
            }
            CommitPass(
                Team.Alpha,
                target.Id,
                alphaPass);
            CommitPass(
                Team.Bravo,
                target.Id,
                bravoPass);
            _nextCheckAt[target.Id] =
                _battle.TimeS +
                SpottingSimulation
                    .CheckIntervalSeconds(
                        nearest);
        }

        private void CommitPass(
            Team team,
            string targetId,
            SpotIntel pass)
        {
            if (pass == null) return;
            Dictionary<string, SpotIntel> intel =
                IntelFor(team);
            SpotIntel current;
            if (!intel.TryGetValue(
                    targetId,
                    out current))
            {
                current = new SpotIntel
                {
                    SpottedAtS =
                        _battle.TimeS
                };
                intel[targetId] = current;
            }
            else if (current.ExpiresAtS <
                _battle.TimeS)
            {
                current.SpottedAtS =
                    _battle.TimeS;
            }
            current.ExpiresAtS =
                _battle.TimeS +
                SpottingSimulation
                    .SpotLingerSeconds;
            current.ShareRangeM =
                pass.ShareRangeM;
            current.SpotterPosition =
                pass.SpotterPosition;
            current.DirectSpotters.Clear();
            foreach (string id in
                pass.DirectSpotters)
            {
                current.DirectSpotters.Add(id);
            }
        }

        private Dictionary<string, SpotIntel>
            IntelFor(Team team)
        {
            return team == Team.Alpha
                ? _alpha
                : _bravo;
        }

        private sealed class SpotIntel
        {
            public float ExpiresAtS;
            public float SpottedAtS;
            public float ShareRangeM;
            public Float3 SpotterPosition;
            public readonly HashSet<string>
                DirectSpotters =
                    new HashSet<string>(
                        StringComparer.Ordinal);
        }
    }
}
