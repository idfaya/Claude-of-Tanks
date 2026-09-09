using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankRunningGearLayout
    {
        public static int RoadWheelCount(
            VehicleDefinition definition,
            float hullLength)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 7;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 7;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return 6;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 6;
            if (definition?.id == "mbt70" ||
                definition?.id == "t14")
                return 7;
            if (TankPumaFamilyDetails.Supports(
                    definition?.id))
                return 6;
            if (UsesBradleyDonor(definition))
                return 6;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 6;
            if (TankBmp3FamilyDetails.Supports(
                    definition?.id))
                return 6;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 6;
            if (TankBmpt2FamilyDetails.Supports(
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
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 1.46f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 1.425f;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return definition.id == "type89" ? 1.25f : 1.278f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 1.276f;
            if (definition?.id == "spz_puma")
                return 1.25f;
            if (definition?.id == "spz_puma_s1")
                return 1.323f;
            if (UsesBradleyDonor(definition))
                return 1.1475f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 1.205f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 1.205f;
            if (definition?.id == "bmp3")
                return 1.32f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.94f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 1.33f;

            return width * 0.47f;
        }

        public static float TrackWidth(
            VehicleDefinition definition,
            float fallback)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 0.64f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 0.58f;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return definition.id == "type89" ? 0.44f : 0.414f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 0.506f;
            if (UsesBradleyDonor(definition))
                return 0.335f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.3f;
            if (definition?.id == "bmp3")
                return 0.38f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.36f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 0.58f;

            return fallback;
        }

        public static float RoadWheelY(
            VehicleDefinition definition,
            float height)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 0.43f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 0.42f;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return definition.id == "type89" ? 0.4f : 0.3645f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 0.44f;
            if (definition?.id == "spz_puma")
                return 0.43f;
            if (definition?.id == "spz_puma_s1")
                return 0.378f;
            if (UsesBradleyDonor(definition))
                return 0.4f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.3f;
            if (definition?.id == "bmp3")
                return 0.37f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.29f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 0.46f;

            return Mathf.Max(0.4f, height * 0.21f);
        }

        public static float RoadWheelRadius(
            VehicleDefinition definition,
            float wheelY)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 0.32f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 0.31f;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return definition.id == "type89" ? 0.32f : 0.2925f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 0.42f;
            if (definition?.id == "spz_puma")
                return 0.36f;
            if (definition?.id == "spz_puma_s1")
                return 0.3105f;
            if (UsesBradleyDonor(definition))
                return 0.3f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankBmp3FamilyDetails.Supports(
                    definition?.id))
                return 0.3f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.235f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 0.375f;

            return wheelY * 0.72f;
        }

        public static float RoadWheelZ(
            VehicleDefinition definition,
            int index,
            int count,
            float length)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id))
                return TankRunningGearStations.M1A3At(index);
            if (TankAbramsM1FamilyDetails.Supports(definition?.id))
                return TankRunningGearStations.AbramsM1At(index);
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id))
                return TankType89RunningGearLayout.RoadWheelZ(definition.id, index);
            if (TankWarriorFamilyDetails.Supports(definition?.id))
                return TankWarriorRunningGearLayout.RoadWheelZ(index);
            if (definition?.id == "spz_puma")
                return TankRunningGearStations.PumaAt(index);
            if (definition?.id == "spz_puma_s1")
                return TankRunningGearStations.PumaS1At(index);
            if (UsesBradleyDonor(definition))
                return TankRunningGearStations.BradleyAt(index);
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return TankRunningGearStations.Bmp2At(index);
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return TankRunningGearStations.Bmp2At(index);
            if (definition?.id == "bmp3")
                return TankRunningGearStations.Bmp3At(index);
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return TankRunningGearStations.UpiorAt(index);
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return TankRunningGearStations.Bmpt2At(index);

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
            if (TankM1A3FamilyDetails.Supports(definition?.id))
                return new Vector2(-3.42f, 0.96f);
            if (TankAbramsM1FamilyDetails.Supports(definition?.id))
                return new Vector2(-3.28f, 1.1f);
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id))
                return TankType89RunningGearLayout.Sprocket(definition.id);
            if (TankWarriorFamilyDetails.Supports(definition?.id))
                return TankWarriorRunningGearLayout.Sprocket();
            if (definition?.id == "spz_puma")
                return new Vector2(2.658f, 0.965f);
            if (definition?.id == "spz_puma_s1")
                return new Vector2(2.853f, 0.8685f);
            if (UsesBradleyDonor(definition))
                return new Vector2(2.53f, 0.63f);
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return new Vector2(2.256f, 0.8f);
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return new Vector2(2.256f, 0.8f);
            if (definition?.id == "bmp3")
                return new Vector2(-2.98f, 0.72f);
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return new Vector2(-2.1f, 0.5f);
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return new Vector2(-3.417f, 0.75f);

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
            if (TankM1A3FamilyDetails.Supports(definition?.id))
                return new Vector2(3.27f, 0.88f);
            if (TankAbramsM1FamilyDetails.Supports(definition?.id))
                return new Vector2(3.02f, 0.85f);
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id))
                return TankType89RunningGearLayout.Idler(definition.id);
            if (TankWarriorFamilyDetails.Supports(definition?.id))
                return TankWarriorRunningGearLayout.Idler();
            if (definition?.id == "spz_puma")
                return new Vector2(-2.814f, 0.84f);
            if (definition?.id == "spz_puma_s1")
                return new Vector2(-2.835f, 0.756f);
            if (UsesBradleyDonor(definition))
                return new Vector2(-2.68f, 0.81f);
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return new Vector2(-2.44f, 0.6f);
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return new Vector2(-2.44f, 0.6f);
            if (definition?.id == "bmp3")
                return new Vector2(2.73f, 0.88f);
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return new Vector2(2.2f, 0.58f);
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return new Vector2(1.463f, 0.62f);

            return new Vector2(
                length * 0.45f,
                wheelY + wheelRadius * 0.04f);
        }

        public static float SprocketRadius(
            VehicleDefinition definition,
            float roadWheelRadius)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 0.35f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 0.32f;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return definition.id == "type89" ? 0.26f : 0.288f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 0.394f;
            if (UsesBradleyDonor(definition))
                return 0.24f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 0.26f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.26f;
            if (definition?.id == "bmp3")
                return 0.35f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.18f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 0.28f;

            return roadWheelRadius * 0.82f;
        }

        public static float IdlerRadius(
            VehicleDefinition definition,
            float roadWheelRadius)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 0.35f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 0.34f;
            if (TankType89FamilyDetails.SupportsRunningGear(definition?.id)) return definition.id == "type89" ? 0.27f : 0.261f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 0.373f;
            if (UsesBradleyDonor(definition))
                return 0.28f;
            if (TankBmp2FamilyDetails.Supports(
                    definition?.id) ||
                TankBwp1FamilyDetails.Supports(
                    definition?.id))
                return 0.24f;
            if (TankBmp3FamilyDetails.IsRok(
                    definition?.id))
                return 0.24f;
            if (definition?.id == "bmp3")
                return 0.29f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.18f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 0.24f;

            return roadWheelRadius * 0.82f * 0.94f;
        }

        public static float TrackFrontZ(
            VehicleDefinition definition,
            float length)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 2.9996f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 2.6839f;
            if (definition?.id == "type89") return 2.3847f;
            if (definition?.id == "type89_light_tiger") return 2.5317f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 2.6407f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 2.0384f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 1.0981f;
            return definition?.id == "bmp3"
                ? 2.4785f
                : length * 0.45f;
        }

        public static float TrackRearZ(
            VehicleDefinition definition,
            float length)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return -3.1496f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return -2.9224f;
            if (definition?.id == "type89") return -2.4906f;
            if (definition?.id == "type89_light_tiger") return -2.4722f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return -2.5857f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return -1.9385f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return -3.2535f;
            return definition?.id == "bmp3"
                ? -2.7875f
                : -length * 0.45f;
        }

        public static float TrackBottomY(
            VehicleDefinition definition,
            float wheelY,
            float roadWheelRadius)
        {
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 0.063f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 0.059f;
            if (definition?.id == "type89") return 0.0568f;
            if (definition?.id == "type89_light_tiger") return 0.0733f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 0.097f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.045f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 0.0692f;
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
            if (TankM1A3FamilyDetails.Supports(definition?.id)) return 1.3319f;
            if (TankAbramsM1FamilyDetails.Supports(definition?.id)) return 1.448f;
            if (definition?.id == "type89") return 1.3478f;
            if (definition?.id == "type89_light_tiger") return 1.0558f;
            if (TankWarriorFamilyDetails.Supports(definition?.id)) return 1.123f;
            if (TankUpiorFamilyDetails.Supports(
                    definition?.id))
                return 0.82f;
            if (TankBmpt2FamilyDetails.Supports(
                    definition?.id))
                return 1.0229f;
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
