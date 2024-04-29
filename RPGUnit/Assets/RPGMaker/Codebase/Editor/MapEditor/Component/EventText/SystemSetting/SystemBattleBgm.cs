using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SystemSetting
{
    public class SystemBattleBgm : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1071") + " : " + SoundHelper.RemoveExtention(eventCommand.parameters[0]) + "(" +
                   eventCommand.parameters[1] + ", " +
                   eventCommand.parameters[2] + ", " +
                   eventCommand.parameters[3] + ")";
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}