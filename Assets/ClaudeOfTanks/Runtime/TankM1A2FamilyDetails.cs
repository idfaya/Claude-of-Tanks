using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A2FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m1a2" ||
                id == "m1a2_tusk" ||
                id == "m1a2_sepv2" ||
                id == "m1a2_sepv3";
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

            Hide(root.Find("Hull"));
            Hide(root.Find("UpperHull"));
            Hide(root.Find("Armor-track_L"));
            Hide(root.Find("Armor-track_R"));
            Hide(turret.Find("Turret"));
            Hide(turret.Find("Gun"));

            TankAbramsM1HullDetails.Build(
                root,
                color,
                false);
            TankAbramsM1TurretDetails.Build(
                turret,
                color,
                false);
            HideLegacyRoofEquipment(turret);
            TankAbramsM1GunDetails.Build(
                turret,
                definition,
                color,
                false);

            TankM1A2HullDetails.Build(
                root,
                definition,
                color);
            TankM1A2TurretDetails.Build(
                turret,
                definition,
                color);
            TankM1A2RoofWeaponDetails.Build(
                turret,
                definition,
                color);
            TankM1A2GhillieDetails.Build(
                root,
                turret,
                definition);
        }

        private static void HideLegacyRoofEquipment(
            Transform turret)
        {
            string[] names =
            {
                "Painted-AbramsM1-CommanderOptic",
                "AbramsM1-CommanderWindow",
                "Painted-AbramsM1-CommanderMgMount",
                "AbramsM1-CommanderMgReceiver",
                "AbramsM1-CommanderMgBarrel",
                "Painted-AbramsM1-LoaderSkate",
                "AbramsM1-LoaderMgReceiver",
                "AbramsM1-LoaderMgBarrel"
            };
            Transform[] parts =
                turret.GetComponentsInChildren<Transform>(true);
            for (int partIndex = 0;
                partIndex < parts.Length;
                partIndex++)
            {
                for (int nameIndex = 0;
                    nameIndex < names.Length;
                    nameIndex++)
                {
                    if (parts[partIndex].name == names[nameIndex])
                        Hide(parts[partIndex]);
                }
            }
        }

        private static void Hide(Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        internal static Color Dark()
        {
            return new Color(0.045f, 0.05f, 0.038f);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.11f, 0.115f, 0.095f);
        }

        internal static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.075f);
        }
    }
}
