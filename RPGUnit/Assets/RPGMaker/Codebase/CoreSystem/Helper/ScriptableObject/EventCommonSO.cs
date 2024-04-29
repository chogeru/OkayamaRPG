using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class EventCommonSO : ScriptableObject
    {
        public List<EventCommonDataModel> dataModels;

        public bool isEquals(EventCommonSO eventCommonSO) {
            if (dataModels.Count != eventCommonSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(eventCommonSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}