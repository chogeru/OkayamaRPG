using RPGMaker.Codebase.Runtime.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress
{
    public class GameTimer : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private Text _minute;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject _prefab;
        private Text       _second;
        private float      _timerCount;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            if (_prefab == null)
            {
                _prefab = HudDistributor.Instance.NowHudHandler().TimerInitObject();
            }

            _second = _prefab.transform.Find("Canvas/DisplayArea/Timer/Second").GetComponent<Text>();
            _minute = _prefab.transform.Find("Canvas/DisplayArea/Timer/Minute").GetComponent<Text>();
            SceneManager.activeSceneChanged += ActiveSceneChanged;
        }
        
        /// <summary>
        /// シーンが切り替わったときに次のシーンが何かを取得
        /// </summary>
        /// <param name="thisScene"></param>
        /// <param name="nextScene"></param>
        void ActiveSceneChanged (UnityEngine.SceneManagement.Scene thisScene, UnityEngine.SceneManagement.Scene nextScene) {
            //タイトルだったら、非表示
            if (nextScene.name == "Title")
            {
                if (_prefab != null) _prefab.SetActive(false);
            }
        }

        public void SetGameTimer(bool toggle, int count) {
            if (toggle)
            {
                _prefab.gameObject.SetActive(true);
                _timerCount = count;

                _second.text = (_timerCount % 60).ToString();
                _minute.text = (_timerCount / 60).ToString();

                StopMainCoroutine();
            }
            else
            {
                StopMainCoroutine();
                _prefab.SetActive(false);
            }
        }

        public void SetGameTimer(bool toggle, float count) {
            if (toggle)
            {
                _prefab.gameObject.SetActive(true);
                _timerCount = count;

                _second.text = (_timerCount % 60).ToString();
                _minute.text = (_timerCount / 60).ToString();

                StopMainCoroutine();
            }
            else
            {
                StopMainCoroutine();
                _prefab.SetActive(false);
            }
        }

        public float GetGameTimer() {
            if (_prefab.activeSelf)
            {
                return _timerCount;
            }
            return -1;
        }

        void Update() {
            if (_timerCount > 0 && !MenuManager.IsMenuActive && !MenuManager.IsShopActive) TimeCount();
        }

        private void StopMainCoroutine() {
        }

        private void TimeCount() {
            _timerCount -= Time.deltaTime;
            var second = ((int) _timerCount % 60).ToString();
            var minute = ((int) _timerCount / 60).ToString();

            _second.gameObject.GetComponent<Text>().text = PrefixNumber(second);
            _minute.gameObject.GetComponent<Text>().text = PrefixNumber(minute);
        }

        private string PrefixNumber(string str) {
            if (str.Length <= 1)
                str = "0" + str;

            return str;
        }
    }
}