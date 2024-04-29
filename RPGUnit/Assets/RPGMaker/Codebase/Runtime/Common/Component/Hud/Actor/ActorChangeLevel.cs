using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag.FlagDataModel;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSaveDataModel;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor
{
    public class ActorChangeLevel
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private List<Variable> _databaseVariables;
        private SaveDataVariablesData _saveDataVariablesData;
        private List<CharacterActorDataModel> _characterActorData;
        private List<string> _messages;
        private Action _callback;

        /**
         * 初期化
         */
        public void Init(
            SaveDataVariablesData saveDataVariablesData,
            List<CharacterActorDataModel> characterActorData
        ) {
            _saveDataVariablesData = saveDataVariablesData;
            _characterActorData = characterActorData;
            _databaseVariables = new DatabaseManagementService().LoadFlags().variables;
        }

        public void ChangeLevel(EventDataModel.EventCommand command, Action callback) {
            var isActorFixed = command.parameters[0] == "0";    // アクター(0:固定 1:変数)
            var actorParam = command.parameters[1];             // アクターID(-1:全員)
            var isAddValue = command.parameters[2] == "0";      // 操作(0:増やす 1:減らす)
            var isConstant = command.parameters[3] == "0";      // オペランドタイプ(0:定数 1:変数)
            var levelParam = command.parameters[4];             // オペランド(設定値 or 変数番号) 実際は変数番号ではなく変数uuid
            var isLevelUpEvent = command.parameters[5] != "0";  // レベルアップを表示(true or false)				

            _callback = callback;

            int levelValue = GetLevelValue();

            if (actorParam == "-1")
            {
                if (isLevelUpEvent)
                {
                    HudDistributor.Instance.NowHudHandler().OpenMessageWindow();
                }

                ChangeLevelAllProcess(isAddValue ? levelValue : -levelValue, isLevelUpEvent);

                if (isLevelUpEvent)
                {
                    TimeHandler.Instance.AddTimeActionFrame(1, ShowLevelupWindow, false);
                    return;
                }
            }
            else
            {
                var actorId = GetActorId();
                if (actorId != null)
                {
                    if (isLevelUpEvent)
                    {
                        HudDistributor.Instance.NowHudHandler().OpenMessageWindow();
                    }

                    ChangeLevelProcess(isAddValue ? levelValue : -levelValue, isLevelUpEvent, actorId);

                    if (isLevelUpEvent)
                    {
                        TimeHandler.Instance.AddTimeActionFrame(1, ShowLevelupWindow, false);
                        return;
                    }
                }
            }

            //ここまで到達した場合にはCBを即実行
            _callback();

            int GetLevelValue()
            {
                if (isConstant)
                {
                    // 定数
                    return int.Parse(levelParam);
                }
                else
                {
                    // 変数
                    var variableValue = GetSaveDataVariableValue(levelParam);
                    return variableValue != null ? int.Parse(variableValue) : 0/*変数が存在しなければ増減0とする*/;
                }
            }

            string GetActorId()
            {
                if (isActorFixed)
                {
                    // 固定
                    var runtimePartyMember = DataManager.Self().GetGameParty().Actors.FirstOrDefault(c => c.ActorId == actorParam);
                    if (runtimePartyMember == null)
                    {
                        //存在しないため新規作成
                        PartyChange partyChange = new PartyChange();
                        partyChange.SetActorData(actorParam);
                    }

                    return actorParam;
                }
                else
                {
                    // 変数
                    var variableValue = GetSaveDataVariableValue(actorParam);
                    if (variableValue == null)
                    {
                        return null;
                    }

                    int actorSerialNo = int.Parse(variableValue);
                    var uuId = _characterActorData.FirstOrDefault(c => c.SerialNumber == actorSerialNo)?.uuId;
                    var runtimePartyMember = DataManager.Self().GetGameParty().Actors.FirstOrDefault(c => c.ActorId == uuId);
                    if (runtimePartyMember == null)
                    {
                        //存在しないため新規作成
                        PartyChange partyChange = new PartyChange();
                        RuntimeActorDataModel actor = partyChange.SetActorData(uuId);

                        //GameActor生成
                        runtimePartyMember = new GameActor(actor);
                    }
                    return runtimePartyMember?.ActorId;
                }
            }

            string GetSaveDataVariableValue(string variableId)
            {
                int variableIndex = _databaseVariables.FindIndex(v => v.id == variableId);
                return
                    variableIndex >= 0 || variableIndex < _saveDataVariablesData.data.Count ?
                        _saveDataVariablesData.data[variableIndex] : null;
            }
        }

        private void ShowLevelupWindow() {
            //次に表示するメッセージ
            var text = _messages[0];
            _messages.RemoveAt(0);

            //表示処理
            HudDistributor.Instance.NowHudHandler().OpenMessageWindow();
            HudDistributor.Instance.NowHudHandler().SetShowMessage(text);

            //InputHandlerへの登録
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Decide, DecideEvent);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.LeftClick, DecideEvent);
        }

        private void DecideEvent() {
            if (HudDistributor.Instance.NowHudHandler().IsInputWait())
            {
                HudDistributor.Instance.NowHudHandler().Next();
                return;
            }
            if (!HudDistributor.Instance.NowHudHandler().IsInputEnd())
            {
                return;
            }

            //InputHandler削除
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Decide, DecideEvent);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.LeftClick, DecideEvent);

            if (_messages.Count > 0)
            {
                TimeHandler.Instance.AddTimeActionFrame(1, ShowLevelupWindow, false);
            }
            else
            {
                HudDistributor.Instance.NowHudHandler().CloseMessageWindow();
                _callback();
            }
        }

        private void ChangeLevelAllProcess(int level, bool show) {
            _messages = new List<string>();

            DataManager.Self().GetGameParty().AllMembers().ForEach(actor => {
                actor.LevelupMessage = new List<string>();
                actor.ChangeLevel(level, false);
                var text = actor.LevelUpText();
                for (int i = 0; i < actor.LevelupMessage.Count; i++)
                {
                    if ((i + 1) % 4 == 0)
                    {
                        _messages.Add(text);
                        text = actor.LevelupMessage[i];
                    }
                    else
                    {
                        text += "\\!\n" + actor.LevelupMessage[i];
                    }
                }
                _messages.Add(text);
            });
        }

        private void ChangeLevelProcess(int level, bool show, string actorId) {
            var index = DataManager.Self().GetGameParty().Actors
                .IndexOf(DataManager.Self().GetGameParty().Actors.FirstOrDefault(c => c.ActorId == actorId));

            GameActor gameActor;
            if (index != -1)
            {
                gameActor = DataManager.Self().GetGameParty().Actors[index];
            }
            else
            {
                //パーティに存在しない場合
                //RuntimeActorDataModel取得
                var actor = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.FirstOrDefault(c => c.actorId == actorId);

                //GameActor生成
                gameActor = new GameActor(actor);
            }

            _messages = new List<string>();
            gameActor.ChangeLevel(level, false);
            gameActor.LevelupMessage = new List<string>();
            var text = gameActor.LevelUpText();
            for (int i = 0; i < gameActor.LevelupMessage.Count; i++)
            {
                if ((i + 1) % 4 == 0)
                {
                    _messages.Add(text);
                    text += gameActor.LevelupMessage[i];
                }
                else
                {
                    text += "\\!\n" + gameActor.LevelupMessage[i];
                }
            }
            _messages.Add(text);
        }
    }
}