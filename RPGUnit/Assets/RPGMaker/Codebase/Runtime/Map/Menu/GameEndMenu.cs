using RPGMaker.Codebase.Runtime.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class GameEndMenu : WindowBase
    {
        [SerializeField] private GameObject _backObject = null;
        [SerializeField] private GameObject _gameEndObject = null;

        private TextMP _backText;

        //変更されるテキスト部分用
        private TextMP _gameEndText;
        private MenuBase _menuBase;
        private bool _isInput = false;

        public void Init(WindowBase manager) {
#if UNITY_SWITCH || UNITY_PS4
            _backObject.SetActive(false);
#endif
            _menuBase = manager as MenuBase;

            //変更する部分のテキストの取得
            _gameEndText = _gameEndObject.transform.Find("Text").GetComponent<TextMP>();
            _backText = _backObject.transform.Find("Text").GetComponent<TextMP>();

            _gameState = GameStateHandler.GameState.MENU;
            _isInput = true;

            //十字キーでの操作登録
            var selects = transform.Find("MenuArea").GetComponentsInChildren<Button>();
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Length];

                selects[i].navigation = nav;
                selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
            }

            selects[0].Select();
            Init();
        }

        public void OpenEnd() {
            //用語される文字列の代入
            //タイトルへ
            _gameEndText.text = TextManager.toTitle;
            //戻る
            _backText.text = TextManager.cancel;
        }

        //もどる項目が押された時のイベント
        public void BackEvent() {
            if (!_isInput) return;
            _isInput = false;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        //タイトルへの項目が押された時のイベント
        public void GameEndEvent() {
            if (!_isInput) return;
            _isInput = false;
            MenuManager.IsEndGameToTitle = true;
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.MENU);
            GameStateHandler.SetGameState(GameStateHandler.GameState.BEFORE_TITLE);
            //画面をフェードアウトする
            HudDistributor.Instance.StaticHudHandler().DisplayInitByScene();
            HudDistributor.Instance.StaticHudHandler().FadeOut(EndGame, UnityEngine.Color.black, 0.5f, true);

            
            void EndGame() {
                _menuBase.EndGame();
            }
        }

        public new void Back() {
            _menuBase.BackMenu();
        }
    }
}