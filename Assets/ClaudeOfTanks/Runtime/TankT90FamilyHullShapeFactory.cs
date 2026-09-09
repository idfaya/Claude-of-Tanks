using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90FamilyHullShapeFactory
    {
        private static readonly TankHullProfilePoint[] Deck =
        {
            P(-3.43f, 1.35f), P(-3.20f, 1.47f),
            P(-3.00f, 1.52f), P(-2.55f, 1.545f),
            P(0.95f, 1.545f), P(1.40f, 1.50f),
            P(1.75f, 1.46f), P(2.30f, 1.40f),
            P(2.90f, 1.26f), P(3.43f, 1.04f)
        };

        private static readonly TankHullProfilePoint[] Belly =
        {
            P(-3.43f, 1.05f), P(-3.36f, 0.86f),
            P(-3.10f, 0.72f), P(-2.62f, 0.48f),
            P(-2.40f, 0.44f), P(2.45f, 0.44f),
            P(2.80f, 0.56f), P(3.10f, 0.71f),
            P(3.43f, 0.82f)
        };

        private static readonly TankHullProfilePoint[] UpperWidth =
        {
            P(-3.43f, 1.02f), P(-3.09f, 1.30f),
            P(-2.96f, 1.60f), P(2.95f, 1.60f),
            P(3.16f, 1.32f), P(3.43f, 0.60f)
        };

        private static readonly TankHullProfilePoint[] LowerWidth =
        {
            P(-3.43f, 0.64f), P(-2.95f, 0.88f),
            P(-2.30f, 0.94f), P(2.35f, 0.94f),
            P(2.85f, 0.88f), P(3.43f, 0.64f)
        };

        private static readonly TankHullProfilePoint[] Sponson =
        {
            P(-3.43f, 1.22f), P(-2.90f, 1.22f),
            P(-2.82f, 1.40f), P(-2.05f, 1.40f),
            P(-1.80f, 1.22f), P(2.42f, 1.22f),
            P(3.43f, 1.22f)
        };

        public static Transform Build(
            string name,
            Transform parent,
            Color color,
            float verticalOffset = 0f,
            float? flatSponsonY = null)
        {
            TankHullProfilePoint[] deck =
                ShiftValues(Deck, verticalOffset);
            TankHullProfilePoint[] belly =
                ShiftValues(Belly, verticalOffset);
            if (flatSponsonY.HasValue)
            {
                return TankHullLoftShapeFactory.Build(
                    name,
                    parent,
                    deck,
                    belly,
                    UpperWidth,
                    LowerWidth,
                    flatSponsonY.Value,
                    color);
            }
            return TankHullLoftShapeFactory.Build(
                name,
                parent,
                deck,
                belly,
                UpperWidth,
                LowerWidth,
                ShiftValues(Sponson, verticalOffset),
                color);
        }

        private static TankHullProfilePoint[] ShiftValues(
            TankHullProfilePoint[] source,
            float offset)
        {
            TankHullProfilePoint[] result =
                new TankHullProfilePoint[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                result[index] = new TankHullProfilePoint(
                    source[index].Z,
                    source[index].Value + offset);
            }
            return result;
        }

        private static TankHullProfilePoint P(float z, float value)
        {
            return new TankHullProfilePoint(z, value);
        }
    }
}
