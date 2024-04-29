using RPGMaker.Codebase.Editor.Common;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Inspector
{
    public class InspectorWindow : BaseWindow
    {
        protected void Awake() {
            titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1562"));
        }
    }
}