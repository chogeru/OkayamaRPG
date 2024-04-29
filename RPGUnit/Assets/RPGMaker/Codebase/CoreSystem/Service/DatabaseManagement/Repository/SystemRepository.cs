using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.IO;
using JsonHelper = RPGMaker.Codebase.CoreSystem.Helper.JsonHelper;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class SystemRepository
    {
        private const string JsonPath = "Assets/RPGMaker/Storage/Initializations/JSON/system.json";

        private static SystemSettingDataModel _systemSettingDataModel;

        public void Save(SystemSettingDataModel systemSettingDataModel) {
            if (systemSettingDataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JsonPath, JsonHelper.ToJson(systemSettingDataModel));

            // キャッシュを更新
            _systemSettingDataModel = systemSettingDataModel;

            SetSerialNumbers();
        }

        public SystemSettingDataModel Load() {
            if (_systemSettingDataModel != null)
                // キャッシュがあればそれを返す
                return _systemSettingDataModel;
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonPath);
            _systemSettingDataModel = JsonHelper.FromJson<SystemSettingDataModel>(jsonString);
#else
            _systemSettingDataModel = ScriptableObjectOperator.GetClass<SystemSettingDataModel>(JsonPath) as SystemSettingDataModel;
#endif

            SetSerialNumbers();

            return _systemSettingDataModel;
        }

        private void SetSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            Action<WithSerialNumberDataModel, int> func = (withSerialNumberDataModel, index) =>
            {
                withSerialNumberDataModel.SerialNumber = index + 1;
            };
            for (var i = 0; i < _systemSettingDataModel.elements.Count; i++)
                func(_systemSettingDataModel.elements[i], i);
            for (var i = 0; i < _systemSettingDataModel.weaponTypes.Count; i++)
                func(_systemSettingDataModel.weaponTypes[i], i);
            for (var i = 0; i < _systemSettingDataModel.armorTypes.Count; i++)
                func(_systemSettingDataModel.armorTypes[i], i);
            for (var i = 0; i < _systemSettingDataModel.skillTypes.Count; i++)
                func(_systemSettingDataModel.skillTypes[i], i);
            for (var i = 0; i < _systemSettingDataModel.equipTypes.Count; i++)
                func(_systemSettingDataModel.equipTypes[i], i);
        }
    }
}