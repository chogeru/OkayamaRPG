using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class VehicleRepository : AbstractDatabaseRepository<VehiclesDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Character/JSON/vehicles.json";
#if !UNITY_EDITOR
        public new List<VehiclesDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<VehiclesDataModel>(JsonPath) as List<VehiclesDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}