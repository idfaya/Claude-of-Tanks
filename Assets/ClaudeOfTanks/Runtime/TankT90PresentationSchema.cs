using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90PresentationSchema
    {
        private static readonly string[] HiddenArmorNames =
        {
            "Hull",
            "UpperHull",
            "Turret",
            "Gun",
            "Armor-upper_glacis",
            "Armor-lower_front",
            "Armor-hull_side_upper_R",
            "Armor-hull_side_upper_L",
            "Armor-hull_side_lower_R",
            "Armor-hull_side_lower_L",
            "Armor-skirt_rubber_R",
            "Armor-skirt_rubber_L",
            "Armor-track_R",
            "Armor-track_L",
            "Armor-slat_cage",
            "Armor-hull_rear",
            "Armor-hull_roof",
            "Armor-turret_cheek_R",
            "Armor-turret_cheek_L",
            "Armor-mantlet",
            "Armor-turret_side_R",
            "Armor-turret_side_L",
            "Armor-turret_bustle",
            "Armor-turret_roof",
            "Armor-turret_cupola_01_front",
            "Armor-turret_cupola_01_rear",
            "Armor-turret_cupola_01_right",
            "Armor-turret_cupola_01_left",
            "Armor-turret_cupola_01_top"
        };

        private static readonly string[] HiddenPrefixes =
        {
            "Soviet-",
            "Painted-Soviet-",
            "ReturnRoller-"
        };

        public static TankPresentationSchema Create()
        {
            List<TankPresentationPart> parts =
                new List<TankPresentationPart>();
            AddHull(parts);
            AddTurret(parts);
            AddGun(parts);
            return new TankPresentationSchema(
                "T90-PresentationSchema",
                "T90-GunFittings",
                5.15f,
                HiddenArmorNames,
                HiddenPrefixes,
                parts.ToArray());
        }

        private static void AddHull(
            List<TankPresentationPart> parts)
        {
            Add(parts, "Painted-T90-LowerTub", PrimitiveType.Cube,
                TankPresentationTarget.Root, V(0f, 0.76f, -0.06f),
                V(2.28f, 0.74f, 5.82f), V(), TankPresentationColor.Base,
                0.47f);
            Add(parts, "Painted-T90-UpperHull", PrimitiveType.Cube,
                TankPresentationTarget.Root, V(0f, 1.16f, -0.08f),
                V(3.26f, 0.34f, 5.86f), V(), TankPresentationColor.Base,
                0.67f);
            Add(parts, "Painted-T90-SweptGlacis", PrimitiveType.Cube,
                TankPresentationTarget.Root, V(0f, 1.08f, 2.44f),
                V(2.92f, 0.12f, 0.72f), V(-20f, 0f, 0f),
                TankPresentationColor.Base, 0.62f);
            for (int row = 0; row < 2; row++)
            for (int side = -1; side <= 1; side += 2)
            for (int column = 0; column < 5; column++)
            {
                Add(parts, "Painted-T90-K5GlacisBrick",
                    PrimitiveType.Cube, TankPresentationTarget.Root,
                    V(side * (0.24f + column * 0.27f),
                        1.21f + row * 0.14f,
                        2.62f - row * 0.5f),
                    V(0.22f, 0.09f, 0.28f),
                    V(-20f, 0f, side * 8f),
                    TankPresentationColor.Base, 0.54f);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Add(parts, "T90-RecoveryEye", PrimitiveType.Cylinder,
                    TankPresentationTarget.Root,
                    V(side * 0.82f, 0.66f, 3.05f),
                    V(0.08f, 0.02f, 0.08f), V(90f, 0f, 0f),
                    TankPresentationColor.Dark);
                for (int panel = 0; panel < 3; panel++)
                    Add(parts, "Painted-T90-K5SkirtPanel",
                        PrimitiveType.Cube, TankPresentationTarget.Root,
                        V(side * 1.83f, 1.06f, 2.55f - panel * 1.02f),
                        V(0.105f, 0.7f, 0.94f), V(),
                        TankPresentationColor.Base, 0.55f);
                for (int panel = 0; panel < 5; panel++)
                    Add(parts, "Painted-T90-RubberSkirt",
                        PrimitiveType.Cube, TankPresentationTarget.Root,
                        V(side * 1.77f, 0.98f, -1.18f + panel * 0.74f),
                        V(0.04f, 0.72f, 0.58f), V(),
                        TankPresentationColor.Base, 0.38f);
                for (int rail = 0; rail < 6; rail++)
                    Add(parts, "T90-RearQuarterSlat",
                        PrimitiveType.Cube, TankPresentationTarget.Root,
                        V(side * 1.93f, 0.78f + rail * 0.105f, -2.26f),
                        V(0.035f, 0.024f, 1.46f), V(),
                        TankPresentationColor.Dark);
            }
            float[] wheelStations =
                { -1.9f, -1.12f, -0.34f, 0.44f, 1.22f, 2.0f };
            float[] rollerStations =
                { -1.38f, 0.14f, 1.65f };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < wheelStations.Length; index++)
                {
                    Vector3 center =
                        V(side * 1.62f, 0.48f, wheelStations[index]);
                    Add(parts, "T90-RoadWheelInset",
                        PrimitiveType.Cylinder,
                        TankPresentationTarget.Root, center,
                        V(0.32f, 0.045f, 0.32f), V(0f, 0f, 90f),
                        TankPresentationColor.Dark);
                    for (int spoke = 0; spoke < 6; spoke++)
                        Add(parts, "Painted-T90-RoadWheelSpoke",
                            PrimitiveType.Cube, TankPresentationTarget.Root,
                            center, V(0.036f, 0.08f, 0.48f),
                            V(spoke * 30f, 0f, 0f),
                            TankPresentationColor.Base, 0.52f);
                }
                for (int index = 0; index < rollerStations.Length; index++)
                    Add(parts, "T90-ReturnRoller", PrimitiveType.Cylinder,
                        TankPresentationTarget.Root,
                        V(side * 1.48f, 0.82f, rollerStations[index]),
                        V(0.086f, 0.035f, 0.086f), V(0f, 0f, 90f),
                        TankPresentationColor.Base, 0.45f);
            }
            AddDeckAndRear(parts);
        }

        private static void AddDeckAndRear(
            List<TankPresentationPart> parts)
        {
            Add(parts, "Painted-T90-DriverHatch", PrimitiveType.Cube,
                TankPresentationTarget.Root, V(0f, 1.36f, 1.48f),
                V(0.48f, 0.08f, 0.52f), V(), TankPresentationColor.Base,
                0.7f);
            for (int line = 0; line < 5; line++)
                Add(parts, "T90-EngineLouvre", PrimitiveType.Cube,
                    TankPresentationTarget.Root,
                    V(0.12f, 1.42f, -1.34f - line * 0.16f),
                    V(1.52f, 0.018f, 0.055f), V(),
                    TankPresentationColor.Dark);
            for (int side = -1; side <= 1; side += 2)
            {
                Add(parts, "Painted-T90-RearStowageBin",
                    PrimitiveType.Cube, TankPresentationTarget.Root,
                    V(side * 0.6f, 1.5f, -3.26f),
                    V(0.98f, 0.3f, 0.3f), V(),
                    TankPresentationColor.Base, 0.6f);
                Add(parts, "T90-SplitUnditchingLog",
                    PrimitiveType.Cylinder, TankPresentationTarget.Root,
                    V(side * 0.64f, 1.3f, -3.44f),
                    V(0.088f, 0.59f, 0.088f), V(0f, 0f, 90f),
                    TankPresentationColor.Wood);
            }
        }

        private static void AddTurret(
            List<TankPresentationPart> parts)
        {
            Add(parts, "Painted-T90-CastDome", PrimitiveType.Sphere,
                TankPresentationTarget.Turret, V(0f, 0.26f, -0.03f),
                V(3.18f, 0.78f, 2.18f), V(), TankPresentationColor.Base,
                0.64f);
            Add(parts, "Painted-T90-TurretRingCollar",
                PrimitiveType.Cylinder, TankPresentationTarget.Turret,
                V(0f, -0.02f, -0.02f), V(1.58f, 0.05f, 1.58f), V(),
                TankPresentationColor.Base, 0.45f);
            Add(parts, "Painted-T90-MantletTunnel", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0f, 0.22f, 1.0f),
                V(0.54f, 0.3f, 0.6f), V(), TankPresentationColor.Base,
                0.43f);
            AddKontaktAndOptics(parts);
            AddRoofWeaponsAndBustle(parts);
        }

        private static void AddKontaktAndOptics(
            List<TankPresentationPart> parts)
        {
            Add(parts, "Painted-T90-K5RoofPanel", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(-0.72f, 0.69f, 0.08f),
                V(0.72f, 0.06f, 0.74f), V(), TankPresentationColor.Base,
                0.57f);
            Add(parts, "Painted-T90-K5RoofPanel", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0f, 0.76f, -0.16f),
                V(0.76f, 0.06f, 0.86f), V(), TankPresentationColor.Base,
                0.57f);
            Add(parts, "Painted-T90-K5RoofPanel", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0.72f, 0.69f, 0.08f),
                V(0.72f, 0.06f, 0.74f), V(), TankPresentationColor.Base,
                0.57f);
            for (int side = -1; side <= 1; side += 2)
            {
                Add(parts, "Painted-T90-K5CheekLeaf",
                    PrimitiveType.Cube, TankPresentationTarget.Turret,
                    V(side * 0.62f, 0.44f, 1.16f),
                    V(0.7f, 0.34f, 0.48f),
                    V(-25f, -side * 25f, 0f),
                    TankPresentationColor.Base, 0.53f);
                Add(parts, "Painted-T90-K5CheekLeaf",
                    PrimitiveType.Cube, TankPresentationTarget.Turret,
                    V(side * 1.25f, 0.4f, 0.72f),
                    V(0.76f, 0.34f, 0.36f),
                    V(-23f, -side * 41f, 0f),
                    TankPresentationColor.Base, 0.53f);
                Add(parts, "T90-K5CheekSeam", PrimitiveType.Cube,
                    TankPresentationTarget.Turret,
                    V(side * 0.98f, 0.44f, 0.93f),
                    V(0.035f, 0.3f, 0.24f), V(),
                    TankPresentationColor.Dark);
                Add(parts, "Painted-T90-ShtoraHousing",
                    PrimitiveType.Cylinder, TankPresentationTarget.Turret,
                    V(side * 0.52f, 0.5f, 1.3f),
                    V(0.14f, 0.11f, 0.14f), V(90f, 0f, 0f),
                    TankPresentationColor.Base, 0.58f);
                Add(parts, "T90-ShtoraLens", PrimitiveType.Cylinder,
                    TankPresentationTarget.Turret,
                    V(side * 0.52f, 0.5f, 1.43f),
                    V(0.09f, 0.025f, 0.09f), V(90f, 0f, 0f),
                    TankPresentationColor.ShtoraGlass);
            }
            Add(parts, "T90-K5VertexGapPlate", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0f, 0.42f, 1.31f),
                V(0.56f, 0.34f, 0.04f), V(),
                TankPresentationColor.Dark);
            Add(parts, "Painted-T90-1G46SightHousing",
                PrimitiveType.Cube, TankPresentationTarget.Turret,
                V(-0.42f, 0.7f, 0.42f), V(0.26f, 0.15f, 0.3f), V(),
                TankPresentationColor.Base, 0.6f);
            Add(parts, "T90-1G46Lens", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(-0.42f, 0.71f, 0.59f),
                V(0.22f, 0.1f, 0.018f), V(),
                TankPresentationColor.Glass);
        }

        private static void AddRoofWeaponsAndBustle(
            List<TankPresentationPart> parts)
        {
            Add(parts, "Painted-T90-CommanderCupola",
                PrimitiveType.Cylinder, TankPresentationTarget.Turret,
                V(0.52f, 0.7f, -0.3f), V(0.25f, 0.12f, 0.25f), V(),
                TankPresentationColor.Base, 0.6f);
            Add(parts, "T90-NsvtReceiver", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0.6f, 0.91f, -0.42f),
                V(0.26f, 0.18f, 0.46f), V(), TankPresentationColor.Dark);
            Add(parts, "T90-NsvtShield", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0.49f, 0.92f, -0.18f),
                V(0.32f, 0.22f, 0.04f), V(), TankPresentationColor.Dark);
            Add(parts, "T90-NsvtBarrel", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0.64f, 0.94f, 0.2f),
                V(0.04f, 0.04f, 0.76f), V(), TankPresentationColor.Dark);
            Add(parts, "Painted-T90-BustleRack", PrimitiveType.Cube,
                TankPresentationTarget.Turret, V(0f, 0.55f, -1.52f),
                V(1.5f, 0.53f, 0.46f), V(), TankPresentationColor.Base,
                0.58f);
            for (int rail = 0; rail < 5; rail++)
                Add(parts, "T90-BustleRail", PrimitiveType.Cube,
                    TankPresentationTarget.Turret,
                    V(-0.65f + rail * 0.325f, 0.76f, -1.75f),
                    V(0.04f, 0.3f, 0.04f), V(),
                    TankPresentationColor.Dark);
            for (int side = -1; side <= 1; side += 2)
            for (int tube = 0; tube < 4; tube++)
                Add(parts, "Painted-T90-SmokeLauncher",
                    PrimitiveType.Cylinder, TankPresentationTarget.Turret,
                    V(side * (1.02f + tube * 0.06f),
                        0.52f + tube * 0.024f,
                        -0.18f - tube * 0.04f),
                    V(0.04f, 0.13f, 0.04f),
                    V(62f, 0f, side * 18f),
                    TankPresentationColor.Base, 0.52f);
            Add(parts, "T90-OpvtMast", PrimitiveType.Cylinder,
                TankPresentationTarget.Turret, V(0.32f, 0.98f, -1.13f),
                V(0.052f, 0.64f, 0.052f), V(),
                TankPresentationColor.Dark);
            Add(parts, "T90-RadioWhip", PrimitiveType.Cylinder,
                TankPresentationTarget.Turret, V(-0.27f, 1.24f, -1.1f),
                V(0.014f, 1.2f, 0.014f), V(),
                TankPresentationColor.Dark);
            Add(parts, "T90-RadioWhip", PrimitiveType.Cylinder,
                TankPresentationTarget.Turret, V(1.04f, 1.15f, 0.6f),
                V(0.014f, 1.1f, 0.014f), V(),
                TankPresentationColor.Dark);
        }

        private static void AddGun(
            List<TankPresentationPart> parts)
        {
            Add(parts, "Painted-T90-2A46MSaddle",
                PrimitiveType.Cylinder, TankPresentationTarget.GunFittings,
                V(0f, 0.02f, 0.16f), V(0.284f, 0.31f, 0.164f),
                V(90f, 0f, 0f), TankPresentationColor.Base, 0.49f);
            Add(parts, "Painted-T90-2A46MRoot",
                PrimitiveType.Cylinder, TankPresentationTarget.GunFittings,
                V(0f, 0f, 1.02f), V(0.115f, 0.56f, 0.115f),
                V(90f, 0f, 0f), TankPresentationColor.Base, 0.46f);
            Add(parts, "Painted-T90-2A46MEvacuator",
                PrimitiveType.Cylinder, TankPresentationTarget.GunFittings,
                V(0f, 0f, 2.6f), V(0.126f, 0.21f, 0.126f),
                V(90f, 0f, 0f), TankPresentationColor.Base, 0.43f);
            Add(parts, "Painted-T90-2A46MForwardTube",
                PrimitiveType.Cylinder, TankPresentationTarget.GunFittings,
                V(0f, 0f, 3.7f), V(0.104f, 0f, 0.104f),
                V(90f, 0f, 0f), TankPresentationColor.Base, 0.47f,
                TankPresentationLengthMode.Fixed, 0f,
                TankPresentationLengthMode.GunLengthMinus, 2.85f, 0.4f);
            for (int ring = 0; ring < 4; ring++)
                Add(parts, "T90-2A46MSleeveRing",
                    PrimitiveType.Cylinder,
                    TankPresentationTarget.GunFittings,
                    V(0f, 0f, 1.6f + ring * 0.72f),
                    V(0.122f, 0.016f, 0.122f),
                    V(90f, 0f, 0f), TankPresentationColor.Dark);
            Add(parts, "T90-MuzzleBore", PrimitiveType.Cylinder,
                TankPresentationTarget.GunFittings, V(0f, 0f, 0f),
                V(0.066f, 0.009f, 0.066f), V(90f, 0f, 0f),
                TankPresentationColor.Black,
                1f, TankPresentationLengthMode.GunLengthPlus, 0.004f);
        }

        private static void Add(
            List<TankPresentationPart> parts,
            string name,
            PrimitiveType type,
            TankPresentationTarget target,
            Vector3 position,
            Vector3 scale,
            Vector3 rotation,
            TankPresentationColor color,
            float colorMultiplier = 1f,
            TankPresentationLengthMode positionZMode =
                TankPresentationLengthMode.Fixed,
            float positionZValue = 0f,
            TankPresentationLengthMode scaleYMode =
                TankPresentationLengthMode.Fixed,
            float scaleYValue = 0f,
            float scaleYMinimum = 0f)
        {
            parts.Add(new TankPresentationPart(
                name,
                type,
                target,
                position,
                scale,
                rotation,
                color,
                colorMultiplier,
                positionZMode,
                positionZValue,
                scaleYMode,
                scaleYValue,
                scaleYMinimum));
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
