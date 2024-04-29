using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Map
{
    public class MapGetPoint : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var variables = _GetVariablesList();
            var events = EventManagementService.LoadEvent();
            ret = indent;
            var mapSelectName = new List<string> { "WORD_0822", "WORD_1186", "WORD_1187", "WORD_1188"};
            var layerName = new List<string> {"WORD_1475", "WORD_1476", "WORD_1477", "WORD_1478"};
            var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[0]);
            var name = data?.name;
            if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1184") + " : #" +
                   (variables.IndexOf(data) + 1).ToString("0000") + name + ", ";

            if (eventCommand.parameters[1] == "2" || eventCommand.parameters[1] == "0")
                ret += EditorLocalize.LocalizeText(mapSelectName[int.Parse(eventCommand.parameters[1])]) + "," +
                       EditorLocalize.LocalizeText(layerName[int.Parse(eventCommand.parameters[2])]);
            else 
                ret += EditorLocalize.LocalizeText(mapSelectName[
                    int.Parse(eventCommand.parameters[1])
                ]);

            if (eventCommand.parameters[3] == "0")
            {
                ret += "(" +
                       int.Parse(eventCommand.parameters[4]) + "," +
                       -int.Parse(eventCommand.parameters[5]) + ")";
            }
            else if (eventCommand.parameters[3] == "1")
            {
                var variable1 = variables.FirstOrDefault(c => c.id == eventCommand.parameters[4]);
                var variable2 = variables.FirstOrDefault(c => c.id == eventCommand.parameters[5]);
                var name1 = variable1?.name;
                var name2 = variable2?.name;
                if (name1 == "") name1 = EditorLocalize.LocalizeText("WORD_1518");
                if (name2 == "") name2 = EditorLocalize.LocalizeText("WORD_1518");
                ret += "(#" + (variables.IndexOf(variable1) + 1).ToString("0000") + name1 + ",";
                ret += "#" + (variables.IndexOf(variable2) + 1).ToString("0000") + name2 + ")";
            }
            else if (eventCommand.parameters[3] == "2")
            {
                var index = eventCommand.parameters.Count >= 7 ? 6 : 4;
                ret += ",(" + GetTargetCharacterName(eventCommand.parameters[index]) + ")";
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }
    }
}