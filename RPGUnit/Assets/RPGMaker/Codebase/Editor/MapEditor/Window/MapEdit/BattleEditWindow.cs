using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using RPGMaker.Codebase.Editor.MapEditor.Component.GridView;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit
{
    /// <summary>
    ///     バトル編集ウィンドウ.
    /// </summary>
    public class BattleEditWindow : BaseWindow
    {
        private const  string Uxml = "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/MapEdit/battle_edit.uxml";
        private static Label  _pointLabel;

        // データプロパティ
        private MapDataModel _mapDataModel;

        // UI要素プロパティ
        private MapEditCanvas       _mapEditCanvas;
        private List<TileDataModelInfo> _tileEntities;

        /**
         * 初期化
         */
        public void Init(
            MapDataModel mapDataModel,
            List<TileDataModelInfo> tileEntities
        ) {
            _mapDataModel = mapDataModel;
            _tileEntities = tileEntities;
            InitUI();

            Refresh();
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

            _mapEditCanvas.Refresh(_mapDataModel);
        }

        private void OnGUI() {
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
        private void InitUI() {
            VisualElement uxmlElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree();
            StyleSheet styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayout);
            StyleSheet styleMapEdit = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkMapEdit);
            StyleSheet styleLayerInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayerInventory);
            if (!EditorGUIUtility.isProSkin)
            {
                styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayout);
                styleMapEdit = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightMapEdit);
                styleLayerInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayerInventory);
            }
            uxmlElement.styleSheets.Add(styleLayout);
            uxmlElement.styleSheets.Add(styleMapEdit);
            uxmlElement.styleSheets.Add(styleLayerInventory);
            EditorLocalize.LocalizeElements(uxmlElement);

            // マップ
            //----------------------------------------------------------------------
            _mapEditCanvas?.Dispose();
            _mapEditCanvas = new MapEditCanvas(_mapDataModel, Repaint);

            VisualElement mapCanvasContainer = uxmlElement.Query<VisualElement>("map_canvas_container");
            mapCanvasContainer.Add(_mapEditCanvas);
            
            // リージョンレイヤーを表示し、ターゲットレイヤーに設定。
            var targetLayer = _mapDataModel.LayersForEditor[(int) MapDataModel.Layer.LayerType.Region];
            targetLayer.tilemap.gameObject.SetActive(true);
            _mapEditCanvas.ChangeTargetLayer(targetLayer);

            

            // 選択座標の表示
            //----------------------------------------------------------------------
            VisualElement pointLabelContainer = uxmlElement.Query<VisualElement>("point_label_container");
            _pointLabel = new Label();
            pointLabelContainer.Add(_pointLabel);
            PointDisplay(0, 0);

            // リージョンレイヤータイル一覧の表示
            //----------------------------------------------------------------------
            VisualElement layerPanelContainer = uxmlElement.Query<VisualElement>("layer_inventory_main_container");
            var tileDataModels = _tileEntities
                .Where(e => e.type == TileDataModel.Type.Region)
                .OrderBy(e => e.serialNumber)
                .ToList();

            var tiles = new List<TileDataModel>();
            for (int i = 0; i < tileDataModels.Count; i++)
                tiles.Add(tileDataModels[i].TileDataModel);
            tiles = tiles.OrderBy(e => e.regionId).ToList();

            layerPanelContainer.Add(new TileGridView(
                tiles,
                tileDataModel =>
                {
                    // 描画するタイルを変更。
                    _mapEditCanvas.ChangeTileToDraw(tileDataModel, tiles);

                    // 選択したリージョンタイルに対応したエンカウンターインスペクタを表示。
                    Inspector.Inspector.EncounterSceneView(_mapDataModel.id, tileDataModel.regionId % 1000, null);
                },
                13,
                1 + 64));


            rootVisualElement.Clear();
            rootVisualElement.Add(uxmlElement);

            // mapCanvasのレイアウトが完了したタイミングでサイズが取れるようになるので、そのタイミングで初回Renderするように
            _mapEditCanvas.RegisterCallback<GeometryChangedEvent>(v => { _mapEditCanvas.Refresh(); });
        }

        public void ChangeDrawMode(MapEditCanvas.DrawMode drawMode) {
            _mapEditCanvas.ChangeDrawMode(drawMode);
        }

        //選択座標の更新
        public static void PointDisplay(int x, int y) {
            if (_pointLabel != null)
            {
                _pointLabel.text = EditorLocalize.LocalizeText("WORD_0983") + ":　　　　" + x + "," + y;
                _pointLabel.style.color = Color.white;
                if (!EditorGUIUtility.isProSkin)
                {
                    _pointLabel.style.color = Color.black;
                }
            }
        }

        private void OnDestroy() {
            _mapEditCanvas?.Dispose();
        }
    }
}