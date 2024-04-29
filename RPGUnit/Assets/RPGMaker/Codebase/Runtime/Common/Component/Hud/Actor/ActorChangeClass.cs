using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor
{
    public class ActorChangeClass
    {
        public void ChangeClass(CharacterActorDataModel actorData, EventDataModel.EventCommand command) {
            var classId = command.parameters[1];
            var save = command.parameters[2] == "1" ? true : false;

            var party = DataManager.Self().GetGameParty();
            var index = party.Actors.IndexOf(party.Actors.FirstOrDefault(c => c.ActorId == actorData.uuId));
            if(index >= 0)
            {
                party.Actors[index].ChangeClass(classId, save);
            }
            else
            {
                //パーティに存在しない場合
                //RuntimeActorDataModel取得
                var actor = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.FirstOrDefault(c => c.actorId == actorData.uuId);
                if (actor == null)
                {
                    //存在しないため新規作成
                    PartyChange partyChange = new PartyChange();
                    actor = partyChange.SetActorData(actorData.uuId);
                }

                //GameActor生成
                GameActor gameActor = new GameActor(actor);

                //職業変更
                gameActor.ChangeClass(classId, save);
            }
        }
    }
}