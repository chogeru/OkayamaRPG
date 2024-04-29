using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Timing
{
    public class TimingWait : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1087") + " : " +
                   string.Format("{0} ", eventCommand.parameters[0]) + EditorLocalize.LocalizeText("WORD_1088");

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}