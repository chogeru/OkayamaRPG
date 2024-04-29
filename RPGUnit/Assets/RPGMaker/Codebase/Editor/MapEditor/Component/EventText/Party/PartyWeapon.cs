using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Party
{
    public class PartyWeapon : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var weaponDataModels = DatabaseManagementService.LoadWeapon();
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1101") + " : " +
                                               weaponDataModels.FirstOrDefault(c => c.basic.id == eventCommand.parameters[0])?.basic.name;

            if (int.Parse(eventCommand.parameters[1]) == 0)
                ret += " + ";
            else
                ret += " - ";

            if (int.Parse(eventCommand.parameters[2]) == 0)
            {
                ret += eventCommand.parameters[3];
            }
            else
            {
                var variable = _GetVariablesList();
                var variableData = variable.FirstOrDefault(c => c.id == eventCommand.parameters[3]);
                var name = variableData?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variable.IndexOf(variableData) + 1).ToString("0000") + name;
            }

            if (int.Parse(eventCommand.parameters[4]) == 1)
                ret += "(" + EditorLocalize.LocalizeText("WORD_1152") + ")";

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