using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map
{
    [Serializable]
    public class MapDataModel : WithSerialNumberDataModel
    {
        public enum ImageZoomIndex
        {
            Zoom1,
            Zoom2,
            Zoom4,
        }

        public const int MinWidth = 1;
        public const int MaxWidth = 256;
        public const int MinHeight = 1;
        public const int MaxHeight = 256;

        public enum MapScrollType
        {
            NoLoop,
            LoopVertical,
            LoopHorizontal,
            LoopBoth
        }

        private const string      PrefabDir  = "Assets/RPGMaker/Storage/Map/SavedMaps/";
        private const string      SamplePrefabDir = "Assets/RPGMaker/Storage/Map/SampleMaps/";

        public string         id;
        public int            index;
        public string         displayName;
        public int            width;
        public int            height;
        public string         memo;
        public string         name;
        public Background     background;
        public parallax       Parallax;

        public MapScrollType  scrollType;
        public bool           autoPlayBGM;
        public bool           autoPlayBgs;
        public string         bgmID;
        public SoundState     bgmState;
        public string         bgsID;
        public SoundState     bgsState;
        public bool           forbidDash;

        public List<MapLayer> layerJsons;

        private MapPrefabManager mapPrefabManagerForRuntime;
        private MapPrefabManager mapPrefabManagerForEditor;

        public bool changePrefab;
        public bool isSampleMap;

        public MapDataModel(
            string uuid,
            int index,
            string name,
            string displayName,
            int width,
            int height,
            MapScrollType scrollType,

            bool autoPlayBgm,
            string bgmId,
            SoundState bgmState,
            bool autoPlayBgs,
            string bgsId,
            SoundState bgsState,

            bool forbidDash,

            string memo,
            List<MapLayer> layerJsons,
            Background background,
            parallax parallax,

            [CanBeNull] GameObject mapPrefabForEditor = null
        ) {
            id = uuid;
            this.index = index;
            this.name = name;
            this.displayName = displayName;
            this.width = width;
            this.height = height;
            this.scrollType = scrollType;

            autoPlayBGM = autoPlayBgm;
            bgmID = bgmId;
            this.bgmState = bgmState;
            this.autoPlayBgs = autoPlayBgs;
            bgsID = bgsId;
            this.bgsState = bgsState;

            this.forbidDash = forbidDash;

            this.memo = memo;
            this.layerJsons = layerJsons;
            this.background = background;
            Parallax = parallax;

            MapPrefabManagerForRuntime = new MapPrefabManager(this);

#if UNITY_EDITOR
            //リスト管理
            if (MapPrefamManagerForEditorList == null)
            {
                MapPrefamManagerForEditorList = new Dictionary<string, MapPrefabManager>();
            }
            if (MapPrefamManagerForEditorList.ContainsKey(id) && mapPrefabForEditor != null)
            {
                //既に読み込み済みだった場合には、前に読み込んでいたPrefabを設定する
                MapPrefabManagerForEditor = MapPrefamManagerForEditorList[id];
            }
            else
            {
                if (MapPrefamManagerForEditorList.ContainsKey(id))
                {
                    //本来ここに来ることはあり得ないが、万が一来た場合には、元々管理していたデータを管理外にする
                    //MapPrefamManagerForEditorList[id].UnloadPrefabAndLayers();
                    MapPrefamManagerForEditorList.Remove(id);
                }
                //まだ登録が無い場合には、新規に読込、リスト管理する
                MapPrefabManagerForEditor = new MapPrefabManager(this, mapPrefabForEditor);
                MapPrefamManagerForEditorList.Add(id, mapPrefabManagerForEditor);
            }

            EditorApplication.playModeStateChanged -= ChangePlayMode;
            EditorApplication.playModeStateChanged += ChangePlayMode;

            EditorApplication.quitting -= Quit;
            EditorApplication.quitting += Quit;
#else
            MapPrefabManagerForEditor = new MapPrefabManager(this, mapPrefabForEditor);
#endif

            this.changePrefab = false;
        }

#if UNITY_EDITOR
        private static void ChangePlayMode(PlayModeStateChange state) {
            //Runtime実行中
            if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
            }
            //Editor実行中
            else if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
            }
            //切り替え中
            else
            {
                Quit();
            }
        }
#endif

        public MapPrefabManager MapPrefabManagerForRuntime
        {
            get
            {
                mapPrefabManagerForRuntime ??= new MapPrefabManager(this);
                return mapPrefabManagerForRuntime;
            }

            private set
            {
                mapPrefabManagerForRuntime = value;
            }
        }

#if UNITY_EDITOR
        //Prefabをリスト管理するもの
        //リスト管理しているが、基本的には1つのみとなる
        public static Dictionary<string, MapPrefabManager> MapPrefamManagerForEditorList;

        public static void Quit() {
            List<MapPrefabManager> list = new List<MapPrefabManager>();
            foreach (var data in MapPrefamManagerForEditorList.Values)
            {
                list.Add(data);
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].UnloadPrefabAndLayers();
            }
        }
#endif

        public MapPrefabManager MapPrefabManagerForEditor
        {
            get
            {
#if UNITY_EDITOR
                //リスト管理しているPrefabを返却する
                if (MapPrefamManagerForEditorList.ContainsKey(id))
                    return MapPrefamManagerForEditorList[id];

#endif
                //万が一無かったら新規作成する
                mapPrefabManagerForEditor ??= new MapPrefabManager(this);
#if UNITY_EDITOR
                MapPrefamManagerForEditorList.Add(id, mapPrefabManagerForEditor);
#endif
                return mapPrefabManagerForEditor;
            }

            private set
            {
                mapPrefabManagerForEditor = value;
            }
        }

        public List<Layer> LayersForRuntime => MapPrefabManagerForRuntime.layers;
        public List<Layer> LayersForEditor => MapPrefabManagerForEditor.layers;

        // データコピー
        public static MapDataModel CopyData(MapDataModel mapDataModel) {
            var clonedMapDataModel = JsonHelper.Clone(mapDataModel);
            clonedMapDataModel.MapPrefabManagerForRuntime = new MapPrefabManager(clonedMapDataModel);
            clonedMapDataModel.MapPrefabManagerForEditor = new MapPrefabManager(clonedMapDataModel);
            return clonedMapDataModel;
        }

        public static void CopyData(MapDataModel mapDataModelFrom, MapDataModel mapDataModelTo) {
            mapDataModelTo.id = mapDataModelFrom.id;
            mapDataModelTo.index = mapDataModelFrom.index;
            mapDataModelTo.displayName = mapDataModelFrom.displayName;
            mapDataModelTo.width = mapDataModelFrom.width;
            mapDataModelTo.height = mapDataModelFrom.height;
            mapDataModelTo.memo = mapDataModelFrom.memo;
            mapDataModelTo.name = mapDataModelFrom.name;
            mapDataModelTo.background.imageName = mapDataModelFrom.background.imageName;
            mapDataModelTo.background.imageZoomIndex = mapDataModelFrom.background.imageZoomIndex;
            mapDataModelTo.background.showInEditor = mapDataModelFrom.background.showInEditor;
            mapDataModelTo.Parallax.loopX = mapDataModelFrom.Parallax.loopX;
            mapDataModelTo.Parallax.loopY = mapDataModelFrom.Parallax.loopY;
            mapDataModelTo.Parallax.name = mapDataModelFrom.Parallax.name;
            mapDataModelTo.Parallax.show = mapDataModelFrom.Parallax.show;
            mapDataModelTo.Parallax.sx = mapDataModelFrom.Parallax.sx;
            mapDataModelTo.Parallax.sy = mapDataModelFrom.Parallax.sy;
            mapDataModelTo.Parallax.zoom0 = mapDataModelFrom.Parallax.zoom0;
            mapDataModelTo.Parallax.zoom2 = mapDataModelFrom.Parallax.zoom2;
            mapDataModelTo.Parallax.zoom4 = mapDataModelFrom.Parallax.zoom4;
            mapDataModelTo.scrollType = mapDataModelFrom.scrollType;
            mapDataModelTo.autoPlayBGM = mapDataModelFrom.autoPlayBGM;
            mapDataModelTo.autoPlayBgs = mapDataModelFrom.autoPlayBgs;
            mapDataModelTo.bgmID = mapDataModelFrom.bgmID;
            mapDataModelTo.bgmState.volume = mapDataModelFrom.bgmState.volume;
            mapDataModelTo.bgmState.pitch = mapDataModelFrom.bgmState.pitch;
            mapDataModelTo.bgmState.pan = mapDataModelFrom.bgmState.pan;
            mapDataModelTo.bgsID = mapDataModelFrom.bgsID;
            mapDataModelTo.bgsState.volume = mapDataModelFrom.bgsState.volume;
            mapDataModelTo.bgsState.pitch = mapDataModelFrom.bgsState.pitch;
            mapDataModelTo.bgsState.pan = mapDataModelFrom.bgsState.pan;
            mapDataModelTo.forbidDash = mapDataModelFrom.forbidDash;

            mapDataModelTo.layerJsons = new List<MapLayer>();
            for (int i = 0; i < mapDataModelFrom.layerJsons.Count; i++)
            {
                List<string> tileIdsOnPallete = new List<string>();
                for (int j = 0; j < mapDataModelFrom.layerJsons[i].tileIdsOnPalette.Count; j++)
                {
                    tileIdsOnPallete.Add(mapDataModelFrom.layerJsons[i].tileIdsOnPalette[j]);
                }
                MapLayer layer = new MapLayer(mapDataModelFrom.layerJsons[i].type, tileIdsOnPallete);
                mapDataModelTo.layerJsons.Add(layer);
            }

            if (mapDataModelTo.MapPrefabManagerForRuntime == null)
                mapDataModelTo.MapPrefabManagerForRuntime = new MapPrefabManager(mapDataModelTo);
            if (mapDataModelTo.mapPrefabManagerForEditor == null)
                mapDataModelTo.mapPrefabManagerForEditor = new MapPrefabManager(mapDataModelTo);

            mapDataModelTo.changePrefab = mapDataModelFrom.changePrefab;
            mapDataModelTo.isSampleMap = mapDataModelFrom.isSampleMap;
        }


        // マッププレハブコピー（指定のデータモデルに指定IDで複製）
        public static void CopyMapPrefabForEditor(MapDataModel mapDataModel, string id, bool isSourceSampleMap = false) {
            var prefab = mapDataModel.MapPrefabManagerForEditor.LoadPrefab(isSourceSampleMap);
            UnityEditorWrapper.PrefabUtilityWrapper.SaveAsPrefabAsset(prefab, PrefabDir + id + ".prefab");
            mapDataModel.MapPrefabManagerForEditor.DeletePrefab();
        }

        // JumpPreviewクラス、RoutePreviewクラス用のマッププレハブのインスタンス化。
        // これらのクラスは、MapDataModel内のマッププレハブを使用していないので、
        // MapDataModelとマッププレハブを別々に指定できるものを用意した。
        public static GameObject InstantiateMapPrefab(MapDataModel mapDataModel, GameObject mapPrefab)
        {
            return new MapPrefabManager(mapDataModel, Object.Instantiate(mapPrefab)).CorrectionMapPrefab();
        }

        // レイヤー種別からインデックスを取得
        public static int GetLayerIndexByType(Layer.LayerType type) {
            return type switch
            {
                Layer.LayerType.DistantView => 0,
                Layer.LayerType.Background => 1,
                Layer.LayerType.BackgroundCollision => 2,
                Layer.LayerType.A => 3,
                Layer.LayerType.A_Effect => 4,
                Layer.LayerType.B => 5,
                Layer.LayerType.B_Effect => 6,
                Layer.LayerType.Shadow => 7,
                Layer.LayerType.C => 8,
                Layer.LayerType.C_Effect => 9,
                Layer.LayerType.D => 10,
                Layer.LayerType.D_Effect => 11,
                Layer.LayerType.ForRoute => 12,
                Layer.LayerType.Region => 13,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static parallax CreateDefaultParallax() {
            return new parallax(
                false,
                false,
                "",
                false,
                0,
                0,
                true,
                false,
                false
            );
        }

#if UNITY_EDITOR
        // キャッシュを使わず直接マップPrefabを読み込む
        public GameObject EditorDirectLoadMapPrefab(bool isSampleMap = false) {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath(isSampleMap));
        }
#endif
        
        public string GetPrefabPath(bool isSampleMap = false) {
            return (!isSampleMap ? PrefabDir : SamplePrefabDir) + id + ".prefab";
        }

        public Layer GetLayerByTypeForRuntime(Layer.LayerType layerType)
        {
            return MapPrefabManagerForRuntime.GetLayerByType(layerType);
        }

        public Layer GetLayerByTypeForEditor(Layer.LayerType layerType)
        {
            return MapPrefabManagerForEditor.GetLayerByType(layerType);
        }

        public Transform GetLayerTransformForRuntime(Layer.LayerType layerType)
        {
            return MapPrefabManagerForRuntime.GetLayerTransform(layerType);
        }

        public Transform GetLayerTransformForEditor(Layer.LayerType layerType)
        {
            return MapPrefabManagerForEditor.GetLayerTransform(layerType);
        }

	    public bool isEqual(MapDataModel data)
	    {
            if (layerJsons.Count != data.layerJsons.Count)
                return false;

            for (int i = 0; i < layerJsons.Count; i++)
                if (!layerJsons[i].isEqual(data.layerJsons[i]))
                    return false;

	        return id == data.id &&
	               index == data.index &&
	               displayName == data.displayName &&
	               width == data.width &&
	               height == data.height &&
	               memo == data.memo &&
	               name == data.name &&
	               background.isEqual(data.background) &&
	               Parallax.isEqual(data.Parallax) &&
	               scrollType == data.scrollType &&
	               autoPlayBGM == data.autoPlayBGM &&
	               autoPlayBgs == data.autoPlayBgs &&
	               bgmID == data.bgmID &&
	               bgmState.isEqual(data.bgmState) &&
	               bgsID == data.bgsID &&
	               bgsState.isEqual(data.bgsState) &&
	               forbidDash == data.forbidDash &&
	               changePrefab == data.changePrefab &&
	               isSampleMap == data.isSampleMap;
	    }

        [Serializable]
        public class SoundState
        {
            public int pan;
            public int pitch;
            public int volume;

            public SoundState(
                int pan,
                int pitch,
                int volume
            ) {
                this.pan = pan;
                this.pitch = pitch;
                this.volume = volume;
            }

            public static SoundState CreateDefault() {
                return new SoundState(
                    0,
                    100,
                    90
                );
            }

		    public bool isEqual(SoundState state)
		    {
		        return pan == state.pan &&
		               pitch == state.pitch &&
		               volume == state.volume;
		    }
        }

        // local class / enum
        //-------------------------------------------------------------------------------------------------------
        [Serializable]
        public class Layer
        {
            public enum LayerType
            {
                DistantView,
                Background,
                BackgroundCollision,
                A,
                A_Effect,
                B,
                B_Effect,
                Shadow,
                C,
                C_Effect,
                D,
                D_Effect,
                ForRoute,
                Region,
            }

            public        Grid    grid;
            public        Sprite  spr;
            public        Tilemap tilemap;
            public        List<TileDataModelInfo> 
                                  tilesOnPalette;
            public List<TileDataModel> tiles => ToTileData();
            private List<TileDataModel> ToTileData() {
                var tile = new List<TileDataModel>();
                for (int i = 0; i < tilesOnPalette.Count; i++)
                {
                    tile.Add(tilesOnPalette[i].TileDataModel);
                }
                return tile;
            }

            public LayerType type;

            public Layer(LayerType type, Tilemap tilemap, List<TileDataModelInfo> tilesOnPalette) {
                this.type = type;
                this.tilemap = tilemap;
                this.tilesOnPalette = tilesOnPalette;
            }

            public Layer(
                LayerType type,
                Grid grid,
                Vector2 cellSize,
                Tilemap tilemap,
                List<TileDataModelInfo> tilesOnPalette
            ) {
                this.type = type;
                this.grid = grid;
                this.tilemap = tilemap;
                tilemap.transform.SetParent(grid.gameObject.transform);
                grid.gameObject.GetComponent<Grid>().cellSize = new Vector3(cellSize.x, cellSize.y, 1);
                this.tilesOnPalette = tilesOnPalette;
            }

            public Layer(LayerType type, Sprite spr) {
                this.type = type;
                this.spr = spr;
            }

            public TileDataModel GetTileDataModelByPosition(Vector2 pos) {
                return tilemap.GetTile<TileDataModel>(new Vector3Int((int) pos.x, (int) pos.y, 0));
            }

            /// <summary>
            /// Y座標補正ありのTileDataModel取得メソッド。
            /// </summary>
            /// <param name="pos">取得タイル位置 (yが0より大きい場合、符号を反転させる)</param>
            /// <returns>TileDataModel</returns>
            public TileDataModel GetTileDataModelWithYPosCorrection(Vector2Int pos) {
                if (pos.y > 0)
                {
                    pos.y = -pos.y;
                }

                return tilemap.GetTile<TileDataModel>(new Vector3Int(pos.x, pos.y , 0));
            }
        }

        [Serializable]
        public class Background
        {
            public string imageName;
            public ImageZoomIndex imageZoomIndex;
            public bool showInEditor = true;

            public static Background CreateDefault()
            {
                return new Background();
            }

            public Background()
            {
            }

            public Background(string imageName, ImageZoomIndex imageZoomIndex, bool showInEditor)
            {
                this.imageName = imageName;
                this.imageZoomIndex = imageZoomIndex;
                this.showInEditor = showInEditor;
            }

            public string ImageFilePath =>
                !string.IsNullOrEmpty(imageName) ?
                    System.IO.Path.ChangeExtension(PathManager.MAP_BACKGROUND + imageName, ".png") :
                    null;

		    public bool isEqual(Background background)
		    {
		        return imageName == background.imageName &&
		               imageZoomIndex == background.imageZoomIndex &&
		               showInEditor == background.showInEditor;
		    }
        }

        [Serializable]
        public class parallax
        {
            public bool   loopX;
            public bool   loopY;
            public string name;
            public bool   show;
            public int    sx;       // ループ (スクロール) 速度
            public int    sy;       // ループ (スクロール) 速度
            public bool   zoom0;
            public bool   zoom2;
            public bool   zoom4;

            public parallax(
                bool loopX,
                bool loopY,
                string name,
                bool show,
                int sx,
                int sy,
                bool zoom0,
                bool zoom2,
                bool zoom4
            ) {
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

            public string ImageFilePath =>
                !string.IsNullOrEmpty(name) ?
                    System.IO.Path.ChangeExtension(PathManager.MAP_PARALLAX + name, ".png") :
                    null;

            public int GetZoomScale()
            {
                var bools = new bool[] { zoom0, zoom2, zoom4 };
                if (bools.Count(b => b) != 1)
                {
                    // trueの個数が1以外！
                }

                int scaleIndex = Array.IndexOf(bools, true);
                return 1 << scaleIndex;
            }

		    public bool isEqual(parallax data)
		    {
		        return loopX == data.loopX &&
		               loopY == data.loopY &&
		               name == data.name &&
		               show == data.show &&
		               sx == data.sx &&
		               sy == data.sy &&
		               zoom0 == data.zoom0 &&
		               zoom2 == data.zoom2 &&
		               zoom4 == data.zoom4;
		    }
        }

        public class MapPrefabManager
        {
            private readonly MapDataModel mapDataModel;
            private GameObject _mapPrefab;
            private List<Layer> _mapLayers;

            public MapPrefabManager(MapDataModel mapDataModel, GameObject mapPrefab = null)
            {
                 this.mapDataModel = mapDataModel;
                _mapPrefab = mapPrefab;
            }

            // マッププレハブ。
            // 以前はLoadPrefab()が呼ばれていたが混乱の元なのでフィールド値を返すだけに変更し、
            // 以前これを参照していた所はLoadPrefab()に差し替えた。
            public GameObject mapPrefab => _mapPrefab;

            public List<Layer> layers => LoadLayers();

            public Transform GetLayerTransform(Layer.LayerType layerType)
            {
                // レイヤータイプと各レイヤーのゲームオブジェクトのインデックス値は1対1で対応している。
                int childIndex = (int)layerType;
                return LoadPrefab().transform.GetChild(childIndex);
            }

            //--------------------------------------------------------------------------------------------------------------
            // 重いファイルは必要な時にLoadするための処理
            //--------------------------------------------------------------------------------------------------------------
            // マップPrefabを読み込む (キャッシュの仕組み＆インスタンス化を含む)
            // HACK: このメソッドの呼び出しの多くは、適切な引数の値が指定されていません。
            //     サンプルマップの場合でも多くは引数が省略され、不正なディレクトリからファイルをロードしようとして
            //     失敗します。何らかの修正が必要です！
            //
            //     以下のメソッド内で暫定的に対応しています。
            //       UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents
            public GameObject LoadPrefab(bool isSampleMap = false)
            {
                if (_mapPrefab == null)
                {
                    mapDataModel.changePrefab = false;
                    var loadPrefab =
                        UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(
                            mapDataModel.GetPrefabPath(isSampleMap));
                    _mapPrefab = Object.Instantiate(
                        loadPrefab);
                    UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);

                    
                    CorrectionMapPrefab();
                }

                return _mapPrefab;
            }

            // マップPrefabを削除
            public void DeletePrefab()
            {
                if (_mapPrefab != null)
                {
                    Object.DestroyImmediate(_mapPrefab);
                    _mapPrefab = null;
                    _mapLayers = null;
                }
            }

            // レイヤー（タイル関連ファイル含む）を読み込む
            public List<Layer> LoadLayers()
            {
                if (_mapLayers != null)
                {
                    bool isNull = true;
                    for (int i = 0; i < _mapLayers.Count; i++)
                    {
                        if (_mapLayers[i].tilemap != null)
                        {
                            isNull = false;
                            break;
                        }
                    }
                    if (!isNull)
                        return _mapLayers;
                }

                var mapPrefab = LoadPrefab();
#if UNITY_EDITOR
                var tileEntities = new TileRepository().GetTileTable();
#endif
                _mapLayers = null;
                for (int index = 0; index < mapDataModel.layerJsons.Count; index++)
                {
                    Layer layer;
                    var layerType = (Layer.LayerType)System.Enum.Parse(typeof(Layer.LayerType), mapDataModel.layerJsons[index].type);

                    List<TileDataModelInfo> tilesOnPalette = new List<TileDataModelInfo>();
#if UNITY_EDITOR    // Runtime時はパレット情報不要
                    for (int i = 0; i < mapDataModel.layerJsons[index].tileIdsOnPalette.Count; i++)
                    {
                        TileDataModelInfo data = null;
                        for (int i2 = 0; i2 < tileEntities.Count; i2++)
                            if (tileEntities[i2].id == mapDataModel.layerJsons[index].tileIdsOnPalette[i])
                            {
                                data = tileEntities[i2];
                                break;
                            }
                        if (data != null)
                        {
                            tilesOnPalette.Add(data);
                            continue;
                        }

                        if (data != null)
                            tilesOnPalette.Add(data);
                    }
#endif

                    if (mapPrefab.transform.GetChild(index).GetComponent<SpriteRenderer>() != null)
                    {
                        var spr = mapPrefab.transform.GetChild(index).GetComponent<SpriteRenderer>().sprite;
                        layer = new Layer(layerType, spr);
                    }
                    else
                    {
                        var tilemap = mapPrefab.transform.GetChild(index).GetComponent<Tilemap>();
                        layer = new Layer(layerType, tilemap, tilesOnPalette);
                    }

                    if (_mapLayers == null)
                        _mapLayers = new List<Layer>();
                    _mapLayers.Add(layer);
                }
                return _mapLayers;
            }

            /// <summary>
            /// 新規作成またはロードしたマッププレハブの内容を、使用できる状態に補正する。
            /// </summary>
            /// <returns>補正済マッププレハブ</returns>
            /// <remarks>
            /// JumpPreviewクラス、RoutePreviewクラスはMapDataModel内のマッププレハブを使用していないので、
            /// これらのクラスからの呼び出し用にMapDataModelとマッププレハブを別々に指定できるメソッドを用意した。
            /// </remarks>
            public GameObject CorrectionMapPrefab()
            {
                foreach (Layer layer in layers)
                {
                    var layerTransform = GetLayerTransform(layer.type);

                    layerTransform.name = $"Layer {layer.type}";

                    Service.MapManagement.MapRenderingOrderManager.SetLayerRendererSortingLayer(
                        layerTransform.gameObject, layer.type);

                    if (layer.type == Layer.LayerType.Background ||
                        layer.type == Layer.LayerType.DistantView)
                    {
                        layerTransform.position =
                            new Vector3(0f, Service.MapManagement.MapManagementService.YPositionOffsetToMapTile, 0f);
                    }
                    else
                    {
                        layerTransform.position = Vector3.zero;
                    }

                    // エディターのマップシーンで使用するスプライトマスクの効力を無効に。
                    var spriteRenderer = layerTransform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                    }
                }

                if (_mapPrefab.transform.localScale.x - Mathf.Floor(_mapPrefab.transform.localScale.x) != 0)
                {
                    _mapPrefab.transform.localScale = new Vector3(1f, 1f, 1f);
                    _mapPrefab.GetComponent<Grid>().cellGap = new Vector3(0, 0, 0);
                }

                for (int i = 0; i < _mapPrefab.transform.childCount; i++)
                {
                    if (_mapPrefab.transform.GetChild(i).transform.localScale.x - Mathf.Floor(_mapPrefab.transform.GetChild(i).transform.localScale.x) != 0)
                    {
                        _mapPrefab.transform.GetChild(i).transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                }

                return mapPrefab;
            }

            // PrefabとLayerをアンロードする
            public void UnloadPrefabAndLayers()
            {
                if (_mapPrefab != null)
                {
#if UNITY_EDITOR
                    //Prefabが変更されている場合、ここで保存する
                    if (mapDataModel.changePrefab)
                    {
                        UnityEditorWrapper.PrefabUtilityWrapper.SaveAsPrefabAsset(_mapPrefab, mapDataModel.GetPrefabPath());
                    }
#endif
                    Object.DestroyImmediate(_mapPrefab);
                    _mapPrefab = null;
                }

#if UNITY_EDITOR
                //キャッシュを破棄
                MapRepository.RemoveCache(mapDataModel.id);
                //管理外とする
                MapDataModel.MapPrefamManagerForEditorList.Remove(mapDataModel.id);
#endif

                _mapLayers = null;
            }

            // 種別を指定してレイヤーを取得
            public Layer GetLayerByType(Layer.LayerType type)
            {
                var layers = LoadLayers();
                return layers[GetLayerIndexByType(type)];
            }
        }
    }

    public static partial class CSharpExtensionMethods
    {
        /// <summary>
        /// ImageZoomIndex列挙型の値に対応するズーム倍率を取得する。
        /// </summary>
        /// <param name="imageZoomIndex">ImageZoomIndex列挙型の値。</param>
        /// <returns>ズーム倍率。</returns>
        public static int GetZoomValue(this MapDataModel.ImageZoomIndex imageZoomIndex)
        {
            return 1 << (int)imageZoomIndex;
        }
    }
}