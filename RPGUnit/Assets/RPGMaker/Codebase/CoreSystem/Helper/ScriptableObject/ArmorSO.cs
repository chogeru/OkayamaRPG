using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class ArmorSO : ScriptableObject
    {
        public List<ArmorDataModel> dataModels;

        public bool isEquals(ArmorSO armorSO) {
            if (dataModels.Count != armorSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(armorSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}