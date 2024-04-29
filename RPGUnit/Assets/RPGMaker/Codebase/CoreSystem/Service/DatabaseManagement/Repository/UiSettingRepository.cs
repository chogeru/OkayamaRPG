using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using System;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class UiSettingRepository
    {
        private const string JsonPath = "Assets/RPGMaker/Storage/Ui/JSON/ui.json";

        private static UiSettingDataModel _uiSettingDataModel;

        public void Save(UiSettingDataModel uiSettingDataModel) {
            if (uiSettingDataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JsonPath, JsonUtility.ToJson(uiSettingDataModel));

            // キャッシュを更新
            _uiSettingDataModel = uiSettingDataModel;
        }

        public UiSettingDataModel Load() {
            if (_uiSettingDataModel != null)
                // キャッシュがあればをそれを返す
                return _uiSettingDataModel;
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonPath);
            _uiSettingDataModel = JsonHelper.FromJson<UiSettingDataModel>(jsonString);
#else
            _uiSettingDataModel = ScriptableObjectOperator.GetClass<UiSettingDataModel>(JsonPath) as UiSettingDataModel;
#endif
            return _uiSettingDataModel;
        }
    }
}