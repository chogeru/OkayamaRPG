using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    [Serializable]
    public class MapSubDataModel
    {
        public MapSubDataModel(string id, string name, List<EventSubDataModel> events, List<Region> regions) {
            ID = id;
            Name = name;
            Events = events;
            Regions = regions;
        }

        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public List<EventSubDataModel> Events { get; set; }
        [field: SerializeField] public List<Region> Regions { get; private set; }
    }

    [Serializable]
    public class Region
    {
        public int ID;

        public Region(int id) {
            ID = id;
        }
    }
}