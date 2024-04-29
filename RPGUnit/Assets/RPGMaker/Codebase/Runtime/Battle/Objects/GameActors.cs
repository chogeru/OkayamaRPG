using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// Game_Actor をまとめて扱えるようにしたクラス。ほぼ、$dataActorsと同じ
    /// </summary>
    public class GameActors
    {
        /// <summary>
        /// アクターの配列
        /// </summary>
        private readonly Dictionary<string, GameActor> _data = new Dictionary<string, GameActor>();

        /// <summary>
        /// 指定IDのアクターを返す
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public GameActor Actor(RuntimeActorDataModel actor) {
            if (DataManager.Self().GetActorDataModel(actor.actorId) == null) return null;

            //既に存在するGameActorから探す
            var actors = DataManager.Self().GetGameParty().Members();
            for (var i = 0; i < actors.Count; i++)
                if (actor.actorId == actors[i].Id)
                    return (GameActor) actors[i];

            //万が一ここに到達したら、データが正常に読み込めていないので、読み込みなおす
            DataManager.Self().ReloadGameParty();
            actors = DataManager.Self().GetGameParty().Members();
            for (var i = 0; i < actors.Count; i++)
                if (actor.actorId == actors[i].Id)
                    return (GameActor) actors[i];

            //これでもNGであれば、nullを返却
            return null;
        }
    }
}