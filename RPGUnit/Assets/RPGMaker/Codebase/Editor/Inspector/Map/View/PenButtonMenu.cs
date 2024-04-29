using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[マップ編集] 時に表示するペンボタン用クラス
    /// </summary>
    public class PenButtonMenu : VisualElement
    {
        public enum EditTarget
        {
            Map,
            Battle
        }

        private readonly List<int> _btnTypes = new List<int>
        {
            (int) MenuWindow.BtnType.Pen,
            (int) MenuWindow.BtnType.Rectangle,
            (int) MenuWindow.BtnType.Ellipse,
            (int) MenuWindow.BtnType.Fill,
            (int) MenuWindow.BtnType.Shadow,
            (int) MenuWindow.BtnType.Eraser
        };

        private readonly string mainUxml = "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/PenMenu.uxml";
        private          Image  _activeButton;
        private          Image  _previousButton;
        private          bool   _isUniqueEraser;

        private readonly EditTarget    _editTarget;
        private readonly VisualElement _menuArea;

        public PenButtonMenu(EditTarget editTarget) {
            _editTarget = editTarget;

            Clear();
            var items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mainUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            labelFromUxml.style.flexGrow = 1;
            items.Add(labelFromUxml);
            _menuArea = items.Query<VisualElement>("menu_area");
            _menuArea.style.flexDirection = FlexDirection.Row;
            InitUi();
        }

        public VisualElement GetPenButtonMenuElement() {
            return _menuArea;
        }

        private void InitUi() {
            for (var i = 0; i < _btnTypes.Count; i++)
            {
                var image = new Image();
                image.name = _btnTypes[i].ToString("000");
                image.image =
                    AssetDatabase.LoadAssetAtPath<Texture>(
                        MenuEditorView.ImagePath + MenuEditorView.EditerMode() + MenuEditorView.ImagePathActive +
                        MenuEditorView.ImageIconMenu +
                        _btnTypes[i].ToString("000") + ".png"
                    );
                image.style.width = image.image.width;
                image.style.height = image.image.height;
                image.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
                _menuArea.Add(image);
            }

            if (_editTarget == EditTarget.Battle) SetButtonDisplayStyle(MenuWindow.BtnType.Shadow, DisplayStyle.None);

            // 初期アクティブボタンを設定。
            ChangeActiveButton(GetButtonImage(MenuWindow.BtnType.Pen));
        }

        private void SetButtonDisplayStyle(MenuWindow.BtnType btnType, DisplayStyle displayStyle) {
            GetButtonImage(btnType).style.display = displayStyle;
        }

        private Image GetButtonImage(MenuWindow.BtnType btnType) {
            return (Image) _menuArea.Children().ElementAt(_btnTypes.IndexOf((int) btnType));
        }

        private void OnMouseDownEvent(MouseEventBase<MouseDownEvent> evt) {
            ChangeActiveButton((Image) evt.target);
        }

        private void ChangeActiveButton(Image activeButtonImage) {
            // オートタイルCと大型パーツ専用処理
            if ((MapEditor.MapEditor.GetTileToDraw()?.type == CoreSystem.Knowledge.DataModel.Map.TileDataModel.Type.AutoTileC ||
                MapEditor.MapEditor.GetTileToDraw()?.type == CoreSystem.Knowledge.DataModel.Map.TileDataModel.Type.LargeParts) &&
                activeButtonImage == GetButtonImage(MenuWindow.BtnType.Fill))
                activeButtonImage = GetButtonImage(MenuWindow.BtnType.Pen);

            if (_activeButton != null && _previousButton != null )
                if (_isUniqueEraser == true)
                    _isUniqueEraser = false;
                else
                    _isUniqueEraser = UniqueEraser((MenuWindow.BtnType) int.Parse(activeButtonImage.name), (MenuWindow.BtnType) int.Parse(_activeButton.name), (MenuWindow.BtnType) int.Parse(_previousButton.name));

            if (_activeButton != null && _isUniqueEraser == false)
                _activeButton.image =
                    AssetDatabase.LoadAssetAtPath<Texture>(
                        MenuEditorView.ImagePath + MenuEditorView.EditerMode() + MenuEditorView.ImagePathActive + MenuEditorView.ImageIconMenu + _activeButton.name + ".png");

            if (_previousButton != null && _isUniqueEraser == false)
                _previousButton.image =
                    AssetDatabase.LoadAssetAtPath<Texture>(
                        MenuEditorView.ImagePath + MenuEditorView.EditerMode() + MenuEditorView.ImagePathActive + MenuEditorView.ImageIconMenu + _previousButton.name + ".png");

            _previousButton = _activeButton;

            var image = activeButtonImage;
            var iconName = image.name;
            image.image =
                AssetDatabase.LoadAssetAtPath<Texture>(
                    MenuEditorView.ImagePath + MenuEditorView.EditerMode() + MenuEditorView.ImagePathDisable + MenuEditorView.ImageIconMenuD + iconName + ".png");
            SelectAction((MenuWindow.BtnType) int.Parse(image.name), _previousButton == null ? (MenuWindow.BtnType) int.Parse(image.name) : (MenuWindow.BtnType) int.Parse(_previousButton.name));
            _activeButton = image;
        }

        private void SelectAction(MenuWindow.BtnType btnType, MenuWindow.BtnType previousBtnType) {
            switch (btnType)
            {
                case MenuWindow.BtnType.Pen:
                    MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.Put);
                    break;
                case MenuWindow.BtnType.Rectangle:
                    if (_isUniqueEraser == true)
                        MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.DeleteRectangle);
                    else
                        MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.Rectangle);
                    break;
                case MenuWindow.BtnType.Ellipse:
                    if (_isUniqueEraser == true)
                        MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.DeleteEllipse);
                    else
                        MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.Ellipse);
                    break;
                case MenuWindow.BtnType.Fill:
                    MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.Fill);
                    break;
                case MenuWindow.BtnType.Shadow:
                    MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.Shadow);
                    break;
                case MenuWindow.BtnType.Eraser:
                    if (_isUniqueEraser == true)
                        if (previousBtnType == MenuWindow.BtnType.Rectangle)
                            MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.DeleteRectangle);
                        else
                            MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.DeleteEllipse);
                    else
                        MapEditor.MapEditor.ChangeDrawMode(_editTarget, MapEditCanvas.DrawMode.Delete);
                    break;
            }
        }

        // 特殊消しゴム判定
        private bool UniqueEraser(MenuWindow.BtnType next, MenuWindow.BtnType active, MenuWindow.BtnType previous) {
            if (next == MenuWindow.BtnType.Eraser && 
                (active == MenuWindow.BtnType.Rectangle || active == MenuWindow.BtnType.Ellipse))
                return true;
            else if (active == MenuWindow.BtnType.Eraser &&
                (next == MenuWindow.BtnType.Rectangle || next == MenuWindow.BtnType.Ellipse))
                return true;
            return false;
        }

        public void ResetButton(MenuWindow.BtnType type) {
            ChangeActiveButton(GetButtonImage(type));
        }

        public void ButtonDisplayChange(MenuWindow.BtnType type, DisplayStyle displayStyle) {
            SetButtonDisplayStyle(type, displayStyle);
        }
    }
}