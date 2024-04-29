using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class WeaponRepository : AbstractDatabaseRepository<WeaponDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Initializations/JSON/weapon.json";
#if !UNITY_EDITOR
        public new List<WeaponDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<WeaponDataModel>(JsonPath) as List<WeaponDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif

#if UNITY_EDITOR
        public void SetWeaponEquipType() {
            
            var system = new SystemRepository().Load();
            Load();

            for (int i = 0; i < DataModels.Count; i++)
            {
                if (DataModels[i].basic.equipmentTypeId == "")
                {
                    DataModels[i].basic.equipmentTypeId = system.equipTypes[0].id;
                }
            }

            Save(DataModels);
        }

#endif
    }
}