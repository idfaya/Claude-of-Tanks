using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if UNITY_SERVER
            return;
#else
            if (Application.isBatchMode &&
                System.Array.IndexOf(
                    System.Environment.GetCommandLineArgs(),
                    "--cot-server") >= 0)
            {
                return;
            }
            GameSettings.Current.Apply();
            if (Object.FindObjectOfType<GameFlowController>() != null)
            {
                return;
            }

            GameObject runtime = new GameObject("Claude of Tanks Runtime");
            runtime.AddComponent<GameFlowController>();
            Object.DontDestroyOnLoad(runtime);
#endif
        }
    }
}
