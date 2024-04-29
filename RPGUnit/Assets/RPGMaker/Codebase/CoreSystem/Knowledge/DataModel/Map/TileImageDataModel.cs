using System;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map
{
    [Serializable]
    public class TileImageDataModel
    {
        public string    filename;
        public Texture2D texture;

        public TileImageDataModel(Texture2D tex, string fn) {
            texture = tex;
            filename = fn;
        }
    }
}