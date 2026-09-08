using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankNationalFamilyDetails
    {
        public static bool ExcludesGenericSideArmor(
            string id)
        {
            return
                TankLeopardFamilyDetails.Supports(id) ||
                TankChallengerFamilyDetails.Supports(id) ||
                TankMerkavaFamilyDetails.Supports(id) ||
                TankKoreanFamilyDetails.Supports(id) ||
                TankJapaneseFamilyDetails.Supports(id) ||
                TankFrenchFamilyDetails.Supports(id) ||
                TankItalianFamilyDetails.Supports(id) ||
                TankSwedishFamilyDetails.Supports(id);
        }

        public static bool OwnsIfvWeaponPackage(
            string id)
        {
            return TankSwedishFamilyDetails
                .OwnsIfvWeaponPackage(id);
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
            TankSovietFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankLeopardFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankChallengerFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankMerkavaFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankKoreanFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankJapaneseFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankFrenchFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankItalianFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankSwedishFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
        }
    }
}
