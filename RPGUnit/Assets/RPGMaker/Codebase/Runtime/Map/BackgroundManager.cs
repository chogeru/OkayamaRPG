using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// 背景の管理コンポーネント
    /// </summary>
    [DisallowMultipleComponent]
    public class BackgroundManager : MonoBehaviour
    {
        /// <summary>
        /// 背景データ
        /// </summary>
        private MapDataModel.Background _background;

        /// <summary>
        /// 背景画像
        /// </summary>
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            // spriteを取得する
            TryGetComponent(out _spriteRenderer);
            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 開始
        /// </summary>
        private void Start()
        {
            _spriteRenderer.enabled = true;
        }

        /// <summary>
        /// 更新
        /// </summary>
        private void LateUpdate()
        {
        }

        /// <summary>
        /// 背景データ設定
        /// </summary>
        /// <param name="background"></param>
        public void SetData(MapDataModel.Background background)
        {
            _background = background;

            if (string.IsNullOrEmpty(background.imageName))
                return;

            float scale = background.imageZoomIndex.GetZoomValue();

            var imagePath = Path.ChangeExtension(
                PathManager.MAP_BACKGROUND + background.imageName, ".png");
            var texture2d = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(imagePath);
            if (texture2d == null)
                return;

            // 表示倍率を設定。
            transform.localScale = new Vector3((float) scale, (float) scale, transform.localScale.z);

            _spriteRenderer.drawMode = SpriteDrawMode.Simple;
            _spriteRenderer.sprite = Sprite.Create(
                texture2d,
                new Rect(0f, 0f, texture2d.width, texture2d.height),
                new Vector2(0f, 1f),
                96f,
                0u);
        }

        /// <summary>
        /// 削除時（スクロール座標初期化）
        /// </summary>
        private void OnDestroy()
        {
            if (_spriteRenderer == null)
                return;
        }
    }
}