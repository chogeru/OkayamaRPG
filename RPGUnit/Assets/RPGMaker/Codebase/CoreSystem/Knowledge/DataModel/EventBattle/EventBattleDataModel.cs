using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle
{
    [Serializable]
    public class EventBattleDataModel
    {
        public string                eventId;
        public List<EventBattlePage> pages;

        public EventBattleDataModel(string eventId, List<EventBattlePage> pages) {
            this.eventId = eventId;
            this.pages = pages;
        }

        public static EventBattleDataModel CreateDefault() {
            return new EventBattleDataModel(null, new List<EventBattlePage>());
        }

        public static EventBattlePage CreateDefaultEventBattlePage(int page) {
            return new EventBattlePage(
                page,
                "",
                new EventBattleCondition(1, 0,
                    new EventBattlePageConditionTurn(0, 0, 0),
                    new EventBattlePageConditionActorHp(0, "", 0),
                    new EventBattlePageConditionEnemyHp(0, "", 0),
                    new EventBattlePageConditionSwitchData(0, ""),
                    0));
        }

        public bool isEqual(EventBattleDataModel data) {
            if (eventId != data.eventId ||
            	pages.Count != data.pages.Count)
                return false;

            for (int i = 0; i < pages.Count; i++)
                if (!pages[i].isEqual(data.pages[i]))
                    return false;

            return true;
        }

        [Serializable]
        public class EventBattlePage
        {
            public EventBattleCondition condition;
            public string               eventId;
            public int                  page;

            public EventBattlePage(int page, string eventId, EventBattleCondition condition) {
                this.page = page;
                this.eventId = eventId;
                this.condition = condition;
            }

            public bool isEqual(EventBattlePage data) {
                return condition.isEqual(data.condition) &&
                       eventId == data.eventId &&
                       page == data.page;
            }
        }

        [Serializable]
        public class EventBattleCondition
        {
            public EventBattlePageConditionActorHp    actorHp;
            public EventBattlePageConditionEnemyHp    enemyHp;
            public int                                run;
            public int                                span;
            public EventBattlePageConditionSwitchData switchData;
            public EventBattlePageConditionTurn       turn;
            public int                                turnEnd;

            public EventBattleCondition(
                int run,
                int turnEnd,
                EventBattlePageConditionTurn turn,
                EventBattlePageConditionActorHp actorHp,
                EventBattlePageConditionEnemyHp enemyHp,
                EventBattlePageConditionSwitchData switchData,
                int span
            ) {
                this.run = run;
                this.turnEnd = turnEnd;
                this.turn = turn;
                this.actorHp = actorHp;
                this.enemyHp = enemyHp;
                this.switchData = switchData;
                this.span = span;
            }

            public bool isEqual(EventBattleCondition data) {
                return actorHp.isEqual(data.actorHp) &&
                       enemyHp.isEqual(data.enemyHp) &&
                       run == data.run &&
                       span == data.span &&
                       switchData.isEqual(data.switchData) &&
                       turn.isEqual(data.turn) &&
                       turnEnd == data.turnEnd;
            }
        }

        [Serializable]
        public class EventBattlePageConditionActorHp
        {
            public string actorId;
            public int    enabled;
            public int    value;

            public EventBattlePageConditionActorHp(int enabled, string actorId, int value) {
                this.enabled = enabled;
                this.actorId = actorId;
                this.value = value;
            }

            public bool isEqual(EventBattlePageConditionActorHp data) {
                return actorId == data.actorId && 
                	   enabled == data.enabled && 
                	   value == data.value;
            }
        }

        [Serializable]
        public class EventBattlePageConditionEnemyHp
        {
            public int    enabled;
            public string enemyId;
            public int    value;

            public EventBattlePageConditionEnemyHp(int enabled, string enemyId, int value) {
                this.enabled = enabled;
                this.enemyId = enemyId;
                this.value = value;
            }

            public bool isEqual(EventBattlePageConditionEnemyHp data) {
                return enabled == data.enabled &&
                       enemyId == data.enemyId && 
                       value == data.value;
            }
        }

        [Serializable]
        public class EventBattlePageConditionSwitchData
        {
            public int    enabled;
            public string switchId;

            public EventBattlePageConditionSwitchData(int enabled, string switchId) {
                this.enabled = enabled;
                this.switchId = switchId;
            }

            public bool isEqual(EventBattlePageConditionSwitchData data) {
                return enabled == data.enabled && 
                       switchId == data.switchId;
            }
        }

        [Serializable]
        public class EventBattlePageConditionTurn
        {
            public int enabled;
            public int end;
            public int start;

            public EventBattlePageConditionTurn(int enabled, int start, int end) {
                this.enabled = enabled;
                this.start = start;
                this.end = end;
            }

            public bool isEqual(EventBattlePageConditionTurn data) {
                return enabled == data.enabled &&
                       start == data.start && 
                       end == data.end;
            }
        }
    }
}