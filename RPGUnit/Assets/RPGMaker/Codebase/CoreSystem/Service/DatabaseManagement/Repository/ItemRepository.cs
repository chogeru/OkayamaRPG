using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class ItemRepository : AbstractDatabaseRepository<ItemDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Item/JSON/item.json";
        public void DeleteItem(int itemId) {
            throw new NotImplementedException();
        }

        public void ChangeMaximum(int maximumNum) {
            throw new NotImplementedException();
        }

#if !UNITY_EDITOR
        public new List<ItemDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<ItemDataModel>(JsonPath) as List<ItemDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}