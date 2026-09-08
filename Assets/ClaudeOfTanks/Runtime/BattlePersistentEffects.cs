using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattlePersistentEffects : MonoBehaviour
    {
        public const int TankPoolSize = 16;
        public const int TrackPrintLimit = 96;
        public const float TrackPrintLifetimeS = 12f;

        private readonly Dictionary<string, TankFxNode> _active =
            new Dictionary<string, TankFxNode>(StringComparer.Ordinal);
        private readonly Queue<TankFxNode> _available =
            new Queue<TankFxNode>();
        private readonly List<TankFxNode> _nodes =
            new List<TankFxNode>();
        private readonly List<string> _staleIds = new List<string>();

        private GameSettings _settings;
        private Material _particleMaterial;
        private BattleTrackPrints _trackPrints;
        private int _frame;

        public int ActiveTankCount => _active.Count;
        public int ActiveTrackPrintCount => _trackPrints.ActiveCount;

        public int ActiveSystemCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _nodes.Count; i++)
                {
                    TankFxNode node = _nodes[i];
                    if (node.DustActive) count++;
                    if (node.ExhaustActive) count++;
                    if (node.FlameActive) count++;
                    if (node.SmokeActive) count++;
                }
                return count;
            }
        }

        public static BattlePersistentEffects Create(
            Transform parent,
            Material particleMaterial,
            GameSettings settings)
        {
            GameObject root = new GameObject("PersistentEffects");
            root.transform.SetParent(parent, false);
            BattlePersistentEffects effects =
                root.AddComponent<BattlePersistentEffects>();
            effects.Initialize(
                particleMaterial,
                settings ?? throw new ArgumentNullException(nameof(settings)));
            return effects;
        }

        public void Sync(IList<TankState> tanks)
        {
            _frame++;
            if (tanks != null)
            {
                for (int i = 0; i < tanks.Count; i++)
                {
                    TankState tank = tanks[i];
                    if (tank == null || string.IsNullOrEmpty(tank.Id)) continue;
                    TankFxNode node;
                    if (!_active.TryGetValue(tank.Id, out node))
                    {
                        if (_available.Count == 0) continue;
                        node = _available.Dequeue();
                        node.OwnerId = tank.Id;
                        node.HasPosition = false;
                        node.Root.SetActive(true);
                        _active.Add(tank.Id, node);
                    }
                    node.SeenFrame = _frame;
                    SyncNode(node, tank);
                }
            }

            _staleIds.Clear();
            foreach (KeyValuePair<string, TankFxNode> pair in _active)
            {
                if (pair.Value.SeenFrame != _frame)
                    _staleIds.Add(pair.Key);
            }
            for (int i = 0; i < _staleIds.Count; i++)
                ReleaseNode(_staleIds[i]);
        }

        public void ResetAll()
        {
            _active.Clear();
            _available.Clear();
            for (int i = 0; i < _nodes.Count; i++)
            {
                TankFxNode node = _nodes[i];
                Clear(node);
                node.OwnerId = null;
                node.Root.SetActive(false);
                _available.Enqueue(node);
            }
            _trackPrints.ResetAll();
        }

        private void Initialize(
            Material particleMaterial,
            GameSettings settings)
        {
            _particleMaterial = particleMaterial;
            _settings = settings;
            _trackPrints = BattleTrackPrints.Create(transform);
            for (int i = 0; i < TankPoolSize; i++)
            {
                TankFxNode node = CreateTankNode(i);
                _nodes.Add(node);
                _available.Enqueue(node);
            }
        }

        private void SyncNode(TankFxNode node, TankState tank)
        {
            Vector3 position = tank.Position.ToUnity();
            node.Root.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, tank.Yaw * Mathf.Rad2Deg, 0f));

            bool alive = !tank.Destroyed;
            bool burning = tank.Combat.Fire.Burning;
            float speed = Mathf.Abs(tank.SpeedMps);
            float motionScale = _settings.ReducedMotion ? 0.35f : 1f;
            SetEmission(node.Dust, alive && speed > 1.4f
                ? Mathf.Lerp(4f, 24f, Mathf.Clamp01(speed / 18f)) * motionScale
                : 0f,
                ref node.DustActive);
            SetEmission(node.Exhaust, alive
                ? Mathf.Lerp(1.5f, 5f, Mathf.Clamp01(speed / 16f)) * motionScale
                : 0f,
                ref node.ExhaustActive);
            SetEmission(node.Flame, burning
                ? (tank.Destroyed ? 16f : 11f) * motionScale
                : 0f,
                ref node.FlameActive);
            SetEmission(node.Smoke, burning || tank.Destroyed
                ? (tank.Destroyed ? 13f : 8f) * motionScale
                : 0f,
                ref node.SmokeActive);

            if (node.HasPosition && alive && speed > 1.4f)
            {
                float distance = HorizontalDistance(
                    node.LastPosition,
                    position);
                node.TrackDistance += distance;
                float spacing = _settings.ReducedMotion ? 2.2f : 1.1f;
                if (node.TrackDistance >= spacing)
                {
                    node.TrackDistance %= spacing;
                    _trackPrints.Stamp(tank.Id, position, tank.Yaw);
                }
            }
            else if (!alive || speed <= 1.4f)
            {
                node.TrackDistance = 0f;
            }
            node.LastPosition = position;
            node.HasPosition = true;
        }

        private TankFxNode CreateTankNode(int index)
        {
            GameObject root = new GameObject("TankPersistent-" + index);
            root.transform.SetParent(transform, false);
            TankFxNode node = new TankFxNode
            {
                Root = root,
                Dust = CreateParticles(
                    "TrackDust",
                    root.transform,
                    new Vector3(0f, 0.18f, -1.45f),
                    new Color(0.43f, 0.38f, 0.29f, 0.42f),
                    0.38f,
                    1.35f,
                    1.8f,
                    34),
                Exhaust = CreateParticles(
                    "EngineExhaust",
                    root.transform,
                    new Vector3(0f, 1.15f, -1.35f),
                    new Color(0.18f, 0.2f, 0.19f, 0.46f),
                    0.22f,
                    1.7f,
                    1.25f,
                    24),
                Flame = CreateParticles(
                    "Fire",
                    root.transform,
                    new Vector3(0f, 1.05f, 0f),
                    new Color(1f, 0.28f, 0.025f, 0.82f),
                    0.3f,
                    1.45f,
                    1.15f,
                    30),
                Smoke = CreateParticles(
                    "SmokeColumn",
                    root.transform,
                    new Vector3(0f, 1.45f, 0f),
                    new Color(0.12f, 0.115f, 0.105f, 0.58f),
                    0.7f,
                    3.4f,
                    4.2f,
                    56)
            };
            Seed(node.Dust, index, 0);
            Seed(node.Exhaust, index, 1);
            Seed(node.Flame, index, 2);
            Seed(node.Smoke, index, 3);
            root.SetActive(false);
            return node;
        }

        private ParticleSystem CreateParticles(
            string name,
            Transform parent,
            Vector3 localPosition,
            Color color,
            float startSize,
            float endSize,
            float lifetime,
            int maximum)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;
            root.transform.localRotation = name == "TrackDust"
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.Euler(-90f, 0f, 0f);
            ParticleSystem particles = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true;
            main.maxParticles = maximum;
            main.startLifetime = lifetime;
            main.startSpeed = name == "TrackDust"
                ? new ParticleSystem.MinMaxCurve(0.6f, 1.8f)
                : new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(
                startSize,
                startSize * 1.35f);
            main.startColor = color;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = name == "TrackDust" ? 42f : 18f;
            shape.radius = name == "SmokeColumn" ? 0.8f : 0.42f;

            ParticleSystem.SizeOverLifetimeModule size =
                particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                AnimationCurve.Linear(0f, 1f, 1f, endSize / startSize));
            ParticleSystem.ColorOverLifetimeModule fade =
                particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = FadeGradient(color);

            ParticleSystemRenderer renderer =
                root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = _particleMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return particles;
        }

        private void ReleaseNode(string ownerId)
        {
            TankFxNode node = _active[ownerId];
            _active.Remove(ownerId);
            Clear(node);
            node.OwnerId = null;
            node.Root.SetActive(false);
            _available.Enqueue(node);
            _trackPrints.ReleaseOwner(ownerId);
        }

        private static void Clear(TankFxNode node)
        {
            StopAndClear(node.Dust);
            StopAndClear(node.Exhaust);
            StopAndClear(node.Flame);
            StopAndClear(node.Smoke);
            node.DustActive = false;
            node.ExhaustActive = false;
            node.FlameActive = false;
            node.SmokeActive = false;
            node.HasPosition = false;
            node.TrackDistance = 0f;
        }

        private static void SetEmission(
            ParticleSystem particles,
            float rate,
            ref bool active)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = rate;
            bool enabled = rate > 0f;
            if (enabled && !active) particles.Play();
            else if (!enabled && active)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            active = enabled;
        }

        private static void StopAndClear(ParticleSystem particles)
        {
            particles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void Seed(
            ParticleSystem particles,
            int nodeIndex,
            int channel)
        {
            StopAndClear(particles);
            particles.useAutoRandomSeed = false;
            particles.randomSeed =
                0x9e3779b9u +
                (uint)(nodeIndex * 4 + channel + 1) * 2654435761u;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x;
            float z = a.z - b.z;
            return Mathf.Sqrt(x * x + z * z);
        }

        private static ParticleSystem.MinMaxGradient FadeGradient(Color color)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(color.a * 0.55f, 0.65f),
                    new GradientAlphaKey(0f, 1f)
                });
            return new ParticleSystem.MinMaxGradient(gradient);
        }

        private sealed class TankFxNode
        {
            public string OwnerId;
            public GameObject Root;
            public ParticleSystem Dust;
            public ParticleSystem Exhaust;
            public ParticleSystem Flame;
            public ParticleSystem Smoke;
            public int SeenFrame;
            public Vector3 LastPosition;
            public bool HasPosition;
            public float TrackDistance;
            public bool DustActive;
            public bool ExhaustActive;
            public bool FlameActive;
            public bool SmokeActive;
        }

    }
}
