using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimePlayerDataModel
    {
        public PlayerMap            map;

        [Serializable]
        public class PlayerMap
        {
            public int          blendMode;
            public int          dashing;
            public int          direction;
            public string       mapId;
            public int          moveSpeed;
            public int          nameDisplay;
            public float        opacity;
            public int          realX;
            public int          realY;
            public int          transparent;
            public List<Vhicle> vehicles;
            public int          x;
            public int          y;
            public int          cameraX;
            public int          cameraY;
            public string       eventBattleBack1 = "";
            public string       eventBattleBack2 = "";
        }

        [Serializable]
        public class Vhicle
        {
            public string    assetId;
            public int       direction;
            public string    id;
            public string    mapId;
            public List<int> moveTags;
            public string    name;
            public int       realX;
            public int       realY;
            public int       speed;
            public int       x;
            public int       y;
            public int       ride;
        }
    }
}