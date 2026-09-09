using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90ATurretRevolutionDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddShtora(turret, color);
            AddCupolas(turret, color);
            AddCrosswindMast(turret);
        }

        private static void AddShtora(
            Transform turret,
            Color color)
        {
            const float scale = 1.32f;
            const float eyeX = 0.7f;
            const float eyeY = 0.38f;
            const float eyeZ = 1.86f;
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * eyeX;
                Box("Painted-T90A-ShtoraHousing", turret,
                    V(x, eyeY, eyeZ),
                    V(0.24f * scale, 0.27f * scale, 0.22f * scale),
                    color * 0.5f);
                Cylinder("T90A-ShtoraDrum", turret,
                    V(x, eyeY, eyeZ + 0.0975f * scale),
                    0.1f * scale,
                    0.1f * scale,
                    0.055f * scale,
                    16,
                    TankT90AFamilyDetails.Dark());
                Cylinder("T90A-ShtoraRim", turret,
                    V(x, eyeY, eyeZ + 0.092f * scale),
                    0.106f * scale,
                    0.106f * scale,
                    0.016f * scale,
                    16,
                    color * 0.45f);
                Cylinder("T90A-ShtoraLens", turret,
                    V(x, eyeY, eyeZ + 0.123f * scale),
                    0.072f * scale,
                    0.072f * scale,
                    0.014f * scale,
                    16,
                    TankT90AFamilyDetails.ShtoraGlass());
                Box("Painted-T90A-ShtoraTop", turret,
                    V(x, eyeY + 0.155f * scale, eyeZ + 0.01f * scale),
                    V(0.27f * scale, 0.04f * scale, 0.24f * scale),
                    color * 0.5f);
                for (int fin = 0; fin < 3; fin++)
                {
                    Box("T90A-ShtoraVentFin", turret,
                        V(
                            x,
                            eyeY + (-0.056f + fin * 0.056f) * scale,
                            eyeZ + 0.118f * scale),
                        V(0.19f * scale, 0.024f * scale, 0.014f * scale),
                        TankT90AFamilyDetails.Dark());
                }
                for (int edge = -1; edge <= 1; edge += 2)
                {
                    Box("T90A-ShtoraSidePlate", turret,
                        V(
                            side * (eyeX + edge * 0.122f * scale),
                            eyeY,
                            eyeZ - 0.005f * scale),
                        V(0.014f * scale, 0.21f * scale, 0.19f * scale),
                        color * 0.45f);
                }
                Box("T90A-ShtoraUnderBracket", turret,
                    V(
                        x,
                        eyeY - 0.155f * scale,
                        eyeZ - 0.045f * scale),
                    V(0.18f * scale, 0.045f * scale, 0.16f * scale),
                    TankT90AFamilyDetails.Dark());
            }
        }

        private static void AddCupolas(
            Transform turret,
            Color color)
        {
            Vector2[] stations =
            {
                new Vector2(-0.35f, -0.48f),
                new Vector2(0.52f, -0.42f)
            };
            for (int index = 0; index < stations.Length; index++)
            {
                float x = stations[index].x;
                float z = stations[index].y;
                Cylinder("Painted-T90A-RoofCupola", turret,
                    V(x, 0.575f, z), 0.255f, 0.285f, 0.18f, 18,
                    color * 0.6f);
                Cylinder("T90A-RoofCupolaRim", turret,
                    V(x, 0.685f, z), 0.235f, 0.255f, 0.055f, 18,
                    TankT90AFamilyDetails.Dark());
                Cylinder("Painted-T90A-RoofCupolaLid", turret,
                    V(x, 0.718f, z), 0.205f, 0.205f, 0.028f, 16,
                    color * 0.55f);
                Box("T90A-CupolaPeriscope", turret,
                    V(x, 0.724f, z - 0.17f),
                    V(0.055f, 0.025f, 0.105f),
                    TankT90AFamilyDetails.Dark());
                foreach (float angle in new[] { -0.62f, 0f, 0.62f })
                {
                    Transform periscope = Box(
                        "T90A-CupolaPeriscope",
                        turret,
                        V(
                            x + Mathf.Sin(angle) * 0.2f,
                            0.69f,
                            z + Mathf.Cos(angle) * 0.2f),
                        V(0.06f, 0.052f, 0.032f),
                        TankT90AFamilyDetails.Dark());
                    periscope.localRotation =
                        Quaternion.Euler(
                            0f,
                            -angle * Mathf.Rad2Deg,
                            0f);
                }
            }
            Box("Painted-T90A-CupolaLampBridge", turret,
                V(0.52f, 0.69f, -0.18f),
                V(0.39f, 0.055f, 0.15f),
                color * 0.5f);
            foreach (float x in new[] { 0.405f, 0.635f })
            {
                Box("Painted-T90A-CupolaLampPod", turret,
                    V(x, 0.76f, -0.105f),
                    V(0.145f, 0.135f, 0.19f),
                    color * 0.5f);
                Cylinder("T90A-CupolaLampDrum", turret,
                    V(x, 0.76f, 0.0025f),
                    0.053f, 0.053f, 0.025f, 14,
                    TankT90AFamilyDetails.Dark(),
                    TankShapeAxis.Z);
                Cylinder("T90A-CupolaLampLens", turret,
                    V(x, 0.76f, 0.021f),
                    0.043f, 0.043f, 0.012f, 14,
                    TankT90AFamilyDetails.Glass(),
                    TankShapeAxis.Z);
            }
        }

        private static void AddCrosswindMast(Transform turret)
        {
            Box("T90A-CrosswindMast", turret,
                V(-0.23f, 0.6825f, -1.28f),
                V(0.044f, 0.445f, 0.044f),
                TankT90AFamilyDetails.Dark());
            Box("T90A-CrosswindMastHead", turret,
                V(-0.23f, 0.875f, -1.28f),
                V(0.06f, 0.06f, 0.06f),
                TankT90AFamilyDetails.Dark());
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radiusTop,
            float radiusBottom,
            float length,
            int segments,
            Color color,
            TankShapeAxis axis = TankShapeAxis.Y)
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
    }
}
