using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankViewPose
    {
        public static bool IsGeneratedMesh(
            MeshFilter filter)
        {
            return filter.name.StartsWith(
                    "Armor-") ||
                filter.name.StartsWith(
                    "TrackLinks-") ||
                (filter.sharedMesh != null &&
                 filter.sharedMesh.name ==
                    filter.name + "Mesh");
        }

        public static void Apply(
            Transform root,
            Transform turret,
            TankGunPitchRig gunPitchRig,
            TankState tank)
        {
            root.position = tank.Position.ToUnity();
            root.rotation = Quaternion.Euler(
                -TankPoseMath.VisualPitchRad(tank) *
                    Mathf.Rad2Deg,
                tank.Yaw * Mathf.Rad2Deg,
                tank.HullRollRad *
                    Mathf.Rad2Deg);
            turret.localRotation =
                Quaternion.Euler(
                    0f,
                    tank.TurretYaw *
                        Mathf.Rad2Deg,
                    0f);
            gunPitchRig.Apply(
                tank.GunPitchRad);
        }
    }
}
