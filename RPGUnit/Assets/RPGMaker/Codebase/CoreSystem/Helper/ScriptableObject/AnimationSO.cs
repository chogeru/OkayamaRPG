using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class AnimationSO : ScriptableObject
    {
        public List<AnimationDataModel> dataModels;

        public bool isEquals(AnimationSO animationSO) {
            if (dataModels.Count != animationSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(animationSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}