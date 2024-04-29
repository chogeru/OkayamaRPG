using System;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    [Serializable]
    public class EventSubDataModel
    {
        public EventSubDataModel(string id, string mapId, string name, string mapName = null) {
            ID = id;
            MapId = mapId;
            Name = name;
            MapName = mapName;
        }

        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public string MapId { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string MapName { get; private set; }
    }
}