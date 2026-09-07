using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public enum BattleCameraMode
    {
        Arcade,
        Sniper
    }

    public sealed class BattleCameraRig
    {
        private static readonly float[] ArcadeDistances = { 24f, 18f, 13f, 9f, 6f, 4f };
        private static readonly float[] SniperZooms = { 2f, 4f, 8f };
        private const float BaseFov = 60f;
        private int _arcadeIndex = 2;
        private int _sniperIndex;

        public BattleCameraMode Mode { get; private set; } = BattleCameraMode.Arcade;
        public float ArcadeDistance => ArcadeDistances[_arcadeIndex];
        public float Zoom => Mode == BattleCameraMode.Sniper ? SniperZooms[_sniperIndex] : 1f;

        public void Reset()
        {
            Mode = BattleCameraMode.Arcade;
            _arcadeIndex = 2;
            _sniperIndex = 0;
        }

        public void ToggleSniper()
        {
            Mode = Mode == BattleCameraMode.Arcade
                ? BattleCameraMode.Sniper
                : BattleCameraMode.Arcade;
        }

        public void StepZoom(int steps)
        {
            int direction = steps >= 0 ? 1 : -1;
            for (int i = 0; i < Mathf.Abs(steps); i++)
            {
                StepZoomOnce(direction);
            }
        }

        public void Apply(Camera camera, TankState player, Vector3 aimPoint, float deltaTime)
        {
            if (camera == null || player == null)
            {
                return;
            }

            float gunYaw = player.Yaw + player.TurretYaw;
            Vector3 gunForward = new Vector3(Mathf.Sin(gunYaw), 0f, Mathf.Cos(gunYaw));
            Vector3 position = player.Position.ToUnity();

            if (Mode == BattleCameraMode.Sniper)
            {
                Vector3 anchor = position + Vector3.up * 1.7f + gunForward * 1.2f;
                Vector3 direction = aimPoint - anchor;
                Vector2 horizontal = new Vector2(direction.x, direction.z);
                if (horizontal.sqrMagnitude < 144f ||
                    direction.y < -horizontal.magnitude * 0.55f)
                {
                    direction = gunForward + Vector3.down * 0.025f;
                }

                camera.transform.position = anchor;
                camera.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                camera.fieldOfView = BaseFov / Zoom;
                return;
            }

            Vector3 pivot = position + Vector3.up * 1.8f;
            Vector3 desired = pivot - gunForward * ArcadeDistance +
                Vector3.up * (3f + ArcadeDistance * 0.28f);
            float blend = deltaTime <= 0f ? 1f : 1f - Mathf.Exp(-7f * deltaTime);
            camera.transform.position = Vector3.Lerp(camera.transform.position, desired, blend);
            camera.transform.rotation = Quaternion.LookRotation(
                pivot + gunForward * 20f - camera.transform.position,
                Vector3.up);
            camera.fieldOfView = BaseFov;
        }

        private void StepZoomOnce(int direction)
        {
            if (Mode == BattleCameraMode.Arcade)
            {
                if (direction > 0 && _arcadeIndex == ArcadeDistances.Length - 1)
                {
                    Mode = BattleCameraMode.Sniper;
                    _sniperIndex = 0;
                    return;
                }

                _arcadeIndex = Mathf.Clamp(
                    _arcadeIndex + direction,
                    0,
                    ArcadeDistances.Length - 1);
                return;
            }

            if (direction < 0 && _sniperIndex == 0)
            {
                Mode = BattleCameraMode.Arcade;
                _arcadeIndex = ArcadeDistances.Length - 1;
                return;
            }

            _sniperIndex = Mathf.Clamp(
                _sniperIndex + direction,
                0,
                SniperZooms.Length - 1);
        }
    }
}
