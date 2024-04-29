using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 敵の選択用のウィンドウ
    /// </summary>
    public class WindowBattleEnemy : WindowSelectable
    {
        /// <summary>
        /// 敵一覧
        /// </summary>
        private List<GameEnemy> _enemies;

        /// <summary>
        /// 初期化
        /// </summary>
        override public void Initialize() {
            _enemies = new List<GameEnemy>();
            base.Initialize();
            Refresh();

            //共通UIの適応を開始
            Init();

            Hide();

            SetUpNav();
        }

        /// <summary>
        /// ウィンドウが持つ最大項目数を返す
        /// </summary>
        /// <returns></returns>
        public override int MaxItems() {
            return _enemies.Count;
        }

        /// <summary>
        /// 選択中の[敵キャラ]を返す
        /// </summary>
        /// <returns></returns>
        public GameEnemy Enemy() {
            return _enemies[Index()];
        }

        /// <summary>
        /// 選択中の[敵キャラ]の番号を返す
        /// </summary>
        /// <returns></returns>
        public int EnemyIndex() {
            var enemy = Enemy();
            return enemy?.Index() ?? -1;
        }

        /// <summary>
        /// 指定番号の項目を描画
        /// </summary>
        /// <param name="index"></param>
        public override void DrawItem(int index) {
            if (_enemies.Count < index + 1) return;

            selectors[index].SetUp(index, _enemies[index].Name(), (int idx) =>
            {
                Select(index);
                BattleManager.GetSpriteSet().SelectEnemy(_enemies[index]);
            }, OnClickSelection);
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public override void Show() {
            Refresh();
            var selects = gameObject.transform.Find("WindowArea/List").gameObject.GetComponentsInChildren<Selectable>();
            //ボタンの有効状態切り替え
            bool flg = false;
            for (int i = 0; selects != null && i < selects.Length; i++)
            {
                selects[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                //元々フォーカスが当たっていた場所に、フォーカスしなおし
                if (selects[i].GetComponent<WindowButtonBase>().IsHighlight())
                {
                    flg = true;
                    selects[i].GetComponent<Button>().Select();
                }
            }
            if (!flg && selects != null)
            {
                selects[0].GetComponent<Button>().Select();
            }
            EventSystem.current.SetSelectedGameObject(selects[0].gameObject);
            Select(0);

            //ナビゲーション設定しなおし
            SetUpNav();

            base.Show();
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        public override void Hide() {
            base.Hide();
            var selects = gameObject.transform.Find("WindowArea/List").gameObject.GetComponentsInChildren<Selectable>();
            for (int i = 0; selects != null && i < selects.Length; i++)
            {
                selects[i].GetComponent<WindowButtonBase>().SetEnabled(false);
            }
            DataManager.Self().GetGameTroop().Select(null);
            if (BattleManager.GetSpriteSet() != null) 
                BattleManager.GetSpriteSet().SelectEnemy(null);
        }

        /// <summary>
        /// コンテンツの再描画
        /// </summary>
        public new void Refresh() {
            _enemies = DataManager.Self().GetGameTroop().AliveMembers().Aggregate(new List<GameEnemy>(), (l, e) =>
            {
                l.Add((GameEnemy) e);
                return l;
            });
            base.Refresh();
        }

        /// <summary>
        /// 指定した番号の項目を選択
        /// </summary>
        /// <param name="index"></param>
        public new void Select(int index) {
            base.Select(index);
            DataManager.Self().GetGameTroop().Select(Enemy());
        }

        /// <summary>
        /// ナビゲーション設定
        /// </summary>
        private void SetUpNav() {
            // 選択UIのナビゲーションを設定する
            var selects = gameObject.transform.Find("WindowArea/List").gameObject.GetComponentsInChildren<Selectable>();
            var half = (selects.Length + 1) / 2; // 項目の半分の値（切り上げ）
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;

                // 前半部分
                if (half - 1 >= i)
                {
                    nav.selectOnRight = selects[selects.Length % 2 == 1 && i == selects.Length - half ? i : i + half];
                    nav.selectOnLeft = selects[i == 0 ? i : i + half - 1]; // 先頭は自身を設定
                    nav.selectOnDown = selects[i == half - 1 ? i : i + 1]; // 前半の末尾は自身を設定
                    nav.selectOnUp = selects[i == 0 ? i : i - 1]; // 先頭は自身を設定
                }
                // 後半部分
                else
                {
                    nav.selectOnRight =
                        selects[selects.Length % 2 == 0 && i == selects.Length - 1 ? i : i - half + 1]; // 末尾は自身を設定
                    nav.selectOnLeft = selects[i - half];
                    nav.selectOnDown = selects[i == selects.Length - 1 ? i : i + 1]; // 末尾は自身を設定
                    nav.selectOnUp = selects[i == half ? i : i - 1]; // 後半の先頭は自身を設定
                }

                selects[i].navigation = nav;
            }
        }
    }
}