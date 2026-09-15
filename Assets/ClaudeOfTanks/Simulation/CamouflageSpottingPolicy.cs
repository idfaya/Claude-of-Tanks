using System;

namespace ClaudeOfTanks.Simulation
{
    public static class CamouflageSpottingPolicy
    {
        public const float MatchingPaintBonus =
            0.035f;

        public static float Bonus(
            string camouflageId,
            string mapId)
        {
            string pattern =
                camouflageId ??
                "factory";
            if (pattern == "factory" ||
                pattern == "signature" ||
                pattern.StartsWith(
                    "sig_",
                    StringComparison.Ordinal) ||
                pattern.StartsWith(
                    "service_",
                    StringComparison.Ordinal))
            {
                return 0f;
            }
            if (pattern == "auto" ||
                pattern == "custom")
                return MatchingPaintBonus;
            string biome = BiomeFor(mapId);
            return Matches(pattern, biome)
                ? MatchingPaintBonus
                : 0f;
        }

        private static bool Matches(
            string pattern,
            string biome)
        {
            switch (biome)
            {
                case "winter":
                    return ContainsAny(
                        pattern,
                        "winter",
                        "washworn",
                        "merdcwinter",
                        "winterbands");
                case "desert":
                    return ContainsAny(
                        pattern,
                        "desert",
                        "chocchip",
                        "pinkdesert",
                        "digitaldesert");
                case "urban":
                    return ContainsAny(
                        pattern,
                        "urban",
                        "berlin");
                case "autumn":
                    return ContainsAny(
                        pattern,
                        "autumn",
                        "oakleaf");
                case "coastal":
                    return ContainsAny(
                        pattern,
                        "summer",
                        "merdc",
                        "dpm",
                        "naval");
                case "verdant":
                    return ContainsAny(
                        pattern,
                        "summer",
                        "merdc",
                        "tropic",
                        "flecktarn",
                        "amoeba",
                        "dpm",
                        "tigerstripe",
                        "m90");
                default:
                    return false;
            }
        }

        private static string BiomeFor(
            string mapId)
        {
            switch (mapId)
            {
                case "winter":
                case "frosthollow":
                case "alpine":
                    return "winter";
                case "desert":
                case "sirocco":
                case "steppe":
                case "badlands":
                case "caldera":
                    return "desert";
                case "urban":
                case "cinder":
                case "railyard":
                case "foundry":
                case "ruinspires":
                case "blackglass":
                    return "urban";
                case "autumn":
                case "amberford":
                    return "autumn";
                case "coastal":
                case "delta":
                case "fjord":
                case "monsoon":
                    return "coastal";
                default:
                    return "verdant";
            }
        }

        private static bool ContainsAny(
            string value,
            params string[] candidates)
        {
            for (int i = 0;
                i < candidates.Length;
                i++)
            {
                if (value.IndexOf(
                        candidates[i],
                        StringComparison
                            .OrdinalIgnoreCase) >=
                    0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
