using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Map
{
    public class MoveMapScroll : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1173") + " : ";
            var directions = new List<string> {"WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"};
            var speeds = new List<string>
                {"WORD_0847", "WORD_0848", "WORD_0849", "WORD_0985", "WORD_0851", "WORD_0852"};
            ret += EditorLocalize.LocalizeText(directions[int.Parse(eventCommand.parameters[0])]) + ",";
            ret += int.Parse(eventCommand.parameters[1]) + ",";
            ret += EditorLocalize.LocalizeText(speeds[int.Parse(eventCommand.parameters[2])]);
            if (eventCommand.parameters[3] == "1")
                ret += ",(" + EditorLocalize.LocalizeText("WORD_1087") + ")";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}