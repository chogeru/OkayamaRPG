using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository
{
    public class TileGroupRepository
    {
        private const string JsonFile = "Assets/RPGMaker/Storage/Map/JSON/tileGroup.json";

        private static List<TileGroupDataModel> _tileGroupDataModels;

        public List<TileGroupDataModel> GetTileGroupEntities() {
            if (_tileGroupDataModels != null) return _tileGroupDataModels;

            var jsons = GetTileGroupJsons();
            _tileGroupDataModels = jsons.Select(ConvertJsonToEntity).ToList();

#if ENABLE_DEVELOPMENT_FIX
            // 重複タイル削除
            if (!File.Exists("Assets/tilegroupinitialize.txt"))
            {
                File.WriteAllText("Assets/tilegroupinitialize.txt", "");

                for (int i = 0; i < _tileGroupDataModels.Count; i++)
                {
                    bool fix = false;
                    for (int i2 = 0; i2 < _tileGroupDataModels[i].tileDataModels.Count; i2++)
                    {
                        for (int i3 = i2 + 1; i3 < _tileGroupDataModels[i].tileDataModels.Count; i3++)
                        {
                            if (_tileGroupDataModels[i].tileDataModels[i2].id == _tileGroupDataModels[i].tileDataModels[i3].id)
                            {
                                _tileGroupDataModels[i].tileDataModels.RemoveAt(i3);
                                i3--;
                                fix = true;
                            }
                        }
                        if (fix)
                            StoreTileGroupEntity(_tileGroupDataModels[i]);
                    }
                }
            }
#endif

            SetSerialNumbers();

            return _tileGroupDataModels;
        }

        public void StoreTileGroupEntity(TileGroupDataModel tileGroupDataModel) {
            var jsons = GetTileGroupJsons();
            var jsonToAdd = ConvertEntityToJson(tileGroupDataModel);

            var edited = false;
            for (var index = 0; index < jsons.Count; index++)
                if (jsons[index].id == jsonToAdd.id)
                {
                    jsons[index] = jsonToAdd;
                    edited = true;
                    break;
                }

            // 追加
            if (!edited)
            {
                jsons.Add(jsonToAdd);
                _tileGroupDataModels.Add(tileGroupDataModel);
            }

            File.WriteAllText(JsonFile, JsonHelper.ToJsonArray(jsons));

            SetSerialNumbers();
        }

        public void RemoveTileGroupEntity(TileGroupDataModel tileGroupDataModel) {
            _tileGroupDataModels.Remove(tileGroupDataModel);

            var jsons = GetTileGroupJsons();
            var jsonToRemove = ConvertEntityToJson(tileGroupDataModel);

            // Removeで削除できなかったため一致時にRemoveAt
            for (var index = 0; index < jsons.Count; index++)
                if (jsons[index].id == jsonToRemove.id)
                {
                    jsons.RemoveAt(index);
                    break;
                }

            File.WriteAllText(JsonFile, JsonHelper.ToJsonArray(jsons));

            SetSerialNumbers();
        }

        public void ResetTileGroupEntity() {
            _tileGroupDataModels = null;
        }

        private List<TileGroupJson> GetTileGroupJsons() {
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFile);
            return JsonHelper.FromJsonArray<TileGroupJson>(jsonString);
        }

        private TileGroupDataModel ConvertJsonToEntity(TileGroupJson json) {
            var tileEntities = new TileRepository().GetTileTable();
            var tileGroupData = new TileGroupDataModel(
                json.id,
                json.name,
                json.tileList.Select(tile => { return tileEntities.FirstOrDefault(entity => entity.id == tile.id); })
                    .ToList()
            );

            // タイルが読み込めない場合は削除
            for (int i = 0; i < tileGroupData.tileDataModels.Count; i++)
            {
                if (tileGroupData.tileDataModels[i] == null)
                {
                    tileGroupData.tileDataModels.RemoveAt(i);
                    i--;
                }
            }

            return tileGroupData;
        }

        private TileGroupJson ConvertEntityToJson(TileGroupDataModel dataModel) {
            return new TileGroupJson(
                dataModel.id,
                dataModel.name,
                dataModel.tileDataModels
                    .Select(tileEntity => new TileGroupJson.Tile(tileEntity.id, tileEntity.type.ToString())) != null ?
                dataModel.tileDataModels
                    .Select(tileEntity => new TileGroupJson.Tile(tileEntity.id, tileEntity.type.ToString())).ToList() :
                new List<TileGroupJson.Tile>()
            );
        }

        private void SetSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            for (var i = 0; i < _tileGroupDataModels.Count; i++)
            {
                if (_tileGroupDataModels[i] == null) continue;
                _tileGroupDataModels[i].SerialNumber = i + 1;
            }
        }
    }
}