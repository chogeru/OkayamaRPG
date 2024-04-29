using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class TroopRepository : AbstractDatabaseRepository<TroopDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Character/JSON/troop.json";
#if !UNITY_EDITOR
        public new List<TroopDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<TroopDataModel>(JsonPath) as List<TroopDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}