using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Helper.SO;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.IO;
using UnityEngine;
using JsonHelper = RPGMaker.Codebase.CoreSystem.Helper.JsonHelper;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class FlagsRepository
    {
        private static readonly string JSON_PATH = "Assets/RPGMaker/Storage/Flags/JSON/flags.json";

        private static FlagDataModel _flagDataModel;

        public void Save(FlagDataModel flagDataModel) {
            if (flagDataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JSON_PATH, JsonUtility.ToJson(flagDataModel));

            // キャッシュを更新
            _flagDataModel = flagDataModel;

            SetSerialNumbers();
        }

        public void SaveVariable(FlagDataModel.Variable variable) {
            if (_flagDataModel != null) Load();

            var targetIndex = _flagDataModel.variables.FindIndex(item => item.id == variable.id);
            _flagDataModel.variables[targetIndex] = variable;

            SetSerialNumbers();

            Save(_flagDataModel);
        }

        public void SaveSwitch(FlagDataModel.Switch sw) {
            if (_flagDataModel != null) Load();

            var targetIndex = _flagDataModel.switches.FindIndex(item => item.id == sw.id);
            _flagDataModel.switches[targetIndex] = sw;

            SetSerialNumbers();

            Save(_flagDataModel);
        }

        public FlagDataModel Load() {
            if (_flagDataModel != null)
                // キャッシュがあればそれを返す
                return _flagDataModel;
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH);
            _flagDataModel = JsonHelper.FromJson<FlagDataModel>(jsonString);
#else
            _flagDataModel = AddressableManager.Load.LoadAssetSync<FlagsSO>(JSON_PATH).dataModel;
#endif
            SetSerialNumbers();

            return _flagDataModel;
        }

        private void SetSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            Action<WithSerialNumberDataModel, int> func = (withSerialNumberDataModel, index) =>
            {
                withSerialNumberDataModel.SerialNumber = index + 1;
            };
            for (var i = 0; i < _flagDataModel.switches.Count; i++) func(_flagDataModel.switches[i], i);
            for (var i = 0; i < _flagDataModel.variables.Count; i++) func(_flagDataModel.variables[i], i);
        }
    }
}