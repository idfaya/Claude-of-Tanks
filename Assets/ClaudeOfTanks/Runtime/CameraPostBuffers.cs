using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal sealed class CameraPostBuffers : IDisposable
    {
        private RenderTexture _bloomA;
        private RenderTexture _bloomB;
        private RenderTexture _aoA;
        private RenderTexture _aoB;
        private BufferKey _bloomKey;
        private BufferKey _aoKey;

        public RenderTexture BloomA => _bloomA;
        public RenderTexture BloomB => _bloomB;
        public RenderTexture AoA => _aoA;
        public RenderTexture AoB => _aoB;

        public void EnsureBloom(
            RenderTexture source,
            int downsample)
        {
            BufferKey key = Key(
                source,
                downsample,
                source.format);
            if (_bloomA != null && _bloomKey.Equals(key))
                return;
            ReleasePair(ref _bloomA, ref _bloomB);
            CreatePair(
                source,
                key,
                "PostBloom",
                out _bloomA,
                out _bloomB);
            _bloomKey = key;
        }

        public void EnsureAmbientOcclusion(
            RenderTexture source,
            int downsample)
        {
            RenderTextureFormat format =
                SystemInfo.SupportsRenderTextureFormat(
                    RenderTextureFormat.R8)
                    ? RenderTextureFormat.R8
                    : RenderTextureFormat.ARGB32;
            BufferKey key = Key(
                source,
                downsample,
                format);
            if (_aoA != null && _aoKey.Equals(key))
                return;
            ReleasePair(ref _aoA, ref _aoB);
            CreatePair(
                source,
                key,
                "PostAo",
                out _aoA,
                out _aoB);
            _aoKey = key;
        }

        public void Dispose()
        {
            ReleasePair(ref _bloomA, ref _bloomB);
            ReleasePair(ref _aoA, ref _aoB);
            _bloomKey = default;
            _aoKey = default;
        }

        private static BufferKey Key(
            RenderTexture source,
            int downsample,
            RenderTextureFormat format)
        {
            return new BufferKey(
                Mathf.Max(1, source.width / downsample),
                Mathf.Max(1, source.height / downsample),
                format);
        }

        private static void CreatePair(
            RenderTexture source,
            BufferKey key,
            string name,
            out RenderTexture first,
            out RenderTexture second)
        {
            RenderTextureDescriptor descriptor =
                source.descriptor;
            descriptor.width = key.Width;
            descriptor.height = key.Height;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.colorFormat = key.Format;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            first = Create(descriptor, name + "A");
            second = Create(descriptor, name + "B");
        }

        private static RenderTexture Create(
            RenderTextureDescriptor descriptor,
            string name)
        {
            RenderTexture target = new RenderTexture(descriptor)
            {
                name = name,
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            target.Create();
            return target;
        }

        private static void ReleasePair(
            ref RenderTexture first,
            ref RenderTexture second)
        {
            Release(first);
            Release(second);
            first = null;
            second = null;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }

        private readonly struct BufferKey :
            IEquatable<BufferKey>
        {
            public BufferKey(
                int width,
                int height,
                RenderTextureFormat format)
            {
                Width = width;
                Height = height;
                Format = format;
            }

            public int Width { get; }
            public int Height { get; }
            public RenderTextureFormat Format { get; }

            public bool Equals(BufferKey other)
            {
                return Width == other.Width &&
                    Height == other.Height &&
                    Format == other.Format;
            }
        }
    }
}
