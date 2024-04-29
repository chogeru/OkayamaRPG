using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Sound.View;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Sound
{
    /// <summary>
    /// サウンドのHierarchy
    /// </summary>
    public class SoundHierarchy : AbstractHierarchy
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SoundHierarchy() {
            View = new SoundHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public SoundHierarchyView View { get; }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
        }

        /// <summary>
        /// BGMのInspector表示
        /// </summary>
        /// <param name="num"></param>
        public void OpenBgmInspector(int num) {
            Inspector.Inspector.SoundView(EditorLocalize.LocalizeText("WORD_0931"), num);
        }

        /// <summary>
        /// SEのInspector表示
        /// </summary>
        /// <param name="num"></param>
        public void OpenSeInspector(int num) {
            Inspector.Inspector.SoundView(EditorLocalize.LocalizeText("WORD_0947"), num);
        }
    }
}