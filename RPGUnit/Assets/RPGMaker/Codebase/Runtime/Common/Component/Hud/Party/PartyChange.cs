using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Map;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Party
{
    public class PartyChange
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private string _actorId;
        private bool   _isAdd;
        private bool   _isInit;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="isAdd"></param>
        /// <param name="isInit"></param>
        public void Init(string actorId, bool isAdd, bool isInit) {
            _actorId = actorId;
            _isAdd = isAdd;
            _isInit = isInit;
        }

        /// <summary>
        /// メンバー追加、削除
        /// </summary>
        public void SetPartyChange() {
            var haveCharacter = DataManager.Self().GetRuntimeSaveDataModel().ActorInParty(_actorId);
            if (_isAdd)
            {
                if (!haveCharacter)
                {
                    //現在のパーティメンバーを取得
                    var workList = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors.ToList();
                    if (workList.Count == 4) return;

                    //現在のセーブデータを参照し、既に追加するアクターのデータが存在していた場合には、それを取得する
                    var runtimeSave = DataManager.Self().GetRuntimeSaveDataModel();
                    RuntimeActorDataModel addActor = null;
                    for (int i = 0; i < runtimeSave.runtimeActorDataModels.Count; i++)
                        if (runtimeSave.runtimeActorDataModels[i].actorId == _actorId)
                        {
                            if (_isInit)
                            {
                                //既にActorDataが存在する場合は一度配列から削除する
                                runtimeSave.runtimeActorDataModels.RemoveAt(i);
                            }
                            else
                            {
                                //初期化しない場合、このデータをパーティに追加する
                                addActor = runtimeSave.runtimeActorDataModels[i];
                            }
                            break;
                        }

                    //ActorData未生成、又は初期化する設定であった場合には、新規にActorDataを作成する
                    if (_isInit || addActor == null)
                    {
                        //パーティに追加するActorDataの差し替え
                        addActor = SetActorData(_actorId);
                    }

                    //パーティメンバーへ追加
                    workList.Add(addActor.actorId);
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors = workList;

                    //バトルはUpdateで切り替わる
                    MapManager.AddPartyOnMap(_actorId);
                    if (GameStateHandler.IsBattle())
                    {
                        BattleManager.GetSpriteSet().CreateActors();
                    }
                }
            }
            else
            {
                if (haveCharacter)
                {
                    //パーティメンバーを離脱させる
                    //離脱時には、初期化するチェックボックスは無効
                    var runtimeSave = DataManager.Self().GetRuntimeSaveDataModel();
                    if (!runtimeSave.runtimePartyDataModel.actors.Contains(_actorId))
                        return;
                    int index = runtimeSave.runtimePartyDataModel.actors.IndexOf(_actorId);                    
                    runtimeSave.runtimePartyDataModel.actors.RemoveAt(index);

                    //バトルはUpdateで切り替わる
                    MapManager.SubPartyOnMap(_actorId);
                    if (GameStateHandler.IsBattle())
                    {
                        BattleManager.GetSpriteSet().CreateActors();
                    }
                }
            }
        }

        public RuntimeActorDataModel SetActorData(string actorData) {
            //ActorDataを作成
            var runtimeSave = DataManager.Self().GetRuntimeSaveDataModel();
            CoreSystem.Knowledge.Misc.BattleSceneTransition.Actor battleSceneTransitionActorData = new CoreSystem.Knowledge.Misc.BattleSceneTransition.Actor();
            battleSceneTransitionActorData.id = actorData;
            battleSceneTransitionActorData.level = -1;
            battleSceneTransitionActorData.equipIds = new string[] { };
            RuntimeActorDataModel newActor = DataManager.Self().CreateActorData(battleSceneTransitionActorData);

            //作成したActorDataを追加
            runtimeSave.runtimeActorDataModels.Add(newActor);

            //ActorDataを返却
            return newActor;
        }
    }
}