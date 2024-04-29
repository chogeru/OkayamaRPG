using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class EventMapJson : IJsonStructure
    {
        public string eventId;
        public string mapId;
        public string name;
        public string note;
        public int    x;
        public int    y;

        public string GetID() {
            return eventId;
        }
    }

    [Serializable]
    public class EventMapPageJson
    {
        public EventMapPageConditionsJson conditions;
        public int                        eventTrigger;
        public EventMapPageImageJson      image;
        public EventMapPageMoveJson       move;
        public int                        page;
        public int                        priority;
        public EventMapPageWalkJson       walk;
    }

    [Serializable]
    public class EventMapPageConditionActorJson
    {
        public string actorId;
        public int    enabled;
    }

    [Serializable]
    public class EventMapPageConditionImageJson
    {
        public int    enabled;
        public string imageName;
    }

    [Serializable]
    public class EventMapPageConditionItemJson
    {
        public int    enabled;
        public string itemId;
    }

    [Serializable]
    public class EventMapPageConditionsJson
    {
        public EventMapPageConditionActorJson      actor;
        public EventMapPageConditionImageJson      image;
        public EventMapPageConditionItemJson       item;
        public EventMapPageConditionSelfSwitchJson selfSwitch;
        public EventMapPageConditionSwitchJson     switchOne;
        public EventMapPageConditionSwitchJson     switchTwo;
        public EventMapPageConditionVariablesJson  variables;
    }

    [Serializable]
    public class EventMapPageConditionSelfSwitchJson
    {
        public int    enabled;
        public string selfSwitch = "A";

        public EventMapPageConditionSelfSwitchJson(int enabled, string selfSwitch) {
            this.enabled = enabled;
            this.selfSwitch = selfSwitch;
        }
    }

    [Serializable]
    public class EventMapPageConditionSwitchJson
    {
        public int    enabled;
        public string switchId = "A";

        public EventMapPageConditionSwitchJson(int enabled, string switchId) {
            this.enabled = enabled;
            this.switchId = switchId;
        }
    }

    [Serializable]
    public class EventMapPageConditionVariablesJson
    {
        public int    enabled;
        public int    value;
        public string variableId;

        public EventMapPageConditionVariablesJson(int enabled, string variableId, int value) {
            this.enabled = enabled;
            this.variableId = variableId;
            this.value = value;
        }
    }

    [Serializable]
    public class EventMapPageImageJson
    {
        public int    direction;
        public string name;

        public EventMapPageImageJson(string name, int direction) {
            this.name = name;
            this.direction = direction;
        }
    }

    [Serializable]
    public class EventMapPageMoveJson
    {
        public int                  frequency;
        public int                  moveType;
        public EventMoveRouteJson[] route;
        public int                  speed;

        public EventMapPageMoveJson(int moveType, int speed, int frequency, EventMoveRouteJson[] route) {
            this.moveType = moveType;
            this.speed = speed;
            this.frequency = frequency;
            this.route = route;
        }
    }

    [Serializable]
    public class EventMapPageWalkJson
    {
        public int directionFix;
        public int stepping;
        public int through;
        public int walking;

        public EventMapPageWalkJson(int walking, int stepping, int directionFix, int through) {
            this.walking = walking;
            this.stepping = stepping;
            this.directionFix = directionFix;
            this.through = through;
        }
    }

    [Serializable]
    public class EventMoveRouteJson
    {
        public int      code;
        public string[] parameters;

        public EventMoveRouteJson(int code, string[] parameters) {
            this.code = code;
            this.parameters = parameters;
        }
    }
}