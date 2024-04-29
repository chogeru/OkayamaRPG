using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class EventSO : ScriptableObject
    {
        public EventDataModel dataModel;

        public bool isEquals(EventSO eventSO) {
            return dataModel.isEqual(eventSO.dataModel);
        }
    }
}