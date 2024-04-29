using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon
{
    [Serializable]
    public class EventCommonDataModel : WithSerialNumberDataModel
    {
        public List<EventCommonCondition> conditions;
        public string                     eventId;
        public string                     name;

        public EventCommonDataModel(string eventId, string name, List<EventCommonCondition> conditions) {
            this.eventId = eventId;
            this.name = name;
            this.conditions = conditions;
        }

        public static EventCommonDataModel CreateDefault(string id, string name) {
            return new EventCommonDataModel(id, name, new List<EventCommonCondition>());
        }

        public EventCommonDataModel Clone() {
            return (EventCommonDataModel) MemberwiseClone();
        }

        public bool isEqual(EventCommonDataModel data) {
            if (conditions.Count != data.conditions.Count)
                return false;

            for (int i = 0; i < conditions.Count; i++)
                if (!conditions[i].isEqual(data.conditions[i]))
                    return false;

            return eventId == data.eventId && 
                   name == data.name;
        }

        [Serializable]
        public struct EventCommonCondition
        {
            public int    trigger;
            public string switchId;

            public EventCommonCondition(int trigger, string switchId) {
                this.trigger = trigger;
                this.switchId = switchId;
            }

            public bool isEqual(EventCommonCondition data) {
                return trigger == data.trigger && 
                       switchId == data.switchId;
            }
        }
    }
}