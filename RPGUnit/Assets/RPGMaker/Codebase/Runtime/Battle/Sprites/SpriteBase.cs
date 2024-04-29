using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// アニメーションする表示物のためのクラス
    /// </summary>
    public class SpriteBase : Sprite
    {
        /// <summary>
        /// 付随する[アニメーション]のスプライト
        /// </summary>
        protected List<CharacterAnimation> _animationSprites;
        /// <summary>
        /// アニメーションを適用する対象
        /// </summary>
        protected SpriteBase _effectTarget;
        /// <summary>
        /// 非表示か
        /// </summary>
        protected bool _hiding;
        /// <summary>
        /// アニメーション実行数
        /// </summary>
        protected int _animationCount;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void Initialize() {
            base.Initialize();
            _animationSprites = new List<CharacterAnimation>();
            _effectTarget = this;
            _hiding = false;
            _initialized = true;
            _animationCount = 0;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public virtual void UpdateTimeHandler() {
            if (!_initialized) return;
            UpdateVisibility();
        }

        /// <summary>
        /// 非表示とする
        /// </summary>
        public virtual void Hide() {
            _hiding = true;
            UpdateVisibility();
        }

        /// <summary>
        /// 表示する
        /// </summary>
        public virtual void Show() {
            _hiding = false;
            UpdateVisibility();
        }

        /// <summary>
        /// 表示状況を更新する
        /// </summary>
        public virtual void UpdateVisibility() {
            Visible = !_hiding;
        }

        /// <summary>
        /// アニメーションの再生開始
        /// </summary>
        /// <param name="animation">再生するアニメーションのデータ</param>
        /// <param name="mirror">反転させるかどうか</param>
        /// <param name="delay">再生開始までの遅延時間（フレーム数）</param>
        /// <returns></returns>
        public async void StartAnimation(AnimationDataModel animation, bool mirror, float delay, bool isActor) {
            //先にアニメーション数をインクリメント
            _animationCount++;

            //指定されたフレーム数待つ
            await Task.Delay(Mathf.RoundToInt(delay / 60f * 1000));
            try
            {
                if (gameObject == null) return;
            } catch (Exception) { return; }

            //アニメーション部品を先に作る
            var characterAnimation = new GameObject().AddComponent<CharacterAnimation>();
            characterAnimation.gameObject.transform.SetParent(gameObject.transform);
            characterAnimation.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            characterAnimation.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _animationSprites.Add(characterAnimation);

            //アニメーションを再生
            characterAnimation.PlayAnimationForBattle(null, () =>
            {
                //アニメーションを破棄
                _animationSprites.Remove(characterAnimation);
                DestroyImmediate(characterAnimation.gameObject);

                //アニメーション数をデクリメント
                _animationCount--;
            }, animation.id, isActor);
        }

        /// <summary>
        /// アニメーション再生中かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsAnimationPlaying() {
            return _animationCount > 0;
        }
    }
}