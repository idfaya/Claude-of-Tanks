using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindObjectOfType<BattleController>() != null)
            {
                return;
            }

            GameObject runtime = new GameObject("Claude of Tanks Runtime");
            runtime.AddComponent<BattleController>();
            Object.DontDestroyOnLoad(runtime);
        }
    }
}
