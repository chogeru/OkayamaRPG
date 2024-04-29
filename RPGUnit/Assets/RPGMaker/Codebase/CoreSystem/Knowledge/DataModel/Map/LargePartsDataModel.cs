using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map
{
    [Serializable]
    public class LargePartsDataModel
    {
        public string parentId;
        public int    x;
        public int    y;

        public LargePartsDataModel(string parentId, int x, int y) {
            this.parentId = parentId;
            this.x = x;
            this.y = y;
        }

	    public bool isEqual(LargePartsDataModel data)
	    {
	        return parentId == data.parentId &&
	               x == data.x &&
	               y == data.y;
	    }
    }
}