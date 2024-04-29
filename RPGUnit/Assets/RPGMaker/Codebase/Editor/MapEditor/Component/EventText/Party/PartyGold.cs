using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Party
{
    public class PartyGold : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            if (int.Parse(eventCommand.parameters[0]) == 0)
                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1099") + " : + ";
            else
                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1099") + " : - ";

            if (int.Parse(eventCommand.parameters[1]) == 0)
            {
                ret += eventCommand.parameters[2];
            }
            else
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[2]);
                if (data == null) data = variables[0];
                var name = data?.name;
                if (name == "")
                {
                    name = EditorLocalize.LocalizeText("WORD_1518");
                }

                ret += "#" + (variables.IndexOf(data) + 1).ToString("0000") + name;
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                fileNames.Add(flagDataModel.variables[i]);
            }

            return fileNames;
        }
    }
}