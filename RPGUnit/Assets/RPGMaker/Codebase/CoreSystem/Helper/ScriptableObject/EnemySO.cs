using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class EnemySO : ScriptableObject
    {
        public List<EnemyDataModel> dataModels;

        public bool isEquals(EnemySO enemySO) {
            if (dataModels.Count != enemySO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(enemySO.dataModels[i]))
                    return false;

            return true;
        }
    }
}