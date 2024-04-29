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
    /// 遠景ビュー管理用
    /// </summary>
    public class DistantViewCanvas
    {
        private       VisualElement _distantView;

        private Vector2    _imageContentSize;
        private ScrollView _images;

        private LayerInventory _layerInventory;

        // データプロパティ
        private MapDataModel _mapDataModel;

        // UI要素プロパティ
        private MapEditCanvas _mapEditCanvas;
        private Vector2       _texOriginalSize;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="mapEditCanvas"></param>
        /// <param name="layerInventory"></param>
        /// <param name="images"></param>
        /// <param name="buttonContainer"></param>
        /// <returns></returns>
        public VisualElement CreateButtonMenu(
            MapDataModel mapDataModel,
            MapEditCanvas mapEditCanvas,
            LayerInventory layerInventory,
            ScrollView images,
            VisualElement buttonContainer
        ) {
            _mapDataModel = mapDataModel;
            _mapEditCanvas = mapEditCanvas;
            _layerInventory = layerInventory;
            _images = images;
            return DistantViewButtonMenu(buttonContainer);
        }

        /// <summary>
        /// 遠景の読み込み
        /// </summary>
        private void LoadDistantViewImage() {
            var path = AssetManageImporter.StartToFile(
                "png", PathManager.MAP_PARALLAX, new Vector2(0, 1), true, false, true, true);
            if (!string.IsNullOrEmpty(path))
            {
                var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(path);
                var spr = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                path = Path.GetFileNameWithoutExtension(path);
                _mapDataModel.Parallax.name = path;
                _distantView.style.backgroundImage = tex;
                
                //Mapのリロード
                var mapEditWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                        MapEditWindow;
                mapEditWindow?.UpdateDistantView(_mapDataModel, spr, tex);

                MapEditor.SetDistantViewToInspector(_mapDataModel);
            }
        }

        /// <summary>
        /// 遠景画像の削除
        /// </summary>
        /// <param name="fileName">遠景画像名</param>
        private void DeleteDistantViewImage(string fileName) {
            //画像名が設定されていない場合は実行しない
            if (!string.IsNullOrEmpty(fileName))
            {
                File.Delete(PathManager.MAP_PARALLAX + fileName);
                _distantView.style.backgroundImage = null;
                _mapDataModel.Parallax.name = "";
                _mapEditCanvas.MapImageChange(MapDataModel.Layer.LayerType.DistantView, null);
                MapEditor.SetDistantViewToInspector(_mapDataModel);
            }
        }

        /// <summary>
        /// ボタンメニューの作成
        /// </summary>
        /// <param name="buttonContainer"></param>
        /// <returns></returns>
        private VisualElement DistantViewButtonMenu(VisualElement buttonContainer) {
            _images = new ScrollView {name = "images"};
            _images.style.height = Length.Percent(100);
            _images.RegisterCallback<GeometryChangedEvent>(v =>
            {
                if (_distantView.style.backgroundImage.value.texture == null) return;

                _imageContentSize.x = _images.contentRect.width;
                _imageContentSize.y = _images.contentRect.height;
                BackgroundImageHelper.FixAspectRatio(_distantView, _imageContentSize, _texOriginalSize,
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

            var layerPanelBtnContainer = buttonContainer;

            var scrollView = new ScrollView();
            scrollView.style.height = Length.Percent(100);

            var btnLoadImage = new Button {text = EditorLocalize.LocalizeText("WORD_3024")};
            btnLoadImage.clicked += LoadDistantViewImage;
            scrollView.Add(btnLoadImage);

            var btnDeleteImage = new Button {text = EditorLocalize.LocalizeText("WORD_3025")};
            btnDeleteImage.clicked += () =>
            {
                DeleteDistantViewImage(Path.ChangeExtension(_mapDataModel.Parallax.name, ".png"));
            };
            scrollView.Add(btnDeleteImage);
            layerPanelBtnContainer.Add(scrollView);

            _distantView = new VisualElement();
            UpdateImagePreview(UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                _mapDataModel.Parallax.ImageFilePath));

            _images.Add(_distantView);
            _layerInventory.Add(_images);
            _layerInventory.SetTileListViewActive(false);
            MapEditor.SetDistantViewToInspector(_mapDataModel);

            return layerPanelBtnContainer;
        }

        /// <summary>
        /// 遠景タブ内の遠景画像のプレビュー表示に画像を反映する
        /// </summary>
        /// <param name="tex2d">表示するテクスチャ</param>
        public void UpdateImagePreview(Texture2D tex2d) {
            _distantView.style.backgroundImage = tex2d;

            if (tex2d == null) return;

            _texOriginalSize.x = tex2d.width;
            _texOriginalSize.y = tex2d.height;
            _imageContentSize.x = _images.contentRect.width;
            _imageContentSize.y = _images.contentRect.height;
            BackgroundImageHelper.FixAspectRatio(
                _distantView, _imageContentSize, _texOriginalSize, LengthUnit.Pixel);
        }
    }
}