using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimePartyDataModel
    {
        public List<string> actors;
        public List<Armor>  armors;
        public int          gold;
        public int          inBattle;
        public List<Item>   items;
        public LastItem     lastItem;
        public int          menuActorId;
        public int          steps;
        public int          targetActorId;
        public List<Weapon> weapons;
        public LastData     lastData;

        public RuntimePartyDataModel() {
            inBattle = 0;
            gold = 0;
            steps = 0;
            lastItem = new LastItem();
            lastItem.itemId = "";
            menuActorId = 0;
            targetActorId = 0;
            actors = new List<string> {"1", "2", "3", "4"};
            var item = new Item
            {
                itemId = "",
                value = 0
            };
            items = new List<Item> {item};
            var weapon = new Weapon
            {
                weaponId = "",
                value = 2
            };
            weapons = new List<Weapon> {weapon};
            var armor = new Armor
            {
                armorId = "",
                value = 2
            };
            armors = new List<Armor> {armor};

            lastData = new LastData();
            lastData.skillId = 0;
            lastData.itemId = 0;
            lastData.actionActorId = 0;
            lastData.actionEnemyIndex = 0;
            lastData.targetActorId = 0;
            lastData.targetEnemyIndex = 0;
        }

        [Serializable]
        public class LastItem
        {
            public string itemId;
        }


        [Serializable]
        public class Item
        {
            public string itemId;
            public int    value;
        }

        [Serializable]
        public class Weapon
        {
            public int    value;
            public string weaponId;
        }

        [Serializable]
        public class Armor
        {
            public string armorId;
            public int    value;
        }

        [Serializable]
        public class LastData
        {
            public int skillId;
            public int itemId;
            public int actionActorId;
            public int actionEnemyIndex;
            public int targetActorId;
            public int targetEnemyIndex;
        }
    }
}