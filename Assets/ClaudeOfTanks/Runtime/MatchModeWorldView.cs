using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class MatchModeWorldView : IDisposable
    {
        private const int ZoneCount = 3;
        private readonly Transform _root;
        private readonly Transform _alphaFlag;
        private readonly Transform _bravoFlag;
        private readonly Transform[] _zones =
            new Transform[ZoneCount];
        private readonly Transform _ball;
        private readonly Dictionary<string, Transform> _pickups =
            new Dictionary<string, Transform>(StringComparer.Ordinal);
        private readonly List<string> _stalePickups =
            new List<string>();
        private readonly Material _alpha;
        private readonly Material _bravo;
        private readonly Material _neutral;
        private readonly Material _ballMaterial;
        private readonly Material _heal;
        private readonly Material _ammo;

        private MatchModeWorldView(Transform root)
        {
            _root = root;
            _alpha = Material(new Color(0.16f, 0.85f, 0.32f));
            _bravo = Material(new Color(0.95f, 0.2f, 0.16f));
            _neutral = Material(new Color(0.92f, 0.78f, 0.25f));
            _ballMaterial = Material(new Color(1f, 0.68f, 0.12f));
            _heal = Material(new Color(0.2f, 0.9f, 0.5f));
            _ammo = Material(new Color(0.95f, 0.88f, 0.35f));
            _alphaFlag = CreateFlag("AlphaFlag", _alpha);
            _bravoFlag = CreateFlag("BravoFlag", _bravo);
            for (int i = 0; i < ZoneCount; i++)
            {
                _zones[i] = CreateZone("Zone-" + i);
            }
            _ball = CreateBall();
            HideAll();
        }

        public Transform Root => _root;

        public static MatchModeWorldView Create(Transform parent)
        {
            GameObject root = new GameObject("MatchModeWorldView");
            root.transform.SetParent(parent, false);
            return new MatchModeWorldView(root.transform);
        }

        public void Sync(MatchModeState mode)
        {
            HideAll();
            if (mode == null ||
                mode.Id == GameModeId.Standard)
            {
                SyncPickups(mode);
                return;
            }
            if (mode.Id == GameModeId.CaptureTheFlag)
            {
                Place(_alphaFlag, mode.AlphaFlag, 1.2f);
                Place(_bravoFlag, mode.BravoFlag, 1.2f);
            }
            else if (mode.Id == GameModeId.ZoneControl)
            {
                for (int i = 0; i < ZoneCount; i++)
                {
                    Renderer renderer =
                        _zones[i].GetComponent<Renderer>();
                    renderer.sharedMaterial =
                        mode.ZoneOwners[i] == Team.Alpha
                            ? _alpha
                            : mode.ZoneOwners[i] == Team.Bravo
                                ? _bravo
                                : _neutral;
                    float height =
                        0.05f +
                        Mathf.Abs(mode.ZoneControl[i]) *
                        0.08f;
                    _zones[i].localScale =
                        new Vector3(30f, height, 30f);
                    Place(_zones[i], mode.Zones[i], 0.03f);
                }
            }
            else if (mode.Id == GameModeId.TurboBall)
            {
                Place(_ball, mode.BallPosition, 0f);
            }
            SyncPickups(mode);
        }

        public void Dispose()
        {
            Release(_root != null ? _root.gameObject : null);
            Release(_alpha);
            Release(_bravo);
            Release(_neutral);
            Release(_ballMaterial);
            Release(_heal);
            Release(_ammo);
        }

        private void HideAll()
        {
            SetActive(_alphaFlag, false);
            SetActive(_bravoFlag, false);
            for (int i = 0; i < _zones.Length; i++)
                SetActive(_zones[i], false);
            SetActive(_ball, false);
        }

        private void SyncPickups(MatchModeState mode)
        {
            _stalePickups.Clear();
            foreach (string id in _pickups.Keys)
                _stalePickups.Add(id);
            if (mode != null && mode.Pickups != null)
            {
                for (int i = 0; i < mode.Pickups.Count; i++)
                {
                    ModePickup pickup = mode.Pickups[i];
                    if (pickup == null ||
                        !pickup.Active ||
                        string.IsNullOrEmpty(pickup.Id))
                    {
                        continue;
                    }
                    Transform view = EnsurePickup(pickup);
                    view.GetComponent<Renderer>().sharedMaterial =
                        pickup.Kind == "ammo" ? _ammo : _heal;
                    Place(view, pickup.Position, 0.4f);
                    _stalePickups.Remove(pickup.Id);
                }
            }
            for (int i = 0; i < _stalePickups.Count; i++)
            {
                string id = _stalePickups[i];
                Release(_pickups[id].gameObject);
                _pickups.Remove(id);
            }
        }

        private Transform EnsurePickup(ModePickup pickup)
        {
            Transform view;
            if (_pickups.TryGetValue(pickup.Id, out view))
                return view;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Pickup-" + pickup.Id;
            obj.transform.SetParent(_root, false);
            obj.transform.localScale = new Vector3(1.6f, 0.8f, 1.6f);
            StripCollider(obj);
            _pickups.Add(pickup.Id, obj.transform);
            return obj.transform;
        }

        private Transform CreateFlag(string name, Material material)
        {
            GameObject flag = new GameObject(name);
            flag.transform.SetParent(_root, false);
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(flag.transform, false);
            pole.transform.localScale = new Vector3(0.08f, 1.6f, 0.08f);
            pole.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            StripCollider(pole);
            pole.GetComponent<Renderer>().sharedMaterial = material;
            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = "Cloth";
            cloth.transform.SetParent(flag.transform, false);
            cloth.transform.localScale = new Vector3(1.2f, 0.7f, 0.06f);
            cloth.transform.localPosition = new Vector3(0.58f, 1.42f, 0f);
            StripCollider(cloth);
            cloth.GetComponent<Renderer>().sharedMaterial = material;
            return flag.transform;
        }

        private Transform CreateZone(string name)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name;
            obj.transform.SetParent(_root, false);
            StripCollider(obj);
            obj.GetComponent<Renderer>().sharedMaterial = _neutral;
            return obj.transform;
        }

        private Transform CreateBall()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "TurboBall";
            obj.transform.SetParent(_root, false);
            obj.transform.localScale = Vector3.one * 4.4f;
            StripCollider(obj);
            obj.GetComponent<Renderer>().sharedMaterial = _ballMaterial;
            return obj.transform;
        }

        private static void Place(
            Transform transform,
            Float3 position,
            float lift)
        {
            transform.gameObject.SetActive(true);
            transform.position =
                new Vector3(position.X, position.Y + lift, position.Z);
        }

        private static void SetActive(Transform transform, bool active)
        {
            if (transform != null)
                transform.gameObject.SetActive(active);
        }

        private static Material Material(Color color)
        {
            return new Material(Shader.Find("Standard")) { color = color };
        }

        private static void StripCollider(GameObject obj)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null) Release(collider);
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
