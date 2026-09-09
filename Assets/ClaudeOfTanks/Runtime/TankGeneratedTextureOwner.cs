using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class TankGeneratedTextureOwner : MonoBehaviour
    {
        private Texture2D _texture;

        public void Initialize(Texture2D texture)
        {
            _texture = texture;
        }

        public static void ReleaseOwnedTextures(Transform root)
        {
            if (root == null) return;
            TankGeneratedTextureOwner[] owners =
                root.GetComponentsInChildren<
                    TankGeneratedTextureOwner>(true);
            for (int index = 0; index < owners.Length; index++)
                owners[index].Release();
        }

        private void Release()
        {
            if (_texture == null) return;
            if (Application.isPlaying)
                Destroy(_texture);
            else
                DestroyImmediate(_texture);
            _texture = null;
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
