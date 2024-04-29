using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class WordSO : ScriptableObject
    {
        public WordDefinitionDataModel dataModel;

        public bool isEquals(WordSO wordSO) {
            return dataModel.isEqual(wordSO.dataModel);
        }
    }
}