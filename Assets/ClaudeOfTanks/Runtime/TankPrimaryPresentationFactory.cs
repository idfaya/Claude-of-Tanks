using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class
        TankPrimaryPresentationFactory
    {
        public static bool Supports(string id)
        {
            switch (id)
            {
                case "strv122":
                case "kv2":
                case "fv4034":
                case "challenger2":
                case "challenger2e":
                case "ua_challenger2":
                case "challenger_3":
                case "challenger_3x":
                case "leo1a5":
                case "leopard2_proto":
                case "leo2a4":
                case "leo2a4_otco":
                case "leo2a4m":
                case "leo2a5":
                case "leo2a5_a5nl":
                case "leo2a6":
                case "leo2a6m":
                case "leo2_revolution":
                case "leo2a7":
                case "leo2a7v":
                case "leo2a6_ua":
                case "mbt70":
                case "t14":
                case "kf51":
                case "kf51b":
                case "m46_patton":
                case "m47_patton":
                case "m48":
                case "m60a1":
                case "m60a2":
                case "m60a3":
                case "merkava1b":
                case "merkava2b":
                case "merkava2d":
                case "merkava3c":
                case "merkava3d":
                case "merkava4b":
                case "m551_sheridan":
                case "m551a1_tts":
                case "m4a3e8":
                case "tiger1":
                case "t34_85":
                case "is2":
                case "panther_g":
                case "t72b_1987":
                case "is3":
                case "t34_85_cad":
                case "newc_tiger":
                case "newc_pziii":
                case "pziii_konserwa":
                case "leichttraktor":
                case "recon_tank":
                case "q_heavy":
                case "tiger2":
                case "sherman_jumbo":
                case "jagdtiger":
                case "jpz_e100":
                case "sturmtiger":
                case "t95":
                case "t30":
                case "is7":
                case "object279":
                case "is6b":
                case "is1":
                case "t72b3":
                case "merkava4":
                case "t44":
                case "t54":
                case "is3_bergman":
                case "isu152":
                case "isu122s":
                case "comet":
                case "challenger_cruiser":
                case "charioteer":
                case "m26_pershing":
                case "m45_patton":
                    return true;
                default:
                    return false;
            }
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (definition == null ||
                !Supports(definition.id))
            {
                return;
            }
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));

            if (UsesFixedCasemate(
                    definition.id))
            {
                BuildHullGun(
                    root,
                    definition,
                    color);
            }
            else if (!HasOwnedGun(
                         definition.id))
            {
                BuildGun(
                    turret,
                    definition,
                    color);
            }
        }

        private static void BuildHullGun(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);
            float axisY =
                definition.armor.gunPivot.y;
            float start =
                definition.armor.gunPivot.z;
            Transform assembly =
                new GameObject(
                    definition.id +
                    "-FixedGunAssembly")
                    .transform;
            assembly.SetParent(root, false);
            AddGunParts(
                assembly,
                definition.id,
                axisY,
                start,
                length,
                radius,
                color);
        }

        private static void BuildGun(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    definition.id +
                    "-GunAssembly");
            AddGunParts(
                fittings,
                definition.id,
                0f,
                0f,
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5f),
                TankAuthoredDetails.ResolveGunRadius(
                    definition),
                color);
        }

        private static void AddGunParts(
            Transform parent,
            string id,
            float axisY,
            float start,
            float length,
            float radius,
            Color color)
        {
            float sleeveLength =
                Mathf.Min(0.96f, length * 0.3f);
            Transform sleeve =
                TankShapeFactory.CylinderPart(
                    "Painted-" + id +
                    "-GunRootSleeve",
                    parent,
                    radius * 1.55f,
                    radius * 1.4f,
                    sleeveLength,
                    18,
                    TankShapeAxis.Z,
                    color * 0.48f);
            sleeve.localPosition =
                new Vector3(
                    0f,
                    axisY,
                    start +
                    sleeveLength * 0.5f);
            float tubeLength =
                Mathf.Max(
                    0.2f,
                    length - sleeveLength);
            Transform tube =
                TankShapeFactory.CylinderPart(
                    "Painted-" + id +
                    "-MainGunTube",
                    parent,
                    radius,
                    radius * 0.94f,
                    tubeLength,
                    24,
                    TankShapeAxis.Z,
                    color * 0.46f);
            tube.localPosition =
                new Vector3(
                    0f,
                    axisY,
                    start +
                    sleeveLength +
                    tubeLength * 0.5f);
            Transform bore =
                TankShapeFactory.CylinderPart(
                    id + "-MuzzleBore",
                    parent,
                    radius * 0.58f,
                    radius * 0.58f,
                    0.045f,
                    16,
                    TankShapeAxis.Z,
                    new Color(
                        0.025f,
                        0.028f,
                        0.024f));
            bore.localPosition =
                new Vector3(
                    0f,
                    axisY,
                    start + length -
                    0.01f);
        }

        private static bool HasOwnedGun(string id)
        {
            return id == "t14" ||
                id == "kf51" ||
                id == "kf51b" ||
                id == "mbt70" ||
                id == "leo1a5";
        }

        private static bool UsesFixedCasemate(
            string id)
        {
            return id == "jagdtiger" ||
                id == "jpz_e100" ||
                id == "sturmtiger" ||
                id == "t95" ||
                id == "isu152" ||
                id == "isu122s";
        }

        private static void HideRenderer(
            Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }
}
