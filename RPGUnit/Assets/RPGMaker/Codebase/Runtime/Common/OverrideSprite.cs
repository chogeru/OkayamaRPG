using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class OverrideSprite : MonoBehaviour
    {
        private static readonly int idMainTex = Shader.PropertyToID("_MainTex");


        [SerializeField] private Texture               _texture;
        private                  MaterialPropertyBlock block;
        private                  Image                 sr;

        public Texture SetTexture
        {
            set => _texture = value;
        }

        public Texture OverrideTexture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (block == null)
                {
                    Init();
                }

                if (_texture != null)
                {
                    block.SetTexture(idMainTex, _texture);
                }
            }
        }

        private void Awake() {
            Init();
            OverrideTexture = _texture;
        }

        private void LateUpdate() {
        }

        private void OnValidate() {
            if (_texture != null)
                OverrideTexture = _texture;
        }

        public void UpdateTexture() {
            Init();
            OverrideTexture = _texture;
        }

        private void Init() {
            block = new MaterialPropertyBlock();
            sr = GetComponentInChildren<Image>();
        }
    }
}