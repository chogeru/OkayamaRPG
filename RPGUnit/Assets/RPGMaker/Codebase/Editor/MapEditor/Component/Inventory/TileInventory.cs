using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.ListView;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Inventory
{
    /**
     * タイルインベントリーコンポーネント
     */
    public class TileInventory : VisualElement
    {
        public enum TileInventoryType
        {
            MapEdit,
            TileEdit,
            TileGroupEdit
        }

        private static readonly TileDataModel.Type[] TabTileTypes =
        {
            TileDataModel.Type.AutoTileA,
            TileDataModel.Type.AutoTileB,
            TileDataModel.Type.AutoTileC,
            TileDataModel.Type.NormalTile,
            TileDataModel.Type.LargeParts,
            TileDataModel.Type.Effect
        };

        private static readonly Dictionary<TileDataModel.Type, string> TabTypeWoadList =
            new Dictionary<TileDataModel.Type, string>
            {
                {TileDataModel.Type.AutoTileA, "WORD_0740"},
                {TileDataModel.Type.AutoTileB, "WORD_0741"},
                {TileDataModel.Type.AutoTileC, "WORD_0742"},
                {TileDataModel.Type.NormalTile, "WORD_0743"},
                {TileDataModel.Type.LargeParts, "WORD_0744"},
                {TileDataModel.Type.Effect, "WORD_0745"}
            };

        private readonly Action<TileDataModel.Type> _onChangeTileType;

        // コールバック関数プロパティ
        private readonly Action<TileDataModel> _onSelectTile;

        // UI要素プロパティ
        private VisualElement _tabs;

        // データプロパティ
        private TileListView _tileListView;

        /**
         * コンストラクタ
         */
        public TileInventory(
            TileInventoryType type,
            List<TileDataModelInfo> tileEntities,
            Action<TileDataModel> onSelectTile,
            Action<TileDataModel.Type> onChangeTileType
        ) {
            Type = type;
            TileEntities = tileEntities;
            _onSelectTile = onSelectTile;
            _onChangeTileType = onChangeTileType;
            InitUI();
            ChangeTileType(TileDataModel.Type.AutoTileA);
        }

        public TileInventoryType Type { get; }

        // 大型パーツタイル用
        public List<TileDataModelInfo> TileEntities { get; private set; }

        // 状態プロパティ
        public TileDataModel.Type CurrentTileType { get; private set; }
        public TileDataModel CurrentSelectingTile { get; private set; }

        /**
         * データおよび表示を更新
         */
        public void Refresh([CanBeNull] List<TileDataModelInfo> tiles = null) {
            if (tiles != null) TileEntities = tiles;
            _tileListView.Refresh(GetTilesOfCurrentType());
        }

        /**
         * UI初期化
         */
        private void InitUI() {
            style.height = Length.Percent(100);
            style.minHeight = 100f;
            // タブ
            _tabs = new VisualElement();
            _tabs.style.flexDirection = FlexDirection.Row;
            _tabs.style.minHeight = 25f;
            foreach (var tabTileType in TabTileTypes)
            {
                var btn = new Button {text = EditorLocalize.LocalizeText(TabTypeWoadList[tabTileType])};
                btn.clicked += () => { ChangeTileType(tabTileType); };
                _tabs.Add(btn);
            }

            // タイルリスト
            _tileListView = new TileListView(
                GetTilesOfCurrentType(), SelectTile, null, isDoubleCursor: Type == TileInventoryType.TileEdit || Type == TileInventoryType.TileGroupEdit);

            // 要素配置
            Clear();
            style.flexDirection = FlexDirection.Column;
            Add(_tabs);
            Add(_tileListView);
        }

        /**
         * 表示するタイルタイプを変更
         */
        private void ChangeTileType(TileDataModel.Type tileType) {
            // 選択中のタイルタイプが判別できるように、変更後のタイルタイプのボタンのみ無効に設定。
            foreach (var (button, tabTileTypeIndex) in _tabs.Children().Select(
                (ve, index) => ((Button) ve, index)))
                button.SetEnabled(TabTileTypes[tabTileTypeIndex] != tileType);

            CurrentTileType = tileType;
            CurrentSelectingTile = null;
            Refresh();
            _onChangeTileType?.Invoke(tileType);
        }

        /**
         * タイルを選択する
         */
        private void SelectTile(TileDataModel tileDataModel, List<TileDataModel> tileDataModels) {
            CurrentSelectingTile = tileDataModel;
            _onSelectTile?.Invoke(CurrentSelectingTile);
        }

        /**
         * 現在表示するべきタイプのタイル一覧を取得
         */
        private List<TileDataModelInfo> GetTilesOfCurrentType() {
            return TileEntities.FindAll(tile => tile.type.Equals(CurrentTileType)).ToList();
        }
    }
}