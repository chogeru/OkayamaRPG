using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map
{
    [Serializable]
    public class MapBaseDataModel
    {
        public string id;
        public string name;
        public int SerialNumber;

        public MapBaseDataModel(string id, string name, int SerialNumber) {
            this.id = id;
            this.name = name;
            this.SerialNumber = SerialNumber;
        }

	    public bool isEqual(MapBaseDataModel data)
	    {
	        return id == data.id &&
	               name == data.name &&
	               SerialNumber == data.SerialNumber;
	    }
    }
}