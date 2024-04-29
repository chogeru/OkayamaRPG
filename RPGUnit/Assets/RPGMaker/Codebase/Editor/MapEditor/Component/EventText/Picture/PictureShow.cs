using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Picture
{
    public class PictureShow : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1116") + " : #" + eventCommand.parameters[0] + ", ";
            ret += eventCommand.parameters[1] + ", ";
            var origin = new List<string> {"WORD_0294", "WORD_0290"};
            ret += EditorLocalize.LocalizeText(origin[int.Parse(eventCommand.parameters[2])]) + " ";
            if (eventCommand.parameters[3] == "0")
            {
                ret += "(" + eventCommand.parameters[4] + "," +
                       eventCommand.parameters[5] + "), ";
            }
            else
            {
                var variableList = _GetVariablesList();

                var data1 = variableList.FirstOrDefault(c => c.id == eventCommand.parameters[4]);
                var data2 = variableList.FirstOrDefault(c => c.id == eventCommand.parameters[5]);
                var name1 = data1?.name;
                var name2 = data2?.name;
                if (name1 == "") name1 = EditorLocalize.LocalizeText("WORD_1518");
                if (name2 == "") name2 = EditorLocalize.LocalizeText("WORD_1518");

                ret += "(#" + (variableList.IndexOf(data1) + 1).ToString("0000") + name1 + ", ";
                ret += "#" + (variableList.IndexOf(data2) + 1).ToString("0000") + name2 + "), ";
            }

            ret += "(" + eventCommand.parameters[6] + "%," +
                   eventCommand.parameters[7] + "%), ";
            ret += eventCommand.parameters[8] + ", ";
            var blendmode = new List<string> {"WORD_0548", "WORD_0976", "WORD_0977", "WORD_0978"};
            ret += EditorLocalize.LocalizeText(blendmode[int.Parse(eventCommand.parameters[9])]) + " ";

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