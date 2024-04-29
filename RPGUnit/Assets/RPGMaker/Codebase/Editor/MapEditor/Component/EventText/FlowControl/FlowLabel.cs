using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.FlowControl
{
    public class FlowLabel : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1169") + " : " + eventCommand.parameters[0];

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}