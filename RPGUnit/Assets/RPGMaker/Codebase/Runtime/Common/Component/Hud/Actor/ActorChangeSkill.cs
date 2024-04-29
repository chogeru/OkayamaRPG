using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using System.Collections.Generic;
using System.Linq;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag.FlagDataModel;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSaveDataModel;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor
{
    public class ActorChangeSkill
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private List<CharacterActorDataModel> _characterActorData;
        private List<RuntimeActorDataModel> _runtimeActorDataModel;
        private SaveDataVariablesData _saveDataVariablesData;
        private List<Variable> _databaseVariables;

        /**
         * 初期化
         */
        public void Init(
            List<RuntimeActorDataModel> runtimeActorData,
            SaveDataVariablesData saveDataVariablesData,
            List<CharacterActorDataModel> characterActorData
        ) {
            _runtimeActorDataModel = runtimeActorData;
            _saveDataVariablesData = saveDataVariablesData;
            _characterActorData = characterActorData;

            var databaseManagementService = new DatabaseManagementService();
            _databaseVariables = databaseManagementService.LoadFlags().variables;
        }

        public void ChangeSkill(SkillCustomDataModel skillData, EventDataModel.EventCommand command) {
            var isFixedValue = command.parameters[0] == "0" ? true : false;
            var actorId = command.parameters[1];
            var isAddValue = command.parameters[2] == "0" ? true : false;

            var index = 0;

            if (isFixedValue)
            {
                if (actorId == "-1") //パーティ全体
                {
                    for (var i = 0; i < _runtimeActorDataModel.Count; i++)
                        ChangeSkillProcess(isAddValue, skillData, i);
                }
                else //個々のキャラクター
                {
                    index = _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == actorId));
                    if (index >= 0)
                    {
                        ChangeSkillProcess(isAddValue, skillData, index);
                    }
                    else
                    {
                        //存在しないため新規作成
                        PartyChange partyChange = new PartyChange();
                        partyChange.SetActorData(actorId);
                        ChangeSkillProcess(isAddValue, skillData,
                            _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == actorId)));
                    }
                }
            }
            else
            {
                //MVの挙動から
                //変数内の数値によって経験値を変動させるのは、該当のIDのユーザー（=SerialNoが一致するアクター）
                int variableIndex = _databaseVariables.FindIndex(v => v.id == command.parameters[1]);
                if (variableIndex >= 0)
                {
                    int actorSerialNo = int.Parse(_saveDataVariablesData.data[variableIndex]);
                    if (actorSerialNo >= 0)
                    {
                        int indexActor = _characterActorData.IndexOf(_characterActorData.FirstOrDefault(c => c.SerialNumber == actorSerialNo));
                        if (indexActor >= 0)
                        {
                            index = _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == _characterActorData[indexActor].uuId));
                            if (index >= 0)
                            {
                                ChangeSkillProcess(isAddValue, skillData, index);
                            }
                            else
                            {
                                //存在しないため新規作成
                                PartyChange partyChange = new PartyChange();
                                partyChange.SetActorData(actorId);
                                ChangeSkillProcess(isAddValue, skillData,
                                    _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == _characterActorData[indexActor].uuId)));
                            }
                        }
                    }
                }
            }
        }

        private void ChangeSkillProcess(bool isAddValue, SkillCustomDataModel skillData, int index) {
            if (isAddValue)
            {
                if (!_runtimeActorDataModel[index].skills.Contains(skillData.basic.id))
                    _runtimeActorDataModel[index].skills.Add(skillData.basic.id);
            }
            else
            {
                var removeIndex = _runtimeActorDataModel[index].skills.IndexOf(_runtimeActorDataModel[index].skills
                    .FirstOrDefault(c => c == skillData.basic.id));
                if (removeIndex != -1) _runtimeActorDataModel[index].skills.RemoveAt(removeIndex);
            }
        }
    }
}