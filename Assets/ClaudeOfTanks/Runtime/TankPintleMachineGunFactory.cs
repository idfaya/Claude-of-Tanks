using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal enum TankMachineGunShield
    {
        None,
        Standard,
        Low,
        Armored
    }

    internal static class TankPintleMachineGunFactory
    {
        public static Transform Build(
            string prefix,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            TankMachineGunClass weaponClass,
            float scale,
            float elevation,
            bool includeAmmo,
            TankMachineGunShield shield,
            bool barrelBridge,
            Color darkColor,
            Color detailColor)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Machine-gun prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            TankMachineGunSpec spec =
                TankMachineGunSpec.Resolve(weaponClass);
            float s = scale * spec.Scale;
            Transform root = Root(
                prefix,
                parent,
                position,
                rotation);
            AddCradle(prefix, root, s, darkColor);
            float receiverWidth = spec.Receiver.x * s;
            float receiverHeight = spec.Receiver.y * s;
            float receiverDepth = spec.Receiver.z * s;
            float columnHeight = 0.16f * s;
            float columnTop = 0.014f + columnHeight;
            float receiverY =
                columnTop + 0.08f * s + receiverHeight * 0.5f;
            float receiverZ = 0.06f * s;
            AddReceiver(
                prefix,
                root,
                s,
                receiverWidth,
                receiverHeight,
                receiverDepth,
                receiverY,
                receiverZ,
                darkColor);
            float trunnionY = receiverY + 0.004f;
            float trunnionZ = receiverZ + receiverDepth * 0.5f;
            AddBarrel(
                prefix,
                root,
                spec,
                s,
                trunnionY,
                trunnionZ,
                elevation,
                barrelBridge,
                darkColor);
            if (includeAmmo)
            {
                AddAmmo(
                    prefix,
                    root,
                    s,
                    receiverWidth,
                    receiverY,
                    receiverZ,
                    darkColor,
                    detailColor);
            }
            if (shield != TankMachineGunShield.None)
            {
                AddShield(
                    prefix,
                    root,
                    s,
                    receiverY,
                    trunnionZ,
                    shield,
                    darkColor);
            }
            return root;
        }

        private static void AddCradle(
            string prefix,
            Transform root,
            float s,
            Color dark)
        {
            Cylinder(prefix + "-Bearing", root,
                V(0f, 0.007f, 0f),
                0.03f * s, 0.038f * s, 0.014f, 14,
                TankShapeAxis.Y, dark);
            Transform ring = TankShapeFactory.TorusPart(
                prefix + "-BearingRing",
                root,
                0.031f * s,
                0.006f * s,
                18,
                dark);
            ring.localPosition = V(0f, 0.015f, 0f);
            float columnHeight = 0.16f * s;
            Cylinder(prefix + "-Spindle", root,
                V(0f, 0.014f + columnHeight * 0.5f, 0f),
                0.018f * s, 0.023f * s, columnHeight, 12,
                TankShapeAxis.Y, dark);
            float columnTop = 0.014f + columnHeight;
            Box(prefix + "-CradleBridge", root,
                V(0f, columnTop + 0.0225f * s, 0.01f),
                V(0.115f * s, 0.045f * s, 0.15f * s), dark);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform fork = Box(
                    prefix + "-CradleFork",
                    root,
                    V(side * 0.052f * s,
                        columnTop + 0.07f * s,
                        0.045f * s),
                    V(0.02f * s, 0.095f * s, 0.105f * s),
                    dark);
                fork.localRotation =
                    Quaternion.Euler(side * 0.05f * Mathf.Rad2Deg, 0f, 0f);
            }
            Cylinder(prefix + "-Trunnion", root,
                V(0f, columnTop + 0.105f * s, 0.065f * s),
                0.025f * s, 0.025f * s, 0.13f * s, 12,
                TankShapeAxis.X, dark);
        }

        private static void AddReceiver(
            string prefix,
            Transform root,
            float s,
            float width,
            float height,
            float depth,
            float y,
            float z,
            Color dark)
        {
            Box(prefix + "-Receiver", root,
                V(0f, y, z), V(width, height, depth), dark);
            Box(prefix + "-ReceiverCover", root,
                V(0f, y + height * 0.5f + 0.009f * s, z + 0.005f * s),
                V(width * 0.92f, 0.018f * s, depth * 0.88f), dark);
            Box(prefix + "-ReceiverSidePlate", root,
                V(width * 0.5f + 0.01f * s, y, z - 0.025f * s),
                V(0.02f * s, height * 0.70f, depth * 0.54f), dark);
            Box(prefix + "-BufferHead", root,
                V(0f, y - 0.005f * s, z - depth * 0.5f - 0.025f * s),
                V(width * 0.72f, height * 0.58f, 0.05f * s), dark);
            Box(prefix + "-ChargingHandle", root,
                V(-width * 0.5f - 0.026f * s, y + 0.018f * s, z - 0.015f * s),
                V(0.052f * s, 0.017f * s, 0.075f * s), dark);
            Box(prefix + "-RearSight", root,
                V(0f, y + height * 0.5f + 0.03f * s, z - depth * 0.22f),
                V(0.044f * s, 0.045f * s, 0.018f * s), dark);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform grip = Box(
                    prefix + "-SpadeGrip",
                    root,
                    V(side * 0.036f * s,
                        y - 0.012f * s,
                        z - depth * 0.5f - 0.08f * s),
                    V(0.018f * s, 0.026f * s, 0.095f * s),
                    dark);
                grip.localRotation =
                    Quaternion.Euler(side * 0.08f * Mathf.Rad2Deg, 0f, 0f);
                Box(prefix + "-SpadeHandle", root,
                    V(side * 0.045f * s,
                        y - 0.042f * s,
                        z - depth * 0.5f - 0.122f * s),
                    V(0.035f * s, 0.018f * s, 0.018f * s), dark);
            }
            Box(prefix + "-FrontSight", root,
                V(0f, y + height * 0.5f + 0.022f * s, z + depth * 0.28f),
                V(0.012f * s, 0.02f * s, 0.02f * s), dark);
        }

        private static void AddBarrel(
            string prefix,
            Transform root,
            TankMachineGunSpec spec,
            float s,
            float y,
            float z,
            float elevation,
            bool barrelBridge,
            Color dark)
        {
            Transform aim = Root(
                prefix + "-Aim",
                root,
                V(0f, y, z),
                Quaternion.Euler(-elevation * Mathf.Rad2Deg, 0f, 0f));
            if (spec.Jacket == 1)
            {
                Cylinder(prefix + "-Jacket", aim,
                    V(0f, 0f, 0.075f * s),
                    spec.BarrelRadius * s * 1.85f,
                    spec.BarrelRadius * s * 1.85f,
                    0.15f * s, 14, TankShapeAxis.Z, dark);
                for (int index = 0; index < 4; index++)
                {
                    Torus(prefix + "-JacketRing", aim,
                        V(0f, 0f, (0.03f + index * 0.034f) * s),
                        spec.BarrelRadius * s * 1.88f,
                        0.0035f * s, 12, dark);
                }
            }
            else if (spec.Jacket == 2)
            {
                for (int index = 0; index < 5; index++)
                {
                    Cylinder(prefix + "-JacketFin", aim,
                        V(0f, 0f, (0.03f + index * 0.028f) * s),
                        spec.BarrelRadius * s * 1.5f,
                        spec.BarrelRadius * s * 1.5f,
                        0.02f * s, 12, TankShapeAxis.Z, dark);
                }
            }
            else if (spec.Jacket == 3)
            {
                Cylinder(prefix + "-Jacket", aim,
                    V(0f, 0f, 0.065f * s),
                    spec.BarrelRadius * s * 1.32f,
                    spec.BarrelRadius * s * 1.32f,
                    0.13f * s, 12, TankShapeAxis.Z, dark);
                for (int index = 0; index < 4; index++)
                {
                    Torus(prefix + "-JacketRing", aim,
                        V(0f, 0f, (0.026f + index * 0.03f) * s),
                        spec.BarrelRadius * s * 1.34f,
                        0.003f * s, 12, dark);
                }
            }
            else if (barrelBridge)
            {
                Cylinder(prefix + "-BarrelBridge", aim,
                    V(0f, 0f, 0.0525f * s),
                    spec.BarrelRadius * s * 1.12f,
                    spec.BarrelRadius * s * 1.12f,
                    0.105f * s, 10, TankShapeAxis.Z, dark);
            }
            float barrelLength = spec.BarrelLength * s;
            Cylinder(prefix + "-Barrel", aim,
                V(0f, 0f, 0.10f * s + barrelLength * 0.5f),
                spec.BarrelRadius * s,
                spec.BarrelRadius * s,
                barrelLength, 10, TankShapeAxis.Z, dark);
            Cylinder(prefix + "-FlashHider", aim,
                V(0f, 0f,
                    0.10f * s + barrelLength +
                    spec.FlashLength * s * 0.5f),
                spec.FlashRadius * s,
                spec.FlashRadius * s,
                spec.FlashLength * s,
                12, TankShapeAxis.Z, dark);
            Cylinder(prefix + "-MuzzleBore", aim,
                V(0f, 0f,
                    0.10f * s + barrelLength +
                    spec.FlashLength * s + 0.006f),
                spec.BarrelRadius * s * 0.55f,
                spec.BarrelRadius * s * 0.55f,
                0.01f, 10, TankShapeAxis.Z, Color.black);
            Box(prefix + "-BarrelSight", aim,
                V(0f, spec.BarrelRadius * s + 0.014f * s, 0.18f * s),
                V(0.012f * s, 0.026f * s, 0.015f * s), dark);
        }

        private static void AddAmmo(
            string prefix,
            Transform root,
            float s,
            float receiverWidth,
            float receiverY,
            float receiverZ,
            Color dark,
            Color detail)
        {
            float x = -(receiverWidth * 0.5f + 0.055f * s);
            Box(prefix + "-AmmoCan", root,
                V(x, receiverY - 0.005f, receiverZ - 0.02f),
                V(0.085f * s, 0.11f * s, 0.17f * s), detail);
            Box(prefix + "-AmmoCanLid", root,
                V(x, receiverY + 0.058f * s, receiverZ - 0.02f),
                V(0.079f * s, 0.008f * s, 0.158f * s), dark);
            Box(prefix + "-AmmoCanLatch", root,
                V(x - 0.05f * s, receiverY + 0.002f * s, receiverZ - 0.02f),
                V(0.018f * s, 0.06f * s, 0.02f * s), dark);
            for (int index = 0; index < 5; index++)
            {
                float t = index / 4f;
                Transform link = Box(
                    prefix + "-FeedLink",
                    root,
                    V(
                        x * (1f - t) - receiverWidth * 0.36f * t,
                        receiverY + (0.02f + t * 0.012f) * s,
                        receiverZ + (0.065f + t * 0.07f) * s),
                    V(0.018f * s, 0.026f * s, 0.024f * s),
                    dark);
                link.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    (-0.10f + t * 0.16f) * Mathf.Rad2Deg);
            }
        }

        private static void AddShield(
            string prefix,
            Transform root,
            float s,
            float receiverY,
            float trunnionZ,
            TankMachineGunShield variant,
            Color dark)
        {
            float sideWidth =
                variant == TankMachineGunShield.Armored ? 0.18f : 0.145f;
            float height = variant == TankMachineGunShield.Low
                ? 0.14f
                : variant == TankMachineGunShield.Armored
                    ? 0.27f
                    : 0.22f;
            float shieldZ = trunnionZ + 0.035f * s;
            for (int side = -1; side <= 1; side += 2)
            {
                Transform panel = Box(
                    prefix + "-Shield",
                    root,
                    V(side * (0.075f + sideWidth * 0.5f) * s,
                        receiverY + 0.018f * s,
                        shieldZ),
                    V(sideWidth * s, height * s, 0.022f * s),
                    dark);
                panel.localRotation = Quaternion.Euler(
                    0f,
                    -side * 0.055f * Mathf.Rad2Deg,
                    side * 0.035f * Mathf.Rad2Deg);
                Box(prefix + "-ShieldPost", root,
                    V(side * (0.148f + sideWidth * 0.45f) * s,
                        receiverY + 0.006f * s,
                        shieldZ - 0.02f * s),
                    V(0.018f * s, height * 0.82f * s, 0.03f * s),
                    dark);
                Transform brace = Box(
                    prefix + "-ShieldBrace",
                    root,
                    V(side * 0.115f * s,
                        receiverY - height * 0.30f * s,
                        shieldZ - 0.06f * s),
                    V(0.02f * s, 0.02f * s, 0.14f * s),
                    dark);
                brace.localRotation = Quaternion.Euler(
                    -0.22f * Mathf.Rad2Deg,
                    0f,
                    side * 0.08f * Mathf.Rad2Deg);
                Transform edge = Box(
                    prefix + "-ShieldFoldedEdge",
                    root,
                    V(side * (0.075f + sideWidth - 0.012f) * s,
                        receiverY + 0.018f * s,
                        shieldZ - 0.01f * s),
                    V(0.022f * s, height * 0.92f * s, 0.045f * s),
                    dark);
                edge.localRotation = Quaternion.Euler(
                    0f,
                    -side * 0.10f * Mathf.Rad2Deg,
                    0f);
                Transform slot = Box(
                    prefix + "-ShieldVisionSlot",
                    root,
                    V(side * 0.145f * s,
                        receiverY + height * 0.18f * s,
                        shieldZ + 0.014f * s),
                    V(sideWidth * 0.43f * s, 0.032f * s, 0.012f * s),
                    Color.black);
                slot.localRotation = Quaternion.Euler(
                    0f,
                    -side * 0.055f * Mathf.Rad2Deg,
                    0f);
                foreach (float yFactor in new[] { -0.28f, 0.30f })
                {
                    Cylinder(prefix + "-ShieldFastener", root,
                        V(side * (0.075f + sideWidth * 0.70f) * s,
                            receiverY + yFactor * height * s,
                            shieldZ + 0.018f * s),
                        0.009f * s, 0.009f * s, 0.012f * s, 8,
                        TankShapeAxis.Z, dark);
                }
            }
            Box(prefix + "-ShieldTopBridge", root,
                V(0f, receiverY + height * 0.48f * s, shieldZ),
                V(0.19f * s, 0.032f * s, 0.026f * s), dark);
            if (variant == TankMachineGunShield.Armored)
            {
                Box(prefix + "-ShieldArmorRoof", root,
                    V(0f, receiverY + height * 0.58f * s, shieldZ - 0.07f * s),
                    V(0.34f * s, 0.035f * s, 0.18f * s), dark);
            }
        }

        private static Transform Root(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localRotation = rotation;
            return root;
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

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Torus(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float tube,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.TorusPart(
                name, parent, radius, tube, segments, color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }
}
