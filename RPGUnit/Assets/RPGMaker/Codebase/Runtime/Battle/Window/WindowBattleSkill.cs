namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 戦闘シーンでのスキル選択ウィンドウ
    /// UniteではWindow...系は戦闘シーンでのみ利用のため、ほぼWrapper
    /// </summary>
    public class WindowBattleSkill : WindowSkillList
    {
        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize() {
            base.Initialize();

            Init();
            Hide();
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