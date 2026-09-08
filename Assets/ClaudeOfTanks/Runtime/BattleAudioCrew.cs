using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class BattleAudio
    {
        private BattleCrewVoice _crewVoice;
        private BattleCrewVoiceDirector _crewDirector;

        public BattleCrewVoice CrewVoice => _crewVoice;

        public void PresentResult(string result)
        {
            _crewDirector.PresentResult(result, Time.unscaledTime);
        }

        public void SyncAwareness(
            IList<TankState> tanks,
            string listenerOwnerId,
            Func<Float3, Float3, bool> isOccluded = null,
            bool? viewerSpotted = null)
        {
            _crewDirector.SyncAwareness(
                tanks,
                listenerOwnerId,
                isOccluded,
                viewerSpotted,
                Time.unscaledTime);
        }

        private void InitializeCrewVoice()
        {
            _crewVoice = BattleCrewVoice.Create(transform, _settings);
            _crewDirector = new BattleCrewVoiceDirector(_crewVoice);
        }

        private void BeginCrewVoice(bool announce)
        {
            if (announce)
                _crewDirector.BeginBattle(Time.unscaledTime);
        }

        private void PlayCrewVoice(
            BattleEvent battleEvent,
            string listenerOwnerId,
            TankState occupiedTank)
        {
            _crewDirector.Handle(
                battleEvent,
                listenerOwnerId,
                occupiedTank,
                Time.unscaledTime);
        }

        private void SyncCrewVoice(TankState occupiedTank)
        {
            _crewDirector.Sync(occupiedTank, Time.unscaledTime);
        }

        private void ResetCrewVoice()
        {
            _crewDirector.ResetAll();
        }
    }
}
