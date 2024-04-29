using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class MapSO : ScriptableObject
    {
        public MapDataModel dataModel;

        public bool isEquals(MapSO mapSO) {
            // MapPrefabの変更を検知できない為現状はfalseを返却
            //return dataModel.isEqual(mapSO.dataModel);
            return false;
        }
    }
}