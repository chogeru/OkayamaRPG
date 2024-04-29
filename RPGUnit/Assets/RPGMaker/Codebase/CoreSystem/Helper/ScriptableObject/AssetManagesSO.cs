using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class AssetManagesSO : ScriptableObject
    {
        public List<AssetManageDataModel> dataModels;

        public bool isEquals(AssetManagesSO assetManagesSO) {
            if (dataModels.Count != assetManagesSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(assetManagesSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}