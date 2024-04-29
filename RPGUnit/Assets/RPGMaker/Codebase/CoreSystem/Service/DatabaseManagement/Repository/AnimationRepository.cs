using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class AnimationRepository : AbstractDatabaseRepository<AnimationDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Animation/JSON/animation.json";
#if !UNITY_EDITOR
        public new List<AnimationDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<AnimationDataModel>(JsonPath) as List<AnimationDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}