using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventDoorModalwindow : BaseModalWindow
    {
        private Button                  _CANCEL_button;
        private EventEditCanvas         _eventEditCanvas;
        private List<EventMapDataModel> _eventList;
        private VisualElement           _image;
        private Button                  _imageButton;
        private string                  _imageName;

        //returnする値
        private MapDataModel  _map;
        private VisualElement _map_select_list;

        //MAPの情報が入る
        private MapDataModel         _mapDataModel;
        private List<MapDataModel>   _mapDatas;
        private MapManagementService _mapManagementService;
        private Button               _OK_button;
        private int                  _X;

        //表示要素
        private Label _X_point;
        private int   _Y;
        private Label _Y_point;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_door_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventDoorModalwindow>();

            _eventEditCanvas = eventEditCanvas;
            _mapDataModel = mapDataModel;
            _eventList = eventList;

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(400, 145);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {
            //MAPデータの読み込み
            _mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            _mapDatas = _mapManagementService.LoadMaps();

            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            //マップ選択
            //初期値
            _map = _mapDatas[0];
            _map_select_list = labelFromUxml.Query<VisualElement>("map_select_list");
            var mapPopupField = new PopupFieldBase<string>(MapNameList(), MapIdToIndex(_mapDataModel));
            _map_select_list.Add(mapPopupField);
            mapPopupField.RegisterValueChangedCallback(evt =>
            {
                //マップID
                //マップ刷新
                if (_mapDatas != null)
                {
                    //選択データの保持を実施
                    _map = _mapDatas[mapPopupField.index];
                    //マップの刷新処理を実施
                    _eventEditCanvas = MapEditor.ReloadEventMap(_mapDatas[mapPopupField.index], _eventList, 0);
                    _eventEditCanvas.Refresh();
                    _eventEditCanvas.StartMapPointMode(EventEditCanvas.PointDelivery.Door, this);
                }
            });

            //座標の表示部位
            _X_point = labelFromUxml.Query<Label>("X_point");
            _Y_point = labelFromUxml.Query<Label>("Y_point");
            //初期値
            _X_point.text = "0";
            _Y_point.text = "0";
            //座標の取得開始
            _eventEditCanvas.StartMapPointMode(EventEditCanvas.PointDelivery.Door, this);

            // 画像選択
            _imageButton = labelFromUxml.Query<Button>("button");
            _image = labelFromUxml.Query<VisualElement>("image");

            // 基本データを取得する
            var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var manageData = _databaseManagementService.LoadAssetManage();
            AssetManageDataModel defaultData = null;
            for (int i = 0; i < manageData.Count; i++)
                if (manageData[i].id == "b4022bca-6fc5-4a6a-bb8b-aafcc2fb68f4")
                {
                    defaultData = manageData[i];
                    break;
                }

            if (defaultData != null)
            {
                _imageName = "b4022bca-6fc5-4a6a-bb8b-aafcc2fb68f4";
                ImageSetting(AssetDatabase.LoadAssetAtPath<Texture2D>(
                    PathManager.IMAGE_OBJECT + defaultData.imageSettings[0].path));
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
                    _map,
                    _X,
                    _Y,
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

        //MAP名をList<string>で返す
        private List<string> MapNameList() {
            var returnList = new List<string>();
            foreach (var data in _mapDatas) returnList.Add(data.name);
            return returnList;
        }

        //MapIdをIndexで返す
        private int MapIdToIndex(MapDataModel mapDataModel) {
            var returnIndex = 0;
            for (var i = 0; i < _mapDatas.Count; i++)
                if (mapDataModel.id == _mapDatas[i].id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        //座標更新部分
        public void ChangePoint(Vector3Int point) {
            _X = point.x;
            _Y = point.y;
            _X_point.text = point.x.ToString();
            _Y_point.text = Mathf.Abs(point.y).ToString();
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