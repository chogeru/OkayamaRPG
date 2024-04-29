using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using System.Collections.Generic;
using System.Linq;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag.FlagDataModel;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSaveDataModel;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor
{
    public class ActorChangeState
    {
        private List<Variable> _databaseVariables;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private List<RuntimeActorDataModel> _runtimeActorDataModel;
        private SaveDataVariablesData _saveDataVariablesData;
        private List<CharacterActorDataModel> _characterActorData;

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

        public void ChangeState(StateDataModel state, EventDataModel.EventCommand command) {
            var isFixedValue = command.parameters[0] == "0" ? true : false;
            var actorId = command.parameters[1];
            var isAddValue = command.parameters[2] == "0" ? true : false;
            var index = 0;

            if (isFixedValue)
            {
                if (actorId == "-1") //パーティ全体
                {
                    for (var i = 0; i < _runtimeActorDataModel.Count; i++)
                        ChangeStateProcess(isAddValue, i, state);
                }
                else //個々のキャラクター
                {
                    index = _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == actorId));
                    if (index != -1)
                    {
                        ChangeStateProcess(isAddValue, index, state);
                    }
                    else
                    {
                        //存在しないため新規作成
                        PartyChange partyChange = new PartyChange();
                        partyChange.SetActorData(actorId);
                        ChangeStateProcess(isAddValue,
                            _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == actorId)), state);
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
                                ChangeStateProcess(isAddValue, index, state);
                            }
                            else
                            {
                                //存在しないため新規作成
                                PartyChange partyChange = new PartyChange();
                                partyChange.SetActorData(actorId);
                                ChangeStateProcess(isAddValue,
                                    _runtimeActorDataModel.IndexOf(_runtimeActorDataModel.FirstOrDefault(c => c.actorId == _characterActorData[indexActor].uuId)), state);
                            }
                        }
                    }
                }
            }
        }

        private void ChangeStateProcess(bool isAddValue, int index, StateDataModel value) {
            bool flg = false;
            if (isAddValue)
            {
                //GameActorを検索する
                var actors = DataManager.Self().GetGameParty().Actors;
                for (int i = 0; i < actors.Count; i++)
                {
                    if (actors[i].ActorId == _runtimeActorDataModel[index].actorId)
                    {
                        //ステートが付与可能なタイミングかどうかのチェック
                        if (actors[i].IsStateTiming(value.id))
                        {
                            //ステート付与
                            actors[i].AddState(value.id);
                            flg = true;
                            //装備の反映
                            ResetEquipment(index, actors[i]);
                        }
                    }
                }
                if (!flg)
                {
                    //パーティに存在しない場合
                    GameActor actor = new GameActor(_runtimeActorDataModel[index]);
                    actor.AddState(value.id);
                }
            }
            else
            {
                //GameActorを検索する
                var actors = DataManager.Self().GetGameParty().Actors;
                for (int i = 0; i < actors.Count; i++)
                {
                    if (actors[i].ActorId == _runtimeActorDataModel[index].actorId)
                    {
                        //ステート解除
                        actors[i].RemoveState(value.id);
                        flg = true;
                        //装備の反映
                        ResetEquipment(index, actors[i]);
                    }
                }
                if (!flg)
                {
                    //パーティに存在しない場合
                    GameActor actor = new GameActor(_runtimeActorDataModel[index]);
                    actor.RemoveState(value.id);
                }
            }
        }

        private void ResetEquipment(int index, GameActor actor) {
            //アクターが装備するものを頭から順にチェックしなおし
            SystemSettingDataModel systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            for (var j = 0; j < _runtimeActorDataModel[index].equips.Count; j++)
            {
                //装備種別を取得
                SystemSettingDataModel.EquipType equipType = null;
                for (int j2 = 0; j2 < systemSettingDataModel.equipTypes.Count; j2++)
                    if (systemSettingDataModel.equipTypes[j2].id == _runtimeActorDataModel[index].equips[j].equipType)
                    {
                        equipType = systemSettingDataModel.equipTypes[j2];
                        break;
                    }

                //装備が封印されているかどうか
                bool ret = actor.IsEquipTypeSealed(j);
                if (ret)
                {
                    //装備を外す
                    ItemManager.RemoveEquipment(_runtimeActorDataModel[index], equipType, j);
                }
            }
            //GameActorへ反映
            //装備封印以外の要因で、装備を外すことになった場合は、以下の処理内で外れる
            actor.ResetActorData();
        }
    }
}