using JetBrains.Annotations;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGMaker.Codebase.Runtime.GameOver
{
    /// <summary>
    /// GameOverのScene制御
    /// </summary>
    public class SceneGameOver : SceneBase
    {

        private bool _isInput = true;
        
        protected override void Start() {
            //状態の更新
            GameStateHandler.SetGameState(GameStateHandler.GameState.GAME_OVER);

            //BGM再生
            PlayMe();

            HudDistributor.Instance.StaticHudHandler().DisplayInitByScene();
            HudDistributor.Instance.StaticHudHandler().FadeInFixedBlack(Init,   false, 0.5f, true);
        }

        protected async void PlayMe() {
            SoundManager.Self().Init();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_ME, DataManager.Self().GetSystemDataModel().bgm.gameOverMe);
            await SoundManager.Self().PlayMe();
        }

        protected override void Init() {
            base.Init();
        }

        public override void Update() {
            base.Update();
            if (Input.anyKey && _isInput)
            {
                _isInput = false;
                // HUD系UIハンドリング
                HudDistributor.Instance.AllDestroyHudHandler();
                HudDistributor.Instance.StaticHudHandler().FadeOut(()=>{
                    //状態の更新
                    GameStateHandler.SetGameState(GameStateHandler.GameState.TITLE);
                    SceneManager.LoadScene("Title");
                }, UnityEngine.Color.black, 0.5f, true);
            }
        }

        public void FadeIn(bool isWhite, [CanBeNull] Action callBack = null) {
        }

        private IEnumerator FadeInCoroutine([CanBeNull] Action callBack = null) {
            SetFadeActive(true);
            UpdateFade();
            yield return new WaitForSeconds(0.05f);
            if (fadeDuration > 0)
            {
                StartCoroutine(FadeInCoroutine(callBack));
            }
            else
            {
                SetFadeActive(false);
                callBack?.Invoke();
            }
        }

        public void FadeOut(bool isWhite, [CanBeNull] Action callBack = null) {
        }

        private IEnumerator FadeOutCoroutine([CanBeNull] Action callBack = null) {
            SetFadeActive(true);
            UpdateFade();
            yield return new WaitForSeconds(0.05f);
            if (fadeDuration > 0)
                StartCoroutine(FadeOutCoroutine(callBack));
            else
                callBack?.Invoke();
        }
    }
}