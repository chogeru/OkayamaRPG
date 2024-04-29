using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class EventMapSO : ScriptableObject
    {
        public List<EventMapDataModel> dataModels;

        public bool isEquals(EventMapSO eventMapSO) {
            if (dataModels.Count != eventMapSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(eventMapSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}