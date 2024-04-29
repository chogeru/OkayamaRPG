using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle
{
    [Serializable]
    public class VehiclesDataModel : WithSerialNumberDataModel
    {
        // 移動領域。
        public enum MoveAriaType
        {
            None,   // なし。
            Low,    // 低空域。
            High,   // 高空域。
        }

        public BGM       bgm;
        public string    id;
        public string    images;
        public List<int> initialPos;
        public string    mapId;
        public List<int> moveTags;
        public string    name;
        public int       speed;

        public VehiclesDataModel(
            string id,
            string name,
            List<int> moveTags,
            int speed,
            string mapId,
            List<int> initialPos,
            BGM bgm,
            string images
        ) {
            this.id = id;
            this.name = name;
            this.moveTags = moveTags;
            this.speed = speed;
            this.mapId = mapId;
            this.initialPos = initialPos;
            this.bgm = bgm;
            this.images = images;
        }

        // 移動領域の取得。
        public MoveAriaType MoveAria
        {
            get
            {
                for (var index = 0; index < moveTags.Count; index++)
                {
                    if (moveTags[index] != 0)
                    {
                        return (MoveAriaType)index;
                    }
                }

                return (MoveAriaType)0;
            }
        }

        public static VehiclesDataModel CreateDefault() {
            return new VehiclesDataModel(
                Guid.NewGuid().ToString(),
                "",
                new List<int> {0, 0, 0},
                10,
                "",
                new List<int> {0, 0},
                BGM.CreateDefault(),
                ""
            );
        }

        public bool isEqual(VehiclesDataModel data) {
            if (bgm.name != data.bgm.name ||
                bgm.pan != data.bgm.pan ||
                bgm.pitch != data.bgm.pitch ||
                bgm.volume != data.bgm.volume ||
                id != data.id ||
                images != data.images ||
                initialPos.Count != data.initialPos.Count ||
                mapId != data.mapId ||
                moveTags.Count != data.moveTags.Count ||
                name != data.name ||
                speed != data.speed)
                return false;

            for (int i = 0; i < initialPos.Count; i++)
                if (initialPos[i] != data.initialPos[i])
                    return false;

            for (int i = 0; i < moveTags.Count; i++)
                if (moveTags[i] != data.moveTags[i])
                    return false;

            return true;
        }

        [Serializable]
        public class BGM
        {
            public string name;
            public int    pan;
            public int    pitch;
            public int    volume;

            public BGM(
                string name,
                int pan,
                int pitch,
                int volume
            ) {
                this.name = name;
                this.pan = pan;
                this.pitch = pitch;
                this.volume = volume;
            }

            public static BGM CreateDefault() {
                return new BGM(
                    "",
                    0,
                    100,
                    90
                );
            }

		    public bool isEqual(BGM data)
		    {
		        return name == data.name &&
		               pan == data.pan &&
		               pitch == data.pitch &&
		               volume == data.volume;
		    }
        }
    }
}