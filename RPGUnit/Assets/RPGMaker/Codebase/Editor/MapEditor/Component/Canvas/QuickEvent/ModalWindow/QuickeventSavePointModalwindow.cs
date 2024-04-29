using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventSavePointModalwindow : BaseModalWindow
    {
        private Button          _CANCEL_button;
        private EventEditCanvas _eventEditCanvas;
        private VisualElement   _image;
        private Button          _imageButton;

        //returnする値
        private string _imageName;

        //表示要素
        private Button _OK_button;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_save_point_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventSavePointModalwindow>();

            _eventEditCanvas = eventEditCanvas;

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(320, 145);
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

            // 要素取得

            // アイテム設定

            // 画像選択
            _imageButton = labelFromUxml.Query<Button>("button");
            _image = labelFromUxml.Query<VisualElement>("image");

            // 基本データを取得する
            var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var manageData = _databaseManagementService.LoadAssetManage();
            AssetManageDataModel defaultData = null;
            for (int i = 0; i < manageData.Count; i++)
                if (manageData[i].id == "65098de5-0edb-46d1-a82e-521adae8a06b")
                {
                    defaultData = manageData[i];
                    break;
                }

            if (defaultData != null)
            {
                _imageName = defaultData.id;
                ImageSetting(ImageManager.LoadSvCharacter(defaultData.id));
            }
            else
            {
                _imageName = "";
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
                    _imageName
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
            // ウィンドウサイズ取得
            var width = _imageButton.layout.width;
            var height = _imageButton.layout.height;
            if (float.IsNaN(width)) width = 65;
            if (float.IsNaN(height)) height = 80;

            BackgroundImageHelper.SetBackground(_image, new Vector2(width, height), tex);
        }

        private void OnDestroy() {
            // ウィンドウが閉じられた際にcallbackが呼ばれていなければ呼ぶ
            if (_callBackWindow != null) _callBackWindow(null);
        }
    }
}