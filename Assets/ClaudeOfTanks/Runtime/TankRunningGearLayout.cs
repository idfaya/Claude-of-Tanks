using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankRunningGearLayout
    {
        private static readonly float[]
            PumaWheelStations =
            {
                1.791f,
                1.009f,
                0.247f,
                -0.68f,
                -1.43f,
                -2.173f
            };

        private static readonly float[]
            PumaS1WheelStations =
            {
                2.178f,
                1.359f,
                0.531f,
                -0.306f,
                -1.152f,
                -1.98f
            };

        private static readonly float[]
            BradleyWheelStations =
            {
                1.88f,
                1.13f,
                0.38f,
                -0.37f,
                -1.12f,
                -1.87f
            };

        private static readonly float[]
            Bmp2WheelStations =
            {
                1.506f,
                0.786f,
                0.066f,
                -0.654f,
                -1.374f,
                -2.094f
            };

        private static readonly float[]
            Bmp3WheelStations =
            {
                1.79f,
                1.04f,
                0.055f,
                -0.62f,
                -1.315f,
                -2.15f
            };

        public static int RoadWheelCount(
            VehicleDefinition definition,
            float hullLength)
        {
            if (definition?.id == "mbt70" ||
                definition?.id == "t14")
                return 7;
            if (TankPumaFamilyDetails.Supports(
                    definition?.id))
                return 6;
            if (UsesBradleyDonor(definition))
                return 6;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 6;
            if (TankBmp3FamilyDetails.Supports(
                    definition?.id))
                return 6;

            return Mathf.Clamp(
                Mathf.RoundToInt(hullLength * 0.9f),
                4,
                8);
        }

        public static float TrackCenterX(
            VehicleDefinition definition,
            float width)
        {
            if (definition?.id == "spz_puma")
                return 1.25f;
            if (definition?.id == "spz_puma_s1")
                return 1.323f;
            if (UsesBradleyDonor(definition))
                return 1.1475f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 1.205f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 1.205f;
            if (definition?.id == "bmp3")
                return 1.32f;

            return width * 0.47f;
        }

        public static float TrackWidth(
            VehicleDefinition definition,
            float fallback)
        {
            if (UsesBradleyDonor(definition))
                return 0.335f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.3f;
            if (definition?.id == "bmp3")
                return 0.38f;

            return fallback;
        }

        public static float RoadWheelY(
            VehicleDefinition definition,
            float height)
        {
            if (definition?.id == "spz_puma")
                return 0.43f;
            if (definition?.id == "spz_puma_s1")
                return 0.378f;
            if (UsesBradleyDonor(definition))
                return 0.4f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.3f;
            if (definition?.id == "bmp3")
                return 0.37f;

            return Mathf.Max(0.4f, height * 0.21f);
        }

        public static float RoadWheelRadius(
            VehicleDefinition definition,
            float wheelY)
        {
            if (definition?.id == "spz_puma")
                return 0.36f;
            if (definition?.id == "spz_puma_s1")
                return 0.3105f;
            if (UsesBradleyDonor(definition))
                return 0.3f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankBmp3FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;

            return wheelY * 0.72f;
        }

        public static float RoadWheelZ(
            VehicleDefinition definition,
            int index,
            int count,
            float length)
        {
            if (definition?.id == "spz_puma")
                return PumaWheelStations[index];
            if (definition?.id == "spz_puma_s1")
                return PumaS1WheelStations[index];
            if (UsesBradleyDonor(definition))
                return BradleyWheelStations[index];
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return Bmp2WheelStations[index];
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return Bmp2WheelStations[index];
            if (definition?.id == "bmp3")
                return Bmp3WheelStations[index];

            return Mathf.Lerp(
                -length * 0.38f,
                length * 0.38f,
                index / (float)(count - 1));
        }

        public static Vector2 SprocketPosition(
            VehicleDefinition definition,
            float length,
            float wheelY,
            float wheelRadius)
        {
            if (definition?.id == "spz_puma")
                return new Vector2(2.658f, 0.965f);
            if (definition?.id == "spz_puma_s1")
                return new Vector2(2.853f, 0.8685f);
            if (UsesBradleyDonor(definition))
                return new Vector2(2.53f, 0.63f);
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return new Vector2(2.256f, 0.8f);
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return new Vector2(2.256f, 0.8f);
            if (definition?.id == "bmp3")
                return new Vector2(-2.98f, 0.72f);

            return new Vector2(
                -length * 0.45f,
                wheelY + wheelRadius * 0.12f);
        }

        public static Vector2 IdlerPosition(
            VehicleDefinition definition,
            float length,
            float wheelY,
            float wheelRadius)
        {
            if (definition?.id == "spz_puma")
                return new Vector2(-2.814f, 0.84f);
            if (definition?.id == "spz_puma_s1")
                return new Vector2(-2.835f, 0.756f);
            if (UsesBradleyDonor(definition))
                return new Vector2(-2.68f, 0.81f);
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return new Vector2(-2.44f, 0.6f);
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return new Vector2(-2.44f, 0.6f);
            if (definition?.id == "bmp3")
                return new Vector2(2.73f, 0.88f);

            return new Vector2(
                length * 0.45f,
                wheelY + wheelRadius * 0.04f);
        }

        public static float SprocketRadius(
            VehicleDefinition definition,
            float roadWheelRadius)
        {
            if (UsesBradleyDonor(definition))
                return 0.24f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 0.26f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.26f;
            if (definition?.id == "bmp3")
                return 0.35f;

            return roadWheelRadius * 0.82f;
        }

        public static float IdlerRadius(
            VehicleDefinition definition,
            float roadWheelRadius)
        {
            if (UsesBradleyDonor(definition))
                return 0.28f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id))
                return 0.24f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.24f;
            if (definition?.id == "bmp3")
                return 0.29f;

            return roadWheelRadius * 0.82f * 0.94f;
        }

        public static float TrackFrontZ(
            VehicleDefinition definition,
            float length)
        {
            return definition?.id == "bmp3"
                ? 2.4785f
                : length * 0.45f;
        }

        public static float TrackRearZ(
            VehicleDefinition definition,
            float length)
        {
            return definition?.id == "bmp3"
                ? -2.7875f
                : -length * 0.45f;
        }

        public static float TrackBottomY(
            VehicleDefinition definition,
            float wheelY,
            float roadWheelRadius)
        {
            return definition?.id == "bmp3"
                ? 0.01f
                : Mathf.Max(
                    0.06f,
                    wheelY - roadWheelRadius * 0.94f);
        }

        public static float TrackTopY(
            VehicleDefinition definition,
            float wheelY,
            float roadWheelRadius)
        {
            return definition?.id == "bmp3"
                ? 1.18f
                : wheelY + roadWheelRadius * 0.98f;
        }

        private static bool UsesBradleyDonor(
            VehicleDefinition definition)
        {
            string id = definition?.id;
            return
                TankBradleyFamilyDetails.Supports(id) ||
                TankMarder1A3FamilyDetails.Supports(id);
        }
    }
}
