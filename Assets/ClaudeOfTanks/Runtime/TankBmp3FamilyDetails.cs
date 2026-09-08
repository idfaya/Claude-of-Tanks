using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp3FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "bmp3" ||
                id == "bmp3_rok";
        }

        public static bool IsRok(string id)
        {
            return id == "bmp3_rok";
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

            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(root.Find("Armor-track_L"));
            HideRenderer(root.Find("Armor-track_R"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));

            bool rok = IsRok(definition.id);
            if (rok)
            {
                TankBmp2HullDetails.Build(
                    root,
                    color);
                AddRokHullKit(
                    root,
                    color);
            }
            else
            {
                HideNamedRenderers(
                    root,
                    "ReturnRoller-L");
                HideNamedRenderers(
                    root,
                    "ReturnRoller-R");
                TankBmp3HullDetails.Build(
                    root,
                    color);
                AddMainReturnRollers(
                    root);
            }
            TankBmp3TurretDetails.Build(
                turret,
                color,
                rok);
            TankBmp3GunDetails.Build(
                turret,
                definition,
                color,
                rok);
        }

        internal static Color Dark()
        {
            return new Color(
                0.055f,
                0.06f,
                0.052f);
        }

        internal static Color Gunmetal()
        {
            return new Color(
                0.115f,
                0.12f,
                0.105f);
        }

        internal static Color Lens()
        {
            return new Color(
                0.016f,
                0.075f,
                0.09f);
        }

        private static void AddRokHullKit(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 8;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Bmp3Rok-SidePanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.61f,
                            1.26f,
                            2.42f - panel * 0.7f),
                        new Vector3(0.08f, 0.34f, 0.58f),
                        color * 0.66f);
                }
                for (int panel = 0;
                    panel < 3;
                    panel++)
                {
                    Transform glacis =
                        TankDetailGeometry.Part(
                            "Painted-Bmp3Rok-GlacisPanel",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side *
                                    (0.32f +
                                     panel * 0.3f),
                                1.46f -
                                    panel * 0.025f,
                                2.35f -
                                    panel * 0.12f),
                            new Vector3(0.27f, 0.085f, 0.3f),
                            color * 0.72f);
                    glacis.localRotation =
                        Quaternion.Euler(
                            -16f,
                            0f,
                            0f);
                }
                TankDetailGeometry.Part(
                    "Painted-Bmp3Rok-LightPlatform",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.14f,
                        1.3575f,
                        2.87f),
                    new Vector3(0.24f, 0.145f, 0.18f),
                    color * 0.61f);
                TankDetailGeometry.Part(
                    "Bmp3Rok-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 1.14f,
                        1.435f,
                        2.94f),
                    new Vector3(0.09f, 0.09f, 0.065f),
                    Lens());
            }
        }

        private static void AddMainReturnRollers(
            Transform root)
        {
            float[] stations =
            {
                1.3f,
                -0.1f,
                -1.5f
            };
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int index = 0;
                    index < stations.Length;
                    index++)
                {
                    Transform roller =
                        TankDetailGeometry.Part(
                            "Bmp3-ReturnRoller",
                            PrimitiveType.Cylinder,
                            root,
                            new Vector3(
                                side * 1.32f,
                                1.02f,
                                stations[index]),
                            new Vector3(
                                0.07f,
                                0.12f,
                                0.07f),
                            Gunmetal());
                    roller.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static void HideNamedRenderers(
            Transform root,
            string name)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>();
            for (int index = 0;
                index < parts.Length;
                index++)
            {
                if (parts[index].name == name)
                    HideRenderer(parts[index]);
            }
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
