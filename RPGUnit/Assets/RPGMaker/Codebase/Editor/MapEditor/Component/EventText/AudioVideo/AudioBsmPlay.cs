using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.AudioVideo
{
    public class AudioBsmPlay : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_0941") + " : " + eventCommand.parameters[0];
            ret += "(" + eventCommand.parameters[1] + " ," +
                   eventCommand.parameters[2] + " ," +
                   eventCommand.parameters[3] + ")";
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}