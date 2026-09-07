using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class TankView
    {
        private readonly Transform _root;
        private readonly Transform _turret;
        private readonly Renderer[] _renderers;
        private readonly Color _aliveColor;

        private TankView(Transform root, Transform turret, Renderer[] renderers, Color aliveColor)
        {
            _root = root;
            _turret = turret;
            _renderers = renderers;
            _aliveColor = aliveColor;
        }

        public Transform Root => _root;

        public static TankView Create(TankState tank)
        {
            Color teamColor = tank.Team == Team.Alpha
                ? new Color(0.18f, 0.48f, 0.24f)
                : new Color(0.62f, 0.20f, 0.15f);
            GameObject root = new GameObject(tank.Id);

            CreatePart("Hull", PrimitiveType.Cube, root.transform, new Vector3(0f, 1.05f, 0f),
                new Vector3(3.4f, 0.9f, 5.4f), teamColor);
            CreatePart("UpperHull", PrimitiveType.Cube, root.transform, new Vector3(0f, 1.7f, 0.15f),
                new Vector3(2.85f, 0.55f, 3.4f), teamColor * 1.08f);
            CreatePart("LeftTrack", PrimitiveType.Cube, root.transform, new Vector3(-1.75f, 0.65f, 0f),
                new Vector3(0.55f, 0.75f, 5.5f), new Color(0.09f, 0.09f, 0.08f));
            CreatePart("RightTrack", PrimitiveType.Cube, root.transform, new Vector3(1.75f, 0.65f, 0f),
                new Vector3(0.55f, 0.75f, 5.5f), new Color(0.09f, 0.09f, 0.08f));

            GameObject turretRoot = new GameObject("TurretRoot");
            turretRoot.transform.SetParent(root.transform, false);
            turretRoot.transform.localPosition = new Vector3(0f, 2.05f, 0.15f);
            CreatePart("Turret", PrimitiveType.Cylinder, turretRoot.transform, Vector3.zero,
                new Vector3(2.05f, 0.55f, 2.05f), teamColor * 0.92f);
            Transform barrel = CreatePart("Gun", PrimitiveType.Cube, turretRoot.transform,
                new Vector3(0f, 0.05f, 2.65f), new Vector3(0.24f, 0.24f, 4.8f),
                new Color(0.12f, 0.14f, 0.12f));
            barrel.localRotation = Quaternion.identity;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            TankView view = new TankView(root.transform, turretRoot.transform, renderers, teamColor);
            view.Sync(tank);
            return view;
        }

        public void Sync(TankState tank)
        {
            _root.position = tank.Position.ToUnity();
            _root.rotation = Quaternion.Euler(0f, tank.Yaw * Mathf.Rad2Deg, 0f);
            _turret.localRotation = Quaternion.Euler(0f, tank.TurretYaw * Mathf.Rad2Deg, 0f);

            Color color = tank.Destroyed ? new Color(0.08f, 0.08f, 0.075f) : _aliveColor;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].gameObject.name == "Hull" ||
                    _renderers[i].gameObject.name == "UpperHull" ||
                    _renderers[i].gameObject.name == "Turret")
                {
                    _renderers[i].material.color = color;
                }
            }
        }

        public void Destroy()
        {
            Object.Destroy(_root.gameObject);
        }

        private static Transform CreatePart(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
            return part.transform;
        }
    }

    internal static class VectorConversion
    {
        public static Vector3 ToUnity(this Float3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Float3 ToSimulation(this Vector3 value)
        {
            return new Float3(value.x, value.y, value.z);
        }
    }
}
