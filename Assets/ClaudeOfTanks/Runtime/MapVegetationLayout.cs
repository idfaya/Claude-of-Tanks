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
        public readonly float CanopyCenterHeight;
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
            TreeArchetype archetype =
                MapVegetationPlacementBuilder.Archetype(species);
            Index = index;
            Species = species;
            X = x;
            Z = z;
            Yaw = yaw;
            Scale = scale;
            Height = archetype.FallHeightM * scale;
            CanopyCenterHeight = archetype.CanopyCenterM * scale;
            CrownRadius = archetype.CanopyRadiusM * scale;
            TrunkHeight = archetype.TrunkHeightM * scale;
            TrunkRadius = archetype.TrunkRadiusM * scale;
        }
    }

    public readonly struct TreeArchetype
    {
        public readonly string Family;
        public readonly float TrunkRadiusM;
        public readonly float TrunkHeightM;
        public readonly float CanopyCenterM;
        public readonly float CanopyRadiusM;
        public readonly float FallHeightM;

        public TreeArchetype(
            string family,
            float trunkRadiusM,
            float trunkHeightM,
            float canopyCenterM,
            float canopyRadiusM,
            float fallHeightM)
        {
            Family = family;
            TrunkRadiusM = trunkRadiusM;
            TrunkHeightM = trunkHeightM;
            CanopyCenterM = canopyCenterM;
            CanopyRadiusM = canopyRadiusM;
            FallHeightM = fallHeightM;
        }
    }

    public static class MapVegetationPlacementBuilder
    {
        public const float WorldHalfExtentM = 500f;
        private static readonly TreeArchetype DefaultArchetype =
            new TreeArchetype("broadleaf", 0.32f, 3.2f, 4.35f, 3.0f, 6.8f);

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

        public static bool IsBirchFamily(string species)
        {
            return species == "birch" || species == "aspen";
        }

        public static TreeArchetype Archetype(string species)
        {
            switch (species)
            {
                case "pine":
                    return new TreeArchetype("conifer", 0.24f, 3.2f, 4.5f, 2.5f, 6.8f);
                case "spruce":
                    return new TreeArchetype("conifer", 0.22f, 3.8f, 5.6f, 2.1f, 8.1f);
                case "fir":
                    return new TreeArchetype("conifer", 0.28f, 3.4f, 4.8f, 2.7f, 7.0f);
                case "cedar":
                    return new TreeArchetype("conifer", 0.30f, 3.0f, 4.2f, 3.0f, 6.4f);
                case "cypress":
                    return new TreeArchetype("conifer", 0.18f, 4.1f, 5.4f, 1.35f, 7.8f);
                case "oak":
                    return DefaultArchetype;
                case "poplar":
                    return new TreeArchetype("broadleaf", 0.23f, 4.2f, 5.6f, 1.8f, 8.0f);
                case "willow":
                    return new TreeArchetype("broadleaf", 0.38f, 2.5f, 3.55f, 3.7f, 6.2f);
                case "acacia":
                    return new TreeArchetype("broadleaf", 0.30f, 3.4f, 4.25f, 3.6f, 6.4f);
                case "eucalyptus":
                    return new TreeArchetype("broadleaf", 0.25f, 4.8f, 6.1f, 2.0f, 8.8f);
                case "palm":
                    return new TreeArchetype("palm", 0.26f, 5.2f, 6.1f, 3.1f, 7.4f);
                case "birch":
                    return new TreeArchetype("birch", 0.18f, 4.0f, 4.8f, 2.4f, 6.8f);
                case "aspen":
                    return new TreeArchetype("birch", 0.16f, 4.6f, 5.5f, 1.9f, 7.6f);
                default:
                    return DefaultArchetype;
            }
        }
    }
}
