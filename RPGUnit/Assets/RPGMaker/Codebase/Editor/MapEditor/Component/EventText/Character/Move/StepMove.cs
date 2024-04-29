using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Runtime.Event.Character;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Move
{
    public class StepMove : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_2802") + " : ";
            var index = int.Parse(eventCommand.parameters[0]);
            var typeName = EditorLocalize.LocalizeText(CommandEditor.Character.Move.StepMove._typeNameList[index]);
            ret += typeName;

            if (index == StepMoveProcessor._jumpIndex)
            {
                ret += $" (x : {eventCommand.parameters[1]}, y : {eventCommand.parameters[2]})";
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

    }
}
