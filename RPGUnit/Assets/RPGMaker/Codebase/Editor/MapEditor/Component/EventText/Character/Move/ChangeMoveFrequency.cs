using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Move
{
    public class ChangeMoveFrequency : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1548") + " : ";
            var index = int.Parse(eventCommand.parameters[0]);
            var frequencyName = EditorLocalize.LocalizeText(CommandEditor.Character.Move.ChangeMoveFrequency._frequencyNameList[index]);
            ret += frequencyName;

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

    }
}
