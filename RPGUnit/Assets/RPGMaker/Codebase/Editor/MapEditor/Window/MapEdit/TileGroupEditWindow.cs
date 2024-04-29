using JetBrains.Annotations;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Inspector.Map.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.Inventory;
using RPGMaker.Codebase.Editor.MapEditor.Component.ListView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit
{
    /// <summary>
    ///     タイルグループ編集ウィンドウ.
    /// </summary>
    public class TileGroupEditWindow : EditorWindow
    {
        private const string Uxml = "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/MapEdit/tile_group_edit.uxml";
        private       Button _btnAddTileToGroup;
        private       List<TileDataModelInfo> _tileEntities;

        // データプロパティ
        private TileGroupDataModel _tileGroupDataModel;
        private TileInventory      _tileInventory;

        // UI要素プロパティ
        private TileListView _tilesInGroup;

        /**
         * 初期化
         */
        public void Init(TileGroupDataModel tileGroupDataModel, List<TileDataModelInfo> tileEntities) {
            _tileGroupDataModel = tileGroupDataModel;
            _tileEntities = tileEntities;
            InitUI();
            Refresh();
            Inspector.Inspector.TileGroupEditView(tileGroupDataModel);
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh(
            [CanBeNull] TileGroupDataModel tileGroupDataModel = null,
            [CanBeNull] List<TileDataModelInfo> tileEntities = null
        ) {
            if (tileGroupDataModel != null) _tileGroupDataModel = tileGroupDataModel;
            if (tileEntities != null) _tileEntities = tileEntities;

            _tilesInGroup.Refresh(_tileGroupDataModel.tileDataModels);
            _tileInventory.Refresh(_tileEntities);
        }

        /**
         * UI初期化
         */
        private void InitUI() {
            rootVisualElement.Clear();

            VisualElement uxmlElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree();
            
            StyleSheet styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayout);
            StyleSheet styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkTileInventory);
            if (!EditorGUIUtility.isProSkin)
            {
                styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayout);
                styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightTileInventory);
            }
            uxmlElement.styleSheets.Add(styleLayout);
            uxmlElement.styleSheets.Add(styleTileInventory);
            
            EditorLocalize.LocalizeElements(uxmlElement);
            VisualElement tileGroupContainer = uxmlElement.Query<VisualElement>("tile_group_container");
            VisualElement tileListContainer = uxmlElement.Query<VisualElement>("tile_list_container");

            // タイルの存在チェック
            for (int i = 0; i < _tileGroupDataModel.tileDataModels.Count; i++)
            {
                if (!File.Exists(TileRepository.GetAssetPath(_tileGroupDataModel.tileDataModels[i])))
                {
                    _tileGroupDataModel.tileDataModels.RemoveAt(i);
                    i--;
                }
            }

            // グループのタイル一覧
            _tilesInGroup = new TileListView(
                _tileGroupDataModel.tileDataModels,
                (tile, tileList) =>
                    Inspector.Inspector.MapTileView(tile, TileInspector.TYPE.TILEGROUP),
                tileList => { OnMouseDownEventDelete(); },
                isDoubleCursor: true);
            tileGroupContainer.Add(_tilesInGroup);

            // グループからタイルを削除ボタン
            Button btnRemoveTileFromGroup = uxmlElement.Query<Button>("remove_tile_button");
            btnRemoveTileFromGroup.clicked += RemoveCurrentSelectingTileFromGroup;

            // 全タイルリスト
            _tileInventory = new TileInventory(
                TileInventory.TileInventoryType.TileGroupEdit,
                _tileEntities,
                tile => Inspector.Inspector.MapTileView(tile, TileInspector.TYPE.TILEGROUP),
                null
            );
            _tileInventory.RegisterCallback<MouseDownEvent>(OnMouseDownEventAdd);
            tileListContainer.Add(_tileInventory);

            // 選択したタイルをグループに追加ボタン
            _btnAddTileToGroup = uxmlElement.Query<Button>("add_tile_button");
            _btnAddTileToGroup.clicked += AddCurrentSelectingTileToGroup;

            // 要素配置
            rootVisualElement.Add(uxmlElement);
        }

        /**
         * コンテキストメニューの削除用処理
         */
        private void OnMouseDownEventDelete() {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false,
                () => { RemoveCurrentSelectingTileFromGroup(); });

            menu.ShowAsContext();
        }

        /**
         * コンテキストメニューの追加用処理
         */
        private void OnMouseDownEventAdd(MouseEventBase<MouseDownEvent> evt) {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0050")), false,
                    () => { AddCurrentSelectingTileToGroup(); });

                menu.ShowAsContext();
            }
        }

        /**
         * タイルをグループに追加
         */
        private void AddCurrentSelectingTileToGroup() {
            var targetTile = _tileInventory.CurrentSelectingTile;
            if (targetTile == null) return;

            //大型パーツの場合は特殊処理を実施
            bool addTile = false;
            if (targetTile.type == TileDataModel.Type.LargeParts)
            {
                var tiles = _tileInventory.TileEntities.Where(
                    t => t.type == TileDataModel.Type.LargeParts &&
                         t.largePartsDataModel.parentId == targetTile.largePartsDataModel.parentId);

                foreach (var tile in tiles)
                {
                    bool find = false;
                    for (int i = 0; i < _tileGroupDataModel.tileDataModels.Count; i++)
                        if (_tileGroupDataModel.tileDataModels[i].id == tile.id)
                        {
                            find = true;
                            break;
                        }
                    if (find == false)
                    {
                        _tileGroupDataModel.tileDataModels.Add(tile);
                        addTile = true;
                    }
                }
            }
            else
            {
                bool find = false;
                for (int i = 0; i < _tileGroupDataModel.tileDataModels.Count; i++)
                    if (_tileGroupDataModel.tileDataModels[i].id == targetTile.id)
                    {
                        find = true;
                        break;
                    }
                if (find == false)
                {
                    _tileGroupDataModel.tileDataModels.Add(targetTile.tileDataModelInfo);
                    addTile = true;
                }
            }

            if (addTile)
            {
                _tilesInGroup.Refresh(_tileGroupDataModel.tileDataModels);
                MapEditor.SaveTileGroup(_tileGroupDataModel);
            }
        }

        /**
         * タイルをグループから削除
         */
        private void RemoveCurrentSelectingTileFromGroup() {
            var targetTile = _tilesInGroup.CurrentSelectingTile;
            if (targetTile == null) return;

            var tiles = _tilesInGroup.CurrentSelectingTile.type == TileDataModel.Type.LargeParts
                ? _tileGroupDataModel.tileDataModels.Where(
                        tile => tile.type == TileDataModel.Type.LargeParts &&
                                tile.largePartsDataModel.parentId ==
                                _tilesInGroup.CurrentSelectingTile.largePartsDataModel.parentId)
                    .ToArray()
                : new[] {_tilesInGroup.CurrentSelectingTile.tileDataModelInfo};

            foreach (var tile in tiles) _tileGroupDataModel.tileDataModels.RemoveAll(t => t.id == tile.id);

            _tilesInGroup.Refresh(_tileGroupDataModel.tileDataModels, TileListView.SelectionTileAfterRefreshType.EndTile);
            MapEditor.SaveTileGroup(_tileGroupDataModel);
        }
    }
}