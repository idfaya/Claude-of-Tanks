using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public enum ImpactDecalKind
    {
        Penetration,
        Gouge,
        Scuff,
        Scorch
    }

    public sealed class BattleImpactDecals : MonoBehaviour
    {
        public const int PoolSize = 48;
        private const float SurfaceLiftM = 0.026f;

        private readonly Queue<DecalNode> _available =
            new Queue<DecalNode>();
        private readonly List<DecalNode> _active =
            new List<DecalNode>();
        private readonly List<DecalNode> _nodes =
            new List<DecalNode>();
        private BattleImpactDecalAssets _assets;
        private uint _noise = 0x51f7a3u;

        public int ActiveCount => _active.Count;

        public static BattleImpactDecals Create(
            Transform parent)
        {
            GameObject root = new GameObject("ImpactDecals");
            root.transform.SetParent(parent, false);
            BattleImpactDecals decals =
                root.AddComponent<BattleImpactDecals>();
            decals.Initialize();
            return decals;
        }

        public bool StampImpact(
            BattleEvent battleEvent,
            Transform target)
        {
            if (battleEvent.Type != BattleEventType.ShellHit ||
                target == null)
            {
                return false;
            }
            ImpactDecalKind kind = Classify(battleEvent);
            DecalNode node = Acquire();
            Vector3 normal = Normal(battleEvent);
            Vector3 surfacePosition;
            Transform surface = ResolveSurface(
                target,
                battleEvent.Position.ToUnity(),
                normal,
                out surfacePosition);
            Configure(
                node,
                target,
                surface,
                surfacePosition,
                normal,
                Tangent(battleEvent),
                kind,
                battleEvent.CaliberMm);
            return true;
        }

        public void StampGroundScorch(BattleEvent battleEvent)
        {
            DecalNode node = Acquire();
            Configure(
                node,
                null,
                transform,
                battleEvent.Position.ToUnity(),
                Vector3.up,
                HorizontalDirection(battleEvent),
                ImpactDecalKind.Scorch,
                Mathf.Max(105f, battleEvent.CaliberMm));
            float size = Mathf.Clamp(
                battleEvent.CaliberMm / 22f,
                4.4f,
                7.2f);
            node.Root.transform.localScale =
                new Vector3(size, size * 0.82f, 1f);
        }

        public void ClearTarget(Transform target)
        {
            if (target == null) return;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Owner == target)
                    ReleaseAt(i);
            }
        }

        public void ResetAll()
        {
            _active.Clear();
            _available.Clear();
            for (int i = 0; i < _nodes.Count; i++)
            {
                DecalNode node = _nodes[i];
                node.Owner = null;
                node.Root.transform.SetParent(transform, false);
                node.Root.SetActive(false);
                _available.Enqueue(node);
            }
        }

        public static ImpactDecalKind Classify(
            BattleEvent battleEvent)
        {
            bool explosive =
                string.Equals(
                    battleEvent.ShellType,
                    "HE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    battleEvent.ShellType,
                    "HESH",
                    StringComparison.OrdinalIgnoreCase);
            if (explosive) return ImpactDecalKind.Scorch;
            if (battleEvent.Penetrated)
                return ImpactDecalKind.Penetration;
            float incidence =
                Mathf.Abs(
                    Vector3.Dot(
                        Direction(battleEvent),
                        Normal(battleEvent)));
            return incidence < 0.42f
                ? ImpactDecalKind.Gouge
                : ImpactDecalKind.Scuff;
        }

        private void Initialize()
        {
            _assets = new BattleImpactDecalAssets();
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject root =
                    new GameObject("ImpactDecal-" + i);
                root.transform.SetParent(transform, false);
                MeshFilter filter =
                    root.AddComponent<MeshFilter>();
                MeshRenderer renderer =
                    root.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _assets.Material;
                renderer.shadowCastingMode =
                    ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                root.SetActive(false);
                DecalNode node = new DecalNode
                {
                    Root = root,
                    Filter = filter
                };
                _nodes.Add(node);
                _available.Enqueue(node);
            }
        }

        private void Configure(
            DecalNode node,
            Transform owner,
            Transform surface,
            Vector3 position,
            Vector3 normal,
            Vector3 tangent,
            ImpactDecalKind kind,
            float caliberMm)
        {
            node.Owner = owner;
            node.Root.transform.SetParent(surface, true);
            node.Root.transform.position =
                position + normal * SurfaceLiftM;
            node.Root.transform.rotation =
                Quaternion.LookRotation(-normal, tangent) *
                Quaternion.AngleAxis(
                    Next01() * 18f - 9f,
                    Vector3.forward);
            float size = Mathf.Clamp(
                caliberMm / 180f,
                0.22f,
                0.9f);
            switch (kind)
            {
                case ImpactDecalKind.Penetration:
                    node.Filter.sharedMesh =
                        _assets.PenetrationMesh;
                    node.Root.transform.localScale =
                        Vector3.one * size;
                    break;
                case ImpactDecalKind.Gouge:
                    node.Filter.sharedMesh =
                        _assets.GougeMesh;
                    node.Root.transform.localScale =
                        new Vector3(
                            size * 0.42f,
                            size * 1.9f,
                            size);
                    break;
                case ImpactDecalKind.Scuff:
                    node.Filter.sharedMesh =
                        _assets.ScuffMesh;
                    node.Root.transform.localScale =
                        Vector3.one * size * 0.8f;
                    break;
                default:
                    node.Filter.sharedMesh =
                        _assets.ScorchMesh;
                    node.Root.transform.localScale =
                        new Vector3(
                            size * 2.2f,
                            size * 1.65f,
                            size);
                    break;
            }
            node.Root.SetActive(true);
            _active.Add(node);
        }

        private DecalNode Acquire()
        {
            if (_available.Count > 0)
                return _available.Dequeue();
            DecalNode oldest = _active[0];
            _active.RemoveAt(0);
            return oldest;
        }

        private void ReleaseAt(int index)
        {
            DecalNode node = _active[index];
            _active.RemoveAt(index);
            node.Owner = null;
            node.Root.transform.SetParent(transform, false);
            node.Root.SetActive(false);
            _available.Enqueue(node);
        }

        private static Transform ResolveSurface(
            Transform target,
            Vector3 position,
            Vector3 normal,
            out Vector3 surfacePosition)
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>();
            Transform closest = target;
            surfacePosition = position;
            float bestRayDistance = float.PositiveInfinity;
            float bestFallbackDistance = float.PositiveInfinity;
            Ray ray = new Ray(
                position + normal * 10f,
                -normal);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!renderer.enabled ||
                    renderer.gameObject.name.StartsWith(
                        "ImpactDecal-",
                        StringComparison.Ordinal))
                {
                    continue;
                }
                float rayDistance;
                if (renderer.bounds.IntersectRay(
                        ray,
                        out rayDistance) &&
                    rayDistance < bestRayDistance)
                {
                    bestRayDistance = rayDistance;
                    closest = AttachmentFrame(
                        renderer,
                        target);
                    surfacePosition =
                        ray.GetPoint(rayDistance);
                }
                if (!float.IsPositiveInfinity(
                    bestRayDistance))
                {
                    continue;
                }
                Vector3 fallback =
                    renderer.bounds.ClosestPoint(position);
                float fallbackDistance =
                    (fallback - position).sqrMagnitude;
                if (fallbackDistance >=
                    bestFallbackDistance)
                {
                    continue;
                }
                bestFallbackDistance = fallbackDistance;
                closest = AttachmentFrame(
                    renderer,
                    target);
                surfacePosition = fallback;
            }
            return closest;
        }

        private static Transform AttachmentFrame(
            Renderer renderer,
            Transform target)
        {
            return renderer.transform.parent != null
                ? renderer.transform.parent
                : target;
        }

        private static Vector3 Normal(BattleEvent battleEvent)
        {
            Vector3 normal = battleEvent.Normal.ToUnity();
            return normal.sqrMagnitude < 0.01f
                ? Vector3.up
                : normal.normalized;
        }

        private static Vector3 Direction(BattleEvent battleEvent)
        {
            Vector3 direction =
                battleEvent.Direction.ToUnity();
            return direction.sqrMagnitude < 0.01f
                ? Vector3.forward
                : direction.normalized;
        }

        private static Vector3 Tangent(BattleEvent battleEvent)
        {
            Vector3 normal = Normal(battleEvent);
            Vector3 tangent = Vector3.ProjectOnPlane(
                Direction(battleEvent),
                normal);
            if (tangent.sqrMagnitude < 0.01f)
            {
                tangent = Mathf.Abs(
                    Vector3.Dot(normal, Vector3.up)) > 0.95f
                    ? Vector3.forward
                    : Vector3.up;
            }
            return tangent.normalized;
        }

        private static Vector3 HorizontalDirection(
            BattleEvent battleEvent)
        {
            Vector3 direction =
                Direction(battleEvent);
            direction.y = 0f;
            return direction.sqrMagnitude < 0.01f
                ? Vector3.forward
                : direction.normalized;
        }

        private float Next01()
        {
            _noise = _noise * 1664525u + 1013904223u;
            return (_noise >> 8) / 16777216f;
        }

        private void OnDestroy()
        {
            _assets?.Dispose();
        }

        private sealed class DecalNode
        {
            public Transform Owner;
            public GameObject Root;
            public MeshFilter Filter;
        }
    }
}
