using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// ステートアイコン( img/system/IconSet.png )表示用スプライト
    /// </summary>
    public class SpriteStateIcon : SpriteBase
    {
        /// <summary>
        /// 対象バトラー
        /// </summary>
        private GameBattler _battler;
        /// <summary>
        /// アニメーション番号
        /// </summary>
        private int _animationIndex;
        /// <summary>
        /// ステートアイコンを表示するGameObject
        /// </summary>
        private GameObject _child;
        /// <summary>
        /// ステートアイコンを表示するImage
        /// </summary>
        private Image _image;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize() {
            base.Initialize();

            InitMembers();

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                TimeHandler.Instance.AddTimeAction(1.0f, UpdateTimeHandler, true);
            }
            else
            {
                UpdateTimeHandler();
            }
#else
            TimeHandler.Instance.AddTimeAction(1.0f, UpdateTimeHandler, true);
#endif
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
        }

        /// <summary>
        /// メンバ変数の初期化
        /// </summary>
        public void InitMembers() {
            //Unite固有初期化処理
            _initialized = true;
            _child = gameObject;
            _image = GetComponent<Image>();
            _child.transform.localScale = new Vector3(1f, 1f, 1f);
            _image.enabled = false;

            //メンバ変数初期化処理
            _battler = null;
            _animationIndex = 0;

            if (gameObject.GetComponent<RectTransform>() == null) gameObject.AddComponent<RectTransform>();
            gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1.0f, 0.0f);
            gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 0.0f);
            gameObject.GetComponent<RectTransform>().pivot = new Vector2(1.0f, 0.0f);
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        }

        /// <summary>
        /// 対象バトラーに対する準備
        /// </summary>
        /// <param name="battler"></param>
        public void Setup(GameBattler battler) {
            _battler = battler;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public override void UpdateTimeHandler() {
            if (!_initialized) return;
            base.UpdateTimeHandler();

            UpdateIcon();
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        }

        /// <summary>
        /// アイコンのアップデート
        /// </summary>
        public void UpdateIcon() {
            var icons = new List<string>();
            if (_battler != null && _battler.IsAlive())
            {
                icons = _battler.AllIcons();
            }

            if (icons.Count > 0)
            {
                _animationIndex++;
                if (_animationIndex >= icons.Count)
                {
                    _animationIndex = 0;
                }

                //拡張子はAllIconsの戻り値で取得しているため、ここからは除外
                var iconSetTexture = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + icons[_animationIndex]);
                var iconTexture = iconSetTexture;

                _image.enabled = true;
                _image.sprite = UnityEngine.Sprite.Create(
                    iconTexture,
                    new Rect(0, 0, iconTexture.width, iconTexture.height),
                    Vector2.zero);
            }
            else
            {
                _animationIndex = 0;
                _image.enabled = false;
            }
        }
    }
}