using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class SystemSO : ScriptableObject
    {
        public SystemSettingDataModel dataModels;

        public bool isEquals(SystemSO systemSO) {
            return dataModels.isEqual(systemSO.dataModels);
        }
    }
}