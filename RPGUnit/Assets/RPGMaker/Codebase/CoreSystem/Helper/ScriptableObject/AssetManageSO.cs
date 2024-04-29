using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class AssetManageSO : ScriptableObject
    {
        public AssetManageDataModel dataModel;

        public bool isEquals(AssetManageSO assetManageSO) {
            return dataModel.isEqual(assetManageSO.dataModel);
        }
    }
}