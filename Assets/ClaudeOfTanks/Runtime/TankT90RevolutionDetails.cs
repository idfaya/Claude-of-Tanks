using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90RevolutionDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            Color color)
        {
            AddUnditchingLogs(root);
            AddRearFuelDrums(root, color);
            AddShtora(turret, color);
            AddCupolas(turret, color);
            AddSmokeBanks(turret, color);
            AddOpvt(turret, color);
            AddAntennas(turret);
        }

        private static void AddUnditchingLogs(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder("T90-SplitUnditchingLog", root,
                    V(side * 0.64f, 1.3f, -3.44f),
                    0.088f, 0.088f, 1.18f, 10,
                    TankShapeAxis.X, Wood());
                Cylinder("T90-UnditchingLogEnd", root,
                    V(side * 1.235f, 1.3f, -3.44f),
                    0.082f, 0.082f, 0.012f, 10,
                    TankShapeAxis.X, Detail());
                Cylinder("T90-UnditchingLogEnd", root,
                    V(side * 0.045f, 1.3f, -3.44f),
                    0.082f, 0.082f, 0.012f, 10,
                    TankShapeAxis.X, Detail());
            }
            foreach (float x in new[] { -0.49f, 0.49f })
                Cylinder("T90-UnditchingLogStrap", root,
                    V(x, 1.3f, -3.44f), 0.094f, 0.094f, 0.04f, 10,
                    TankShapeAxis.X, Dark());
            foreach (float x in new[] { -0.3f, 0.3f })
                Cylinder("T90-UnditchingLogStrap", root,
                    V(x, 1.3f, -3.44f), 0.094f, 0.094f, 0.035f, 10,
                    TankShapeAxis.X, Dark());
        }

        private static void AddRearFuelDrums(
            Transform root,
            Color color)
        {
            AddFuelDrum(root, color, -1.26f, 1.08f, 0.13f, 0.34f);
            AddFuelDrum(root, color, 1.18f, 1.02f, 0.105f, 0.29f);
        }

        private static void AddFuelDrum(
            Transform root,
            Color color,
            float x,
            float y,
            float radius,
            float length)
        {
            Cylinder("Painted-T90-RearFuelDrum", root,
                V(x, y, -3.33f), radius, radius, length, 14,
                TankShapeAxis.Z, color * 0.5f);
            Cylinder("T90-RearFuelDrumBand", root,
                V(x, y, -3.18f), radius + 0.01f, radius + 0.01f,
                0.035f, 14, TankShapeAxis.Z, Dark());
            Cylinder("T90-RearFuelDrumBand", root,
                V(x, y, -3.43f), radius + 0.01f, radius + 0.01f,
                0.035f, 14, TankShapeAxis.Z, Dark());
            Cylinder("T90-RearFuelDrumCap", root,
                V(x, y, -3.56f), radius * 0.72f, radius * 0.72f,
                0.012f, 12, TankShapeAxis.Z, Detail());
            Box("T90-RearFuelDrumCradle", root,
                V(x, y - radius * 0.82f, -3.34f),
                V(radius * 1.75f, 0.045f, 0.1f), Dark());
            Box("T90-RearFuelDrumBracket", root,
                V(x, y - 0.08f, -3.25f),
                V(0.045f, 0.24f, 0.1f), Dark());
        }

        private static void AddShtora(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 0.52f;
                Box("T90-ShtoraHousing", turret,
                    V(x, 0.5f, 1.3f), V(0.24f, 0.27f, 0.22f), Dark());
                Cylinder("T90-ShtoraDrum", turret,
                    V(x, 0.5f, 1.3975f), 0.1f, 0.1f, 0.055f, 16,
                    TankShapeAxis.Z, Dark());
                Cylinder("T90-ShtoraRim", turret,
                    V(x, 0.5f, 1.392f), 0.106f, 0.106f, 0.016f, 16,
                    TankShapeAxis.Z, color * 0.45f);
                Cylinder("T90-ShtoraLens", turret,
                    V(x, 0.5f, 1.423f), 0.072f, 0.072f, 0.014f, 16,
                    TankShapeAxis.Z, ShtoraGlass());
                Box("Painted-T90-ShtoraTop", turret,
                    V(x, 0.655f, 1.31f), V(0.27f, 0.04f, 0.24f),
                    color * 0.52f);
                for (int fin = 0; fin < 3; fin++)
                {
                    Box("T90-ShtoraVentFin", turret,
                        V(x, 0.444f + fin * 0.056f, 1.418f),
                        V(0.19f, 0.024f, 0.014f), Dark());
                }
                for (int edge = -1; edge <= 1; edge += 2)
                {
                    Box("T90-ShtoraSidePlate", turret,
                        V(side * (0.52f + edge * 0.122f), 0.5f, 1.295f),
                        V(0.014f, 0.21f, 0.19f), Detail());
                }
                Box("T90-ShtoraUnderBracket", turret,
                    V(x, 0.345f, 1.255f), V(0.18f, 0.045f, 0.16f),
                    Dark());
            }
        }

        private static void AddCupolas(
            Transform turret,
            Color color)
        {
            Cylinder("Painted-T90-CommanderCupola", turret,
                V(0.52f, 0.7f, -0.3f), 0.25f, 0.27f, 0.16f, 16,
                TankShapeAxis.Y, color * 0.6f);
            Cylinder("T90-CommanderCupolaRim", turret,
                V(0.52f, 0.791f, -0.3f), 0.215f, 0.215f, 0.022f, 14,
                TankShapeAxis.Y, Dark());
            Cylinder("Painted-T90-CommanderCupolaLid", turret,
                V(0.52f, 0.801f, -0.3f), 0.205f, 0.205f, 0.024f, 14,
                TankShapeAxis.Y, color * 0.55f);
            Cylinder("Painted-T90-GunnerHatch", turret,
                V(-0.35f, 0.73f, -0.22f), 0.21f, 0.23f, 0.12f, 14,
                TankShapeAxis.Y, color * 0.55f);
            Cylinder("T90-GunnerHatchRim", turret,
                V(-0.35f, 0.804f, -0.22f), 0.185f, 0.185f, 0.028f, 12,
                TankShapeAxis.Y, Dark());
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject bankObject = new GameObject("T90-SmokeBank");
                Transform bank = bankObject.transform;
                bank.SetParent(turret, false);
                bank.localPosition = V(side * 1.3f, 0.52f, -0.24f);
                bank.localRotation = Quaternion.Euler(
                    0f,
                    side * 1.02f * Mathf.Rad2Deg,
                    0f);
                const int count = 4;
                const float spacing = 0.1f;
                const float splay = 0.3f;
                const float arc = 0.5f;
                Quaternion pitch =
                    Quaternion.Euler(-0.42f * Mathf.Rad2Deg, 0f, 0f);
                for (int tube = 0; tube < count; tube++)
                {
                    float offset = tube - (count - 1) * 0.5f;
                    float yaw = splay + offset * (arc / count);
                    Vector3 position = V(
                        Mathf.Cos(splay) * offset * spacing,
                        0f,
                        -Mathf.Sin(splay) * offset * spacing);
                    Quaternion rotation =
                        pitch *
                        Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f);
                    Cylinder("Painted-T90-SmokeLauncher", bank,
                        position, 0.04f, 0.04f, 0.26f, 8,
                        TankShapeAxis.Z, color * 0.52f, rotation);
                    Cylinder("T90-SmokeLauncherCap", bank,
                        position + rotation * V(0f, 0f, 0.137f),
                        0.0352f, 0.0352f, 0.012f, 8,
                        TankShapeAxis.Z, Dark(), rotation);
                }
                Transform bracket = TankShapeFactory.BoxPart(
                    "T90-SmokeBankBracket",
                    bank,
                    V(0.46f, 0.05f, 0.08f),
                    Dark());
                bracket.localPosition = V(0f, -0.06f, -0.06f);
                bracket.localRotation = Quaternion.Euler(
                    0f,
                    splay * 0.5f * Mathf.Rad2Deg,
                    0f);
            }
        }

        private static void AddOpvt(
            Transform turret,
            Color color)
        {
            Cylinder("T90-OpvtMast", turret,
                V(0.32f, 0.975f, -1.13f), 0.052f, 0.052f, 1.27f, 10,
                TankShapeAxis.Y, color * 0.45f);
            Cylinder("T90-OpvtBaseCollar", turret,
                V(0.32f, 0.4f, -1.13f), 0.058f, 0.058f, 0.05f, 10,
                TankShapeAxis.Y, Dark());
        }

        private static void AddAntennas(Transform turret)
        {
            AddAntenna(turret, -0.27f, 0.5f, -1.098f, 2.38f, 0.016f, 0.02f);
            AddAntenna(turret, 1.04f, 0.5f, 0.6f, 2.2f, 0.016f, 0.02f);
            AddAntenna(turret, -0.48f, 0.47f, -0.18f, 2.5f, 0.013f, -0.018f);
            AddAntenna(turret, 1.16f, 0.47f, -0.8f, 2.33f, 0.013f, 0.018f);
            AddAntenna(turret, 0.38f, 0.47f, -1.07f, 1.18f, 0.013f, 0.018f);
        }

        private static void AddAntenna(
            Transform turret,
            float x,
            float y,
            float z,
            float height,
            float radius,
            float rake)
        {
            GameObject rootObject = new GameObject("T90-AntennaStation");
            Transform root = rootObject.transform;
            root.SetParent(turret, false);
            root.localPosition = V(x, y, z);
            Cylinder("T90-AntennaBase", root,
                V(0f, 0.04f, 0f), 0.035f, 0.045f, 0.08f, 10,
                TankShapeAxis.Y, Dark());
            Cylinder("T90-AntennaCollar", root,
                V(0f, 0.1f, 0f), 0.02f, 0.02f, 0.05f, 8,
                TankShapeAxis.Y, Dark());
            Transform whip = TankShapeFactory.BoxPart(
                "T90-RadioWhip",
                root,
                V(radius * 2f, height, radius * 2f),
                Detail());
            whip.localPosition = V(
                -Mathf.Sin(rake) * height * 0.5f,
                0.12f + Mathf.Cos(rake) * height * 0.5f,
                0f);
            whip.localRotation =
                Quaternion.Euler(0f, 0f, rake * Mathf.Rad2Deg);
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radiusTop,
            float radiusBottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color,
            Quaternion? rotation = null)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radiusTop,
                radiusBottom,
                length,
                segments,
                axis,
                color);
            part.localPosition = position;
            part.localRotation = rotation ?? Quaternion.identity;
            return part;
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part = TankShapeFactory.BoxPart(
                name,
                parent,
                size,
                color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.055f, 0.065f, 0.05f);
        }

        private static Color Detail()
        {
            return new Color(0.2f, 0.23f, 0.18f);
        }

        private static Color Wood()
        {
            return new Color(0.2f, 0.12f, 0.065f);
        }

        private static Color ShtoraGlass()
        {
            return new Color(0.5f, 0.07f, 0.035f);
        }
    }
}
