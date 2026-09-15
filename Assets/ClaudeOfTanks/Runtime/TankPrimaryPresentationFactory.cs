using UnityEngine;
namespace ClaudeOfTanks.Runtime
{
    internal static class TankPrimaryPresentationFactory
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
            if (definition == null || !Supports(definition.id)) return;
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));

            BuildHull(root, definition, color);
            if (UsesFixedCasemate(definition.id))
            {
                BuildCasemate(root, definition, color);
                BuildHullGun(root, definition, color);
                return;
            }
            if (definition.id == "kv2" ||
                definition.id == "q_heavy")
                BuildBoxTurret(turret, definition, color);
            else if (UsesCastTurret(definition.id))
                BuildCastTurret(turret, definition, color);
            else
                BuildAngularTurret(turret, definition, color);

            if (!HasOwnedGun(definition.id))
                BuildGun(turret, definition, color);
        }
        private static void BuildCasemate(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            float width = definition.dims.widthM;
            float height = definition.dims.heightM;
            float length = definition.dims.hullLengthM;
            float half = width * 0.40f;
            float roof = height * 0.72f;
            float front = length * 0.24f;
            float rear = -length * 0.29f;
            Vector2[] plan =
            {
                new Vector2(-half * 0.72f, front),
                new Vector2(half * 0.72f, front),
                new Vector2(half, front * 0.55f),
                new Vector2(half, rear),
                new Vector2(-half, rear),
                new Vector2(-half, front * 0.55f)
            };
            TankShapeFactory.PolyMultiLoftPart(
                "Painted-Primary-" + definition.id + "-CasemateShell",
                root,
                plan,
                new[]
                {
                    new TankShapeLoftRing(
                        height * 0.43f,
                        1.00f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        roof,
                        0.76f,
                        new Vector2(0f, -0.08f))
                },
                color * 0.62f);
        }
        private static void BuildHullGun(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                TankAuthoredDetails.ResolveGunLength(definition, 5.0f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(definition);
            float axisY = definition.armor.gunPivot.y;
            float start = definition.armor.gunPivot.z;
            Transform assembly =
                new GameObject(
                    "Primary-" + definition.id + "-FixedGunAssembly")
                    .transform;
            assembly.SetParent(root, false);
            Transform sleeve = TankShapeFactory.CylinderPart(
                "Painted-Primary-" + definition.id + "-GunRootSleeve",
                assembly,
                radius * 1.55f,
                radius * 1.40f,
                0.82f,
                18,
                TankShapeAxis.Z,
                color * 0.48f);
            sleeve.localPosition =
                new Vector3(0f, axisY, start + 0.41f);
            Transform tube = TankShapeFactory.CylinderPart(
                "Painted-Primary-" + definition.id + "-MainGunTube",
                assembly,
                radius,
                radius * 0.94f,
                Mathf.Max(0.2f, length - 0.82f),
                24,
                TankShapeAxis.Z,
                color * 0.46f);
            tube.localPosition =
                new Vector3(
                    0f,
                    axisY,
                    start + 0.82f +
                    Mathf.Max(0.2f, length - 0.82f) * 0.5f);
            Transform bore = TankShapeFactory.CylinderPart(
                "Primary-" + definition.id + "-MuzzleBore",
                assembly,
                radius * 0.58f,
                radius * 0.58f,
                0.045f,
                16,
                TankShapeAxis.Z,
                new Color(0.025f, 0.028f, 0.024f));
            bore.localPosition =
                new Vector3(0f, axisY, start + length - 0.01f);
        }
        private static void BuildHull(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            float width = definition.dims.widthM;
            float height = definition.dims.heightM;
            float length = definition.dims.hullLengthM;
            float rear = -length * 0.49f;
            float front = length * 0.51f;
            float half = width * 0.47f;
            float belly = Mathf.Max(0.30f, height * 0.18f);
            float deckRatio = IsLowProfile(definition.id)
                ? 0.53f
                : UsesCastTurret(definition.id)
                    ? 0.57f
                    : 0.59f;
            float deck = height * deckRatio;
            TankHullLoftShapeFactory.Build(
                "Painted-Primary-" + definition.id + "-HullLoft",
                root,
                Curve(
                    rear, deck - 0.18f,
                    rear + length * 0.10f, deck,
                    -length * 0.08f, deck,
                    length * 0.20f, deck - 0.08f,
                    front - length * 0.14f, deck - 0.30f,
                    front, deck - 0.70f),
                Curve(
                    rear, belly + 0.18f,
                    rear + length * 0.10f, belly,
                    front - length * 0.14f, belly,
                    front, belly + 0.17f),
                Curve(
                    rear, half * 0.68f,
                    rear + length * 0.12f, half,
                    front - length * 0.14f, half,
                    front, half * 0.56f),
                Curve(
                    rear, half * 0.48f,
                    rear + length * 0.12f, half * 0.72f,
                    front - length * 0.14f, half * 0.70f,
                    front, half * 0.44f),
                Curve(
                    rear, deck - 0.36f,
                    rear + length * 0.12f, deck - 0.22f,
                    front - length * 0.14f, deck - 0.31f,
                    front, deck - 0.57f),
                color * 0.66f);
        }
        private static void BuildCastTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            float width = definition.dims.widthM;
            float roof = TankDetailGeometry.TurretRoofY(definition);
            TankShapeFactory.LathePart(
                "Painted-Primary-" + definition.id + "-CastTurretShell",
                turret,
                new[]
                {
                    width * 0.28f,
                    width * 0.35f,
                    width * 0.38f,
                    width * 0.34f,
                    width * 0.22f,
                    width * 0.08f
                },
                new[]
                {
                    -0.14f,
                    0.04f,
                    roof * 0.38f,
                    roof * 0.68f,
                    roof * 0.88f,
                    roof
                },
                32,
                1.06f,
                color * 0.62f,
                width * 0.39f,
                0.82f);
        }
        private static void BuildAngularTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            float width = definition.dims.widthM;
            float roof = TankDetailGeometry.TurretRoofY(definition);
            float half = width * 0.42f;
            float front = IsLowProfile(definition.id) ? 1.54f : 1.78f;
            float rear = IsRearTurret(definition.id) ? -2.55f : -2.18f;
            Vector2[] plan =
            {
                new Vector2(-0.30f, front),
                new Vector2(0.30f, front),
                new Vector2(half * 0.68f, front * 0.70f),
                new Vector2(half, 0.34f),
                new Vector2(half * 0.96f, rear * 0.62f),
                new Vector2(half * 0.72f, rear),
                new Vector2(-half * 0.72f, rear),
                new Vector2(-half * 0.96f, rear * 0.62f),
                new Vector2(-half, 0.34f),
                new Vector2(-half * 0.68f, front * 0.70f)
            };
            TankShapeFactory.PolyMultiLoftPart(
                "Painted-Primary-" + definition.id + "-TurretShell",
                turret,
                plan,
                new[]
                {
                    new TankShapeLoftRing(
                        -0.08f,
                        1.00f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        roof * 0.42f,
                        0.91f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        roof,
                        IsLowProfile(definition.id) ? 0.56f : 0.61f,
                        new Vector2(
                            0f,
                            IsRearTurret(definition.id) ? -0.20f : -0.10f))
                },
                color * 0.62f);
        }
        private static void BuildBoxTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            float roof = TankDetailGeometry.TurretRoofY(definition);
            Transform shell = TankShapeFactory.BoxPart(
                "Painted-Primary-kv2-BoxTurretShell",
                turret,
                new Vector3(2.75f, Mathf.Max(1.55f, roof), 2.45f),
                color * 0.62f);
            shell.localPosition =
                new Vector3(0f, roof * 0.48f, -0.28f);
            Transform rear = TankShapeFactory.BoxPart(
                "Painted-Primary-kv2-RearTurretWall",
                turret,
                new Vector3(2.60f, roof * 0.76f, 0.42f),
                color * 0.56f);
            rear.localPosition =
                new Vector3(0f, roof * 0.48f, -1.46f);
        }
        private static void BuildGun(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings = TankDetailGeometry.GunFittingsRoot(
                gun,
                "Primary-" + definition.id + "-GunAssembly");
            float length =
                TankAuthoredDetails.ResolveGunLength(definition, 5.0f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(definition);
            float start = definition.id == "kv2" ? 0.72f : 0.96f;
            Transform sleeve = TankShapeFactory.CylinderPart(
                "Painted-Primary-" + definition.id + "-GunRootSleeve",
                fittings,
                radius * 1.55f,
                radius * 1.40f,
                start,
                18,
                TankShapeAxis.Z,
                color * 0.48f);
            sleeve.localPosition =
                new Vector3(0f, 0f, start * 0.5f);
            Transform tube = TankShapeFactory.CylinderPart(
                "Painted-Primary-" + definition.id + "-MainGunTube",
                fittings,
                radius,
                radius * 0.94f,
                Mathf.Max(0.2f, length - start),
                24,
                TankShapeAxis.Z,
                color * 0.46f);
            tube.localPosition =
                new Vector3(0f, 0f, (start + length) * 0.5f);
            Transform bore = TankShapeFactory.CylinderPart(
                "Primary-" + definition.id + "-MuzzleBore",
                fittings,
                radius * 0.58f,
                radius * 0.58f,
                0.045f,
                16,
                TankShapeAxis.Z,
                new Color(0.025f, 0.028f, 0.024f));
            bore.localPosition =
                new Vector3(0f, 0f, length - 0.01f);
        }
        private static bool UsesCastTurret(string id)
        {
            switch (id)
            {
                case "m46_patton":
                case "m47_patton":
                case "m48":
                case "m60a1":
                case "m60a2":
                case "m60a3":
                case "m551_sheridan":
                case "m551a1_tts":
                case "m4a3e8":
                case "t34_85":
                case "is2":
                case "is3":
                case "t34_85_cad":
                case "sherman_jumbo":
                case "t30":
                case "is7":
                case "object279":
                case "is6b":
                case "is1":
                case "t44":
                case "t54":
                case "is3_bergman":
                case "comet":
                case "charioteer":
                case "m26_pershing":
                case "m45_patton":
                    return true;
                default:
                    return false;
            }
        }
        private static bool IsLowProfile(string id)
        {
            return id == "t14" ||
                id == "kf51" ||
                id == "kf51b" ||
                id == "mbt70";
        }
        private static bool IsRearTurret(string id)
        {
            return id.StartsWith("merkava");
        }
        private static bool HasOwnedGun(string id)
        {
            return id == "t14" ||
                id == "kf51" ||
                id == "kf51b" ||
                id == "mbt70" ||
                id == "leo1a5";
        }
        private static bool UsesFixedCasemate(string id)
        {
            return id == "jagdtiger" ||
                id == "jpz_e100" ||
                id == "sturmtiger" ||
                id == "t95" ||
                id == "isu152" ||
                id == "isu122s";
        }
        private static TankHullProfilePoint[] Curve(
            params float[] values)
        {
            TankHullProfilePoint[] curve =
                new TankHullProfilePoint[values.Length / 2];
            for (int index = 0; index < curve.Length; index++)
            {
                curve[index] =
                    new TankHullProfilePoint(
                        values[index * 2],
                        values[index * 2 + 1]);
            }
            return curve;
        }
        private static void HideRenderer(Transform part)
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
