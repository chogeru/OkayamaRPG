using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// パーティコマンド( [戦う][逃げる] )を表示するウィンドウ
    /// </summary>
    public class WindowPartyCommand : WindowCommand
    {
        /// <summary>
        /// 最初に選択されるボタン
        /// </summary>
        private GameObject _focusButton;

        /// <summary>
        /// 初期化
        /// </summary>
        override public void Initialize() {
            base.Initialize();
            Openness = 0;
            Deactivate();

            //共通UIの適応を開始
            Init();

            // 選択UIのナビゲーションを設定する
            var selects = gameObject.transform.Find("WindowArea/List").gameObject.GetComponentsInChildren<Selectable>();
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Length];
                selects[i].navigation = nav;
            }

            // ボタン取得
            _focusButton = gameObject.transform.Find("WindowArea/List/Fight").gameObject;
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public override void Show() {
            // 選択UIのナビゲーションを設定する
            base.Show();
            var selects = gameObject.transform.Find("WindowArea/List").gameObject.GetComponentsInChildren<Selectable>();
            for (var i = 0; selects != null && i < selects.Length; i++)
            {
                selects[i].GetComponent<WindowButtonBase>().SetEnabled(true);
            }
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        public override void Hide() {
            // 選択UIのナビゲーションを設定する
            base.Hide();
            var selects = gameObject.transform.Find("WindowArea/List").gameObject.GetComponentsInChildren<Selectable>();
            for (var i = 0; selects != null && i < selects.Length; i++)
            {
                selects[i].GetComponent<WindowButtonBase>().SetEnabled(false);
            }
        }

        /// <summary>
        /// メニューに全項目を追加。 個々の追加は addCommand で行っている
        /// </summary>
        public override void MakeCommandList() {
            AddCommand(TextManager.fight, "fight");
            AddCommand(TextManager.escape, "escape", BattleManager.CanEscape());

            //逃走可否に応じてボタンの有効無効を切り替える
            if (BattleManager.CanEscape())
            {
                transform.Find("WindowArea/List/Escape").GetComponent<WindowButtonBase>().SetGray(false);
            }
            else
            {
                transform.Find("WindowArea/List/Escape").GetComponent<WindowButtonBase>().SetGray(true);
            }
        }

        /// <summary>
        /// 各コマンドのローカライズ
        /// </summary>
        private void WordsSet() {
            TextMP fight = transform.Find("WindowArea/List/Fight/Text").GetComponent<TextMP>();
            TextMP escape = transform.Find("WindowArea/List/Escape/Text").GetComponent<TextMP>();

            fight.text = TextManager.fight;
            escape.text = TextManager.escape;
        }

        /// <summary>
        /// コマンドを設定
        /// </summary>
        public void Setup() {
            // ボタンを選択状態にする
            if (_focusButton != null)
                EventSystem.current.SetSelectedGameObject(_focusButton);

            ClearCommandList();
            MakeCommandList();
            Refresh();
            Select(0);
            Activate();
            Open();
            WordsSet();
        }
    }
}