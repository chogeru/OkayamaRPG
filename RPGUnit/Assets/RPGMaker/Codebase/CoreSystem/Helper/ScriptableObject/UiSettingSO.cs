using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class UiSettingSO : ScriptableObject
    {
        public UiSettingDataModel dataModel;

        public bool isEquals(UiSettingSO uiSettingSO) {
            return dataModel.isEqual(uiSettingSO.dataModel);
        }
    }
}