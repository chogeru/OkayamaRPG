using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.ListView
{
    /**
     * タイル用画像リストコンポーネント
     */
    public class TileImageListView : UnityEngine.UIElements.ListView
    {
        // データプロパティ
        private List<string> _images;

        /**
         * コンストラクタ
         */
        public TileImageListView(Action<TileImageDataModel> onSelectionChange) {
            // 画像リストを取得
            _images = GetImages();

            selectionType = SelectionType.Single;
            fixedItemHeight = 16;
            makeItem = () => new Label();
            bindItem = (e, i) =>
            {
                var target = (Label) e;
                target.text = _images[i];
                target.RemoveFromClassList("list-row-even");
                target.RemoveFromClassList("list-row-odd");
                target.AddToClassList(i % 2 == 0 ? "list-row-even" : "list-row-odd");
            };

            style.flexDirection = FlexDirection.Row;

            this.onSelectionChange += obj => {
                // タイル用データを生成
                TileImageDataModel tileImageDataModel = 
                    new TileImageDataModel(new MapManagementService().ReadImage(_images[selectedIndex]), _images[selectedIndex]);
                onSelectionChange?.Invoke(tileImageDataModel); 
            };
        }

        /**
         * データおよび表示を更新
         */
        new public void Refresh() {
            _images = GetImages();
            itemsSource = _images;
            style.height = Length.Percent(100);
            Rebuild();
        }

        /**
         * 画像リストを取得
         */
        private List<string> GetImages() {
            var images = Directory.GetFiles(PathManager.MAP_TILE_IMAGE)
                            .Select(Path.GetFileName)
                            .Where(filename =>
                                filename.EndsWith(".gif") || filename.EndsWith(".jpg") || filename.EndsWith(".png"))
                            .ToList();
            return images;
        }
    }
}