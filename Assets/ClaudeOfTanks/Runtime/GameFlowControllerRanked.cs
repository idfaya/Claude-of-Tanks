using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class GameFlowController
    {
        private void StartRankedBattle()
        {
            if (_networkBattle != null) return;
            DestroyGarage();
            GameObject battleObject = new GameObject(
                "RankedBattle");
            battleObject.transform.SetParent(transform, false);
            battleObject.SetActive(false);
            _networkBattle =
                battleObject.AddComponent<NetworkBattleController>();
            try
            {
                _networkBattle.ConfigureRanked(
                    _ranked.TakeHandoff(),
                    FinishRankedBattle);
                battleObject.SetActive(true);
            }
            catch (Exception error)
            {
                Debug.LogError(
                    "Ranked battle failed: " + error.Message);
                _ranked.ReturnFromBattle();
                _networkBattle = null;
                ReleaseObject(battleObject);
                ShowGarage();
                _rankedPanel.Open();
            }
        }

        private void FinishRankedBattle()
        {
            GameObject battleObject =
                _networkBattle != null
                    ? _networkBattle.gameObject
                    : null;
            _networkBattle = null;
            _ranked.ReturnFromBattle();
            ReleaseObject(battleObject);
            ShowGarage();
            _rankedPanel.Open();
        }
    }
}
