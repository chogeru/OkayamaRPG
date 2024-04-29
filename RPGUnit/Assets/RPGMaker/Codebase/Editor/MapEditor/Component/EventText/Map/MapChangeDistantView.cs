using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Map
{
    public class MapChangeDistantView : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1181") + " : " + eventCommand.parameters[0];
            if (eventCommand.parameters[1] == "1" || eventCommand.parameters[2] == "1")
            {
                ret += "(";
                if (eventCommand.parameters[1] == "1") ret += EditorLocalize.LocalizeText("WORD_1182");

                if (eventCommand.parameters[1] == "1" && eventCommand.parameters[2] == "1") ret += ", ";

                if (eventCommand.parameters[2] == "1") ret += EditorLocalize.LocalizeText("WORD_1183");

                ret += ")";
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}