using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    public class MessageInputNumber : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            var variables = _GetVariablesList();
            var variablesDropdownChoices = new List<string>();
            var variablesNameDropdownChoices = new List<string>();

            for (var j = 0; j < variables.Count; j++)
            {
                variablesDropdownChoices.Add(variables[j].id);
                if (variables[j].name != null)
                {
                    variablesNameDropdownChoices.Add(variables[j].name);
                }
                else
                {
                    variablesNameDropdownChoices.Add("");
                }
            }

            var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[0]);
            if (data == null) data = variables[0];

            var name = data.name;
            if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1209") + " : " + EditorLocalize.LocalizeText("WORD_0839") +
                                               " #" + (variables.IndexOf(data) + 1).ToString("0000") + name + ", ";

            ret += "/" + EditorLocalize.LocalizeText("WORD_1210") + " : " + eventCommand.parameters[1];
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