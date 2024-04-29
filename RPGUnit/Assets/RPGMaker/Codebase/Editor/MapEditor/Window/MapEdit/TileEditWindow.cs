using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Inventory;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit
{
    /// <summary>
    /// タイル編集ウィンドウ.
    /// </summary>
    public class TileEditWindow : EditorWindow
    {
        private const string Uxml = "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/MapEdit/tile_edit.uxml";

        // データプロパティ
        private List<TileDataModelInfo>  _tileEntities;

        // UI要素プロパティ
        private TileInventory      _tileInventory;
        private TileImageInventory _tileImageInventory;
        private Button             _btnImportTileImage;

        /**
         * 初期化
         */
        public void Init(List<TileDataModelInfo> tileEntities) {
            _tileEntities = tileEntities;
            InitUI();
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh([CanBeNull] List<TileDataModelInfo> tileEntities = null) {
            if (tileEntities != null) _tileEntities = tileEntities;
            _tileInventory.Refresh(_tileEntities);
            _tileImageInventory.Refresh();
        }

        /**
         * 表示サイズの更新
         * ウィンドウサイズを変更した際に調節する
         */
        public void RefreshWindowSize(float windowWidth) {
            _tileImageInventory.style.height = windowWidth / 1.5f;
            _tileInventory.style.height = windowWidth / 1.5f;
        }

        /**
         * UI初期化
         */
        private void InitUI() {
            rootVisualElement.Clear();

            VisualElement uxmlElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree();
            
            StyleSheet styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkLayout);
            StyleSheet styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkTileInventory);
            StyleSheet styleTileImageInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssDarkTileImageInventory);
            if (!EditorGUIUtility.isProSkin)
            {
                styleLayout = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightLayout);
                styleTileInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightTileInventory);
                styleTileImageInventory = AssetDatabase.LoadAssetAtPath<StyleSheet>(MapEditor.UssLightTileImageInventory);
            }
            uxmlElement.styleSheets.Add(styleLayout);
            uxmlElement.styleSheets.Add(styleTileInventory);
            uxmlElement.styleSheets.Add(styleTileImageInventory);
            
            EditorLocalize.LocalizeElements(uxmlElement);
            VisualElement tileListContainer = uxmlElement.Query<VisualElement>("tile_list_container");
            VisualElement fileListContainer = uxmlElement.Query<VisualElement>("file_list_container");

            // タイルリスト
            _tileInventory = new TileInventory(
                TileInventory.TileInventoryType.TileEdit,
                _tileEntities,
                SelectTile,
                null
            );
            tileListContainer.Add(_tileInventory);

            // タイル画像リスト
            _tileImageInventory = new TileImageInventory(SelectTileImage);
            fileListContainer.Add(_tileImageInventory);

            // タイル画像を読み込むボタン
            _btnImportTileImage = uxmlElement.Query<Button>("load_tile_button");
            _btnImportTileImage.clicked += ImportTileImage;

            // 要素配置
            rootVisualElement.Add(uxmlElement);
        }

        /**
         * タイルを選択し、インスペクターに反映する
         */
        private static void SelectTile(TileDataModel tileDataModel) {
            MapEditor.SetTileEntityToInspector(tileDataModel);
        }

        /**
         * タイル画像を選択し、新規タイルエンティティを一時的に生成し、インスペクターに反映する
         */
        private static void SelectTileImage(TileImageDataModel tileImageDataModel) {
            var tileEntity = MapEditor.CreateTile(tileImageDataModel, TileDataModel.Type.AutoTileA);
            MapEditor.SetTileEntityToInspector(tileEntity, true);
        }

        /**
         * タイル用画像を読み込む
         */
        private void ImportTileImage() {
            MapEditor.ImportImageForTile();
            MapEditor.ReloadTileImageEntities();
        }
    }
}