using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class TileGroupJson : IJsonStructure
    {
        public string     id;
        public string     name;
        public List<Tile> tileList;

        public TileGroupJson(string id, string name, List<Tile> tileList) {
            this.id = id;
            this.name = name;
            this.tileList = tileList;
        }

        public string GetID() {
            return id;
        }

        [Serializable]
        public class Tile
        {
            public string id;
            public string type;

            public Tile(string id, string type) {
                this.id = id;
                this.type = type;
            }
        }
    }
}