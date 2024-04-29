using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Component.Character
{
    public class CharacterGraphic : MonoBehaviour
    {
        protected readonly float           TILE_SIZE = 96;
        protected          SpriteDataModel _currentSprite;
        protected          int             _currentSpriteIndex;

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected CharacterMoveDirectionEnum _directionEnum;
        protected CharacterMoveDirectionEnum _tempDirectionEnum = CharacterMoveDirectionEnum.Down;
        protected int _frameCount;
        protected float _frameTime;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected Image           _image;

        protected SpriteDataModel _leftDirectionSprite;
        protected SpriteDataModel _downDirectionSprite;
        protected SpriteDataModel _rightDirectionSprite;
        protected SpriteDataModel _upDirectionSprite;
        protected SpriteDataModel _damageSprite;

        protected SpriteDataModel _leftDirectionSpriteNormal;
        protected SpriteDataModel _downDirectionSpriteNormal;
        protected SpriteDataModel _rightDirectionSpriteNormal;
        protected SpriteDataModel _upDirectionSpriteNormal;
        protected SpriteDataModel _damageSpriteNormal;

        protected SpriteDataModel _leftDirectionSpriteBush;
        protected SpriteDataModel _downDirectionSpriteBush;
        protected SpriteDataModel _rightDirectionSpriteBush;
        protected SpriteDataModel _upDirectionSpriteBush;
        protected SpriteDataModel _damageSpriteBush;

        protected Sprite _up;
        protected Sprite _down;
        protected Sprite _left;
        protected Sprite _right;
        protected Sprite _damage;

        protected Sprite _upBush;
        protected Sprite _downBush;
        protected Sprite _leftBush;
        protected Sprite _rightBush;
        protected Sprite _damageBush;

        protected int _upIndex;
        protected int _downIndex;
        protected int _leftIndex;
        protected int _rightIndex;
        protected int _damageIndex;

        protected int _upSpeed;
        protected int _downSpeed;
        protected int _leftSpeed;
        protected int _rightSpeed;
        protected int _damageSpeed;

        protected bool _isBush;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected Material        _material;

        //立ち止まっているときの画像Index
        protected readonly int stopTextureIndex = 0;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        public virtual int GetCurrentAnimationSpeed() {
            return _currentSprite != null ? _currentSprite.animationSpeed : 0;
        }

        /// <summary>
        /// 各画面でSDキャラ画像の1枚目を表示するためのプロパティ
        /// </summary>
        /// <returns></returns>
        public virtual Sprite GetCurrentSprite() {
            return _currentSprite?.sprite;
        }

        /// <summary>
        /// 各画面でSDキャラ画像の1枚目を表示するためのプロパティ
        /// </summary>
        /// <returns></returns>
        public virtual Material GetMaterial() {
            return _material;
        }

        /// <summary>
        /// 各画面でSDキャラ画像の1枚目を表示するためのプロパティ
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetSize() {
            return _image.transform.localScale;
        }

        /// <summary>
        /// 一時的に保存する向き
        /// </summary>
        public CharacterMoveDirectionEnum GetTempDirection() { return _tempDirectionEnum; }
        public void SetTempDirection(CharacterMoveDirectionEnum directionEnum) { _tempDirectionEnum = directionEnum; }

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public virtual void Init(string assetId, bool isFirstInit = true) {
            if (assetId == "")
            {
                //フキダシ用に作成する
                // コンポーネント設定
                gameObject.GetOrAddComponent<Canvas>();
                gameObject.name = "actor";
                _image = gameObject.GetOrAddComponent<Image>();
                _material = new Material(_image.material.shader);
                _image.material = _material;
                // サイズ、位置設定
                transform.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                transform.position = new Vector3(0, 0, 0);
                return;
            }

            SetUpSprites(assetId);

            // コンポーネント設定
            gameObject.GetOrAddComponent<Canvas>();
            gameObject.name = "actor";
            _image = gameObject.GetOrAddComponent<Image>();
            _material = new Material(_image.material.shader);
            _image.material = _material;

            // サイズ、位置設定
            transform.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
            transform.position = new Vector3(0, 0, 0);
            if (_downDirectionSprite?.sprite?.texture != null)
            {
                SetSize(new Vector2(
                    _downDirectionSprite.sprite.texture.width / _downDirectionSprite.animationFrame / TILE_SIZE,
                    _downDirectionSprite.sprite.texture.height / TILE_SIZE));
                transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0);
            }

            if (isFirstInit)
                ChangeDirection(CharacterMoveDirectionEnum.Down);
            _isBush = false;
        }

        protected virtual void Update() {
            // 位置を修正
            if (_downDirectionSprite?.sprite?.texture != null &&
                (transform.GetComponent<RectTransform>().pivot != new Vector2(0.5f, 0) ||
                 transform.GetComponent<RectTransform>().anchoredPosition != new Vector2(0.5f, 0)))
            {
                transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0);
            }
        }

        public virtual void ChangeAsset(string assetId) {
            // 画像が設定されていなければ初期化
            bool isEnabled = IsEnabledImage();
            Init(assetId, false);

            if (_image != null && _image.enabled == false)
            {
                _image.enabled = true;
            }

            SetUpSprites(assetId);
            _currentSprite = _directionEnum switch
            {
                CharacterMoveDirectionEnum.Up => _upDirectionSprite,
                CharacterMoveDirectionEnum.Down => _downDirectionSprite,
                CharacterMoveDirectionEnum.Left => _leftDirectionSprite,
                CharacterMoveDirectionEnum.Right => _rightDirectionSprite,
                CharacterMoveDirectionEnum.Damage => _damageSprite,
                _ => throw new ArgumentOutOfRangeException()
            };

            //今回始めて画像を描画した場合
            if (!isEnabled && IsEnabledImage())
                _currentSpriteIndex = 0;

            ChangeTextureSize();
            Render();
        }

        public virtual void ChangeDirection(CharacterMoveDirectionEnum directionEnum) {
            if (directionEnum == _directionEnum)
            {
                //同一の向きであったとしても、_tempDirectionEnum は更新する
                _tempDirectionEnum = directionEnum;
                return;
            }

            _directionEnum = directionEnum;
            _tempDirectionEnum = directionEnum;
            _currentSprite = _directionEnum switch
            {
                CharacterMoveDirectionEnum.Up => _upDirectionSprite,
                CharacterMoveDirectionEnum.Down => _downDirectionSprite,
                CharacterMoveDirectionEnum.Left => _leftDirectionSprite,
                CharacterMoveDirectionEnum.Right => _rightDirectionSprite,
                CharacterMoveDirectionEnum.Damage => _damageSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
            _currentSpriteIndex = 0;
            ChangeTextureSize();
            Render();
        }

        public void SetBush(bool isBush) {
            if (_isBush == isBush) return;
            _isBush = isBush;

            if (!_isBush)
            {
                if (_downDirectionSpriteNormal == null)
                {
                    SetUpNormalSprites();
                }
                _downDirectionSprite = _downDirectionSpriteNormal;
                _leftDirectionSprite = _leftDirectionSpriteNormal;
                _rightDirectionSprite = _rightDirectionSpriteNormal;
                _upDirectionSprite = _upDirectionSpriteNormal;
                _damageSprite = _damageSpriteNormal;
            }
            else
            {
                if (_downDirectionSpriteBush == null)
                {
                    SetUpBushSprites();
                }
                _downDirectionSprite = _downDirectionSpriteBush;
                _leftDirectionSprite = _leftDirectionSpriteBush;
                _rightDirectionSprite = _rightDirectionSpriteBush;
                _upDirectionSprite = _upDirectionSpriteBush;
                _damageSprite = _damageSpriteBush;
            }

            _currentSprite = _directionEnum switch
            {
                CharacterMoveDirectionEnum.Up => _upDirectionSprite,
                CharacterMoveDirectionEnum.Down => _downDirectionSprite,
                CharacterMoveDirectionEnum.Left => _leftDirectionSprite,
                CharacterMoveDirectionEnum.Right => _rightDirectionSprite,
                CharacterMoveDirectionEnum.Damage => _damageSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
            ChangeTextureSize();
            Render();
        }

        public virtual void Reset() {
            _currentSpriteIndex = 0;
            ChangeTextureSize();
            Render();
        }

        protected virtual void ChangeTextureSize() {
            var spriteData = _directionEnum switch
            {
                CharacterMoveDirectionEnum.Up => _upDirectionSprite,
                CharacterMoveDirectionEnum.Down => _downDirectionSprite,
                CharacterMoveDirectionEnum.Left => _leftDirectionSprite,
                CharacterMoveDirectionEnum.Right => _rightDirectionSprite,
                CharacterMoveDirectionEnum.Damage => _damageSprite,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (spriteData?.sprite?.texture != null)
                SetSize(new Vector2(
                    spriteData.sprite.texture.width / spriteData.animationFrame / TILE_SIZE,
                    spriteData.sprite.texture.height / TILE_SIZE));
        }

        public virtual void Step(CharacterMoveDirectionEnum directionEnum, bool isLockDirection) {
            if (directionEnum != _directionEnum && !isLockDirection)
            {
                ChangeDirection(directionEnum);
                return;
            }

            StepAnimation();
        }


        public virtual CharacterMoveDirectionEnum GetCurrentDirection() {
            return _directionEnum;
        }

        private float elapsedTime;
        private float elapsedTimeBef;

        public virtual void StepAnimation() {
            if (_currentSprite == null) return;

            if (elapsedTime == 0) elapsedTime = Time.realtimeSinceStartup;
            elapsedTimeBef = elapsedTime;
            elapsedTime = Time.realtimeSinceStartup;

            _frameTime += (elapsedTime - elapsedTimeBef) * 60;
            _frameCount = (int) _frameTime;

            int animationFrameCount = _currentSprite.animationFrame != 0 ?
                _currentSprite.animationSpeed / _currentSprite.animationFrame : 0;

            if (animationFrameCount == 0 || _frameCount >= animationFrameCount)
            {
                _frameTime = _frameTime - _frameCount;
                _frameCount = 0;
                _currentSpriteIndex++;
                if (_currentSpriteIndex >= _currentSprite.animationFrame) _currentSpriteIndex = 0;
                Render();
            }
        }

        public virtual void DamageAnimation() {
            if (_currentSprite == null) return;

            bool flg = false;
            if (_currentSprite.animationFrame == 0)
            {
                flg = true;
            }
            else if ((_currentSprite.animationSpeed / _currentSprite.animationFrame) == 0)
            {
                flg = true;
            }
            _frameTime += Time.deltaTime * 60;
            _frameCount = (int) _frameTime;

            if (flg || _frameCount % (_currentSprite.animationSpeed / _currentSprite.animationFrame) == 0)
            {
                _frameTime = _frameTime - _frameCount;
                _frameCount = 0;
                _currentSpriteIndex++;
                if (_currentSpriteIndex >= _currentSprite.animationFrame) _currentSpriteIndex = _currentSprite.animationFrame - 1;
                Render();
            }
        }

        //静止画像描画
        public virtual void StopTexture() {
            //画像未設定の場合は処理終了
            if (_currentSprite == null) return;

            //現在のアニメーションが静止画像ではない場合、静止画像に切り替えてRenderを行う
            if (_currentSpriteIndex != stopTextureIndex)
            {
                _currentSpriteIndex = stopTextureIndex;
                Render();
            }
        }

        protected virtual void Render() {
            if (_currentSprite == null) return;

            // テクスチャがnullであればImageを無効化する
            if (_currentSprite.sprite == null)
            {
                _image.enabled = false;
                return;
            }

            _image.sprite = _currentSprite.sprite;
            SetTextureUV(new Vector2(1.0f / _currentSprite.animationFrame * _currentSpriteIndex, 0),
                new Vector2(1.0f / _currentSprite.animationFrame * (_currentSpriteIndex + 1), 1));
        }

        protected virtual void SetUpSprites(string assetId) {
            if (assetId != "")
            {
                // idとファイル名は同名の為そのままロード
#if UNITY_EDITOR
                var inputString =
                    UnityEditorWrapper.AssetDatabaseWrapper
                        .LoadAssetAtPath<TextAsset>(
                            "Assets/RPGMaker/Storage/AssetManage/JSON/Assets/" + assetId + ".json");

                if (inputString == null)
                {
                    //画像のIDが取得できなければ、画像を消す
                    _downDirectionSprite = null;
                    _leftDirectionSprite = null;
                    _rightDirectionSprite = null;
                    _upDirectionSprite = null;
                    _damageSprite = null;

                    _downDirectionSpriteNormal = null;
                    _leftDirectionSpriteNormal = null;
                    _rightDirectionSpriteNormal = null;
                    _upDirectionSpriteNormal = null;
                    _damageSpriteNormal = null;

                    _downDirectionSpriteBush = null;
                    _leftDirectionSpriteBush = null;
                    _rightDirectionSpriteBush = null;
                    _upDirectionSpriteBush = null;
                    _damageSpriteBush = null;

                    if (_currentSprite != null)
                        _currentSprite.sprite = null;
                    if (_image != null)
                        _image.enabled = false;
                    return;
                }

                var assetManageData = JsonHelper.FromJson<AssetManageDataModel>(inputString.text);
#else
                var assetManageData =
                    ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + assetId + ".asset") as AssetManageDataModel;
                if (assetManageData == null)
                {
                    //画像のIDが取得できなければ、画像を消す
                    _downDirectionSprite = null;
                    _leftDirectionSprite = null;
                    _rightDirectionSprite = null;
                    _upDirectionSprite = null;
                    _damageSprite = null;

                    _downDirectionSpriteNormal = null;
                    _leftDirectionSpriteNormal = null;
                    _rightDirectionSpriteNormal = null;
                    _upDirectionSpriteNormal = null;
                    _damageSpriteNormal = null;

                    _downDirectionSpriteBush = null;
                    _leftDirectionSpriteBush = null;
                    _rightDirectionSpriteBush = null;
                    _upDirectionSpriteBush = null;
                    _damageSpriteBush = null;

                    if (_currentSprite != null)
                        _currentSprite.sprite = null;
                    if (_image != null)
                        _image.enabled = false;
                    return;
                }
#endif

                //初期化処理
                ClearNormalSprites();
                ClearBushSprites();

                //各方向のキャラクター画像を読み込む。タイプ別
                _down = assetManageData.assetTypeId == (int) AssetCategoryEnum.MOVE_CHARACTER
                            ? LoadCharacterSprite(assetManageData.imageSettings[0].path)
                            : LoadObjectSprite(assetManageData.imageSettings[0].path);
                _left = assetManageData.assetTypeId == (int) AssetCategoryEnum.MOVE_CHARACTER
                            ? LoadCharacterSprite(assetManageData.imageSettings[1].path)
                            : LoadObjectSprite(assetManageData.imageSettings[1].path);
                _right = assetManageData.assetTypeId == (int) AssetCategoryEnum.MOVE_CHARACTER
                            ? LoadCharacterSprite(assetManageData.imageSettings[2].path)
                            : LoadObjectSprite(assetManageData.imageSettings[2].path);
                _up = assetManageData.assetTypeId == (int) AssetCategoryEnum.MOVE_CHARACTER
                            ? LoadCharacterSprite(assetManageData.imageSettings[3].path)
                            : LoadObjectSprite(assetManageData.imageSettings[3].path);
                _damage = assetManageData.assetTypeId == (int) AssetCategoryEnum.MOVE_CHARACTER
                            ? LoadCharacterSprite(assetManageData.imageSettings[4].path)
                            : LoadObjectSprite(assetManageData.imageSettings[4].path);

                //コマ数入ってくる
                _downIndex = assetManageData.imageSettings[0].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[0].animationFrame;
                _leftIndex = assetManageData.imageSettings[1].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[1].animationFrame;
                _rightIndex = assetManageData.imageSettings[2].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[2].animationFrame;
                _upIndex = assetManageData.imageSettings[3].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[3].animationFrame;
                _damageIndex = assetManageData.imageSettings[4].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[4].animationFrame;

                // アニメーション速度
                _downSpeed = assetManageData.imageSettings[0].animationSpeed == 0
                    ? 1
                    : assetManageData.imageSettings[0].animationSpeed;
                _leftSpeed = assetManageData.imageSettings[1].animationSpeed == 0
                    ? 1
                    : assetManageData.imageSettings[1].animationSpeed;
                _rightSpeed = assetManageData.imageSettings[2].animationSpeed == 0
                    ? 1
                    : assetManageData.imageSettings[2].animationSpeed;
                _upSpeed = assetManageData.imageSettings[3].animationSpeed == 0
                    ? 1
                    : assetManageData.imageSettings[3].animationSpeed;
                _damageSpeed = assetManageData.imageSettings[4].animationSpeed == 0
                    ? 1
                    : assetManageData.imageSettings[4].animationSpeed;


                if (!_isBush)
                {
                    SetUpNormalSprites();
                    _downDirectionSprite = _downDirectionSpriteNormal;
                    _leftDirectionSprite = _leftDirectionSpriteNormal;
                    _rightDirectionSprite = _rightDirectionSpriteNormal;
                    _upDirectionSprite = _upDirectionSpriteNormal;
                    _damageSprite = _damageSpriteNormal;
                }
                else
                {
                    SetUpBushSprites();
                    _downDirectionSprite = _downDirectionSpriteBush;
                    _leftDirectionSprite = _leftDirectionSpriteBush;
                    _rightDirectionSprite = _rightDirectionSpriteBush;
                    _upDirectionSprite = _upDirectionSpriteBush;
                    _damageSprite = _damageSpriteBush;
                }
            }
            //画像のIDが入ってこなかったら画像を消す
            else
            {
                _downDirectionSprite = null;
                _leftDirectionSprite = null;
                _rightDirectionSprite = null;
                _upDirectionSprite = null;
                _damageSprite = null;

                _downDirectionSpriteNormal = null;
                _leftDirectionSpriteNormal = null;
                _rightDirectionSpriteNormal = null;
                _upDirectionSpriteNormal = null;
                _damageSpriteNormal = null;

                _downDirectionSpriteBush = null;
                _leftDirectionSpriteBush = null;
                _rightDirectionSpriteBush = null;
                _upDirectionSpriteBush = null;
                _damageSpriteBush = null;

                if (_currentSprite != null)
                    _currentSprite.sprite = null;
                if (_image != null)
                    _image.enabled = false;
            }
        }

        private void SetUpNormalSprites() {
            if (_down != null)
            {
                _downDirectionSpriteNormal = new SpriteDataModel(_down, _downIndex, _downSpeed);
            }
            else
            {
                _downDirectionSpriteNormal = null;
            }
            if (_left != null)
            {
                _leftDirectionSpriteNormal = new SpriteDataModel(_left, _leftIndex, _leftSpeed);
            }
            else
            {
                _leftDirectionSpriteNormal = null;
            }
            if (_right != null)
            {
                _rightDirectionSpriteNormal = new SpriteDataModel(_right, _rightIndex, _rightSpeed);
            }
            else
            {
                _rightDirectionSpriteNormal = null;
            }
            if (_up != null)
            {
                _upDirectionSpriteNormal = new SpriteDataModel(_up, _upIndex, _upSpeed);
            }
            else
            {
                _upDirectionSpriteNormal = null;
            }
            if (_damage != null)
            {
                _damageSpriteNormal = new SpriteDataModel(_damage, _damageIndex, _damageSpeed);
            }
            else
            {
                _damageSpriteNormal = null;
            }
        }

        private void SetUpBushSprites() {
            if (_down != null)
            {
                _downBush = SetBushToSprite(_down);
                _downDirectionSpriteBush = new SpriteDataModel(_downBush, _downIndex, _downSpeed);
            }
            else
            {
                _downDirectionSpriteBush = null;
            }
            if (_left != null)
            {
                _leftBush = SetBushToSprite(_left);
                _leftDirectionSpriteBush = new SpriteDataModel(_leftBush, _leftIndex, _leftSpeed);
            }
            else
            {
                _leftDirectionSpriteBush = null;
            }
            if (_right != null)
            {
                _rightBush = SetBushToSprite(_right);
                _rightDirectionSpriteBush = new SpriteDataModel(_rightBush, _rightIndex, _rightSpeed);
            }
            else
            {
                _rightDirectionSpriteBush = null;
            }
            if (_up != null)
            {
                _upBush = SetBushToSprite(_up);
                _upDirectionSpriteBush = new SpriteDataModel(_upBush, _upIndex, _upSpeed);
            }
            else
            {
                _upDirectionSpriteBush = null;
            }
            if (_damage != null)
            {
                _damageBush = SetBushToSprite(_damage);
                _damageSpriteBush = new SpriteDataModel(_damageBush, _damageIndex, _damageSpeed);
            }
            else
            {
                _damageSpriteBush = null;
            }
        }

        private void ClearNormalSprites() {
            _downDirectionSprite = null;
            _leftDirectionSprite = null;
            _rightDirectionSprite = null;
            _upDirectionSprite = null;
            _damageSprite = null;
        }

        private void ClearBushSprites() {
            _downDirectionSpriteBush = null;
            _leftDirectionSpriteBush = null;
            _rightDirectionSpriteBush = null;
            _upDirectionSpriteBush = null;
            _damageSpriteBush = null;
        }

        /// <summary>
        /// 茂み時の足元を半透明に
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        private Sprite SetBushToSprite(Sprite sp) {
            Texture2D texture = sp.texture.Copy(FilterBush);
            return Sprite.Create(
                texture: texture,
                rect: new Rect(0, 0, texture.width, texture.height),
                pivot: new Vector2(0.5f, 0.5f)
            );
        }

        private Color[] FilterBush(Color[] data, int w, int h) {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < 48; y++)
                {
                    //茂みの半透明処理
                    if (data.Length > x + y * w)
                    {
                        data[x + y * w] = new Color(data[x + y * w].r, data[x + y * w].g, data[x + y * w].b, data[x + y * w].a * 0.5f);
                    }
                }
            }

            return data;
        }

        // テクスチャUVの設定
        // start:開始UV位置
        // end:終了UV位置
        protected virtual void SetTextureUV(Vector2 start, Vector2 end) {
            // 数値調整して代入
            // マスクをかけている場合、マテリアルを作り直さないと反映しないため、マスクが有効である場合にも生成しなおす
            if (_material == null || transform.GetComponent<Mask>() != null && transform.GetComponent<Mask>().enabled)
            {
                _material = new Material(_image.material.shader);
                _image.material = _material;
            }

            _material.mainTextureOffset = new Vector2(start.x, start.y);
            _material.SetTextureScale("_MainTex", new Vector2(end.x - start.x, end.y));
        }

        // サイズ設定(1.0が100%)
        protected virtual void SetSize(Vector2 size) {
            _image.transform.localScale = size;
        }

        //キャラクターの画像を読み込み直す部分
        //読み込み直すassetsIDが入ります
        public virtual void ReloadCharacterImage(string id) {
            //処理が重複してしまったため「SetUpSprites」を呼び出しています
            SetUpSprites(id);
            //画像の更新
            var beforeDirection = _directionEnum;
            for (var i = 0; i < 4; i++)
                ChangeDirection((CharacterMoveDirectionEnum) i);

            ChangeDirection(beforeDirection);
        }

        /// <summary>
        ///     画像の表示を有効にするかどうかの切り替え
        /// </summary>
        /// <param name="enable">有効か無効か</param>
        public virtual void SetImageEnable(bool enable) {
            if (_currentSprite?.sprite != null && _image != null)
                _image.enabled = enable;
            else if (_image != null)
                _image.enabled = false;
        }

        /// <summary>
        ///     透明状態の設定
        /// </summary>
        /// <param name="transparent">trueで透明、falseで不透明</param>
        public virtual void SetTransparent(bool transparent) {
            if (_image == null)
                return;

            var alpha = transparent ? 0f : 1f;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
        }

        /// <summary>
        /// 不透明度設定（イベントコマンド＞キャラ画像設定から）
        /// </summary>
        /// <param name="opacity">0～1fを設定</param>
        public virtual void SetOpacity(float opacity) {
            if (_image == null)
                return;

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, opacity);
        }

        /// <summary>
        ///     キャラクター用画像の読込
        /// </summary>
        /// <param name="fileName">画像のファイル名</param>
        /// <returns>読み込まれた画像のSprite</returns>
        protected virtual Sprite LoadCharacterSprite(string fileName) {
            if (string.IsNullOrEmpty(fileName)) return null;

            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                "Assets/RPGMaker/Storage/Images/Characters/" + fileName);
        }

        /// <summary>
        ///     オブジェクト用画像の読込
        /// </summary>
        /// <param name="fileName">画像のファイル名</param>
        /// <returns>読み込まれた画像のSprite</returns>
        protected virtual Sprite LoadObjectSprite(string fileName) {
            if (string.IsNullOrEmpty(fileName)) return null;

            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                "Assets/RPGMaker/Storage/Images/Objects/" + fileName);
        }

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------
        protected class SpriteDataModel
        {
            public SpriteDataModel(Sprite sprite, int animationFrame, int animationSpeed) {
                this.sprite = sprite;
                this.animationFrame = animationFrame;
                this.animationSpeed = animationSpeed;
            }

            public Sprite sprite { get; set; }
            public int animationFrame { get; }
            public int animationSpeed { get; }
        }

        public bool IsEnabledImage() {
            return _image != null;
        }
    }

    static class TextureCopyExtension
    {
        public static Sprite ToSprite(this Texture2D tex) {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        public static Texture2D Copy(this Texture2D source, System.Func<Color[], int, int, Color[]> filter = null) {

            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            var renderTexture = new RenderTexture(texture.width, texture.height, 32);

            // もとのテクスチャをRenderTextureにコピー
            Graphics.Blit(source, renderTexture);
            RenderTexture.active = renderTexture;

            // RenderTexture.activeの内容をtextureに書き込み
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            RenderTexture.active = null;
            // 不要になったので削除
            RenderTexture.DestroyImmediate(renderTexture);

            if (filter != null && texture != null)
            {
                var pixels = filter(texture.GetPixels(), texture.width, texture.height);
                texture.SetPixels(pixels);
                texture.Apply();
            }

            return texture;
        }
    }//class
}