using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository
{
    public class MapRepository
    {
        private const string JsonFile	       = "Assets/RPGMaker/Storage/Map/JSON/Map/";
        private const string JsonBaseFile      = "Assets/RPGMaker/Storage/Map/JSON/mapbase.json";
        private const string OldJsonFile       = "Assets/RPGMaker/Storage/Map/JSON/map.json";
        private const string JsonFileTranslation = "Assets/RPGMaker/Storage/Map/JSON/mapname.json";
        private const string JsonFileSample    = "Assets/RPGMaker/Storage/Map/JSON/MapSample/";
        private const string OldJsonFileSample = "Assets/RPGMaker/Storage/Map/JSON/mapSample.json";

        private static List<MapDataModel> _mapDataModels;
        private static List<MapBaseDataModel> _mapBaseDataModels;
        private static List<MapDataModel> _mapSampleDataModels;

        public enum SaveType
        {
            NO_PREFAB = 0,
            SAVE_PREFAB,
            SAVE_PREFAB_FORCE
        }

        public List<MapDataModel> LoadMapDataModels() {
#if UNITY_EDITOR
            if (_mapDataModels != null)
            {
                // キャッシュがあればそれを返す
                return _mapDataModels;
            }

            _mapDataModels = GetJsons(JsonFile).Select(ConvertJsonToEntity).ToList();
#else
            //UnityEngine.Debug.LogError("Don't call.");
#endif

            SetSerialNumbers();

            return _mapDataModels;
        }

        public List<MapBaseDataModel> LoadMapBaseDataModels() {
#if UNITY_EDITOR
            // 再生中でなければ
            if (!EditorApplication.isPlaying)
            {
                if (_mapBaseDataModels != null)
                {
                    // キャッシュがあればそれを返す
                    return _mapBaseDataModels;
                }
            }

            _mapBaseDataModels = GetBaseJsons(JsonFile).Select(ConvertJsonToEntityBase).ToList();
            SetBaseSerialNumbers();
#else
            _mapBaseDataModels = ScriptableObjectOperator.GetClass<MapBaseDataModel>(JsonBaseFile) as List<MapBaseDataModel>;
#endif

            return _mapBaseDataModels;
        }

        public MapDataModel LoadMapDataModel(string id, bool isSampleMap) {
#if UNITY_EDITOR
            if (!isSampleMap)
            {
                if (_mapDataModels != null)
                {
                    for (int i = 0; i < _mapDataModels.Count; i++)
                    {
                        if (_mapDataModels[i].id == id)
                        {
                            SetSerialNumbers();
                            return _mapDataModels[i];
                        }
                    }
                }

                return ConvertJsonToEntity(
                    JsonHelper.FromJson<MapJson>(
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFile + id + ".json")));
            }
            else
            {
                return ConvertJsonToEntity(
                    JsonHelper.FromJson<MapJson>(
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileSample + id + ".json")));
            }
#else
            return ScriptableObjectOperator.GetClass<MapDataModel>(JsonFile + id + ".json") as MapDataModel;
#endif
        }

        public List<MapDataModel> LoadMapDataModels(bool reload) {
#if UNITY_EDITOR
            _mapDataModels = GetJsons(JsonFile).Select(ConvertJsonToEntity).ToList();

            SetSerialNumbers();

            return _mapDataModels;
#else
            //UnityEngine.Debug.LogError("Don't call.");
            return null;
#endif
        }

#if UNITY_EDITOR
        private List<MapJson> GetJsons(string path) {
            var mapJson = new List<MapJson>();
            var files = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
                mapJson.Add(
                    JsonHelper.FromJson<MapJson>(UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(file)));
            
            mapJson = mapJson.OrderBy(item => item.index).ToList();

            return mapJson;
        }

        private List<MapBaseJson> GetBaseJsons(string path) {
            var mapJson = new List<MapBaseJson>();
            var files = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
                mapJson.Add(
                    JsonHelper.FromJson<MapBaseJson>(UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(file)));

            mapJson = mapJson.OrderBy(item => item.index).ToList();

            return mapJson;
        }
#endif

        public void SaveMapDataModelForEditor(MapDataModel mapDataModel, SaveType savePrefab) {
            // マッププレハブ保存 (存在している場合のみ)
            if (savePrefab == SaveType.SAVE_PREFAB)
            {
                //Unload時に保存する
                mapDataModel.changePrefab = true;
                mapDataModel.isSampleMap = false;
            }
            else if (savePrefab == SaveType.SAVE_PREFAB_FORCE)
            {
                //強制的に保存する
                var mapPrefab = mapDataModel.MapPrefabManagerForEditor.mapPrefab;
                if (mapPrefab != null)
                {
                    UnityEditorWrapper.PrefabUtilityWrapper.SaveAsPrefabAsset(mapPrefab, mapDataModel.GetPrefabPath());
                }
                mapDataModel.changePrefab = true;
                mapDataModel.isSampleMap = false;
            }

            // その他パラメータをJSONに保存
            var json = ConvertEntityToJsonForEditor(mapDataModel);
            File.WriteAllText(JsonFile + mapDataModel.id + ".json", JsonHelper.ToJson(json));

            // JSONに保存したデータを改めて読込、キャッシュする
            if (_mapDataModels == null)
                _mapDataModels = LoadMapDataModels();

            bool flg = false;
            for (int i = 0; i < _mapDataModels.Count; i++)
            {
                if (_mapDataModels[i].id == mapDataModel.id)
                {
                    if (mapDataModel != _mapDataModels[i])
                    {
                        MapDataModel.CopyData(mapDataModel, _mapDataModels[i]);
                    }
                    flg = true;
                    break;
                }
            }
            if (!flg)
            {
                _mapDataModels.Add(MapDataModel.CopyData(mapDataModel));
            }

#if UNITY_EDITOR
            if (!flg)
            {
                AddressableManager.Path.SetAddressToAsset(mapDataModel.GetPrefabPath());
            }
#endif

            SetSerialNumbers();
        }

        public void RemoveMapEntity(MapDataModel mapDataModel) {
            // マッププレハブ削除
            UnityEditorWrapper.PrefabUtilityWrapper.RemovePrefabAsset(mapDataModel.GetPrefabPath());

            if (_mapDataModels == null)
                _mapDataModels = LoadMapDataModels();

            var targetIndex = _mapDataModels.FindIndex(item => item.id == mapDataModel.id);
            if (targetIndex != -1)
            {
                _mapDataModels.RemoveAt(targetIndex);
            }

            File.Delete(JsonFile + mapDataModel.id + ".json");

            SetSerialNumbers();
        }

        public static void RemoveCache(string id) {
            if (_mapDataModels == null) return;
            for (int i = 0; i < _mapDataModels.Count; i++)
            {
                if (_mapDataModels[i].id == id)
                {
                    MapDataModel.CopyData(ConvertJsonToEntity(
                        JsonHelper.FromJson<MapJson>(
                            UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFile + id + ".json"))), _mapDataModels[i]);
                    break;
                }
            }
        }

        public void ResetMapEntity() {
            _mapDataModels = null;
        }

        public static MapDataModel ConvertJsonToEntity(MapJson json) {
            return new MapDataModel(
                json.mapId,
                json.index,
                json.name,
                json.displayName,
                json.width,
                json.height,
                json.scrollType,

                json.autoPlayBGM,
                json.bgmID,
                new MapDataModel.SoundState(json.bgmState.pan,json.bgmState.pitch,json.bgmState.volume),
                json.autoPlayBgs,
                json.bgsID,
                new MapDataModel.SoundState(json.bgsState.pan,json.bgsState.pitch,json.bgsState.volume),

                json.forbidDash,

                json.memo,
                json.layers,
                new MapDataModel.Background(
                    json.background.imageName,
                    (MapDataModel.ImageZoomIndex)json.background.imageZoomIndex,
                    json.background.showInEditor),
                new MapDataModel.parallax(json.Parallax.loopX, json.Parallax.loopY, json.Parallax.name,
                    json.Parallax.show, json.Parallax.sx, json.Parallax.sy, json.Parallax.zoom0, json.Parallax.zoom2,
                    json.Parallax.zoom4)
            );
        }

        public MapBaseDataModel ConvertJsonToEntityBase(MapBaseJson json) {
            return new MapBaseDataModel(
                json.mapId,
                json.name,
                json.SerialNumber
            );
        }

        private MapJson ConvertEntityToJsonForEditor(MapDataModel dataModel) {
            var backgroundLayer = new Background(
                dataModel.background.imageName,
                (int)dataModel.background.imageZoomIndex,
                dataModel.background.showInEditor);

            var parallax = new parallax(
                dataModel.Parallax.loopX,
                dataModel.Parallax.loopY,
                dataModel.Parallax.name,
                dataModel.Parallax.show,
                dataModel.Parallax.sx,
                dataModel.Parallax.sy,
                dataModel.Parallax.zoom0,
                dataModel.Parallax.zoom2,
                dataModel.Parallax.zoom4
            );

            var BGMState = new SoundState(
                dataModel.bgmState.pan,
                dataModel.bgmState.pitch,
                dataModel.bgmState.volume
            );

            var BGSState = new SoundState(
                dataModel.bgsState.pan,
                dataModel.bgsState.pitch,
                dataModel.bgsState.volume
            );

            return new MapJson(
                dataModel.id,
                dataModel.index,
                dataModel.name,
                dataModel.displayName,
                dataModel.MapPrefabManagerForEditor.layers,
                dataModel.width,
                dataModel.height,
                dataModel.scrollType,
                dataModel.autoPlayBGM,
                dataModel.bgmID,
                BGMState,
                dataModel.autoPlayBgs,
                dataModel.bgsID,
                BGSState,
                dataModel.forbidDash,
                backgroundLayer,
                parallax,
                dataModel.memo
            );
        }
        
        private void SetSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            for (var i = 0; i < _mapDataModels.Count; i++)
            {
                _mapDataModels[i].SerialNumber = i + 1;
            }
        }

        private void SetBaseSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            for (var i = 0; i < _mapBaseDataModels.Count; i++)
            {
                _mapBaseDataModels[i].SerialNumber = i + 1;
            }
        }

        // SampleMap
        //--------------------------------------------------------------------------------------------------------------
        public List<MapDataModel> LoadMapSampleDataModels() {
#if UNITY_EDITOR
            // 再生中でなければ
            if (!EditorApplication.isPlaying)
            {
                if (_mapSampleDataModels != null)
                {
                    // キャッシュがあればそれを返す
                    return _mapSampleDataModels;
                }
            }

            _mapSampleDataModels = GetJsons(JsonFileSample).Select(ConvertJsonToEntity).ToList();
#else
            _mapSampleDataModels = ScriptableObjectOperator.GetClass<MapDataModel>(JsonFile) as List<MapDataModel>;
#endif

            SetSampleSerialNumbers();

            return _mapSampleDataModels;
        }

        public void SaveMapSampleDataModelForEditor(MapDataModel mapDataModel) {
            // マッププレハブ保存 (存在している場合のみ)
            var mapPrefab = mapDataModel.MapPrefabManagerForEditor.mapPrefab;
            if (mapPrefab != null)
            {
                UnityEditorWrapper.PrefabUtilityWrapper.SaveAsPrefabAsset(
                    mapPrefab, mapDataModel.GetPrefabPath(true));
            }

            // その他パラメータをJSONに保存
            if (_mapSampleDataModels == null)
                _mapSampleDataModels = LoadMapSampleDataModels();

            var targetIndex = _mapSampleDataModels.FindIndex(item => item.id == mapDataModel.id);
            if (targetIndex != -1)
            {
                _mapSampleDataModels[targetIndex] = mapDataModel;
            }
            else
            {
                _mapSampleDataModels.Add(mapDataModel);
            }

            var json = ConvertEntityToJsonForEditor(mapDataModel);
            File.WriteAllText(JsonFileSample + mapDataModel.id + ".json", JsonHelper.ToJson(json));

#if UNITY_EDITOR
            if (targetIndex == -1)
            {
                AddressableManager.Path.SetAddressToAsset(mapDataModel.GetPrefabPath(true));
            }
#endif

            SetSampleSerialNumbers();
        }

        public void RemoveMapSampleEntity(MapDataModel mapDataModel) {
            // マッププレハブ削除
            UnityEditorWrapper.PrefabUtilityWrapper.RemovePrefabAsset(mapDataModel.GetPrefabPath(true));

            if (_mapSampleDataModels == null)
                _mapSampleDataModels = LoadMapSampleDataModels();

            var targetIndex = _mapSampleDataModels.FindIndex(item => item.id == mapDataModel.id);
            if (targetIndex != -1)
            {
                _mapSampleDataModels.RemoveAt(targetIndex);
            }

            var json = _mapSampleDataModels.Select(ConvertEntityToJsonForEditor);
            File.WriteAllText(JsonFileSample + mapDataModel.id + ".json", JsonHelper.ToJson(json));

#if UNITY_EDITOR
            AddressableManager.Path.SetAddressToAsset(mapDataModel.GetPrefabPath(true));
#endif

            SetSampleSerialNumbers();
        }

        public void ResetMapSampleEntity() {
            _mapSampleDataModels = null;
        }

        private void SetSampleSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            for (var i = 0; i < _mapSampleDataModels.Count; i++)
            {
                _mapSampleDataModels[i].SerialNumber = i + 1;
            }
        }
    }
}