using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class EncounterRepository : AbstractDatabaseRepository<EncounterDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Encounter/JSON/encounter.json";
        
#if !UNITY_EDITOR
        public new List<EncounterDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<EncounterDataModel>(JsonPath) as List<EncounterDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}