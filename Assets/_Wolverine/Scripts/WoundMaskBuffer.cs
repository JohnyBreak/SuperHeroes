using UnityEngine;

namespace Wolverine.Wounds
{
    /// <summary>
    /// Owns the UV-space wound mask RenderTexture used by outer skin materials.
    /// </summary>
    public sealed class WoundMaskBuffer : MonoBehaviour
    {
        [SerializeField] private int _resolution = 512;
        [SerializeField] private Material[] _skinMaterials;

        private RenderTexture _maskTexture;
        private RenderTexture _fadeTemporaryTexture;
        private Material _fadeMaterial;
        private bool _isInitialized;

        public RenderTexture MaskTexture => _maskTexture;

        public void Initialize(Material[] skinMaterials, Shader fadeShader)
        {
            _skinMaterials = skinMaterials;
            EnsureResources(fadeShader);
            Clear();
            BindToSkin();
        }

        public void EnsureResources(Shader fadeShader)
        {
            if (_isInitialized && _maskTexture != null)
            {
                return;
            }

            ReleaseTextures();

            _maskTexture = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.ARGB32)
            {
                name = "WoundMask",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false
            };
            _maskTexture.Create();

            _fadeTemporaryTexture = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.ARGB32)
            {
                name = "WoundMaskFadeTemporary",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _fadeTemporaryTexture.Create();

            if (fadeShader != null)
            {
                _fadeMaterial = new Material(fadeShader);
            }

            _isInitialized = true;
        }

        public void BindToSkin()
        {
            if (_skinMaterials == null || _maskTexture == null)
            {
                return;
            }

            for (int index = 0; index < _skinMaterials.Length; index++)
            {
                Material material = _skinMaterials[index];
                if (material != null)
                {
                    material.SetTexture("_WoundMask", _maskTexture);
                }
            }
        }

        public void Clear()
        {
            if (_maskTexture == null)
            {
                return;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = _maskTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;
        }

        public void Fade(float fadeAmount)
        {
            if (_maskTexture == null || _fadeMaterial == null)
            {
                return;
            }

            _fadeMaterial.SetFloat("_FadeAmount", Mathf.Max(0f, fadeAmount));
            Graphics.Blit(_maskTexture, _fadeTemporaryTexture, _fadeMaterial);
            Graphics.Blit(_fadeTemporaryTexture, _maskTexture);
        }

        private void OnDestroy()
        {
            ReleaseTextures();

            if (_fadeMaterial != null)
            {
                Destroy(_fadeMaterial);
            }
        }

        private void ReleaseTextures()
        {
            if (_maskTexture != null)
            {
                _maskTexture.Release();
                Destroy(_maskTexture);
                _maskTexture = null;
            }

            if (_fadeTemporaryTexture != null)
            {
                _fadeTemporaryTexture.Release();
                Destroy(_fadeTemporaryTexture);
                _fadeTemporaryTexture = null;
            }

            _isInitialized = false;
        }
    }
}
