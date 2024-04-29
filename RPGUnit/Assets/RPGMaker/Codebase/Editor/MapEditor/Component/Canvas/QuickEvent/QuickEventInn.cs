using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent
{
    public class QuickEventInn : QuickEventEdit
    {
        // $に金額を入れる
        private const string         INN_TEXT      = "WORD_3107";
        private const string         INN_TEXT_ELSE = "WORD_3110";
        private       EventDataModel _eventDataModel;

        private EventManagementService _eventManagementService;
        private EventMapDataModel      _eventMapDataModel;

        public void Init(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            Vector3Int pos,
            MapDataModel mapDataModel
        ) {
            _eventManagementService = new EventManagementService();

            //簡単選択ポップアップ
            //-------------------------------------------------------------
            var QuickeventInnModalwindow = new QuickeventInnModalwindow();
            QuickeventInnModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1580"),
                data =>
                {
                    // 0.sdName 1.price
                    var modalValue = (List<object>) data;

                    if (data != null)
                    {
                        //イベントデータの作成
                        _eventMapDataModel = MapEditor.CreateEvent(mapDataModel, pos.x, pos.y);
                        _eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId);

                        // イベント設定 (文章、条件分岐、所持金増減が返る)

                        var eventMapPage = _eventMapDataModel.pages[EventMapDataModel.EventMapPage.DefaultPage];
                        eventMapPage.image.sdName = modalValue[0]?.ToString();
                        eventMapPage.condition.image.enabled = 1;

                        var eventCommandList = SetEvent();
                        eventCommandList[0].parameters[0] = EditorLocalize.LocalizeText(INN_TEXT)
                            .Replace("$", modalValue[1].ToString());
                        eventCommandList[1].parameters[39] = modalValue[1].ToString();
                        eventCommandList[2].parameters[2] = modalValue[1].ToString();

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
        private List<EventDataModel.EventCommand> SetEvent() {
            // 返り値用
            var eventCommands = new List<EventDataModel.EventCommand>();

            // テキスト
            //-------------------------------------------------------------
            // テキスト設定
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT,
                    new List<string> {"0", "0", "0", "0", "2", EditorLocalize.LocalizeText("WORD_0092"), "", "", ""},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // テキスト文章
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                    new List<string> {EditorLocalize.LocalizeText(INN_TEXT).Replace("$", ""), "2203"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 返り値配列に追加
            eventCommands.Add(_eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1]);

            // 選択設定
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT,
                    new List<string> {"2", "0", "0", "0", "1", EditorLocalize.LocalizeText("WORD_0092")},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 選択肢ラベル
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                    new List<string> {"1", "2203", EditorLocalize.LocalizeText("WORD_3108") },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 条件分岐
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_FLOW_IF,
                    new List<string>
                    {
                        "1", "0", "0", "0", "caa9981e-f67a-49cd-a381-a5352fce02ec",
                        "0", "0", "0", "0", "0", "0", "0", "0", "0", "0",
                        "0", "0", "0", "0", "0", "0", "0", "0",
                        "ce89f973-6ed6-41fc-99d7-e3ca5fc1ce8c", "1", "661cb624-e856-494e-94b8-0e41064e90b0",
                        "51ec1c98-b2b3-49bc-b488-fc945381eaa0", "8fd93d41-fb58-401b-8d6b-f7d5396d3fec",
                        "0", "0", "0", "0", "0", "0", "0", "0", "0", "1", "0",
                        "10", "0", "0", "0", "0", "0", "0", "0", "0", "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 返り値配列に追加
            eventCommands.Add(_eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1]);

            // 所持金増減
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_PARTY_GOLD,
                    new List<string> {"1", "0", "10"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 返り値配列に追加
            eventCommands.Add(_eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1]);

            // フェードアウト
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_DISPLAY_FADEOUT,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // ME再生
            //-------------------------------------------------------------
            //サウンドファイルの取得部分
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            var path = "Assets/RPGMaker/Storage/Sounds/ME/";
            var dir = new DirectoryInfo(path);
            var info = dir.GetFiles("*.ogg");
            var fileNames = new List<string>();
            foreach (var f in info) fileNames.Add(f.Name.Replace(".ogg", ""));
            info = dir.GetFiles("*.wav");
            foreach (var f in info) fileNames.Add(f.Name.Replace(".wav", ""));
            // 初期値があるか確認
            var defaultValue = fileNames[0];
            if (fileNames.Contains("Inn2"))
                defaultValue = "Inn2";

            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_AUDIO_ME_PLAY,
                    new List<string> {defaultValue, "90", "100", "0"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // Wait
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                    new List<string> {"300"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 回復
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_ACTOR_HEAL,
                    new List<string> {"0", "-1"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // フェードイン
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_DISPLAY_FADEIN,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 空行
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    0,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // else
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_FLOW_ELSE,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // テキスト
            //-------------------------------------------------------------
            // テキスト設定
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT,
                    new List<string> {"0", "0", "0", "0", "2", EditorLocalize.LocalizeText("WORD_0092"), "", "", ""},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // テキスト文章
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                    new List<string> {EditorLocalize.LocalizeText(INN_TEXT_ELSE), "2203"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 空行
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    0,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // ENDIF
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_FLOW_ENDIF,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 空行
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    0,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // ENDIF
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                    new List<string> {"2", "2203", EditorLocalize.LocalizeText("WORD_3109") },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 空行
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    0,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // ENDIF
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END,
                    new List<string> {"-1", "2203", " "},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            return eventCommands;
        }
    }
}