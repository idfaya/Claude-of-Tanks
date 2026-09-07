using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattleCameraRigTests
    {
        [Test]
        public void WheelZoomTraversesArcadeAndSniperLadders()
        {
            BattleCameraRig rig = new BattleCameraRig();

            Assert.That(rig.Mode, Is.EqualTo(BattleCameraMode.Arcade));
            Assert.That(rig.ArcadeDistance, Is.EqualTo(13f));

            rig.StepZoom(3);
            Assert.That(rig.ArcadeDistance, Is.EqualTo(4f));

            rig.StepZoom(1);
            Assert.That(rig.Mode, Is.EqualTo(BattleCameraMode.Sniper));
            Assert.That(rig.Zoom, Is.EqualTo(2f));

            rig.StepZoom(2);
            Assert.That(rig.Zoom, Is.EqualTo(8f));

            rig.StepZoom(-3);
            Assert.That(rig.Mode, Is.EqualTo(BattleCameraMode.Arcade));
            Assert.That(rig.ArcadeDistance, Is.EqualTo(4f));
        }

        [Test]
        public void CameraTracksWorldGunBearingAndAppliesScopedFov()
        {
            GameObject cameraObject = new GameObject("CameraRigTest");
            Camera camera = cameraObject.AddComponent<Camera>();
            TankState player = new TankState(
                "player", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            BattleCameraRig rig = new BattleCameraRig();

            try
            {
                rig.Apply(camera, player, new Vector3(0f, 1.7f, 100f), 0f);
                Vector3 firstPosition = camera.transform.position;

                player.Yaw = MathUtil.Pi * 0.5f;
                player.TurretYaw = -MathUtil.Pi * 0.5f;
                rig.Apply(camera, player, new Vector3(0f, 1.7f, 100f), 0f);

                Assert.That(camera.transform.position.x, Is.EqualTo(firstPosition.x).Within(0.001f));
                Assert.That(camera.transform.position.z, Is.EqualTo(firstPosition.z).Within(0.001f));
                Assert.That(camera.fieldOfView, Is.EqualTo(60f));

                rig.ToggleSniper();
                rig.Apply(camera, player, new Vector3(0f, 1.7f, 100f), 0f);

                Assert.That(rig.Mode, Is.EqualTo(BattleCameraMode.Sniper));
                Assert.That(camera.fieldOfView, Is.EqualTo(30f));
                Assert.That(camera.transform.position.z, Is.EqualTo(1.2f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
