using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankNationalFamilyDetails
    {
        public static bool ExcludesGenericSideArmor(
            string id)
        {
            return
                TankAbramsXFamilyDetails.Supports(id) ||
                TankM1A3FamilyDetails.Supports(id) ||
                TankM1A2FamilyDetails.Supports(id) ||
                TankAbramsM1FamilyDetails.Supports(id) ||
                TankLeopardFamilyDetails.Supports(id) ||
                TankChallengerFamilyDetails.Supports(id) ||
                TankMerkavaFamilyDetails.Supports(id) ||
                TankKoreanFamilyDetails.Supports(id) ||
                TankJapaneseFamilyDetails.Supports(id) ||
                TankFrenchFamilyDetails.Supports(id) ||
                TankItalianFamilyDetails.Supports(id) ||
                TankSwedishFamilyDetails.Supports(id) ||
                TankChineseFamilyDetails.Supports(id) ||
                TankPattonFamilyDetails.Supports(id) ||
                TankSheridanFamilyDetails.Supports(id) ||
                TankKf51FamilyDetails.Supports(id) ||
                TankLeopard1A5FamilyDetails.Supports(id) ||
                TankMbt70FamilyDetails.Supports(id) ||
                TankT14FamilyDetails.Supports(id) ||
                TankPumaFamilyDetails.Supports(id) ||
                TankBradleyFamilyDetails.Supports(id) ||
                TankMarder1A3FamilyDetails.Supports(id) ||
                TankBmp2FamilyDetails.Supports(id) ||
                TankBmp3FamilyDetails.Supports(id) ||
                TankBwp1FamilyDetails.Supports(id) ||
                TankT62Obr1975FamilyDetails.Supports(id) ||
                TankUpiorFamilyDetails.Supports(id) ||
                TankBmpt2FamilyDetails.Supports(id) ||
                TankWarriorFamilyDetails.Supports(id) ||
                TankType89FamilyDetails.Supports(id) ||
                TankType89LightTigerDetails.Supports(id);
        }

        public static bool OwnsIfvWeaponPackage(
            string id)
        {
            return
                TankSwedishFamilyDetails
                    .OwnsIfvWeaponPackage(id) ||
                TankPumaFamilyDetails.Supports(id) ||
                TankBradleyFamilyDetails.Supports(id) ||
                TankMarder1A3FamilyDetails.Supports(id) ||
                TankBmp2FamilyDetails.Supports(id) ||
                TankBmp3FamilyDetails.Supports(id) ||
                TankBwp1FamilyDetails.Supports(id) ||
                TankUpiorFamilyDetails.Supports(id) ||
                TankBmpt2FamilyDetails.Supports(id) ||
                TankWarriorFamilyDetails.Supports(id) ||
                TankType89FamilyDetails.Supports(id) ||
                TankType89LightTigerDetails.Supports(id);
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
            TankAbramsXFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankM1A3FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankM1A2FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankAbramsM1FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankSovietFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankT62Obr1975FamilyDetails.Build(
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
            TankChineseFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankPattonFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankSheridanFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankKf51FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankLeopard1A5FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankMbt70FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankT14FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankPumaFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankBradleyFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankMarder1A3FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankBmp2FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankBmp3FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankBwp1FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankUpiorFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankBmpt2FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankWarriorFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankType89FamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankType89LightTigerDetails.Build(
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
