using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// Unity用の部品
    /// </summary>
    public class Sprite : MonoBehaviour
    {
        protected Bitmap        _bitmap;
        private   RectTransform _imageRectTransform;

        protected bool            _initialized = false;
        private   Material        _material;
        private   float           _opacity;
        private   RectTransform   _rectTransform;
        private   OverrideTexture _texture;
        private   bool            _visible;
        protected float           _x;
        protected float           _y;
        public    SpriteAnchor    anchor = new SpriteAnchor();

        /// <summary>
        /// Bitmap
        /// </summary>
        public Bitmap bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
                var imageSprite = gameObject.GetComponent<Image>();
                if (imageSprite == null) imageSprite = gameObject.transform.Find("Sprite").GetComponent<Image>();

                if (_bitmap != null && _bitmap?.UnitySprite != null)
                {
                    imageSprite.sprite = _bitmap.UnitySprite;
                    imageSprite.SetNativeSize();
                }
                else
                {
                    imageSprite.enabled = false;
                }
            }
        }

        /// <summary>
        /// Texture
        /// </summary>
        public OverrideTexture texture
        {
            get => _texture;
            set
            {
                _texture = value;
                var overrideSprite = gameObject.GetComponent<OverrideSprite>();
                if (overrideSprite == null) overrideSprite = gameObject.AddComponent<OverrideSprite>();

                overrideSprite.OverrideTexture = _texture.UnityTexture;
            }
        }

        /// <summary>
        /// 親
        /// </summary>
        public Sprite Parent { get; set; }

        /// <summary>
        /// X
        /// </summary>
        public virtual float X
        {
            get => _x;
            set
            {
                _x = value;
                if (gameObject)
                {
                    var pos = gameObject.transform.position;
                    pos.x = value / 74;
                    gameObject.transform.position = pos;
                }
            }
        }

        /// <summary>
        /// Y
        /// </summary>
        public virtual float Y
        {
            get => _y;
            set
            {
                _y = value;
                if (gameObject)
                {
                    var pos = gameObject.transform.localPosition;
                    pos.y = _y;
                    gameObject.transform.localPosition = pos;
                }
            }
        }

        /// <summary>
        /// 透過度
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                var canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.alpha = _opacity / 255;
            }
        }

        /// <summary>
        /// 表示、非表示設定
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                Opacity = _visible ? 255 : 0;
            }
        }

        /// <summary>
        /// Sprite設定
        /// </summary>
        /// <param name="texture2D"></param>
        public void SetSprite(Texture2D texture2D) {

            if (texture2D == null) return;
            
            var image = gameObject.GetComponent<Image>();
            if (image == null) image = gameObject.transform.Find("Sprite").GetComponent<Image>();

            var sprite = UnityEngine.Sprite.Create(texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);

            image.sprite = sprite;
            image.SetNativeSize();
        }

        /// <summary>
        /// テクスチャUVの設定
        /// </summary>
        /// <param name="start">開始UV位置</param>
        /// <param name="end">終了UV位置</param>
        public void SetTextureUV(Vector2 start, Vector2 end) {
            // マテリアルがnullの場合は作成する
            if (_material == null)
            {
                var image = gameObject.GetComponent<Image>();
                if (image == null) image = gameObject.transform.Find("Sprite").GetComponent<Image>();

                _material = new Material(image.material.shader);
                image.material = _material;
            }

            // 数値調整して代入
            _material.mainTextureOffset = new Vector2(start.x, start.y);
            _material.SetTextureScale("_MainTex", new Vector2(end.x - start.x, end.y));
        }

        /// <summary>
        /// サイズ設定
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(Vector2 size) {
            if (_rectTransform == null)
                _rectTransform = transform.GetComponent<RectTransform>();
            if (_imageRectTransform == null)
            {
                if (gameObject.transform.Find("Sprite") != null)
                    _imageRectTransform = gameObject.transform.Find("Sprite").transform.GetComponent<RectTransform>();
                else if (gameObject.transform.GetComponent<RectTransform>() != null)
                    _imageRectTransform = gameObject.transform.GetComponent<RectTransform>();
            }

            _rectTransform.sizeDelta = size;
            _imageRectTransform.sizeDelta = size;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public virtual void Initialize() {
        }

        /// <summary>
        /// BlendColor設定
        /// </summary>
        /// <param name="color"></param>
        public void SetBlendColor(List<int> color) {
            var image = gameObject.GetComponent<Image>();
            if (image)
            {
                var r = color[0] / 255f;
                var g = color[1] / 255f;
                var b = color[2] / 255f;
                var a = color[3] / 255f;

                image.color = new Color(r, g, b, a);
            }
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public virtual void Update() {
        }
    }

    /// <summary>
    /// SpriteのAnchor設定用クラス
    /// </summary>
    public class SpriteAnchor
    {
        public double x;
        public double y;
    }
}