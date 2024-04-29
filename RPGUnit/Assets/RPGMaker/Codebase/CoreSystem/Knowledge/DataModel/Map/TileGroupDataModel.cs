using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map
{
    [Serializable]
    public class TileGroupDataModel : WithSerialNumberDataModel
    {
        public string              id;
        public string              name;
        public List<TileDataModelInfo> tileDataModels;

        public TileGroupDataModel(string id, string name, List<TileDataModelInfo> tileDataModels) {
            this.id = id;
            this.name = name;
            this.tileDataModels = tileDataModels;
        }

        public TileGroupDataModel Clone() {
            return (TileGroupDataModel) MemberwiseClone();
        }
    }
}