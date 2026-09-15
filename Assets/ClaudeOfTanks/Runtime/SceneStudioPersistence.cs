using System;
using System.IO;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class SceneStudioController
    {
        public string Capture()
        {
            string directory = Path.Combine(
                Application.persistentDataPath,
                "ClaudeOfTanks",
                "Studio");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(
                directory,
                "studio-" +
                DateTime.UtcNow.ToString(
                    "yyyyMMdd-HHmmss") +
                ".tga");
            int width = Math.Max(
                640,
                Screen.width * 2);
            int height = Math.Max(
                360,
                Screen.height * 2);
            RenderTexture target =
                new RenderTexture(
                    width,
                    height,
                    24);
            RenderTexture previous =
                RenderTexture.active;
            RenderTexture previousCamera =
                _camera.targetTexture;
            Texture2D image =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGB24,
                    false);
            try
            {
                _camera.targetTexture = target;
                RenderTexture.active = target;
                _camera.Render();
                image.ReadPixels(
                    new Rect(
                        0f,
                        0f,
                        width,
                        height),
                    0,
                    0);
                image.Apply();
                File.WriteAllBytes(
                    path,
                    EncodeTga(image));
            }
            finally
            {
                _camera.targetTexture =
                    previousCamera;
                RenderTexture.active = previous;
                if (Application.isPlaying)
                {
                    Destroy(image);
                    Destroy(target);
                }
                else
                {
                    DestroyImmediate(image);
                    DestroyImmediate(target);
                }
            }
            return path;
        }

        private static byte[] EncodeTga(
            Texture2D image)
        {
            Color32[] pixels =
                image.GetPixels32();
            byte[] bytes =
                new byte[
                    18 +
                    pixels.Length * 3];
            bytes[2] = 2;
            bytes[12] =
                (byte)(image.width & 0xff);
            bytes[13] =
                (byte)(image.width >> 8);
            bytes[14] =
                (byte)(image.height & 0xff);
            bytes[15] =
                (byte)(image.height >> 8);
            bytes[16] = 24;
            bytes[17] = 0x20;
            int cursor = 18;
            for (int i = 0;
                i < pixels.Length;
                i++)
            {
                bytes[cursor++] = pixels[i].b;
                bytes[cursor++] = pixels[i].g;
                bytes[cursor++] = pixels[i].r;
            }
            return bytes;
        }

        public string ExportSceneJson()
        {
            StudioSceneRecord record =
                new StudioSceneRecord
                {
                    mapId = _mapId,
                    actors =
                        new StudioActorRecord[
                            _actors.Count]
                };
            for (int i = 0;
                i < _actors.Count;
                i++)
            {
                StudioActorSnapshot actor =
                    ActorAt(i);
                record.actors[i] =
                    new StudioActorRecord
                    {
                        vehicleId =
                            actor.VehicleId,
                        camouflageId =
                            actor.CamouflageId,
                        x = actor.Position.X,
                        z = actor.Position.Z,
                        hullYawDeg =
                            actor.HullYawDeg,
                        turretYawDeg =
                            actor.TurretYawDeg,
                        gunPitchDeg =
                            actor.GunPitchDeg,
                        destroyed =
                            actor.Destroyed
                    };
            }
            return JsonUtility.ToJson(
                record,
                true);
        }

        public bool LoadSceneJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;
            StudioSceneRecord record;
            try
            {
                record =
                    JsonUtility.FromJson<
                        StudioSceneRecord>(
                            json);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (record == null ||
                string.IsNullOrEmpty(
                    record.mapId) ||
                record.actors == null ||
                record.actors.Length == 0 ||
                record.actors.Length >
                    MaximumActors)
            {
                return false;
            }
            try
            {
                _catalog.GetMap(record.mapId);
                for (int i = 0;
                    i < record.actors.Length;
                    i++)
                {
                    _catalog.GetVehicle(
                        record.actors[i]
                            .vehicleId);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            for (int i = 0;
                i < _actors.Count;
                i++)
            {
                _actors[i].View.Destroy();
            }
            _actors.Clear();
            _selectedIndex = -1;
            SetMap(record.mapId);
            for (int i = 0;
                i < record.actors.Length;
                i++)
            {
                StudioActorRecord actor =
                    record.actors[i];
                AddActor(actor.vehicleId);
                SetSelectedPose(
                    actor.x,
                    actor.z,
                    actor.hullYawDeg,
                    actor.turretYawDeg,
                    actor.gunPitchDeg);
                SetSelectedCamouflage(
                    actor.camouflageId);
                SetSelectedDestroyed(
                    actor.destroyed);
            }
            SelectActor(0);
            return true;
        }

        [Serializable]
        private sealed class StudioSceneRecord
        {
            public string mapId;
            public StudioActorRecord[] actors;
        }

        [Serializable]
        private sealed class StudioActorRecord
        {
            public string vehicleId;
            public string camouflageId;
            public float x;
            public float z;
            public float hullYawDeg;
            public float turretYawDeg;
            public float gunPitchDeg;
            public bool destroyed;
        }
    }
}
