using RPGMaker.Codebase.CoreSystem.Helper;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event
{
    [Serializable]
    public class EventDataModel
    {
        // enum / static consts
        //--------------------------------------------------------------------------------------------------------------
        public enum EventSequence
        {
            Start,
            Exec,
            End
        }

        public static List<string> ActorVariables = new List<string>
        {
            "Level", "EXP", "HP", "MP", "Max HP", "Max MP", "Attack", "Defense", "M.Attack", "M.Defence", "Agility",
            "Luck"
        };

        public static List<string> EnemyVariables = new List<string>
            {"HP", "MP", "Max HP", "Max MP", "Attack", "Defense", "M.Attack", "M.Defence", "Agility", "Luck"};

        public static List<string> CharacterVariables = new List<string>
            {"Map X", "Map Y", "Direction", "Screen X", "Screen Y"};

        public static List<string> OtherVariables = new List<string>
        {
            "Map ID", "Party Members", "Gold", "Steps", "Play Time", "Timer", "Save Count", "Battle Count", "Win Count",
            "Escape Count"
        };

        public List<EventCommand> eventCommands;

        // properties
        //--------------------------------------------------------------------------------------------------------------
        public string id;
        public int    page;
        public int    type;

        /**
         * constructor
         */
        public EventDataModel(string id, int page, int type, List<EventCommand> eventCommands) {
            this.id = id;
            this.page = page;
            this.type = type;
            this.eventCommands = eventCommands;
        }

        // methods
        //--------------------------------------------------------------------------------------------------------------
        public EventDataModel Clone() {
            var @event = new EventDataModel(id, page, type, new List<EventCommand>());
            for (var i = 0; i < eventCommands.Count; i++) @event.eventCommands.Add(eventCommands[i].Clone());

            return @event;
        }

        public static EventDataModel CreateDefault() {
            return new EventDataModel(Guid.NewGuid().ToString(), 0, 0, new List<EventCommand>());
        }

        public static int GetNextEventId() {
            var eventId = -1;
            var guidEventPaths =
                UnityEditorWrapper.AssetDatabaseWrapper.FindAssets("", new[] {"Assets/Tkool/Editor/Event"});
            var pathsEvents = new string[guidEventPaths.Length];
            for (var i = 0; i < guidEventPaths.Length; i++)
            {
                pathsEvents[i] = UnityEditorWrapper.AssetDatabaseWrapper.GUIDToAssetPath(guidEventPaths[i]);
                string[] del = {"_"};
                var path = pathsEvents[i].Split(del, StringSplitOptions.None);
                var eventIdWork = int.Parse(path[1]);
                if (eventId < eventIdWork) eventId = eventIdWork;
            }

            eventId++;
            return eventId;
        }

	    public bool isEqual(EventDataModel data)
	    {
	        if (eventCommands.Count != data.eventCommands.Count)
	            return false;
	
	        for (int i = 0; i < eventCommands.Count; i++)
	            if (!eventCommands[i].isEqual(data.eventCommands[i]))
	                return false;
	
	        return id == data.id &&
	               page == data.page &&
	               type == data.type;
	    }

        [Serializable]
        public class EventCommand
        {
            public enum EventSequence
            {
                Start,
                Exec,
                End
            }

            public int                         code;
            public int                         indent; //条件文などの入れ子になるイベント制御用
            public List<string>                parameters;
            public List<EventCommandMoveRoute> route;

            public EventCommand(int code, List<string> parameters, List<EventCommandMoveRoute> route, int indent = 0) {
                this.code = code;
                this.parameters = parameters;
                this.route = route;
                this.indent = indent;
            }

            public EventCommand Clone() {
                return (EventCommand) MemberwiseClone();
            }

            public bool isEqual(EventCommand data) {
                if (code != data.code ||
                    indent != data.indent ||
                    parameters.Count != data.parameters.Count ||
                    route.Count != data.route.Count) 
                    return false;

                for (int i = 0; i < parameters.Count; i++)
                    if (parameters[i] != data.parameters[i]) return false;

                for (int i = 0; i < route.Count; i++)
                    if (!route[i].isEqual(data.route[i])) return false;

                return true;
            }
        }

        [Serializable]
        public class EventCommandMoveRoute
        {
            public int          code;
            public int          codeIndex;
            public List<string> parameters;

            public EventCommandMoveRoute(int code, List<string> parameters, int codeIndex) {
                this.code = code;
                this.parameters = parameters;
                this.codeIndex = codeIndex;
            }

            public bool isEqual(EventCommandMoveRoute data) {
                if (code != data.code || 
                	codeIndex != data.codeIndex ||
                	parameters.Count != data.parameters.Count)
                    return false;

                for (int i = 0; i < parameters.Count; i++)
                    if (parameters[i] != data.parameters[i])
                        return false;
                        
                return true;
            }
        }
    }
}