using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankType89LightTigerDetails
    {
        public static bool Supports(string id)
        {
            return id == "type89_light_tiger";
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
            AddHull(root, color);
            AddTurret(turret, color);
            AddGun(turret, definition, color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            Part("Painted-LightTiger-Tub", root,
                new Vector3(0f, 0.612f, -0.072f),
                new Vector3(1.944f, 0.558f, 5.976f), color * 0.56f);
            Part("Painted-LightTiger-TroopCell", root,
                new Vector3(0f, 1.485f, -0.05f),
                new Vector3(2.75f, 0.58f, 4.55f), color * 0.68f);
            Transform glacis = Part("Painted-LightTiger-PlanarGlacis", root,
                new Vector3(0f, 1.15f, 2.37f),
                new Vector3(2.55f, 0.12f, 1.72f), color * 0.72f);
            glacis.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            Transform lower = Part("Painted-LightTiger-LowerBow", root,
                new Vector3(0f, 0.57f, 2.65f),
                new Vector3(1.82f, 0.12f, 1.05f), color * 0.5f);
            lower.localRotation = Quaternion.Euler(30f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-LightTiger-Shoulder", root,
                    new Vector3(side * 1.29f, 1.53f, -0.3f),
                    new Vector3(0.43f, 0.22f, 5.22f), color * 0.64f);
                for (int cassette = 0; cassette < 9; cassette++)
                {
                    float z = 1.998f - cassette * 0.576f;
                    Part("Painted-LightTiger-SideCassette", root,
                        new Vector3(side * 1.593f, 1.287f, z),
                        new Vector3(0.09f, cassette == 8 ? 0.738f : 0.864f, 0.576f),
                        color * 0.63f);
                    Part("LightTiger-CassetteFace", root,
                        new Vector3(side * 1.706f, 1.287f, z),
                        new Vector3(0.045f, cassette == 8 ? 0.66f : 0.78f, 0.52f),
                        Gunmetal());
                }
                Part("LightTiger-MarkerLamp", root,
                    new Vector3(side * 1.53f, 1.69f, 1.84f),
                    new Vector3(0.08f, 0.12f, 0.16f), Lens());
                Part("Painted-LightTiger-RearBox", root,
                    new Vector3(side * 1.053f, 1.287f, -2.943f),
                    new Vector3(0.297f, 0.378f, 0.27f), color * 0.6f);
            }
            Part("Painted-LightTiger-EngineDeck", root,
                new Vector3(-0.666f, 1.76f, 1.06f),
                new Vector3(1.224f, 0.04f, 1.566f), color * 0.62f);
            for (int slat = 0; slat < 9; slat++)
                Part("LightTiger-EngineLouvre", root,
                    new Vector3(-0.666f, 1.785f, 0.46f + slat * 0.144f),
                    new Vector3(1.062f, 0.016f, 0.045f), Dark());
            AddVertical("Painted-LightTiger-DriverHatch", root,
                0.27f, 0.047f, 0.639f, 1.778f, 1.179f, color * 0.66f);
            Part("Painted-LightTiger-RearRamp", root,
                new Vector3(0f, 1.026f, -3.0735f),
                new Vector3(1.44f, 0.81f, 0.05f), color * 0.58f);
            Part("LightTiger-EmergencyDoor", root,
                new Vector3(0.36f, 1.017f, -3.103f),
                new Vector3(0.576f, 0.63f, 0.018f), Dark());
        }

        private static void AddTurret(
            Transform turret,
            Color color)
        {
            AddVertical("Painted-LightTiger-Bearing", turret,
                0.84f, 0.063f, 0f, -0.018f, -0.018f, color * 0.52f);
            Part("Painted-LightTiger-FacetedTurret", turret,
                new Vector3(0f, 0.324f, -0.18f),
                new Vector3(1.94f, 0.648f, 2.75f), color * 0.69f);
            Part("Painted-LightTiger-FlatMask", turret,
                new Vector3(0f, 0.324f, 1.17f),
                new Vector3(1.458f, 0.558f, 0.162f), color * 0.61f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-LightTiger-JyuMatPod", turret,
                    new Vector3(side * 1.251f, 0.414f, 0.342f),
                    new Vector3(0.414f, 0.414f, 0.558f), color * 0.59f);
                for (int tube = -1; tube <= 1; tube += 2)
                    Part("LightTiger-JyuMatMouth", turret,
                        new Vector3(side * 1.251f, 0.414f + tube * 0.108f, 0.638f),
                        new Vector3(0.252f, 0.126f, 0.023f), Dark());
                Part("Painted-LightTiger-RoofOptic", turret,
                    new Vector3(side * 0.423f, 0.726f, 0.432f),
                    new Vector3(0.342f, 0.264f, 0.324f), color * 0.6f);
                Part("LightTiger-RoofOpticLens", turret,
                    new Vector3(side * 0.423f, 0.726f, 0.609f),
                    new Vector3(0.207f, 0.138f, 0.013f), Lens());
                for (int smoke = 0; smoke < 6; smoke++)
                {
                    Transform tubePart = TankDetailGeometry.Part(
                        "Painted-LightTiger-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(side * (1.02f + smoke * 0.035f),
                            0.48f + smoke * 0.018f, -0.43f - smoke * 0.045f),
                        new Vector3(0.037f, 0.122f, 0.037f), color * 0.45f);
                    tubePart.localRotation = Quaternion.Euler(67f, 0f, side * 18f);
                }
                Part("LightTiger-Camera", turret,
                    new Vector3(side * 0.828f, 0.738f, -0.648f),
                    new Vector3(0.198f, 0.207f, 0.252f), color * 0.52f);
            }
            Part("Painted-LightTiger-PanoramaTower", turret,
                new Vector3(-0.324f, 0.92f, -0.504f),
                new Vector3(0.32f, 0.34f, 0.32f), color * 0.6f);
            Part("LightTiger-PanoramaLens", turret,
                new Vector3(-0.324f, 0.95f, -0.332f),
                new Vector3(0.2f, 0.13f, 0.02f), Lens());
            Part("Painted-LightTiger-RwsMount", turret,
                new Vector3(0.378f, 0.88f, -0.846f),
                new Vector3(0.28f, 0.25f, 0.34f), color * 0.58f);
            Part("LightTiger-RwsReceiver", turret,
                new Vector3(0.378f, 1.03f, -0.72f),
                new Vector3(0.15f, 0.12f, 0.34f), Gunmetal());
            AddAxial("LightTiger-RwsBarrel", turret,
                0.018f, 0.72f, -0.32f, Dark(), 0.378f, 1.03f);
            Part("LightTiger-BustleRack", turret,
                new Vector3(0f, 0.387f, -1.953f),
                new Vector3(1.458f, 0.198f, 0.414f), Gunmetal());
        }

        private static void AddGun(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings = TankDetailGeometry.GunFittingsRoot(
                gun, "LightTiger-GunFittings");
            float length = TankAuthoredDetails.ResolveGunLength(definition, 2.358f);
            Part("Painted-LightTiger-GunMask", fittings,
                new Vector3(0f, 0f, 0.108f),
                new Vector3(0.828f, 0.468f, 0.198f), color * 0.57f);
            AddAxial("Painted-LightTiger-KdeBarrel", fittings,
                0.056f, length - 0.5f, 0.5f + (length - 0.5f) * 0.5f,
                color * 0.4f);
            AddAxial("LightTiger-MuzzleBore", fittings,
                0.034f, 0.022f, length - 0.004f, Color.black);
            AddAxial("LightTiger-CoaxBarrel", fittings,
                0.019f, 1.94f, 1.45f, Dark(), 0.198f, -0.025f);
        }

        private static Transform Part(string name, Transform parent,
            Vector3 position, Vector3 scale, Color color)
        {
            return TankDetailGeometry.Part(
                name, PrimitiveType.Cube, parent, position, scale, color);
        }

        private static void AddVertical(string name, Transform parent,
            float radius, float height, float x, float y, float z, Color color)
        {
            TankDetailGeometry.Part(name, PrimitiveType.Cylinder, parent,
                new Vector3(x, y, z), new Vector3(radius, height, radius), color);
        }

        private static void AddAxial(string name, Transform parent,
            float radius, float length, float z, Color color,
            float x = 0f, float y = 0f)
        {
            Transform part = TankDetailGeometry.Part(name, PrimitiveType.Cylinder,
                parent, new Vector3(x, y, z),
                new Vector3(radius, length * 0.5f, radius), color);
            part.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void Hide(Transform part)
        {
            Renderer renderer = part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private static Color Dark() => new Color(0.045f, 0.055f, 0.045f);
        private static Color Gunmetal() => new Color(0.11f, 0.12f, 0.1f);
        private static Color Lens() => new Color(0.02f, 0.075f, 0.08f);
    }
}
