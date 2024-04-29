using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    public class BattleExecCommand : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1111");

            if (eventCommand.parameters[0] == "0")
            {
                var memberIndex = 0;
                if (int.TryParse(eventCommand.parameters[1], out memberIndex))
                    memberIndex -= 1; // 1から始まる番号で格納されているのでインデックス用に調整

                if (GetEnemyNameList().Count <= memberIndex)
                {
                    memberIndex = 0;
                }

                string data = "";
                if (GetEnemyNameList().Count > memberIndex)
                    data = GetEnemyNameList()[memberIndex];

                ret += $" : #{memberIndex + 1} {data}, ";
            }
            else
            {
                var actorId = eventCommand.parameters[1];
                var actorData = _GetCharacterList();
                var data = actorData.FirstOrDefault(c => c.uuId == actorId);
                var actorName = data != null ? data.basic.name : string.Empty;
                ret += $" : {actorName}, ";
            }

            var skillID = eventCommand.parameters[2];
            var skillDataList = _GetSkillList();
            var skillData = skillDataList.FirstOrDefault(c => c.basic.id == skillID);
            var skillName = skillData != null ? skillData.basic.name : string.Empty;
            ret += $"{skillName}, ";

            var target = eventCommand.parameters[3] switch
            {
                "0" => EditorLocalize.LocalizeText("WORD_1113"),
                "1" => EditorLocalize.LocalizeText("WORD_0447"),
                _ => eventCommand.parameters[3]
            };
            ret += target;

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        //キャラクターListの取得
        private List<CharacterActorDataModel> _GetCharacterList() {
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            var fileNames = new List<CharacterActorDataModel>();
            for (var i = 0; i < characterActorDataModels.Count; i++) fileNames.Add(characterActorDataModels[i]);

            return fileNames;
        }

        //スキルListの取得
        private List<SkillCustomDataModel> _GetSkillList() {
            var skillCustomDataModels = DatabaseManagementService.LoadSkillCustom();
            var fileNames = new List<SkillCustomDataModel>();
            for (var i = 0; i < skillCustomDataModels.Count; i++) fileNames.Add(skillCustomDataModels[i]);

            return fileNames;
        }
    }
}