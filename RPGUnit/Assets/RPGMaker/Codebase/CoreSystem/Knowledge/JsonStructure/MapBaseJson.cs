using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class MapBaseJson : IJsonStructure
    {
        public string mapId;
        public int index;
        public string name;
        public int SerialNumber;

        public MapBaseJson(
            string id,
            int index,
            string name,
            int SerialNumber
        )
        {
            mapId = id;
            this.index = index;
            this.name = name;
            this.SerialNumber = SerialNumber;
        }

        public string GetID() {
            return mapId.ToString();
        }
    }
}