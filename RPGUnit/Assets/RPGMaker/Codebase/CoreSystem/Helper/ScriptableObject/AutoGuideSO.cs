using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class AutoGuideSO : ScriptableObject
    {
        public List<AutoGuideDataModel> dataModels;

        public bool isEquals(AutoGuideSO autoGuideSO) {
            if (dataModels.Count != autoGuideSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(autoGuideSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}