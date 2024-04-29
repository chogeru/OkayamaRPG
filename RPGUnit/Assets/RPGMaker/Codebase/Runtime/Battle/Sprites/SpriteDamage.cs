using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    public enum DamageEnum
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Miss
    }

    /// <summary>
    /// ダメージをポップアップさせるスプライト
    /// </summary>
    public class SpriteDamage : Sprite
    {
        /// <summary>
        /// ダメージ画像
        /// </summary>
        private readonly Dictionary<DamageEnum, string> _damageSprites = new Dictionary<DamageEnum, string>
        {
            {DamageEnum.Zero, "Damage_000"},
            {DamageEnum.One, "Damage_001"},
            {DamageEnum.Two, "Damage_002"},
            {DamageEnum.Three, "Damage_003"},
            {DamageEnum.Four, "Damage_004"},
            {DamageEnum.Five, "Damage_005"},
            {DamageEnum.Six, "Damage_006"},
            {DamageEnum.Seven, "Damage_007"},
            {DamageEnum.Eight, "Damage_008"},
            {DamageEnum.Nine, "Damage_009"},
            {DamageEnum.Miss, "Damage_040"}
        };

        /// <summary>
        /// 継続時間
        /// </summary>
        private int _duration;
        /// <summary>
        /// 現在の継続時間
        /// </summary>
        private int _nowDurationCount;
        /// <summary>
        /// MISS表示かどうか
        /// </summary>
        private bool _isMiss;
        /// <summary>
        /// フラッシュの色の配列 [ 赤, 緑, 青, 強さ ]
        /// </summary>
        private List<float> _flashColor;

        /// <summary>
        /// damage表示中かどうか
        /// </summary>
        private bool _isPlaying;
        /// <summary>
        /// damage用の画像
        /// </summary>
        private readonly List<Image> _numbers = new List<Image>();
        /// <summary>
        /// damage表示を行うRootObject
        /// </summary>
        private GameObject _rootObj;
        /// <summary>
        /// damageを跳ねるための補正値
        /// </summary>
        private List<float> _ry = new List<float> { 0, 5, 4, 3, 2, 1, 0, -1, -2, -3, -4, -5, 3, 2, 1, 0, -1, -2, -3 };
        /// <summary>
        /// damage表示用のSprite
        /// </summary>
        private readonly Dictionary<DamageEnum, UnityEngine.Sprite> _sprites =
            new Dictionary<DamageEnum, UnityEngine.Sprite>();

        /// <summary>
        /// ダメージ表示対象が敵かどうか
        /// </summary>
        private bool _isEnemy = false;
        /// <summary>
        /// ダメージ表示対象が敵かどうか
        /// </summary>
        public void SetIsEnemy(bool isEnemy) {
            _isEnemy = isEnemy;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize() {
            base.Initialize();

            //各種初期化
            gameObject.AddComponent<RectTransform>();
            gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.0f);
            gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.0f);
            gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.0f);
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);

            _duration = 90;
            _flashColor = new List<float> { 0f, 0f, 0f, 0f };

            foreach (var damageSprite in _damageSprites.Select((value, index) => new { value, index }))
            {
                var sp = ImageManager.LoadDamage(damageSprite.value.Value);
                _sprites.Add((DamageEnum) damageSprite.index, sp);
            }

            _rootObj = new GameObject();

            //Y座標は親から取得
            float posY = transform.parent.GetComponent<RectTransform>().anchoredPosition.y - 
                         transform.parent.GetComponent<RectTransform>().sizeDelta.y / 2f + 30.0f;

            //X座標は敵と味方で取得する場所が異なる
            float posX = 0;
            if (_isEnemy)
                posX = transform.parent.parent.GetComponent<RectTransform>().anchoredPosition.x +
                       transform.parent.GetComponent<RectTransform>().anchoredPosition.x;
            else
                posX = transform.parent.GetComponent<RectTransform>().anchoredPosition.x;

            //エフェクトを利用する際にマスクを使うため、対象の下に数値を入れると、数値にまでマスクがかかる
            //そのため、SpritesetBattle下に配置する
            _rootObj.transform.SetParent(GameObject.Find("SpriteSetBattle").transform);
            _rootObj.transform.localPosition = new Vector3(posX, posY, -2f);

            _rootObj.transform.localScale = new Vector3(1f, 1f, 1f);
            for (var i = 0; i < 9; i++)
            {
                var child = new GameObject();
                _numbers.Add(child.AddComponent<Image>());
                _numbers[i].color = new Color(1f, 1f, 1f, 0f);
                child.transform.SetParent(_rootObj.transform);
                child.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            _rootObj.SetActive(false);
            _initialized = true;
        }

        /// <summary>
        /// 対象に対する準備
        /// </summary>
        /// <param name="target"></param>
        public void Setup(GameBattler target) {
            var result = target.Result;
            if (result == null) return;

            if (!_isPlaying)
            {
                _rootObj.SetActive(true);
                _duration = 90;
                _nowDurationCount = 0;
                _flashColor = new List<float> { 1f, 1f, 1f, 1f };
                _isPlaying = true;

                TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
            }

            if (result.Missed || result.Evaded)
            {
                _isMiss = true;
                CreateMiss();
            }
            else if (result.HpAffected)
            {
                _isMiss = false;
                CreateDigits(result.HpDamage);

                if (result.HpDamage < 0)
                {
                    SetupHealEffect();
                }
            }
            else if (target.IsAlive() && result.MpDamage != 0)
            {
                _isMiss = false;
                CreateDigits(result.MpDamage);

                if (result.MpDamage < 0)
                {
                    SetupHealEffect();
                }
            }

            if (result.Critical)
            {
                _isMiss = false;
                SetupCriticalEffect();
            }
        }

        /// <summary>
        /// クリティカル効果の準備
        /// </summary>
        public void SetupCriticalEffect() {
            _flashColor = new List<float> { 1f, 0f, 0f, 1f };
        }

        /// <summary>
        /// 回復効果の準備
        /// </summary>
        public void SetupHealEffect() {
            _flashColor = new List<float> { 176f / 255f, 1f, 144f / 255f, 1f };
        }

        /// <summary>
        /// 数値の幅(ピクセル)を返す
        /// </summary>
        /// <returns></returns>
        public float DigitWidth() {
            return 46f;
        }

        /// <summary>
        /// ミスのスプライトを生成
        /// </summary>
        public void CreateMiss() {
            var w = DigitWidth();
            for (var j = _numbers.Count - 1; j > -1; j--)
            {
                _numbers[j].gameObject.SetActive(true);
                if (j >= 1)
                {
                    _numbers[j].gameObject.SetActive(false);
                }
                else
                {
                    _numbers[j].gameObject.SetActive(true);
                    _numbers[j].sprite = _sprites[DamageEnum.Miss];
                    _numbers[j].transform.localPosition = new Vector2(_numbers[j].transform.localPosition.x, _numbers[j].sprite.rect.height / 2.0f);
                    _numbers[j].SetNativeSize();
                    _numbers[j].transform.localPosition = new Vector2(Vector3.zero.x, _numbers[j].transform.localPosition.y);
                }
            }
        }

        /// <summary>
        /// 指定行位置に数値スプライトを生成
        /// </summary>
        /// <param name="baseRow"></param>
        /// <param name="value"></param>
        public void CreateDigits(int value) {
            var str = Math.Abs(value).ToString();
            var w = DigitWidth();
            float strCnt = str.Length / 2f - 0.5f;
            for (var j = _numbers.Count - 1; j > -1; j--)
            {
                _numbers[j].gameObject.SetActive(true);
                if (j >= str.Length)
                {
                    _numbers[j].gameObject.SetActive(false);
                }
                else
                {
                    var n = int.Parse(str[j].ToString());
                    _numbers[j].sprite = CreateImage(n);
                    _numbers[j].transform.localPosition = new Vector2(_numbers[j].transform.localPosition.x, _numbers[j].sprite.rect.height / 2.0f);
                    _numbers[j].SetNativeSize();
                    _numbers[j].transform.localPosition = new Vector2(strCnt * w, _numbers[j].transform.localPosition.y);
                    strCnt = strCnt - 1f;
                }
            }
        }

        /// <summary>
        /// 指定された数値のSpriteデータを返却
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private UnityEngine.Sprite CreateImage(int n) {
            return _sprites[(DamageEnum) n];
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public void UpdateTimeHandler() {
            if (!_initialized) return;
            try
            {
                if (_nowDurationCount < _duration)
                {
                    for (var i = 0; i < _numbers.Count; i++)
                        if (_numbers[i].sprite != null)
                            UpdateChild(i, _numbers[i]);
                    _nowDurationCount++;
                }
                else
                {
                    _isPlaying = false;
                    _rootObj.SetActive(false);
                    _nowDurationCount = 0;
                    for (int i = 0; i < _numbers.Count; i++)
                    {
                        _numbers[i].sprite = null;
                        _numbers[i].gameObject.SetActive(false);
                        _numbers[i].color = new Color(1f, 1f, 1f, 0f);
                    }
                    TimeHandler.Instance.RemoveTimeAction(UpdateTimeHandler);

                    //最後に破棄
                    Destroy(_rootObj);
                    Destroy(this);
                }
            }
            catch (Exception)
            {
                //万が一ここに来てしまったら終了処理
                TimeHandler.Instance.RemoveTimeAction(UpdateTimeHandler);
            }
        }

        /// <summary>
        /// 指定小スプライトをアップデート
        /// </summary>
        /// <param name="i"></param>
        /// <param name="image"></param>
        public void UpdateChild(int i, Image image) {
            //各数値ごとに補正値を設定
            float ry = 0;
            if (_isMiss)
            {
                if (_nowDurationCount >= 0 && _nowDurationCount < _ry.Count)
                {
                    ry = _ry[_nowDurationCount] * 5.0f;
                }
            }
            else if (_nowDurationCount - i >= 0 && _nowDurationCount - i < _ry.Count)
            {
                ry = _ry[_nowDurationCount - i] * 5.0f;
            }

            //補正値分座標をずらす
            image.transform.localPosition =
                new Vector2(image.transform.localPosition.x, image.transform.localPosition.y + ry);

            //色の設定
            image.color = new Color(_flashColor[0], _flashColor[1], _flashColor[2], _flashColor[3]);
        }

        /// <summary>
        /// 再生されているか
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying() {
            return _duration > 0;
        }
    }
}
