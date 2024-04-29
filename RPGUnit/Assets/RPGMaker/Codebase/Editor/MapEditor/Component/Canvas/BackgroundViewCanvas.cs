using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Inventory;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas
{
    /// <summary>
    /// 背景ビュー管理用
    /// </summary>
    public class BackgroundViewCanvas
    {
        private       VisualElement _backgroundView;
        private       VisualElement _buttonContainer;

        private Vector2    _imageContentSize;
        private ScrollView _images;

        private LayerInventory _layerInventory;

        // データプロパティ
        private MapDataModel  _mapDataModel;
        private MapEditCanvas _mapEditCanvas;

        // UI要素プロパティ
        private MapEditWindow _mapEditWindow;
        private Vector2       _texOriginalSize;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="mapEditWindow"></param>
        /// <param name="mapDataModel"></param>
        /// <param name="mapEditCanvas"></param>
        /// <param name="layerInventory"></param>
        /// <param name="images"></param>
        /// <param name="buttonContainer"></param>
        /// <returns></returns>
        public VisualElement CreateButtonMenu(
            MapEditWindow mapEditWindow,
            MapDataModel mapDataModel,
            MapEditCanvas mapEditCanvas,
            LayerInventory layerInventory,
            ScrollView images,
            VisualElement buttonContainer
        ) {
            _mapEditWindow = mapEditWindow;
            _mapDataModel = mapDataModel;
            _mapEditCanvas = mapEditCanvas;
            _layerInventory = layerInventory;
            _images = images;
            _buttonContainer = buttonContainer;
            return BackgroundButtonMenu();
        }

        /// <summary>
        /// 背景の読み込み
        /// </summary>
        private void LoadBackgroundImage() {
            var path = AssetManageImporter.StartToFile(
                "png",
                PathManager.MAP_BACKGROUND,
                new Vector2(0, 1), true, false, true, true, true);
            if (!string.IsNullOrEmpty(path))
            {
                var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(path);
                var spr = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                path = Path.GetFileNameWithoutExtension(path);
                _mapDataModel.background.imageName = path;

                //Mapのリロード
                _mapEditWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                        MapEditWindow;
                _mapEditWindow.UpdateBackgroundView(_mapDataModel, spr, tex);

                MapEditor.SetBackgroundViewToInspector(_mapDataModel);
            }
        }

        /// <summary>
        /// 背景画像の削除
        /// </summary>
        /// <param name="fileName">背景画像名</param>
        private void DeleteBackgroundImage(string fileName) {
            //画像名が設定されていない場合は実行しない
            if (!string.IsNullOrEmpty(fileName))
            {
                File.Delete(PathManager.MAP_BACKGROUND + fileName);
                _backgroundView.style.backgroundImage = null;
                _mapDataModel.background.imageName = "";
                _mapEditCanvas.MapImageChange(MapDataModel.Layer.LayerType.Background, null);
                MapEditor.SetBackgroundViewToInspector(_mapDataModel);
            }
        }

        /// <summary>
        /// ボタンメニューの作成
        /// </summary>
        /// <returns></returns>
        private VisualElement BackgroundButtonMenu() {
            _images = new ScrollView {name = "images"};
            _images.style.height = Length.Percent(100);
            _images.RegisterCallback<GeometryChangedEvent>(v =>
            {
                if (_backgroundView.style.backgroundImage.value.texture == null) return;

                _imageContentSize.x = _images.contentRect.width;
                _imageContentSize.y = _images.contentRect.height;
                BackgroundImageHelper.FixAspectRatio(_backgroundView, _imageContentSize, _texOriginalSize,
                    LengthUnit.Pixel);
            });

            try
            {
                _layerInventory.Remove(_images);
            }
            catch
            {
            }

            ;

            var layerPanelBtnContainer = _buttonContainer;

            var scrollView = new ScrollView();
            scrollView.style.height = Length.Percent(100);

            var btnLoadImage = new Button {text = EditorLocalize.LocalizeText("WORD_1528")};
            btnLoadImage.clicked += LoadBackgroundImage;
            scrollView.Add(btnLoadImage);

            var btnDeleteImage = new Button {text = EditorLocalize.LocalizeText("WORD_0383")};
            btnDeleteImage.clicked += () =>
            {
                DeleteBackgroundImage(Path.ChangeExtension(_mapDataModel.background.imageName, ".png"));
            };
            scrollView.Add(btnDeleteImage);

            layerPanelBtnContainer.Add(scrollView);

            _backgroundView = new VisualElement();
            UpdateImagePreview(UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                _mapDataModel.background.ImageFilePath));

            _images.Add(_backgroundView);
            _layerInventory.Add(_images);
            _layerInventory.SetTileListViewActive(false);
            MapEditor.SetBackgroundViewToInspector(_mapDataModel);

            return layerPanelBtnContainer;
        }

        /// <summary>
        /// 画像をリストに反映
        /// </summary>
        /// <param name="tex2d"></param>
        public void UpdateImagePreview(Texture2D tex2d) {
            _backgroundView.style.backgroundImage = tex2d;

            if (tex2d == null) return;

            _texOriginalSize.x = tex2d.width;
            _texOriginalSize.y = tex2d.height;
            _imageContentSize.x = _images.contentRect.width;
            _imageContentSize.y = _images.contentRect.height;
            BackgroundImageHelper.FixAspectRatio(
                _backgroundView, _imageContentSize, _texOriginalSize, LengthUnit.Pixel);
        }
    }
}