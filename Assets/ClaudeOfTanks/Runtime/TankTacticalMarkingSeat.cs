using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal enum TankTacticalMarkingKind
    {
        Insignia,
        Designation
    }

    internal readonly struct TankTacticalMarkingSeat
    {
        public readonly string Name;
        public readonly TankTacticalMarkingKind Kind;
        public readonly float Size;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public TankTacticalMarkingSeat(
            string name,
            TankTacticalMarkingKind kind,
            float size,
            Vector3 position,
            Quaternion rotation)
        {
            Name = name;
            Kind = kind;
            Size = size;
            Position = position;
            Rotation = rotation;
        }
    }
}
