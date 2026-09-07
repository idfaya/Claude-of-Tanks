using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClaudeOfTanks.Tests
{
    public sealed class GameFlowTests
    {
        [Test]
        public void GarageSelectionDeploysConfiguredBattleAndReturnsCleanly()
        {
            GameObject root = new GameObject("GameFlowTest");
            GameFlowController flow = root.AddComponent<GameFlowController>();
            flow.Initialize();

            try
            {
                Assert.That(flow.IsGarageVisible, Is.True);
                Assert.That(flow.VehicleOptionCount, Is.EqualTo(126));
                Assert.That(flow.MapOptionCount, Is.EqualTo(20));
                Assert.That(Object.FindObjectsOfType<EventSystem>(), Has.Length.EqualTo(1));
                Assert.That(flow.PrivateRoom, Is.Not.Null);
                Assert.That(flow.PrivateRoomPanel, Is.Not.Null);
                Assert.That(flow.LoadoutPanel, Is.Not.Null);
                Assert.That(
                    flow.LoadoutPanel.EquipmentToggleCount,
                    Is.EqualTo(14));
                flow.PrivateRoomPanel.Open();
                Assert.That(flow.PrivateRoomPanel.IsVisible, Is.True);
                flow.PrivateRoomPanel.Close();

                flow.Select(125, 19, GameModeId.EndlessHorde);
                string vehicleId = flow.SelectedVehicleId;
                string mapId = flow.SelectedMapId;
                Assert.That(
                    flow.LoadoutPanel.SetEquipment(
                        "toolbox",
                        true),
                    Is.True);
                Assert.That(
                    flow.LoadoutPanel.SetCamouflage(
                        "winter"),
                    Is.True);

                flow.DeploySelected();

                Assert.That(flow.IsGarageVisible, Is.False);
                Assert.That(flow.ActiveBattle, Is.Not.Null);
                Assert.That(flow.ActiveBattle.VehicleId, Is.EqualTo(vehicleId));
                Assert.That(flow.ActiveBattle.MapId, Is.EqualTo(mapId));
                Assert.That(flow.ActiveBattle.GameMode, Is.EqualTo(GameModeId.EndlessHorde));
                Assert.That(flow.ActiveBattle.Player.Spec.Id, Is.EqualTo(vehicleId));
                Assert.That(
                    flow.ActiveBattle.Player.Combat.Equipment.RepairRate,
                    Is.EqualTo(1.25f));
                Assert.That(Object.FindObjectsOfType<EventSystem>(), Has.Length.EqualTo(1));

                flow.ReturnToGarage();

                Assert.That(flow.IsGarageVisible, Is.True);
                Assert.That(flow.ActiveBattle, Is.Null);
                Assert.That(Object.FindObjectsOfType<EventSystem>(), Has.Length.EqualTo(1));
            }
            finally
            {
                if (flow.Loadout != null &&
                    flow.Loadout.VehicleId != null)
                {
                    flow.Loadout.Reset();
                }
                Object.DestroyImmediate(root);
            }
        }
    }
}
