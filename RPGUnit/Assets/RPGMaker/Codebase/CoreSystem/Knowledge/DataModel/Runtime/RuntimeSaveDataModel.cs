using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimeSaveDataModel
    {
        public List<RuntimeActorDataModel> runtimeActorDataModels;
        public RuntimePartyDataModel runtimePartyDataModel;
        public RuntimePlayerDataModel runtimePlayerDataModel;
        public RuntimeOnMapDataModel RuntimeOnMapDataModel;
        public RuntimeSystemConfigDataModel runtimeSystemConfig;
        public List<SaveDataSelfSwitchesData> selfSwitches;
        public SaveDataSwitchesData switches;
        public SaveDataVariablesData variables;
        public RuntimeScreenDataModel runtimeScreenDataModel;

        public RuntimeSaveDataModel() {
            runtimeSystemConfig = RuntimeSystemConfigDataModel.CreateDefault();
            runtimePartyDataModel = new RuntimePartyDataModel();
            runtimePlayerDataModel = new RuntimePlayerDataModel();
            RuntimeOnMapDataModel = new RuntimeOnMapDataModel();
            runtimeScreenDataModel = new RuntimeScreenDataModel();
            runtimeActorDataModels = new List<RuntimeActorDataModel>();
            selfSwitches = new List<SaveDataSelfSwitchesData>();
            switches = new SaveDataSwitchesData();
            variables = new SaveDataVariablesData();
        }

        public bool HasItem(string id) {
            for (int i = 0; i < runtimePartyDataModel.items.Count; i++)
            {
                if (runtimePartyDataModel.items[i].itemId == id)
                {
                    if (runtimePartyDataModel.items[i].value > 0)
                    {
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        public bool HasSwitchItem(string id) {
            for (int i = 0; i < runtimePartyDataModel.items.Count; i++)
            {
                if (runtimePartyDataModel.items[i].itemId == id)
                {
                    if (runtimePartyDataModel.items[i].value > 0)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int i = 0; i < runtimePartyDataModel.weapons.Count; i++)
            {
                if (runtimePartyDataModel.weapons[i].weaponId == id)
                {
                    if (runtimePartyDataModel.weapons[i].value > 0)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int i = 0; i < runtimePartyDataModel.armors.Count; i++)
            {
                if (runtimePartyDataModel.armors[i].armorId == id)
                {
                    if (runtimePartyDataModel.armors[i].value > 0)
                    {
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        public bool ActorInParty(string id) {
            var work = runtimePartyDataModel.actors;
            if (work.Contains(id)) return true;

            return false;
        }

        [Serializable]
        public class SaveDataSwitchesData
        {
            public List<bool> data;
        }

        [Serializable]
        public class SaveDataVariablesData
        {
            public List<string>               data;
            public List<SoundCommonDataModel> sound;
        }

        [Serializable]
        public class SaveDataSelfSwitchesData
        {
            public List<bool> data;
            public string     id;
        }
    }
}