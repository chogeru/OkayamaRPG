using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[アクターの画像変更]
    /// </summary>
    public class SystemChangeActorImageProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventId, EventDataModel.EventCommand command) {
            var runtimeActorDataModels = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;

            //IDの一致したActorを探す
            bool flg = false;
            for (var i = 0; i < runtimeActorDataModels.Count; i++)
                if (runtimeActorDataModels[i].actorId == command.parameters[0])
                {
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels[i].faceImage =
                        command.parameters[1];
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels[i].characterImage =
                        command.parameters[2];
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels[i].battlerImage =
                        command.parameters[3];
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels[i].advImage =
                        command.parameters[4];

                    //バトルはUpdateで切り替わる
                    if (GameStateHandler.IsMap())
                    {
                        //MAP上のキャラクターの再描画
                        var actorObj = MapManager.OperatingActor;
                        if (actorObj.CharacterId == command.parameters[0])
                        {
                            //先頭キャラクター
                            actorObj.GetComponent<CharacterOnMap>().ChangeAsset(command.parameters[2]);
                        }
                        else
                        {
                            //パーティメンバーの場合
                            var party = MapManager.PartyOnMap;
                            for (int j = 0; j < party.Count; j++)
                            {
                                if (party[j].CharacterId == command.parameters[0])
                                {
                                    party[j].GetComponent<CharacterOnMap>().ChangeAsset(command.parameters[2]);
                                }
                            }
                        }
                    }
                    flg = true;
                    break;
                }

            if (!flg)
            {
                //存在しないため新規作成
                PartyChange partyChange = new PartyChange();
                RuntimeActorDataModel actor = partyChange.SetActorData(command.parameters[0]);

                actor.faceImage = command.parameters[1];
                actor.characterImage = command.parameters[2];
                actor.battlerImage = command.parameters[3];
                actor.advImage = command.parameters[4];
            }

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}