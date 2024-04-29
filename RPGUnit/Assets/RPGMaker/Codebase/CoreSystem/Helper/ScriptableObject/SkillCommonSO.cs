using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCommon;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class SkillCommonSO : ScriptableObject
    {
        public List<SkillCommonDataModel> dataModels;

        public bool isEquals(SkillCommonSO skillCommonSO) {
            if (dataModels.Count != skillCommonSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(skillCommonSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}