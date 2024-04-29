using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor
{
    [Serializable]
    public class ArmorDataModel : WithSerialNumberDataModel
    {
        public Basic                      basic;
        public string                     memo;
        public List<int>                  parameters;
        public List<TraitCommonDataModel> traits;

        public ArmorDataModel(Basic basic, List<int> parameters, List<TraitCommonDataModel> traits, string memo) {
            this.basic = basic;
            this.parameters = parameters;
            this.traits = traits;
            this.memo = memo;
        }

        public static ArmorDataModel CreateDefault(string id) {
            return new ArmorDataModel(Basic.CreateDefault(id), new List<int> {0, 0, 0, 0, 0, 0, 0, 0},
                new List<TraitCommonDataModel>(), "");
        }

        public bool isEqual(ArmorDataModel data) {
            if (!basic.isEqual(data.basic) ||
                memo != data.memo)
                return false;

            if (traits.Count != data.traits.Count)
                return false;
            for (int i = 0; i < traits.Count; i++)
                if (!traits[i].isEqual(data.traits[i]))
                    return false;

            if (parameters.Count != data.parameters.Count)
                return false;
            for (int i = 0; i < parameters.Count; i++)
                if (parameters[i] != data.parameters[i])
                    return false;

            return true;
        }

        [Serializable]
        public class Basic
        {
            public string animationId;
            public string armorTypeId;
            public int    canSell;
            public string description;
            public string equipmentTypeId;
            public string iconId;
            public string id;
            public string name;
            public int    price;
            public int    sell;
            public int    switchItem;

            public Basic(
                string id,
                string name,
                string description,
                string equipmentTypeId,
                string animationId,
                string iconId,
                string armorTypeId,
                int price,
                int sell,
                int canSell,
                int switchItem
            ) {
                this.id = id;
                this.name = name;
                this.description = description;
                this.equipmentTypeId = equipmentTypeId;
                this.animationId = animationId;
                this.iconId = iconId;
                this.armorTypeId = armorTypeId;
                this.price = price;
                this.sell = sell;
                this.canSell = canSell;
                this.switchItem = switchItem;
            }

            public static Basic CreateDefault(string id) {
                return new Basic(
                    id,
                    "",
                    "",
                    "",
                    "",
                    "IconSet_000",
                    "",
                    0,
                    0,
                    0,
                    0
                );
            }

            public bool isEqual(Basic data) {
                return animationId == data.animationId &&
                       armorTypeId == data.armorTypeId &&
                       canSell == data.canSell &&
                       description == data.description &&
                       equipmentTypeId == data.equipmentTypeId &&
                       iconId == data.iconId &&
                       id == data.id &&
                       name == data.name &&
                       price == data.price &&
                       sell == data.sell &&
                       switchItem == data.switchItem;
            }
        }
    }
}