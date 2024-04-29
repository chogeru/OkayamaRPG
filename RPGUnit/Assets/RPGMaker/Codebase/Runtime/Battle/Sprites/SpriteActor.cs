using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// アニメーション用クラス
    /// </summary>
    public class SpriteActorMotion
    {
        public int    index;
        public bool   loop;
        public string nameForUnity;
    }

    /// <summary>
    /// サイドビューのアクター表示用のスプライト
    /// </summary>
    public class SpriteActor : SpriteBattler
    {
        /// <summary>
        /// アクター
        /// </summary>
        private GameActor _actor;
        /// <summary>
        /// SVの画像ファイル名(拡張子を含まない)
        /// </summary>
        private string _battlerName = "";
        /// <summary>
        /// SVの画像を分割したもの
        /// </summary>
        private readonly List<string> _fileNames = new List<string>();
        /// <summary>
        /// モーションの最大カウンタ
        /// </summary>
        private int _motionMax;
        /// <summary>
        /// モーションのカウンタ
        /// </summary>
        private int _motionCount;
        /// <summary>
        /// ループ再生を行わないアニメーション中かどうか
        /// </summary>
        private bool _isWait;
        /// <summary>
        /// メインとなるスプライト
        /// </summary>
        private SpriteBase _mainSprite;
        /// <summary>
        /// AssetManageDataModel
        /// </summary>
        private AssetManageDataModel _manageDataModel;
        /// <summary>
        /// 現在のモーションのindex
        /// </summary>
        private int _motionIndex;
        /// <summary>
        /// 現在のモーション（"wait"など）
        /// </summary>
        private string _motionType = "";
        /// <summary>
        /// 影のスプライト
        /// </summary>
        private Sprite _shadowSprite;
        /// <summary>
        /// ステートのスプライト
        /// </summary>
        private SpriteStateOverlay _stateSprite;
        /// <summary>
        /// SystemSettingDataModel
        /// </summary>
        private SystemSettingDataModel _systemSettingDataModel;
        //private float _startPosition = 300.0f;
        /// <summary>
        /// テクスチャ
        /// </summary>
        private Texture2D    _texture;
        /// <summary>
        /// 武器のスプライト
        /// </summary>
        private SpriteWeapon _weaponSprite;

        /// <summary>
        /// サイドビュー時のモーション指定用の定数
        /// 例えば SpriteActor.Motions["walk"] といった形で使用する
        /// </summary>
        public Dictionary<string, SpriteActorMotion> Motions = new Dictionary<string, SpriteActorMotion>
        {
            {"acvance", new SpriteActorMotion {index = 0, loop = true, nameForUnity = "Move"}},
            {"wait", new SpriteActorMotion {index = 1, loop = true, nameForUnity = "Wait"}},
            {"chant", new SpriteActorMotion {index = 2, loop = true, nameForUnity = "Wait"}},
            {"defence", new SpriteActorMotion {index = 3, loop = true, nameForUnity = "Guard"}},
            {"damage", new SpriteActorMotion {index = 4, loop = false, nameForUnity = "Damage"}},
            {"evade", new SpriteActorMotion {index = 5, loop = false, nameForUnity = "Move"}},
            {"trust", new SpriteActorMotion {index = 6, loop = false, nameForUnity = "Attack1"}},
            {"swing", new SpriteActorMotion {index = 7, loop = false, nameForUnity = "Attack2"}},
            {"projectile", new SpriteActorMotion {index = 8, loop = false, nameForUnity = "Attack2"}},
            {"skill", new SpriteActorMotion {index = 9, loop = false, nameForUnity = "Wait"}},
            {"magic", new SpriteActorMotion {index = 10, loop = false, nameForUnity = "Wait"}},
            {"item", new SpriteActorMotion {index = 11, loop = false, nameForUnity = "Move"}},
            {"escape", new SpriteActorMotion {index = 12, loop = true, nameForUnity = "Escape"}},
            {"win", new SpriteActorMotion {index = 13, loop = true, nameForUnity = "Win"}},
            {"dying", new SpriteActorMotion {index = 14, loop = true, nameForUnity = "Wait"}},
            {"abnormal", new SpriteActorMotion {index = 15, loop = true, nameForUnity = "Wait"}},
            {"sleep", new SpriteActorMotion {index = 16, loop = true, nameForUnity = "Wait"}},
            {"dead", new SpriteActorMotion {index = 17, loop = true, nameForUnity = "Die"}}
        };
        
        /// <summary>
        /// アクターのID
        /// </summary>
        /// <returns></returns>
        public string ActorId() {
            return _actor.ActorId;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="battler"></param>
        public override void Initialize(GameBattler battler) {
            base.Initialize(battler);
            MoveToStartPosition();
            _initialized = true;
            _systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            
            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        public override void InitMembers() {
            base.InitMembers();
            CreateShadowSprite();
            CreateWeaponSprite();
            CreateMainSprite();
            CreateStateSprite();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
            TimeHandler.Instance?.RemoveTimeAction(MotionAnimation);
        }

        /// <summary>
        /// 本体のスプライトを生成
        /// </summary>
        public void CreateMainSprite() {
            _mainSprite = this;
            _mainSprite.anchor.x = 0.5;
            _mainSprite.anchor.y = 1;
            _effectTarget = _mainSprite;
        }

        /// <summary>
        /// 影のスプライトを生成
        /// Uniteでは未使用になる予定
        /// </summary>
        public void CreateShadowSprite() {
            _shadowSprite = new GameObject().AddComponent<Sprite>();
            _shadowSprite.gameObject.AddComponent<Image>();
            _shadowSprite.bitmap = ImageManager.LoadSystem("Shadow2");
            _shadowSprite.anchor.x = 0.5;
            _shadowSprite.anchor.y = 0.5;
            _shadowSprite.Y = -2;
            _shadowSprite.transform.SetParent(gameObject.transform);
            _shadowSprite.X = 0;
            _shadowSprite.transform.localPosition = new Vector3(7f, -92f, _shadowSprite.transform.localPosition.z);
            _shadowSprite.transform.localScale = new Vector3(1f, 1f, 1f);
            _shadowSprite.gameObject.SetActive(false);
        }

        /// <summary>
        /// 武器のスプライトを生成
        /// </summary>
        public void CreateWeaponSprite() {
            var obj = transform.Find("Weapon").gameObject;
            if (obj.GetComponent<Image>() == null) obj.gameObject.AddComponent<Image>();
            _weaponSprite = obj.AddComponent<SpriteWeapon>();
            _weaponSprite.gameObject.transform.SetParent(gameObject.transform);
            _weaponSprite.X = 0;
            _weaponSprite.transform.localPosition = new Vector3(-0.3f, 0f, _weaponSprite.transform.localPosition.z);
            _weaponSprite.transform.localScale = new Vector3(1f, 1f, 1f);
            _weaponSprite.gameObject.SetActive(false);
        }

        /// <summary>
        /// ステートのスプライトを生成
        /// </summary>
        public async void CreateStateSprite() {
            //画面に生成されていないと、以降の処理で座標計算が行えないため待つ
            await Task.Delay(1000 / 60);

            try
            {
                _stateSprite = new GameObject().AddComponent<SpriteStateOverlay>();
                _stateSprite.transform.SetParent(gameObject.transform);
                _stateSprite.Initialize();

                if (_actor != null)
                    _stateSprite.Setup((GameBattler) _actor);
            }
            catch (Exception) { 
            }
        }

        /// <summary>
        /// バトラーを設定
        /// </summary>
        /// <param name="battler"></param>
        public override void SetBattler(GameBattler battler) {
            base.SetBattler(battler);

            var changed = battler != _actor;
            if (changed)
            {
                //アクター情報を保持
                _actor = (GameActor) battler;
                //Uniteでは初期化タイミングの問題があるため、StartMotion前にUpdateBitmapを実行し、画像データの読み込みを行う
                UpdateBitmap();
                //座標設定
                if (battler != null) SetActorHome(battler.Index());
                //初期モーション設定
                StartEntryMotion();
                //ステート設定
                if (_stateSprite != null)
                    _stateSprite.Setup(battler);
                //武器設定
                _weaponSprite.Setup(_actor.WeaponImageId());
            }
        }

        /// <summary>
        /// 開始点に移動
        /// </summary>
        public void MoveToStartPosition() {
            StartMove(300, 0, 0);
        }

        /// <summary>
        /// 指定隊列番号から基点を設定
        /// </summary>
        /// <param name="index"></param>
        public void SetActorHome(int index) {
            if (_systemSettingDataModel == null)
                _systemSettingDataModel = DataManager.Self().GetSystemDataModel();

            //パーティメンバーの人数
            int memberCount = DataManager.Self().GetGameParty().BattleMembers().Count;
            var battler = DataManager.Self().GetGameParty().BattleMembers();
            for (int i = 0; i < battler.Count; i++)
            {
                if (battler[i].IsEscaped) memberCount--;
            }

            //画面のサイズの半分から、ユーザーの入力起点を左上にする
            var displaySize = DataManager.Self().GetSystemDataModel()
                .DisplaySize[DataManager.Self().GetSystemDataModel().displaySize];

            //X座標は傾斜で判断する
            float x = _systemSettingDataModel.battleScene.sidePartyPosition[0] -
                      displaySize.x / 2.0f +
                      _systemSettingDataModel.battleScene.sidePartyInclined * 6.0f * ((memberCount - 1) - index * 2);

            //Y座標は間隔で判断する
            float y = displaySize.y / 2.0f - 
                      _systemSettingDataModel.battleScene.sidePartyPosition[1] +
                      (48.0f + 4.0f * _systemSettingDataModel.battleScene.sideActorSpace) * ((memberCount - 1) - index * 2);

            //計算した座標にアクターを表示
            SetHome(x, y);
        }

        /// <summary>
        /// 逃走済みかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsEscaped() {
            return _actor.IsEscaped;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public override void UpdateTimeHandler() {
            if (!_initialized) return;
            base.UpdateTimeHandler();

            //既に逃走済みの場合
            if (_actor.IsEscaped)
            {
                gameObject.SetActive(false);
                TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
                return;
            }

            UpdateShadow();
            if (_actor != null) UpdateMotion();
        }

        /// <summary>
        /// 影のアップデート
        /// </summary>
        public void UpdateShadow() {
            _shadowSprite.Visible = _actor != null;
        }

        /// <summary>
        /// 主要なアップデート
        /// </summary>
        public override void UpdateMain() {
            base.UpdateMain();
            if (_actor.IsSpriteVisible() && !IsMoving()) UpdateTargetPosition();
        }

        /// <summary>
        /// モーションの準備
        /// </summary>
        public void SetupMotion() {
            if (_actor.IsMotionRequested())
            {
                StartMotion(_actor.MotionType());
                _actor.ClearMotion();

                if (_isWait)
                {
                    _isWait = false;
                    SetupWeaponAnimation();
                }
            }
        }

        /// <summary>
        /// 武器アニメの準備
        /// </summary>
        public void SetupWeaponAnimation() {
            _weaponSprite.gameObject.SetActive(true);
            _weaponSprite.Setup(_actor.WeaponImageId());
            _weaponSprite.StartMotion();
            _actor.ClearWeaponAnimation();
        }

        /// <summary>
        /// 指定モーションを開始
        /// </summary>
        /// <param name="motionType"></param>
        public void StartMotion(string motionType) {
            //Uniteでは素材が、各モーションごとに別になっているため、モーションタイプに応じて、素材のindex番号を設定する
            _motionIndex = Motions[motionType].index;
            if (_manageDataModel != null && _manageDataModel.imageSettings.Count > _motionIndex)
            {
                _motionMax = _manageDataModel.imageSettings[_motionIndex].animationFrame;
            }
            else
            {
                _motionMax = 1;
            }

            if (_battlerName != "")
            {
                //同一モーションなら処理しない
                if (_motionType == motionType)
                    return;

                //初期化
                _motionType = motionType;
                _motionCount = 0;

                // 画像設定
                if (_fileNames.Count > _motionIndex)
                {
                    _texture = (Texture2D) ImageManager.LoadSvActor(_fileNames[_motionIndex]).UnityTexture;
                }
                else
                {
                    _texture = null;
                }

                _mainSprite.SetSprite(_texture);
                _mainSprite.SetTextureUV(new Vector2(0, 0), new Vector2(1.0f / _motionMax, 1));
                var width = _texture?.width / (_motionMax) ?? 0;
                _mainSprite.SetSize(new Vector2(width, _texture?.height ?? 0));
                _mainSprite.enabled = true;

                //親オブジェクトのサイズも合わせる
                transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(width, _texture?.height ?? 0);

                //アニメーション速度の定義は、アニメーションが1巡するまでのフレーム数(60fps)で定義されている
                //1コマを切り替える時間は、1巡するまでの速度 / コマ数 / 60f
                float frameTime = 1f;
                if (_manageDataModel != null && _manageDataModel.imageSettings.Count > _motionIndex)
                {
                    _motionMax = _manageDataModel.imageSettings[_motionIndex].animationFrame;
                    frameTime = 1.0f * _manageDataModel.imageSettings[_motionIndex].animationSpeed / _motionMax / 20.0f;
                }

                //武器による攻撃アニメーションの場合は、武器アニメーションのセットアップを後程実施
                //実施タイミングは、次回 MotionAnimation が実行されたとき
                if (motionType == "swing" || motionType == "trust" || motionType == "projectile")
                {
                    if (_actor.IsWeaponAnimationRequested())
                    {
                        _isWait = true;
                    }
                }

                //登録済みモーション更新処理を削除
                TimeHandler.Instance.RemoveTimeAction(MotionAnimation);
                //今回のモーションの速度で、モーション更新処理を登録
                TimeHandler.Instance.AddTimeAction(frameTime, MotionAnimation, true);
            }
        }

        /// <summary>
        /// 目標位置のアップデート
        /// </summary>
        public void UpdateTargetPosition() {
            if (_actor.IsInputting() || _actor.IsActing())
                StepForward();
            else if (_actor.CanMove() && BattleManager.IsEscaped())
                Retreat();
            else if (!InHomePosition()) StepBack();
        }

        /// <summary>
        /// 画像のアップデート
        /// </summary>
        public override void UpdateBitmap() {
            base.UpdateBitmap();
            var name = _actor.BattlerName;
            if (_battlerName != name)
            {
                //Bitmapデータ差し替え
                _battlerName = name;

                // idとファイル名は同名の為そのままロード
#if UNITY_EDITOR
                var inputString = UnityEditorWrapper.AssetDatabaseWrapper
                    .LoadAssetAtPath<TextAsset>("Assets/RPGMaker/Storage/AssetManage/JSON/Assets/" + _battlerName + ".json");
                if (inputString != null)
                {
                    _manageDataModel = JsonHelper.FromJson<AssetManageDataModel>(inputString.text);
                }
#else
                _manageDataModel = ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + _battlerName + ".asset") as AssetManageDataModel;
#endif
                //コマ毎にデータを詰め、最大コマ数を設定する
                if (_manageDataModel != null)
                {
                    foreach (var image in _manageDataModel.imageSettings)
                        _fileNames.Add(image.path.Replace(".png", ""));
                    _motionMax = _manageDataModel.imageSettings.Count > 0
                        ? _manageDataModel.imageSettings[0].animationFrame
                        : 1;
                }
            }
        }

        /// <summary>
        /// フレームのアップデート
        /// Uniteでは各モーションは別で実装しているため、ここではベースを呼ぶのみとする
        /// </summary>
        public override void UpdateFrame() {
            base.UpdateFrame();
        }

        /// <summary>
        /// 攻撃以外のモーションのアップデート
        /// </summary>
        private void MotionAnimation() {
            _mainSprite.texture = new OverrideTexture(_texture);
            _mainSprite.SetTextureUV(
                new Vector2(1.0f / _motionMax * _motionCount, 0),
                new Vector2(1.0f / _motionMax * (_motionCount + 1), 1));
            _motionCount++;

            if (_motionCount == _motionMax)
            {
                if (Motions[_motionType].loop)
                    _motionCount = 0;
                else
                    _motionCount--;
            }
        }

        /// <summary>
        /// 移動のアップデート
        /// </summary>
        public override void UpdateMove() {
            var bitmap = _mainSprite.texture;
            if (bitmap != null && bitmap.IsReady()) base.UpdateMove();
        }

        /// <summary>
        /// モーションのアップデート
        /// </summary>
        public void UpdateMotion() {
            SetupMotion();

            if (_actor.IsMotionRefreshRequested())
            {
                RefreshMotion();
                _actor.ClearMotion();
            }
        }

        /// <summary>
        /// モーションを再設定
        /// </summary>
        public void RefreshMotion() {
            if (_actor != null)
            {
                if (_motionType == "defence" && !BattleManager.IsInputting()) 
                    return;

                var stateMotion = _actor.StateMotionIndex();
                if (_actor.IsInputting() || _actor.IsActing())
                    StartMotion("acvance");
                else if (stateMotion == 3)
                    StartMotion("dead");
                else if (stateMotion == 2)
                    StartMotion("sleep");
                else if (_actor.IsChanting())
                    StartMotion("chant");
                else if (_actor.IsGuard() || _actor.IsGuardWaiting())
                    StartMotion("defence");
                else if (stateMotion == 1)
                    StartMotion("abnormal");
                else if (_actor.IsDying())
                    StartMotion("dying");
                else if (_actor.IsUndecided())
                    StartMotion("acvance");
                else
                    StartMotion("wait");
            }
        }

        /// <summary>
        /// 入場モーションの準備
        /// </summary>
        public void StartEntryMotion() {
            if (_actor != null && _actor.CanMove())
            {
                StartMotion("acvance");
                StartMove(0, 0, 30);
            }
            else if (!IsMoving())
            {
                RefreshMotion();
                StartMove(0, 0, 0);
            }
        }

        /// <summary>
        /// 入場モーションを終わりまで飛ばす
        /// </summary>
        public void SetEndEntryMotion() {
            X = _homeX + _offsetX + _targetOffsetX ?? 0;
            Y = _homeY + _offsetY + _targetOffsetY ?? 0;
            OnMoveEnd();
            RefreshMotion();
            StartMove(0, 0, 0);
        }

        /// <summary>
        /// 入場モーションを終わりまで飛ばす（プレビュー用）
        /// </summary>
        public void SetEndEntryMotionPreview() {
            X = _homeX + _offsetX + _targetOffsetX ?? 0;
            Y = _homeY + _offsetY + _targetOffsetY ?? 0;
            OnMoveEnd();
            RefreshMotion();
            StartMove(0, 0, 0);
        }


        /// <summary>
        /// 前進しているか
        /// </summary>
        public void StepForward() {
            StartMove(-48, 0, 12);
        }

        /// <summary>
        /// 後退しているか
        /// </summary>
        public void StepBack() {
            StartMove(0, 0, 12);
        }

        /// <summary>
        /// モーションの再開
        /// </summary>
        public void Retreat() {
            StartMove(500, 0, 30);
        }

        /// <summary>
        /// 移動が終わった時に呼ばれるハンドラ
        /// </summary>
        public override void OnMoveEnd() {
            base.OnMoveEnd();
            //アニメーション方法を変更していることが理由の可能性あり
            if (!BattleManager.IsBattleEnd())
            {
                RefreshMotion();
            }
        }

        /// <summary>
        /// ダメージの xオフセットを返す
        /// </summary>
        /// <returns></returns>
        public override float DamageOffsetX() {
            return 24f;
        }

        /// <summary>
        /// ダメージの yオフセットを返す
        /// </summary>
        /// <returns></returns>
        public override float DamageOffsetY() {
            return 0;
        }
    }
}