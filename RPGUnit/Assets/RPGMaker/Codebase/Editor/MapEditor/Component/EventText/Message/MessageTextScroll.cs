using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    public class MessageTextScroll : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1202") + "(S): " + EditorLocalize.LocalizeText("WORD_1129") +
                   " " + eventCommand.parameters[0];
            if (eventCommand.parameters[1] == "1") ret += ", " + EditorLocalize.LocalizeText("WORD_1214");

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}