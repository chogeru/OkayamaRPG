using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Initialization.View;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Initialization
{
    /// <summary>
    /// 初期設定のHierarchy
    /// </summary>
    public class InitializationHierarchy : AbstractHierarchy
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InitializationHierarchy() {
            View = new InitializationHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public InitializationHierarchyView View { get; }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
        }
    }
}