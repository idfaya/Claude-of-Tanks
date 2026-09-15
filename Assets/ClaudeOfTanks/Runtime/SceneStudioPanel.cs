using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ClaudeOfTanks.Runtime.SceneStudioUiFactory;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class SceneStudioPanel :
        MonoBehaviour
    {
        private SceneStudioController _studio;
        private ContentCatalog _catalog;
        private Dropdown _actors;
        private Dropdown _vehicles;
        private Dropdown _maps;
        private Dropdown _camouflage;
        private readonly List<string> _camoIds =
            new List<string>();
        private Slider _x;
        private Slider _z;
        private Slider _hull;
        private Slider _turret;
        private Slider _gun;
        private Slider _distance;
        private Slider _height;
        private Slider _orbit;
        private Slider _fov;
        private Toggle _destroyed;
        private CustomCamouflagePanel _custom;
        private bool _refreshing;

        public static SceneStudioPanel Create(
            Transform parent,
            SceneStudioController studio,
            ContentCatalog catalog)
        {
            GameObject root = new GameObject(
                "SceneStudioUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            Canvas canvas =
                root.GetComponent<Canvas>();
            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;
            CanvasScaler scaler =
                root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1280f, 720f);
            SceneStudioPanel panel =
                root.AddComponent<
                    SceneStudioPanel>();
            panel._studio = studio;
            panel._catalog = catalog;
            panel.Build();
            panel.Refresh();
            return panel;
        }

        public void Refresh()
        {
            if (_actors == null) return;
            _refreshing = true;
            _actors.ClearOptions();
            List<string> actorNames =
                new List<string>();
            for (int i = 0;
                i < _studio.ActorCount;
                i++)
            {
                StudioActorSnapshot actor =
                    _studio.ActorAt(i);
                actorNames.Add(
                    (i + 1) + "  " +
                    actor.VehicleId);
            }
            _actors.AddOptions(actorNames);
            if (_studio.SelectedIndex >= 0)
            {
                _actors.SetValueWithoutNotify(
                    _studio.SelectedIndex);
                StudioActorSnapshot actor =
                    _studio.ActorAt(
                        _studio.SelectedIndex);
                _x.SetValueWithoutNotify(
                    actor.Position.X);
                _z.SetValueWithoutNotify(
                    actor.Position.Z);
                _hull.SetValueWithoutNotify(
                    actor.HullYawDeg);
                _turret.SetValueWithoutNotify(
                    actor.TurretYawDeg);
                _gun.SetValueWithoutNotify(
                    actor.GunPitchDeg);
                _destroyed
                    .SetIsOnWithoutNotify(
                        actor.Destroyed);
                int camo =
                    _camoIds.IndexOf(
                        actor.CamouflageId);
                _camouflage
                    .SetValueWithoutNotify(
                        Mathf.Max(0, camo));
            }
            _refreshing = false;
        }

        private void Build()
        {
            Font font =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");
            Image rail = Image(
                "StudioRail",
                transform,
                new Color(
                    0.025f,
                    0.032f,
                    0.034f,
                    0.96f));
            RectTransform rect =
                rail.rectTransform;
            rect.anchorMin =
                new Vector2(1f, 0f);
            rect.anchorMax =
                new Vector2(1f, 1f);
            rect.pivot =
                new Vector2(1f, 0.5f);
            rect.sizeDelta =
                new Vector2(390f, 0f);
            rect.anchoredPosition =
                Vector2.zero;

            Text title = Label(
                "Title",
                rail.transform,
                font,
                "SCENE STUDIO",
                22);
            Place(title.rectTransform, 20f, 370f, -52f, -14f);
            _actors = Dropdown(
                "Actors",
                rail.transform,
                font);
            PlaceControl(_actors.transform, -96f);
            _actors.onValueChanged.AddListener(
                index =>
                {
                    if (_refreshing) return;
                    _studio.SelectActor(index);
                });

            _vehicles = Dropdown(
                "Vehicles",
                rail.transform,
                font);
            List<string> vehicleNames =
                new List<string>();
            for (int i = 0;
                i < _catalog
                    .ProductionVehicleIds.Length;
                i++)
            {
                vehicleNames.Add(
                    _catalog.GetVehicle(
                        _catalog
                            .ProductionVehicleIds[i])
                        .name);
            }
            _vehicles.AddOptions(vehicleNames);
            PlaceControl(
                _vehicles.transform,
                -140f);
            Button add = Button(
                "AddActor",
                rail.transform,
                font,
                "ADD");
            Place(
                add.GetComponent<RectTransform>(),
                20f,
                184f,
                -190f,
                -150f);
            add.onClick.AddListener(
                () => _studio.AddActor(
                    _catalog
                        .ProductionVehicleIds[
                            _vehicles.value]));
            Button remove = Button(
                "RemoveActor",
                rail.transform,
                font,
                "REMOVE");
            Place(
                remove.GetComponent<
                    RectTransform>(),
                204f,
                370f,
                -190f,
                -150f);
            remove.onClick.AddListener(
                () => _studio
                    .RemoveSelectedActor());

            _maps = Dropdown(
                "Maps",
                rail.transform,
                font);
            List<string> mapNames =
                new List<string>();
            int selectedMap = 0;
            for (int i = 0;
                i < _catalog.Maps.Length;
                i++)
            {
                mapNames.Add(
                    _catalog.Maps[i].name);
                if (_catalog.Maps[i].id ==
                    _studio.MapId)
                {
                    selectedMap = i;
                }
            }
            _maps.AddOptions(mapNames);
            _maps.SetValueWithoutNotify(
                selectedMap);
            PlaceControl(_maps.transform, -236f);
            _maps.onValueChanged.AddListener(
                index =>
                {
                    if (!_refreshing)
                        _studio.SetMap(
                            _catalog.Maps[index].id);
                });

            _camouflage = Dropdown(
                "Camouflage",
                rail.transform,
                font);
            List<string> camoNames =
                new List<string>();
            for (int i = 0;
                i < _catalog.Camouflage.Length;
                i++)
            {
                _camoIds.Add(
                    _catalog.Camouflage[i].id);
                camoNames.Add(
                    _catalog.Camouflage[i].name);
            }
            _camoIds.Add("custom");
            camoNames.Add("Custom");
            _camouflage.AddOptions(camoNames);
            PlaceControl(
                _camouflage.transform,
                -280f);
            _camouflage.onValueChanged
                .AddListener(
                    index =>
                    {
                        if (!_refreshing)
                            _studio
                                .SetSelectedCamouflage(
                                    _camoIds[index]);
                    });

            _x = Slider(
                rail.transform,
                font,
                "X",
                -326f,
                -300f,
                300f);
            _z = Slider(
                rail.transform,
                font,
                "Z",
                -370f,
                -300f,
                300f);
            _hull = Slider(
                rail.transform,
                font,
                "HULL",
                -414f,
                -180f,
                180f);
            _turret = Slider(
                rail.transform,
                font,
                "TURRET",
                -458f,
                -180f,
                180f);
            _gun = Slider(
                rail.transform,
                font,
                "GUN",
                -502f,
                -20f,
                30f);
            _x.onValueChanged.AddListener(
                ignored => ApplyPose());
            _z.onValueChanged.AddListener(
                ignored => ApplyPose());
            _hull.onValueChanged.AddListener(
                ignored => ApplyPose());
            _turret.onValueChanged.AddListener(
                ignored => ApplyPose());
            _gun.onValueChanged.AddListener(
                ignored => ApplyPose());

            _destroyed = Toggle(
                "Destroyed",
                rail.transform,
                font,
                "DESTROYED");
            Place(
                _destroyed.GetComponent<
                    RectTransform>(),
                20f,
                184f,
                -548f,
                -516f);
            _destroyed.onValueChanged
                .AddListener(
                    value =>
                    {
                        if (!_refreshing)
                            _studio
                                .SetSelectedDestroyed(
                                    value);
                    });
            Button custom = Button(
                "CustomCamo",
                rail.transform,
                font,
                "EDIT CAMO");
            Place(
                custom.GetComponent<
                    RectTransform>(),
                204f,
                370f,
                -554f,
                -514f);
            _custom =
                CustomCamouflagePanel.Create(
                    transform);
            custom.onClick.AddListener(
                () => _custom.Open(
                    () =>
                    {
                        _studio
                            .SetSelectedCamouflage(
                                "custom");
                        Refresh();
                    }));

            _distance = Slider(
                rail.transform,
                font,
                "DIST",
                -570f,
                8f,
                60f);
            _height = Slider(
                rail.transform,
                font,
                "HEIGHT",
                -608f,
                2f,
                35f);
            _orbit = Slider(
                rail.transform,
                font,
                "ORBIT",
                -646f,
                -180f,
                180f);
            _fov = Slider(
                rail.transform,
                font,
                "FOV",
                -684f,
                15f,
                80f);
            _distance.value = 22f;
            _height.value = 8f;
            _orbit.value = 35f;
            _fov.value = 42f;
            _distance.onValueChanged
                .AddListener(
                    ignored => ApplyCamera());
            _height.onValueChanged
                .AddListener(
                    ignored => ApplyCamera());
            _orbit.onValueChanged
                .AddListener(
                    ignored => ApplyCamera());
            _fov.onValueChanged
                .AddListener(
                    ignored => ApplyCamera());

            Button capture = Button(
                "Capture",
                rail.transform,
                font,
                "CAPTURE");
            Place(
                capture.GetComponent<
                    RectTransform>(),
                208f,
                284f,
                -52f,
                -14f);
            capture.onClick.AddListener(
                () => Debug.Log(
                    "Studio capture: " +
                    _studio.Capture()));
            Button exit = Button(
                "Exit",
                rail.transform,
                font,
                "GARAGE");
            Place(
                exit.GetComponent<
                    RectTransform>(),
                292f,
                370f,
                -52f,
                -14f);
            exit.onClick.AddListener(
                _studio.Exit);
        }

        private void ApplyPose()
        {
            if (_refreshing) return;
            _studio.SetSelectedPose(
                _x.value,
                _z.value,
                _hull.value,
                _turret.value,
                _gun.value);
        }

        private void ApplyCamera()
        {
            if (_refreshing) return;
            _studio.SetCamera(
                _distance.value,
                _height.value,
                _orbit.value,
                _fov.value);
        }

    }
}
