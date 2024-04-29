using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common
{
    [Serializable]
    public class SoundCommonDataModel
    {
        public string name;
        public int    pan;
        public int    pitch;
        public int    volume;

        public SoundCommonDataModel(string name, int pan, int pitch, int volume) {
            this.name = name;
            this.pan = pan;
            this.pitch = pitch;
            this.volume = volume;
        }

        public static SoundCommonDataModel CreateDefault() {
            return new SoundCommonDataModel("", 0, 0, 100);
        }

	    public bool isEqual(SoundCommonDataModel data)
	    {
	        return name == data.name &&
	         	   pan == data.pan &&
	          	   pitch == data.pitch &&
	               volume == data.volume;
	    }

        /// <summary>
        /// 同一のオーディオかチェックする
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsEqualsSoundInfo(SoundCommonDataModel data)
        {
            return name == data.name && pitch == data.pitch;
        }
    }
}