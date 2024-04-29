#if !UNITY_EDITOR
using System.Collections.Generic;
#endif
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class CharacterActorRepository : AbstractDatabaseRepository<CharacterActorDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Character/JSON/characterActor.json";
#if !UNITY_EDITOR
        public new List<CharacterActorDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<CharacterActorDataModel>(JsonPath) as List<CharacterActorDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}