using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.GameProgress
{
    public class GameTimer : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1049") + " : ";
            if (int.Parse(eventCommand.parameters[0]) == 0)
            {
                ret += EditorLocalize.LocalizeText("WORD_0933");
            }
            else
            {
                ret += EditorLocalize.LocalizeText("WORD_1050") + ", ";
                ret +=
                    int.Parse(eventCommand.parameters[1]) / 60 + EditorLocalize.LocalizeText("WORD_2602") + " " +
                    (int.Parse(eventCommand.parameters[1]) % 60) + EditorLocalize.LocalizeText("WORD_0938");
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}