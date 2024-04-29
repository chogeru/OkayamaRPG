using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// ステート画像(img/system/States.png)のスプライト
    /// </summary>
    public class SpriteStateOverlay : SpriteBase
    {
        /// <summary>
        /// バトラー
        /// </summary>
        private GameBattler _battler;
        /// <summary>
        /// 画像のフレーム数
        /// </summary>
        private int _frameCount;
        /// <summary>
        /// 画像を設定するImage
        /// </summary>
        private Image _image;
        /// <summary>
        /// 現在表示しているフレーム番号
        /// </summary>
        private int _imageFrame;
        /// <summary>
        /// アニメーション中かどうか
        /// </summary>
        private bool _isWait;
        /// <summary>
        /// 画像ファイル名
        /// </summary>
        private string _overlayImageId = "";
        /// <summary>
        /// ステートのパターン
        /// </summary>
        private int _statePattern;
        /// <summary>
        /// 再生中のステートのID
        /// </summary>
        private string _stateId = "";
        /// <summary>
        /// 画像のTexture
        /// </summary>
        private Texture2D _texture;
        /// <summary>
        /// AssetManageDataModel
        /// </summary>
        private AssetManageDataModel assetData;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize() {
            base.Initialize();
            InitMembers();
            _initialized = true;
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        public void InitMembers() {
            _battler = null;
            _overlayImageId = "";
            anchor.x = 0.5;
            anchor.y = 1;

            transform.localPosition = new Vector3(0f, 0f, 0f);
            transform.localScale = new Vector3(1f, 1f, 1f);

            _image = gameObject.AddComponent<Image>();
            _image.enabled = false;
        }

        /// <summary>
        /// 準備
        /// </summary>
        /// <param name="battler"></param>
        public void Setup(GameBattler battler) {
            _battler = battler;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public override void Update() {
            if (_battler == null || _battler.States == null) return;
            if (_battler.States.Count >= 1) _initialized = true;

            if (!_initialized) return;

            base.Update();

            if (!_isWait)
            {
                UpdatePattern();
            }
        }

        /// <summary>
        /// パターンのアップデート
        /// </summary>
        private void UpdatePattern() {
            if (_battler.States.Count == 0)
            {
                //ステートが1つも設定されていない場合は非表示とする
                _image.enabled = false;
            }
            else
            {
                //ステートアニメーションのパターンの切り替えを行う
                bool isUpdate = false;

                //ステートの数分ループする
                for (int i = 0; i < _battler.States.Count; i++)
                {
                    //次に再生する予定のステート
                    _statePattern++;
                    //配列サイズを越えた場合は最初に戻す
                    if (_battler.States.Count - 1 < _statePattern) _statePattern = 0;

                    //オーバーレイ表示のID取得
                    //設定が無ければ次のステートを探す
                    _overlayImageId = _battler.States[_statePattern].overlay;
                    if (string.IsNullOrEmpty(_overlayImageId) || _overlayImageId == "0" || _overlayImageId == "1") continue;

                    //設定されていたため、オーバーレイ表示開始
                    _stateId = _battler.States[_statePattern].id;
                    StartCoroutine(MotionAnimation());
                    isUpdate = true;
                    break;
                }

                //現在のステートを一周しても、ステートの重ね合わせの定義が無ければ非表示にする
                if (!isUpdate)
                {
                    _image.enabled = false;
                }
            }
        }

        /// <summary>
        /// 画像の読み込み
        /// </summary>
        public bool LoadBitmap() {
            //画像のAssetDataを読込
            InitData();

            //画像のTexture等の設定を読込
            if (assetData != null)
            {
                _texture = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/Status/" + assetData.imageSettings[0].path);
                _frameCount = assetData.imageSettings[0].animationFrame;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 画像のAssetData読込
        /// </summary>
        private void InitData() {
#if UNITY_EDITOR

            var databaseManagementService = new DatabaseManagementService();
            var manageData = databaseManagementService.LoadAssetManage();
            assetData = null;
            for (int i = 0; i < manageData.Count; i++)
                if (manageData[i].id == _overlayImageId)
                {
                    assetData = manageData[i];
                    break;
                }
#else
            assetData = ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + _overlayImageId + ".asset") as AssetManageDataModel;
#endif
        }

        /// <summary>
        /// モーション再生処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator MotionAnimation() {
            //アニメーションを開始する際に、Bitmapをロードする
            if (!LoadBitmap())
            {
                yield break;
            }

            //アニメーション開始フラグをONにする
            _isWait = true;

            //画像を有効化
            _image.enabled = true;

            //アニメーション速度設定
            float waitTime = assetData.imageSettings[0].animationSpeed;

            //アニメーションを行う
            while (_imageFrame < assetData.imageSettings[0].animationFrame)
            {
                //Sprite設定
                SetSprite(_texture);
                //TextureUV設定
                SetTextureUV(new Vector2(1.0f / _frameCount * _imageFrame, 0), 
                    new Vector2(1.0f / _frameCount * (_imageFrame + 1), 1));
                //サイズ設定
                SetSize(new Vector2(_texture.width / (_frameCount + 1), _texture.height));

                //次のコマ表示まで待つ
                _imageFrame++;
                yield return new WaitForSeconds(waitTime * (1f / 60f));

                //待った後も同ステートが付与されているかどうかの確認
                bool flg = false;
                for (int i = 0; i < _battler.States.Count; i++)
                {
                    if (_battler.States[i].id == _stateId)
                    {
                        flg = true;
                        break;
                    }
                }
                if (!flg)
                {
                    //ステートが解除されているため、処理を終了する
                    _isWait = false;
                    yield break;
                }
            }

            //最終の処理
            _imageFrame = 0;
            SetSprite(_texture);
            SetTextureUV(new Vector2(1.0f / _frameCount * _imageFrame, 0),
                new Vector2(1.0f / _frameCount * (_imageFrame + 1), 1));
            SetSize(new Vector2(_texture.width / (_frameCount + 1), _texture.height));

            //アニメーション開始フラグをOFFにする
            _isWait = false;
        }
    }
}