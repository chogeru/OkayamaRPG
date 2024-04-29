using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class StateSO : ScriptableObject
    {
        public List<StateDataModel> dataModels;

        public bool isEquals(StateSO stateSO) {
            if (dataModels.Count != stateSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(stateSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}