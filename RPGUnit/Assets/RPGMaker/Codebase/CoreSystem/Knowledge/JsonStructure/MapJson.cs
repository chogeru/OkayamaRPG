using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class MapJson : IJsonStructure
    {
        public string                     mapId;
        public int                        index;
        public string                     displayName;
        public string                     name;
        public int                        width;
        public int                        height;
        public int                        deleted;
        public List<MapEncounter>         encounter;
        public List<MapLayer>             layers;
        
        public MapDataModel.MapScrollType scrollType;
        public bool                       autoPlayBGM;
        public string                     bgmID;
        public SoundState                 bgmState;
        public bool                       autoPlayBgs;
        public string                     bgsID;
        public SoundState                 bgsState;
        public bool                       forbidDash;
        public Background                 background;
        public parallax                   Parallax;
        public string                     memo;

        public MapJson(
            string id,
            int index,
            string name,
            string displayName,
            List<MapDataModel.Layer> layers,
            
            int width,
            int height,
            MapDataModel.MapScrollType scrollType,
            bool autoPlayBGM,
            string bgmID,
            SoundState bgmState,
            bool autoPlayBgs,
            string bgsID,
            SoundState bgsState,
            bool forbidDash,
            Background background,
            parallax Parallax,
            string memo
        )
        {
            mapId = id;
            this.index = index;
            this.name = name;
            this.displayName = displayName;

            this.layers = new List<MapLayer>();
            foreach (var layer in layers)
            {
                var ids = new List<string>();
                if(layer != null && layer.tilesOnPalette != null && layer.tilesOnPalette.Count > 0)
                    ids = layer.tilesOnPalette.Select(item => { return item.id; }).ToList();
                this.layers.Add(new MapLayer(layer.type.ToString(),ids));
            }

            this.width = width;
            this.height = height;
            this.scrollType = scrollType;
            this.autoPlayBGM = autoPlayBGM;
            this.bgmID = bgmID;
            this.bgmState = bgmState;
            this.autoPlayBgs = autoPlayBgs;
            this.bgsID = bgsID;
            this.bgsState = bgsState;
            this.forbidDash = forbidDash;
            this.background = background;
            this.Parallax = Parallax;
            this.memo = memo;
        }

        public string GetID() {
            return mapId.ToString();
        }
    }

    [Serializable]
    public class MapEncounter
    {
        public int region;
    }

    [Serializable]
    public class MapLayer
    {
        public string       type;
        public List<string> tileIdsOnPalette;

        public MapLayer(string type, List<string> tileIdsOnPalette) {
            this.type = type;
            this.tileIdsOnPalette = tileIdsOnPalette;
        }

        public bool isEqual(MapLayer data) {
            if (type != data.type ||
                tileIdsOnPalette.Count != data.tileIdsOnPalette.Count)
                return false;

            for (int i = 0; i < tileIdsOnPalette.Count; i++)
                if (tileIdsOnPalette[i] != data.tileIdsOnPalette[i])
                    return false;

            return true;
        }
    }

    public enum MapScrollType
    {
        NoLoop,
        LoopVertical,
        LoopHorizontal,
        LoopBoth
    }

    [Serializable]
    public class SoundState
    {
        public SoundState(
            int pan,
            int pitch,
            int volume
            ) {
            this.pan = pan;
            this.pitch = pitch;
            this.volume = volume;
        }

        public int pan;
        public int pitch;
        public int volume;
    }

    [Serializable]
    public class Background
    {
        public string imageName;
        public int imageZoomIndex;
        public bool showInEditor = true;

        public Background(string imageName, int imageZoomIndex, bool showInEditor)
        {
            this.imageName = imageName;
            this.imageZoomIndex = imageZoomIndex;
            this.showInEditor = showInEditor;
        }
    }

    [Serializable]
    public class parallax
    {
        public parallax(
            bool   loopX,
            bool   loopY,
            string name,
            bool   show,
            int    sx,
            int    sy,
            bool   zoom0,
            bool   zoom2,
            bool   zoom4
        )
        {
            this.loopX = loopX;
            this.loopY = loopY;
            this.name = name;
            this.show = show;
            this.sx = sx;
            this.sy = sy;
            this.zoom0 = zoom0;
            this.zoom2 = zoom2;
            this.zoom4 = zoom4;
        }

        public bool   loopX;
        public bool   loopY;
        public string name;
        public bool   show;
        public int    sx;
        public int    sy;
        public bool   zoom0;
        public bool   zoom2;
        public bool   zoom4;
    }

    [Serializable]
    public class MapJsonTranslation
    {
        public string mapId;
        public string name;
    }
}