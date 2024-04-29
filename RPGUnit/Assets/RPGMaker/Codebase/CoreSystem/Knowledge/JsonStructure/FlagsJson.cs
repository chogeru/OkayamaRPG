using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class FlagsJson
    {
        public List<SwitchJson>   switches;
        public List<VariableJson> variables;
    }

    [Serializable]
    public class SwitchJson
    {
        public List<FlagEventJson> belongingEvents;
        public string              name;
        public List<FlagEventJson> referringEvents;
        public int                 switchId;
    }

    [Serializable]
    public class VariableJson
    {
        public List<FlagEventJson> belongingEvents;
        public string              name;
        public List<FlagEventJson> referringEvents;
        public int                 variableId;
    }

    [Serializable]
    public class FlagEventJson
    {
        public string eventId;
        public string mapId;
    }
}