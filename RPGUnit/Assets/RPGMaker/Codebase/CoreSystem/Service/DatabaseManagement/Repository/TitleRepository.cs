using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using System;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class TitleRepository
    {
        private const string JsonPath = "Assets/RPGMaker/Storage/Initializations/JSON/title.json";

        private static RuntimeTitleDataModel _runtimeTitleDataModel;

        public void Save(RuntimeTitleDataModel runtimeTitleDataModel) {
            if (runtimeTitleDataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JsonPath, JsonUtility.ToJson(runtimeTitleDataModel));

            // キャッシュを更新
            _runtimeTitleDataModel = runtimeTitleDataModel;
        }

        public RuntimeTitleDataModel Load() {
            if (_runtimeTitleDataModel != null)
            {
                // キャッシュがあればそれを返す
                return _runtimeTitleDataModel;
            }
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonPath);
            _runtimeTitleDataModel = JsonHelper.FromJson<RuntimeTitleDataModel>(jsonString);
#else
            _runtimeTitleDataModel =
 ScriptableObjectOperator.GetClass<RuntimeTitleDataModel>(JsonPath) as RuntimeTitleDataModel;
#endif
            return _runtimeTitleDataModel;
        }
    }
}