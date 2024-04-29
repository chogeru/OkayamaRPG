using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SceneControl
{
    public class SceneSetBattleConfig : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var troopDataModels = DatabaseManagementService.LoadTroop();
            var troopNameList = new List<string>();
            for (var i = 0; i < troopDataModels.Count; i++)
                troopNameList.Add(troopDataModels[i].name);

            var data = troopDataModels.FirstOrDefault(c => c.id == eventCommand.parameters[1]);

            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1052") + " : ";
            ret += EditorLocalize.LocalizeText("WORD_0564") + " ";
            if (eventCommand.parameters[0] == "0")
            {
                ret += " " + (data == null ? "" : data.name);
            }
            else if (eventCommand.parameters[0] == "1")
            {
                var variable = _GetVariablesList();
                var variableData = variable.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                var name = variableData?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variable.IndexOf(variableData) + 1).ToString("0000") + name;
            }
            else if (eventCommand.parameters[0] == "2")
            {
                ret += " " + EditorLocalize.LocalizeText("WORD_1053");
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