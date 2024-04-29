using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    public class ActorChangeParametar : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var variables = _GetVariablesList();
            ret = indent;
            if (eventCommand.parameters[0] == "0")
            {
                var actorId = eventCommand.parameters[1];
                if (actorId == "-1")
                {
                    ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0907") + " : " +
                           EditorLocalize.LocalizeText("WORD_0892") + ", ";
                }
                else
                {
                    var characterActorDataModels =
                        DatabaseManagementService.LoadCharacterActor();
                    var index = characterActorDataModels.IndexOf(
                        characterActorDataModels.FirstOrDefault(c => c.uuId == actorId));
                    if (index < 0) index = 0;
                    ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0907") + " : " +
                           characterActorDataModels[index].basic.name + ", ";
                }
            }
            else
            {
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                var name = data?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0907") + " : #" +
                       (variables.IndexOf(data) + 1).ToString("0000") + name + " " + ", ";
            }

            var parameter = new List<string>
            {
                EditorLocalize.LocalizeText("WORD_0395"),
                EditorLocalize.LocalizeText("WORD_0539"),
                EditorLocalize.LocalizeText("WORD_0177"),
                EditorLocalize.LocalizeText("WORD_0178"),
                EditorLocalize.LocalizeText("WORD_0179"),
                EditorLocalize.LocalizeText("WORD_0180"),
                EditorLocalize.LocalizeText("WORD_0181"),
                EditorLocalize.LocalizeText("WORD_0182")
            };
            ret += parameter[int.Parse(eventCommand.parameters[2])];

            if (int.Parse(eventCommand.parameters[3]) == 0)
                ret += " + ";
            else
                ret += " - ";

            if (int.Parse(eventCommand.parameters[4]) == 0)
            {
                ret += eventCommand.parameters[5];
            }
            else
            {
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[5]);
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