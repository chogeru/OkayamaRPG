using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.GridView
{
    /**
     * タイルグリッドコンポーネント
     */
    public class TileGridView : VisualElement
    {
        private readonly Image                 _cursorImage;
        private readonly Action<TileDataModel> _onSelectionChange;
        private          int                   _selectedIndex = -1;

        private readonly int _tileCount;

        // データプロパティ
        private          List<TileDataModel> _tileEntities;
        private readonly List<VisualElement> tileVes = new List<VisualElement>();

        /**
         * コンストラクタ
         */
        public TileGridView(
            List<TileDataModel> tileEntities,
            Action<TileDataModel> onSelectionChange,
            int xCount,
            int tileCount = 0
        ) {
            _tileEntities = tileEntities;
            _onSelectionChange = onSelectionChange;
            _tileCount = tileCount;

            var cursor = CreateCursorTexture();
            _cursorImage = new Image();
            _cursorImage.style.width = 40;
            _cursorImage.style.height = 40;
            _cursorImage.image = cursor;
            _cursorImage.style.position = Position.Absolute;

            if (tileCount == 0) tileCount = tileEntities.Count;

            for (var tileIndex = 0; tileIndex < tileCount;)
            {
                var rowVe = new VisualElement();
                rowVe.style.flexDirection = FlexDirection.Row;

                for (var x = 0; tileIndex < tileCount && x < xCount; x++, tileIndex++)
                {
                    var tileVe = new VisualElement();

                    tileVe.userData = tileIndex;

                    var image = new Image();
                    image.style.width = 40;
                    image.style.height = 40;
                    image.image = _tileEntities[tileIndex].m_DefaultSprite.texture;
                    tileVe.Add(image);

                    tileVe.RegisterCallback<MouseUpEvent>(evt => Select((int) tileVe.userData));

                    rowVe.Add(tileVe);
                    tileVes.Add(tileVe);
                }

                Add(rowVe);
            }

            Select(0);
        }

        public TileDataModel CurrentSelectingTile { get; private set; }

        private void Select(int selectIndex) {
            if (_selectedIndex >= 0)
            {
                var prevSelectedVe = tileVes[_selectedIndex];
                prevSelectedVe.Remove(_cursorImage);
                prevSelectedVe.Q<Image>().image = _tileEntities[_selectedIndex].m_DefaultSprite.texture;
            }

            _selectedIndex = selectIndex;
            var selectedVe = tileVes[_selectedIndex];
            selectedVe.Add(_cursorImage);
            selectedVe.Q<Image>().image = _tileEntities[_selectedIndex + _tileCount].m_DefaultSprite.texture;

            CurrentSelectingTile = _tileEntities[_selectedIndex];
            _onSelectionChange?.Invoke(_tileEntities[_selectedIndex]);
        }

        public void Refresh() {
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh([CanBeNull] List<TileDataModel> tileEntities) {
            if (tileEntities != null) _tileEntities = tileEntities;
            Refresh();
        }

        private static Texture2D CreateCursorTexture() {
            const int TileSize = 96;
            const int FrameWidth = 4;
            const int FrameBorderWidth = 2;
            const int With1 = FrameBorderWidth;
            const int With2 = With1 + FrameWidth;
            const int With3 = With2 + FrameBorderWidth;

            var texture = new Texture2D(TileSize, TileSize, TextureFormat.RGBA32, false);
            for (var y = 0; y < TileSize; y++)
            for (var x = 0; x < TileSize; x++)
            {
                var color = Color.clear;
                if (x < With1 || x >= TileSize - With1 || y < With1 || y >= TileSize - With1)
                    color = Color.black;
                else if (x < With2 || x >= TileSize - With2 || y < With2 || y >= TileSize - With2)
                    color = Color.white;
                else if (x < With3 || x >= TileSize - With3 || y < With3 || y >= TileSize - With3) color = Color.black;

                texture.SetPixel(x, y, color);
            }

            texture.Apply();

            return texture;
        }
    }
}