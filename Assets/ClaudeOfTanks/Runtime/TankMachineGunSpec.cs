using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal enum TankMachineGunClass
    {
        M2,
        Heavy,
        Dshk,
        Nsvt,
        Kord,
        Mag,
        Mag58
    }

    internal readonly struct TankMachineGunSpec
    {
        public readonly float Scale;
        public readonly Vector3 Receiver;
        public readonly float BarrelRadius;
        public readonly float BarrelLength;
        public readonly float FlashRadius;
        public readonly float FlashLength;
        public readonly int Jacket;

        private TankMachineGunSpec(
            float scale,
            Vector3 receiver,
            float barrelRadius,
            float barrelLength,
            float flashRadius,
            float flashLength,
            int jacket)
        {
            Scale = scale;
            Receiver = receiver;
            BarrelRadius = barrelRadius;
            BarrelLength = barrelLength;
            FlashRadius = flashRadius;
            FlashLength = flashLength;
            Jacket = jacket;
        }

        public static TankMachineGunSpec Resolve(
            TankMachineGunClass weaponClass)
        {
            switch (weaponClass)
            {
                case TankMachineGunClass.Heavy:
                    return S(
                        1f, 0.115f, 0.095f, 0.46f,
                        0.0165f, 0.52f, 0.021f, 0.07f, 1);
                case TankMachineGunClass.Dshk:
                    return S(
                        1.02f, 0.105f, 0.105f, 0.44f,
                        0.0155f, 0.50f, 0.035f, 0.10f, 2);
                case TankMachineGunClass.Nsvt:
                    return S(
                        0.98f, 0.095f, 0.10f, 0.42f,
                        0.024f, 0.55f, 0.035f, 0.10f, 0);
                case TankMachineGunClass.Kord:
                    return S(
                        0.99f, 0.10f, 0.105f, 0.43f,
                        0.022f, 0.57f, 0.033f, 0.10f, 3);
                case TankMachineGunClass.Mag:
                    return S(
                        0.78f, 0.10f, 0.05f, 0.34f,
                        0.012f, 0.46f, 0.017f, 0.06f, 0);
                case TankMachineGunClass.Mag58:
                    return S(
                        0.80f, 0.105f, 0.052f, 0.35f,
                        0.0125f, 0.47f, 0.018f, 0.06f, 3);
                default:
                    return S(
                        1f, 0.115f, 0.095f, 0.46f,
                        0.0165f, 0.52f, 0.021f, 0.07f, 1);
            }
        }

        private static TankMachineGunSpec S(
            float scale,
            float receiverWidth,
            float receiverHeight,
            float receiverDepth,
            float barrelRadius,
            float barrelLength,
            float flashRadius,
            float flashLength,
            int jacket)
        {
            return new TankMachineGunSpec(
                scale,
                new Vector3(
                    receiverWidth,
                    receiverHeight,
                    receiverDepth),
                barrelRadius,
                barrelLength,
                flashRadius,
                flashLength,
                jacket);
        }
    }
}
