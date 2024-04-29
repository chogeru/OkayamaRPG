using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGMaker.Codebase.CoreSystem.Service.MapManagement
{
    public class MapManagementService
    {
        // マップタイルに対する他の背景や遠景の縦位置のオフセット値 (実はタイルの方をずらすべきでは？)
        public const float YPositionOffsetToMapTile = 1f;

        private readonly MapRepository          _mapRepository;
        private readonly TileRepository         _tileRepository;
        private readonly TileGroupRepository    _tileGroupRepository;
        private readonly TileImageRepository    _tileImageRepository;

        public MapManagementService() {
            _mapRepository = new MapRepository();
            _tileRepository = new TileRepository();
            _tileGroupRepository = new TileGroupRepository();
            _tileImageRepository = new TileImageRepository();
        }

        /// <summary>
        /// マップを新規作成する。
        /// </summary>
        /// <returns>マップデータモデル。</returns>
        public MapDataModel CreateMapForEditor() {
            var mapPrefab = new GameObject();
            var grid = mapPrefab.AddComponent<Grid>();
            grid.cellGap = Vector3.zero;

            // レイヤーを新規生成し、json形式に変換
            var layers = CreateMapLayers(mapPrefab);
            var layerJsons = layers.Select(item =>
            {
                return new MapLayer(item.type.ToString(), item.tilesOnPalette?.Select(tile => tile.id).ToList());
            }).ToList();
            
            var mapDataModels = _mapRepository.LoadMapDataModels();
            int index = 0;
            if (mapDataModels.Count > 0)
                index = mapDataModels[mapDataModels.Count - 1].index + 1;
            var mapDataModelSerialNumber = mapDataModels.Count + 1;
            var mapDataModel = new MapDataModel(
                Guid.NewGuid().ToString(),
                index,
#if UNITY_EDITOR
                "#" + string.Format("{0:D4}", mapDataModelSerialNumber) + " " + CoreSystemLocalize.LocalizeText("WORD_1518"),
#else
                "#" + string.Format("{0:D4}", mapDataModelSerialNumber),
#endif
                "",
                10,
                10, 
                MapDataModel.MapScrollType.NoLoop,

                false,
                "",
                MapDataModel.SoundState.CreateDefault(),
                false,
                "",
                MapDataModel.SoundState.CreateDefault(),

                false,

                "",
                layerJsons,
                MapDataModel.Background.CreateDefault(),
                MapDataModel.CreateDefaultParallax(),
                mapPrefab
            );

            mapDataModel.MapPrefabManagerForEditor.CorrectionMapPrefab();

            //新規作成時はPrefabを強制的に保存する
            _mapRepository.SaveMapDataModelForEditor(mapDataModel, MapRepository.SaveType.SAVE_PREFAB_FORCE);

            return mapDataModel;
        }

        private List<MapDataModel.Layer> CreateMapLayers(GameObject prefabRoot) {
            // レイヤー生成
            var layers = new List<MapDataModel.Layer>();
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.DistantView),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.DistantView, null));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.Background),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.Background, null));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.BackgroundCollision),
                new MapDataModel.Layer(
                    MapDataModel.Layer.LayerType.BackgroundCollision, new Tilemap(),
                    new List<TileDataModelInfo>()));

            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.A),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.A, new Tilemap(), new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.A_Effect),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.A_Effect, new Tilemap(),
                    new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.B),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.B, new Tilemap(), new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.B_Effect),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.B_Effect, new Tilemap(),
                    new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.Shadow),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.Shadow, new Tilemap(), new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.C),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.C, new Tilemap(), new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.C_Effect),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.C_Effect, new Tilemap(),
                    new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.D),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.D, new Tilemap(), new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.D_Effect),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.D_Effect, new Tilemap(),
                    new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.ForRoute),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.ForRoute, new Tilemap(),
                    new List<TileDataModelInfo>()));
            layers.Insert(
                MapDataModel.GetLayerIndexByType(MapDataModel.Layer.LayerType.Region),
                new MapDataModel.Layer(MapDataModel.Layer.LayerType.Region, new Tilemap(),
                    new List<TileDataModelInfo>()));

            foreach (var layer in layers)
            {
                // prefabの編集
                var tilemapContainer = new GameObject();
                var tilemap = tilemapContainer.AddComponent<Tilemap>();

                // 遠景のコンポーネント設定
                if (layer.type == MapDataModel.Layer.LayerType.DistantView)
                {
                    // 遠景コンポーネントを設定する
                    var spriteRenderer = tilemapContainer.AddComponent<SpriteRenderer>();
                    spriteRenderer.material = UnityEditorWrapper.AssetDatabaseWrapper.
                        LoadAssetAtPath<Material>("Assets/RPGMaker/Codebase/Runtime/Map/DistantViewMaterial.mat");
                }
                // 背景の設定
                else if (layer.type == MapDataModel.Layer.LayerType.Background)
                {
                    // SpriteRendererを設定する
                    var spriteRenderer = tilemapContainer.AddComponent<SpriteRenderer>();
                }
                else
                {
                    var tilemapRenderer = tilemapContainer.AddComponent<TilemapRenderer>();
                    tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopLeft;
                }

                tilemap.orientation = Tilemap.Orientation.XY;
                tilemap.size = Vector3Int.left;
                tilemapContainer.transform.SetParent(prefabRoot.transform);

                //シャドウの場合はGridを1/4にする
                if (layer.type == MapDataModel.Layer.LayerType.Shadow)
                {
                    tilemap.gameObject.AddComponent<Grid>().cellSize = new Vector3(0.5f, 0.5f, 1);
                }

                // layerEntityの編集
                layer.tilemap = tilemap;
                layer.tilesOnPalette = new List<TileDataModelInfo>();
            }

            return layers;
        }

        //--------------------------------------------------------------------------------------------------------------
        // マップ操作
        //--------------------------------------------------------------------------------------------------------------
        public List<MapDataModel> LoadMaps() {
            return _mapRepository.LoadMapDataModels();
        }

        public List<MapBaseDataModel> LoadMapBase() {
            return _mapRepository.LoadMapBaseDataModels();
        }

        public List<MapDataModel> LoadMaps(bool reload) {
            return _mapRepository.LoadMapDataModels(reload);
        }

        public MapDataModel LoadMapById(string mapId, bool isSampleMap = false) {
            return _mapRepository.LoadMapDataModel(mapId, isSampleMap);
        }

        public void RemoveMap(MapDataModel mapDataModel) {
            _mapRepository.RemoveMapEntity(mapDataModel);
        }

        public void SaveMap(MapDataModel mapDataModel, MapRepository.SaveType saveType) {
            _mapRepository.SaveMapDataModelForEditor(mapDataModel, saveType);
        }

        public void ResetMap() {
            _mapRepository.ResetMapEntity();
            _tileRepository.ResetTileEntity();
            _tileGroupRepository.ResetTileGroupEntity();
            _tileImageRepository.ResetTileImageEntity();
        }

        public List<MapDataModel> LoadMapSamples() {
            return _mapRepository.LoadMapSampleDataModels();
        }

        public void SaveMapSample(MapDataModel mapDataModel) {
            _mapRepository.SaveMapSampleDataModelForEditor(mapDataModel);
        }

        // マップ名の重複チェック
        public string MapNameDuplicateCheck(string name) {
            string add = "";
            bool found = false;
            int count = 2;
            do
            {
                found = false;
                foreach (var data in _mapRepository.LoadMapDataModels())
                {
                    if (name + add == data.name)
                    {
                        add = " (" + count + ")";
                        count++;
                        found = true;
                    }
                }
            } while (found);
            name += add;
            return name;
        }

        //--------------------------------------------------------------------------------------------------------------
        // タイル操作
        //--------------------------------------------------------------------------------------------------------------
        public TileDataModel LoadTile(TileDataModelInfo tileDataModelInfo) {
            return _tileRepository.GetTile(tileDataModelInfo);
        }

        public void AddTile(TileDataModel tileDataModel) {
            _tileRepository.AddTile(tileDataModel);
        }

        public void RemoveTile(string id) {
            _tileRepository.DeleteTile(id);
        }

        public async Task<bool> SaveTile(TileDataModel tileDataModel) {
            return await _tileRepository.StoreTileEntity(tileDataModel);
        }

        public async Task<List<bool>> SaveTile(List<TileDataModel> tileDataModel) {
            return await _tileRepository.StoreTileEntity(tileDataModel);
        }

        public void SaveInspectorTile(TileDataModel tileDataModel) {
            _tileRepository.SaveInspectorTile(tileDataModel);
        }

        public Texture2D ReadImage(string path) {
            return TileImageRepository.ReadImageFromPath(path);
        }

        public List<TileDataModelInfo> LoadTileTable() {
            return _tileRepository.GetTileTable();
        }

        public string GetAssetPath(TileDataModelInfo tileDataModelInfo, bool folderOnly = false) {
            return TileRepository.GetAssetPath(tileDataModelInfo, folderOnly);
        }

        //--------------------------------------------------------------------------------------------------------------
        // タイルグループ操作
        //--------------------------------------------------------------------------------------------------------------
        public List<TileGroupDataModel> LoadTileGroups() {
            return _tileGroupRepository.GetTileGroupEntities();
        }

        public void RemoveTileGroup(TileGroupDataModel tileGroupDataModel) {
            _tileGroupRepository.RemoveTileGroupEntity(tileGroupDataModel);
        }

        public void SaveTileGroup(TileGroupDataModel tileGroupDataModel) {
            _tileGroupRepository.StoreTileGroupEntity(tileGroupDataModel);
        }

        public void ImportTileImageFile() {
            _tileImageRepository.ImportTileImageFile();
        }

        public void RemoveTileImageEntity(TileImageDataModel tileImageDataModel) {
            _tileImageRepository.RemoveTileImageEntity(tileImageDataModel);
        }
    }
}