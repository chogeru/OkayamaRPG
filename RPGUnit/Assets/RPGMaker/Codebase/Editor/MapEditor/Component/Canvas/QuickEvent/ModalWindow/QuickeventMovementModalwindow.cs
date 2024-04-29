using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventMovementModalwindow : BaseModalWindow
    {
        //翻訳用の選択肢データ
        private static readonly List<string> _directionChoices = EditorLocalize.LocalizeTexts(new List<string>
        {
            "WORD_0955",
            "WORD_0956",
            "WORD_0957",
            "WORD_0958"
        });

        private Button _CANCEL_button;

        //returnする値
        private int           _direction;
        private VisualElement _direction_list;

        //選択された値とセーブするデータのまとめ
        private readonly Dictionary<string, int> _directionDictionary = new Dictionary<string, int>
        {
            {_directionChoices[0], (int) EventMoveEnum.MOVEMENT_TURN_DOWN},
            {_directionChoices[1], (int) EventMoveEnum.MOVEMENT_TURN_LEFT},
            {_directionChoices[2], (int) EventMoveEnum.MOVEMENT_TURN_RIGHT},
            {_directionChoices[3], (int) EventMoveEnum.MOVEMENT_TURN_UP}
        };

        private int                     _directionIndex;
        private EventEditCanvas         _eventEditCanvas;
        private List<EventMapDataModel> _eventList;
        private MapDataModel            _map;
        private VisualElement           _map_select_list;

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
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_movement_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventMovementModalwindow>();

            _eventEditCanvas = eventEditCanvas;
            _mapDataModel = mapDataModel;
            _eventList = eventList;

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(400, 77);
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
                    _eventEditCanvas.StartMapPointMode(EventEditCanvas.PointDelivery.Movement, this);
                }
            });

            //座標の表示部位
            _X_point = labelFromUxml.Query<Label>("X_point");
            _Y_point = labelFromUxml.Query<Label>("Y_point");
            //初期値
            _X_point.text = "0";
            _Y_point.text = "0";
            //座標の取得開始
            _eventEditCanvas.StartMapPointMode(EventEditCanvas.PointDelivery.Movement, this);

            //向き選択
            //初期値
            _direction = _directionDictionary[_directionChoices[0]];
            _directionIndex = 0;
            _direction_list = labelFromUxml.Query<VisualElement>("direction_list");
            var directionPopupField = new PopupFieldBase<string>(EditorLocalize.LocalizeTexts(_directionChoices), 0);
            _direction_list.Add(directionPopupField);
            directionPopupField.RegisterValueChangedCallback(evt =>
            {
                _direction = _directionDictionary[_directionChoices[directionPopupField.index]];
                _directionIndex = directionPopupField.index;
            });

            //OKボタン
            _OK_button = labelFromUxml.Query<Button>("OK_button");
            _OK_button.clicked += () =>
            {
                //決定されたのでポップアップのデータを渡す
                var returnValue = new List<object>
                {
                    _direction,
                    _directionIndex,
                    _map,
                    _X,
                    _Y
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

        private void OnDestroy() {
            // ウィンドウが閉じられた際にcallbackが呼ばれていなければ呼ぶ
            if (_callBackWindow != null) _callBackWindow(null);
        }
    }
}