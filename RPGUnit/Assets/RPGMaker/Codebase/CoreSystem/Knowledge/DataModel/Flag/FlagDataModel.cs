using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag
{
    [Serializable]
    public class FlagDataModel
    {
        public List<Switch>   switches;
        public List<Variable> variables;

        public FlagDataModel(List<Switch> switches, List<Variable> variables) {
            this.switches = switches;
            this.variables = variables;
        }

        public bool isEqual(FlagDataModel data) {
            if (switches.Count != data.switches.Count || 
                variables.Count != data.variables.Count)
                return false;

            for (int i = 0; i < switches.Count; i++)
                if (!switches[i].isEqual(data.switches[i]))
                    return false;

            for (int i = 0; i < variables.Count; i++)
                if (!variables[i].isEqual(data.variables[i]))
                    return false;

            return true;
        }

        [Serializable]
        public class Switch : WithSerialNumberDataModel
        {
            public List<Event> events;
            public string      id;
            public string      name;

            public Switch(string id, string name, List<Event> events) {
                this.id = id;
                this.name = name;
                this.events = events;
            }

            public static Switch CreateDefault() {
#if UNITY_EDITOR
                return new Switch(Guid.NewGuid().ToString(), CoreSystemLocalize.LocalizeText("WORD_1518"), new List<Event>());
#else
                return new Switch(Guid.NewGuid().ToString(), "", new List<Event>());
#endif
            }

            public bool isEqual(Switch data) {
                if (events.Count != data.events.Count)
                    return false;
                for (int i = 0; i < events.Count; i++)
                    if (!events[i].isEqual(data.events[i]))
                        return false;

                return id == data.id &&
                       name == data.name;
            }
        }

        [Serializable]
        public class Variable : WithSerialNumberDataModel
        {
            public List<Event> events;
            public string      id;
            public string      name;

            public Variable(string id, string name, List<Event> events) {
                this.id = id;
                this.name = name;
                this.events = events;
            }

            public static Variable CreateDefault() {
#if UNITY_EDITOR
                return new Variable(Guid.NewGuid().ToString(), CoreSystemLocalize.LocalizeText("WORD_1518"), new List<Event>());
#else
                return new Variable(Guid.NewGuid().ToString(), "", new List<Event>());
#endif
            }

            public bool isEqual(Variable data) {
                if (events.Count != data.events.Count)
                    return false;
                for (int i = 0; i < events.Count; i++)
                    if (!events[i].isEqual(data.events[i]))
                        return false;

                return id == data.id &&
                       name == data.name;
            }
        }

        [Serializable]
        public class Event
        {
            public int eventId;
            public int mapId;

            public Event(int mapId, int eventId) {
                this.mapId = mapId;
                this.eventId = eventId;
            }

            public bool isEqual(Event data) {
                return eventId == data.eventId &&
                       mapId == data.mapId;
            }
        }
    }
}