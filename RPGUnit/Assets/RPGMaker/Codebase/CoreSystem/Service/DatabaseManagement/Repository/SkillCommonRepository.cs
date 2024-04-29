using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCommon;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class SkillCommonRepository : AbstractDatabaseRepository<SkillCommonDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Initializations/JSON/skill.json";
        
#if !UNITY_EDITOR
        public new List<SkillCommonDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<SkillCommonDataModel>(JsonPath) as List<SkillCommonDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}