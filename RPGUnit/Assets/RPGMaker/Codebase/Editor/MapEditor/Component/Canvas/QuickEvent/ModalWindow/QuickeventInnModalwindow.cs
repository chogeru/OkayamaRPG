using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventInnModalwindow : BaseModalWindow
    {
        // 初期価格
        private const int DEFAULT_PRICE = 10;

        // 最低、最高価格
        private const int             MIN_PRICE = 0;
        private const int             MAX_PRICE = 9999;
        private       Button          _CANCEL_button;
        private       EventEditCanvas _eventEditCanvas;
        private       VisualElement   _image;
        private       Button          _imageButton;

        //returnする値
        private string _imageName;

        //表示要素
        private Button       _OK_button;
        private IntegerField _price;
        private int          _priceValue;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_inn_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventInnModalwindow>();

            _eventEditCanvas = eventEditCanvas;

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(245, 145);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            // 価格設定
            _priceValue = DEFAULT_PRICE;
            _price = labelFromUxml.Query<IntegerField>("price");
            _price.value = _priceValue;
            _price.RegisterValueChangedCallback(evt =>
            {
                if (_price.value < MIN_PRICE)
                    _price.value = MIN_PRICE;
                else if (_price.value > MAX_PRICE)
                    _price.value = MAX_PRICE;
                _priceValue = _price.value;
            });

            // 画像選択
            _imageButton = labelFromUxml.Query<Button>("button");
            _image = labelFromUxml.Query<VisualElement>("image");

            // 基本データを取得する
            var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var manageData = _databaseManagementService.LoadAssetManage();
            AssetManageDataModel defaultData = null;
            for (int i = 0; i < manageData.Count; i++)
                if (manageData[i].id == "100370ee-0948-45ab-af03-681a93fea5c5")
                {
                    defaultData = manageData[i];
                    break;
                }

            if (defaultData != null)
            {
                _imageName = "100370ee-0948-45ab-af03-681a93fea5c5";
                ImageSetting(ImageManager.LoadSvCharacter(_imageName));
            }

            _imageButton.clickable.clicked += () =>
            {
                var sdSelectModalWindow = new SdSelectModalWindow();
                sdSelectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
                sdSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select IconImage"), data =>
                {
                    var imageName = (string) data;
                    var imageCharacter = ImageManager.LoadSvCharacter(imageName);
                    ImageSetting(imageCharacter);
                    _imageName = imageName;
                }, _imageName);
            };

            //OKボタン
            _OK_button = labelFromUxml.Query<Button>("OK_button");
            _OK_button.clicked += () =>
            {
                //決定されたのでポップアップのデータを渡す
                var returnValue = new List<object>
                {
                    _imageName,
                    _priceValue
                };
                _callBackWindow(returnValue);
                _callBackWindow = null;
                Close();
            };

            //CANCELボタン
            _CANCEL_button = labelFromUxml.Query<Button>("CANCEL_button");
            _CANCEL_button.clicked += () =>
            {
                //入っている初期値で決定にするためこのままこのウィンドウを閉じる
                _callBackWindow(null);
                _callBackWindow = null;
                Close();
            };
        }

        // 画像設定
        private void ImageSetting(Texture2D tex) {

            BackgroundImageHelper.SetBackground(_image, new Vector2(66, 76), tex, LengthUnit.Pixel);
        }

        private void OnDestroy() {
            // ウィンドウが閉じられた際にcallbackが呼ばれていなければ呼ぶ
            if (_callBackWindow != null) _callBackWindow(null);
        }
    }
}