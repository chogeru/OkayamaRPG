using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    public class MessageInputSelectItem : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var itemType = "";
            if (eventCommand.parameters[1] == "0")
                itemType = EditorLocalize.LocalizeText("WORD_1212");
            else if (eventCommand.parameters[1] == "1")
                itemType = EditorLocalize.LocalizeText("WORD_0130");
            else if (eventCommand.parameters[1] == "2")
                itemType = EditorLocalize.LocalizeText("WORD_0549");
            else if (eventCommand.parameters[1] == "3") itemType = EditorLocalize.LocalizeText("WORD_0550");
            var variables = _GetVariablesList();

            var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[0]);
            var name = data.name;
            if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1211") + " : " +
                                               EditorLocalize.LocalizeText("WORD_0839") + " #" + (variables.IndexOf(data) + 1).ToString("0000") +
                                               name + ", " + EditorLocalize.LocalizeText("WORD_0547") + " : " + itemType;

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