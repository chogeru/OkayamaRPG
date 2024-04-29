using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// [敵キャラ]表示用のスプライトクラス
    /// </summary>
    public class SpriteEnemy : SpriteBattler
    {
        /// <summary>
        /// [敵キャラ]のデータ
        /// </summary>
        private GameEnemy _enemy;
        /// <summary>
        /// 出現しているか
        /// </summary>
        private bool _appeared;
        /// <summary>
        /// 色相(0〜360)
        /// </summary>
        private float _battlerHue;
        /// <summary>
        /// 画像ファイル名(拡張子を除く)
        /// </summary>
        private string _battlerName = "";

        private string _baattlerId = "";
        /// <summary>
        /// エフェクトタイプ
        /// </summary>
        [CanBeNull] private string _effectType;
        /// <summary>
        /// エフェクト継続時間
        /// </summary>
        private float _effectDuration;
        /// <summary>
        /// エフェクト終了時間
        /// </summary>
        private float _effectDurationMax;
        /// <summary>
        /// 揺れているか
        /// </summary>
        private float _shake;
        /// <summary>
        /// ステートアイコン
        /// </summary>
        private SpriteStateIcon _stateIconSprite;

        /// <summary>
        /// X
        /// </summary>
        public override float X
        {
            get => _x;
            set
            {
                _x = value;
            }
        }

        /// <summary>
        /// Y
        /// </summary>
        public override float Y
        {
            get => _y;
            set
            {
                _y = value;
            }
        }

        /// <summary>
        /// GameEnemy取得
        /// </summary>
        /// <returns></returns>
        public GameEnemy GetGameEnemy() {
            return _enemy;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="battler"></param>
        public override void Initialize(GameBattler battler) {
            base.Initialize(battler);
            _initialized = true;

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
            }
            else
            {
                UpdateTimeHandler();
            }
#else
            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
#endif
        }

        /// <summary>
        /// GameObject破棄時処理
        /// </summary>
        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        public override void InitMembers() {
            base.InitMembers();
            _enemy = null;
            _appeared = false;
            _baattlerId = "";
            _battlerName = "";
            _battlerHue = 0;
            _effectType = null;
            _effectDuration = 0;
            _shake = 0;
            CreateStateIconSprite();
        }

        /// <summary>
        /// アイコン初期化
        /// </summary>
        public async void CreateStateIconSprite() {
            //画面に生成されていないと、以降の処理で座標計算が行えないため待つ
            await Task.Delay(1000 / 60);

            try
            {
                if (gameObject.transform.parent == null) return;
            } catch (Exception) { return; }

            _stateIconSprite = new GameObject().AddComponent<SpriteStateIcon>();
            _stateIconSprite.gameObject.AddComponent<Image>();
            //敵画像にはマスクがかかるため、親に紐づける
            _stateIconSprite.name = "StateIcon";
            _stateIconSprite.transform.SetParent(gameObject.transform.parent);
            _stateIconSprite.transform.localScale = new Vector3(1f, 1f, 1f);
            _stateIconSprite.transform.localPosition = new Vector3(0f, 0f, 0f);
            _stateIconSprite.Initialize();

            if (_enemy != null)
                _stateIconSprite.Setup((GameBattler) _enemy);
        }

        /// <summary>
        /// バトラーを設定
        /// </summary>
        /// <param name="battler"></param>
        public override void SetBattler(GameBattler battler) {
            base.SetBattler(battler);
            _enemy = (GameEnemy) battler;
            SetHome((float) _enemy.ScreenX, (float) _enemy.ScreenY);

            if (_stateIconSprite != null)
                _stateIconSprite.Setup(battler);
        }
        
        /// <summary>
        /// 敵のID返却
        /// </summary>
        /// <returns></returns>
        public string EnemyId() {
            return _enemy.EnemyId;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public override void UpdateTimeHandler() {
            if (!_initialized) return;
            
            //何らかの理由でGameObjectへの参照が切れている場合は、処理を抜ける
            try
            {
                if (gameObject == null) return;
            } catch (Exception)
            {
                return;
            }

            //既に逃走済みの場合
            if (_enemy.IsEscaped)
            {
                gameObject.SetActive(false);
                TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
                return;
            }

            base.UpdateTimeHandler();

            if (_enemy != null)
            {
                UpdateEffect();
                UpdateStateSprite();
            }
        }

        /// <summary>
        /// 画像のアップデート
        /// </summary>
        public override void UpdateBitmap() {
            base.UpdateBitmap();
            var id = _enemy.EnemyId;
            var name = _enemy.OriginalName();
            var hue = _enemy.BattlerHue();
            if (_battlerName != name || _baattlerId != id || _battlerHue != hue)
            {
                _baattlerId = id;
                _battlerName = name;
                _battlerHue = hue;
                LoadBitmap(_enemy.Enemy.images.image, hue);
                InitVisibility();
            }
        }

        /// <summary>
        /// 指定したビットマップ画像を読み込む
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hue"></param>
        public void LoadBitmap(string name, int hue) {
            bitmap = ImageManager.LoadEnemy(name, hue);
        }

        /// <summary>
        /// フレームのアップデート
        /// </summary>
        public override void UpdateFrame() {
            base.UpdateFrame();
        }

        /// <summary>
        /// 位置のアップデート
        /// </summary>
        public override void UpdatePosition() {
            base.UpdatePosition();
            this.X += this._shake;
        }

        /// <summary>
        /// ステートスプライトをアップデート
        /// </summary>
        public void UpdateStateSprite() {
        }

        /// <summary>
        /// 表示状態を初期化
        /// </summary>
        public void InitVisibility() {
            _appeared = _enemy.IsAlive();
            if (!_appeared) 
                Opacity = 0;
            //Uniteではタイミングが若干異なるため、初期化段階で出現状態であれば、このタイミングで出現要求を行う
            else
                StartEffect("appear");
        }

        /// <summary>
        /// エフェクトの準備
        /// </summary>
        public void SetupEffect() {
            //effectType="select" は、選択中の敵をブリンクさせるために追加したもの
            //可能であれば、SpriteBattler 側に処理を移動する
            if (_effectType == "select")
                return;

            if (_appeared && _enemy.IsEffectRequested())
            {
                StartEffect(_enemy.EffectType());
                _enemy.ClearEffect();
            }

            if (!_appeared && _enemy.IsAlive())
                StartEffect("appear");
            else if (_appeared && _enemy.IsHidden()) 
                StartEffect("disappear");
        }

        /// <summary>
        /// 指定したエフェクトの開始
        /// </summary>
        /// <param name="effectType"></param>
        public void StartEffect(string effectType) {
            _effectType = effectType;
            switch (_effectType)
            {
                case "appear":
                    StartAppear();
                    break;
                case "disappear":
                    StartDisappear();
                    break;
                case "whiten":
                    StartWhiten();
                    break;
                case "select":
                    StartSelect();
                    break;
                case "blink":
                    StartBlink();
                    break;
                case "collapse":
                    StartCollapse();
                    break;
                case "bossCollapse":
                    StartBossCollapse();
                    break;
                case "instantCollapse":
                    StartInstantCollapse();
                    break;
            }

            RevertToNormal();
        }

        /// <summary>
        /// 出現の開始
        /// </summary>
        public void StartAppear() {
            _effectDuration = 16;
            _appeared = true;
        }

        /// <summary>
        /// 消滅の開始
        /// </summary>
        public void StartDisappear() {
            _effectDuration = 32;
            _appeared = false;
        }

        /// <summary>
        /// 白く変化開始
        /// </summary>
        public void StartWhiten() {
            _effectDuration = 32;
        }

        /// <summary>
        /// 敵の選択表示アニメーション開始
        /// </summary>
        public void StartSelect() {
            _effectDuration = 1;
        }

        /// <summary>
        /// 点滅の開始
        /// </summary>
        public void StartBlink() {
            _effectDuration = 20;
        }

        /// <summary>
        /// [消滅エフェクト - 通常]開始
        /// </summary>
        public void StartCollapse() {
            _effectDuration = 32;
            _appeared = false;
        }

        /// <summary>
        /// [消滅エフェクト - ボス]開始
        /// </summary>
        public void StartBossCollapse() {
            _effectDuration = gameObject.GetComponent<Image>().sprite.rect.height;
            _effectDurationMax = _effectDuration;
            _appeared = false;
        }

        /// <summary>
        /// [消滅エフェクト - 瞬間消去]開始
        /// </summary>
        public void StartInstantCollapse() {
            _effectDuration = 16;
            _appeared = false;
        }

        /// <summary>
        /// エフェクトをアップデート
        /// </summary>
        public void UpdateEffect() {
            SetupEffect();
            if (_effectDuration > 0)
            {
                _effectDuration--;
                switch (_effectType)
                {
                    case "whiten":
                        UpdateWhiten();
                        break;
                    case "select":
                        UpdateSelect();
                        _effectDuration = 1;
                        break;
                    case "blink":
                        UpdateBlink();
                        break;
                    case "appear":
                        UpdateAppear();
                        break;
                    case "disappear":
                        UpdateDisappear();
                        break;
                    case "collapse":
                        UpdateCollapse();
                        break;
                    case "bossCollapse":
                        UpdateBossCollapse();
                        break;
                    case "instantCollapse":
                        UpdateInstantCollapse();
                        break;
                }

                if (_effectDuration == 0) _effectType = null;
            }
        }

        /// <summary>
        /// 効果が加わっているか
        /// </summary>
        /// <returns></returns>
        public override bool IsEffecting() {
            return _effectType != null;
        }

        /// <summary>
        /// 状態を通常に戻す
        /// </summary>
        public void RevertToNormal() {
            _shake = 0;
            Opacity = 255;
            SetBlendColor(new List<int> {255, 255, 255, 255});
        }

        /// <summary>
        /// 敵の選択アニメーションをやめる
        /// </summary>
        public void ResetSelect() {
            if (_effectType == "select")
            {
                _shake = 0;
                Opacity = 255;
                SetBlendColor(new List<int> { 255, 255, 255, 255 });
                _effectType = null;
            }
        }

        /// <summary>
        /// 白エフェクトをアップデート
        /// </summary>
        public void UpdateWhiten() {
            //Untite用に速度調整
            var alpha = 128 - (32 - _effectDuration) * 5;
            //Uniteではalphaの数値が逆になるので、数値を反転する
            alpha = 255 - alpha;
            //if (_effectDuration == 0) alpha = 255;
            SetBlendColor(new List<int> {255, 255, 255, (int) alpha});
        }
        
        /// <summary>
        /// 敵の選択アニメーション更新
        /// </summary>
        public void UpdateSelect() {
            float phase = Time.time * 2 * Mathf.PI;
            var alpha = 192 - Mathf.Cos(phase) * 64f;
            SetBlendColor(new List<int> { 255, 255, 255, (int) alpha });
        }

        /// <summary>
        /// 点滅をアップデート
        /// </summary>
        public void UpdateBlink() {
            Opacity = _effectDuration % 10 < 5 ? 255 : 0;
        }

        /// <summary>
        /// 出現エフェクトをアップデート
        /// </summary>
        public void UpdateAppear() {
            Opacity = (16 - _effectDuration) * 16;
        }

        /// <summary>
        /// 消滅エフェクトをアップデート
        /// </summary>
        public void UpdateDisappear() {
            Opacity = 256 - (32 - _effectDuration) * 10;
        }

        /// <summary>
        /// [消滅エフェクト - 通常]をアップデート
        /// </summary>
        public void UpdateCollapse() {
            Opacity *= _effectDuration / (_effectDuration + 1);
            SetBlendColor(new List<int> {255, 128, 128, (int) Opacity});
        }

        /// <summary>
        /// [消滅エフェクト - ボス]をアップデート
        /// </summary>
        public void UpdateBossCollapse() {
            _shake = _effectDuration % 2 * 4 - 2;

            //透過度を減らしていく
            Opacity = 255.0f * (_effectDuration / _effectDurationMax);
            SetBlendColor(new List<int> {255, 255, 255, (int) Opacity});

            //左右に揺らしながら下に沈めていく
            transform.GetComponent<AspectRatioFitter>().enabled = false;
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(_shake,
                (-1.0f * (1.0f - _effectDuration / _effectDurationMax)) * gameObject.GetComponent<Image>().sprite.rect.height);


            if (_effectDuration % 20 == 19)
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.bossCollapse2);
                SoundManager.Self().PlaySe();
            }
        }

        /// <summary>
        /// [消滅エフェクト - 瞬間消去]をアップデート
        /// </summary>
        public void UpdateInstantCollapse() {
            Opacity = 0;
            SetBlendColor(new List<int> {255, 255, 255, (int) Opacity});
        }

        /// <summary>
        /// ダメージ表示のOffsetX
        /// </summary>
        /// <returns></returns>
        public override float DamageOffsetX() {
            return 0;
        }

        /// <summary>
        /// ダメージ表示のOffsetY
        /// </summary>
        /// <returns></returns>
        public override float DamageOffsetY() {
            return 0;
        }
    }
}