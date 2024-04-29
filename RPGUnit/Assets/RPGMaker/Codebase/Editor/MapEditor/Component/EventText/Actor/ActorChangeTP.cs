using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    public class ActorChangeTP : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            if (eventCommand.parameters[0] == "0")
            {
                var actorId = eventCommand.parameters[1];
                if (actorId == "-1")
                {
                    ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0899") + " : " +
                           EditorLocalize.LocalizeText("WORD_0892") + ", ";
                }
                else
                {
                    var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
                    var index = characterActorDataModels.IndexOf(
                        characterActorDataModels.FirstOrDefault(c => c.uuId == actorId));
                    if (index == -1) index = 0;
                    ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0899") + " : " +
                           characterActorDataModels[index].basic.name;
                }
            }
            else
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                var name = data?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0899") + " : #" +
                       (variables.IndexOf(data) + 1).ToString("0000") + " " + name + ", ";
            }

            if (eventCommand.parameters[2] == "" || int.Parse(eventCommand.parameters[2]) == 0)
                ret += " + ";
            else
                ret += " - ";

            if (eventCommand.parameters[3] == "" || int.Parse(eventCommand.parameters[3]) == 0)
            {
                ret += eventCommand.parameters[4];
            }
            else
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[4]);
                var name = data?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variables.IndexOf(data) + 1).ToString("0000") + " " + name;
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