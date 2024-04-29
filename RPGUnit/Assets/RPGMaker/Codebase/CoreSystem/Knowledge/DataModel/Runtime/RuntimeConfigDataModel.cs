using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimeConfigDataModel
    {
        public int alwaysDash;
        public int bgmVolume;
        public int bgsVolume;
        public int commandRemember;
        public int meVolume;
        public int seVolume;

        public RuntimeConfigDataModel() {
            alwaysDash = 0;
            commandRemember = 0;
            bgmVolume = 100;
            bgsVolume = 100;
            meVolume = 100;
            seVolume = 100;
        }
    }
}