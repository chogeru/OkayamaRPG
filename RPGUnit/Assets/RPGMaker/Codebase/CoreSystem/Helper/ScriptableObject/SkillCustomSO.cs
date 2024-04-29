using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class SkillCustomSO : ScriptableObject
    {
        public List<SkillCustomDataModel> dataModels;

        public bool isEquals(SkillCustomSO skillCustomSO) {
            if (dataModels.Count != skillCustomSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(skillCustomSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}