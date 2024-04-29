using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent
{
    public class QuickEventMovement : QuickEventEdit
    {
        private EventDataModel         _eventDataModel;
        private EventManagementService _eventManagementService;
        private EventMapDataModel      _eventMapDataModel;
        private MapManagementService   _mapManagementService;

        public void Init(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            Vector3Int pos,
            MapDataModel mapDataModel
        ) {
            _eventManagementService = new EventManagementService();
            _mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;

            //簡単選択ポップアップ
            //-------------------------------------------------------------
            var quickeventMovementModalwindow = new QuickeventMovementModalwindow();
            quickeventMovementModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1574"),
                data =>
                {
                    var modalValue = (List<object>) data;

                    if (data != null)
                    {
                        _eventMapDataModel = MapEditor.CreateEvent(mapDataModel, pos.x, pos.y, 1);
                        _eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId);

                        // イベント設定 (移動、向きが返る)
                        var eventCommandList = SetEvent();

                        eventCommandList[0].parameters[1] = ((MapDataModel) modalValue[2]).name;
                        eventCommandList[0].parameters[2] = ((MapDataModel) modalValue[2]).id;
                        eventCommandList[0].parameters[3] = ((int) modalValue[3]).ToString();
                        eventCommandList[0].parameters[4] = ((int) modalValue[4]).ToString();
                        eventCommandList[0].parameters[7] = ((MapDataModel) modalValue[2]).id;
                        eventCommandList[1].code = (int) modalValue[0];
                        eventCommandList[1].parameters[1] = ((int) modalValue[1]).ToString();
                        _eventManagementService.SaveEvent(_eventDataModel);
                        MapEditor.ReloadMaps();
                        // ヒエラルキーの該当イベントページを選択状態にする。
                        Hierarchy.Hierarchy.SelectButton(
                            CommonMapHierarchyView.GetEventPageButtonName(_eventMapDataModel.eventId, 0));
                    }
                });
        }

        //-------------------------------------------------------------
        // イベントの設定処理
        //-------------------------------------------------------------
        private List<EventDataModel.EventCommand> SetEvent() {
            // 返り値用
            var eventCommands = new List<EventDataModel.EventCommand>();

            // SEの再生
            //-------------------------------------------------------------
            //サウンドファイルの取得部分
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            var path = "Assets/RPGMaker/Storage/Sounds/SE/";
            var dir = new DirectoryInfo(path);
            var info = dir.GetFiles("*.ogg");
            var fileNames = new List<string>();
            foreach (var f in info) fileNames.Add(f.Name.Replace(".ogg", ""));
            info = dir.GetFiles("*.wav");
            foreach (var f in info) fileNames.Add(f.Name.Replace(".wav", ""));

            // 初期値があるか確認
            var defaultValue = fileNames[0];
            if (fileNames.Contains("Move1"))
                defaultValue = "Move1";

            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_AUDIO_SE_PLAY,
                    new List<string>
                    {
                        defaultValue,
                        "90",
                        "100",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            //移動
            //-------------------------------------------------------------
            //マップの読み込み部分
            var MapEntities = _mapManagementService.LoadMaps();
            //初期値データ代入
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MOVE_PLACE,
                    new List<string>
                    {
                        "0",
                        MapEntities[0].name,
                        MapEntities[0].id,
                        "0",
                        "0",
                        "0",
                        "0",
                        MapEntities[0].id
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            eventCommands.Add(_eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1]);

            //向き
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.MOVEMENT_TURN_DOWN,
                    new List<string>
                    {
                        "-2",
                        "0",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            eventCommands.Add(_eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1]);

            return eventCommands;
        }
    }
}