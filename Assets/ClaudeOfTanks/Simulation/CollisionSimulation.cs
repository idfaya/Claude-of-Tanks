using System;

namespace ClaudeOfTanks.Simulation
{
    public struct RamDamageResult
    {
        public float Total;
        public float ToRammer;
        public float ToVictim;
    }

    public static class CollisionSimulation
    {
        public const float MinimumClosingSpeedMps = 2.5f;

        public static RamDamageResult RamDamage(
            float rammerMassTons, float victimMassTons, float closingSpeedMps)
        {
            float rammerMass = rammerMassTons > 0f ? rammerMassTons : 40f;
            float victimMass = victimMassTons > 0f ? victimMassTons : 40f;
            float speed = MathF.Abs(closingSpeedMps);
            if (speed < MinimumClosingSpeedMps) return default;
            float reducedMass = rammerMass * victimMass / (rammerMass + victimMass);
            float total = MathF.Min(900f, 0.2f * speed * speed * reducedMass);
            return new RamDamageResult
            {
                Total = total,
                ToRammer = total * victimMass / (rammerMass + victimMass) * 0.65f,
                ToVictim = total * rammerMass / (rammerMass + victimMass)
            };
        }
    }
}
