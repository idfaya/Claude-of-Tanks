using System;

namespace ClaudeOfTanks.Simulation
{
    public struct TerrainLandform
    {
        public string Kind;
        public float X;
        public float Z;
        public float Height;
        public float Length;
        public float Width;
        public float RadiusX;
        public float RadiusZ;
        public float YawRad;
    }

    public sealed class LandformHeightField : ITerrainSurface
    {
        private readonly TerrainLandform[] _landforms;

        public LandformHeightField(TerrainLandform[] landforms)
        {
            _landforms = landforms == null
                ? Array.Empty<TerrainLandform>()
                : (TerrainLandform[])landforms.Clone();
        }

        public TerrainLandform[] CopyLandforms()
        {
            return (TerrainLandform[])_landforms.Clone();
        }

        public float HeightAt(float x, float z)
        {
            float height = 0f;
            for (int i = 0; i < _landforms.Length; i++)
            {
                TerrainLandform form = _landforms[i];
                float dx = x - form.X;
                float dz = z - form.Z;
                float cos = MathF.Cos(form.YawRad);
                float sin = MathF.Sin(form.YawRad);
                float localX = dx * cos + dz * sin;
                float localZ = -dx * sin + dz * cos;
                float rx = form.RadiusX > 0f ? form.RadiusX : MathF.Max(1f, form.Width * 0.5f);
                float rz = form.RadiusZ > 0f ? form.RadiusZ : MathF.Max(1f, form.Length * 0.5f);
                float normalized = localX * localX / (rx * rx) + localZ * localZ / (rz * rz);
                if (normalized >= 1f) continue;
                float falloff = 1f - normalized;
                height += form.Height * falloff * falloff;
            }
            return height;
        }

        public Float3 NormalAt(float x, float z)
        {
            const float sample = 0.5f;
            float dx = HeightAt(x - sample, z) - HeightAt(x + sample, z);
            float dz = HeightAt(x, z - sample) - HeightAt(x, z + sample);
            return new Float3(dx, sample * 2f, dz).Normalized;
        }

        public float ResistanceAt(float x, float z)
        {
            return 1f;
        }
    }
}
