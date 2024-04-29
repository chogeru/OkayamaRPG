using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class EventBattleSO : ScriptableObject
    {
        public List<EventBattleDataModel> dataModels;

        public bool isEquals(EventBattleSO eventBattleSO) {
            if (dataModels.Count != eventBattleSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(eventBattleSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}