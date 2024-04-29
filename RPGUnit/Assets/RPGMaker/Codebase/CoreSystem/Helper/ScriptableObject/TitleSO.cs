using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class TitleSO : ScriptableObject
    {
        public RuntimeTitleDataModel dataModel;

        public bool isEquals(TitleSO titleSO) {
            return dataModel.isEqual(titleSO.dataModel);
        }
    }
}