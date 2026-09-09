using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90FamilyDetails
    {
        private const string DisableBakedPresentationVariable =
            "COT_DISABLE_T90_BAKED_PRESENTATION";

        public static bool Supports(string id)
        {
            return id == "t90";
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (!Supports(definition?.id)) return;

            if (!BakedPresentationDisabled() &&
                TankBakedPresentationCatalog.TryBuild(
                    definition.id,
                    root,
                    turret))
            {
                return;
            }

            TankPresentationGenerator.Build(
                TankT90PresentationSchema.Create(),
                root,
                turret,
                definition,
                color);
        }

        private static bool BakedPresentationDisabled()
        {
            return System.Environment.GetEnvironmentVariable(
                DisableBakedPresentationVariable) == "1";
        }

        internal static Color Dark()
        {
            return new Color(0.055f, 0.065f, 0.05f);
        }

        internal static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.075f);
        }

        internal static Color ShtoraGlass()
        {
            return new Color(0.5f, 0.07f, 0.035f);
        }

        internal static Color Wood()
        {
            return new Color(0.2f, 0.12f, 0.065f);
        }
    }
}
