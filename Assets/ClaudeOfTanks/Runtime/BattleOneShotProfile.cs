using System;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public struct BattleOneShotProfile
    {
        public int CoreCount;
        public int SparkCount;
        public int CloudCount;
        public float CoreSize;
        public float SparkSpeed;
        public float CloudSize;
        public float LifetimeS;
        public Color CoreColor;
        public Color SparkColor;
        public Color CloudColor;
        public float SparkSpread;

        public static BattleOneShotProfile Resolve(
            BattleEvent battleEvent,
            bool reducedMotion)
        {
            float caliber = Mathf.Clamp(
                battleEvent.CaliberMm / 120f,
                0.55f,
                1.8f);
            float motion = reducedMotion ? 0.45f : 1f;
            BattleOneShotProfile profile;
            switch (battleEvent.Type)
            {
                case BattleEventType.ShellFired:
                    profile = Create(
                        4, 10, 7,
                        0.8f, 18f, 0.38f, 0.65f,
                        Color.white,
                        new Color(1f, 0.66f, 0.18f, 1f),
                        new Color(0.42f, 0.38f, 0.31f, 0.48f));
                    break;
                case BattleEventType.ShellHit:
                    profile = Impact(battleEvent);
                    break;
                case BattleEventType.StructureHit:
                    profile = Create(
                        2, 12, 14,
                        0.48f, 13f, 0.46f, 0.9f,
                        new Color(1f, 0.82f, 0.5f, 1f),
                        new Color(1f, 0.68f, 0.26f, 1f),
                        new Color(0.48f, 0.44f, 0.38f, 0.58f));
                    break;
                case BattleEventType.PropCrushed:
                    profile = Create(
                        0, 5, 10,
                        0f, 6f, 0.4f, 0.75f,
                        Color.clear,
                        new Color(0.54f, 0.46f, 0.34f, 1f),
                        new Color(0.43f, 0.39f, 0.31f, 0.45f));
                    break;
                case BattleEventType.StructureDestroyed:
                    profile = Create(
                        5, 24, 30,
                        1.2f, 14f, 0.85f, 1.65f,
                        new Color(1f, 0.68f, 0.24f, 1f),
                        new Color(1f, 0.48f, 0.12f, 1f),
                        new Color(0.38f, 0.34f, 0.29f, 0.68f));
                    break;
                default:
                    profile = Create(
                        9, 32, 28,
                        1.6f, 18f, 0.9f, 1.8f,
                        Color.white,
                        new Color(1f, 0.42f, 0.08f, 1f),
                        new Color(0.16f, 0.14f, 0.12f, 0.72f));
                    break;
            }
            profile.Scale(caliber, motion);
            return profile;
        }

        private static BattleOneShotProfile Impact(
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
            if (explosive)
            {
                return Create(
                    battleEvent.Penetrated ? 7 : 4,
                    20, 28,
                    1.1f, 16f, 0.78f, 1.45f,
                    Color.white,
                    new Color(1f, 0.42f, 0.07f, 1f),
                    new Color(0.46f, 0.39f, 0.3f, 0.68f));
            }
            if (battleEvent.Penetrated)
            {
                return Create(
                    4, 28, 12,
                    0.68f, 26f, 0.36f, 0.85f,
                    Color.white,
                    new Color(1f, 0.7f, 0.25f, 1f),
                    new Color(0.22f, 0.2f, 0.18f, 0.62f));
            }
            return Create(
                1, 22, 5,
                0.28f, 34f, 0.2f, 0.58f,
                new Color(1f, 0.86f, 0.58f, 1f),
                new Color(1f, 0.78f, 0.38f, 1f),
                new Color(0.48f, 0.47f, 0.44f, 0.38f),
                0.92f);
        }

        private static BattleOneShotProfile Create(
            int coreCount,
            int sparkCount,
            int cloudCount,
            float coreSize,
            float sparkSpeed,
            float cloudSize,
            float lifetimeS,
            Color coreColor,
            Color sparkColor,
            Color cloudColor,
            float sparkSpread = 0.58f)
        {
            return new BattleOneShotProfile
            {
                CoreCount = coreCount,
                SparkCount = sparkCount,
                CloudCount = cloudCount,
                CoreSize = coreSize,
                SparkSpeed = sparkSpeed,
                CloudSize = cloudSize,
                LifetimeS = lifetimeS,
                CoreColor = coreColor,
                SparkColor = sparkColor,
                CloudColor = cloudColor,
                SparkSpread = sparkSpread
            };
        }

        private void Scale(float caliber, float motion)
        {
            CoreCount = Mathf.Max(
                CoreCount > 0 ? 1 : 0,
                Mathf.CeilToInt(CoreCount * motion));
            SparkCount = Mathf.CeilToInt(
                SparkCount * motion);
            CloudCount = Mathf.CeilToInt(
                CloudCount * motion);
            CoreSize *= caliber;
            SparkSpeed *= Mathf.Lerp(
                0.85f,
                1.15f,
                Mathf.InverseLerp(0.55f, 1.8f, caliber));
            CloudSize *= caliber;
        }
    }
}
