using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBwp1FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "bwp1";
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

            TankBmp2HullDetails.Build(root, color);
            AddPolishHullKit(root, color);
            TankBwp1TurretDetails.Build(turret, color);
            TankBwp1GunDetails.Build(
                turret,
                definition,
                color);
        }

        private static void AddPolishHullKit(
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
                        "Painted-Bwp1-SidePanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.69f,
                            1.14f,
                            2.45f - panel * 0.73f),
                        new Vector3(0.08f, 0.7f, 0.68f),
                        color * 0.67f);
                }
                for (int panel = 0;
                    panel < 5;
                    panel++)
                {
                    Transform glacis =
                        TankDetailGeometry.Part(
                            "Painted-Bwp1-GlacisPanel",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side *
                                    (0.27f +
                                     panel * 0.29f),
                                1.48f,
                                2.3f -
                                    panel * 0.08f),
                            new Vector3(
                                0.25f,
                                0.1f,
                                0.3f),
                            color * 0.72f);
                    glacis.localRotation =
                        Quaternion.Euler(
                            -17.2f,
                            0f,
                            0f);
                }
                TankDetailGeometry.Part(
                    "Painted-Bwp1-LightPlatform",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.14f,
                        1.385f,
                        2.84f),
                    new Vector3(0.24f, 0.16f, 0.18f),
                    color * 0.61f);
                TankDetailGeometry.Part(
                    "Bwp1-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 1.18f,
                        1.515f,
                        2.91f),
                    new Vector3(0.09f, 0.09f, 0.065f),
                    TankBmp2FamilyDetails.Lens());
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
