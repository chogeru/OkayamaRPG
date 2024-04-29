using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using RPGMaker.Codebase.Editor.MapEditor.Component.Inventory;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit
{
    /// <summary>
    /// マップ編集ウィンドウ.
    /// </summary>
    public class MapEditWindow : BaseWindow
    {
        // const
        private const string Uxml = "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/MapEdit/map_edit.uxml";
        private const string ThumbnailImagesDirectory = "Assets/RPGMaker/Storage/Map/ThumbnailImages";

        public static bool isEmphasis = false;

        private static readonly Dictionary<MapDataModel.Layer.LayerType, MapDataModel.Layer.LayerType>
            EffectPlacementModeChangeToLayerTypes =
                new Dictionary<MapDataModel.Layer.LayerType, MapDataModel.Layer.LayerType>
                {
                    {MapDataModel.Layer.LayerType.A, MapDataModel.Layer.LayerType.A_Effect},
                    {MapDataModel.Layer.LayerType.B, MapDataModel.Layer.LayerType.B_Effect},
                    {MapDataModel.Layer.LayerType.C, MapDataModel.Layer.LayerType.C_Effect},
                    {MapDataModel.Layer.LayerType.D, MapDataModel.Layer.LayerType.D_Effect},
                    {MapDataModel.Layer.LayerType.A_Effect, MapDataModel.Layer.LayerType.A},
                    {MapDataModel.Layer.LayerType.B_Effect, MapDataModel.Layer.LayerType.B},
                    {MapDataModel.Layer.LayerType.C_Effect, MapDataModel.Layer.LayerType.C},
                    {MapDataModel.Layer.LayerType.D_Effect, MapDataModel.Layer.LayerType.D},
                    {MapDataModel.Layer.LayerType.Background, MapDataModel.Layer.LayerType.A_Effect},
                };

        // 配置モードフラグ (真でエフェクトの配置モード、偽でタイル配置モード)
        private bool _isEffectPlacementMode = false;

        private MapDataModel.Layer.LayerType _changeBeforeLayerType = MapDataModel.Layer.LayerType.A;
        // データプロパティ
        private MapDataModel        _mapDataModel;
        private List<TileDataModelInfo> _tileEntities;
        private List<string>        _backGroundName = new List<string>();

        // UI要素プロパティ
        private        MapEditCanvas        _mapEditCanvas;
        private        MapPreviewCanvas     _mapPreviewCanvas;
        private        BackgroundViewCanvas _backgroundView;
        private        DistantViewCanvas    _distantView;

        private static Label                _pointLabel;
        private        LayerInventory       _layerInventory;
        private        TileInventory        _tileInventory;
        private        ScrollView           _images;
        private        VisualElement        _layerInventoryContainer;
        private        VisualElement        _layerInventoryMainContainer;
        private        VisualElement        _layerInventoryControllerContainer;
        private        VisualElement        _tileInventoryContainer;
        private        VisualElement        _tileInventoryMainContainer;
        private        VisualElement        _tileInventoryControllerContainer;

        private bool _canRightClick = false;

        /**
         * 初期化
         */
        public void Init(
            MapDataModel mapDataModel,
            List<TileDataModelInfo> tileEntities
        ) {
            _mapDataModel = Hierarchy.Hierarchy.mapManagementService.LoadMapById(mapDataModel.id) ?? mapDataModel;
            _tileEntities = tileEntities;
            _distantView = new DistantViewCanvas();
            _backgroundView = new BackgroundViewCanvas();
            InitUI(MapDataModel.Layer.LayerType.A);
            Refresh();

            MapEditor.SetMapEntityToInspector(_mapDataModel);
        }

        /**
         * マッププレビュー用
         */
        public void InitPreview(
            MapDataModel mapDataModel
        ) {
            _mapDataModel = Hierarchy.Hierarchy.mapManagementService.LoadMapById(mapDataModel.id, true) ?? mapDataModel;

            VisualElement uxmlElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree();
            StyleSheet styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayout);
            StyleSheet styleMapEdit = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkMapEdit);
            StyleSheet styleLayerInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayerInventory);
            StyleSheet styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkTileInventory);
            if (!EditorGUIUtility.isProSkin)
            {
                styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayout);
                styleMapEdit = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightMapEdit);
                styleLayerInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayerInventory);
                styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightTileInventory);
            }
            uxmlElement.styleSheets.Add(styleLayout);
            uxmlElement.styleSheets.Add(styleMapEdit);
            uxmlElement.styleSheets.Add(styleLayerInventory);
            uxmlElement.styleSheets.Add(styleTileInventory);
            EditorLocalize.LocalizeElements(uxmlElement);

            // マップ
            //----------------------------------------------------------------------
            _mapPreviewCanvas?.Dispose();
            _mapPreviewCanvas = new MapPreviewCanvas(_mapDataModel, Repaint);

            VisualElement mapCanvasContainer = uxmlElement.Query<VisualElement>("map_canvas_container");
            mapCanvasContainer.Add(_mapPreviewCanvas);

            // リージョンレイヤーを非表示に。
            if (_mapDataModel.LayersForEditor != null && _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region] != null &&
                _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region].tilemap != null)
                _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region].tilemap.gameObject.SetActive(false);

            // 背景と遠景の表示の有無とスケールを更新。
            MapCanvas.UpdateBackgroundDisplayedAndScaleForEditor(_mapDataModel);
            MapCanvas.UpdateDistantViewDisplayedAndScaleForEditor(_mapDataModel);

            rootVisualElement.Clear();
            rootVisualElement.Add(uxmlElement);

            // mapCanvasのレイアウトが完了したタイミングでサイズが取れるようになるので、そのタイミングで初回Renderするように
            _mapPreviewCanvas.RegisterCallback<GeometryChangedEvent>(v => { _mapPreviewCanvas.Refresh(); });
            uxmlElement.Q<VisualElement>("control_panel_container").style.display = DisplayStyle.None;

            _mapPreviewCanvas?.Refresh(_mapDataModel);
            MapEditor.SetMapEntityToInspector(_mapDataModel, true);
        }

        /// <summary>
        /// マップのサムネイル画像を表示されているVisualElementから取得し、所定の場所にpngファイルとしてセーブする。
        /// </summary>
        public void CaptureAndSaveMapThumbnail() {
            _mapEditCanvas.SetLineGridActive(false);
            _mapEditCanvas.Refresh();
            Texture2D texture2d = ImageUtility.ToTexture2D(
                rootVisualElement.Q<VisualElement>("map_canvas_container").Q<Image>().image, this);
            _mapEditCanvas.SetLineGridActive(true);
            _mapEditCanvas.Refresh();

            string filePath = GetThumbnailImageFilePath(_mapDataModel.id).Replace('\\', '/');
            ImageUtility.SaveAndDestroyTexture(filePath, texture2d);

            AddressableManager.Path.SetAddressToAsset(filePath);

            OutlineEditor.OutlineEditor.UpdateNodesView(_mapDataModel.id);
        }

        /// <summary>
        /// カメラから表示中のマップの全体を原寸でキャプチャする。
        /// </summary>
        /// <returns>キャプチャしたテクスチャ。</returns>
        public Texture2D CaptureFullSizeMapFromCamera() {
            return _mapEditCanvas.CaptureFullSizeMapFromCamera();
        }

        public static string GetThumbnailImageFilePathThatExist(string mapId) {
            string filePath = GetThumbnailImageFilePath(mapId);
            if (filePath != null && !File.Exists(filePath))
            {
                filePath = null;
            }

            return filePath;
        }

        public static string GetThumbnailImageFilePath(string mapId) {
            return !string.IsNullOrEmpty(mapId) ?
                Path.Combine(
                    ThumbnailImagesDirectory,
                    Path.ChangeExtension(mapId, ".png")) :
                null;
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh(
            [CanBeNull] MapDataModel mapDataModel = null,
            [CanBeNull] List<TileDataModelInfo> tileEntities = null
        ) {
            if (mapDataModel != null) _mapDataModel = mapDataModel;
            if (tileEntities != null) _tileEntities = tileEntities;

            _mapEditCanvas?.Refresh(_mapDataModel);
            if (_tileInventory != null)
            {
                _tileInventory.Refresh(_tileEntities);
            }
        }

        void OnGUI() {
            //Windowから高さを取得して、タイル関連の高さを引く
            if (_mapEditCanvas != null)
            {
                var windowHeight = position.size.y - 300f;
                _mapEditCanvas.style.height = windowHeight;
            }
        }

        /**
         * UI初期化
         */
        private void InitUI(MapDataModel.Layer.LayerType startLayerType = 0, bool isSampleMap = false) {
            VisualElement uxmlElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree();
            StyleSheet styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayout);
            StyleSheet styleMapEdit = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkMapEdit);
            StyleSheet styleLayerInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayerInventory);
            StyleSheet styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkTileInventory);
            if (!EditorGUIUtility.isProSkin)
            {
                styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayout);
                styleMapEdit = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightMapEdit);
                styleLayerInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayerInventory);
                styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightTileInventory);
            }
            uxmlElement.styleSheets.Add(styleLayout);
            uxmlElement.styleSheets.Add(styleMapEdit);
            uxmlElement.styleSheets.Add(styleLayerInventory);
            uxmlElement.styleSheets.Add(styleTileInventory);
            
            
            EditorLocalize.LocalizeElements(uxmlElement);

            // マップ
            //----------------------------------------------------------------------
            _mapEditCanvas?.Dispose();
            _mapEditCanvas = new MapEditCanvas(_mapDataModel, Repaint, isSampleMap);

            VisualElement mapCanvasContainer = uxmlElement.Query<VisualElement>("map_canvas_container");
            mapCanvasContainer.Add(_mapEditCanvas);
            
            // 選択座標の表示
            //----------------------------------------------------------------------
            VisualElement pointLabelContainer = uxmlElement.Query<VisualElement>("point_label_container");
            _pointLabel = new Label();
            pointLabelContainer.Add(_pointLabel);
            PointDisplay(0, 0);

            // レイヤー制御部
            //----------------------------------------------------------------------
            _layerInventoryContainer = uxmlElement.Query<VisualElement>("layer_inventory_container");
            _layerInventoryMainContainer = uxmlElement.Query<VisualElement>("layer_inventory_main_container");
            _layerInventoryControllerContainer =
                uxmlElement.Query<VisualElement>("layer_inventory_controller_container");

            // タイルパレット更新
            TilesOnPaletteRefresh();

            _layerInventory = new LayerInventory(
                _mapDataModel.LayersForEditor,
                _mapEditCanvas.ChangeTileToDraw,
                (tileDataModel) =>
                {
                    // 背景コリジョンは追加しない
                    if (tileDataModel.type != TileDataModel.Type.BackgroundCollision &&
                        _layerInventory.CurrentLayer.type != MapDataModel.Layer.LayerType.Shadow)
                    {
                        // コンテキストメニュー
                        var menu = new GenericMenu();

                        // タイルの追加。
                        menu.AddItem(
                            new GUIContent(EditorLocalize.LocalizeText("WORD_1606")),
                            false,
                            ShowTileInventoryView);

                        // タイルの削除。
                        menu.AddItem(
                            new GUIContent(EditorLocalize.LocalizeText("WORD_0786")),
                            false,
                            () => { RemoveCurrentSelectingTileFromCurrentLayer(tileDataModel); });

                        menu.ShowAsContext();
                    }
                },
                ChangeLayer,
                _isEffectPlacementMode,
                startLayerType
            );
            _layerInventoryMainContainer.Add(_layerInventory);

            // タイルがない箇所も右クリックでコンテキストメニューからタイルの追加を行えるように
            BaseClickHandler.ClickEvent(_layerInventoryMainContainer, mouseUpEvent =>
            {
                if (mouseUpEvent != (int) MouseButton.RightMouse)
                    return;
                if (!_canRightClick)
                    return;
                // コンテキストメニュー
                var menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_1606")),
                    false,
                    ShowTileInventoryView);
                menu.ShowAsContext();
            });

            // タイル制御部
            //----------------------------------------------------------------------
            _tileInventoryContainer = uxmlElement.Query<VisualElement>("tile_inventory_container");
            _tileInventoryMainContainer = uxmlElement.Query<VisualElement>("tile_inventory_main_container");
            _tileInventoryControllerContainer = uxmlElement.Query<VisualElement>("tile_inventory_controller_container");

            _tileInventory = new TileInventory(
                TileInventory.TileInventoryType.MapEdit,
                _tileEntities,
                null,
                null
            );
            _tileInventoryMainContainer.Add(_tileInventory);

            // タイルをレイヤーに追加ボタン
            var btnAddTileToLayer = new Button()
                {text = EditorLocalize.LocalizeText("WORD_0792"), name = "tileAddButton"};
            btnAddTileToLayer.clicked += AddCurrentSelectingTileToCurrentLayer;
            _tileInventoryControllerContainer.Add(btnAddTileToLayer);

            // 戻るボタン
            var btnBackToLayerInventory = new Button() {text = EditorLocalize.LocalizeText("WORD_1607")};
            btnBackToLayerInventory.clicked += ShowLayerInventoryView;
            _tileInventoryControllerContainer.Add(btnBackToLayerInventory);

            ShowLayerInventoryView();

            // リージョンレイヤーを非表示に。
            if(_mapDataModel.LayersForEditor != null && _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region] != null &&
                _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region].tilemap != null)
            _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region].tilemap.gameObject.SetActive(false);
            
            // 背景と遠景の表示の有無とスケールを更新。
            MapCanvas.UpdateBackgroundDisplayedAndScaleForEditor(_mapDataModel);
            MapCanvas.UpdateDistantViewDisplayedAndScaleForEditor(_mapDataModel);

            rootVisualElement.Clear();
            rootVisualElement.Add(uxmlElement);

            // mapCanvasのレイアウトが完了したタイミングでサイズが取れるようになるので、そのタイミングで初回Renderするように
            _mapEditCanvas.RegisterCallback<GeometryChangedEvent>(v => { _mapEditCanvas.Refresh(); });

            if (isSampleMap) 
                uxmlElement.Q<VisualElement>("control_panel_container").style.display = DisplayStyle.None;

            // スポイト機能のため、全てのレイヤー情報を渡しておく
            _mapEditCanvas.SetMapLayer(_mapDataModel.LayersForEditor);
            
            // レイヤーの表示を更新する（_tileInventoryを使用する為作成後に呼び出す）
            ChangeLayer(_mapDataModel.GetLayerByTypeForEditor(startLayerType));

        }

        /**
         * レイヤーインベントリを表示する
         */
        private void ShowLayerInventoryView() {
            _layerInventoryContainer.style.display = DisplayStyle.Flex;
            _tileInventoryContainer.style.display = DisplayStyle.None;
        }

        /**
         * タイルインベントリを表示する
         */
        private void ShowTileInventoryView() {
            _layerInventoryContainer.style.display = DisplayStyle.None;
            _tileInventoryContainer.style.display = DisplayStyle.Flex;
        }

        /**
         * 現在選択中のタイルをレイヤーに追加
         */
        private void AddCurrentSelectingTileToCurrentLayer() {
            var targetTile = _tileInventory.CurrentSelectingTile;
            if (targetTile == null)
                return;

            _layerInventory.AddTileToCurrentLayer(targetTile, _tileInventory.TileEntities);
            _layerInventory.Refresh();
            _mapDataModel.LayersForEditor[(int) _layerInventory.CurrentLayer.type].tilesOnPalette = _layerInventory.CurrentLayer.tilesOnPalette;

            MapEditor.SaveMap(_mapDataModel, MapRepository.SaveType.NO_PREFAB);
            _mapEditCanvas.Refresh(_mapDataModel);

            ShowLayerInventoryView();
        }

        /**
         * レイヤーから選択中のタイルを削除
         */
        private void RemoveCurrentSelectingTileFromCurrentLayer(TileDataModel tileDataModel) {
            _layerInventory.RemoveTileFromCurrentLayer(tileDataModel);
            _layerInventory.Refresh();
            _mapDataModel.LayersForEditor[(int) _layerInventory.CurrentLayer.type].tilesOnPalette = _layerInventory.CurrentLayer.tilesOnPalette;

            MapEditor.SaveMap(_mapDataModel, MapRepository.SaveType.NO_PREFAB);
            _mapEditCanvas.Refresh(_mapDataModel);
        }

        /**
         * 選択中のレイヤーにあるタイル群を新しいタイルグループとして保存
         */
        private void SaveCurrentLayerAsTileGroup() {
            TileGroupSelectModalWindow backgroundSelectModalWindow = new TileGroupSelectModalWindow();
            backgroundSelectModalWindow.ShowSaveWindow("Select TileGroup", data => { },
                _layerInventory.CurrentLayer.tilesOnPalette);
        }

        /**
         * 選択中のレイヤーにタイルグループを読み込み
         */
        private void LoadTileGroupToCurrentLayer() {
            TileGroupSelectModalWindow backgroundSelectModalWindow = new TileGroupSelectModalWindow();
            backgroundSelectModalWindow.ShowLoadWindow(EditorLocalize.LocalizeWindowTitle("Select TileGroup"), data =>
            {
                _layerInventory.CurrentLayer.tilesOnPalette = data as List<TileDataModelInfo>;
                _layerInventory.Refresh();
                _mapDataModel.LayersForEditor[(int) _layerInventory.CurrentLayer.type].tilesOnPalette = _layerInventory.CurrentLayer.tilesOnPalette;

                MapEditor.SaveMap(_mapDataModel, MapRepository.SaveType.NO_PREFAB);
                _mapEditCanvas.Refresh(_mapDataModel);
            });
        }

        /**
         * エフェクトの配置モード切り替え
         */
        public void ChangeEffectPlacementMode() {
            _isEffectPlacementMode = !_isEffectPlacementMode;
            //背景だった場合の処理
            if (_changeBeforeLayerType == MapDataModel.Layer.LayerType.Background)
            {
                InitUI(_changeBeforeLayerType);
                _changeBeforeLayerType = MapDataModel.Layer.LayerType.A;
                return;
            }
            _changeBeforeLayerType = _layerInventory.CurrentLayer.type;
            InitUI(EffectPlacementModeChangeToLayerTypes[_layerInventory.CurrentLayer.type]);
        }

        /**
         * 選択中のレイヤーの表示・非表示を切り替える
         */
        private void ToggleCurrentLayerVisibility(bool value) {
            _layerInventory.ToggleCurrentLayerVisibility(value);

            // 「このレイヤーを表示する」トグルの状態を変更してもすぐにマップ表示に反映されないので追加。
            _mapEditCanvas.Refresh();
            MapEditor.SaveMap(_mapDataModel, MapRepository.SaveType.SAVE_PREFAB_FORCE);
        }

        /// <summary>
        /// 強調表示
        /// </summary>
        /// <param name="value"></param>
        private void ToggleCurrentLayerEmphasis(bool value) {
            isEmphasis = value;
            _layerInventory.ToggleCurrentLayerEmphasis(value, _mapDataModel);
            _mapEditCanvas.Refresh();
        }

        /// <summary>
        /// マップをセーブするときに強調表示を戻す
        /// </summary>
        public void SaveCurrentLayerEmphasis() {
            if (_layerInventory == null)
            {
                return;
            }

            _layerInventory.ToggleCurrentLayerEmphasis(false, _mapDataModel);
            _mapEditCanvas.Refresh();
        }

        /// <summary>
        /// マップをセーブ完了時に、強調表示の状態戻す
        /// </summary>
        public void UpdateCurrentLayerEmphasis() {
            if (_layerInventory == null)
            {
                return;
            }

            _layerInventory.ToggleCurrentLayerEmphasis(isEmphasis, _mapDataModel);
            _mapEditCanvas.Refresh();
        }
        
        public void ChangeDrawMode(MapEditCanvas.DrawMode drawMode) {
            _mapEditCanvas?.ChangeDrawMode(drawMode);
        }

        public TileDataModel GetTileToDraw() {
            return _mapEditCanvas?.GetTileToDraw();
        }

        private void ChangeLayer(MapDataModel.Layer mapLayer) {
            _mapEditCanvas.ChangeTargetLayer(mapLayer);

            // レイヤーインベントリがなければ処理しない
            if (_layerInventory == null)
                return;

            _layerInventoryControllerContainer.Clear();

            // 画像リストを削除 
            try
            {
                _layerInventory.Remove(_layerInventory.Q<ScrollView>("images"));
            }
            catch
            {
            }

            ;
            _images = null;

            switch (mapLayer.type)
            {
                //遠景切り替え
                case MapDataModel.Layer.LayerType.DistantView:
                    _layerInventoryControllerContainer.Add(_distantView.CreateButtonMenu(_mapDataModel, _mapEditCanvas,
                        _layerInventory, _images, new VisualElement()));
                    ChangeTileDisplay(false);
                    _canRightClick = false;
                    break;
                case MapDataModel.Layer.LayerType.BackgroundCollision:
                    _layerInventoryControllerContainer.Add(BackGroundCollisionButtonMenu());
                    _canRightClick = false;
                    break;
                case MapDataModel.Layer.LayerType.Background:
                    _layerInventoryControllerContainer.Add(_backgroundView.CreateButtonMenu(this,_mapDataModel,
                        _mapEditCanvas,
                        _layerInventory, _images, new VisualElement()));
                    ChangeTileDisplay(false);
                    _canRightClick = false;
                    break;
                case MapDataModel.Layer.LayerType.A:
                case MapDataModel.Layer.LayerType.B:
                case MapDataModel.Layer.LayerType.C:
                case MapDataModel.Layer.LayerType.D:
                case MapDataModel.Layer.LayerType.A_Effect:
                case MapDataModel.Layer.LayerType.B_Effect:
                case MapDataModel.Layer.LayerType.C_Effect:
                case MapDataModel.Layer.LayerType.D_Effect:
                case MapDataModel.Layer.LayerType.ForRoute:
                    _layerInventoryControllerContainer.Add(TileButtonMenu());
                    _canRightClick = mapLayer.type != MapDataModel.Layer.LayerType.ForRoute;
                    break;
                case MapDataModel.Layer.LayerType.Shadow:
                    _layerInventoryControllerContainer.Add(ShadowButtonMenu());
                    _canRightClick = false;
                    break;
            }

            // 「このレイヤーを表示する」トグルの値を変更後のカレントレイヤーの状態に設定。
            if (mapLayer.tilemap != null)
            {
                var tggleShowLayer = _layerInventoryControllerContainer.Q<Toggle>("toggleShowLayer");
                if (tggleShowLayer != null)
                {
                    if (mapLayer.tilemap.gameObject.activeSelf == false)
                    {
                        mapLayer.tilemap.gameObject.SetActive(true);
                        mapLayer.tilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
                    }               
                    tggleShowLayer.value = mapLayer.tilemap.gameObject.GetComponent<TilemapRenderer>().enabled;
                }
            }
        }

        private VisualElement TileButtonMenu() {
            var ret = new VisualElement();
            var toggleShowLayer = new Toggle()
                {text = EditorLocalize.LocalizeText("WORD_0787"), name = "toggleShowLayer"};
            toggleShowLayer.RegisterValueChangedCallback(evt => { ToggleCurrentLayerVisibility(evt.newValue); });
            ret.Add(toggleShowLayer);

            //強調表示
            var toggleEmphasisShowLayer = new Toggle()
                {text = EditorLocalize.LocalizeText("WORD_3062"), name = "toggleShowLayer"};
            toggleEmphasisShowLayer.RegisterValueChangedCallback(evt =>
            {
                ToggleCurrentLayerEmphasis(evt.newValue);
            });
            ret.Add(toggleEmphasisShowLayer);
            toggleEmphasisShowLayer.value = isEmphasis;
            ToggleCurrentLayerEmphasis(isEmphasis);

            ScrollView scrollView = new ScrollView();
            scrollView.style.height = Length.Percent(100);

            // 『タイルの追加』ボタン。
            scrollView.Add(new Button(ShowTileInventoryView) { text = EditorLocalize.LocalizeText("WORD_1606") });

            var btnSaveTileGroup = new Button() {text = EditorLocalize.LocalizeText("WORD_0788")};
            btnSaveTileGroup.clicked += SaveCurrentLayerAsTileGroup;
            scrollView.Add(btnSaveTileGroup);

            var btnLoadTileGroup = new Button() {text = EditorLocalize.LocalizeText("WORD_0789")};
            btnLoadTileGroup.clicked += LoadTileGroupToCurrentLayer;
            scrollView.Add(btnLoadTileGroup);

            var btnSwitchToEffect = new Button()
            {
                text = EditorLocalize.LocalizeText(_isEffectPlacementMode ? "WORD_1557" : "WORD_0790"),
                name = "btnSwitchToEffect"
            };

            btnSwitchToEffect.clicked += () => { ChangeEffectPlacementMode(); };
            scrollView.Add(btnSwitchToEffect);

            ret.Add(scrollView);

            if (_images != null)
            {
                _layerInventory.Remove(_images);
                _images = null;
            }

            ChangeTileDisplay(true);
            _layerInventory.SetTileListViewActive(true);
            MapEditor.SetMapEntityToInspector(_mapDataModel);

            return ret;
        }

        private VisualElement BackGroundCollisionButtonMenu() {
            var ret = new VisualElement();
            var toggleShowLayer = new Toggle()
                {text = EditorLocalize.LocalizeText("WORD_0787"), name = "toggleShowLayer"};
            toggleShowLayer.RegisterValueChangedCallback(evt => { ToggleCurrentLayerVisibility(evt.newValue); });
            ret.Add(toggleShowLayer);
            //強調表示
            var toggleEmphasisShowLayer = new Toggle()
                {text = EditorLocalize.LocalizeText("WORD_3062"), name = "toggleShowLayer"};
            toggleEmphasisShowLayer.RegisterValueChangedCallback(evt => { ToggleCurrentLayerEmphasis(evt.newValue); });
            ret.Add(toggleEmphasisShowLayer);
            toggleEmphasisShowLayer.value = isEmphasis;
            ToggleCurrentLayerEmphasis(isEmphasis);

            if (_images != null)
            {
                _layerInventory.Remove(_images);
                _images = null;
            }

            ChangeTileDisplay(false);
            _layerInventory.SetTileListViewActive(true);
            MapEditor.SetBackgroundCollisionViewToInspector(_mapDataModel
                .LayersForEditor[(int) MapDataModel.Layer.LayerType.BackgroundCollision].tilesOnPalette[0].TileDataModel);

            return ret;
        }

        // 影用のメニュー
        private VisualElement ShadowButtonMenu() {
            var ret = new VisualElement();
            var toggleShowLayer = new Toggle()
            {
                text = EditorLocalize.LocalizeText("WORD_0787"), name = "toggleShowLayer"
            };
            toggleShowLayer.RegisterValueChangedCallback(evt => { ToggleCurrentLayerVisibility(evt.newValue); });
            ret.Add(toggleShowLayer);
            //強調表示
            var toggleEmphasisShowLayer = new Toggle()
                {text = EditorLocalize.LocalizeText("WORD_3062"), name = "toggleShowLayer"};
            toggleEmphasisShowLayer.RegisterValueChangedCallback(evt => { ToggleCurrentLayerEmphasis(evt.newValue); });
            ret.Add(toggleEmphasisShowLayer);
            toggleEmphasisShowLayer.value = isEmphasis;
            ToggleCurrentLayerEmphasis(isEmphasis);

            if (_images != null)
            {
                _layerInventory.Remove(_images);
                _images = null;
            }

            ChangeTileDisplay(false);
            _layerInventory.SetTileListViewActive(true);
            MapEditor.SetMapEntityToInspector(_mapDataModel);

            return ret;
        }

        // タイルの表示切替
        private void ChangeTileDisplay(bool display) {
            if (display == true)
            {
                if (_tileInventory != null)
                    _tileInventory.style.display = DisplayStyle.Flex;
                if (_layerInventoryControllerContainer.Q<Button>("tileAddButton") != null)
                    _layerInventoryControllerContainer.Q<Button>("tileAddButton").style.display = DisplayStyle.Flex;
            }
            else
            {
                if (_tileInventory != null)
                    _tileInventory.style.display = DisplayStyle.None;
                if (_layerInventoryControllerContainer.Q<Button>("tileAddButton") != null)
                    _layerInventoryControllerContainer.Q<Button>("tileAddButton").style.display = DisplayStyle.None;
            }
        }

        /**
         * 背景の更新
         */
        public void UpdateBackgroundView(MapDataModel mapDataModel, Sprite spr, Texture2D tex2d) {
            _mapEditCanvas.UpdateBackgroundLayer(mapDataModel, spr);
            _backgroundView.UpdateImagePreview(tex2d);
        }

        /**
         * 遠景の更新
         */
        public void UpdateDistantView(MapDataModel mapDataModel, Sprite spr, Texture2D tex2d) {
            _mapEditCanvas.UpdateMapDistantViewLayer(mapDataModel, spr);
            _distantView.UpdateImagePreview(tex2d);
        }

        // マップサイズ変更
        public void ChangeMapSize(int width, int height) {
            var beforeCurrentLayerType = _layerInventory.CurrentLayer.type;
                
            foreach(var layer in _mapDataModel.LayersForEditor)
            {
                _mapEditCanvas.ChangeTargetLayer(layer);
                _mapEditCanvas.ChangeMapSizeForEditor(width, height);
            }

            _mapEditCanvas.DeleteEvent(width, height);

            _mapDataModel.width = width;
            _mapDataModel.height = height;

            MapEditor.SaveMap(_mapDataModel, MapRepository.SaveType.SAVE_PREFAB_FORCE);
            UnityEditorWrapper.AssetDatabaseWrapper.SaveAssets();

            // データ更新
            InitUI(beforeCurrentLayerType);
        }

        //選択座標の更新
        public static void PointDisplay(int x, int y) {
            if (_pointLabel != null)
            {
                if (y < 0)
                {
                    y = y * -1;
                }
                _pointLabel.text = EditorLocalize.LocalizeText("WORD_0983") + ":　　　　" + x + "," + y;

                _pointLabel.style.color = Color.white;
                if (!EditorGUIUtility.isProSkin)
                {
                    _pointLabel.style.color = Color.black;
                }
                
            }
        }

        // タイルパレット更新
        private void TilesOnPaletteRefresh() {
            // 背景コリジョンのタイルデータがなければ追加する
            if (_mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.BackgroundCollision].tilesOnPalette.Count < 1)
                _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.BackgroundCollision].tilesOnPalette =
                    _tileEntities.Where(e => e.type == TileDataModel.Type.BackgroundCollision)
                        .ToList();
        }

        private void OnDestroy() {
            _mapEditCanvas?.Dispose();
        }
    }
}