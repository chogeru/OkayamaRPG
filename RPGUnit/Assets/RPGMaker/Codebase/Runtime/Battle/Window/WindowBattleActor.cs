using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using RPGMaker.Codebase.CoreSystem.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 戦闘中にアクターを選択するウィンドウ
    /// </summary>
    public class WindowBattleActor : WindowBattleStatus
    {
        /// <summary>
        /// 初期化
        /// </summary>
        override public void Initialize() {
            base.Initialize();
            Openness = 255;

            //共通UIの適応を開始
            Init();

            Hide();
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        override public void Show() {
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].GetComponent<WindowButtonBase>().SetEnabled(true);
            }
            SetNavigation();
            Select(0);
            base.Show();
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        override public void Hide() {
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].GetComponent<WindowButtonBase>().SetEnabled(false);
            }
            base.Hide();
            DataManager.Self().GetGameParty().Select(null);
        }

        /// <summary>
        /// 指定した番号の項目を選択
        /// </summary>
        /// <param name="index"></param>
        private new void Select(int index) {
            base.Select(index);
            DataManager.Self().GetGameParty().Select(Actor());
        }

        /// <summary>
        /// アクターデータを取得
        /// </summary>
        /// <returns></returns>
        private GameActor Actor() {
            return (GameActor) DataManager.Self().GetGameParty().Members()[Index()];
        }
        
        /// <summary>
        /// ナビゲーション設定
        /// </summary>
        public void SetNavigation() {
            var selects = gameObject.transform.Find("List").gameObject
                .GetComponentsInChildren<Selectable>();
            var buttons = new List<GameObject>();

            List<Selectable> work = new List<Selectable>();
            for (var i = 0; i < selects.Length; i++)
            {
                if (selects[i].transform.Find("Highlight") == null)
                {
                    continue;
                }
                work.Add(selects[i]);
            }

            for (var i = 0; i < work.Count; i++)
            {
                if(work[i].GetComponent<WindowButtonBase>() != null)
                    work[i].GetComponent<WindowButtonBase>().SetRaycastTarget(true);

                buttons.Add(work[i].gameObject);
                work[i].targetGraphic = work[i].transform.Find("Highlight").GetComponent<Image>();
                var nav = work[i].navigation;
                nav.mode = Navigation.Mode.Explicit;

                nav.selectOnRight = work[i < work.Count - 1 ? i + 1 : 0];
                nav.selectOnLeft = work[i == 0 ? work.Count - 1 : i - 1];
                work[i].navigation = nav;
            }
            buttons[0].GetComponent<Button>().Select();
        }
    }
}