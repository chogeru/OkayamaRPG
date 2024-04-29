using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    [Serializable]
    public class SwitchSubDataModel
    {
        public SwitchSubDataModel(
            int id,
            string name,
            List<EventSubDataModel> belongingEventEntities,
            List<EventSubDataModel> referringEventEntities
        ) {
            ID = id;
            Name = name;
            BelongingEventEntities = belongingEventEntities;
            ReferringEventEntities = referringEventEntities;
        }

        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public string Name { get; private set; }

        [field: SerializeField]
        public List<EventSubDataModel> BelongingEventEntities { get; private set; } // スイッチが所属するイベント

        [field: SerializeField]
        public List<EventSubDataModel> ReferringEventEntities { get; private set; } // このスイッチがonになることで影響を受けるイベント群
    }
}