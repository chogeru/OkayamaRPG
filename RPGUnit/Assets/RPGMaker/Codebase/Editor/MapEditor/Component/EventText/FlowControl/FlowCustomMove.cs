using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.FlowControl
{
    public class FlowCustomMove : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            var name = "";
            var text = "";
            if (eventCommand.parameters[1] == "-1")
                name = EditorLocalize.LocalizeText("WORD_0920");
            else if (eventCommand.parameters[1] == "-2")
                name = EditorLocalize.LocalizeText("WORD_0860");
            else
                name = GetEventDisplayName(eventCommand.parameters[1]);

            ret += "◆" + EditorLocalize.LocalizeText("WORD_2800") + " : " + name;

            text = "";

            if (eventCommand.parameters[2] == "1")
            {
                if (text != "") text += ",";
                text += EditorLocalize.LocalizeText("WORD_0989");
            }

            if (eventCommand.parameters[3] == "1")
            {
                if (text != "") text += ",";
                text += EditorLocalize.LocalizeText("WORD_0990");
            }

            if (eventCommand.parameters[4] == "1")
            {
                if (text != "") text += ",";
                text += EditorLocalize.LocalizeText("WORD_0952");
            }

            if (text != "") text = " (" + text + ")";


            LabelElement.text = ret + text;
            Element.Add(LabelElement);
            return Element;
        }
    }
}
