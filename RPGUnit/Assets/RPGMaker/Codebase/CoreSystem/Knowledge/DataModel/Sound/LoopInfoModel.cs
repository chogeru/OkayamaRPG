using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Sound
{
    [Serializable]
    public class LoopInfoModel
    {
        public string name;
        public int start;
        public int end;

        public LoopInfoModel(
            string name,
            int start,
            int end
        )
        {
            this.name = name;
            this.start = start;
            this.end = end;
        }

	    public bool isEqual(LoopInfoModel data)
	    {
	        return name == data.name && 
	        	   start == data.start && 
	        	   end == data.end;
	    }
    }
}