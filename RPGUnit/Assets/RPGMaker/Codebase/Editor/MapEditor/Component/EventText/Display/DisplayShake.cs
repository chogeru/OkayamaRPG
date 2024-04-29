using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Display
{
    public class DisplayShake : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1222") + " : ";
            ret += eventCommand.parameters[0] + ", " +
                   eventCommand.parameters[1] + ", ";
            ret += eventCommand.parameters[2] + " " + EditorLocalize.LocalizeText("WORD_1088") + " ";
            if (eventCommand.parameters[3] == "1") ret += "(" + EditorLocalize.LocalizeText("WORD_1087") + ")";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}