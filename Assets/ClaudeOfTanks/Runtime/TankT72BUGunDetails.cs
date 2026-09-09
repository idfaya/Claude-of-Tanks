using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72BUGunDetails
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
                    "T72BU-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5.56f);

            AddZCylinder(
                "Painted-T72BU-2A46M4Saddle",
                fittings,
                new Vector3(0f, -0.02f, 0.18f),
                0.19f,
                0.56f,
                color * 0.5f,
                new Vector3(1.35f, 0.82f, 1f));
            AddZCylinder(
                "Painted-T72BU-2A46M4Root",
                fittings,
                new Vector3(0f, 0f, 0.98f),
                0.11f,
                1.0f,
                color * 0.46f);
            AddZCylinder(
                "Painted-T72BU-2A46M4Evacuator",
                fittings,
                new Vector3(0f, 0f, 2.82f),
                0.132f,
                0.78f,
                color * 0.43f);
            AddZCylinder(
                "Painted-T72BU-2A46M4ForwardTube",
                fittings,
                new Vector3(0f, 0f, 3.85f),
                0.117f,
                Math.Max(0.4f, length - 3.0f),
                color * 0.47f);
            for (int ring = 0;
                ring < 4;
                ring++)
            {
                AddZCylinder(
                    "T72BU-2A46M4SleeveRing",
                    fittings,
                    new Vector3(0f, 0f, 1.9f + ring * 0.78f),
                    0.122f,
                    0.032f,
                    TankT72BUFamilyDetails.Dark());
            }
            AddZCylinder(
                "T72BU-MuzzleBore",
                fittings,
                new Vector3(0f, 0f, length + 0.004f),
                0.068f,
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
