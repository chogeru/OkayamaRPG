using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class ClassRepository : AbstractDatabaseRepository<ClassDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Character/JSON/class.json";
#if !UNITY_EDITOR
        public new List<ClassDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<ClassDataModel>(JsonPath) as List<ClassDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}