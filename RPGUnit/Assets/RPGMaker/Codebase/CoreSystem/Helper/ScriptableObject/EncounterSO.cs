using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class EncounterSO : ScriptableObject
    {
        public List<EncounterDataModel> dataModels;

        public bool isEquals(EncounterSO encounterSO) {
            if (dataModels.Count != encounterSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(encounterSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}