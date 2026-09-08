using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        [Test]
        public void GarageBuildsProductionWorkshopAndTacticalPresentation()
        {
            GameObject root = new GameObject("GaragePresentationTest");
            GameFlowController flow =
                root.AddComponent<GameFlowController>();
            flow.Initialize();

            try
            {
                Transform stage = root.transform.Find(
                    "Garage/WorkshopStage");
                Assert.That(stage, Is.Not.Null);
                Assert.That(
                    stage.Find("Turntable/TurntableBase"),
                    Is.Not.Null);
                Assert.That(
                    stage.Find("Turntable/TurntableDeck"),
                    Is.Not.Null);
                Assert.That(
                    stage.Find("WorkshopShell/RearWall"),
                    Is.Not.Null);
                Assert.That(
                    stage.Find("WorkshopShell/Ceiling"),
                    Is.Not.Null);
                Assert.That(
                    stage.Find("RoofTrusses").childCount,
                    Is.GreaterThanOrEqualTo(8));

                Transform highBayLights =
                    stage.Find("HighBayLights");
                Assert.That(highBayLights, Is.Not.Null);
                Assert.That(
                    highBayLights.GetComponentsInChildren<Light>(),
                    Has.Length.EqualTo(3));

                Transform ui = root.transform.Find(
                    "Garage/GarageUI");
                Assert.That(ui, Is.Not.Null);
                Assert.That(ui.Find("TopBrandRail"), Is.Not.Null);
                Assert.That(ui.Find("SelectionRail"), Is.Not.Null);
                Assert.That(ui.Find("VehicleStatus"), Is.Not.Null);
                Assert.That(ui.Find("CommandBand"), Is.Not.Null);

                CanvasScaler scaler =
                    ui.GetComponent<CanvasScaler>();
                Assert.That(
                    scaler.referenceResolution,
                    Is.EqualTo(new Vector2(1280f, 720f)));

                Button deploy =
                    ui.Find("CommandBand/Deploy")
                        .GetComponent<Button>();
                Assert.That(deploy, Is.Not.Null);
                Color deployColor =
                    deploy.colors.normalColor;
                Assert.That(
                    deployColor.r,
                    Is.GreaterThan(deployColor.b + 0.35f));
                Button[] buttons =
                    ui.GetComponentsInChildren<Button>(true);
                Assert.That(buttons.Length, Is.GreaterThan(12));
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i].name == "Rail" &&
                        buttons[i].transition ==
                            Selectable.Transition.None)
                    {
                        continue;
                    }
                    Assert.That(
                        buttons[i]
                            .GetComponent<UiAudioButtonFeedback>(),
                        Is.Not.Null,
                        buttons[i].name);
                }

                flow.Select(
                    2,
                    3,
                    GameModeId.ZoneControl);
                Text vehicleTitle =
                    ui.Find("VehicleStatus/VehicleName")
                        .GetComponent<Text>();
                Assert.That(
                    vehicleTitle.text,
                    Is.EqualTo("M1A2 ABRAMS"));
                Text battleContext =
                    ui.Find("VehicleStatus/BattleContext")
                        .GetComponent<Text>();
                Assert.That(
                    battleContext.text,
                    Does.Contain(
                        flow.SelectedMapId.ToUpperInvariant()));
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
