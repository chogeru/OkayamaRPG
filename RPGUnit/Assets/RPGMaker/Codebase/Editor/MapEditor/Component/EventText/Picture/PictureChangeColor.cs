using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Picture
{
    public class PictureChangeColor : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1130") + " : ";
            ret += "#" + eventCommand.parameters[0] + ", ";
            ret += "(" + eventCommand.parameters[2] + "," +
                   eventCommand.parameters[3] + "," +
                   eventCommand.parameters[4] + "," +
                   eventCommand.parameters[5] + "), ";
            ret += eventCommand.parameters[6] + " " + EditorLocalize.LocalizeText("WORD_1088") + " ";
            if (eventCommand.parameters[7] == "1") ret += "(" + EditorLocalize.LocalizeText("WORD_1087") + ")";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}