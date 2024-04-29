using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    public class ActorChangeSkill : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var variables = _GetVariablesList();
            var skills = _GetSkillList();
            ret = indent;
            if (eventCommand.parameters[0] == "0")
            {
                var actorId = eventCommand.parameters[1];
                if (actorId == "-1")
                {
                    ret += "◆" + EditorLocalize.LocalizeText("WORD_0908") + " : " +
                           EditorLocalize.LocalizeText("WORD_0892") + ", ";
                }
                else
                {
                    var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
                    var index = characterActorDataModels.IndexOf(
                        characterActorDataModels.FirstOrDefault(c => c.uuId == actorId));
                    ret += "◆" + EditorLocalize.LocalizeText("WORD_0908") + " : " +
                           characterActorDataModels[index].basic.name;
                }
            }
            else
            {
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                var name = data.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "◆" + EditorLocalize.LocalizeText("WORD_0908") + " : #" +
                       (variables.IndexOf(data) + 1).ToString("0000") + " " + name + ", ";
            }

            if (int.Parse(eventCommand.parameters[2]) == 0)
                ret += " + ";
            else
                ret += " - ";

            var skill = skills.FirstOrDefault(c => c.basic.id == eventCommand.parameters[3]);
            var skillName = "";
            if (skill != null && skill.basic.name != null) skillName = skill.basic.name;
            else skillName = skills[0].basic.name;
            ret += skillName;

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

        private List<SkillCustomDataModel> _GetSkillList() {
            var skillCustomDataModels = DatabaseManagementService.LoadSkillCustom();
            var fileNames = new List<SkillCustomDataModel>();
            for (var i = 0; i < skillCustomDataModels.Count; i++) fileNames.Add(skillCustomDataModels[i]);

            return fileNames;
        }
    }
}