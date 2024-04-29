using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Map
{
    public class MapChangeName : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1175") + " : ";
            if (eventCommand.parameters[0] == "1")
            {
                ret += "ON";
            }
            else
            {
                ret += "OFF";
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}