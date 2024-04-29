using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.GameProgress
{
    public class GameSelfSwitch : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1048") + " : ";
            ret +=
                eventCommand.parameters[0] + " = ";

            if (int.Parse(eventCommand.parameters[1]) == 0)
                ret += "OFF";
            else
                ret += "ON";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}