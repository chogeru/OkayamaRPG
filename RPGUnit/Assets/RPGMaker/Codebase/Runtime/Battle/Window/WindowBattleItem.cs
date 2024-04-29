using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 戦闘シーンでのアイテム選択ウィンドウ
    /// UniteではWindow...系は戦闘シーンでのみ利用のため、ほぼWrapper
    /// </summary>
    public class WindowBattleItem : WindowItemList
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void Initialize() {
            base.Initialize();

            //共通UIの適応を開始
            Init();
            Hide();
        }

        /// <summary>
        /// 指定したアイテムが含まれるか
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Includes(GameItem item) {
            return DataManager.Self().GetGameParty().CanUse(item);
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public override void Show() {
            SelectLast();
            ShowHelpWindow();
            base.Show();
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        public override void Hide() {
            HideHelpWindow();
            base.Hide();
        }
    }
}