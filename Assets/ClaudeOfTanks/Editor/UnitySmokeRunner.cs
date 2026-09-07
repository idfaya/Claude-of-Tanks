using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClaudeOfTanks.Editor
{
    public static class UnitySmokeRunner
    {
        private const string ScenePath = "Assets/Scenes/Battle.unity";
        private static double _captureAt;

        public static void EnterPlayMode()
        {
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            _captureAt = EditorApplication.timeSinceStartup + 3d;
            EditorApplication.update -= CaptureWhenReady;
            EditorApplication.update += CaptureWhenReady;
        }

        private static void CaptureWhenReady()
        {
            if (EditorApplication.timeSinceStartup < _captureAt)
            {
                return;
            }

            EditorApplication.update -= CaptureWhenReady;
            GameObject runtime = GameObject.Find("Claude of Tanks Runtime");
            GameObject player = GameObject.Find("player");
            if (runtime == null || player == null)
            {
                Debug.LogError("UNITY_SMOKE_FAILED: runtime or player object was not created.");
                return;
            }

            string output = Path.GetFullPath("Logs/unity-play-smoke.png");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            ScreenCapture.CaptureScreenshot(output, 1);
            Debug.Log("UNITY_SMOKE_PASSED: Play Mode runtime and player are active. Screenshot: " + output);
        }
    }
}
