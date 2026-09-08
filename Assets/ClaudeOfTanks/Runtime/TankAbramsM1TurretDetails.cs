using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsM1TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color,
            bool heavyArmor)
        {
            AddShell(turret, color, heavyArmor);
            AddRoof(turret, color, heavyArmor);
            AddWeapons(turret, color, heavyArmor);
            AddRackLoad(turret, color, heavyArmor);
        }

        private static void AddShell(
            Transform turret,
            Color color,
            bool heavyArmor)
        {
            TankDetailGeometry.Part(
                "Painted-AbramsM1-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.12f, 0.12f),
                new Vector3(1.2f, 0.12f, 1.2f),
                color * 0.53f);
            Part("Painted-AbramsM1-TurretCore", turret,
                new Vector3(0f, 0.31f, -0.38f),
                new Vector3(2.75f, 0.68f, 2.72f),
                color * 0.68f);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform cheek = Part(
                    heavyArmor
                        ? "Painted-AbramsM1Ha-Cheek"
                        : "Painted-AbramsM1-Cheek",
                    turret,
                    new Vector3(side * 0.9f, 0.23f, 1.33f),
                    new Vector3(
                        heavyArmor ? 1.48f : 1.38f,
                        0.74f,
                        1.55f),
                    color * (heavyArmor ? 0.61f : 0.64f));
                cheek.localRotation =
                    Quaternion.Euler(0f, side * -18f, 0f);
                Part("Painted-AbramsM1-Flank", turret,
                    new Vector3(side * 1.5f, 0.28f, -0.53f),
                    new Vector3(
                        heavyArmor ? 0.22f : 0.15f,
                        0.62f,
                        2.62f),
                    color * 0.6f);
            }
            Part("Painted-AbramsM1-DeepBustle", turret,
                new Vector3(-0.04f, 0.37f, -2.16f),
                new Vector3(3.0f, 0.72f, 2.05f),
                color * 0.65f);
            Part("Painted-AbramsM1-BlowoffRoof", turret,
                new Vector3(0f, 0.77f, -1.82f),
                new Vector3(2.42f, 0.055f, 1.68f),
                color * 0.72f);
            for (int seam = 0; seam < 4; seam++)
                Part("AbramsM1-BlowoffSeam", turret,
                    new Vector3(-0.88f + seam * 0.58f, 0.804f, -1.82f),
                    new Vector3(0.025f, 0.012f, 1.56f),
                    TankAbramsM1FamilyDetails.Dark());
        }

        private static void AddRoof(
            Transform turret,
            Color color,
            bool heavyArmor)
        {
            AddHatch(turret, color, -0.75f, -0.67f);
            AddHatch(turret, color, 0.7f, -0.35f);
            Part("Painted-AbramsM1-GunnerSight", turret,
                new Vector3(0.78f, 0.72f, 0.83f),
                new Vector3(0.56f, 0.28f, 0.48f),
                color * 0.58f);
            Part("AbramsM1-GunnerSightLens", turret,
                new Vector3(0.78f, 0.67f, 1.09f),
                new Vector3(0.42f, 0.12f, 0.025f),
                TankAbramsM1FamilyDetails.Glass());
            Part("Painted-AbramsM1-CommanderOptic", turret,
                new Vector3(-0.7f, heavyArmor ? 1.04f : 1f, 0.305f),
                new Vector3(
                    heavyArmor ? 0.44f : 0.32f,
                    heavyArmor ? 0.3f : 0.23f,
                    heavyArmor ? 0.32f : 0.27f),
                color * 0.52f);
            if (heavyArmor)
            {
                Part("AbramsM1Ha-CommanderWindow", turret,
                    new Vector3(-0.7f, 1.04f, 0.478f),
                    new Vector3(0.305f, 0.155f, 0.018f),
                    TankAbramsM1FamilyDetails.Glass());
            }
            else
            {
                for (int eye = -1; eye <= 1; eye += 2)
                    Part("AbramsM1-CommanderWindow", turret,
                        new Vector3(-0.7f + eye * 0.075f, 1f, 0.455f),
                        new Vector3(0.105f, 0.085f, 0.018f),
                        TankAbramsM1FamilyDetails.Glass());
            }
            for (int side = -1; side <= 1; side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-AbramsM1-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 1.16f, 0.82f, -2.2f),
                    new Vector3(0.11f, 0.08f, 0.11f),
                    color * 0.52f);
                Transform whip = TankDetailGeometry.Part(
                    "AbramsM1-RadioAntenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 1.16f, 1.2f, -2.2f),
                    new Vector3(0.012f, 0.36f, 0.012f),
                    TankAbramsM1FamilyDetails.Dark());
                whip.localRotation =
                    Quaternion.Euler(0f, 0f, side * -4f);
            }
        }

        private static void AddHatch(
            Transform turret,
            Color color,
            float x,
            float z)
        {
            TankDetailGeometry.Part(
                "Painted-AbramsM1-CrewHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, 0.82f, z),
                new Vector3(0.3f, 0.055f, 0.3f),
                color * 0.69f);
            for (int scope = -1; scope <= 1; scope++)
                Part("AbramsM1-HatchPeriscope", turret,
                    new Vector3(x + scope * 0.15f, 0.89f, z + 0.2f),
                    new Vector3(0.1f, 0.05f, 0.045f),
                    TankAbramsM1FamilyDetails.Glass());
        }

        private static void AddWeapons(
            Transform turret,
            Color color,
            bool heavyArmor)
        {
            Part("Painted-AbramsM1-CommanderMgMount", turret,
                new Vector3(-0.74f, 0.93f, -0.44f),
                new Vector3(0.32f, 0.12f, 0.34f),
                color * 0.55f);
            Part("AbramsM1-CommanderMgReceiver", turret,
                new Vector3(-0.74f, 1.1f, -0.12f),
                new Vector3(0.16f, 0.13f, 0.42f),
                TankAbramsM1FamilyDetails.Gunmetal());
            AddAxial("AbramsM1-CommanderMgBarrel", turret,
                0.018f, 0.82f, 0.5f,
                TankAbramsM1FamilyDetails.Dark(), -0.74f, 1.1f);
            if (heavyArmor)
                Part("Painted-AbramsM1Ha-MgShield", turret,
                    new Vector3(-0.74f, 1.18f, -0.02f),
                    new Vector3(0.5f, 0.32f, 0.08f),
                    color * 0.58f);
            Part("Painted-AbramsM1-LoaderSkate", turret,
                new Vector3(0.72f, 0.9f, -0.2f),
                new Vector3(0.72f, 0.05f, 0.72f),
                color * 0.54f);
            Part("AbramsM1-LoaderMgReceiver", turret,
                new Vector3(0.72f, 1.02f, 0.18f),
                new Vector3(0.13f, 0.11f, 0.36f),
                TankAbramsM1FamilyDetails.Gunmetal());
            AddAxial("AbramsM1-LoaderMgBarrel", turret,
                0.014f, 0.62f, 0.62f,
                TankAbramsM1FamilyDetails.Dark(), 0.72f, 1.02f);
        }

        private static void AddRackLoad(
            Transform turret,
            Color color,
            bool heavyArmor)
        {
            Part("Painted-AbramsM1-RackDuffel", turret,
                new Vector3(-0.62f, 0.64f, -3.05f),
                new Vector3(0.52f, 0.24f, 0.42f),
                color * 0.55f);
            Part(
                heavyArmor
                    ? "Painted-AbramsM1Ha-Bedroll"
                    : "Painted-AbramsM1-Satchel",
                turret,
                new Vector3(-0.62f, 0.8f, -3.05f),
                new Vector3(
                    heavyArmor ? 0.38f : 0.3f,
                    0.1f,
                    heavyArmor ? 0.22f : 0.26f),
                color * 0.48f);
            if (heavyArmor)
                for (int link = 0; link < 3; link++)
                    Part("AbramsM1Ha-SpareTrackLink", turret,
                        new Vector3(0.15f + link * 0.22f, 0.48f, -3.16f),
                        new Vector3(0.18f, 0.09f, 0.32f),
                        TankAbramsM1FamilyDetails.Gunmetal());
        }

        private static void AddAxial(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x,
            float y)
        {
            Transform part = TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(radius, length * 0.5f, radius),
                color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static Transform Part(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                PrimitiveType.Cube,
                parent,
                position,
                scale,
                color);
        }
    }
}
