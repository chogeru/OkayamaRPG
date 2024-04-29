using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.AudioVideo
{
    public class AudioBgmFadeOut : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_0937") + " : " + eventCommand.parameters[0] + " " +
                   EditorLocalize.LocalizeText("WORD_0938");
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}