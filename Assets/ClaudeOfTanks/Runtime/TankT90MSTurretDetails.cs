using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSTurretDetails
    {
        private static readonly TankWeldedStation[] OuterSkinStations =
        {
            S(1.34f, 0.05f, 0.46f,
                -0.33f, 0.33f, -0.25f, 0.25f,
                -0.25f, 0.25f),
            S(1.10f, 0.03f, 0.55f,
                -0.78f, 0.78f, -0.52f, 0.52f,
                -0.62f, 0.62f),
            S(0.70f, 0.12f, 0.60f,
                -1.27f, 1.27f, -0.96f, 0.96f,
                -0.84f, 0.84f),
            S(0.20f, 0.18f, 0.61f,
                -1.55f, 1.55f, -1.30f, 1.30f,
                -0.80f, 0.80f),
            S(-0.35f, 0.18f, 0.60f,
                -1.58f, 1.58f, -1.31f, 1.31f,
                -0.84f, 0.84f),
            S(-0.88f, 0.16f, 0.52f,
                -1.45f, 1.45f, -1.20f, 1.20f,
                -0.80f, 0.80f),
            S(-1.20f, 0.13f, 0.45f,
                -1.27f, 1.27f, -1.06f, 1.06f,
                -0.77f, 0.77f),
            S(-1.48f, 0.12f, 0.38f,
                -1.08f, 1.08f, -0.91f, 0.91f,
                -0.82f, 0.82f),
            S(-1.72f, 0.15f, 0.32f,
                -0.88f, 0.88f, -0.75f, 0.75f,
                -0.70f, 0.70f)
        };

        public static void Build(
            Transform turret,
            Color color,
            string tacticalNumber)
        {
            HideRenderer(turret.Find("Turret"));
            GameObject rootObject =
                new GameObject("T90MS-PresentationRoot");
            Transform root = rootObject.transform;
            root.SetParent(turret, false);
            root.localPosition = V(0f, 0.043f, -0.15f);

            AddInnerShell(root, color);
            AddOuterSkin(root, color);
            AddRing(root);
            AddCrownFacets(root, color);
            TankT90MSTurretArmorDetails.Build(root, color);
            TankT90MSBustleDetails.Build(root, color);
            TankT90MSTurretEquipmentDetails.Build(root, color);
            TankT90MarkingSeats.Build(
                "t90ms",
                root,
                tacticalNumber);
        }

        private static void AddInnerShell(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90MS-InnerWeldedShell",
                root,
                new[]
                {
                    W(-1.929f, 1.619f, 2.117f,
                        -0.723f, 0.715f, -0.715f, 0.715f,
                        -0.723f, 0.709f),
                    W(-1.800f, 1.619f, 2.117f,
                        -0.775f, 0.756f, -0.775f, 0.756f,
                        -0.770f, 0.750f),
                    W(-1.650f, 1.619f, 2.118f,
                        -0.823f, 0.803f, -0.823f, 0.803f,
                        -0.818f, 0.798f),
                    W(-1.500f, 1.619f, 2.118f,
                        -0.870f, 0.851f, -0.870f, 0.851f,
                        -0.865f, 0.845f),
                    W(-1.350f, 1.515f, 2.140f,
                        -0.916f, 0.896f, -0.916f, 0.896f,
                        -0.912f, 0.892f),
                    W(-1.200f, 1.472f, 2.152f,
                        -0.962f, 0.942f, -0.730f, 0.722f,
                        -0.959f, 0.939f),
                    W(-1.050f, 1.443f, 2.186f,
                        -1.008f, 0.988f, -0.886f, 0.884f,
                        -1.006f, 0.986f),
                    W(-0.900f, 1.443f, 2.186f,
                        -1.053f, 1.034f, -1.023f, 0.999f,
                        -1.053f, 1.033f),
                    W(-0.750f, 1.443f, 2.186f,
                        -1.104f, 1.083f, -1.102f, 1.083f,
                        -1.100f, 1.080f),
                    W(-0.600f, 1.443f, 2.186f,
                        -1.154f, 1.135f, -1.153f, 1.135f,
                        -1.147f, 1.127f),
                    W(-0.450f, 1.443f, 2.159f,
                        -1.202f, 1.186f, -1.202f, 1.186f,
                        -1.195f, 1.175f),
                    W(-0.300f, 1.443f, 2.146f,
                        -1.248f, 1.235f, -1.248f, 1.235f,
                        -1.242f, 1.223f),
                    W(-0.150f, 1.443f, 2.133f,
                        -1.295f, 1.285f, -1.295f, 1.285f,
                        -1.289f, 1.263f),
                    W(0f, 1.443f, 2.120f,
                        -1.342f, 1.332f, -1.342f, 1.332f,
                        -0.895f, 1.296f),
                    W(0.150f, 1.443f, 2.107f,
                        -1.389f, 1.375f, -1.213f, 1.218f,
                        -0.775f, 1.240f),
                    W(0.300f, 1.443f, 2.094f,
                        -1.434f, 1.417f, -1.190f, 1.197f,
                        -0.655f, 0.971f),
                    W(0.450f, 1.443f, 2.080f,
                        -1.297f, 1.296f, -1.073f, 1.053f,
                        -0.954f, 0.673f),
                    W(0.600f, 1.443f, 2.065f,
                        -1.140f, 1.140f, -0.900f, 0.865f,
                        -0.417f, 0.676f),
                    W(0.750f, 1.467f, 2.028f,
                        -0.983f, 0.983f, -0.765f, 0.746f,
                        -0.983f, 0.983f),
                    W(0.900f, 1.516f, 1.990f,
                        -0.827f, 0.827f, -0.654f, 0.656f,
                        -0.826f, 0.827f),
                    W(1.040f, 1.643f, 1.955f,
                        -0.412f, 0.412f, -0.412f, 0.411f,
                        -0.412f, 0.412f)
                },
                color * 0.63f);
        }

        private static void AddOuterSkin(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90MS-OuterWeldedSkin",
                root,
                OuterSkinStations,
                color * 0.61f);
        }

        internal static TankWeldedStation OuterSkinStationAt(
            float z)
        {
            if (z >= OuterSkinStations[0].Z)
                return OuterSkinStations[0];
            int last = OuterSkinStations.Length - 1;
            if (z <= OuterSkinStations[last].Z)
                return OuterSkinStations[last];
            for (int index = 0; index < last; index++)
            {
                TankWeldedStation a = OuterSkinStations[index];
                TankWeldedStation b = OuterSkinStations[index + 1];
                if (z > a.Z || z < b.Z) continue;
                float t = (z - a.Z) / (b.Z - a.Z);
                return S(
                    z,
                    Mathf.Lerp(a.BottomY, b.BottomY, t),
                    Mathf.Lerp(a.TopY, b.TopY, t),
                    Mathf.Lerp(a.MiddleLeftX, b.MiddleLeftX, t),
                    Mathf.Lerp(a.MiddleRightX, b.MiddleRightX, t),
                    Mathf.Lerp(a.BottomLeftX, b.BottomLeftX, t),
                    Mathf.Lerp(a.BottomRightX, b.BottomRightX, t),
                    Mathf.Lerp(a.TopLeftX, b.TopLeftX, t),
                    Mathf.Lerp(a.TopRightX, b.TopRightX, t));
            }
            return OuterSkinStations[last];
        }

        private static void AddRing(Transform root)
        {
            Transform ring = TankShapeFactory.CylinderPart(
                "T90MS-BuriedTurretRing",
                root,
                0.76f,
                0.88f,
                0.16f,
                24,
                TankShapeAxis.Y,
                Dark());
            ring.localPosition = V(0f, 0.015f, -0.06f);
        }

        private static void AddCrownFacets(
            Transform root,
            Color color)
        {
            Transform left = Box(
                "Painted-T90MS-CrownFacet",
                root,
                V(-0.55f, 0.615f, -0.30f),
                V(0.72f, 0.075f, 0.92f),
                color * 0.64f);
            left.localRotation = Quaternion.Euler(
                -0.055f * Mathf.Rad2Deg,
                -0.08f * Mathf.Rad2Deg,
                0f);
            Transform center = Box(
                "Painted-T90MS-CrownFacet",
                root,
                V(0.16f, 0.625f, -0.30f),
                V(0.68f, 0.075f, 0.96f),
                color * 0.64f);
            center.localRotation = Quaternion.Euler(
                -0.045f * Mathf.Rad2Deg,
                0.02f * Mathf.Rad2Deg,
                0f);
            Transform right = Box(
                "Painted-T90MS-CrownFacet",
                root,
                V(0.74f, 0.590f, -0.42f),
                V(0.50f, 0.070f, 0.82f),
                color * 0.64f);
            right.localRotation = Quaternion.Euler(
                -0.065f * Mathf.Rad2Deg,
                0.10f * Mathf.Rad2Deg,
                0f);
            foreach (float x in new[] { -0.18f, 0.48f })
            {
                Box("T90MS-CrownWeld", root,
                    V(x, 0.662f, -0.34f),
                    V(0.025f, 0.055f, 0.72f),
                    Dark());
            }
        }

        private static TankWeldedStation W(
            float z,
            float bottomY,
            float topY,
            float middleLeft,
            float middleRight,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            return S(
                z < 0f ? z * 0.82f : z,
                bottomY - 1.443f,
                topY - 1.443f,
                middleLeft,
                middleRight,
                bottomLeft,
                bottomRight,
                topLeft * 0.78f,
                topRight * 0.78f);
        }

        private static TankWeldedStation S(
            float z,
            float bottomY,
            float topY,
            float middleLeft,
            float middleRight,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            return new TankWeldedStation(
                z,
                bottomY,
                topY,
                middleLeft,
                middleRight,
                bottomLeft,
                bottomRight,
                topLeft,
                topRight);
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }
    }
}
