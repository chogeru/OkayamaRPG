using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SystemSetting
{
    public class SystemIsSort : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            string isSave = null;
            if (eventCommand.parameters[0] == "1") isSave = EditorLocalize.LocalizeText("WORD_1076");
            if (eventCommand.parameters[0] == "0") isSave = EditorLocalize.LocalizeText("WORD_0775");
            ret = "◆" + EditorLocalize.LocalizeText("WORD_1079") + " : " + isSave;
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}