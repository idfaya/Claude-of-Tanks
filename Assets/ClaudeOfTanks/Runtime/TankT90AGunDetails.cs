using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AGunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "T90A-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5.65f);

            AddZCylinder(
                "Painted-T90A-CastGunCollar",
                fittings,
                new Vector3(0f, 0.02f, 0.16f),
                0.21f,
                0.3f,
                color * 0.49f,
                new Vector3(1.45f, 0.82f, 1f));
            AddZCylinder(
                "Painted-T90A-2A46M2RootSleeve",
                fittings,
                new Vector3(0f, 0f, 0.98f),
                0.126f,
                0.98f,
                color * 0.46f);
            AddZCylinder(
                "Painted-T90A-RecoilCover",
                fittings,
                new Vector3(0f, 0.04f, 1.46f),
                0.15f,
                0.56f,
                color * 0.44f,
                new Vector3(1.3f, 0.72f, 1f));
            AddZCylinder(
                "Painted-T90A-2A46M2Evacuator",
                fittings,
                new Vector3(0f, 0f, 2.88f),
                0.13f,
                0.46f,
                color * 0.43f);
            AddZCylinder(
                "Painted-T90A-2A46M2ForwardTube",
                fittings,
                new Vector3(0f, 0f, 3.72f),
                0.106f,
                Math.Max(0.4f, length - 2.85f),
                color * 0.47f);
            for (int ring = 0;
                ring < 5;
                ring++)
            {
                AddZCylinder(
                    "T90A-2A46M2SleeveRing",
                    fittings,
                    new Vector3(0f, 0f, 1.47f + ring * 0.62f),
                    0.122f,
                    0.03f,
                    TankT90AFamilyDetails.Dark());
            }
            AddZCylinder(
                "T90A-CoaxPort",
                fittings,
                new Vector3(0.2f, 0.07f, 0.27f),
                0.03f,
                0.018f,
                TankT90AFamilyDetails.Dark());
            AddZCylinder(
                "T90A-MuzzleBore",
                fittings,
                new Vector3(0f, 0f, length + 0.004f),
                0.066f,
                0.018f,
                Color.black);
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            Color color)
        {
            AddZCylinder(
                name,
                parent,
                position,
                radius,
                length,
                color,
                Vector3.one);
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            Color color,
            Vector3 scale)
        {
            Transform part = TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                position,
                new Vector3(
                    radius * scale.x,
                    length * 0.5f * scale.z,
                    radius * scale.y),
                color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
