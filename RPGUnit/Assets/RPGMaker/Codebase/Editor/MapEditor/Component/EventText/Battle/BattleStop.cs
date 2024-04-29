using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    public class BattleStop : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1115") + " : ";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}