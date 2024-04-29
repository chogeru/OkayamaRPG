using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Sound;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class LoopInfoSO : ScriptableObject
    {
        public List<LoopInfoModel> dataModels;

        public bool isEquals(LoopInfoSO loopInfoSO) {
            if (dataModels.Count != loopInfoSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(loopInfoSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}