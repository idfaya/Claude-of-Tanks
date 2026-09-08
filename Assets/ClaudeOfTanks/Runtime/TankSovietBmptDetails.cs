using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSovietBmptDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            Color color,
            float width,
            float height)
        {
            HideAuthoritativeGun(turret);
            AddStation(turret, color);
            AddCannons(turret);
            AddMissileRacks(turret, color);
            AddSensors(turret, color);
            AddSmokeProgram(turret, color);
            AddBowGrenadeLaunchers(
                root,
                color,
                width,
                height);
        }

        private static void HideAuthoritativeGun(
            Transform turret)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Renderer renderer =
                gun.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }

        private static void AddStation(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Soviet-BMPT-Turntable",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.06f, -0.06f),
                new Vector3(1.04f, 0.14f, 1.04f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Soviet-BMPT-WeaponStation",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.62f, 0.1f),
                new Vector3(0.96f, 0.52f, 1.52f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-Soviet-BMPT-RoofStep",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.92f, 0.06f),
                new Vector3(0.86f, 0.08f, 1.34f),
                color * 0.8f);
        }

        private static void AddCannons(
            Transform turret)
        {
            const float visibleLength = 2.75f;
            const float startZ = 0.6f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Soviet-BMPT-Cannon",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.2f,
                        0.32f,
                        startZ +
                            visibleLength * 0.5f),
                    new Vector3(
                        0.082f,
                        0.082f,
                        visibleLength),
                    new Color(
                        0.1f,
                        0.11f,
                        0.09f));
                TankDetailGeometry.Part(
                    "Soviet-BMPT-Muzzle",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.2f,
                        0.32f,
                        3.28f),
                    new Vector3(
                        0.116f,
                        0.116f,
                        0.2f),
                    new Color(
                        0.07f,
                        0.08f,
                        0.07f));
            }
        }

        private static void AddMissileRacks(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int web = 0;
                    web < 2;
                    web++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Soviet-BMPT-RackWeb",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                (0.9f +
                                 web * 0.23f),
                            0.47f,
                            0.2f),
                        new Vector3(
                            0.06f,
                            0.4f,
                            0.48f),
                        color * 0.64f);
                }
                for (int column = 0;
                    column < 2;
                    column++)
                {
                    for (int row = 0;
                        row < 2;
                        row++)
                    {
                        Transform missile =
                            TankDetailGeometry.Part(
                                "Painted-Soviet-BMPT-MissileTube",
                                PrimitiveType.Cylinder,
                                turret,
                                new Vector3(
                                    side *
                                        (1.005f +
                                         column * 0.24f),
                                    0.35f +
                                        row * 0.25f,
                                    0.26f),
                                new Vector3(
                                    0.085f,
                                    0.425f,
                                    0.085f),
                                color * 0.58f);
                        missile.localRotation =
                            Quaternion.Euler(
                                90f,
                                0f,
                                0f);
                    }
                }
            }
        }

        private static void AddSensors(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Soviet-BMPT-PanoramicPost",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.36f, 1.16f, -0.34f),
                new Vector3(0.13f, 0.3f, 0.13f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Painted-Soviet-BMPT-PanoramicHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.36f, 1.37f, -0.33f),
                new Vector3(0.3f, 0.2f, 0.28f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Soviet-BMPT-PanoramicLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.36f, 1.38f, -0.185f),
                new Vector3(0.2f, 0.12f, 0.024f),
                new Color(
                    0.035f,
                    0.07f,
                    0.075f));
            TankDetailGeometry.Part(
                "Soviet-BMPT-MetMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.4f, 1.19f, -0.52f),
                new Vector3(0.022f, 0.26f, 0.022f),
                color * 0.34f);
        }

        private static void AddSmokeProgram(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                AddSmokeBank(
                    turret,
                    color,
                    side,
                    0.84f,
                    4,
                    0.78f,
                    0.52f);
                AddSmokeBank(
                    turret,
                    color,
                    side,
                    -0.76f,
                    3,
                    0.76f,
                    0.5f);
            }
        }

        private static void AddSmokeBank(
            Transform turret,
            Color color,
            int side,
            float z,
            int count,
            float x,
            float y)
        {
            for (int tube = 0;
                tube < count;
                tube++)
            {
                Transform launcher =
                    TankDetailGeometry.Part(
                        "Painted-Soviet-BMPT-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side *
                                (x +
                                 tube * 0.045f),
                            y +
                                tube * 0.025f,
                            z -
                                tube * 0.035f),
                        new Vector3(
                            0.04f,
                            0.12f,
                            0.04f),
                        color * 0.52f);
                launcher.localRotation =
                    Quaternion.Euler(
                        62f,
                        0f,
                        side * 18f);
            }
        }

        private static void AddBowGrenadeLaunchers(
            Transform root,
            Color color,
            float width,
            float height)
        {
            float x = Mathf.Min(
                1.28f,
                width * 0.34f);
            float y = Mathf.Min(
                1.4f,
                height * 0.49f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Soviet-BMPT-GrenadePod",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * x,
                        y,
                        2.53f),
                    new Vector3(
                        0.24f,
                        0.2f,
                        0.36f),
                    color * 0.68f);
                Transform barrel =
                    TankDetailGeometry.Part(
                        "Soviet-BMPT-GrenadeBarrel",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * x,
                            y + 0.04f,
                            2.79f),
                        new Vector3(
                            0.034f,
                            0.15f,
                            0.034f),
                        new Color(
                            0.08f,
                            0.09f,
                            0.08f));
                barrel.localRotation =
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f);
            }
        }
    }
}
