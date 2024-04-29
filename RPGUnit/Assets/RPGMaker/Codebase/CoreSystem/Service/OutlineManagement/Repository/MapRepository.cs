using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RPGMaker.Codebase.CoreSystem.Service.OutlineManagement.Repository
{
    public class MapRepository
    {
        private const string JsonDirOfMap = "Assets/RPGMaker/Storage/Map/JSON/Map";

        /**
         * マップ一覧をJSONから読み出す.
         */
        public List<MapSubDataModel> GetMaps() {
            var mapJson = new List<MapJson>();
            var files = Directory.GetFiles(JsonDirOfMap, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
                mapJson.Add(
                    JsonHelper.FromJson<MapJson>(UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(file)));

            mapJson = mapJson.OrderBy(item => item.index).ToList();

            return mapJson.Select(mapJson => DataConverter.ConvertMapToObject(mapJson, new List<EventSubDataModel>()))
                .ToList();
        }
    }
}