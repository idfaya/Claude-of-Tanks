using System;
using System.Collections.Generic;
using ClaudeOfTanks.Simulation;

namespace ClaudeOfTanks.Runtime
{
    public readonly struct VegetationTreePlacement
    {
        public readonly int Index;
        public readonly string Species;
        public readonly float X;
        public readonly float Z;
        public readonly float Yaw;
        public readonly float Scale;
        public readonly float Height;
        public readonly float TrunkHeight;
        public readonly float TrunkRadius;
        public readonly float CrownRadius;

        public VegetationTreePlacement(
            int index,
            string species,
            float x,
            float z,
            float yaw,
            float scale)
        {
            bool conifer = MapVegetationPlacementBuilder.IsConifer(species);
            bool palm = string.Equals(species, "palm", StringComparison.Ordinal);
            bool narrow = conifer ||
                string.Equals(species, "poplar", StringComparison.Ordinal) ||
                string.Equals(species, "cypress", StringComparison.Ordinal);
            Index = index;
            Species = species;
            X = x;
            Z = z;
            Yaw = yaw;
            Scale = scale;
            Height = (palm ? 12f : narrow ? 11f : 8.5f) * scale;
            CrownRadius = (palm ? 3.4f : narrow ? 2.25f : 3.2f) * scale;
            TrunkHeight = Height * (palm ? 0.78f : 0.48f);
            TrunkRadius = 0.21f * scale;
        }
    }

    public static class MapVegetationPlacementBuilder
    {
        public const float WorldHalfExtentM = 500f;

        public static VegetationTreePlacement[] Expand(MapDefinition map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            MapVegetationStand[] stands =
                map.unityVegetation?.stands ?? Array.Empty<MapVegetationStand>();
            List<VegetationTreePlacement> trees =
                new List<VegetationTreePlacement>(
                    Math.Max(0, map.unityVegetation?.treeCount ?? 0));
            for (int standIndex = 0; standIndex < stands.Length; standIndex++)
            {
                MapVegetationStand stand = stands[standIndex];
                DeterministicRandom random = new DeterministicRandom(
                    stand.seed == 0u ? 1u : stand.seed);
                for (int treeIndex = 0; treeIndex < stand.count; treeIndex++)
                {
                    float angle = random.NextFloat() * MathUtil.Pi * 2f;
                    float radius = stand.radius > 0f
                        ? MathF.Sqrt(random.NextFloat()) * stand.radius
                        : 0f;
                    float x = MathUtil.Clamp(
                        stand.x + MathF.Cos(angle) * radius,
                        -WorldHalfExtentM + 1f,
                        WorldHalfExtentM - 1f);
                    float z = MathUtil.Clamp(
                        stand.z + MathF.Sin(angle) * radius,
                        -WorldHalfExtentM + 1f,
                        WorldHalfExtentM - 1f);
                    trees.Add(new VegetationTreePlacement(
                        trees.Count,
                        stand.species,
                        x,
                        z,
                        random.NextFloat() * MathUtil.Pi * 2f,
                        0.78f + random.NextFloat() * 0.48f));
                }
            }
            return trees.ToArray();
        }

        public static bool IsConifer(string species)
        {
            return species == "pine" ||
                species == "spruce" ||
                species == "fir" ||
                species == "cedar" ||
                species == "cypress";
        }
    }
}
