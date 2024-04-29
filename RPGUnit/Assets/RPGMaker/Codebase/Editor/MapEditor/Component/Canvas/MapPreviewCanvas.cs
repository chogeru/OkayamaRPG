using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using System;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas
{
    /// <summary>
    /// マップ表示用
    /// </summary>
    public class MapPreviewCanvas : MapCanvas
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="repaintMethod"></param>
        public MapPreviewCanvas(MapDataModel mapDataModel, Action repaintMethod)
            : base(mapDataModel, repaintMethod, true)
        {
            style.height = Length.Percent(100);
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------
        protected override void OnMouseDown(IMouseEvent e) {
            if (e.altKey || (MouseButton)e.button == MouseButton.MiddleMouse)
            {
                IsMouseDown = true;
                return;
            }
        }

        protected override void OnMouseUp(IMouseEvent e) {
            base.OnMouseUp(e);
        }

        protected override void OnMouseDrag(MouseMoveEvent e) {
            // 中央ボタン(ホイール)押下中ならベースに処理させる。
            if (e.pressedButtons == MousePressedButtons.Middle)
            {
                base.OnMouseDrag(e);
                return;
            }

            if (!IsMouseDown || !(e.pressedButtons == MousePressedButtons.Left || e.pressedButtons == MousePressedButtons.Right))
            {
                //マウスをクリックしていない
                IsMouseDown = false;
                return;
            }

            // Altキー押下中ならマップ移動。
            if (e.altKey)
            {
                IsAltOn = true;
                base.OnMouseDrag(e);
                return;
            }

            if (IsAltOn)
            {
                IsMouseDown = false;
                IsAltOn = false;
                return;
            }
        }
    }
}