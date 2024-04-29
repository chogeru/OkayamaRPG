using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class EventJson : IJsonStructure
    {
        public EventCommand[] eventCommands;
        public string         id;
        public int            page;
        public int            type;

        public EventJson(string id, int type, int page, EventCommand[] eventCommands) {
            this.id = id;
            this.type = type;
            this.page = page;
            this.eventCommands = eventCommands;
        }

        public string GetID() {
            return id;
        }
    }

    [Serializable]
    public class EventCommand
    {
        public int                 code;
        public int                 pageNo;
        public string[]            parameters;
        public EventCommandRoute[] route;

        public EventCommand(int code, string[] parameters, int pageNo, EventCommandRoute[] route) {
            this.code = code;
            this.parameters = parameters;
            this.pageNo = pageNo;
            this.route = route;
        }
    }

    [Serializable]
    public class EventCommandRoute
    {
        public int      code;
        public int      codeIndex;
        public string[] parameters;

        public EventCommandRoute(int code, string[] parameters, int codeIndex) {
            this.code = code;
            this.parameters = parameters;
            this.codeIndex = codeIndex;
        }
    }
}