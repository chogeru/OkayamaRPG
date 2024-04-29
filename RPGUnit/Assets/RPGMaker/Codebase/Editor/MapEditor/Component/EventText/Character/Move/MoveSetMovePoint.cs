using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Move
{
    public class MoveSetMovePoint : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            var name = "";
            var text = "";
            if (eventCommand.parameters[0] == "-1")
                name = EditorLocalize.LocalizeText("WORD_0920");
            else if (eventCommand.parameters[0] == "-2")
                name = EditorLocalize.LocalizeText("WORD_0860");
            else
                name = GetEventDisplayName(eventCommand.parameters[0]);

            var moveKindTextDropdownChoices = new List<string>
            {
                EditorLocalize.LocalizeText("WORD_1001"),
                EditorLocalize.LocalizeText("WORD_0447"),
                EditorLocalize.LocalizeText("WORD_1002"),
                EditorLocalize.LocalizeText("WORD_1003")
            };
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1000") + " : " + name + "," + " " +
                   moveKindTextDropdownChoices[int.Parse(eventCommand.parameters[1])];

            text = "";

            if (eventCommand.parameters[6] == "1") text += EditorLocalize.LocalizeText("WORD_0867");
            if (eventCommand.parameters[7] == "1")
            {
                if (text != "") text += ",";
                text += EditorLocalize.LocalizeText("WORD_0989");
            }

            if (eventCommand.parameters[8] == "1")
            {
                if (text != "") text += ",";
                text += EditorLocalize.LocalizeText("WORD_0990");
            }

            if (eventCommand.parameters[9] == "1")
            {
                if (text != "") text += ",";
                text += EditorLocalize.LocalizeText("WORD_0952");
            }

            if (text != "") text = " (" + text + ")";

            LabelElement.text = ret + text;
            Element.Add(LabelElement);
            return Element;
        }
    }
}