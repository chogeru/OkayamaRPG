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
    public class QuickEventDoor : QuickEventEdit
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
            var QuickeventDoorModalwindow = new QuickeventDoorModalwindow();
            QuickeventDoorModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1573"),
                data =>
                {
                    var modalValue = (List<object>) data;

                    if (data != null)
                    {
                        _eventMapDataModel = MapEditor.CreateEvent(mapDataModel, pos.x, pos.y);
                        _eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId);

                        // イベントを追加
                        var Movement = SetEvent();

                        Movement.parameters[1] = ((MapDataModel) modalValue[0]).name;
                        Movement.parameters[2] = ((MapDataModel) modalValue[0]).id;
                        Movement.parameters[3] = ((int) modalValue[1]).ToString();
                        Movement.parameters[4] = ((int) modalValue[2]).ToString();
                        Movement.parameters[7] = ((MapDataModel) modalValue[0]).id;

                        _eventMapDataModel.pages[0].image.sdName = modalValue[3]?.ToString();
                        _eventMapDataModel.pages[0].condition.image.enabled = 1;

                        _eventMapDataModel.pages[0].walk.through = 1;
                        _eventMapDataModel.pages[0].priority = 1;
                        _eventMapDataModel.pages[0].eventTrigger = 1;

                        _eventManagementService.SaveEventMap(_eventMapDataModel);
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
        private EventDataModel.EventCommand SetEvent() {
            // 返り値用
            EventDataModel.EventCommand eventCommand;

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
            if (fileNames.Contains("Open1"))
                defaultValue = "Open1";

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

            // 向き変更
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.MOVEMENT_TURN_LEFT,
                    new List<string>
                    {
                        "-1",
                        "1",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // Wait
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                    new List<string>
                    {
                        "3"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 向き変更
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.MOVEMENT_TURN_RIGHT,
                    new List<string>
                    {
                        "-1",
                        "2",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // Wait
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                    new List<string>
                    {
                        "3"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 向き変更
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.MOVEMENT_TURN_UP,
                    new List<string>
                    {
                        "-1",
                        "3",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            //SEの追加
            //-------------------------------------------------------------
            // 初期値があるか確認
            defaultValue = fileNames[0];
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

            eventCommand = _eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1];
            return eventCommand;
        }
    }
}