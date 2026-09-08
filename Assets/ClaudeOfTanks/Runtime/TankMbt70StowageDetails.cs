using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMbt70StowageDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddRearBasket(
                turret,
                color,
                width);
            AddServiceKit(
                turret,
                color,
                roof);
            AddAntennas(
                turret,
                color,
                roof);
        }

        private static void AddRearBasket(
            Transform turret,
            Color color,
            float width)
        {
            const float rear = -3.06f;
            for (int rail = 0;
                rail < 2;
                rail++)
            {
                TankDetailGeometry.Part(
                    "MBT70-BasketRearRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0.19f + rail * 0.51f,
                        rear),
                    new Vector3(3.2f, 0.05f, 0.05f),
                    color * 0.4f);
            }
            for (int post = 0;
                post < 13;
                post++)
            {
                TankDetailGeometry.Part(
                    "MBT70-BasketPost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -1.5f + post * 0.25f,
                        0.45f,
                        rear),
                    new Vector3(0.035f, 0.5f, 0.035f),
                    color * 0.4f);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float rackX = side * 0.68f;
                TankDetailGeometry.Part(
                    "MBT70-BustleRackFloor",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(rackX, 0.205f, -2.84f),
                    new Vector3(1.26f, 0.035f, 0.38f),
                    color * 0.42f);
                for (int rail = 0;
                    rail < 3;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "MBT70-BustleRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            rackX -
                                0.52f +
                                rail * 0.52f,
                            0.35f,
                            -2.84f),
                        new Vector3(0.035f, 0.28f, 0.38f),
                        color * 0.4f);
                }
                for (int cargo = 0;
                    cargo < 2;
                    cargo++)
                {
                    TankDetailGeometry.Part(
                        "Painted-MBT70-BustleCargo",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            rackX +
                                (cargo == 0 ? -0.29f : 0.29f),
                            0.33f,
                            -2.84f),
                        new Vector3(0.48f, 0.22f, 0.3f),
                        color * (0.57f +
                            cargo * 0.05f));
                }
            }
            for (int can = 0;
                can < 2;
                can++)
            {
                float x = -1.04f + can * 0.24f;
                TankDetailGeometry.Part(
                    "Painted-MBT70-BustleJerryCan",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.36f, -2.76f),
                    new Vector3(0.2f, 0.3f, 0.18f),
                    color * 0.7f);
                TankDetailGeometry.Part(
                    "MBT70-BustleJerryCanRib",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.36f, -2.855f),
                    new Vector3(0.025f, 0.23f, 0.014f),
                    TankMbt70FamilyDetails.Gunmetal())
                    .localRotation =
                    Quaternion.Euler(0f, 0f, 36f);
            }
            AddCableSegment(
                turret,
                new Vector3(-1.18f, 0.73f, -2.4f),
                new Vector3(0f, 0.77f, -2.58f));
            AddCableSegment(
                turret,
                new Vector3(0f, 0.77f, -2.58f),
                new Vector3(1.18f, 0.73f, -2.4f));
            TankDetailGeometry.Part(
                "MBT70-BasketWidthRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.19f, -2.66f),
                new Vector3(width * 0.82f, 0.035f, 0.035f),
                color * 0.39f);
        }

        private static void AddCableSegment(
            Transform turret,
            Vector3 start,
            Vector3 end)
        {
            Vector3 delta = end - start;
            Transform segment =
                TankDetailGeometry.Part(
                    "MBT70-BustleTowCable",
                    PrimitiveType.Cylinder,
                    turret,
                    (start + end) * 0.5f,
                    new Vector3(
                        0.018f,
                        delta.magnitude * 0.5f,
                        0.018f),
                    TankMbt70FamilyDetails.Gunmetal());
            segment.localRotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    delta.normalized);
        }

        private static void AddServiceKit(
            Transform turret,
            Color color,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-MBT70-SideLocker",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.47f,
                        0.49f,
                        -1.25f),
                    new Vector3(0.24f, 0.18f, 0.38f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "Painted-MBT70-SideLocker",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.44f,
                        0.52f,
                        -1.72f),
                    new Vector3(0.22f, 0.16f, 0.34f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "MBT70-CableRaceway",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.18f,
                        roof + 0.04f,
                        -1.52f),
                    new Vector3(0.055f, 0.055f, 0.64f),
                    TankMbt70FamilyDetails.Gunmetal());
                for (int link = 0;
                    link < 4;
                    link++)
                {
                    TankDetailGeometry.Part(
                        "MBT70-SpareTrackLink",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 1.641f,
                            0.32f + link * 0.1f,
                            -2.44f),
                        new Vector3(0.08f, 0.07f, 0.42f),
                        TankMbt70FamilyDetails.Gunmetal());
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            Color color,
            float roof)
        {
            float[] heights =
            {
                0.78f,
                0.92f
            };
            for (int index = 0;
                index < heights.Length;
                index++)
            {
                int side = index == 0 ? -1 : 1;
                TankDetailGeometry.Part(
                    "Painted-MBT70-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * 0.94f,
                        roof + 0.04f,
                        -1.34f),
                    new Vector3(0.05f, 0.05f, 0.05f),
                    color * 0.52f);
                Transform whip =
                    TankDetailGeometry.Part(
                        "MBT70-Antenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * 0.94f,
                            roof + heights[index] * 0.5f,
                            -1.34f),
                        new Vector3(
                            0.012f,
                            heights[index] * 0.5f,
                            0.012f),
                        TankMbt70FamilyDetails.Gunmetal());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * -2.3f);
            }
        }
    }
}
