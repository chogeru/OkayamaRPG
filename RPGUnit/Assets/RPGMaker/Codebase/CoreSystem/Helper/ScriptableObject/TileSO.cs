using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class TileSO : ScriptableObject
    {
        public List<TileDataModel> dataModels;

        public bool isEquals(TileSO tileSO) {
            if (dataModels.Count != tileSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(tileSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}