using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class ItemSO : ScriptableObject
    {
        public List<ItemDataModel> dataModels;

        public bool isEquals(ItemSO itemSO) {
            if (dataModels.Count != itemSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(itemSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}