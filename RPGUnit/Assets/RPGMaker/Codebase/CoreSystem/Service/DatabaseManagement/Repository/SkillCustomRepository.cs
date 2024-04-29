using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class SkillCustomRepository : AbstractDatabaseRepository<SkillCustomDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Initializations/JSON/skillCustom.json";
#if !UNITY_EDITOR
        public new List<SkillCustomDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<SkillCustomDataModel>(JsonPath) as List<SkillCustomDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}