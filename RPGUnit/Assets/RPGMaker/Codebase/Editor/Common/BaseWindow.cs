using RPGMaker.Codebase.Editor.Common.View;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    ///     メニュー、ヒエラルキー、シーン、インスペクターウィンドウの基底抽象クラス.
    /// </summary>
    public abstract class BaseWindow : EditorWindow
    {
        /// <summary>
        /// ウィンドウがキーボードフォーカスを失ったときに呼び出されます。
        /// </summary>
        private void OnLostFocus()
        {
            foreach (var imTextField in rootVisualElement.Query<ImTextField>().ToList())
            {
                imTextField.OnOwnerWindowLostFocus();
            }
        }
    }
}