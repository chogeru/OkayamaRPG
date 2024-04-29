using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class TroopSO : ScriptableObject
    {
        public List<TroopDataModel> dataModels;

        public bool isEquals(TroopSO troopSO) {
            if (dataModels.Count != troopSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(troopSO.dataModels[i]) == false)
                    return false;
            
            return true;     
        }
    }
}