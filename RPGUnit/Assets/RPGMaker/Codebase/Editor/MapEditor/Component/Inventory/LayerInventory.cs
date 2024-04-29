using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.ListView;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Inventory
{
    /**
     * レイヤーインベントリーコンポーネント
     */
    public class LayerInventory : VisualElement
    {
        private static readonly MapDataModel.Layer.LayerType[] NonEffectPlacementModeLayerTypes =
        {
            MapDataModel.Layer.LayerType.DistantView,
            MapDataModel.Layer.LayerType.Background,
            MapDataModel.Layer.LayerType.BackgroundCollision,
            MapDataModel.Layer.LayerType.A,
            MapDataModel.Layer.LayerType.B,
            MapDataModel.Layer.LayerType.Shadow,
            MapDataModel.Layer.LayerType.C,
            MapDataModel.Layer.LayerType.D,
            MapDataModel.Layer.LayerType.ForRoute
        };

        private static readonly MapDataModel.Layer.LayerType[] EffectPlacementModeLayerTypes =
        {
            MapDataModel.Layer.LayerType.A_Effect,
            MapDataModel.Layer.LayerType.B_Effect,
            MapDataModel.Layer.LayerType.C_Effect,
            MapDataModel.Layer.LayerType.D_Effect
        };

        private static readonly Dictionary<MapDataModel.Layer.LayerType, string> LayerTypeWoadList =
            new Dictionary<MapDataModel.Layer.LayerType, string>
            {
                {MapDataModel.Layer.LayerType.DistantView, "WORD_0779"},
                {MapDataModel.Layer.LayerType.Background, "WORD_0780"},
                {MapDataModel.Layer.LayerType.BackgroundCollision, "WORD_0791"},
                {MapDataModel.Layer.LayerType.A, "WORD_0781"},
                {MapDataModel.Layer.LayerType.B, "WORD_0782"},
                {MapDataModel.Layer.LayerType.Shadow, "WORD_0803"},
                {MapDataModel.Layer.LayerType.C, "WORD_0783"},
                {MapDataModel.Layer.LayerType.D, "WORD_0784"},
                {MapDataModel.Layer.LayerType.ForRoute, "WORD_0784"}
            };

        private static readonly Dictionary<MapDataModel.Layer.LayerType, string> EffectTypeWoadList =
            new Dictionary<MapDataModel.Layer.LayerType, string>
            {
                {MapDataModel.Layer.LayerType.A_Effect, "WORD_3064"},
                {MapDataModel.Layer.LayerType.B_Effect, "WORD_3065"},
                {MapDataModel.Layer.LayerType.C_Effect, "WORD_3066"},
                {MapDataModel.Layer.LayerType.D_Effect, "WORD_3067"}
            };

        private readonly Action<MapDataModel.Layer> _onChangeLayer;
        private readonly Action<TileDataModel>      _onRightClickTile;

        // コールバック関数プロパティ
        private readonly Action<TileDataModel, List<TileDataModel>> _onSelectTile;

        // データプロパティ
        private List<MapDataModel.Layer> _layers;

        //下画面の右ボタンの切り替え用
        private MapEditWindow _mapEditWindow;

        // UI要素プロパティ
        private VisualElement _tabs;
        private TileListView  _tileListView;

        /**
         * コンストラクタ
         */
        public LayerInventory(
            List<MapDataModel.Layer> layers,
            Action<TileDataModel, List<TileDataModel>> onSelectTile,
            Action<TileDataModel> onRightClickTile,
            Action<MapDataModel.Layer> onChangeLayer,
            bool isEffectPlacementMode,
            MapDataModel.Layer.LayerType layerType
        ) {
            _layers = layers;
            _onSelectTile = onSelectTile;
            _onRightClickTile = onRightClickTile;
            _onChangeLayer = onChangeLayer;

            InitUI(isEffectPlacementMode);
            ChangeLayer(layers.Single(layer => layer.type == layerType));
        }

        // 状態プロパティ
        public MapDataModel.Layer CurrentLayer { get; private set; }
        public TileDataModel CurrentSelectingTile { get; private set; }

        /**
         * データおよび表示更新
         */
        public void Refresh([CanBeNull] List<MapDataModel.Layer> layers = null, bool selectionClear = false) {
            if (layers != null)
            {
                _layers = layers;
                ChangeLayer(layers.First());
            }

            var tiles = new List<TileDataModelInfo>();
            for (int i = 0; i < CurrentLayer?.tilesOnPalette?.Count; i++)
                tiles.Add(CurrentLayer.tilesOnPalette[i]);

            _tileListView.Refresh(
                tiles,
                selectionClear ?
                    TileListView.SelectionTileAfterRefreshType.Clear:
                    TileListView.SelectionTileAfterRefreshType.FirstTile);
        }


        /**
         * UI初期化
         */
        private void InitUI(bool isEffectPlacementMode) {
            var layerTypes =
                isEffectPlacementMode ? EffectPlacementModeLayerTypes : NonEffectPlacementModeLayerTypes;

            var layerTypesWord = isEffectPlacementMode ? EffectTypeWoadList : LayerTypeWoadList;

            // タブ
            _tabs = new VisualElement();
            _tabs.style.minHeight = 25f;
            _tabs.style.flexDirection = FlexDirection.Row;
            foreach (var layerType in layerTypes)
                //ForRouteタブを表示させない
                if (layerType != MapDataModel.Layer.LayerType.ForRoute)
                {
                    var layer = _layers.Single(layer => layer.type == layerType);
                    var btn = new Button
                    {
                        text = EditorLocalize.LocalizeText(layerTypesWord[layer.type]),
                        name = LayerTypeToTabButtonName(layerType)
                    };
                    btn.clicked += () => { ChangeLayer(layer); };
                    _tabs.Add(btn);
                }

            // タイルリスト
            _tileListView = new TileListView(_layers.First().tilesOnPalette, SelectTile, _onRightClickTile);

            // 要素配置
            Clear();
            style.flexDirection = FlexDirection.Column;
            style.height = Length.Percent(100);
            Add(_tabs);
            Add(_tileListView);
        }

        /**
         * 表示レイヤーを変更する
         */
        private void ChangeLayer(MapDataModel.Layer layer) {
            // 選択中のレイヤーが判別できるように、変更後のレイヤーのボタンのみ無効に設定。
            foreach (var button in _tabs.Children().Select(ve => (Button) ve))
                button.SetEnabled(TabButtonToLayerType(button) != layer.type);

            CurrentLayer = layer;
            CurrentSelectingTile = null;
            Refresh(null, true);
            _onChangeLayer?.Invoke(layer);
        }

        /**
         * 指定のレイヤータイプのタブボタンに設定する名前を取得
         */
        private string LayerTypeToTabButtonName(MapDataModel.Layer.LayerType layerType) {
            return $"{layerType.ToString()}_LayerTabButton";
        }

        /**
         * タブボタンに設定されている名前からレイヤータイプ取得
         */
        private MapDataModel.Layer.LayerType TabButtonToLayerType(Button tabButton) {
            return ((MapDataModel.Layer.LayerType[]) Enum.GetValues(typeof(MapDataModel.Layer.LayerType))).Single(
                layerType => LayerTypeToTabButtonName(layerType) == tabButton.name);
        }

        /**
         * 現在表示中のレイヤーにタイル一覧をまとめて設定
         */
        public void SetTilesInCurrentLayer(List<TileDataModel> tiles) {
            var tile = new List<TileDataModelInfo>();
            for (int i = 0; i < tiles.Count; i++)
                tile.Add(tiles[i].tileDataModelInfo);

            CurrentLayer.tilesOnPalette = tile;
        }

        /**
         * 現在表示中のレイヤーにタイルを追加
         */
        public void AddTileToCurrentLayer(TileDataModel tileDataModel, List<TileDataModelInfo> tleEntities) {
            if (CurrentLayer.tilesOnPalette == null)
                return;

            if (CurrentLayer.tilesOnPalette.Contains(tileDataModel.tileDataModelInfo)) return;

            if (tileDataModel.type == TileDataModel.Type.LargeParts)
            {
                var tiles = tleEntities.Where(
                    t => t.type == TileDataModel.Type.LargeParts &&
                         t.largePartsDataModel.parentId == tileDataModel.largePartsDataModel.parentId);

                foreach (var tile in tiles) CurrentLayer.tilesOnPalette.Add(tile);
            }
            else
            {
                CurrentLayer.tilesOnPalette.Add(tileDataModel.tileDataModelInfo);
            }

            Refresh();
        }

        /**
         * 現在表示中のレイヤーから、選択中のタイルを削除
         */
        public void RemoveCurrentSelectingTileFromCurrentLayer() {
            if (!CurrentSelectingTile) return;

            var tiles = CurrentSelectingTile.type == TileDataModel.Type.LargeParts
                ? CurrentLayer.tilesOnPalette.Where(
                        tile => tile.type == TileDataModel.Type.LargeParts &&
                                tile.largePartsDataModel.parentId == CurrentSelectingTile.largePartsDataModel.parentId)
                    .ToArray()
                : new[] {CurrentSelectingTile.tileDataModelInfo};

            foreach (var tile in tiles) CurrentLayer.tilesOnPalette.Remove(tile);

            Refresh();
        }

        public void RemoveTileFromCurrentLayer(TileDataModel tileDataModel) {
            if (!CurrentLayer.tilesOnPalette.Where(tile => tile.id == tileDataModel.id).Any())
                return;

            if (tileDataModel.type != TileDataModel.Type.LargeParts)
            {
                CurrentLayer.tilesOnPalette.RemoveAll(tile => tile.id == tileDataModel.id);
            }
            else
            {
                var tiles = CurrentLayer.tilesOnPalette.Where(tile => tile.type == TileDataModel.Type.LargeParts &&
                                    tile.largePartsDataModel.parentId == tileDataModel.largePartsDataModel.parentId).ToArray();
                foreach (var tile in tiles) CurrentLayer.tilesOnPalette.RemoveAll(t => t.id == tile.id);
            }

            Refresh();
        }

        /**
         * 現在選択中のレイヤーの表示・非表示を切り替える
         */
        public void ToggleCurrentLayerVisibility(bool value) {
            if (CurrentLayer.tilemap == null)
                return;

            CurrentLayer.tilemap.gameObject.GetComponent<TilemapRenderer>().enabled = value;
        }

        public void ToggleCurrentLayerEmphasis(bool value, MapDataModel mapDataModel) {
            foreach (var layer in _layers)
            {
                if (layer.spr != null)
                {
                    var sprTransform = mapDataModel.GetLayerTransformForEditor(layer.type);
                    var spr = sprTransform.GetComponent<SpriteRenderer>();
                    spr.color = new Color(1f, 1f, 1f, value ? 0.5f : 1f);
                }
                if (layer.tilemap == null) continue;
                layer.tilemap.color = new Color(1f,1f,1f,1f);
                if (CurrentLayer == layer) continue;
                if (value)
                {
                    layer.tilemap.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            
            
        }
        

        /**
         * タイルを選択する
         */
        private void SelectTile(TileDataModel tileDataModel, List<TileDataModel> tileDataModels) {
            CurrentSelectingTile = tileDataModel;
            _onSelectTile?.Invoke(tileDataModel, tileDataModels);
        }

        public void SetTileListViewActive(bool active) {
            if (active)
                _tileListView.style.display = DisplayStyle.Flex;
            else
                _tileListView.style.display = DisplayStyle.None;
        }
    }
}