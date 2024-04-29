using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag.FlagDataModel;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSaveDataModel;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor
{
    public class ActorChangeHp
    {
        private List<Variable> _databaseVariables;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private List<RuntimeActorDataModel> _runtimeActorDataModels;
        private SaveDataVariablesData       _saveDataVariablesData;
        private List<CharacterActorDataModel> _characterActorData;

        /**
         * 初期化
         */
        public void Init(RuntimeSaveDataModel saveDataModel) {
            _runtimeActorDataModels = saveDataModel.runtimeActorDataModels;
            _saveDataVariablesData = saveDataModel.variables;

            var databaseManagementService = new DatabaseManagementService();
            _databaseVariables = databaseManagementService.LoadFlags().variables;
            _characterActorData = DataManager.Self().GetActorDataModels();
        }

        public void ChangeHP(EventDataModel.EventCommand command) {
            var isFixedValue = command.parameters[0] == "0";
            var isAddValue = command.parameters[2] == "0";
            var isConstant = command.parameters[3] == "0";
            var isKnockOut = command.parameters[5] == "1";

            var value = 0;
            if (isConstant)
            {
                if (!int.TryParse(command.parameters[4], out value))
                    return;
            }
            else
            {
                var index = _databaseVariables.FindIndex(v => v.id == command.parameters[4]);
                if (index == -1) return;

                if (!int.TryParse(_saveDataVariablesData.data[index], out value))
                    return;
            }

            value = isAddValue ? value : -value;

            if (isFixedValue)
            {
                var actorId = command.parameters[1];
                if (actorId == "-1") //パーティ全体
                {
                    // パーティ全体
                    foreach (var actorDataModel in _runtimeActorDataModels)
                        ChangeHpProcess(isKnockOut, actorDataModel, value);
                    return;
                }

                // 個々のキャラクター
                var actor = _runtimeActorDataModels.FirstOrDefault(c => c.actorId == actorId);
                if(actor != null)
                {
                    ChangeHpProcess(isKnockOut, actor, value);
                }
                else
                {
                    //存在しないため新規作成
                    PartyChange partyChange = new PartyChange();
                    actor = partyChange.SetActorData(actorId);
                    ChangeHpProcess(isKnockOut, actor, value);
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
                            int index = _runtimeActorDataModels.IndexOf(_runtimeActorDataModels.FirstOrDefault(c => c.actorId == _characterActorData[indexActor].uuId));
                            if (index >= 0)
                            {
                                ChangeHpProcess(isKnockOut, _runtimeActorDataModels[index], value);
                            }
                            else
                            {
                                //存在しないため新規作成
                                PartyChange partyChange = new PartyChange();
                                RuntimeActorDataModel actor = partyChange.SetActorData(_characterActorData[indexActor].uuId);
                                ChangeHpProcess(isKnockOut, actor, value);
                            }
                        }
                    }
                }
            }
        }

        private void ChangeHpProcess(bool knockout, RuntimeActorDataModel targetDataModel, int value) {
            if (targetDataModel == null) return;
            var actorsWork = DataManager.Self().GetGameParty().Actors;
            var actor = actorsWork.FirstOrDefault(c => c.ActorId == targetDataModel?.actorId);
            if (actor == null)
            {
                //パーティに存在しない場合
                //RuntimeActorDataModel取得
                var runtimeActorData = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.FirstOrDefault(c => c.actorId == targetDataModel?.actorId);

                //GameActor生成
                actor = new GameActor(runtimeActorData);
            }

            var selectClass = DataManager.Self().GetClassDataModels()
                .FirstOrDefault(c => c.id == targetDataModel.classId);
            var maxHP = targetDataModel.GetCurrentMaxHp(selectClass);
            var minHP = knockout ? 0 : 1;
            targetDataModel.hp = Math.Max(minHP, Math.Min(maxHP, targetDataModel.hp + value));

            if (knockout && targetDataModel.hp == 0)
            {
                // HP=0の場合は戦闘不能のステートを付与 戦闘不能は固定で0
                // ただし、Uniteではステートを付与できるタイミングに制限を設けられるため、ステート付与可能かどうかをチェックする
                bool isKnockout = false;
                var stateDataModels = DataManager.Self().GetStateDataModels();

                var actors = DataManager.Self().GetGameParty().Actors;
                for (int i = 0; i < actors.Count; i++)
                {
                    if (actors[i].ActorId == targetDataModel.actorId)
                    {
                        //ステートが付与可能なタイミングかどうかのチェック
                        if (actors[i].IsStateTiming(stateDataModels[0].id))
                        {
                            //ステート付与
                            actors[i].AddState(stateDataModels[0].id);
                            isKnockout = true;
                        }
                    }
                }

                if (!isKnockout)
                {
                    //戦闘不能ステートを付与できなかったため、HPを1に戻す
                    targetDataModel.hp = 1;
                }
            }
        }
    }
}