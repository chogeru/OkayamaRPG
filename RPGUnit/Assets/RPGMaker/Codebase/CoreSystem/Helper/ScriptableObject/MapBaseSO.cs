using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class MapBaseSO : ScriptableObject
    {
        public List<MapBaseDataModel> dataModel;

        public bool isEquals(MapBaseSO mapBaseSO) {
            if (dataModel.Count != mapBaseSO.dataModel.Count)
                return false;

            for (int i = 0; i < dataModel.Count; i++)
                if (dataModel[i].isEqual(mapBaseSO.dataModel[i]) == false)
                    return false;

            return true;
        }
    }
}