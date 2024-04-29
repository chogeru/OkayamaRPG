using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Runtime.Common;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    ///     遠景の管理コンポーネント
    /// </summary>
    public class DistantViewManager : MonoBehaviour
    {
        //遠景画像の入ったファイルパス
        public static readonly string PATH = "Assets/RPGMaker/Storage/Images/Parallaxes/";

        // ループ速度に掛ける割合
        private readonly float AUTO_LOOP_SPEED_RATIO = 16.25f;
        private readonly float PARALLAX_LOOP_SPEED_RATIO = 32.5f;

        private Vector2 _parallaxLoopSpeedRatio;
        private Vector3? _prevCameraPosition;

        // ループ速度
        private float _loopSpeedX;
        private float _loopSpeedY;

        // 遠景データ
        private MapDataModel.parallax _parallax;

        // 遠景画像
        private SpriteRenderer _spriteRenderer;

        private bool ExistsDistantView()
        {
            return !string.IsNullOrEmpty(_parallax?.name);
        }

        private void Awake() {
            // spriteを取得する
            TryGetComponent(out _spriteRenderer);
            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 開始
        /// </summary>
        private void Start() {
            if (!ExistsDistantView())
            {
                return;
            }
            
            if (_spriteRenderer?.sprite == null)
                return;

            SetMaterial();
        }

        /// <summary>
        /// マテリアル設定
        /// </summary>
        private void SetMaterial() {

            _spriteRenderer.enabled = true;

            _spriteRenderer.material =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Material>(
                    "Assets/RPGMaker/Codebase/Runtime/Map/DistantViewMaterial.mat");
                    
            // 遠景画像の左上が画面の左上に位置するよう設定。
            _spriteRenderer.material.mainTextureOffset =
                new Vector2(
                    0,
                    -(float)DataManager.Self().GetSystemDataModel().GetDisplaySize().y /
                    (_spriteRenderer.sprite.texture.height *
                     transform.localScale.y));
            
        }

        /// <summary>
        /// 更新
        /// </summary>
        private void LateUpdate() {
            if (!ExistsDistantView())
            {
                return;
            }

            // 設定値による遠景スプライトのテクスチャスクロール。
            _spriteRenderer.material.mainTextureOffset +=
                new Vector2(_loopSpeedX, _loopSpeedY) * Time.deltaTime;

            // メインカメラの位置に関わる処理。
            var cameraTransform = transform.root.Find("New Game Object/Main Camera");
            if (cameraTransform != null)
            {
                var cameraPosition = cameraTransform.position;

                // 遠景スプライトは静止して見えるように、カメラに追従して移動させる
                transform.localPosition = new Vector3(
                    cameraPosition.x,
                    cameraPosition.y,
                    transform.localPosition.z);

                // ループ設定がある方向に、カメラの移動量に応じてテクスチャスクロールをさせる (視差効果)。
                if (_prevCameraPosition != null && _parallax != null)
                {
                    var delta = cameraPosition - (Vector3)_prevCameraPosition;
                    _spriteRenderer.material.mainTextureOffset +=
                        new Vector2(
                            _parallax.loopX ? delta.x * _parallaxLoopSpeedRatio.x : 0f,
                            _parallax.loopY ? delta.y * _parallaxLoopSpeedRatio.y : 0f);
                }

                _prevCameraPosition = cameraPosition;
            }
        }

        /// <summary>
        /// 遠景データ設定
        /// </summary>
        /// <param name="parallax"></param>
        public void SetData(MapDataModel.parallax parallax) {
            _parallax = parallax;

            if (!ExistsDistantView())
            {
                return;
            }

            float zoomScale = parallax.GetZoomScale();

            var imagePath = Path.ChangeExtension(PATH + parallax.name, ".png");
            var texture2d = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(imagePath);
            if (texture2d == null) return;

            //TextureのWrapModeを、Repeatに強制的に設定する
            texture2d.wrapMode = TextureWrapMode.Repeat;

            // 表示倍率を設定。
            transform.localScale = new Vector3((float)zoomScale, (float)zoomScale, transform.localScale.z);

            _spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            _spriteRenderer.sprite = Sprite.Create(
                texture2d, 
                new Rect(0f, 0f, texture2d.width, texture2d.height),
                new Vector2(0.5f, 0.5f),
                96f,
                0u,
                SpriteMeshType.FullRect);

			// 表示倍率と画面サイズから、drawModeがTiled時の遠景サイズを設定。
            _spriteRenderer.size =
                (Vector2)DataManager.Self().GetSystemDataModel().GetDisplaySize() /
                (96f * zoomScale);

            SetMaterial();

            var zoomedTextureSize = new Vector2(texture2d.width, texture2d.height) * zoomScale;


            // 各パラメータを適用
            // ループ設定
            _loopSpeedX = _parallax.loopX ? _parallax.sx * +AUTO_LOOP_SPEED_RATIO / zoomedTextureSize.x : 0f;
            _loopSpeedY = _parallax.loopY ? _parallax.sy * -AUTO_LOOP_SPEED_RATIO / zoomedTextureSize.y : 0f;

            _parallaxLoopSpeedRatio = new Vector2(
                PARALLAX_LOOP_SPEED_RATIO / zoomedTextureSize.x,
                PARALLAX_LOOP_SPEED_RATIO / zoomedTextureSize.y);
        }

        /// <summary>
        /// 削除時（スクロール座標初期化）
        /// </summary>
        private void OnDestroy() {
            if (!ExistsDistantView())
            {
                return;
            }

            if (_spriteRenderer == null) return;

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
#endif
            {
                _spriteRenderer.material.mainTextureOffset = new Vector2(0, 0);
            }

        }
    }
}