using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;
using System;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class WordDefinitionRepository
    {
        private static readonly string JSON_PATH = "Assets/RPGMaker/Storage/Word/JSON/words.json";

        private static WordDefinitionDataModel _wordDefinitionDataModel;
        
        public void Save(WordDefinitionDataModel wordDefinitionDataModel) {
            if (wordDefinitionDataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JSON_PATH, JsonUtility.ToJson(wordDefinitionDataModel));

            // キャッシュを更新
            _wordDefinitionDataModel = wordDefinitionDataModel;
        }

        public WordDefinitionDataModel Load() {
            if (_wordDefinitionDataModel != null) return _wordDefinitionDataModel;
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH);
            _wordDefinitionDataModel = JsonHelper.FromJson<WordDefinitionDataModel>(jsonString);
#else
            _wordDefinitionDataModel =
 ScriptableObjectOperator.GetClass<WordDefinitionDataModel>(JSON_PATH) as WordDefinitionDataModel;
#endif
            return _wordDefinitionDataModel;
        }
    }
}