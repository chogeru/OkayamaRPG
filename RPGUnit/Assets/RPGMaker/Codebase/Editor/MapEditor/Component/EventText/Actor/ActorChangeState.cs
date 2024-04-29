using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    public class ActorChangeState : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var variables = _GetVariablesList();
            ret = indent;
            if (eventCommand.parameters[0] == "0")
            {
                var actorId = eventCommand.parameters[1];
                if (actorId == "-1")
                {
                    ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0900") + " : " +
                           EditorLocalize.LocalizeText("WORD_0892") + ", ";
                }
                else
                {
                    var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
                    var index = characterActorDataModels.IndexOf(
                        characterActorDataModels.FirstOrDefault(c => c.uuId == actorId));
                    if (index < 0) index = 0;
                    ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0900") + " : " +
                           characterActorDataModels[index].basic.name;
                }
            }
            else
            {
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                var name = data?.name;
                if (name == "")
                {
                    name = EditorLocalize.LocalizeText("WORD_1518");
                }

                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0900") + " : #" +
                       (variables.IndexOf(data) + 1).ToString("0000") + " " + name + ", ";
            }

            var stateDataModels = DatabaseManagementService.LoadStateEdit();
            if (int.Parse(eventCommand.parameters[2]) == 0)
            {
                ret += " + ";
            }
            else
            {
                ret += " - ";
            }

            int parse;
            if (int.TryParse(eventCommand.parameters[3], out parse))
                ret += stateDataModels[int.Parse(eventCommand.parameters[3])].name;
            else if (eventCommand.parameters[3] != "")
            {
                bool isNone = true;

                for (int i = 0; i < stateDataModels.Count; i++)
                    if (stateDataModels[i].id == eventCommand.parameters[3])
                    {
                        ret += stateDataModels[i].name;
                        isNone = false;
                        break;
                    }

                //設定しているデータがなかった場合、「なし」を表示させる
                if (isNone)
                {
                    ret += EditorLocalize.LocalizeText("WORD_0113");
                }
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