using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public class ClassSO : ScriptableObject
    {
        public List<ClassDataModel> dataModels;

        public bool isEquals(ClassSO classSO) {
            if (dataModels.Count != classSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(classSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}