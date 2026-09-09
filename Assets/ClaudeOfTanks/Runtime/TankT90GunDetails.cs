using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90GunDetails
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
                    "T90-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5.15f);

            AddZCylinder(
                "Painted-T90-2A46MSaddle",
                fittings,
                new Vector3(0f, 0.02f, 0.16f),
                0.2f,
                0.62f,
                color * 0.49f,
                new Vector3(1.42f, 0.82f, 1f));
            AddZCylinder(
                "Painted-T90-2A46MRoot",
                fittings,
                new Vector3(0f, 0f, 1.02f),
                0.115f,
                1.12f,
                color * 0.46f);
            AddZCylinder(
                "Painted-T90-2A46MEvacuator",
                fittings,
                new Vector3(0f, 0f, 2.6f),
                0.126f,
                0.42f,
                color * 0.43f);
            AddZCylinder(
                "Painted-T90-2A46MForwardTube",
                fittings,
                new Vector3(0f, 0f, 3.7f),
                0.104f,
                Math.Max(0.4f, length - 2.85f),
                color * 0.47f);
            for (int ring = 0;
                ring < 4;
                ring++)
            {
                AddZCylinder(
                    "T90-2A46MSleeveRing",
                    fittings,
                    new Vector3(0f, 0f, 1.6f + ring * 0.72f),
                    0.122f,
                    0.032f,
                    TankT90FamilyDetails.Dark());
            }
            AddZCylinder(
                "T90-MuzzleBore",
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
